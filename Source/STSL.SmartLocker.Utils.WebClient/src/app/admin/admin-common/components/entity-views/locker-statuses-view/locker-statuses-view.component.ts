import { AfterViewInit, Component, DestroyRef, Input, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import * as moment from 'moment';
import { BehaviorSubject, Observable, concatMap, filter, map, of } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { AuthService } from 'src/app/auth/services/auth.service';
import { FeatureFlag } from 'src/app/feature-flag/feature-flag.type';
import { FeatureFlagService } from 'src/app/feature-flag/services/feature-flag.service';
import { convertBatteryVoltageToPercentage, useLoading } from 'src/app/shared/lib/utilities';
import { IPagingRequest, IPagingResponse, ISortedRequest, PageFilterSort, PagingRequest, SortedRequest } from 'src/app/shared/models/api';
import { defaultCardHolderAliasPlural, defaultCardHolderAliasSingular } from 'src/app/shared/models/card-holder';
import { DomainConstants } from 'src/app/shared/models/common';
import { StickyPosition, TableColumn } from 'src/app/shared/models/data-table';
import { LockerSecurityType, lockSecurityTypeToString } from 'src/app/shared/models/locker';
import { ILockerBankDTO, LockerBankBehaviour } from 'src/app/shared/models/locker-bank';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { ISelectCardHoldersOrCardCredentialsDialogData, getCardCredentialIdsOnly, getCardHolderIdsOnly } from '../../../dialogs/view-entities/select-card-holders-or-card-credentials-dialog/select-card-holders-or-card-credentials-dialog.component';

const fullLockerStatusColumns: TableColumn<ILockerStatusDTO>[] = [
  'lockerId',
  'lockId',
  { name: 'locker', sortable: true },
  // { name: 'assignedTo', sortable: true },
  'assignedToCardHolderId',
  'assignedToUniqueIdentifier',
  { name: 'lockSerialNumber', sortable: true },
  'lockFirmwareVersion',
  'lockOperatingMode',
  'lastAuditSubjectId',
  'lastAuditObjectId',
  'lastAuditSubjectUniqueIdentifier',
  'lastAuditObjectUniqueIdentifier',
  'lastAuditSubject',
  'lastAuditObject',
  'lastAuditSubjectSN',
  'lastAuditObjectSN',
  'lastCommunication',
  'boundaryAddress',
  // 'lastAudit',
  'lastAuditCategory',
  { name: 'lastAuditDescription', headingDisplay: 'Last Event' },
  // 'lastAuditTime',
  'batteryVoltage',
];

const minimalLockerStatusColumns: TableColumn<ILockerStatusDTO>[] = [
  { name: 'serviceTag', sortable: true, overrideOrderPriority: 3 },
  { name: 'label', sortable: true, overrideOrderPriority: 2 },
  // { name: 'assignedTo', sortable: true },
  // { name: 'lockSerialNumber', sortable: true },
  // 'lockOperatingMode',
  // 'lastAudit',
  // 'lastAuditCategory',
  { name: 'securityType', getValue: x => lockSecurityTypeToString(x.securityType) },
  { name: 'lastAuditDescription', headingDisplay: 'Last Event', getValue: x => x.securityType === LockerSecurityType.SmartLock ? x.lastAuditDescription : 'N/A' },
];

@Component({
  selector: 'admin-locker-statuses-view',
  templateUrl: './locker-statuses-view.component.html',
})
export class LockerStatusesViewComponent implements OnInit, AfterViewInit {

  private readonly adminApiService = inject(AdminApiService);
  private readonly authService = inject(AuthService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly featureFlagService = inject(FeatureFlagService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('actions') actions?: TemplateRef<ILockerStatusDTO>;
  @ViewChild('assignedToColumn') assignedToColumn?: TemplateRef<ILockerStatusDTO>;
  @ViewChild('batteryColumn') batteryColumn?: TemplateRef<ILockerStatusDTO>;
  @ViewChild('lastAuditTimeColumn') lastAuditTimeColumn?: TemplateRef<ILockerStatusDTO>;
  @ViewChild('lastCommunicationTimeColumn') lastCommunicationTimeColumn?: TemplateRef<ILockerStatusDTO>;

  @Input() hideLockerAuditsAction = false;
  @Input() currentlySelectedLockerBank$: Observable<ILockerBankDTO | undefined> = of();
  @Input() lockerStatusActions?: TemplateRef<ILockerStatusDTO>;
  @Input() disableAssignment = false;
  @Input() disableViewConfigStatus = false;

  currentlySelectedLockerBank?: ILockerBankDTO;

  private readonly lockerStatusesSubject = new BehaviorSubject<IPagingResponse<ILockerStatusDTO> | undefined>(undefined);
  readonly lockerStatuses$ = this.lockerStatusesSubject.asObservable();

  private readonly isLoadingLockerStatusesSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerStatuses$ = this.isLoadingLockerStatusesSubject.asObservable();

  readonly lockerStatusFilters = ['label', 'assignedTo', 'serviceTag'];

  private lockerStatusPageFilterSort = PageFilterSort.usingFilter(this.lockerStatusFilters);

  readonly StickyPosition = StickyPosition;
  readonly LockerBankBehaviour = LockerBankBehaviour;
  readonly LockerSecurityType = LockerSecurityType;

  cardHolderAliasSingular = this.authService.cardHolderAliasSingular;
  cardHolderAliasPlural = this.authService.cardHolderAliasPlural;

  private readonly lockerStatusColumnsSubject = new BehaviorSubject<TableColumn<ILockerStatusDTO>[]>([]);
  readonly lockerStatusColumns$ = this.lockerStatusColumnsSubject.asObservable();

  lastResponseReceivedAt?: number;

  ngOnInit(): void {
    this.currentlySelectedLockerBank$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(lockerBank => {
      this.currentlySelectedLockerBank = lockerBank;
      this.lockerStatusPageFilterSort.resetQuery();
      this.getLockerStatuses();
    });

    this.featureFlagService.observeFeatureFlagChanges(FeatureFlag.ShowAllLockerStatusFields).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(flagIsSet => this.lockerStatusColumnsSubject.next(this.setTableColumns(flagIsSet)));

    this.authService.currentTenantDetails$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(x => {
      this.cardHolderAliasSingular = x?.cardHolderAliasSingular ?? defaultCardHolderAliasSingular;
      this.cardHolderAliasPlural = x?.cardHolderAliasPlural ?? defaultCardHolderAliasPlural;
    });
  }

  ngAfterViewInit(): void {
    if(this.actions) {
      this.lockerStatusColumnsSubject.next(this.setTableColumns(this.featureFlagService.hasFeatureFlagSet(FeatureFlag.ShowAllLockerStatusFields)));
      // TODO: explicit change detection here
    }
  }

  lockerStatusFiltersChanged(filterProperties: string[]) {
    this.lockerStatusPageFilterSort.filter.filterProperties = filterProperties;
  }

  searchLockerStatuses(searchParams: { query: string; filterBy?: string }) {
    const query = searchParams.query;
    this.lockerStatusPageFilterSort.resetQuery(query)
    this.getLockerStatuses();
  }

  viewLockerAuditsForLocker(lockerStatus: ILockerStatusDTO) {
    this.adminDialogService.openViewLockerAuditsDialog(lockerStatus);
  }

  viewLockUpdateStatusForLocker(lockerStatus: ILockerStatusDTO) {
    if(lockerStatus.lockId) {
      const { isLoading$, source$ } = useLoading(this.adminApiService.Locks.IsConfigPending(lockerStatus.lockId).pipe(takeUntilDestroyed(this.destroyRef)));

      this.adminDialogService.showIsLoading(isLoading$, 'Contacting lock gateway, please wait..');

      source$.subscribe(isPending => {
        if(isPending) {
          this.adminDialogService.alert('Update is still in progress', 'Update pending');
        } else {
          this.adminDialogService.alert('No updates in progress', 'Lock is up to date');
        }
      });
    }
  }

  viewLocationReferenceImage()
  {
    if(this.currentlySelectedLockerBank)
    {
      this.adminApiService.Locations.ReferenceImages.getSingle(this.currentlySelectedLockerBank?.locationId)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(image => {
          this.adminDialogService.viewReferenceImage(image?.blobSASToken);
        });
    }
  }

  viewLockerBankReferenceImage(){
    if(this.currentlySelectedLockerBank)
    {
      this.adminApiService.LockerBanks.ReferenceImages.getSingle(this.currentlySelectedLockerBank?.id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(image => {
          this.adminDialogService.viewReferenceImage(image?.blobSASToken);
        });
    }
  }

  unassignCardCredentialsFromLockerId(lockerStatus: ILockerStatusDTO) {
    const { lockerId, securityType, assignedTo, assignedToUniqueIdentifier, label } = lockerStatus;
    this.adminDialogService.confirm(`Are you sure you want to unassign ${assignedTo} (${assignedToUniqueIdentifier}) from ${label}?`, `Unassign ${label}?`)
    .pipe(
      filter(confirmed => confirmed),
      concatMap(() => {
        const { isLoading$, source$ } = useLoading(securityType === LockerSecurityType.SmartLock ?
        this.adminApiService.Lockers.Owners.updateManyForSmartLocker(lockerId, [])
        :
        this.adminApiService.Lockers.Owners.updateManyForDumbLocker(lockerId, []));
        this.adminDialogService.showIsLoading(isLoading$);
        return source$;
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(deleted => deleted && this.getLockerStatuses());
  }

  assignTemporaryBankCardHolders() {
    if(!this.currentlySelectedLockerBank) {
      return;
    }

    const currentLockerBankId = this.currentlySelectedLockerBank.id;
    const { isLoading$, source$ } = useLoading(
      this.adminApiService.LockerBanks.LeaseUsers.getMany(currentLockerBankId, { page: new PagingRequest(DomainConstants.MaxUserCardCredentialsPerLocker) })
        .pipe(map(x => x.results))
    );

    const dialogSettings: ISelectCardHoldersOrCardCredentialsDialogData = {
      currentlySelectedCardHoldersAndCardCredentials$: source$,
      isLoadingCurrentlySelectedCardHoldersAndCardCredentials$: isLoading$,
    };

    this.adminDialogService.openSelectCardHoldersOrCardCredentialsDialog(dialogSettings).pipe(
      concatMap(assignedCardHoldersAndCredentials => {
        const cardCredentialIds = getCardCredentialIdsOnly(assignedCardHoldersAndCredentials);
        return this.adminApiService.LockerBanks.LeaseUsers.updateMany(currentLockerBankId, cardCredentialIds);
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(updated => updated && this.getLockerStatuses());
  }

  updateLockerCardCredentialsFromSelectionDialog(lockerStatus: ILockerStatusDTO, title: string = 'Assign to ' + lockerStatus.label) {
    const { lockerId, securityType } = lockerStatus;

    const isSmartLocker = securityType === LockerSecurityType.SmartLock;

    const { isLoading$, source$ } = useLoading(
      this.adminApiService.Lockers.Owners.getMany(lockerId, { page: new PagingRequest(DomainConstants.MaxUserCardCredentialsPerLocker) })
        .pipe(map(x => x.results))
    );

    const dialogSettings: ISelectCardHoldersOrCardCredentialsDialogData = {
      currentlySelectedCardHoldersAndCardCredentials$: source$,
      isLoadingCurrentlySelectedCardHoldersAndCardCredentials$: isLoading$,
      limitCardHolderCount: DomainConstants.MaxUserCardCredentialsPerLocker,
      limitCardHoldersCardCredentialCount: DomainConstants.MaxUserCardCredentialsPerLocker,
      allowUsersWithoutACard: !isSmartLocker,
      selectCardHoldersOnly: !isSmartLocker,
    };

    this.adminDialogService.openSelectCardHoldersOrCardCredentialsDialog(dialogSettings, title).pipe(
      concatMap(assignedCardHoldersAndCredentials => {
        const { isLoading$, source$ } = useLoading(isSmartLocker ?
          this.adminApiService.Lockers.Owners.updateManyForSmartLocker(lockerId, getCardCredentialIdsOnly(assignedCardHoldersAndCredentials))
          :
          this.adminApiService.Lockers.Owners.updateManyForDumbLocker(lockerId, getCardHolderIdsOnly(assignedCardHoldersAndCredentials)));
        this.adminDialogService.showIsLoading(isLoading$);
        return source$;
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(updated => updated && this.getLockerStatuses());
  }

  onPagingEvent(page: IPagingRequest) {
    this.lockerStatusPageFilterSort.page = page;
    this.getLockerStatuses();
  }

  onSortingEvent(sort: ISortedRequest = new SortedRequest()) {
    this.lockerStatusPageFilterSort.sort = sort;
    if(this.lockerStatusPageFilterSort.sort.sortBy === 'lockOnline') {
      this.lockerStatusPageFilterSort.sort.sortBy = 'lastcommunication';
    }
    this.getLockerStatuses();
  }

  lockHasRecentCommunication(lockerStatus: ILockerStatusDTO) {
    // TODO: [3] Get rid of MomentJS
    const minutesSinceLastCommunicationConsideredToBeOnline = 60;

    return lockerStatus.lastCommunication !== undefined
      && this.lastResponseReceivedAt !== undefined
      && moment.utc(this.lastResponseReceivedAt).subtract(minutesSinceLastCommunicationConsideredToBeOnline, 'minutes').isBefore(moment.utc(lockerStatus.lastCommunication));
  }

  /*getBatteryLevelClasses(batteryVoltage: number): { [klass: string]: boolean } {
    return {
      'text-red-500': batteryVoltage <= 3.45,
      'text-yellow-500': batteryVoltage > 3.45 && batteryVoltage < 3.6,
      'text-green-600': batteryVoltage >= 3.6,
    };
  }*/

  getBatteryLevelColour(voltage: number): string {
    if (voltage >= 3.6) return '#57C4AD';
    else if (voltage > 3.45 && voltage < 3.6) return '#EDA247';
    else return '#DB4352';
  }

  getBatteryIcon(voltage: number): string {
    if (voltage >= 4.532) return 'battery_full'; // 95% + 
    else if (voltage < 4.532 && voltage >= 4.298) return 'battery_6_bar'; // 80 - 94%
    else if (voltage < 4.298 && voltage >= 4.110) return 'battery_5_bar'; // 60 - 79%
    else if (voltage < 4.110 && voltage >= 3.954) return 'battery_4_bar'; // 40 - 59%
    else if (voltage < 3.954 && voltage >= 3.772) return 'battery_3_bar'; // 20 - 49%
    else if (voltage < 3.772 && voltage >= 3.580) return 'battery_2_bar'; // 10 - 19%
    else if (voltage < 3.580 && voltage >= 3.300) return 'battery_1_bar'; // 1 - 10%
    else return 'battery_alert'; // battery dead
  }

  getBatteryVoltagePercentage(batteryVoltage: number): string {
    return convertBatteryVoltageToPercentage(batteryVoltage);
  }

  private setTableColumns(isFlagSetToShowAllLockerStatusFields: boolean) {
    return [...isFlagSetToShowAllLockerStatusFields ? fullLockerStatusColumns : minimalLockerStatusColumns, ...this.specialRenderingColumns()];
  }

  private specialRenderingColumns(): TableColumn<ILockerStatusDTO>[] {
    return [
      { name: 'assignedTo', template: this.assignedToColumn, sortable: true, overrideOrderPriority: 1 },
      { name: 'lastAuditTime', template: this.lastAuditTimeColumn, headingDisplay: 'Last Event Time', sortable: true },
      { name: 'battery', template: this.batteryColumn, sortable: true },
      { name: 'lockOnline', template: this.lastCommunicationTimeColumn, sortable: true },
      { name: 'actions', template: this.actions, stickyPosition: StickyPosition.End },
    ]
  }

  private getLockerStatuses() {
    const onResponse = {
      next: (x: IPagingResponse<ILockerStatusDTO>) => {
        this.lockerStatusesSubject.next(x);
        this.isLoadingLockerStatusesSubject.next(false);
        this.lastResponseReceivedAt = Date.now();
      },
      error: () => this.isLoadingLockerStatusesSubject.next(false),
    };

    if(this.currentlySelectedLockerBank) {
      this.isLoadingLockerStatusesSubject.next(true);
      this.adminApiService.LockerBanks.Lockers
        .Statuses(this.currentlySelectedLockerBank.id, this.lockerStatusPageFilterSort).subscribe(onResponse);
    }
  }
}
