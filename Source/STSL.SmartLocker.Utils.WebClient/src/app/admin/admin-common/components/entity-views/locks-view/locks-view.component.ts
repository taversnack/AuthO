import { AfterViewInit, Component, DestroyRef, Input, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, concatMap, map, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { DATA_TABLE_COLUMN_SETTINGS, StickyPosition, TableColumn, defaultTableColumnSettings } from 'src/app/shared/models/data-table';
import { ILockDTO } from 'src/app/shared/models/lock';
import { ISelectLockersDialogData } from '../../../dialogs/view-entities/select-lockers-dialog/select-lockers-dialog.component';
import { IEntityPermissions } from '../../../models/entity-views';

const lockColumns: TableColumn<ILockDTO>[] = [
  'serialNumber',
  'firmwareVersion',
  'operatingMode',
];

@Component({
  selector: 'admin-locks-view',
  templateUrl: './locks-view.component.html',
  providers: [{ provide: DATA_TABLE_COLUMN_SETTINGS, useValue: { ...defaultTableColumnSettings, sortable: true } }]
})
export class LocksViewComponent implements OnInit, AfterViewInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('actions') actions?: TemplateRef<ILockDTO>;
  @ViewChild('installationDateColumn') installationDateColumn?: TemplateRef<ILockDTO>;

  @Input() permissions: IEntityPermissions = {};
  @Input() lockActions?: TemplateRef<ILockDTO>;
  @Input() disableAssignment = false;
  @Input() disableReassignment = false;
  @Input() disableUnassignment = false;
  @Input() showLockerIdColumn = false;
  @Input() inlineActions = false;

  private readonly locksSubject = new BehaviorSubject<IPagingResponse<ILockDTO> | undefined>(undefined);
  readonly locks$ = this.locksSubject.asObservable();

  private readonly isLoadingLocksSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLocks$ = this.isLoadingLocksSubject.asObservable();

  readonly lockFilters = ['serialNumber', 'installationDateUtc', 'firmwareVersion', 'operatingMode'];

  private lockPageFilterSort = PageFilterSort.usingFilter(this.lockFilters);

  readonly StickyPosition = StickyPosition;

  showActionsButton = false;

  private readonly lockColumnsSubject = new BehaviorSubject<TableColumn<ILockDTO>[]>([]);
  readonly lockColumns$ = this.lockColumnsSubject.asObservable();

  ngOnInit(): void {
    this.showActionsButton = this.permissions.allowDeleting || this.permissions.allowEditing || (this.lockActions !== undefined && !this.inlineActions);

    this.getLocks();
  }

  ngAfterViewInit(): void {
    if(this.actions) {
      this.lockColumnsSubject.next([...lockColumns, ...this.specialRenderingColumns()]);
    }
  }

  lockFiltersChanged(filterProperties: string[]) {
    this.lockPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLocks(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockPageFilterSort.resetQuery(query);
    this.getLocks();
  }

  createOrEditLock(lock?: ILockDTO) {
    this.adminDialogService.editLock({ lock }).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.getLocks())
  }

  deleteLock(lock: ILockDTO) {
    const message = `Are you sure you want to delete ${lock.id}?`;
    const title = `Delete ${lock.serialNumber}?`;
    this.adminDialogService.confirm()

    this.adminDialogService.confirm(message, title).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.Locks.deleteSingle(lock.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getLocks());
  }

  // Should the (re)assigning & removing be on the locks overview page?
  assignOrReassignLockToLocker(lock: ILockDTO) {
    // If lock already has a lockerId, we are reassigning, thus we get the currently selected locker
    const dialogSettings: ISelectLockersDialogData = lock.lockerId ? {
      currentlySelectedLockers$: this.adminApiService.Lockers.getSingle(lock.lockerId).pipe(map(locker => locker ? [locker] : [])),
      allowMultipleLockers: false,
    } : {};

    const title = (lock.lockerId ? 'Reassign Lock ' : 'Assign Lock ') + lock.serialNumber;

    this.adminDialogService.openSelectLockersDialog(dialogSettings, title).pipe(
      concatMap(lockers => {
        const lockerId = lockers?.shift()?.id;
        return lockerId ? this.adminApiService.Locks.partialUpdateSingle(lock.id, { lockerId }) : of(false)
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(updated => updated && this.getLocks());
  }

  removeLockFromLocker(lock: ILockDTO) {
    const message = `Are you sure you want to remove lock ${lock.id} (${lock.serialNumber}) from locker ${lock.lockerId}?`;
    const title = `Remove ${lock.serialNumber}?`;

    this.adminDialogService.confirm(message, title).pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.Locks.partialUpdateSingle(lock.id, { lockerId: null }) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(removed => removed && this.getLocks());
  }

  onPagingEvent(page: IPagingRequest) {
    this.lockPageFilterSort.page = page;
    this.getLocks();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.lockPageFilterSort.sort = sort;
    this.getLocks();
  }

  private specialRenderingColumns(): TableColumn<ILockDTO>[] {
    const columns = [
      { name: 'installationDateUtc', headingDisplay: 'Installation Date', template: this.installationDateColumn },
      { name: 'actions', template: this.actions, sortable: false, stickyPosition: StickyPosition.End },
    ];
    return this.showLockerIdColumn ? [{ name: 'lockerId', sortable: false }, ...columns] : columns;
  }

  private getLocks() {
    const onResponse = {
      next: (x: IPagingResponse<ILockDTO>) => {
        this.locksSubject.next(x);
        this.isLoadingLocksSubject.next(false);
      },
      error: () => this.isLoadingLocksSubject.next(false),
    };

    this.isLoadingLocksSubject.next(true);
    this.adminApiService.Locks.getMany(this.lockPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
  }
}
