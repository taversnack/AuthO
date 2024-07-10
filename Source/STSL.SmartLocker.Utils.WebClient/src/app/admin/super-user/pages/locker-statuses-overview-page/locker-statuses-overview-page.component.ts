import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, concatMap, map, of } from 'rxjs';
import { ISelectLockersDialogData } from 'src/app/admin/admin-common/dialogs/view-entities/select-lockers-dialog/select-lockers-dialog.component';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { useLoading } from 'src/app/shared/lib/utilities';
import { IPageFilterSort } from 'src/app/shared/models/api';
import { ILocationDTO } from 'src/app/shared/models/location';
import { LockerSecurityType } from 'src/app/shared/models/locker';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  LockerStatuses = 2,
};

@Component({
  selector: 'super-user-locker-statuses-overview-page',
  templateUrl: './locker-statuses-overview-page.component.html',
})
export class LockerStatusesOverviewPageComponent {

  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly destroyRef = inject(DestroyRef);

  selectedTabIndex: TabIndex = TabIndex.Locations;

  private readonly currentlySelectedLocationSubject = new BehaviorSubject<ILocationDTO | undefined>(undefined);
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(undefined);
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

  readonly LockerSecurityType = LockerSecurityType;

  viewLockerBanksForLocation(location?: ILocationDTO) {
    this.currentlySelectedLocationSubject.next(location);
    this.selectedTabIndex = TabIndex.LockerBanks;
  }

  viewLockerStatusesForLockerBank(lockerBank: ILockerBankDTO) {
    this.currentlySelectedLockerBankSubject.next(lockerBank);
    this.selectedTabIndex = TabIndex.LockerStatuses;
  }

  viewLeaseHistoryForLocker(lockerStatus: ILockerStatusDTO) {
    const isSmartLocker = lockerStatus.securityType === LockerSecurityType.SmartLock;
    this.adminDialogService.openViewLockerLeasesDialog({
      hideLockerColumns: true,
      hideLockColumns: !isSmartLocker,
      hideCardCredentialColumns: !isSmartLocker,
      useEndpoint: (pageFilterSort?: IPageFilterSort) => this.adminApiService.Lockers.Leases.getMany(lockerStatus.lockerId, pageFilterSort),
    });
  }

  editLock(lockerStatus: ILockerStatusDTO) {
    if(!lockerStatus.lockId) {
      return;
    }

    const { isLoading$, source$ } = useLoading(this.adminApiService.Locks.getSingle(lockerStatus.lockId).pipe(takeUntilDestroyed(this.destroyRef)));

    this.adminDialogService.showIsLoading(isLoading$);

    source$.pipe(
      concatMap(lock => lock ? this.adminDialogService.editLock({ lock }) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(edited => edited && this.refreshCurrentLockerStatusesView());
  }

  forceLockUpdate(lockerStatus: ILockerStatusDTO) {
    if(lockerStatus.lockId) {
      this.adminApiService.Locks.ForceConfigUpdate(lockerStatus.lockId).subscribe();
    }
  }

  assignOrReassignLockToLocker(lockerStatus: ILockerStatusDTO) {
    const { lockerId, lockId, lockSerialNumber } = lockerStatus;

    if(!lockId || !lockerId) {
      return;
    }

    const dialogSettings: ISelectLockersDialogData = lockerId ? {
      currentlySelectedLockers$: this.adminApiService.Lockers.getSingle(lockerId).pipe(map(locker => locker ? [locker] : [])),
      allowMultipleLockers: false,
    } : {};

    const title = (lockerId ? 'Reassign Lock ' : 'Assign Lock ') + lockSerialNumber;

    this.adminDialogService.openSelectLockersDialog(dialogSettings, title).pipe(
      concatMap(lockers => {
        const lockerId = lockers?.shift()?.id;
        return lockerId ? this.adminApiService.Locks.partialUpdateSingle(lockId, { lockerId }) : of(false)
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(updated => updated && this.refreshCurrentLockerStatusesView());
  }

  unassignLockFromLocker(lockerStatus: ILockerStatusDTO) {
    const { lockId, lockSerialNumber, label } = lockerStatus;

    if(!lockId) {
      return;
    }

    const lockerId = null;
    this.adminDialogService.confirm(`Are you sure you wish to remove lock ${lockSerialNumber} from ${label}?`).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.Locks.partialUpdateSingle(lockId, { lockerId }) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(updated => updated && this.refreshCurrentLockerStatusesView());
  }

  private refreshCurrentLockerStatusesView() {
    this.currentlySelectedLockerBankSubject.next(this.currentlySelectedLockerBankSubject.getValue());
  }
}
