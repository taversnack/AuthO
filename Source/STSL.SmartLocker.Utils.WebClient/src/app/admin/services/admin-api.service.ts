import { Injectable, OnDestroy, inject } from '@angular/core';
import { Subscription, map } from 'rxjs';
import { UnassignedTenantsAuthError } from 'src/app/auth/models/auth-error';
import { AuthService } from 'src/app/auth/services/auth.service';
import { ICreateBulkCardHolderAndCardCredentialsDTO, IMoveLockersToLockerBankDTO } from 'src/app/shared/models/bulk';
import { ICardCredentialDTO, ICreateCardCredentialDTO, IUpdateCardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ICardHolderAndCardCredentialsDTO } from 'src/app/shared/models/card-credential-and-card-holder';
import { ICardHolderDTO, ICreateCardHolderDTO, IUpdateCardHolderDTO } from 'src/app/shared/models/card-holder';
import { EntityId } from 'src/app/shared/models/common';
import { IGlobalLockerSearchResultDTO } from 'src/app/shared/models/global-locker-search-result';
import { ICreateLocationDTO, ILocationDTO, IUpdateLocationDTO } from 'src/app/shared/models/location';
import { ICreateLockDTO, ILockDTO, IUpdateLockDTO } from 'src/app/shared/models/lock';
import { ICreateLockerDTO, ILockerDTO, IUpdateLockerDTO } from 'src/app/shared/models/locker';
import { ICreateLockerAndLockDTO, ILockerAndLockDTO } from 'src/app/shared/models/locker-and-lock';
import { ILockerAuditDTO } from 'src/app/shared/models/locker-audit';
import { ICreateLockerBankDTO, ILockerBankDTO, IUpdateLockerBankDTO } from 'src/app/shared/models/locker-bank';
import { ILockerBankAdminDTO } from 'src/app/shared/models/locker-bank-admin';
import { ICreateLockerCardCredentialDTO } from 'src/app/shared/models/locker-card-credential';
import { ILockerLeaseDTO } from 'src/app/shared/models/locker-lease';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { ICreateReferenceImageDTO, IUpdateReferenceImageDTO, IViewReferenceImageDTO } from 'src/app/shared/models/reference-image';
import { ICreateTenantDTO, ITenantDTO, IUpdateTenantDTO } from 'src/app/shared/models/tenant';
import { ApiService } from 'src/app/shared/services/api.service';
import { ILockerBankAdminLocationSummaryDTO } from 'src/app/v2/shared/models/location-locker-bank-summary';

@Injectable({
  providedIn: 'root'
})
export class AdminApiService implements OnDestroy {

  private readonly apiService = inject(ApiService);
  private readonly authService = inject(AuthService);

  private currentTenantId?: EntityId = this.authService.currentTenant;
  private readonly currentTenantSubscription: Subscription = this.authService.currentTenant$.subscribe(x => this.currentTenantId = x);

  // API Endpoints
  Tenants = this.apiService.generateCrudEndpoints<ITenantDTO, ICreateTenantDTO, IUpdateTenantDTO>(this.endpointFor());

  private readonly locationsEndpoint = this.endpointFor('locations');
  private readonly lockerBanksEndpoint = this.endpointFor('locker-banks');
  private readonly lockersEndpoint = this.endpointFor('lockers');
  private readonly locksEndpoint = this.endpointFor('locks');
  private readonly cardHoldersEndpoint = this.endpointFor('card-holders');
  private readonly lockerLeasesEndpoint = this.endpointFor('locker-leases');
  private readonly locationsReferenceImagesEndpoint = this.endpointFor('locations/reference-images');
  private readonly lockerBanksReferenceImagesEndpoint = this.endpointFor('locker-banks/reference-images');

  Locations = {
    ...this.apiService.generateCrudEndpoints<ILocationDTO, ICreateLocationDTO, IUpdateLocationDTO>(this.locationsEndpoint),
    LockerBanks: this.apiService.generateNestedResource<ILockerBankDTO, ICreateLockerBankDTO>(this.locationsEndpoint, 'locker-banks'),
    ReferenceImages: {
      ...this.apiService.generateCrudEndpoints<IViewReferenceImageDTO, ICreateReferenceImageDTO, IUpdateReferenceImageDTO>(this.locationsReferenceImagesEndpoint),
    },
  };

  LockerBanks = {
    ...this.apiService.generateCrudEndpoints<ILockerBankDTO, ICreateLockerBankDTO, IUpdateLockerBankDTO>(this.lockerBanksEndpoint),
    Lockers: {
      ...this.apiService.generateNestedResource<ILockerDTO, ICreateLockerDTO>(this.lockerBanksEndpoint, 'lockers'),
      Statuses: this.apiService.generateNestedResourcePageFilterSortRequest<ILockerStatusDTO>(this.lockerBanksEndpoint, 'lockers-status'),
      Move: this.apiService.generateNestedPartialUpdateSingle<IMoveLockersToLockerBankDTO>(this.lockerBanksEndpoint, 'lockers'),
    },
    LockersWithLocks: {
      ...this.apiService.generateNestedResource<ILockerAndLockDTO, ICreateLockerAndLockDTO>(this.lockerBanksEndpoint, 'lockers-and-locks'),
    },
    Admins: {
      getMany: this.apiService.generateNestedResourcePageFilterSortRequest<ICardHolderDTO>(this.lockerBanksEndpoint, 'admins'),
      updateMany: this.apiService.generateNestedUpdateMany<EntityId>(this.lockerBanksEndpoint, 'admins'),
      deleteSingle: this.apiService.generateNestedDelete(this.lockerBanksEndpoint, 'admins'),
    },
    SpecialCards: {
      getMany: this.apiService.generateNestedResourcePageFilterSortRequest<ICardHolderAndCardCredentialsDTO>(this.lockerBanksEndpoint, 'special-cards'),
      updateMany: this.apiService.generateNestedUpdateMany<EntityId>(this.lockerBanksEndpoint, 'special-cards'),
    },
    LeaseUsers: {
      getMany: this.apiService.generateNestedResourcePageFilterSortRequest<ICardHolderAndCardCredentialsDTO>(this.lockerBanksEndpoint, 'lease-users'),
      updateMany: this.apiService.generateNestedUpdateMany<EntityId>(this.lockerBanksEndpoint, 'lease-users'),
    },
    ReferenceImages: {
      ...this.apiService.generateCrudEndpoints<IViewReferenceImageDTO, ICreateReferenceImageDTO, IUpdateReferenceImageDTO>(this.lockerBanksReferenceImagesEndpoint),
    },
  };

  Lockers = {
    ...this.apiService.generateCrudEndpoints<ILockerDTO, ICreateLockerDTO, IUpdateLockerDTO>(this.lockersEndpoint),
    // Lock: this.apiService.generateGetSingle<ILockDTO>(this.lockersEndpoint, 'lock'),
    Audits: this.apiService.generateNestedResourcePageFilterSortRequest<ILockerAuditDTO>(this.lockersEndpoint, 'audits'),
    WithLocationsAndLockerBanksAndLocks: this.apiService.generatePageFilterSortRequestWithCustomReturn<IGlobalLockerSearchResultDTO>(this.endpointFor('lockers/with-location-locker-bank-lock')),
    Leases: {
      getSingle: this.apiService.generateGetSingle<ILockerLeaseDTO>(this.endpointFor('lockers/leases')),
      getMany: this.apiService.generateNestedResourcePageFilterSortRequest<ILockerLeaseDTO>(this.lockersEndpoint, 'leases'),
    },
    Owners: {
      getMany: this.apiService.generateNestedResourcePageFilterSortRequest<ICardHolderAndCardCredentialsDTO>(this.lockersEndpoint, 'owners'),
      updateManyForSmartLocker: this.apiService.generateNestedUpdateMany<EntityId>(this.lockersEndpoint, 'owners/card-credentials'),
      updateManyForDumbLocker: this.apiService.generateNestedUpdateMany<EntityId>(this.lockersEndpoint, 'owners/card-holders'),
    }
  };

  Locks = {
    ...this.apiService.generateCrudEndpoints<ILockDTO, ICreateLockDTO, IUpdateLockDTO>(this.locksEndpoint),
    partialUpdateSingle: (lockId: EntityId, body: { lockerId: EntityId | null }) => this.apiService.patch(this.locksEndpoint, { body, requestSettings: { urlFragments: [lockId] } }),
    ForceConfigUpdate: (lockId: EntityId) => this.apiService.put(this.locksEndpoint, { requestSettings: { urlFragments: [lockId, 'config'] } }),
    IsConfigPending: (lockId: EntityId) => this.apiService.get(this.locksEndpoint, { requestSettings: { urlFragments: [lockId, 'config-pending'] } }).pipe(map(x => x?.data)),
  };

  CardHolders = {
    ...this.apiService.generateCrudEndpoints<ICardHolderDTO, ICreateCardHolderDTO, IUpdateCardHolderDTO>(this.cardHoldersEndpoint),
    LockerLeases: this.apiService.generateNestedResourcePageFilterSortRequest<ILockerLeaseDTO>(this.cardHoldersEndpoint, 'locker-leases'),
    CardCredentials: 
    {
      ...this.apiService.generateNestedResource<ICardCredentialDTO, ICreateLockerCardCredentialDTO>(this.cardHoldersEndpoint, 'card-credentials'),
      getSingleFromCCure: this.apiService.generateGetSingle<ICardCredentialDTO>(this.endpointFor('card-holders/card-credentials-ccure')),
    },
    WithUserCardCredentials: this.apiService.generatePageFilterSortRequest<ICardHolderAndCardCredentialsDTO>(this.endpointFor('card-holders/with-user-card-credentials')),
    WithSpecialCardCredentials: this.apiService.generatePageFilterSortRequest<ICardHolderAndCardCredentialsDTO>(this.endpointFor('card-holders/with-special-card-credentials')),
    
  };

  CardCredentials = {
    ...this.apiService.generateCrudEndpoints<ICardCredentialDTO, ICreateCardCredentialDTO, IUpdateCardCredentialDTO>(this.endpointFor('card-credentials')),
  };

  LockerBankAdmins = {
    getMany: this.apiService.generatePageFilterSortRequest<ILockerBankAdminDTO>(this.endpointFor('locker-bank-admins')),
    Locations: {
      getMany: this.apiService.generatePageFilterSortRequest<ILocationDTO>(this.endpointFor('locker-bank-admins/locations')),
      LockerBanks: this.apiService.generateNestedResourcePageFilterSortRequest<ILockerBankDTO>(this.endpointFor('locker-bank-admins/locations'), 'locker-banks')
    },
    LockerBanks: this.apiService.generatePageFilterSortRequest<ILockerBankDTO>(this.endpointFor('locker-bank-admins/locker-banks')),
    LockerBankSummaries: this.apiService.generatePageFilterSortRequest<ILockerBankAdminLocationSummaryDTO>(this.endpointFor('locker-bank-admins/locker-bank-summaries'))
  };

  LockerLeases = {
    getMany: this.apiService.generatePageFilterSortRequest<ILockerLeaseDTO>(this.lockerLeasesEndpoint),
  };

  BulkOperations = {
    createManyCardHolderAndCardCredentialPairs: this.apiService.generateCreateSingle<null, ICreateBulkCardHolderAndCardCredentialsDTO>(this.endpointFor('bulk-operations/card-holder-and-credentials')),
  };

  ngOnDestroy(): void {
    this.currentTenantSubscription.unsubscribe();
  }

  // NOTE: Requires i18n
  private readonly errorMessageNoTenantSelected = 'You do not have a tenant selected. Please select a tenant and try again';

  private endpointFor(endpoint?: string): ((baseUrl: string) => string) {
    return baseUrl => {
      if(endpoint) {
        if(!this.currentTenantId) {
          throw new UnassignedTenantsAuthError(this.errorMessageNoTenantSelected);
        }
        return `${baseUrl}/tenants/${this.currentTenantId}/${endpoint}`;
      }
      return `${baseUrl}/tenants`;
    }
  }
}

