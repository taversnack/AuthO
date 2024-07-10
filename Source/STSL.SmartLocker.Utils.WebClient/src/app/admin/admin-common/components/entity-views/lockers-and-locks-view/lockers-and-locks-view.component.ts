import { AfterViewInit, ChangeDetectorRef, Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, concatMap, filter, map, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { checkEntityIdEquality } from 'src/app/shared/lib/utilities';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { StickyPosition, TableColumn } from 'src/app/shared/models/data-table';
import { IGlobalLockerSearchResultDTO, ILockerWithLocationLockerBankLock } from 'src/app/shared/models/global-locker-search-result';
import { ILockerDTO, lockSecurityTypeToString } from 'src/app/shared/models/locker';
import { ILockerBankDTO, LockerBankBehaviour } from 'src/app/shared/models/locker-bank';
import { IEditLockerDialogData } from '../../../dialogs/edit-entities/edit-locker-dialog/edit-locker-dialog.component';
import { IEntityPermissions } from '../../../models/entity-views';

@Component({
  selector: 'admin-lockers-and-locks-view',
  templateUrl: './lockers-and-locks-view.component.html',
})
export class LockersAndLocksViewComponent implements OnInit, AfterViewInit {

  private readonly adminDialogService = inject(AdminDialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);

  @ViewChild('lockerAndLockActionsTemplate') lockerAndLockActionsTemplate?: TemplateRef<ILockerWithLocationLockerBankLock>;

  @ViewChild('installationDateColumn') installationDateColumn?: TemplateRef<ILockerWithLocationLockerBankLock>;
  @ViewChild('otherInfoColumn') otherInfoColumn?: TemplateRef<ILockerWithLocationLockerBankLock>;

  @Input() lockerPermissions: IEntityPermissions = {};
  @Input() currentlySelectedLockerBank$: Observable<ILockerBankDTO | undefined> = of();
  @Input() lockerAndLockActions?: TemplateRef<ILockerWithLocationLockerBankLock>;
  @Input() lockerBankActions?: TemplateRef<ILockerBankDTO | undefined>;
  @Input() inlineLockerAndLockActions = false;
  @Input() useMinimalColumns: boolean = true;
  @Input() allowShowingAll: boolean = false;

  @Output() currentlySelectedLockerBankChanged = new EventEmitter<ILockerBankDTO | undefined>();

  private readonly lockerAndLocksSubject = new BehaviorSubject<IPagingResponse<ILockerWithLocationLockerBankLock> | undefined>(undefined);
  readonly lockerAndLocks$ = this.lockerAndLocksSubject.asObservable();

  private readonly isLoadingLockerAndLocksSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerAndLocks$ = this.isLoadingLockerAndLocksSubject.asObservable();

  readonly lockerAndLockFilters = ['label', 'serviceTag', 'serialNumber'];

  private lockerAndLockPageFilterSort = PageFilterSort.usingFilter(this.lockerAndLockFilters);

  currentlySelectedLockerBank?: ILockerBankDTO;

  readonly StickyPosition = StickyPosition;

  private readonly lockerAndLockColumnsSubject = new BehaviorSubject<TableColumn<ILockerWithLocationLockerBankLock>[]>([]);
  readonly lockerAndLockColumns$ = this.lockerAndLockColumnsSubject.asObservable();

  showLockerBankActionsButton = false;
  showLockerAndLockActionsButton = false;

  private showActionsColumn = false;

  ngOnInit(): void {
    this.currentlySelectedLockerBank$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockerBank => {
      this.currentlySelectedLockerBank = lockerBank;
      this.lockerAndLockPageFilterSort.resetQuery();
      this.getLockerAndLocks();
    });

    const hasAnyLockerAndLockPermissions = this.lockerPermissions.allowDeleting || this.lockerPermissions.allowEditing;

    this.showActionsColumn = hasAnyLockerAndLockPermissions || this.lockerAndLockActions !== undefined;

    this.showLockerAndLockActionsButton = hasAnyLockerAndLockPermissions || (this.lockerAndLockActions !== undefined && !this.inlineLockerAndLockActions);
    this.showLockerBankActionsButton = this.lockerPermissions.allowCreating || this.lockerBankActions !== undefined;
  }

  ngAfterViewInit(): void {
    this.setTableColumns();
    // this.changeDetectorRef.detectChanges();
  }

  private setTableColumns() {
    if(this.lockerAndLockActionsTemplate) {
      const baseColumns: TableColumn<ILockerWithLocationLockerBankLock>[] = [
        { name: 'label', getValue: x => x.locker.label, sortable: true },
        { name: 'serviceTag', getValue: x => x.locker.serviceTag, sortable: true },
        { name: 'lockSerialNumber', getValue: x => x.lock?.serialNumber, sortable: true },
        { name: 'lockSecurityType', getValue: x => lockSecurityTypeToString(x.locker.securityType), sortable: true },
      ];

      const extendedColumns: TableColumn<ILockerWithLocationLockerBankLock>[] = [
        { name: 'installationDateUtc', headingDisplay: 'Lock Installation Date', template: this.installationDateColumn, sortable: true },
        { name: 'otherInfo', template: this.otherInfoColumn, sortable: false },
      ];

      const locationColumns: TableColumn<ILockerWithLocationLockerBankLock>[] = [
        { name: 'location', getValue: x => x.location?.name, sortable: false },
      ];

      const lockerBankColumns: TableColumn<ILockerWithLocationLockerBankLock>[] = [
        { name: 'lockerBank', getValue: x => x.lockerBank?.name, sortable: false },
      ];

      const columns: TableColumn<ILockerWithLocationLockerBankLock>[] = [
        ...(this.allowShowingAll && !this.currentlySelectedLockerBank) ? [...locationColumns, ...lockerBankColumns] : [],
        ...baseColumns,
        ...this.useMinimalColumns ? [] : extendedColumns,
        ...this.showActionsColumn ? [{ name: 'actions', template: this.lockerAndLockActionsTemplate, sortable: false, stickyPosition: StickyPosition.End }] : [],
      ];
      this.lockerAndLockColumnsSubject.next(columns);
      this.changeDetectorRef.detectChanges();
    }
  }

  createOrEditLocker(lockerAndLock?: ILockerWithLocationLockerBankLock) {
    if(!this.currentlySelectedLockerBank) {
      return;
    }

    const entity = lockerAndLock?.locker;
    const { id: lockerBankId, behaviour } = this.currentlySelectedLockerBank;
    const allowSmartLocksOnly = behaviour === LockerBankBehaviour.Temporary || lockerAndLock?.lock !== undefined;

    const dialogSettings: IEditLockerDialogData | undefined = entity ? { entity, lockerBankId, allowSmartLocksOnly } : lockerBankId ? { lockerBankId, allowSmartLocksOnly } : undefined;

    if(dialogSettings) {
      this.adminDialogService.editLocker(dialogSettings).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(edited => edited && this.getLockerAndLocks());
    }
  }

  deleteLocker(locker: ILockerDTO) {
    this.adminDialogService.confirm(`Are you sure you want to delete ${locker.label}?`, `Delete ${locker.serviceTag || locker.label}?`)
    .pipe(
      concatMap(confirmed => confirmed ? this.adminApiService.Lockers.deleteSingle(locker.id) : of(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getLockerAndLocks());
  }

  viewAllLockers() {
    this.currentlySelectedLockerBankChanged.emit(undefined);
  }

  lockerAndLockFiltersChanged(filterProperties: string[]) {
    this.lockerAndLockPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLockerAndLocks(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockerAndLockPageFilterSort.resetQuery(query);
    this.getLockerAndLocks();
  }

  onPagingEvent(page: IPagingRequest) {
    this.lockerAndLockPageFilterSort.page = page;
    this.getLockerAndLocks();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.lockerAndLockPageFilterSort.sort = sort;
    this.getLockerAndLocks();
  }

  private getLockerAndLocks() {
    const onResponse = {
      next: (x: IPagingResponse<ILockerWithLocationLockerBankLock>) => {
        this.lockerAndLocksSubject.next(x);
        this.isLoadingLockerAndLocksSubject.next(false);
        this.setTableColumns();
      },
      error: () => this.isLoadingLockerAndLocksSubject.next(false),
    };

    // NOTE: Hacky race condition fix, just disallow them!
    this.adminDialogService.showIsLoading(this.isLoadingLockerAndLocks$.pipe(takeUntilDestroyed(this.destroyRef)), 'Loading lockers..');

    if(this.currentlySelectedLockerBank) {
      this.isLoadingLockerAndLocksSubject.next(true);
      this.adminApiService.LockerBanks.LockersWithLocks.getMany(this.currentlySelectedLockerBank.id, this.lockerAndLockPageFilterSort)
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
    } else if(this.allowShowingAll) {
      this.isLoadingLockerAndLocksSubject.next(true);
      this.adminApiService.Lockers.WithLocationsAndLockerBanksAndLocks(this.lockerAndLockPageFilterSort)
      .pipe(
        filter((x): x is IGlobalLockerSearchResultDTO => x !== null),
        map(x =>
          ({
            ...x.lockerAndLocks,
            results: x.lockerAndLocks.results.map(lockerAndLock => {
              const lockerBank = x.lockerBanks.find(({ id }) => checkEntityIdEquality(id, lockerAndLock.locker.lockerBankId));
              const location = lockerBank ? x.locations.find(({ id }) => checkEntityIdEquality(id, lockerBank.locationId)) : undefined;
              const { locker, lock } = lockerAndLock;

              return { location, lockerBank, locker, lock };
            })
          })
        ),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe(onResponse);
    }
  }
}
