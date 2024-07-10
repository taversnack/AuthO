import { AfterViewInit, ChangeDetectorRef, Component, DestroyRef, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, map, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { DialogBase } from 'src/app/dialog/directives/dialog-component-base.directive';
import { arrayWithRemovedIndex, arrayWithReplacedIndex, checkEntityIdEquality, lockFormatCsnToDecimal, sortByString } from 'src/app/shared/lib/utilities';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, PageFilterSortRequest, PagingResponse, SortOrder, SortedRequest } from 'src/app/shared/models/api';
import { ICardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ICardHolderAndCardCredentialsDTO } from 'src/app/shared/models/card-credential-and-card-holder';
import { ICardHolderDTO, defaultCardHolderAliasPlural, defaultCardHolderAliasSingular } from 'src/app/shared/models/card-holder';
import { DomainConstants, EntityId } from 'src/app/shared/models/common';
import { ITableSettings, StickyPosition, TableColumn, defaultTableSettings } from 'src/app/shared/models/data-table';


export const getCardHolderIdsOnly = (cardHoldersAndCredentials: ICardHolderAndCardCredentialsDTO[]): EntityId[] =>
  cardHoldersAndCredentials.map(({ cardHolder }) => cardHolder.id);

export const getCardCredentialIdsOnly = (cardHoldersAndCredentials: ICardHolderAndCardCredentialsDTO[]): EntityId[] =>
  cardHoldersAndCredentials.flatMap(x => x.cardCredentials ?? []).map(({ id }) => id);

export interface ISelectCardHoldersOrCardCredentialsDialogData {
  currentlySelectedCardHoldersAndCardCredentials$?: Observable<ICardHolderAndCardCredentialsDTO[]>;
  isLoadingCurrentlySelectedCardHoldersAndCardCredentials$?: Observable<boolean>;
  useEndpoint?: PageFilterSortRequest<ICardHolderAndCardCredentialsDTO>;
  /**
   * Disallow selecting card credentials as part of the maximum count which is the default behaviour.
   * Users only will be returned, card credentials arrays will always be empty.
   */
  selectCardHoldersOnly?: boolean;
  /**
   * Will allow selection of users without a card, will select the card holder only as no card credentials
   * are available.
   */
  allowUsersWithoutACard?: boolean;
  limitCardHolderCount?: number;
  limitCardHoldersCardCredentialCount?: number;
  limitTotalCardCount?: number;
  ensureCardSelectionsHaveSerialNumber?: boolean;
}

const enum TabIndex {
  CardHolders = 0,
  Selected = 1,
};

@Component({
  selector: 'admin-select-card-holders-or-card-credentials-dialog',
  templateUrl: './select-card-holders-or-card-credentials-dialog.component.html',
})
export class SelectCardHoldersOrCardCredentialsDialogComponent extends DialogBase<SelectCardHoldersOrCardCredentialsDialogComponent, ISelectCardHoldersOrCardCredentialsDialogData, ICardHolderAndCardCredentialsDTO[]> implements OnInit, AfterViewInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly authService = inject(AuthService);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('cardHolderActions') private cardHolderActionsColumn?: TemplateRef<ICardHolderAndCardCredentialsDTO>;
  @ViewChild('selectedCardHolderActions') private selectedCardHolderActionsColumn?: TemplateRef<ICardHolderAndCardCredentialsDTO>;

  readonly settings: Required<ISelectCardHoldersOrCardCredentialsDialogData> = {
    currentlySelectedCardHoldersAndCardCredentials$: of([]),
    isLoadingCurrentlySelectedCardHoldersAndCardCredentials$: of(false),
    useEndpoint: this.adminApiService.CardHolders.WithUserCardCredentials,
    selectCardHoldersOnly: false,
    allowUsersWithoutACard: false,
    limitCardHolderCount: DomainConstants.MaxUserCardCredentialsPerLocker,
    limitCardHoldersCardCredentialCount: 1,
    limitTotalCardCount: DomainConstants.MaxUserCardCredentialsPerLocker,
    ensureCardSelectionsHaveSerialNumber: true,
     ...this.data
  };

  cardHolderAliasSingular = this.authService.cardHolderAliasSingular;
  cardHolderAliasPlural = this.authService.cardHolderAliasPlural;
  cardHolderUniqueIdentifierAlias = this.authService.cardHolderUniqueIdentifierAlias;

  private readonly cardHoldersAndCardCredentialsSubject = new BehaviorSubject<IPagingResponse<ICardHolderAndCardCredentialsDTO> | undefined>(undefined);
  readonly cardHoldersAndCardCredentials$ = this.cardHoldersAndCardCredentialsSubject.asObservable();

  private readonly currentlySelectedCardHolderSubject = new BehaviorSubject<ICardHolderAndCardCredentialsDTO | undefined>(undefined);
  readonly currentlySelectedCardHolder$ = this.currentlySelectedCardHolderSubject.asObservable();

  readonly cardCredentialsForCurrentlySelectedCardHolder$ = this.currentlySelectedCardHolder$.pipe(map(x => x ? new PagingResponse(x.cardCredentials) : undefined));

  private readonly isLoadingCardHoldersAndCardCredentialsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingCardHoldersAndCardCredentials$ = this.isLoadingCardHoldersAndCardCredentialsSubject.asObservable();

  private readonly selectedCardHoldersAndCardCredentialsSubject = new BehaviorSubject<ICardHolderAndCardCredentialsDTO[]>([]);
  readonly selectedCardHoldersAndCardCredentialsAsPagingResponse$ = this.selectedCardHoldersAndCardCredentialsSubject.asObservable().pipe(map(x => new PagingResponse(x)));

  readonly cardHolderFilters = ['firstName', 'lastName', 'email', { name: 'uniqueIdentifier', display: this.cardHolderUniqueIdentifierAlias }];

  private readonly cardHolderBaseColumns: TableColumn<ICardHolderAndCardCredentialsDTO>[] = [
    { name: 'firstName', getValue: x => x.cardHolder.firstName, sortable: true },
    { name: 'lastName', getValue: x => x.cardHolder.lastName, sortable: true },
    { name: 'email', getValue: x => x.cardHolder.email, sortable: true },
    { name: 'uniqueIdentifier', headingDisplay: this.cardHolderUniqueIdentifierAlias, getValue: x => x.cardHolder.uniqueIdentifier, sortable: true },
  ];

  private readonly cardCredentialBaseColumns: TableColumn<ICardCredentialDTO>[] = [
    { name: 'serialNumber', getValue: x => x.serialNumber ? lockFormatCsnToDecimal(x.serialNumber) : undefined },
    'hidNumber',
    'cardType',
    'cardLabel',
  ];

  private readonly cardHolderColumnsSubject = new BehaviorSubject<TableColumn<ICardHolderAndCardCredentialsDTO>[]>(this.cardHolderBaseColumns);
  readonly cardHolderColumns$ = this.cardHolderColumnsSubject.asObservable();

  private readonly cardHolderAndCardCredentialColumnsSubject = new BehaviorSubject<TableColumn<ICardHolderAndCardCredentialsDTO>[]>(this.cardHolderBaseColumns);
  readonly cardHolderAndCardCredentialColumns$ = this.cardHolderAndCardCredentialColumnsSubject.asObservable();

  private readonly cardCredentialColumnsSubject = new BehaviorSubject<TableColumn<ICardCredentialDTO>[]>(this.cardCredentialBaseColumns);
  readonly cardCredentialColumns$ = this.cardCredentialColumnsSubject.asObservable();

  noPagingTableSettings: ITableSettings = { ...defaultTableSettings, disablePaging: true };

  private cardHolderAndCredentialsPageFilterSort = PageFilterSort.usingFilter(this.cardHolderFilters);

  readonly StickyPosition = StickyPosition;

  selectedTabIndex = TabIndex.CardHolders;

  disableCardsTab = true;

  selectedCardHoldersLength = 0;
  selectedCardCredentialsLength = 0;

  ngOnInit(): void {
    this.settings?.currentlySelectedCardHoldersAndCardCredentials$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => this.selectedCardHoldersAndCardCredentialsSubject.next(x));

    this.settings?.isLoadingCurrentlySelectedCardHoldersAndCardCredentials$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(x => this.isLoadingCardHoldersAndCardCredentialsSubject.next(this.isLoadingCardHoldersAndCardCredentialsSubject.getValue() || x));

    this.authService.currentTenantDetails$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => {
      this.cardHolderAliasSingular = x?.cardHolderAliasSingular ?? defaultCardHolderAliasSingular;
      this.cardHolderAliasPlural = x?.cardHolderAliasPlural ?? defaultCardHolderAliasPlural;
    });

    this.selectedCardHoldersAndCardCredentialsAsPagingResponse$.subscribe(x => {
      this.selectedCardHoldersLength = x?.totalRecords ?? 0;
      // doing this sort of counting on each change may not be viable for larger arrays but we are expecting this to be small
      // the advantage is
      this.selectedCardCredentialsLength = x.results.flatMap(({ cardCredentials }) => cardCredentials).length;
    });

    this.isLoadingCardHoldersAndCardCredentialsSubject.next(false);
  }

  ngAfterViewInit(): void {
    this.cardHolderColumnsSubject.next([
      ...this.cardHolderBaseColumns,
      { name: 'actions', template: this.cardHolderActionsColumn, sortable: false, stickyPosition: StickyPosition.End },
    ]);

    this.cardHolderAndCardCredentialColumnsSubject.next([
      ...this.cardHolderBaseColumns,
      // ...this.cardHolderBaseColumns.map(x => typeof x === 'string' ? x : ({ ...x, sortable: false })),
      // Card credential columns if necessary (could depend on this.settings)
      { name: 'actions', template: this.selectedCardHolderActionsColumn, sortable: false, stickyPosition: StickyPosition.End },
    ]);

    this.changeDetectorRef.detectChanges();
  }

  toggleSelectedCardHolder({ cardHolder, cardCredentials }: ICardHolderAndCardCredentialsDTO, isSelected: boolean) {

    const { selectCardHoldersOnly, allowUsersWithoutACard, limitCardHolderCount, limitTotalCardCount } = this.settings;

    // const totalCardLimitRemaining = limitTotalCardCount - this.selectedCardCredentialsLength < 0 ? 0 : limitTotalCardCount - this.selectedCardCredentialsLength;

    const selectedCardHoldersAndCardCredentials = this.selectedCardHoldersAndCardCredentialsSubject.getValue();

    const cardHolderAlreadySelectedIndex = selectedCardHoldersAndCardCredentials.findIndex(x => x.cardHolder.id === cardHolder.id);

    if(cardHolderAlreadySelectedIndex >= 0) {
      if(!isSelected) {
        this.selectedCardHoldersAndCardCredentialsSubject.next(arrayWithRemovedIndex(selectedCardHoldersAndCardCredentials, cardHolderAlreadySelectedIndex));
      }
    } else if(isSelected) {
      const currentSelection = limitCardHolderCount === 1 ? [] : selectedCardHoldersAndCardCredentials;
      
      if(this.selectedCardHoldersLength >= limitCardHolderCount && limitCardHolderCount > 1) {
        return;
      }

      if(selectCardHoldersOnly) {
        this.selectedCardHoldersAndCardCredentialsSubject.next([...currentSelection, { cardHolder, cardCredentials: [] }]);
      } else {
        const cardHolderFirstCardCredential = cardCredentials?.length ? cardCredentials[0] : undefined;

        if(cardHolderFirstCardCredential) {
          this.selectedCardHoldersAndCardCredentialsSubject.next([...currentSelection, { cardHolder, cardCredentials: [cardHolderFirstCardCredential] }]);
        } else if(allowUsersWithoutACard) {
          this.selectedCardHoldersAndCardCredentialsSubject.next([...currentSelection, { cardHolder, cardCredentials: [] }]);
        } else {
          // If there is no currently stored card credential and is a smart lock, do a C-Cure lookup to get credential
          this.getCardHolderCredentialsFromCCure(cardHolder);
        }
      }
    }
  }

  // TODO: Implement full logic for all settings & for use with selecting individual card credentials
  // where cardholder has multiple
  selectCardCredentialFromCardHolder(cardHolderAndCardCredentials: ICardHolderAndCardCredentialsDTO) {
    const { ensureCardSelectionsHaveSerialNumber, limitCardHoldersCardCredentialCount, limitTotalCardCount } = this.settings;
    const selectedCardHoldersAndCardCredentials = this.selectedCardHoldersAndCardCredentialsSubject.getValue();
    const selectedCardHolderIndex = selectedCardHoldersAndCardCredentials.findIndex(x => checkEntityIdEquality(x.cardHolder.id, cardHolderAndCardCredentials.cardHolder.id));
    const currentlySelectedCards = selectedCardHolderIndex >= 0 ? selectedCardHoldersAndCardCredentials[selectedCardHolderIndex].cardCredentials : [];

    this.adminDialogService.openSelectCardCredentialsFromList(
      {
        cardCredentials: cardHolderAndCardCredentials.cardCredentials ?? [],
        selectionLimit: Math.min(limitCardHoldersCardCredentialCount, (limitTotalCardCount - this.selectedCardCredentialsLength) + (currentlySelectedCards?.length ?? 0)),
        currentlySelectedCards,
        ensureCardSelectionsHaveSerialNumber
      }
    ).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardCredentials => {
      if(cardCredentials.length) {
        const cardHolderAndCardCredentialsToInsert: ICardHolderAndCardCredentialsDTO = { cardHolder: cardHolderAndCardCredentials.cardHolder, cardCredentials };
        this.selectedCardHoldersAndCardCredentialsSubject.next(
          selectedCardHolderIndex >= 0 ? arrayWithReplacedIndex(selectedCardHoldersAndCardCredentials, selectedCardHolderIndex, cardHolderAndCardCredentialsToInsert)
          :
          [...selectedCardHoldersAndCardCredentials, cardHolderAndCardCredentialsToInsert]
        );
      } else if(selectedCardHolderIndex >= 0) {
        this.selectedCardHoldersAndCardCredentialsSubject.next(arrayWithRemovedIndex(selectedCardHoldersAndCardCredentials, selectedCardHolderIndex));
      }
    });
  }

  removeCardHolderWithMultipleCardsSelected(cardHolderAndCardCredentials: ICardHolderAndCardCredentialsDTO) {
    const selectedCardHoldersAndCardCredentials = this.selectedCardHoldersAndCardCredentialsSubject.getValue();
    const selectedCardHolderIndex = selectedCardHoldersAndCardCredentials.findIndex(x => checkEntityIdEquality(x.cardHolder.id, cardHolderAndCardCredentials.cardHolder.id));
    if(selectedCardHolderIndex >= 0) {
      this.selectedCardHoldersAndCardCredentialsSubject.next(arrayWithRemovedIndex(selectedCardHoldersAndCardCredentials, selectedCardHolderIndex));
    }
  }

  confirmSelection() {
    this.close(this.selectedCardHoldersAndCardCredentialsSubject.getValue());
  }

  resetSelection() {
    this.selectedCardHoldersAndCardCredentialsSubject.next([]);
  }

  cardHolderFiltersChanged(filterProperties: string[]) {
    this.cardHolderAndCredentialsPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchCardHolders(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    if (query) {
      this.cardHolderAndCredentialsPageFilterSort.filter.filterValue = query;
        if (query.length < 3) {
        this.cardHolderAndCredentialsPageFilterSort.filter.startsWith = true;
        } else {
        this.cardHolderAndCredentialsPageFilterSort.filter.startsWith = false;
      }
      this.getCardHoldersAndCardCredentials();
     } else {
      this.cardHoldersAndCardCredentialsSubject.next({ ...new PagingResponse<ICardHolderAndCardCredentialsDTO>(), results: [] });
      this.isLoadingCardHoldersAndCardCredentialsSubject.next(false);
     }
  }

  onCardHolderPagingEvent(page: IPagingRequest) {
    this.cardHolderAndCredentialsPageFilterSort.page = page;
    this.getCardHoldersAndCardCredentials();
  }

  onCardHolderSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.cardHolderAndCredentialsPageFilterSort.sort = sort;
    this.getCardHoldersAndCardCredentials();
  }

  onSelectedCardHolderSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    const { sortBy, sortOrder } = sort;

    const descending = sortOrder === SortOrder.Descending;

    const x = [...this.selectedCardHoldersAndCardCredentialsSubject.getValue()];

    switch(sortBy) {
      case 'firstName': this.selectedCardHoldersAndCardCredentialsSubject.next(x.sort((a, b) => sortByString(a.cardHolder.firstName, b.cardHolder.firstName, descending))); return;
      case 'email': this.selectedCardHoldersAndCardCredentialsSubject.next(x.sort((a, b) => sortByString(a.cardHolder.email ?? '', b.cardHolder.email ?? '', descending))); return;
      case 'uniqueId': this.selectedCardHoldersAndCardCredentialsSubject.next(x.sort((a, b) => sortByString(a.cardHolder.uniqueIdentifier ?? '', b.cardHolder.uniqueIdentifier ?? '', descending))); return;
      default: this.selectedCardHoldersAndCardCredentialsSubject.next(x.sort((a, b) => sortByString(a.cardHolder.lastName, b.cardHolder.lastName, descending))); return;
    }
  }

  isCardHolderChecked(cardHolder: ICardHolderDTO): boolean {
    return this.selectedCardHoldersAndCardCredentialsSubject.getValue()?.some(x => checkEntityIdEquality(x.cardHolder.id, cardHolder.id)) ?? false;
  }

  private getCardHoldersAndCardCredentials() {
    const onResponse = {
        next: (x: IPagingResponse<ICardHolderAndCardCredentialsDTO>) => {
            this.cardHoldersAndCardCredentialsSubject.next(x);
            this.isLoadingCardHoldersAndCardCredentialsSubject.next(false);
        },
        error: () => this.isLoadingCardHoldersAndCardCredentialsSubject.next(false),
    };

    if (this.cardHolderAndCredentialsPageFilterSort.filter.filterValue) {
        this.isLoadingCardHoldersAndCardCredentialsSubject.next(true);
        this.settings.useEndpoint(this.cardHolderAndCredentialsPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
    } else {
        this.cardHoldersAndCardCredentialsSubject.next({ ...new PagingResponse<ICardHolderAndCardCredentialsDTO>(), results: [] });
        this.isLoadingCardHoldersAndCardCredentialsSubject.next(false);
    }
  }

  //TODO: Include refactor of this function when re-designing modal
  private getCardHolderCredentialsFromCCure(cardHolder: ICardHolderDTO) {

    this.isLoadingCardHoldersAndCardCredentialsSubject.next(true);

    this.adminApiService.CardHolders.CardCredentials
      .getSingleFromCCure(cardHolder.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(
      {
        next: cardCredential => {
          if(cardCredential != null) // should never be null due to 404 Error 
          {
            const currentSelection = this.selectedCardHoldersAndCardCredentialsSubject.getValue();
            this.selectedCardHoldersAndCardCredentialsSubject.next([...currentSelection, { cardHolder, cardCredentials: [cardCredential] }]);
            this.isLoadingCardHoldersAndCardCredentialsSubject.next(false);
            // Refresh list for newly created card credentials
            //TODO: Probably dont need to requery the database for this, maybe just append the new card credential to the card holder to avoid re-query?
            this.getCardHoldersAndCardCredentials()
          }
        },
        error: () => {
          this.isLoadingCardHoldersAndCardCredentialsSubject.next(false);

          this.adminDialogService.error('No valid card credentials found for '+ cardHolder.firstName + " " + cardHolder.lastName, 'Unable to assign ' + this.cardHolderAliasSingular + ' to locker');
           // Refresh list to reset checkbox selection state
            //TODO: Probably dont need to requery the database for this, Issue was that the checkbox state wasn't being updated, even when no card holders were added to the selected list
          this.getCardHoldersAndCardCredentials()

        }
      });
  }
}
