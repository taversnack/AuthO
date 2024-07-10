import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, concatMap, filter, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { ICardHolderDTO } from 'src/app/shared/models/card-holder';
import { DATA_TABLE_COLUMN_SETTINGS, StickyPosition, defaultTableColumnSettings } from 'src/app/shared/models/data-table';
import { IEntityPermissions } from '../../../models/entity-views';

@Component({
  selector: 'admin-card-holders-view',
  templateUrl: './card-holders-view.component.html',
  providers: [{ provide: DATA_TABLE_COLUMN_SETTINGS, useValue: { ...defaultTableColumnSettings, sortable: true } }]
})
export class CardHoldersViewComponent implements OnInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() permissions: IEntityPermissions = {};
  @Input() hideCardCredentialsAction = false;
  @Input() cardHolderActions?: TemplateRef<ICardHolderDTO>;
  @Input() inlineActions = false;

  @Output() viewCardCredentialsClicked = new EventEmitter<ICardHolderDTO>();

  private readonly cardHoldersSubject = new BehaviorSubject<IPagingResponse<ICardHolderDTO> | undefined>(undefined);
  readonly cardHolders$ = this.cardHoldersSubject.asObservable();

  private readonly isLoadingCardHoldersSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingCardHolders$ = this.isLoadingCardHoldersSubject.asObservable();

  cardHolderAliasSingular = this.authService.cardHolderAliasSingular;
  cardHolderAliasPlural = this.authService.cardHolderAliasPlural;
  cardHolderUniqueIdentifierAlias = this.authService.cardHolderUniqueIdentifierAlias;
  
  readonly cardHolderFilters = ['firstName', 'lastName', 'email', { name: 'uniqueIdentifier', display: this.cardHolderUniqueIdentifierAlias }];

  private cardHolderPageFilterSort = PageFilterSort.usingFilter(this.cardHolderFilters);

  readonly StickyPosition = StickyPosition;

  showActionsButton = false;

  ngOnInit(): void {
    this.getCardHolders();

    this.authService.cardHolderAliasSingular$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => this.cardHolderAliasSingular = x);

    this.showActionsButton = this.permissions.allowDeleting || this.permissions.allowEditing || (this.cardHolderActions !== undefined && !this.inlineActions);
  }

  cardHolderFiltersChanged(filterProperties: string[]) {
    this.cardHolderPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchCardHolders(searchParams: { query: string; filterBy?: string }) {
    const { query } = searchParams;
    this.cardHolderPageFilterSort.resetQuery(query);
    this.getCardHolders();
  }

  createOrEditCardHolder(entity?: ICardHolderDTO) {
    this.adminDialogService.editCardHolder({ entity }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.getCardHolders());
  }

  deleteCardHolder(cardHolder: ICardHolderDTO) {
    const message = `Are you sure you want to delete ${cardHolder.firstName} ${cardHolder.lastName}?`;
    const title = `Delete ${cardHolder.uniqueIdentifier ?? cardHolder.firstName}?`;

    this.adminDialogService.confirm(message, title).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.CardHolders.deleteSingle(cardHolder.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getCardHolders());
  }

  viewCardCredentialsForCardHolder(cardHolder: ICardHolderDTO) {
    this.viewCardCredentialsClicked.emit(cardHolder);
  }

  onPagingEvent(page: IPagingRequest) {
    this.cardHolderPageFilterSort.page = page;
    this.getCardHolders();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.cardHolderPageFilterSort.sort = sort;
    this.getCardHolders();
  }

  private getCardHolders() {
    const onResponse = {
      next: (x: IPagingResponse<ICardHolderDTO>) => {
        this.cardHoldersSubject.next(x);
        this.isLoadingCardHoldersSubject.next(false);
      },
      error: () => this.isLoadingCardHoldersSubject.next(false),
    };

    this.isLoadingCardHoldersSubject.next(true);

    this.adminApiService.CardHolders.getMany(this.cardHolderPageFilterSort).subscribe(onResponse);
  }
}
