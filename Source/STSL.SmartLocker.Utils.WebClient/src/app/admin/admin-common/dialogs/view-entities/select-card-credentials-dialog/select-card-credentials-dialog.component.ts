import { AfterViewInit, ChangeDetectorRef, Component, DestroyRef, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { AuthService } from 'src/app/auth/services/auth.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { lockFormatCsnToDecimal } from 'src/app/shared/lib/utilities';
import { IPagingResponse, PagingResponse } from 'src/app/shared/models/api';
import { CardType, ICardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ICardCredentialAndCardHolderDTO, ICardHolderAndCardCredentialsDTO } from 'src/app/shared/models/card-credential-and-card-holder';
import { ICardHolderDTO } from 'src/app/shared/models/card-holder';
import { StickyPosition, TableColumn } from 'src/app/shared/models/data-table';
import { IEntityPermissions } from '../../../models/entity-views';

export interface ISelectCardCredentialsDialogData {
  cardHolderPermissions?: IEntityPermissions;
  cardCredentialsPermissions?: IEntityPermissions;
  allowMultipleTenants?: boolean;
  allowMultipleCardCredentials?: boolean;
  allowCardCredentialEnrolment?: boolean;
  currentlySelectedCardCredentialAndCardHolders$?: Observable<IPagingResponse<ICardCredentialAndCardHolderDTO>>,
  isLoadingCurrentlySelectedCardCredentialsAndCardHolders$?: Observable<boolean>,
  showCurrentlySelectedCardCredentialsAndCardHolders?: boolean,
  useMinimalCardCredentialColumns?: boolean,
  cardTypeFilter?: CardType | null,
  showDecimalCsn?: boolean;
  showHexCsn?: boolean;
}

const defaultSelectCardCredentialsDialogData: Required<ISelectCardCredentialsDialogData> = {
  cardHolderPermissions: {},
  cardCredentialsPermissions: {},
  allowMultipleTenants: false,
  allowMultipleCardCredentials: false,
  allowCardCredentialEnrolment: true,
  currentlySelectedCardCredentialAndCardHolders$: of(new PagingResponse<ICardCredentialAndCardHolderDTO>()),
  isLoadingCurrentlySelectedCardCredentialsAndCardHolders$: of(false),
  showCurrentlySelectedCardCredentialsAndCardHolders: true,
  useMinimalCardCredentialColumns: true,
  cardTypeFilter: null,
  showDecimalCsn: true,
  showHexCsn: false,
}

const enum TabIndex {
  CardHolders = 0,
  CardCredentials = 1,
};

@Component({
  selector: 'admin-select-card-credentials-dialog',
  templateUrl: './select-card-credentials-dialog.component.html',
})
export class SelectCardCredentialsDialogComponent extends DialogBase<SelectCardCredentialsDialogComponent, ISelectCardCredentialsDialogData, ICardHolderAndCardCredentialsDTO[]> implements OnInit, AfterViewInit {

  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);

  @ViewChild('cardCredentialAndCardHolderActions') cardCredentialAndCardHolderActions?: TemplateRef<ICardCredentialAndCardHolderDTO>;
  @ViewChild('cardHolderNameColumn') cardHolderNameColumn?: TemplateRef<ICardCredentialAndCardHolderDTO>;
  @ViewChild('cardCredentialSerialAndHidNumbers') cardCredentialSerialAndHidNumbersColumn?: TemplateRef<ICardCredentialAndCardHolderDTO>;

  private readonly currentlySelectedCardHolderSubject = new BehaviorSubject<ICardHolderDTO | undefined>(undefined);
  readonly currentlySelectedCardHolder$ = this.currentlySelectedCardHolderSubject.asObservable();

  readonly settings: Required<ISelectCardCredentialsDialogData> = { ...defaultSelectCardCredentialsDialogData, ...this.data };

  cardHolderAliasSingular = this.authService.cardHolderAliasSingular;

  readonly lockFormatCsnToDecimal = lockFormatCsnToDecimal;

  selectedTabIndex: TabIndex = TabIndex.CardHolders;

  // TODO: Just use the ICardCredentialAndCardHolderDTO type instead
  selectedCardHoldersAndCredentials: ICardHolderAndCardCredentialsDTO[] = [];

  private readonly cardCredentialsAndCardHoldersColumnsSubject = new BehaviorSubject<TableColumn<ICardCredentialAndCardHolderDTO>[]>([]);
  readonly cardCredentialsAndCardHoldersColumns$ =  this.cardCredentialsAndCardHoldersColumnsSubject.asObservable();

  // TODO: Change the paging responses to just be arrays. Need to create a new local table component for local data sources.
  private readonly currentlySelectedCardCredentialsAndCardHoldersSubject = new BehaviorSubject<IPagingResponse<ICardCredentialAndCardHolderDTO> | undefined>(undefined);
  readonly currentlySelectedCardCredentialsAndCardHolders$ = this.currentlySelectedCardCredentialsAndCardHoldersSubject.asObservable();

  ngOnInit(): void {
    this.authService.cardHolderAliasSingular$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => this.cardHolderAliasSingular = x);

    this.settings.currentlySelectedCardCredentialAndCardHolders$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardCredentialAndCardHolders => {
      this.currentlySelectedCardCredentialsAndCardHoldersSubject.next(cardCredentialAndCardHolders);

      const unknownCardHolder: ICardHolderDTO = { firstName: 'unknown', lastName: this.cardHolderAliasSingular, isVerified: false, id: '' };
      const unknownCardHolderAndCredentials: ICardHolderAndCardCredentialsDTO = { cardHolder: unknownCardHolder, cardCredentials: [] };

      this.selectedCardHoldersAndCredentials = [];
      for(const { cardHolder, cardCredential } of cardCredentialAndCardHolders.results) {
        if(cardHolder) {
          this.toggleCardHolderAndCredential(cardHolder, cardCredential);
        } else {
          const cardCredentialExists = unknownCardHolderAndCredentials.cardCredentials?.find(({ id }) => cardCredential.id === id);
          if(!cardCredentialExists) {
            unknownCardHolderAndCredentials.cardCredentials ??= [];
            unknownCardHolderAndCredentials.cardCredentials.push(cardCredential);
          }
        }
      }

    });
  }

  ngAfterViewInit(): void {
    this.cardCredentialsAndCardHoldersColumnsSubject.next([
      { name: 'cardHolderName', headingDisplay: this.cardHolderAliasSingular + ' Name', template: this.cardHolderNameColumn, sortable: false },
      { name: 'cardHolderEmail', headingDisplay: this.cardHolderAliasSingular + ' Email', getValue: x => x.cardHolder?.email, sortable: false },
      { name: 'cardHolderUniqueIdentifier', headingDisplay: this.cardHolderAliasSingular + ' Unique Id', getValue: x => x.cardHolder?.uniqueIdentifier, sortable: false },
      { name: 'cardNumbers', template: this.cardCredentialSerialAndHidNumbersColumn, sortable: false },
      { name: 'cardType', getValue: x => x.cardCredential.cardType, sortable: false },
      { name: 'cardLabel', getValue: x => x.cardCredential.cardLabel, sortable: false },
      { name: 'actions', template: this.cardCredentialAndCardHolderActions, sortable: false, stickyPosition: StickyPosition.End },
    ]);
    // NOTE: Manual change detection is required if the table is instantiated with data prior to the columns being set
    // NG0100 ridiculousness; ViewChild requires the view to be initialized but pushing data in AfterViewInit causes problems
    // due to the async nature.
    this.changeDetector.detectChanges();
  }

  useCard(credential: ICardCredentialDTO, isSelected: boolean = true, overrideCurrentCardHolder?: ICardHolderDTO) {
    const holder = overrideCurrentCardHolder ?? this.currentlySelectedCardHolderSubject.getValue();
    if(!holder) {
      return;
    }

    this.toggleCardHolderAndCredential(holder, credential, isSelected);

    // TODO: Just use the ICardCredentialAndCardHolderDTO type instead, will avoid having to keep grouping, ungrouping card holders & credentials for the sake
    // of a little bit of normalization that actually hardly matters since we're only normalizing the cardHolder (reference type)..
    // it seemed like a good idea initially to group them but oh well, things change!
    const cardCredentialAndCardHolders = new PagingResponse(
      this.selectedCardHoldersAndCredentials.flatMap(({ cardHolder, cardCredentials }) => (cardCredentials ?? []).map(cardCredential => ({ cardHolder, cardCredential })))
    );

    this.currentlySelectedCardCredentialsAndCardHoldersSubject.next(cardCredentialAndCardHolders);
  }

  private toggleCardHolderAndCredential(cardHolder: ICardHolderDTO, cardCredential: ICardCredentialDTO, isSelected: boolean = true) {
    const selectedCardHolderIndex = this.selectedCardHoldersAndCredentials.findIndex(x => x.cardHolder.id === cardHolder.id);
    // The cardholder is not in the list, we add them & credential if isSelected
    if(selectedCardHolderIndex < 0) {
      if(isSelected) {
        this.settings.allowMultipleTenants ?
        this.selectedCardHoldersAndCredentials.push({ cardHolder, cardCredentials: [cardCredential] })
        :
        this.selectedCardHoldersAndCredentials = [{ cardHolder, cardCredentials: [cardCredential] }];
      }
      return;
    }

    // We have a cardHolder already in the list, we try to find if the credential is already in the list
    const selectedCardCredentialIndex = this.selectedCardHoldersAndCredentials[selectedCardHolderIndex]?.cardCredentials?.findIndex(({ id }) => id === cardCredential.id) ?? -1;

    if(isSelected) {
      if(selectedCardCredentialIndex < 0) {
        if(this.settings.allowMultipleCardCredentials) {
          this.selectedCardHoldersAndCredentials[selectedCardHolderIndex].cardCredentials ??= [];
          this.selectedCardHoldersAndCredentials[selectedCardHolderIndex].cardCredentials?.push(cardCredential);
        } else {
          this.selectedCardHoldersAndCredentials = [{ cardHolder, cardCredentials: [cardCredential] }];
        }
      }
    } else {
      if(selectedCardCredentialIndex >= 0) {
        if(this.selectedCardHoldersAndCredentials[selectedCardHolderIndex]?.cardCredentials?.length ?? 0 > 1) {
          this.selectedCardHoldersAndCredentials[selectedCardHolderIndex].cardCredentials ??= [];
          this.selectedCardHoldersAndCredentials[selectedCardHolderIndex].cardCredentials?.splice(selectedCardCredentialIndex, 1);
        } else {
          this.selectedCardHoldersAndCredentials.splice(selectedCardHolderIndex, 1);
        }
      }
    }
  }

  viewCardCredentialsForCardHolder(cardHolder: ICardHolderDTO) {
    this.currentlySelectedCardHolderSubject.next(cardHolder);
    this.selectedTabIndex = TabIndex.CardCredentials;
  }

  confirmCardCredentialSelection() {
    // TODO: [5] Format message for confirming card holders and their credentials
    const selectedCardHoldersAndCredentialsMessage = `${this.selectedCardHoldersAndCredentials.map(({ cardHolder: { firstName, lastName } }) => `${firstName} ${lastName}`)}`;
    this.dialogService.confirm(`Adding the following ${this.cardHolderAliasSingular} and their card credentials: ${selectedCardHoldersAndCredentialsMessage}`).subscribe(confirmed => {
      if(confirmed) {
        this.close(this.selectedCardHoldersAndCredentials);
      }
    });
  }

  resetCardCredentialSelection() {
    this.selectedCardHoldersAndCredentials = [];
    this.currentlySelectedCardCredentialsAndCardHoldersSubject.next(new PagingResponse());
  }

  isCardCredentialChecked(cardCredential: ICardCredentialDTO) {
    return this.selectedCardHoldersAndCredentials.some(x => x.cardHolder.id === cardCredential.cardHolderId && x.cardCredentials?.some(({ id }) => id === cardCredential.id));
  }

}
