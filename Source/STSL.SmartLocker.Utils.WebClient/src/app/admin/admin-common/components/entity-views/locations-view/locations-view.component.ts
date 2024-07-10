import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, TemplateRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, concatMap, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, SortedRequest } from 'src/app/shared/models/api';
import { DATA_TABLE_COLUMN_SETTINGS, StickyPosition, defaultTableColumnSettings } from 'src/app/shared/models/data-table';
import { ILocationDTO } from 'src/app/shared/models/location';
import { IEntityPermissions } from '../../../models/entity-views';

@Component({
  selector: 'admin-locations-view',
  templateUrl: './locations-view.component.html',
  providers: [{ provide: DATA_TABLE_COLUMN_SETTINGS, useValue: { ...defaultTableColumnSettings, sortable: true } }]
})
export class LocationsViewComponent implements OnInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly destroyRef = inject(DestroyRef);

  @Input() useAdminRegisteredLocationsOnly = true;
  @Input() permissions: IEntityPermissions = {};
  @Input() hideLockerBanksAction = false;
  @Input() locationActions?: TemplateRef<ILocationDTO>;

  @Output() viewLockerBanksClicked = new EventEmitter<ILocationDTO>();

  private readonly locationsSubject = new BehaviorSubject<IPagingResponse<ILocationDTO> | undefined>(undefined);
  readonly locations$ = this.locationsSubject.asObservable();

  private readonly isLoadingLocationsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLocations$ = this.isLoadingLocationsSubject.asObservable();

  readonly locationFilters = [ 'name', 'description' ];

  private locationPageFilterSort = PageFilterSort.usingFilter(this.locationFilters);

  readonly StickyPosition = StickyPosition;

  showActionsButton = false;

  ngOnInit(): void {
    this.getLocations();
    this.showActionsButton = this.permissions.allowDeleting || this.permissions.allowEditing || this.locationActions !== undefined;
  }

  locationFiltersChanged(filterProperties: string[]) {
    this.locationPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLocations(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.locationPageFilterSort.resetQuery(query);
    this.getLocations();
  }

  createOrEditLocation(entity?: ILocationDTO) {
    this.adminDialogService.editLocation({ entity }).pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe(createdOrEdited => createdOrEdited && this.getLocations());
  }

  deleteLocation(location: ILocationDTO) {
    this.adminDialogService.confirm(`Are you sure you want to delete ${location.name}?`, `Delete ${location.name}?`)
    .pipe(concatMap(confirmed => confirmed ? this.adminApiService.Locations.deleteSingle(location.id) : of(false)), takeUntilDestroyed(this.destroyRef))
    .subscribe(deleted => deleted && this.getLocations());
  }
  
  editReferenceImage(entity: ILocationDTO) {
    this.adminDialogService.editReferenceImage(
      { entity,
        endPoints: this.adminApiService.Locations.ReferenceImages
      });
  }

  viewLockerBanksForLocation(location: ILocationDTO) {
    this.viewLockerBanksClicked.emit(location);
  }

  onPagingEvent(page: IPagingRequest) {
    this.locationPageFilterSort.page = page;
    this.getLocations();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.locationPageFilterSort.sort = sort;
    this.getLocations();
  }

  private getLocations() {
    const onResponse = {
      next: (x: IPagingResponse<ILocationDTO>) => {
        this.locationsSubject.next(x);
        this.isLoadingLocationsSubject.next(false);
      },
      error: () => this.isLoadingLocationsSubject.next(false),
    };

    this.isLoadingLocationsSubject.next(true);

    // NOTE: Hacky race condition fix, just disallow them!
    this.adminDialogService.showIsLoading(this.isLoadingLocations$.pipe(takeUntilDestroyed(this.destroyRef)), 'Loading locations..');

    this.useAdminRegisteredLocationsOnly ?
    this.adminApiService.LockerBankAdmins.Locations.getMany(this.locationPageFilterSort).subscribe(onResponse)
    :
    this.adminApiService.Locations.getMany(this.locationPageFilterSort).subscribe(onResponse);
  }
}
