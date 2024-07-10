import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, concatMap, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { DATA_TABLE_COLUMN_SETTINGS, StickyPosition, defaultTableColumnSettings } from 'src/app/shared/models/data-table';
import { ILocationDTO } from 'src/app/shared/models/location';
import { ILockerBankDTO } from 'src/app/shared/models/locker-bank';
import { IEditLockerBankDialogData } from '../../../dialogs/edit-entities/edit-locker-bank-dialog/edit-locker-bank-dialog.component';
import { IEntityPermissions } from '../../../models/entity-views';

@Component({
  selector: 'admin-locker-banks-view',
  templateUrl: './locker-banks-view.component.html',
  providers: [{ provide: DATA_TABLE_COLUMN_SETTINGS, useValue: { ...defaultTableColumnSettings, sortable: true } }]
})
export class LockerBanksViewComponent implements OnInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() useAdminRegisteredLockerBanksOnly = true;
  @Input() permissions: IEntityPermissions = {};
  @Input() hideLockersAction = false;
  @Input() allowShowingAll = true;
  @Input() currentlySelectedLocation$: Observable<ILocationDTO | undefined> = of();
  @Input() lockerBankActions?: TemplateRef<ILockerBankDTO>;
  @Input() inlineActions = false;

  @Output() currentlySelectedLocationChanged = new EventEmitter<ILocationDTO | undefined>();
  @Output() viewLockersClicked = new EventEmitter<ILockerBankDTO>();

  currentlySelectedLocation?: ILocationDTO;

  private readonly lockerBanksSubject = new BehaviorSubject<IPagingResponse<ILockerBankDTO> | undefined>(undefined);
  readonly lockerBanks$ = this.lockerBanksSubject.asObservable();

  private readonly isLoadingLockerBanksSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerBanks$ = this.isLoadingLockerBanksSubject.asObservable();

  readonly lockerBankFilters = ['name', 'description'];

  private lockerBankPageFilterSort = PageFilterSort.usingFilter(this.lockerBankFilters);

  readonly StickyPosition = StickyPosition;

  showActionsButton = false;

  ngOnInit(): void {
    this.currentlySelectedLocation$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(location => {
      this.currentlySelectedLocation = location;
      this.lockerBankPageFilterSort.resetQuery();
      this.getLockerBanks();
    });

    this.showActionsButton = this.permissions.allowDeleting || this.permissions.allowEditing || (this.lockerBankActions !== undefined && !this.inlineActions);
  }

  viewAllLockerBanks() {
    this.currentlySelectedLocationChanged.emit(undefined);
  }

  lockerBankFiltersChanged(filterProperties: string[]) {
    this.lockerBankPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLockerBanks(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockerBankPageFilterSort.resetQuery(query);
    this.getLockerBanks();
  }

  createOrEditLockerBank(entity?: ILockerBankDTO) {
    const locationId = this.currentlySelectedLocation?.id;
    const dialogSettings: IEditLockerBankDialogData | undefined = entity ? { entity, locationId: entity.locationId } : locationId ? { locationId } : undefined;

    if(dialogSettings) {
      this.adminDialogService.editLockerBank(dialogSettings).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.getLockerBanks());
    }
  }

  deleteLockerBank(lockerBank: ILockerBankDTO) {
    this.adminDialogService.confirm(`Are you sure you want to delete ${lockerBank.name}?`, `Delete ${lockerBank.name}?`)
    .pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.LockerBanks.deleteSingle(lockerBank.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getLockerBanks());
  }

  viewLockersForLockerBank(lockerBank: ILockerBankDTO) {
    this.viewLockersClicked.emit(lockerBank);
  }

  editReferenceImage(entity: ILockerBankDTO) {
    this.adminDialogService.editReferenceImage(
      { entity,
        endPoints: this.adminApiService.LockerBanks.ReferenceImages
      })
  }

  onPagingEvent(page: IPagingRequest) {
    this.lockerBankPageFilterSort.page = page;
    this.getLockerBanks();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.lockerBankPageFilterSort.sort = sort;
    this.getLockerBanks();
  }

  private getLockerBanks() {
    const onResponse = {
      next: (x: IPagingResponse<ILockerBankDTO>) => {
        this.lockerBanksSubject.next(x);
        this.isLoadingLockerBanksSubject.next(false);
      },
      error: () => this.isLoadingLockerBanksSubject.next(false),
    };

    // NOTE: Hacky fix to prevent async race conditions where a user clicks one entity, then another the second loads first
    // then suddenly the first replaces it!
    this.adminDialogService.showIsLoading(this.isLoadingLockerBanks$.pipe(takeUntilDestroyed(this.destroyRef)), 'Loading locker banks..');

    if(this.useAdminRegisteredLockerBanksOnly) {
      if(this.currentlySelectedLocation) {
        this.isLoadingLockerBanksSubject.next(true);
        this.adminApiService.LockerBankAdmins.Locations.LockerBanks(this.currentlySelectedLocation.id, this.lockerBankPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
      } else if(this.allowShowingAll) {
        this.isLoadingLockerBanksSubject.next(true);
        this.adminApiService.LockerBankAdmins.LockerBanks(this.lockerBankPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
      }
    } else {
      if(this.currentlySelectedLocation) {
        this.isLoadingLockerBanksSubject.next(true);
        this.adminApiService.Locations.LockerBanks.getMany(this.currentlySelectedLocation.id, this.lockerBankPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
      } else if(this.allowShowingAll) {
        this.isLoadingLockerBanksSubject.next(true);
        this.adminApiService.LockerBanks.getMany(this.lockerBankPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
      }
    }
  }
}
