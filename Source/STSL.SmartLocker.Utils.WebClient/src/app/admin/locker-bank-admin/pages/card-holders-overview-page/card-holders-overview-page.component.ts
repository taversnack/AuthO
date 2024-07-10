import { Component, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from 'src/app/auth/services/auth.service';
import { ICardHolderDTO } from 'src/app/shared/models/card-holder';

const enum TabIndex {
  CardHolders = 0,
  CardCredentials = 1,
};

@Component({
  selector: 'locker-bank-admin-card-holders-overview-page',
  templateUrl: './card-holders-overview-page.component.html',
})
export class CardHoldersOverviewPageComponent {

  private readonly authService = inject(AuthService);

  readonly cardHolderAliasPlural$ = this.authService.cardHolderAliasPlural$;

  private readonly currentlySelectedCardHolderSubject = new BehaviorSubject<ICardHolderDTO | undefined>(undefined);
  readonly currentlySelectedCardHolder$ = this.currentlySelectedCardHolderSubject.asObservable();

  selectedTabIndex: TabIndex = TabIndex.CardHolders;

  viewCardCredentialsForCardHolder(cardHolder: ICardHolderDTO) {
    this.currentlySelectedCardHolderSubject.next(cardHolder);
    this.selectedTabIndex = TabIndex.CardCredentials;
  }
}
