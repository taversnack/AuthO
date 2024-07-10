import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, concatMap, map } from 'rxjs';
import { IEntityPermissions, entityFullPermissions } from 'src/app/admin/admin-common/models/entity-views';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { IPageFilterSort } from 'src/app/shared/models/api';
import { CardType, ICreateCardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ICardHolderDTO, ICreateCardHolderDTO } from 'src/app/shared/models/card-holder';

const enum TabIndex {
  CardHolders = 0,
  CardCredentials = 1,
};

@Component({
  selector: 'super-user-card-holders-and-card-credentials-overview-page',
  templateUrl: './card-holders-and-card-credentials-overview-page.component.html',
})
export class CardHoldersAndCardCredentialsOverviewPageComponent implements OnInit {

  private readonly authService = inject(AuthService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly destroyRef = inject(DestroyRef);

  cardHolderAliasPlural = this.authService.cardHolderAliasPlural;

  readonly cardHolderPermissions: IEntityPermissions = entityFullPermissions;
  readonly cardCredentialPermissions: IEntityPermissions = entityFullPermissions;

  private readonly currentlySelectedCardHolderSubject = new BehaviorSubject<ICardHolderDTO | undefined>(undefined);
  readonly currentlySelectedCardHolder$ = this.currentlySelectedCardHolderSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.CardHolders;

  ngOnInit(): void {
    this.authService.cardHolderAliasPlural$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => this.cardHolderAliasPlural = x);
  }

  viewAllCardCredentials() {
    this.currentlySelectedCardHolderSubject.next(undefined);
  }

  viewCardCredentialsForCardHolder(cardHolder: ICardHolderDTO) {
    this.currentlySelectedCardHolderSubject.next(cardHolder);
    this.selectedTabIndex = TabIndex.CardCredentials;
  }

  viewLockerLeasesForCardHolder(cardHolder: ICardHolderDTO) {
    this.adminDialogService.openViewLockerLeasesDialog({
      hideCardHolderColumns: true,
      useEndpoint: (pageFilterSort?: IPageFilterSort) => this.adminApiService.CardHolders.LockerLeases(cardHolder.id, pageFilterSort),
    });
  }

  createManyCardHolderAndCardCredentialPairsFromCSV() {
    const requiredFields = ['firstName', 'lastName', 'email', 'uniqueIdentifier', 'serialNumber', 'hidNumber'];
    const cardType = CardType.User;
    const isVerified = false;

    this.adminDialogService.getCsvContents(requiredFields).pipe(map(rows => {

      const cardHolderAndCardCredentials = rows.map(x => {
        const { firstName, lastName, email, uniqueIdentifier, serialNumber, hidNumber } = x;
        const cardHolder: ICreateCardHolderDTO = { firstName, lastName, email: email || undefined, uniqueIdentifier, isVerified };
        const cardCredential: ICreateCardCredentialDTO = { serialNumber: serialNumber || undefined, hidNumber, cardType };
        return { cardHolder, cardCredential };
      });

      return { cardHolderAndCardCredentials };
    }),
    concatMap(x => this.adminApiService.BulkOperations.createManyCardHolderAndCardCredentialPairs(x)),
    takeUntilDestroyed(this.destroyRef)).subscribe({ complete: () => this.viewAllCardCredentials() });
  }
}
