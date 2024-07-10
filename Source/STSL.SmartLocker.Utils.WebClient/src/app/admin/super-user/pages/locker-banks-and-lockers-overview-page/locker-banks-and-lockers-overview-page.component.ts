import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Subject, concatMap, map, of } from 'rxjs';
import { ISelectCardHoldersDialogData } from 'src/app/admin/admin-common/dialogs/view-entities/select-card-holders-dialog/select-card-holders-dialog.component';
import { ISelectCardHoldersOrCardCredentialsDialogData, getCardCredentialIdsOnly } from 'src/app/admin/admin-common/dialogs/view-entities/select-card-holders-or-card-credentials-dialog/select-card-holders-or-card-credentials-dialog.component';
import { ISelectLocksDialogData } from 'src/app/admin/admin-common/dialogs/view-entities/select-locks-dialog/select-locks-dialog.component';
import { entityFullPermissions } from 'src/app/admin/admin-common/models/entity-views';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { checkEntityIdEquality, useLoading } from 'src/app/shared/lib/utilities';
import { PagingRequest } from 'src/app/shared/models/api';
import { DomainConstants } from 'src/app/shared/models/common';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ICreateLockerDTO, LockerSecurityType } from 'src/app/shared/models/locker';
import { ILockerAndLockDTO } from 'src/app/shared/models/locker-and-lock';
import { ILockerBankDTO, LockerBankBehaviour } from 'src/app/shared/models/locker-bank';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  Lockers = 2
};

@Component({
  selector: 'super-user-locker-banks-and-lockers-overview-page',
  templateUrl: './locker-banks-and-lockers-overview-page.component.html',
})
export class LockerBanksAndLockersOverviewPageComponent {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly currentlySelectedLocationSubject = new Subject<ILocationDTO | undefined>();
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(undefined);
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.Locations;

  readonly locationPermissions = entityFullPermissions;
  readonly lockerBankPermissions = entityFullPermissions;
  readonly lockerPermissions = entityFullPermissions;

  readonly LockerSecurityType = LockerSecurityType;

  viewAllLockerBanks() {
    this.currentlySelectedLocationSubject.next(undefined);
  }

  viewAllLockers() {
    this.currentlySelectedLockerBankSubject.next(undefined);
  }

  viewLockerBanksForLocation(location?: ILocationDTO) {
    this.currentlySelectedLocationSubject.next(location);
    this.selectedTabIndex = TabIndex.LockerBanks;
  }

  viewLockersForLockerBank(lockerBank?: ILockerBankDTO) {
    this.currentlySelectedLockerBankSubject.next(lockerBank);
    this.selectedTabIndex = TabIndex.Lockers;
  }

  addSpecialCardCredentialsForLockerBank(lockerBank: ILockerBankDTO) {
    const lockerBankId = lockerBank.id;

    const { isLoading$, source$ } = useLoading(
      this.adminApiService.LockerBanks.SpecialCards.getMany(lockerBankId, { page: new PagingRequest(DomainConstants.MaxSpecialCardCredentialsPerLocker) }).pipe(map(x => x.results))
    );

    const dialogSettings: ISelectCardHoldersOrCardCredentialsDialogData = {
      useEndpoint: this.adminApiService.CardHolders.WithSpecialCardCredentials,
      currentlySelectedCardHoldersAndCardCredentials$: source$,
      isLoadingCurrentlySelectedCardHoldersAndCardCredentials$: isLoading$,
      limitTotalCardCount: DomainConstants.MaxSpecialCardCredentialsPerLocker,
      limitCardHoldersCardCredentialCount: DomainConstants.MaxSpecialCardCredentialsPerLocker,
      // allowUsersWithoutACard: false,
      // selectCardHoldersOnly: false,
    };

    this.adminDialogService.openSelectCardHoldersOrCardCredentialsDialog(dialogSettings).pipe(
      concatMap(assignedCardHoldersAndCredentials => {
        const { isLoading$, source$ } = useLoading(this.adminApiService.LockerBanks.SpecialCards.updateMany(lockerBankId, getCardCredentialIdsOnly(assignedCardHoldersAndCredentials)));
        this.adminDialogService.showIsLoading(isLoading$);
        return source$;
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe();
  }

  openReplaceLockerBankAdminsDialog(lockerBank: ILockerBankDTO) {
    const dialogSettings: ISelectCardHoldersDialogData = {
      currentlySelectedCardHolders: this.adminApiService.LockerBanks.Admins.getMany(lockerBank.id).pipe(map(({ results }) => results)),
      requireCardHolderEmail: true,
      allowMultipleCardHolders: true,
    };

    this.adminDialogService.openSelectCardHoldersDialog(dialogSettings).pipe(
      concatMap(assigned => this.adminApiService.LockerBanks.Admins.updateMany(lockerBank.id, assigned.map(({ id }) => id))),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  editLock(lockerAndLock: ILockerAndLockDTO) {
    if(lockerAndLock.lock) {
      const lock = lockerAndLock.lock;
      this.adminDialogService.editLock({ lock }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.refreshLockersList());
    }
  }

  forceLockUpdate(lockerAndLock: ILockerAndLockDTO) {
    if(lockerAndLock.lock) {
      this.adminApiService.Locks.ForceConfigUpdate(lockerAndLock.lock.id).subscribe(success => this.adminDialogService.alert('Config update has been queued'));
    }
  }

  assignLockToLocker(lockerAndLock: ILockerAndLockDTO) {
    this.assignLockFromSelectionDialog('Assign lock to ' + lockerAndLock.locker.label, lockerAndLock);
  }

  replaceLockForLocker(lockerAndLock: ILockerAndLockDTO) {
    this.assignLockFromSelectionDialog('Replace lock for ' + lockerAndLock.locker.label, lockerAndLock, true);
  }

  removeLockFromLocker(lockerAndLock: ILockerAndLockDTO) {
    if(!lockerAndLock.lock) {
      return;
    }

    const { id: lockId, serialNumber } = lockerAndLock.lock;
    const lockerId = null;

    const message = `Are you sure you wish to remove  ${serialNumber} from ${lockerAndLock.locker.label}?`;
    const title = 'Remove lock?';

    this.adminDialogService.confirm(message, title).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.Locks.partialUpdateSingle(lockId, { lockerId }) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(updated => updated && this.refreshLockersList());
  }

  private assignLockFromSelectionDialog(title: string, lockerAndLock: ILockerAndLockDTO, useCurrentlyAssignedLock = false) {

    const lockerId = lockerAndLock.locker.id;

    let dialogSettings: ISelectLocksDialogData = {
      allowMultipleLocks: false,
      currentlySelectedLocks$: useCurrentlyAssignedLock ? of(lockerAndLock.lock ? [lockerAndLock.lock] : []) : undefined
    };

    this.adminDialogService.openSelectLocksDialog(dialogSettings, title)
    .pipe(
      concatMap(assignedLocks => {
        if(assignedLocks.length) {
          const { id: newLockId } = assignedLocks[0];
          return this.adminApiService.Locks.partialUpdateSingle(newLockId, { lockerId });
        }
        return of(false);
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(updated => updated && this.refreshLockersList());
  }

  createManyLockersFromCSV(lockerBank: ILockerBankDTO) {
    const { id: lockerBankId, behaviour } = lockerBank;

    const LABEL = 'label';
    const SERVICE_TAG = 'serviceTag';
    const SECURITY_TYPE = 'type';

    this.adminDialogService.getCsvContents([LABEL, SERVICE_TAG, SECURITY_TYPE]).pipe(
      concatMap(lockers => {
        if(lockers) {
          // If this is a temporary locker, only allow smart lock type lockers to be created
          const mapper: (locker: { [key: string]: string }) => ICreateLockerDTO = behaviour === LockerBankBehaviour.Temporary ?
          lockers => ({ lockerBankId, label: lockers[LABEL], serviceTag: lockers[SERVICE_TAG], securityType: LockerSecurityType.SmartLock })
          :
          lockers => {
            const intSecurityType = lockers[SECURITY_TYPE] ? parseInt(lockers[SECURITY_TYPE]) : LockerSecurityType.SmartLock;
            return { lockerBankId, label: lockers[LABEL], serviceTag: lockers[SERVICE_TAG], securityType: isNaN(intSecurityType) ? LockerSecurityType.SmartLock : intSecurityType };
          };

          const mappedLockers = lockers.map<ICreateLockerDTO>(mapper);

          return this.adminApiService.LockerBanks.Lockers.createMany(lockerBank.id, mappedLockers);
        }
        return of();
      }),
      takeUntilDestroyed(this.destroyRef)
    )
    .subscribe(created => created && this.refreshLockersList());
  }

  moveLockersFromLockerBank(originLockerBank: ILockerBankDTO) {
    this.adminDialogService.moveLockersFromOneBankToAnother(originLockerBank).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(({ destinationLockerBank }) => {
      const currentlySelectedLockerBank = this.currentlySelectedLockerBankSubject.getValue();
      // If destination or origin bank is currently selected and our request succeeded we should reload
      // because the lockers list could be out of date.
      if(currentlySelectedLockerBank !== undefined &&
        (
          checkEntityIdEquality(currentlySelectedLockerBank.id, originLockerBank.id) ||
          (destinationLockerBank !== undefined && checkEntityIdEquality(currentlySelectedLockerBank.id, destinationLockerBank.id))
        )) {
          this.currentlySelectedLockerBankSubject.next(currentlySelectedLockerBank);
      }
    });
  }

  private refreshLockersList() {
    this.currentlySelectedLockerBankSubject.next(this.currentlySelectedLockerBankSubject.getValue());
  }

  // bulkEditLockers(lockerBank: ILockerBankDTO) {
  //   this.adminDialogService.alert('Arriving before v2.0.0', 'Coming Soon!');
  // }
}
