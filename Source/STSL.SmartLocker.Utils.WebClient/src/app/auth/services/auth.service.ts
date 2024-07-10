import { Injectable, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { BehaviorSubject, Observable, Subscription, filter, map, mergeMap, startWith, tap } from 'rxjs';
import { FeatureFlag } from 'src/app/feature-flag/feature-flag.type';
import { FeatureFlagService } from 'src/app/feature-flag/services/feature-flag.service';
import { checkEntityIdEquality } from 'src/app/shared/lib/utilities';
import { PageFilterSortRequest } from 'src/app/shared/models/api';
import { defaultCardHolderAliasPlural, defaultCardHolderAliasSingular, defaultUniqueIdentifierAlias } from 'src/app/shared/models/card-holder';
import { EntityId } from 'src/app/shared/models/common';
import { ITenantDTO } from 'src/app/shared/models/tenant';
import { ApiService } from 'src/app/shared/services/api.service';
import { environment } from 'src/environments/environment';
import { UnassignedRolesAuthError, UnassignedTenantsAuthError } from '../models/auth-error';
import { UserClaims } from '../models/user-claims';
import { IUserData } from '../models/user-data';
import { UserRole } from '../models/user-roles';

const FAVOURITE_TENANT_STORAGE_KEY = 'GotoSecureFavouriteTenantId';

@Injectable({
  providedIn: 'root'
})
export class AuthService implements OnDestroy {

  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly router = inject(Router);
  private readonly apiService = inject(ApiService);
  private readonly featureFlagService = inject(FeatureFlagService);

  readonly configuration$ = this.oidcSecurityService.getConfiguration();
  readonly isAuthenticated$: Observable<boolean> = this.oidcSecurityService.isAuthenticated$.pipe(map(({ isAuthenticated }) => isAuthenticated));

  private readonly currentTenantSubject = new BehaviorSubject<EntityId | undefined>(undefined);
  readonly currentTenant$ = this.currentTenantSubject.asObservable();

  private userDataSubject = new BehaviorSubject<IUserData | undefined>(undefined);
  readonly userData$ = this.userDataSubject.asObservable();

  private selectedTenantSubject = new BehaviorSubject<EntityId | undefined>(undefined);
  readonly selectedTenant$ = this.selectedTenantSubject.asObservable();

  // NOTE: This is a little hacky; this service could do with lots of refactoring!
  readonly currentTenantDetails$ = this.currentTenant$.pipe(mergeMap(() => this.userData$), map(() => this.currentTenantDetails));
  readonly currentTenantLogoSrc$ = this.currentTenantDetails$.pipe(map(x => this.getLogoSrcFromTenant(x)));
  readonly cardHolderAliasSingular$ = this.currentTenantDetails$.pipe(map(x => x?.cardHolderAliasSingular || defaultCardHolderAliasSingular), startWith(defaultCardHolderAliasSingular));
  readonly cardHolderAliasPlural$ = this.currentTenantDetails$.pipe(map(x => x?.cardHolderAliasPlural || defaultCardHolderAliasPlural), startWith(defaultCardHolderAliasPlural));
  readonly cardHolderUniqueIdentifierAlias$ = this.currentTenantDetails$.pipe(map(x => x?.cardHolderUniqueIdentifierAlias || defaultUniqueIdentifierAlias), startWith(defaultUniqueIdentifierAlias));
  readonly favouriteTenantId$ = this.userData$.pipe(map(() => this.favouriteTenantId));

  get selectedTenant(): EntityId | undefined {
    return this.selectedTenantSubject.getValue();
  }
  
  set selectedTenant(value: EntityId | undefined) {
    if (value && this.tenantRegisteredToUserOrIsSuperUser(value)) {
      this.selectedTenantSubject.next(value);
    }
  }


  get currentTenant(): EntityId | undefined {
    return this.currentTenantSubject.getValue();
  }
  set currentTenant(value: EntityId | undefined) {
    if(value && this.tenantRegisteredToUserOrIsSuperUser(value)) {
      this.currentTenantSubject.next(value);
    }
    this.reloadCurrentPage();
  }

  get currentTenantDetails(): ITenantDTO | undefined {
    const tenantId = this.currentTenant;
    if(!tenantId) {
      return;
    }
    return this.userData?.tenants.find(x => x.id === tenantId);
  }

  get cardHolderAliasSingular(): string {
    return this.currentTenantDetails?.cardHolderAliasSingular ?? defaultCardHolderAliasSingular;
  }

  get cardHolderAliasPlural(): string {
    return this.currentTenantDetails?.cardHolderAliasPlural ?? defaultCardHolderAliasPlural;
  }

  get cardHolderUniqueIdentifierAlias(): string {
    return this.currentTenantDetails?.cardHolderUniqueIdentifierAlias ?? defaultUniqueIdentifierAlias;
  }

  get userData(): IUserData | undefined {
    return this.userDataSubject?.getValue();
  }

  get favouriteTenantId(): EntityId | undefined {
    // Check the entityId is a valid entityId i.e. check string length + format (split on '-', check each ...)
    if(this.userData?.sub) {
      return localStorage.getItem(FAVOURITE_TENANT_STORAGE_KEY + this.userData?.sub ?? '') || undefined;
    }
    return undefined;
  }

  set favouriteTenantId(tenantId: EntityId | undefined) {
    if(this.userData?.sub) {
      localStorage.setItem(FAVOURITE_TENANT_STORAGE_KEY + this.userData?.sub ?? '', tenantId ?? '');
    }
  }

  private authSubscription?: Subscription;

  private readonly getTenantDetailsFromApi: PageFilterSortRequest<ITenantDTO> = this.apiService.generatePageFilterSortRequest<ITenantDTO>('/tenants');

  ngOnDestroy(): void {
    this.authSubscription?.unsubscribe();
  }

  init() {
    this.authSubscription = this.oidcSecurityService.checkAuth()
      .pipe(
        filter(({ idToken }) => idToken !== null),
        mergeMap(() => this.getAndUpdateUserDataFromToken())
      ).subscribe();
  }

  login() {
    this.oidcSecurityService.authorize();
  }

  logoffAndRevokeTokens() {
    this.oidcSecurityService.logoffAndRevokeTokens().subscribe(() => {
      this.currentTenantSubject.next(undefined);
      this.router.navigateByUrl("/");
    });
  }

  refreshUserData() {
    return this.getAndUpdateUserDataFromToken().pipe(
      tap(() => {
        this.updateCurrentTenantIfNotInList(this.userData?.tenants.map(({ id }) => id) ?? []);
        this.reloadCurrentPage();
      })
    );
  }
  
  refreshTenantDetails() {
    return this.updateTenantDetails().pipe(
      tap(() => {
        this.updateCurrentTenantIfNotInList(this.userData?.tenants.map(({ id }) => id) ?? []);
        this.reloadCurrentPage();
      })
    );
  }

  toggleRoleEnabled(role: UserRole) {
    if(!environment.production) {
      switch(role) {
        case UserRole.SuperUser: this.featureFlagService.toggleFeatureFlag(FeatureFlag.DisableSuperUserRole); break;
        case UserRole.TenantAdmin: this.featureFlagService.toggleFeatureFlag(FeatureFlag.DisableTenantAdminRole); break;
        case UserRole.Installer: this.featureFlagService.toggleFeatureFlag(FeatureFlag.DisableInstallerRole); break;
        case UserRole.LockerBankAdmin: this.featureFlagService.toggleFeatureFlag(FeatureFlag.DisableLockerBankAdminRole); break;
      }
    }
    this.reloadCurrentPage();
  }

  isRoleEnabled(role: UserRole): boolean {
    if(!environment.production) {
      switch(role) {
        case UserRole.SuperUser: return !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableSuperUserRole);
        case UserRole.TenantAdmin: return !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableTenantAdminRole);
        case UserRole.Installer: return !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableInstallerRole);
        case UserRole.LockerBankAdmin: return !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableLockerBankAdminRole);
      }
    }
    return true;
  }

  userHasRoleAndRoleIsEnabled(role: UserRole): boolean {
    switch(role) {
      case UserRole.SuperUser:
        return (this.userData?.roles.includes(role) ?? false) && (environment.production || !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableSuperUserRole));
      case UserRole.TenantAdmin:
        return (this.userData?.roles.includes(role) ?? false) && (environment.production || !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableTenantAdminRole));
      case UserRole.Installer:
        return (this.userData?.roles.includes(role) ?? false) && (environment.production || !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableInstallerRole));
      case UserRole.LockerBankAdmin:
        return (this.userData?.roles.includes(role) ?? false) && (environment.production || !this.featureFlagService.hasFeatureFlagSet(FeatureFlag.DisableLockerBankAdminRole));
    }
  }

  private getAndUpdateUserDataFromToken(): Observable<IUserData | undefined> {
    return this.oidcSecurityService.getPayloadFromIdToken().pipe(tap(tokenPayload => this.updateUserData(tokenPayload)));
  }

  private updateUserData(userData: any) {
    if (!userData) {
      this.userDataSubject.next(undefined);
      return;
    }

    const tenants: EntityId[] = userData[UserClaims.Tenants] ?? [];
    let roles: UserRole[] = userData[UserClaims.Roles] ?? [];
    const email = userData[UserClaims.Email] ?? '';

    const { name, nickname, picture, sub } = userData;
    const updatedUserData: IUserData = { name, email, sub, nickname, picture, tenants: tenants.map(id => ({ id, name: '', })), roles, };

    this.userDataSubject.next(updatedUserData);

    if (tenants?.length === 0) {
      throw new UnassignedTenantsAuthError();
    }

    if(roles?.length === 0) {
      throw new UnassignedRolesAuthError();
    }

    // We get some tenant Ids in the access token, however we afterwards get a list of tenant data from the API.
    // Super users will get the list of all, non super-users will get only their registered tenants.
    this.updateCurrentTenantIfNotInList(tenants);

    // Here we get the tenants from the API
    this.updateTenantDetails().subscribe();
  }

  private updateTenantDetails() {
    return this.getTenantDetailsFromApi().pipe(tap(({ results }) => {
      if(this.userData) {
        this.userDataSubject.next({ ...this.userData, tenants: results });

        this.updateCurrentTenantIfNotInList(results.map(({ id }) => id));
      }
    }));
  }

  /**
   * Set the current tenant to first or favourite as default if no tenant is currently selected
   * or the users list of tenants doesn't include the current tenant (maybe deleted)
   *
   * @param tenantIds The tenant Ids to compare currentTenant against
   */
  private updateCurrentTenantIfNotInList(tenantIds: EntityId[]) {
    if (!tenantIds.length) {
      throw new UnassignedTenantsAuthError();
    }

    const selectedTenantId = this.getSelectedTenantFromStorage();
    if (selectedTenantId && this.tenantRegisteredToUserOrIsSuperUser(selectedTenantId)) {
      this.currentTenantSubject.next(selectedTenantId);
    } else {
      const favouriteTenantId = this.favouriteTenantId;
      if (favouriteTenantId && this.tenantRegisteredToUserOrIsSuperUser(favouriteTenantId)) {
        this.currentTenantSubject.next(favouriteTenantId);
      } else {
        this.currentTenantSubject.next(tenantIds[0]);
      }
    }
  }

  private tenantRegisteredToUserOrIsSuperUser(tenantId: EntityId): boolean {
    return this.userData?.roles.includes(UserRole.SuperUser) || this.tenantIsRegisteredToUser(tenantId);
  }

  private tenantIsRegisteredToUser(tenantId: EntityId) {
    const tenantIds = this.userData?.tenants.map(({ id }) => id) ?? [];
    return tenantIds.some(id => checkEntityIdEquality(id, tenantId));
  }

  private reloadCurrentPage() {
    this.router.navigate([this.router.url]);
  }

  private getLogoSrcFromTenant(tenant?: ITenantDTO) {
    if(!tenant?.logo || !tenant.logoMimeType) {
      return;
    }

    return `data:${tenant.logoMimeType};base64,${tenant.logo.toString()}`;
  }

  private getSelectedTenantFromStorage(): EntityId | null | undefined {
    if (this.userData?.sub) {
      return localStorage.getItem('selectedTenantId' + this.userData?.sub);
    }
    return undefined;
  }
}
