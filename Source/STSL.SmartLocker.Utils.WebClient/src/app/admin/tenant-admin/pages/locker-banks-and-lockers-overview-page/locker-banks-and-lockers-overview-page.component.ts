import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Subject, concatMap, map } from 'rxjs';
import { ISelectCardHoldersDialogData } from 'src/app/admin/admin-common/dialogs/view-entities/select-card-holders-dialog/select-card-holders-dialog.component';
import { ISelectCardHoldersOrCardCredentialsDialogData, getCardCredentialIdsOnly } from 'src/app/admin/admin-common/dialogs/view-entities/select-card-holders-or-card-credentials-dialog/select-card-holders-or-card-credentials-dialog.component';
import { entityFullPermissions } from 'src/app/admin/admin-common/models/entity-views';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { checkEntityIdEquality, useLoading } from 'src/app/shared/lib/utilities';
import { PagingRequest } from 'src/app/shared/models/api';
import { DomainConstants } from 'src/app/shared/models/common';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';

const enum TabIndex {
  Locations = 0,
  LockerBanks = 1,
  Lockers = 2
};

@Component({
  selector: 'tenant-admin-locker-banks-and-lockers-overview-page',
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

  openReplaceLockerBankAdminsDialog(lockerBank: ILockerBankDTO) {
    const dialogSettings: ISelectCardHoldersDialogData = {
      currentlySelectedCardHolders: this.adminApiService.LockerBanks.Admins.getMany(lockerBank.id).pipe(map(({ results }) => results)),
      requireCardHolderEmail: true
    };

    this.adminDialogService.openSelectCardHoldersDialog(dialogSettings).pipe(
      concatMap(assigned => this.adminApiService.LockerBanks.Admins.updateMany(lockerBank.id, assigned.map(({ id }) => id))),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
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
}
