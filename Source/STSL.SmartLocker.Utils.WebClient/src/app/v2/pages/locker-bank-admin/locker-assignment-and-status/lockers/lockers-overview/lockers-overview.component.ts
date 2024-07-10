import { ChangeDetectorRef, Component, DestroyRef, EventEmitter, Input, OnInit, Output, ViewChild, inject } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { BehaviorSubject, Subject, debounceTime, map } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { IPagingResponse, PageFilterSort } from 'src/app/shared/models/api';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { ILockerBankAdminLockerBankSummaryDTO } from 'src/app/v2/shared/models/location-locker-bank-summary';
import { faArrowRight, faChevronLeft } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'lockers-overview',
  templateUrl: './lockers-overview.component.html',
  styleUrls: ['lockers-overview.component.css']
})
export class LockersOverviewComponent implements OnInit {
  // Params
  @Input() lockerBank!: ILockerBankAdminLockerBankSummaryDTO;
  @Input() locationName!: string;

  // Click Events
  @Output() backClicked: EventEmitter<void> = new EventEmitter();
  @Output() cardClicked: EventEmitter<ILockerStatusDTO> = new EventEmitter<ILockerStatusDTO>();

  // Listener for the Virtual Scroll Component
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

  // Font Awesome
  faArrowRight = faArrowRight;
  faBackArrow = faChevronLeft;

  // Services
  private readonly adminApiService = inject(AdminApiService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  // Variables
  private scrollEventsSubject = new Subject<void>();
  private readonly lockerStatusesSubject = new BehaviorSubject<IPagingResponse<ILockerStatusDTO> | undefined>(undefined);
  
  // Map here so we get an Observable<LockerSatusDTO[]> instead of IPagingResponse, which is not accepted by the virtual scroll component
  readonly lockerStatuses$ = this.lockerStatusesSubject.asObservable().pipe(
    map(response => response?.results || [])
  );

  // Loader
  private readonly isLoadingLockerStatusesSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerStatuses$ = this.isLoadingLockerStatusesSubject.asObservable();

  // Search Filters
  readonly lockerFilters = ['label', 'assignedTo', 'serviceTag'];
  private lockerStatusPageFilterSort = PageFilterSort.usingFilter(this.lockerFilters);

  // Filter Button Value
  buttonFilter = '';  // Set the default value to "all"


  ngOnInit(): void {
    // Load lockers
    this.fetchLockers();

    // Initialize debounced scroll event handling
    this.scrollEventsSubject.pipe().subscribe(() => {
      this.onDebouncedScroll();
    });
  }

  /* API Request & Filtering */
  fetchLockers(reset: boolean = false): void {
    if (reset) {
        // Clear existing data when applying new filters or starting a new search
        this.lockerStatusesSubject.next(undefined);
    }
    this.isLoadingLockerStatusesSubject.next(true);

    // Adjusting the number of records per page to be more appopriate for infinite scrolling
    this.lockerStatusPageFilterSort.page.recordsPerPage = 10;

    this.adminApiService.LockerBanks.Lockers
        .Statuses(this.lockerBank.id, this.lockerStatusPageFilterSort)
        .subscribe({
          next: (response) => {
            const currentData = this.lockerStatusesSubject.value?.results || [];
            const newData = [...currentData, ...response.results];
            this.lockerStatusesSubject.next({ ...response, results: newData });
            this.isLoadingLockerStatusesSubject.next(false);
          },
          error: () => {
            this.isLoadingLockerStatusesSubject.next(false);
          }
        });
    }

  /* Search for lockers */
  searchLockers(searchParams: { query: string; filterBy?: string }): void {
    // Reset the pageIndex for a new search
    this.lockerStatusPageFilterSort.page.pageIndex = 0;
    
    // Apply the search query
    this.lockerStatusPageFilterSort.filter.filterProperties = this.lockerFilters;
    this.lockerStatusPageFilterSort.filter.filterValue = searchParams.query;
  
    // Fetch data with the updated filter and reset pageIndex
    this.fetchLockers(true);
  }

  /* Apply filters from the buttons "All", "Vacant", "Allocated", "Alerts"*/
  applyButtonFilter(filter?: string): void {
    // Reset pageIndex for a new filter
    this.lockerStatusPageFilterSort.page.pageIndex = 0;
  
    if (filter)
    {
      this.buttonFilter = filter; // this is to toggle the underline styling for the selected filter button
      this.lockerStatusPageFilterSort.filter.filterProperties = [this.buttonFilter];
      this.lockerStatusPageFilterSort.filter.filterValue = undefined; // set null as we're not querying a value specifically
    }
    else {
      this.lockerStatusPageFilterSort.resetQueryAndFilter();
    }
  
    this.fetchLockers(true);
  }

  /* Propogate to base component upon selection*/
  handleCardClick(lockerSummary: ILockerStatusDTO): void {
    this.cardClicked.emit(lockerSummary);
  } 
  

  /* Infinite Scrolling */
  onScroll(): void {
    this.scrollEventsSubject.next();
  }

  onDebouncedScroll(): void {
    const totalRecords = this.lockerStatusesSubject.value?.totalRecords || 0;
    const renderedRange = this.viewport.getRenderedRange();
    const end = renderedRange.end;
    const total = this.viewport.getDataLength();
  
    // Check if all records are already fetched
    if (total >= totalRecords) {
      return;
    }
  
    const loadThreshold = 0.8 * total; // Load more data at 80% scroll
    
    if (end > loadThreshold) {
      this.lockerStatusPageFilterSort.page.pageIndex++;
      this.fetchLockers();
    }
  }
}