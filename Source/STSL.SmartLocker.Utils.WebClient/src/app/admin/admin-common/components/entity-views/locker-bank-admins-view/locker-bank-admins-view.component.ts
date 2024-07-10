import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, concatMap, map, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { FilteredRequest, IPagingRequest, IPagingResponse, PagingRequest } from 'src/app/shared/models/api';
import { ICardHolderDTO, defaultCardHolderAliasPlural, defaultCardHolderAliasSingular } from 'src/app/shared/models/card-holder';
import { StickyPosition } from 'src/app/shared/models/data-table';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';
import { ISelectCardHoldersDialogData } from '../../../dialogs/view-entities/select-card-holders-dialog/select-card-holders-dialog.component';
import { IEntityPermissions } from '../../../models/entity-views';


// TODO: [8] Rewrite the whole admins system, including API endpoints, services, DTOs etc for v2.
@Component({
  selector: 'admin-locker-bank-admins-view',
  templateUrl: './locker-bank-admins-view.component.html',
})
export class LockerBankAdminsViewComponent implements OnInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() permissions: IEntityPermissions = {};
  // @Input() allowShowingAll = true;
  @Input() currentlySelectedLockerBank$: Observable<ILockerBankDTO | undefined> = of();
  @Input() lockerBankAdminActions?: TemplateRef<ICardHolderDTO>;
  @Input() inlineActions = false;

  readonly allowShowingAll = false;

  @Output() currentlySelectedLockerBankChanged = new EventEmitter<ILockerBankDTO | undefined>();

  private readonly lockerBankAdminsSubject = new BehaviorSubject<IPagingResponse<ICardHolderDTO> | undefined>(undefined);
  readonly lockerBankAdmins$ = this.lockerBankAdminsSubject.asObservable();

  private readonly isLoadingLockerBankAdminsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerBankAdmins$ = this.isLoadingLockerBankAdminsSubject.asObservable();

  cardHolderAliasSingular = this.authService.cardHolderAliasSingular;
  cardHolderAliasPlural = this.authService.cardHolderAliasPlural;
  cardHolderUniqueIdentifierAlias = this.authService.cardHolderUniqueIdentifierAlias;

  readonly lockerBankAdminFilters = ['firstName', 'lastName', 'email', { name: 'uniqueIdentifier', display: this.cardHolderUniqueIdentifierAlias }];


  lockerBankAdminPageFilterSort = { filter: new FilteredRequest(), page: new PagingRequest(), };

  currentlySelectedLockerBank?: ILockerBankDTO;

  readonly StickyPosition = StickyPosition;

  showActionsButton = false;

  ngOnInit(): void {
    this.currentlySelectedLockerBank$.subscribe(lockerBank => {
      this.currentlySelectedLockerBank = lockerBank;
      this.getLockerBankAdmins();
    });

    this.authService.currentTenantDetails$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => {
      this.cardHolderAliasSingular = x?.cardHolderAliasSingular ?? defaultCardHolderAliasSingular;
      this.cardHolderAliasPlural = x?.cardHolderAliasPlural ?? defaultCardHolderAliasPlural;
    });

    this.showActionsButton = this.permissions.allowDeleting === true && (this.lockerBankAdminActions !== undefined && !this.inlineActions);
  }

  viewAllLockerBankAdmins() {
    this.currentlySelectedLockerBankChanged.emit(this.currentlySelectedLockerBank);
  }

  lockerBankAdminFiltersChanged(filterProperties: string[]) {
    this.lockerBankAdminPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLockerBankAdmins(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockerBankAdminPageFilterSort.filter.filterValue = query;
    this.getLockerBankAdmins();
  }

  openReplaceLockerBankAdminsDialog() {
    // open select card holders view to find new card holder to add
    const lockerBank = this.currentlySelectedLockerBank;
    if(!lockerBank) {
      return;
    }

    const dialogSettings: ISelectCardHoldersDialogData = {
      currentlySelectedCardHolders: this.adminApiService.LockerBanks.Admins.getMany(lockerBank.id).pipe(map(({ results }) => results)),
      requireCardHolderEmail: true
    };

    this.adminDialogService.openSelectCardHoldersDialog(dialogSettings).pipe(
      concatMap(assigned => this.adminApiService.LockerBanks.Admins.updateMany(lockerBank.id, assigned.map(({ id }) => id))),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(assigned => assigned && this.getLockerBankAdmins());
  }

  removeLockerBankAdmin(lockerBankAdmin: ICardHolderDTO) {
    if(!this.currentlySelectedLockerBank) {
      // NOTE: Throw error, this shouldn't happen
      return;
    }

    const lockerBankId = this.currentlySelectedLockerBank.id;
    const confirmationMessage = `Are you sure you want to remove ${lockerBankAdmin.firstName} ${lockerBankAdmin.lastName} from Locker Bank: ${lockerBankId}?`;
    const dialogTitle = `Remove ${lockerBankAdmin.uniqueIdentifier ?? lockerBankAdmin.firstName}?`;

    this.adminDialogService.confirm(confirmationMessage, dialogTitle).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.LockerBanks.Admins.deleteSingle(lockerBankId, lockerBankAdmin.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getLockerBankAdmins());
  }

  onPagingEvent(page: IPagingRequest) {
    const pageFilterSort = { ...this.lockerBankAdminPageFilterSort, page };
    this.lockerBankAdminPageFilterSort = pageFilterSort;
    this.getLockerBankAdmins();
  }

  private getLockerBankAdmins() {
    const onResponse = {
      next: (x: IPagingResponse<ICardHolderDTO>) => {
        this.lockerBankAdminsSubject.next(x);
        this.isLoadingLockerBankAdminsSubject.next(false);
      },
      error: () => this.isLoadingLockerBankAdminsSubject.next(false),
    };

    if(this.currentlySelectedLockerBank) {
      this.isLoadingLockerBankAdminsSubject.next(true);
      this.adminApiService.LockerBanks.Admins.getMany(this.currentlySelectedLockerBank.id, this.lockerBankAdminPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
    }
  }
}
