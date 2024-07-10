import { Component, DestroyRef, Input, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { FeatureFlag } from 'src/app/feature-flag/feature-flag.type';
import { FeatureFlagService } from 'src/app/feature-flag/services/feature-flag.service';
import { lockFormatCsnToDecimal } from 'src/app/shared/lib/utilities';
import { IPagingRequest, IPagingResponse, PagingRequest } from 'src/app/shared/models/api';
import { StickyPosition, TableColumn } from 'src/app/shared/models/data-table';
import { ILockerDTO } from 'src/app/shared/models/locker';
import { ILockerAuditDTO } from 'src/app/shared/models/locker-audit';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';

const fullLockerAuditColumns: TableColumn<ILockerAuditDTO>[] = [
  'auditCategory',
  { name: 'auditDescription', headingDisplay: 'Event' },
  'subject',
  'object',
  'lockSerialNumber',
  'auditType',
  'objectSN',
  'subjectSN',
  { name: 'decimalSubjectSN', headingDisplay: 'Card CSN', getValue: x => x.subjectSN ? lockFormatCsnToDecimal(x.subjectSN) : undefined }
];

const minimalLockerAuditColumns: TableColumn<ILockerAuditDTO>[] = [
  'auditCategory',
  { name: 'auditDescription', headingDisplay: 'Event' },
  'subject',
  { name: 'decimalSubjectSN', headingDisplay: 'Card CSN', getValue: x => x.subjectSN ? lockFormatCsnToDecimal(x.subjectSN) : undefined }
  // 'auditTime',
];

@Component({
  selector: 'admin-locker-audits-view',
  templateUrl: './locker-audits-view.component.html',
})
export class LockerAuditsViewComponent implements OnInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly featureFlagService = inject(FeatureFlagService);

  @ViewChild('auditTimeColumn') auditTimeColumn?: TemplateRef<ILockerAuditDTO>;
  @ViewChild('decimalSubjectSerialNumberColumn') decimalSubjectSerialNumberColumn?: TemplateRef<ILockerAuditDTO>;

  @Input({ required: true }) currentlySelectedLocker$: Observable<ILockerDTO | ILockerStatusDTO | undefined> = of();

  private readonly lockerAuditsSubject = new BehaviorSubject<IPagingResponse<ILockerAuditDTO> | undefined>(undefined);
  readonly lockerAudits$ = this.lockerAuditsSubject.asObservable();

  private readonly isLoadingLockerAuditsSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerAudits$ = this.isLoadingLockerAuditsSubject.asObservable();

  currentlySelectedLocker?: ILockerDTO | ILockerStatusDTO;
  lockerDisplay: string = 'Locker Audits';

  lockerAuditPageFilterSort = { page: new PagingRequest() };

  readonly StickyPosition = StickyPosition;

  readonly lockFormatCsnToDecimal = lockFormatCsnToDecimal;

  private readonly lockerAuditColumnsSubject = new BehaviorSubject<TableColumn<ILockerAuditDTO>[]>([]);
  readonly lockerAuditColumns$ = this.lockerAuditColumnsSubject.asObservable();

  ngOnInit(): void {
    this.currentlySelectedLocker$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(locker => {
      this.currentlySelectedLocker = locker;
      if((this.currentlySelectedLocker as ILockerDTO).label) {
        const locker = (this.currentlySelectedLocker as ILockerDTO);
        this.lockerDisplay = `${locker.label}${locker.serviceTag ? ' ' + locker.serviceTag : ''}`;
      } else {
        this.lockerDisplay = (this.currentlySelectedLocker as ILockerStatusDTO).label
      }
      this.getLockerAudits();
    });

    this.featureFlagService.observeFeatureFlagChanges(FeatureFlag.ShowAllLockerAuditFields).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(flagIsSet => this.lockerAuditColumnsSubject.next(this.setTableColumns(flagIsSet)));
  }

  ngAfterViewInit(): void {
    if(this.auditTimeColumn) {
      this.lockerAuditColumnsSubject.next(this.setTableColumns(this.featureFlagService.hasFeatureFlagSet(FeatureFlag.ShowAllLockerAuditFields)));
    }
  }

  onPagingEvent(page: IPagingRequest) {
    const pageFilterSort = { ...this.lockerAuditPageFilterSort, page };
    this.lockerAuditPageFilterSort = pageFilterSort;
    this.getLockerAudits();
  }

  getLockerAudits() {
    if(!this.currentlySelectedLocker) {
      return;
    }

    const onResponse = {
      next: (x: IPagingResponse<ILockerAuditDTO>) => {
        this.lockerAuditsSubject.next(x);
        this.isLoadingLockerAuditsSubject.next(false);
      },
      error: () => this.isLoadingLockerAuditsSubject.next(false),
    };

    const lockerId = (this.currentlySelectedLocker as ILockerDTO).id || (this.currentlySelectedLocker as ILockerStatusDTO).lockerId;

    this.isLoadingLockerAuditsSubject.next(true);
    this.adminApiService.Lockers.Audits(lockerId, this.lockerAuditPageFilterSort).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(onResponse);
  }

  private setTableColumns(isFlagSetToShowAllLockerAuditFields: boolean): TableColumn<ILockerAuditDTO>[] {
    const fullColumns = [ ...fullLockerAuditColumns, { name: 'decimalSubjectSerialNumber', template: this.decimalSubjectSerialNumberColumn, headingDisplay: 'Decimal Subject Serial Number' }];
    return [...isFlagSetToShowAllLockerAuditFields ? fullColumns : minimalLockerAuditColumns, { name: 'auditTime', template: this.auditTimeColumn }];
  }
}
