import { AfterViewInit, Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, concatMap, map, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { decimalCsnToLockFormat, lockFormatCsnToDecimal } from 'src/app/shared/lib/utilities';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { CardType, ICardCredentialDTO } from 'src/app/shared/models/card-credential';
import { ICardHolderDTO } from 'src/app/shared/models/card-holder';
import { StickyPosition, TableColumn } from 'src/app/shared/models/data-table';
import { ISelectCardHoldersDialogData } from '../../../dialogs/view-entities/select-card-holders-dialog/select-card-holders-dialog.component';
import { IEntityPermissions } from '../../../models/entity-views';

const minimalCardCredentialColumns: TableColumn<ICardCredentialDTO>[] = [
  'hidNumber',
  'cardLabel',
];

const fullCardCredentialColumns: TableColumn<ICardCredentialDTO>[] = [
  'serialNumber',
  'hidNumber',
  'cardType',
  'cardLabel',
  'cardHolderId'
];

@Component({
  selector: 'admin-card-credentials-view',
  templateUrl: './card-credentials-view.component.html',
})
export class CardCredentialsViewComponent implements OnInit, AfterViewInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('actions') actions?: TemplateRef<ICardCredentialDTO>;
  @ViewChild('serialNumberColumn') serialNumberColumn?: TemplateRef<ICardCredentialDTO>;

  @Input() permissions: IEntityPermissions = {};
  @Input() allowShowingAll = true;
  @Input() currentlySelectedCardHolder$: Observable<ICardHolderDTO | undefined> = of();
  @Input() cardCredentialActions?: TemplateRef<ICardCredentialDTO>;
  @Input() inlineActions = false;
  @Input() hideEnrolAction = false;
  @Input() useMinimalColumns = true;
  @Input() cardTypeFilter: CardType | null = null;
  @Input() disableReassignment = true;

  readonly lockFormatCsnToDecimal = lockFormatCsnToDecimal;

  @Output() currentlySelectedCardHolderChanged = new EventEmitter<ICardHolderDTO | undefined>();

  currentlySelectedCardHolder?: ICardHolderDTO;

  private readonly cardCredentialsSubject = new BehaviorSubject<IPagingResponse<ICardCredentialDTO> | undefined>(undefined);
  readonly cardCredentials$ = this.cardCredentialsSubject.asObservable();

  private readonly isLoadingCardCredentialsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingCardCredentials$ = this.isLoadingCardCredentialsSubject.asObservable();

  readonly cardCredentialFilters = ['hidNumber', 'cardLabel'];

  private cardCredentialPageFilterSort = PageFilterSort.usingFilter(this.cardCredentialFilters);

  readonly StickyPosition = StickyPosition;

  private readonly cardCredentialColumnsSubject = new BehaviorSubject<TableColumn<ICardCredentialDTO>[]>([]);
  readonly cardCredentialColumns$ = this.cardCredentialColumnsSubject.asObservable();

  showActionsButton = false;

  ngOnInit(): void {
    this.currentlySelectedCardHolder$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(cardHolder => {
      this.currentlySelectedCardHolder = cardHolder;
      this.cardCredentialPageFilterSort.resetQuery();
      this.getCardCredentials();
    });

    this.showActionsButton = this.permissions.allowDeleting || this.permissions.allowEditing || (this.cardCredentialActions !== undefined && !this.inlineActions);
  }

  ngAfterViewInit(): void {
    if(this.actions) {
      const showActionsColumn = this.permissions.allowDeleting || this.permissions.allowEditing || this.cardCredentialActions;
      const columns = [{ name: 'csn', headingDisplay: 'CSN', template: this.serialNumberColumn, sortable: false }, ...this.useMinimalColumns ? minimalCardCredentialColumns : fullCardCredentialColumns];
      this.cardCredentialColumnsSubject.next(showActionsColumn ? [...columns, { name: 'actions', template: this.actions, sortable: false, stickyPosition: StickyPosition.End }] : columns);
    }
  }

  viewAllCardCredentials() {
    this.currentlySelectedCardHolderChanged.emit(undefined);
  }

  cardCredentialFiltersChanged(filterProperties: string[]) {
    this.cardCredentialPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchCardCredentials(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.cardCredentialPageFilterSort.resetQuery(query);
    this.getCardCredentials();
  }

  createOrEditCardCredential(cardCredential?: ICardCredentialDTO) {
    const cardHolderId = cardCredential?.cardHolderId ?? this.currentlySelectedCardHolder?.id;
    if(cardHolderId) {
      this.adminDialogService.editCardCredential({ cardCredential, cardHolderId }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.getCardCredentials());
    }
  }

  deleteCardCredential(cardCredential: ICardCredentialDTO) {
    this.adminDialogService.confirm(`Are you sure you want to delete ${cardCredential.hidNumber}?`, `Delete ${cardCredential.hidNumber}?`).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.CardCredentials.deleteSingle(cardCredential.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getCardCredentials());
  }

  enrolCardCredential(cardCredential: ICardCredentialDTO) {
    this.adminDialogService.openEnrolCardCredentialDialog().pipe(
      concatMap(({ csn, hid }) => {
        const serialNumber = decimalCsnToLockFormat(csn);
        return this.adminApiService.CardCredentials.updateSingle(cardCredential.id, { ...cardCredential, serialNumber, hidNumber: hid });
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(edited => edited && this.getCardCredentials());
  }

  reassignCardCredentialToOtherCardHolder(cardCredential: ICardCredentialDTO) {
    const dialogSettings: ISelectCardHoldersDialogData = { allowMultipleCardHolders: false };

    const { id: cardCredentialId, cardHolderId,  ...cardCredentialToUpdate } = cardCredential;

    if(cardHolderId) {
      dialogSettings.currentlySelectedCardHolders = this.adminApiService.CardHolders.getSingle(cardHolderId).pipe(map(cardHolder => cardHolder ? [cardHolder] : []), takeUntilDestroyed(this.destroyRef));
    }

    this.adminDialogService.openSelectCardHoldersDialog(dialogSettings).pipe(
      concatMap(assigned => assigned.length ? this.adminApiService.CardCredentials.updateSingle(cardCredentialId, { ...cardCredentialToUpdate, cardHolderId: assigned[0].id }) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(assigned => assigned && this.getCardCredentials());
  }

  onPagingEvent(page: IPagingRequest) {
    this.cardCredentialPageFilterSort.page = page;
    this.getCardCredentials();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.cardCredentialPageFilterSort.sort = sort;
    this.getCardCredentials();
  }

  private getCardCredentials() {
    const onResponse = {
      next: (x: IPagingResponse<ICardCredentialDTO>) => {
        this.cardCredentialsSubject.next(x);
        this.isLoadingCardCredentialsSubject.next(false);
      },
      error: () => this.isLoadingCardCredentialsSubject.next(false),
    };

    // NOTE: Hacky race condition fix, just disallow them!
    this.adminDialogService.showIsLoading(this.isLoadingCardCredentials$.pipe(takeUntilDestroyed(this.destroyRef)), 'Loading card credentials..');

    const params = this.cardTypeFilter === null ? this.cardCredentialPageFilterSort : { ...this.cardCredentialPageFilterSort, cardType: this.cardTypeFilter };

    if(this.currentlySelectedCardHolder) {
      this.isLoadingCardCredentialsSubject.next(true);
      this.adminApiService.CardHolders.CardCredentials.getMany(this.currentlySelectedCardHolder.id, params).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse)
    } else if(this.allowShowingAll) {
      this.isLoadingCardCredentialsSubject.next(true);
      this.adminApiService.CardCredentials.getMany(params).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
    }
  }
}
