import { Component } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  Lockers = 2,
};

@Component({
  selector: 'locker-bank-admin-locker-banks-and-lockers-overview-page',
  templateUrl: './locker-banks-and-lockers-overview-page.component.html',
})
export class LockerBanksAndLockersOverviewPageComponent {

  // NOTE: Page currently unused; can probably be ported over to TenantAdmin though,
  // so I'm keeping it around for now but it's not included in bundle / module.

  private readonly currentlySelectedLocationSubject = new BehaviorSubject<ILocationDTO | undefined>(undefined);
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(undefined);
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.Locations;

  viewLockerBanksForLocation(location: ILocationDTO) {
    this.currentlySelectedLocationSubject.next(location);
    this.selectedTabIndex = TabIndex.LockerBanks;
  }

  viewLockerStatusesForLockerBank(lockerBank: ILockerBankDTO) {
    this.currentlySelectedLockerBankSubject.next(lockerBank);
    this.selectedTabIndex = TabIndex.Lockers;
  }
}
