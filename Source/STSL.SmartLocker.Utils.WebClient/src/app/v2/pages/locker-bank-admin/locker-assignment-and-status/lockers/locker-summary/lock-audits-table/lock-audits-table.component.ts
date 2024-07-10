import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { Component, Input, OnInit, ViewChild, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subject, debounceTime, map } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { IPagingResponse, PageFilterSort } from 'src/app/shared/models/api';
import { ILockerAuditDTO } from 'src/app/shared/models/locker-audit';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';


@Component({
  selector: 'lock-audit-table',
  templateUrl: './lock-audits-table.component.html',
  styleUrls: ['./lock-audits-table.component.css']
})
export class LocksAuditTableComponent implements OnInit {

    // Services
  private readonly adminApiService = inject(AdminApiService);
  private readonly authService = inject(AuthService);


  @Input() lockerStatus!: ILockerStatusDTO;
  
  // Listener for the Virtual Scroll Component
  @ViewChild(CdkVirtualScrollViewport) viewport!: CdkVirtualScrollViewport;

    // Variables
  private readonly lockerAuditsSubject = new BehaviorSubject<IPagingResponse<ILockerAuditDTO> | undefined>(undefined);
  readonly lockerAudits$ = this.lockerAuditsSubject.asObservable();

  cardHolderAlias = this.authService.cardHolderAliasSingular;

  // Loader
  private readonly isLoadingLockerAuditsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerAudits$ = this.isLoadingLockerAuditsSubject.asObservable();

  // Sort Filters
  readonly auditFilters = ['auditCategory', 'auditTime', 'subject'];
  private lockerAuditsPageFilterSort = PageFilterSort.usingFilter(this.auditFilters);

  private scrollEventsSubject = new Subject<void>();

  // Map here so we get an Observable<LockerSatusDTO[]> instead of IPagingResponse, which is not accepted by the virtual scroll component
  readonly lockAudits$ = this.lockerAuditsSubject.asObservable().pipe(
      map(response => response?.results || [])
  );

  displayedColumns: string[] = ['description', this.cardHolderAlias, 'time'];

  ngOnInit(): void {
    this.getLockerAudits();

    // Initialize debounced scroll event handling
    this.scrollEventsSubject.pipe(
      debounceTime(750) // Debounce time in milliseconds
    ).subscribe(() => {
      this.onDebouncedScroll();
    });
  }

  getLockerAudits() {
    if(!this.lockerStatus) {
      return;
    }

    // Adjusting the number of records per page to be more appopriate for infinite scrolling
    this.lockerAuditsPageFilterSort.page.recordsPerPage = 5;

    const onResponse = (response: IPagingResponse<ILockerAuditDTO>) => {
        const currentData = this.lockerAuditsSubject.value?.results || [];
        const newData = [...currentData, ...response.results];
        this.lockerAuditsSubject.next({ ...response, results: newData });
        this.isLoadingLockerAuditsSubject.next(false);

        console.log(this.lockAudits$);
    };
    
    this.isLoadingLockerAuditsSubject.next(true);
    this.adminApiService.Lockers.Audits(this.lockerStatus.lockerId, this.lockerAuditsPageFilterSort )
        .subscribe(onResponse);
  }

  /*private setTableColumns(isFlagSetToShowAllLockerAuditFields: boolean): TableColumn<ILockerAuditDTO>[] {
    const fullColumns = [ ...fullLockerAuditColumns, { name: 'decimalSubjectSerialNumber', template: this.decimalSubjectSerialNumberColumn, headingDisplay: 'Decimal Subject Serial Number' }];
    return [...isFlagSetToShowAllLockerAuditFields ? fullColumns : minimalLockerAuditColumns, { name: 'auditTime', template: this.auditTimeColumn }];
  }*/

  onScroll(): void {
    this.scrollEventsSubject.next();
  }

  onDebouncedScroll(): void {
    // Get the total number of records that can be obtained
    const totalRecords = this.lockerAuditsSubject.value?.totalRecords || 0;
  
    const end = this.viewport.getRenderedRange().end;
    const total = this.viewport.getDataLength();
  
    if (total >= totalRecords) {
      return;
    }
  
    if (end === total) {
      this.lockerAuditsPageFilterSort.page.pageIndex++;
      this.getLockerAudits();
    }
  }
}