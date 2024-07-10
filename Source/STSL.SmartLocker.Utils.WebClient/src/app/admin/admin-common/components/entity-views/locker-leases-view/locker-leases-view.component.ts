import { AfterViewInit, ChangeDetectorRef, Component, DestroyRef, Input, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { lockFormatCsnToDecimal } from 'src/app/shared/lib/utilities';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, PageFilterSortRequest, SortedRequest } from 'src/app/shared/models/api';
import { StickyPosition, TableColumn } from 'src/app/shared/models/data-table';
import { lockSecurityTypeToString } from 'src/app/shared/models/locker';
import { ILockerLeaseDTO } from 'src/app/shared/models/locker-lease';
import { SearchFilters } from 'src/app/shared/models/search-filters';

@Component({
  selector: 'admin-locker-leases-view',
  templateUrl: './locker-leases-view.component.html',
})
export class LockerLeasesViewComponent implements OnInit, AfterViewInit {

  // private readonly dialogService = inject(DialogService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetector = inject(ChangeDetectorRef);

  @ViewChild('actions') actions?: TemplateRef<ILockerLeaseDTO>;
  @ViewChild('startedAtColumn') startedAtColumn?: TemplateRef<ILockerLeaseDTO>;
  @ViewChild('endedAtColumn') endedAtColumn?: TemplateRef<ILockerLeaseDTO>;
  @ViewChild('lockInstallationDateColumn') lockInstallationDateColumn?: TemplateRef<ILockerLeaseDTO>;

  @Input() useEndpoint: PageFilterSortRequest<ILockerLeaseDTO> = this.adminApiService.LockerLeases.getMany;

  @Input() hideCardCredentialColumns: boolean = false;
  @Input() hideCardHolderColumns: boolean = false;
  @Input() hideLockerColumns: boolean = false;
  @Input() hideLockColumns: boolean = false;
  @Input() hideEndedByMasterCardColumn: boolean = true;

  private readonly lockerLeasesSubject = new BehaviorSubject<IPagingResponse<ILockerLeaseDTO> | undefined>(undefined);
  readonly lockerLeases$ = this.lockerLeasesSubject.asObservable();

  private readonly isLoadingLockerLeasesSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerLeases$ = this.isLoadingLockerLeasesSubject.asObservable();

  private readonly lockerLeaseColumnsSubject = new BehaviorSubject<TableColumn<ILockerLeaseDTO>[]>([]);
  readonly lockerLeaseColumns$ = this.lockerLeaseColumnsSubject.asObservable();

  readonly lockerLeaseFilters: SearchFilters = [
    { name: 'cardholdername', display: 'Leaseholder Name' },
    { name: 'cardholderuniqueidentifier', display: 'Leaseholder Unique ID' },
    { name: 'cardholderemail', display: 'Leaseholder Email' },
    'lockerServiceTag',
    'lockerLabel',
    'lockerBankBehaviour',
  ];

  private lockerLeasePageFilterSort = PageFilterSort.usingFilter(this.lockerLeaseFilters);

  readonly StickyPosition = StickyPosition;

  ngOnInit(): void {
    this.getLockerLeases();
  }

  ngAfterViewInit(): void {
    const noValue = undefined;

    // TODO: Thin out columns

    const cardCredentialColumns: TableColumn<ILockerLeaseDTO>[] = [
      { name: 'cardcredentialserialnumber', headingDisplay: 'Card CSN', getValue: x => x.cardCredential?.serialNumber ? lockFormatCsnToDecimal(x.cardCredential.serialNumber) : noValue, sortable: false },
      { name: 'cardcredentialhidnumber', headingDisplay: 'Card HID', getValue: x => x.cardCredential?.hidNumber, sortable: false },
      // { name: 'cardType', getValue: x => x.cardCredential?.cardType },
      { name: 'cardLabel', getValue: x => x.cardCredential?.cardLabel },
    ];

    const cardHolderColumns: TableColumn<ILockerLeaseDTO>[] = [
      { name: 'cardholdername', headingDisplay: 'Leaseholder Name', getValue: x => x.cardHolder ? `${x.cardHolder.firstName} ${x.cardHolder.lastName}` : noValue },
      { name: 'cardholderuniqueidentifier', headingDisplay: 'Leaseholder Unique ID', getValue: x => x.cardHolder?.uniqueIdentifier },
      { name: 'cardholderemail', headingDisplay: 'Leaseholder Email', getValue: x => x.cardHolder?.email },
    ];

    const lockerColumns: TableColumn<ILockerLeaseDTO>[] = [
      { name: 'lockerLabel', getValue: x => x.locker?.label },
      { name: 'lockerServiceTag', getValue: x => x.locker?.serviceTag },
      { name: 'lockerSecurityType', getValue: x => x.locker ? lockSecurityTypeToString(x.locker.securityType) : noValue },
    ];

    const lockColumns: TableColumn<ILockerLeaseDTO>[] = [
      { name: 'lockSerialNumber', getValue: x => x.lock?.serialNumber },
      { name: 'lockInstallationDate', template: this.lockInstallationDateColumn },
      // { name: 'lockFirmwareVersion', getValue: x => x.lock?.firmwareVersion },
      { name: 'lockOperatingMode', getValue: x => x.lock?.operatingMode },
    ];

    this.lockerLeaseColumnsSubject.next([
      { name: 'startedAt', template: this.startedAtColumn, sortable: true },
      { name: 'endedAt', template: this.endedAtColumn, sortable: true },
      { name: 'leaseType', getValue: x => x.lockerBankBehaviour },
      ...this.hideEndedByMasterCardColumn ? [] : ['endedByMasterCard'] as TableColumn<ILockerLeaseDTO>[],
      ...this.hideCardCredentialColumns ? [] : cardCredentialColumns,
      ...this.hideCardHolderColumns ? [] : cardHolderColumns,
      ...this.hideLockerColumns ? [] : lockerColumns,
      ...this.hideLockColumns ? [] : lockColumns,
      // { name: 'actions', template: this.actions, sortable: false, stickyPosition: StickyPosition.End },
    ]);

    this.changeDetector.detectChanges();
  }

  lockerLeaseFiltersChanged(filterProperties: string[]) {
    this.lockerLeasePageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLockerLeases(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockerLeasePageFilterSort.resetQuery(query);
    this.getLockerLeases();
  }

  onPagingEvent(page: IPagingRequest) {
    this.lockerLeasePageFilterSort.page = page;
    this.getLockerLeases();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.lockerLeasePageFilterSort.sort = sort;
    this.getLockerLeases();
  }

  private getLockerLeases() {
    const onResponse = {
      next: (x: IPagingResponse<ILockerLeaseDTO>) => {
        this.lockerLeasesSubject.next(x);
        this.isLoadingLockerLeasesSubject.next(false);
      },
      error: () => this.isLoadingLockerLeasesSubject.next(false),
    };

    this.isLoadingLockerLeasesSubject.next(true);

    this.useEndpoint(this.lockerLeasePageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
  }
}
