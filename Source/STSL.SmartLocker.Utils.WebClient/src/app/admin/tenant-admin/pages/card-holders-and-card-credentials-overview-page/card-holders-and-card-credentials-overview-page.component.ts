import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { entityFullPermissions } from 'src/app/admin/admin-common/models/entity-views';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { IPageFilterSort } from 'src/app/shared/models/api';
import { ICardHolderDTO } from 'src/app/shared/models/card-holder';

const enum TabIndex {
  CardHolders = 0,
  CardCredentials = 1,
};

@Component({
  selector: 'tenant-admin-card-holders-and-card-credentials-overview-page',
  templateUrl: './card-holders-and-card-credentials-overview-page.component.html',
})
export class CardHoldersAndCardCredentialsOverviewPageComponent {

  private readonly authService = inject(AuthService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);

  cardHolderAliasPlural$ = this.authService.cardHolderAliasPlural$;

  private readonly currentlySelectedCardHolderSubject = new BehaviorSubject<ICardHolderDTO | undefined>(undefined);
  readonly currentlySelectedCardHolder$ = this.currentlySelectedCardHolderSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.CardHolders;

  readonly cardHolderPermissions = entityFullPermissions;
  readonly cardCredentialPermissions = entityFullPermissions;

  viewCardCredentialsForCardHolder(cardHolder?: ICardHolderDTO) {
    this.currentlySelectedCardHolderSubject.next(cardHolder);
    this.selectedTabIndex = TabIndex.CardCredentials;
  }

  viewLockerLeasesForCardHolder(cardHolder: ICardHolderDTO) {
    this.adminDialogService.openViewLockerLeasesDialog({
      hideCardHolderColumns: true,
      useEndpoint: (pageFilterSort?: IPageFilterSort) => this.adminApiService.CardHolders.LockerLeases(cardHolder.id, pageFilterSort),
    });
  }
}
