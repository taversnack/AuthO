import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { entityFullPermissions } from 'src/app/admin/admin-common/models/entity-views';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  Admins = 2
};

@Component({
  selector: 'super-user-locker-bank-admins-overview-page',
  templateUrl: './locker-bank-admins-overview-page.component.html',
})
export class LockerBankAdminsOverviewPageComponent {

  private readonly currentlySelectedLocationSubject = new Subject<ILocationDTO | undefined>();
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new Subject<ILockerBankDTO | undefined>();
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.Locations;

  readonly lockerBankAdminPermissions = entityFullPermissions;

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

  viewAdminsForLockerBank(lockerBank?: ILockerBankDTO) {
    this.currentlySelectedLockerBankSubject.next(lockerBank);
    this.selectedTabIndex = TabIndex.Admins;
  }
}
