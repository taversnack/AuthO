import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
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
  selector: 'tenant-admin-locker-statuses-overview-page',
  templateUrl: './locker-statuses-overview-page.component.html',
})
export class LockerStatusesOverviewPageComponent {

  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);

  selectedTabIndex: TabIndex = TabIndex.Locations;

  private readonly currentlySelectedLocationSubject = new BehaviorSubject<ILocationDTO | undefined>(undefined);
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(undefined);
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

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
}
