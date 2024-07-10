import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, concatMap, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { DATA_TABLE_COLUMN_SETTINGS, StickyPosition, defaultTableColumnSettings } from 'src/app/shared/models/data-table';
import { ILockerDTO } from 'src/app/shared/models/locker';
import { ILockerBankDTO, LockerBankBehaviour } from 'src/app/shared/models/locker-bank';
import { IEditLockerDialogData } from '../../../dialogs/edit-entities/edit-locker-dialog/edit-locker-dialog.component';
import { IEntityPermissions } from '../../../models/entity-views';

@Component({
  selector: 'admin-lockers-view',
  templateUrl: './lockers-view.component.html',
  providers: [{ provide: DATA_TABLE_COLUMN_SETTINGS, useValue: { ...defaultTableColumnSettings, sortable: true } }]
})
export class LockersViewComponent implements OnInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() permissions: IEntityPermissions = {};
  @Input() hideTenantsAction = false;
  @Input() allowShowingAll = true;
  @Input() currentlySelectedLockerBank$: Observable<ILockerBankDTO | undefined> = of();
  @Input() lockerActions?: TemplateRef<ILockerDTO>;
  @Input() inlineActions = false;

  @Output() viewTenantsForLockerClicked = new EventEmitter<ILockerDTO>();
  @Output() currentlySelectedLockerBankChanged = new EventEmitter<ILockerBankDTO | undefined>();

  private readonly lockersSubject = new BehaviorSubject<IPagingResponse<ILockerDTO> | undefined>(undefined);
  readonly lockers$ = this.lockersSubject.asObservable();

  private readonly isLoadingLockersSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockers$ = this.isLoadingLockersSubject.asObservable();

  readonly lockerFilters = ['label', 'serviceTag'];

  private lockerPageFilterSort = PageFilterSort.usingFilter(this.lockerFilters);

  currentlySelectedLockerBank?: ILockerBankDTO;

  readonly StickyPosition = StickyPosition;

  showActionsButton = false;

  ngOnInit(): void {
    this.currentlySelectedLockerBank$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockerBank => {
      this.currentlySelectedLockerBank = lockerBank;
      this.lockerPageFilterSort.resetQuery();
      this.getLockers();
    });

    this.showActionsButton = this.permissions.allowDeleting || this.permissions.allowEditing || (this.lockerActions !== undefined && !this.inlineActions);
  }

  viewTenantsForLocker(locker: ILockerDTO) {
    this.viewTenantsForLockerClicked.emit(locker);
  }

  viewAllLockers() {
    this.currentlySelectedLockerBankChanged.emit(undefined);
  }

  lockerFiltersChanged(filterProperties: string[]) {
    this.lockerPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLockers(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockerPageFilterSort.resetQuery(query);
    this.getLockers();
  }

  createOrEditLocker(entity?: ILockerDTO) {
    if(!this.currentlySelectedLockerBank) {
      return;
    }

    const { id: lockerBankId, behaviour } = this.currentlySelectedLockerBank;
    const allowSmartLocksOnly = behaviour === LockerBankBehaviour.Temporary;

    const dialogSettings: IEditLockerDialogData | undefined = entity ? { entity, lockerBankId, allowSmartLocksOnly } : lockerBankId ? { lockerBankId, allowSmartLocksOnly } : undefined;

    if(dialogSettings) {
      this.adminDialogService.editLocker(dialogSettings).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.getLockers());
    }
  }

  deleteLocker(locker: ILockerDTO) {
    this.adminDialogService.confirm(`Are you sure you want to delete ${locker.label}?`, `Delete ${locker.serviceTag || locker.label}?`)
    .pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.Lockers.deleteSingle(locker.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getLockers());
  }

  onPagingEvent(page: IPagingRequest) {
    this.lockerPageFilterSort.page = page;
    this.getLockers();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.lockerPageFilterSort.sort = sort;
    this.getLockers();
  }

  private getLockers() {
    const onResponse = {
      next: (x: IPagingResponse<ILockerDTO>) => {
        this.lockersSubject.next(x);
        this.isLoadingLockersSubject.next(false);
      },
      error: () => this.isLoadingLockersSubject.next(false),
    };

    // NOTE: Hacky race condition fix, just disallow them!
    this.adminDialogService.showIsLoading(this.isLoadingLockers$.pipe(takeUntilDestroyed(this.destroyRef)), 'Loading lockers..');

    if(this.currentlySelectedLockerBank) {
      this.isLoadingLockersSubject.next(true);
      this.adminApiService.LockerBanks.Lockers.getMany(this.currentlySelectedLockerBank.id, this.lockerPageFilterSort).subscribe(onResponse);
    } else if(this.allowShowingAll) {
      this.isLoadingLockersSubject.next(true);
      this.adminApiService.Lockers.getMany(this.lockerPageFilterSort).subscribe(onResponse);
    }
  }
}
