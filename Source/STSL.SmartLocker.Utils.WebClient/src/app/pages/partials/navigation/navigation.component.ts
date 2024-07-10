import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { IUserData } from 'src/app/auth/models/user-data';
import { UserRole } from 'src/app/auth/models/user-roles';
import { AuthService } from 'src/app/auth/services/auth.service';
import { FeatureFlag } from 'src/app/feature-flag/feature-flag.type';
import { FeatureFlagService } from 'src/app/feature-flag/services/feature-flag.service';
import { checkEntityIdEquality } from 'src/app/shared/lib/utilities';
import { DateTime, EntityId } from 'src/app/shared/models/common';
import { ITenantDTO } from 'src/app/shared/models/tenant';
import { environment } from 'src/environments/environment';
import { AnnouncekitComponent } from 'announcekit-angular/public-api';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavigationComponent implements OnInit {

  private readonly authService = inject(AuthService);
  private readonly featureFlagService = inject(FeatureFlagService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild("announceKitTemplate", { static: false })
  announceKitTemplate!: AnnouncekitComponent;

  menuIsOpen = false;

  checkEntityIdEquality = checkEntityIdEquality;

  userData?: IUserData;
  user!: {
    name?: string,
    email?: string,
    id: string
  }

  readonly inDevelopment = !environment.production;

  readonly isUserLoggedIn$ = this.authService.isAuthenticated$;
  currentTenant?: ITenantDTO;

  favouriteTenantId = this.authService.favouriteTenantId ?? '';

  selectedTenant: EntityId | undefined;

  readonly UserRole = UserRole;

  // NOTE: This should probably be moved out into the same file as feature flag definitions
  // Would be another good use case of having a value with a string display property.
  readonly featureFlags = [
    { name: 'Full locker audit fields', value: FeatureFlag.ShowAllLockerAuditFields },
    { name: 'Full locker status fields', value: FeatureFlag.ShowAllLockerStatusFields },
    { name: 'Disable creating shift locker banks', value: FeatureFlag.DisableShiftLockerBanks },
  ];

  ngOnInit(): void {
    this.authService.userData$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(userData => {
      this.favouriteTenantId = this.authService.favouriteTenantId ?? '';
      this.userData = userData;
      this.user = {
        name: this.userData?.name,
        email: this.userData?.email,
        id: this.userData!.email
      }

      console.log(this.user);
    });

    this.authService.selectedTenant$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(selectedTenant => {
      this.selectedTenant = selectedTenant;
    });

    this.authService.currentTenant$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(currentTenant => {
      this.selectedTenant = currentTenant;
    });
  }

  loginUser() {
    this.authService.login();
  }

  logoutUser() {
    this.authService.logoffAndRevokeTokens();
  }

  switchTenant(tenantId: EntityId) {
    this.selectedTenant = tenantId;
    this.setSelectedTenantInStorage(tenantId);
    this.reloadCurrentPage();
    
  }

  toggleFavouriteTenant(tenant: ITenantDTO, event: Event) {
    this.authService.favouriteTenantId = checkEntityIdEquality(tenant.id, this.favouriteTenantId) ? undefined : tenant.id;
    this.favouriteTenantId = this.authService.favouriteTenantId ?? '';

    event.stopPropagation();
  }

  toggleRole(role: UserRole) {
    this.authService.toggleRoleEnabled(role);
  }

  isRoleEnabled(role: UserRole) {
    return this.authService.userHasRoleAndRoleIsEnabled(role);
  }

  toggleFeatureFlag(flag: FeatureFlag) {
    this.featureFlagService.toggleFeatureFlag(flag);
  }

  isFeatureFlagSet(flag: FeatureFlag) {
    return this.featureFlagService.hasFeatureFlagSet(flag);
  }

  menuOpened() {
    this.menuIsOpen = true;
  }

  menuClosed() {
    this.menuIsOpen = false;
  }
  setSelectedTenantInStorage(tenantId: EntityId) {
    if (this.userData?.sub) {
      localStorage.setItem('selectedTenantId' + this.userData?.sub, tenantId);
    }
  }

  private reloadCurrentPage() {
    window.location.reload();
  }

}

