import { ChangeDetectorRef, Component, DestroyRef, EventEmitter, Input, OnInit, Output, ViewChild, inject } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { BehaviorSubject, Observable, Subject, debounceTime, map, catchError, throwError } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { IPagingResponse, PageFilterSort } from 'src/app/shared/models/api';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { faArrowRight, faChevronLeft, faCircle, faCirclePlus } from '@fortawesome/free-solid-svg-icons';
import { ICardHolderAndCardCredentialsDTO } from 'src/app/shared/models/card-credential-and-card-holder';
import { AuthService } from 'src/app/auth/services/auth.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LockerSecurityType } from 'src/app/shared/models/locker';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';

@Component({
  selector: 'assign-card-holders',
  templateUrl: './assign-card-holders.component.html',
  styleUrls: ['assign-card-holders.component.css']
})
export class AssignCardHoldersComponent implements OnInit {
  // Params
  @Input() locationName!: string;
  @Input() lockerBankName!: string;
  @Input() lockerLabel!: string;

  @Input() lockerStatus!: ILockerStatusDTO;

  // Close Event
  @Output() backClicked: EventEmitter<void> = new EventEmitter();
  @Output() cardHolderSelected: EventEmitter<ICardHolderAndCardCredentialsDTO> = new EventEmitter<ICardHolderAndCardCredentialsDTO>();

  // Listener for the Virtual Scroll Component
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

  // Font Awesome
  faArrowRight = faArrowRight;
  faBackArrow = faChevronLeft;
  faCirclePlus = faCirclePlus;

  // Services
  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  // Variables

  private readonly itemSize = 75 // fixed height of each search result
  private totalRecords = 0; // total records in the query

  private scrollEventsSubject = new Subject<void>();
  private readonly cardHoldersSubject = new BehaviorSubject<IPagingResponse<ICardHolderAndCardCredentialsDTO> | undefined>(undefined);

  private readonly selectedCardHolderSubject = new BehaviorSubject<ICardHolderAndCardCredentialsDTO | undefined>(undefined);
  private selectedCardHolder$: Observable<ICardHolderAndCardCredentialsDTO | undefined> = this.selectedCardHolderSubject.asObservable();

    // Alias'
  readonly cardHolderPluralAlias = this.authService.cardHolderAliasPlural;
  readonly cardHolderSingleAlias = this.authService.cardHolderAliasSingular;

  // Map here so we get an Observable<LockerSatusDTO[]> instead of IPagingResponse, which is not accepted by the virtual scroll component
  readonly cardHolders$ = this.cardHoldersSubject.asObservable().pipe(
    map(response => response?.results || [])
  );

  // Loader
  private readonly isLoadingCardHoldersSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingCardHolders$ = this.isLoadingCardHoldersSubject.asObservable();

  // Search Filters
  readonly cardHolderFilters = ['firstName', 'lastName', 'email', 'uniqueIdentifier'];
  private cardHoldersPageFilterSort = PageFilterSort.usingFilter(this.cardHolderFilters);


  ngOnInit(): void {
    // Initialize debounced scroll event handling
    this.scrollEventsSubject.pipe(
      debounceTime(500) // Debounce time in milliseconds
    ).subscribe(() => {
      this.onDebouncedScroll();
    });
  }

  /* API Request & Filtering */
  fetchCardHolders(reset: boolean = false): void {
    if (reset) {
        // Clear existing data when applying new filters or starting a new search
        this.cardHoldersSubject.next(undefined);
    }

      this.isLoadingCardHoldersSubject.next(true);
      this.cardHoldersPageFilterSort.page.recordsPerPage = 20;
      this.adminApiService.CardHolders.WithUserCardCredentials(this.cardHoldersPageFilterSort)
          .subscribe({
              next: (response) => {
                  const currentData = this.cardHoldersSubject.value?.results || [];
                  const newData = [...currentData, ...response.results];
                  this.cardHoldersSubject.next({ ...response, results: newData });

                  this.totalRecords = response.totalRecords;

                  this.updateVirtualScrollSize();

                  this.isLoadingCardHoldersSubject.next(false); 
              },
              error: () => {
                  this.isLoadingCardHoldersSubject.next(false);
              }
          });
  }

  updateVirtualScrollSize() {
    if (this.viewport) {
      this.viewport.setTotalContentSize(this.totalRecords * this.itemSize);
    }
  }

  /* User Assignment */
  handleSelectedCardHolder(cardHolder: ICardHolderAndCardCredentialsDTO): void {
    // should never be null as it's passed through Input.
    if (this.lockerStatus)
    {
      // Work with credentials if it's a smart lock
      if (this.lockerStatus.securityType === LockerSecurityType.SmartLock)
      {
        this.handleSmartLockerAssignment(cardHolder);
      }
      // Assign without checks if not a smart locker
      else {
        this.selectedCardHolderSubject.next(cardHolder);
        this.cardHolderSelected.emit(this.selectedCardHolderSubject.value);
      }
    }

  }

  // Handle smart locker assignment
  handleSmartLockerAssignment(cardHolder: ICardHolderAndCardCredentialsDTO) {
    const cardCredentials = cardHolder.cardCredentials;

    if (!cardCredentials || cardCredentials.length === 0)
    {
      this.getCardHolderCredentialsFromCCure(cardHolder).subscribe(updatedCardHolder => {
        this.selectedCardHolderSubject.next(updatedCardHolder)
        this.cardHolderSelected.emit(this.selectedCardHolderSubject.value);
      });
    }
    else {
      this.selectedCardHolderSubject.next(cardHolder);
      this.cardHolderSelected.emit(this.selectedCardHolderSubject.value);
    }
  }

 /* Infinite Scrolling */
 onScroll(): void {
  this.scrollEventsSubject.next();
}

onDebouncedScroll(): void {
  const totalRecords = this.cardHoldersSubject.value?.totalRecords || 0;
  const renderedRange = this.viewport.getRenderedRange();
  const end = renderedRange.end;
  const total = this.viewport.getDataLength();

  // Check if all records are already fetched
  if (total >= totalRecords) {
    return;
  }

  const loadThreshold = 0.6 * total; // Load more data at 60% scroll

  if (end > loadThreshold) {
    this.cardHoldersPageFilterSort.page.pageIndex++;
    this.fetchCardHolders();
  }
}

  searchCardHolders(searchParams: { query: string; filterBy?: string }): void {
    const query  = searchParams.query;

    if(query)
    {
      // if the search was cleared
      if (query === '')
      {
        this.cardHoldersSubject.next(undefined);
      }

      // Must input 3 characters minimum
      //if (query.length >= 3) {
        // Apply the search query
        this.cardHoldersPageFilterSort.filter.filterValue = query;
        // Reset the pageIndex for a new search
        this.cardHoldersPageFilterSort.page.pageIndex = 0;
        // Fetch data with the updated filter and reset pageIndex
        this.fetchCardHolders(true);
      //}
    }
    else {
      this.cardHoldersSubject.next(undefined);
    }
  }


  /* C-Cure Lookup for new card credentials */
  private getCardHolderCredentialsFromCCure(cardHolderAndCredentials: ICardHolderAndCardCredentialsDTO): Observable<ICardHolderAndCardCredentialsDTO> {
  const cardHolder = cardHolderAndCredentials.cardHolder;

  this.isLoadingCardHoldersSubject.next(true);

  return this.adminApiService.CardHolders.CardCredentials
    .getSingleFromCCure(cardHolder.id)
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      map(cardCredential => {
        if (cardCredential != null) {
          // Append the new card credential to the array
          cardHolderAndCredentials.cardCredentials = cardHolderAndCredentials.cardCredentials || [];
          cardHolderAndCredentials.cardCredentials.push(cardCredential);
          this.isLoadingCardHoldersSubject.next(false);
          return cardHolderAndCredentials;
        }
        throw new Error('Card credential is null, expected valid credentials.');
      }),
      catchError(error => {
        this.isLoadingCardHoldersSubject.next(false);
        this.adminDialogService.error("Please direct " + cardHolder.firstName + ' ' + cardHolder.lastName + " to enrolment device to register credentials", 'No card credentials found');
        return throwError(() => new Error('Error fetching card credentials: ' + error.message));
      })
    );
  }
}