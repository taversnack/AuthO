import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Subject, combineLatest, concatMap, filter, takeUntil, tap } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { IPagingRequest } from 'src/app/shared/models/api';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  LockerStatuses = 2,
};

@Component({
  selector: 'locker-bank-admin-locker-statuses-overview-page',
  templateUrl: './locker-statuses-overview-page.component.html',
})
export class LockerStatusesOverviewPageComponent implements OnInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly authService = inject(AuthService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly destroyRef = inject(DestroyRef);

  selectedTabIndex: TabIndex = TabIndex.Locations;

  private readonly currentlySelectedLocationSubject = new BehaviorSubject<ILocationDTO | undefined>(undefined);
  readonly currentlySelectedLocation$ = this.currentlySelectedLocationSubject.asObservable();

  private readonly currentlySelectedLockerBankSubject = new BehaviorSubject<ILockerBankDTO | undefined>(undefined);
  readonly currentlySelectedLockerBank$ = this.currentlySelectedLockerBankSubject.asObservable();

  hideLocationsTab = false;
  hideLockerBanksTab = false;

  ngOnInit(): void {
    // Show only lockers tab if 1 locker bank, else show list of locker banks, only show list of locations if multiple locations.
    const page: IPagingRequest = { pageIndex: 0, recordsPerPage: 2 };

    const isLoading$ = new Subject<boolean>();
    const finishLoading = () => { isLoading$.next(false); isLoading$.complete() };
    const unregisterLoadingCleanup = this.destroyRef.onDestroy(finishLoading);
    const finishLoadingAndCleanup = () => { unregisterLoadingCleanup(); finishLoading(); };

    this.adminDialogService.showIsLoading(isLoading$);

    this.authService.currentTenantDetails$.pipe(
      filter(x => x?.name !== ''),
      concatMap(() => combineLatest([
        this.adminApiService.LockerBankAdmins.LockerBanks({ page }).pipe(
          tap(lockerBanks => {
            if(lockerBanks.results.length === 1) {
              finishLoadingAndCleanup();
              this.hideLocationsTab = true;
              this.hideLockerBanksTab = true;
              this.viewLockerStatusesForLockerBank(lockerBanks.results[0]);
            }
          }),
          takeUntil(isLoading$),
          takeUntilDestroyed(this.destroyRef)
        ),
        this.adminApiService.LockerBankAdmins.Locations.getMany({ page }).pipe(
            tap(locations => {
              if(locations.results.length === 1 && !this.hideLockerBanksTab) {
                this.hideLocationsTab = true;
                this.viewLockerBanksForLocation(locations.results[0]);
              }
          }),
          takeUntil(isLoading$),
          takeUntilDestroyed(this.destroyRef),
        )
      ]))
    ).subscribe({ next: finishLoadingAndCleanup, error: finishLoadingAndCleanup });
  }

  viewLockerBanksForLocation(location?: ILocationDTO) {
    this.currentlySelectedLocationSubject.next(location);
    this.selectedTabIndex = TabIndex.LockerBanks;
  }

  viewLockerStatusesForLockerBank(lockerBank: ILockerBankDTO) {
    this.currentlySelectedLockerBankSubject.next(lockerBank);
    this.selectedTabIndex = TabIndex.LockerStatuses;
  }

  // Placeholder in case it's needed by locker bank admins
  /*
  viewLeaseHistoryForLocker(lockerStatus: ILockerStatusDTO) {
    const isSmartLocker = lockerStatus.securityType === LockerSecurityType.SmartLock;
    this.adminDialogService.openViewLockerLeasesDialog({
      hideLockerColumns: true,
      hideLockColumns: !isSmartLocker,
      hideCardCredentialColumns: !isSmartLocker,
      useEndpoint: (pageFilterSort?: IPageFilterSort) => this.adminApiService.Lockers.Leases.getMany(lockerStatus.lockerId, pageFilterSort),
    });
  }
  */
}
