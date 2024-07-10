import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { faChevronLeft, faInfoCircle, faCircleMinus, faPlus, IconDefinition, faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { LockerSecurityType } from 'src/app/shared/models/locker';
import { convertBatteryVoltageToPercentage, useLoading } from 'src/app/shared/lib/utilities';
import { faExclamationTriangle, faSpinner, faCheckCircle }from '@fortawesome/free-solid-svg-icons';
import { AuthService } from 'src/app/auth/services/auth.service';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { BehaviorSubject, finalize } from 'rxjs';
import { IPagingResponse } from 'src/app/shared/models/api';
import { ICardHolderAndCardCredentialsDTO } from 'src/app/shared/models/card-credential-and-card-holder';
import { DialogService } from 'src/app/dialog/services/dialog.service';
import { getCardCredentialIdsOnly, getCardHolderIdsOnly } from 'src/app/admin/admin-common/dialogs/view-entities/select-card-holders-or-card-credentials-dialog/select-card-holders-or-card-credentials-dialog.component';
import { AdminDialogService } from 'src/app/admin/services/admin-dialog.service';
import { lockSecurityTypeToString } from 'src/app/shared/models/locker';
import * as moment from 'moment';
import { getBatteryIcon, getBatteryLevelColour, getLockerUsageStatus, lockerBehaviourTypeToString } from 'src/app/v2/shared/utils/functions';
import { ILockerBankAdminLockerBankSummaryDTO, LockerBankBehaviour } from 'src/app/v2/shared/models/location-locker-bank-summary';

@Component({
  selector: 'locker-summary',
  templateUrl: './locker-summary.component.html',
  styleUrls: ['locker-summary.component.css']
})
export class LockerSummaryComponent implements OnInit {
  
  // Services
  private readonly authService = inject(AuthService);
  private readonly adminApiService = inject(AdminApiService);
  private readonly adminDialogService = inject(AdminDialogService);
  private readonly dialogService = inject(DialogService);
  private readonly changeDetector = inject(ChangeDetectorRef)

  // Params
  @Input() locationName!: string;
  @Input() lockerBankSummary!: ILockerBankAdminLockerBankSummaryDTO;
  @Input() lockerStatus!: ILockerStatusDTO;
  /* Margin inputs */
  @Input() fullWidth = false;
  @Input() fullHeight = false;
  @Input() verticallyCentered = false;
  
   // Close Event
   @Output() backClicked: EventEmitter<void> = new EventEmitter();
    // Card Click Event (Recieved from the card component)
   //@Output() summaryCardClicked: EventEmitter<ILockerStatusDTO> = new EventEmitter<ILockerStatusDTO>();

   // Font Awesome.. 
   faBackArrow = faChevronLeft;
   faInfo = faInfoCircle;
   faCircleMinus = faCircleMinus;
   faPlus = faPlus;
   faSpinner = faSpinner;
   faCheckCircle = faCheckCircle;
   faExclamationTriangle = faExclamationTriangle;
   faHelp = faCircleQuestion;

   // Variables
   cardHolderAliasPlural = this.authService.cardHolderAliasPlural;

   showAssignCardHoldersComponent = false;

   showSaveButton: boolean = false;
   isSaving = false;
   showSuccessMessage = false;

   readonly LockerSecurityType = LockerSecurityType;

   // Locker Owners
   private readonly lockerOwnersSubject = new BehaviorSubject<ICardHolderAndCardCredentialsDTO[]>([]);
   readonly lockerOwners$ = this.lockerOwnersSubject.asObservable();
   // Initially loaded owners, so that we can compare states when users are added or removed to show save button
   private initialLockerOwners: ICardHolderAndCardCredentialsDTO[] = [];

  // Loader
  private readonly isLoadingLockerOwnersSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerOwners$ = this.isLoadingLockerOwnersSubject.asObservable();

   ngOnInit(): void {
       
    this.fetchData();
   }

   /* API Request & Filtering */
  fetchData(): void {
    const lockerId = this.lockerStatus.lockerId;
  
    this.isLoadingLockerOwnersSubject.next(true);
    this.adminApiService.Lockers.Owners.getMany(lockerId)
      .subscribe({
          next: (response: IPagingResponse<ICardHolderAndCardCredentialsDTO>) => {
              this.lockerOwnersSubject.next(response.results || []);
              this.initialLockerOwners = response.results;
              this.isLoadingLockerOwnersSubject.next(false);
          },
          error: () => this.isLoadingLockerOwnersSubject.next(false)
      });

      this.updateSaveButtonVisibility();
  }

  /* Unassign user from locker */
  removeCardHolder(cardHolderId: string): void {
    const currentOwners = this.lockerOwnersSubject.value;
    const updatedOwners = currentOwners.filter(owner => owner.cardHolder.id !== cardHolderId);
    this.lockerOwnersSubject.next(updatedOwners);
    
    this.updateSaveButtonVisibility();
  }

  /* call back function from assign users componment to add a new user to locker */
  addCardHolder(cardHolder: ICardHolderAndCardCredentialsDTO): void {
    const currentOwners = this.lockerOwnersSubject.value || [];
  
    // check if selected card holder is already an owner or has been selected already
    if (!currentOwners.some(owner => owner.cardHolder.id === cardHolder.cardHolder.id)) {
      this.lockerOwnersSubject.next([...currentOwners, cardHolder]);
      this.updateSaveButtonVisibility();
    }

    // switch component view back to base
    this.toggleAssignCardHolders();
  }

  toggleAssignCardHolders() {
    this.showAssignCardHoldersComponent = !this.showAssignCardHoldersComponent;
    this.changeDetector.detectChanges();
  }

  // Show a warning when multiple are assigned to a permanent locker
  showMultipleStaffAssignedWarning() {
    return this.lockerBankSummary.behaviour === LockerBankBehaviour.Permanent && this.lockerOwnersSubject.value.length > 1
  }

  /* Save changes and update lockers / locks */
  saveChanges() {
    const isSmartLocker = this.lockerStatus.securityType === LockerSecurityType.SmartLock;
    const lockerId = this.lockerStatus.lockerId;
    const lockerOwners = this.lockerOwnersSubject.getValue();
  
    // Determine the API method based on locker type
    const apiCall$ = isSmartLocker ?
      this.adminApiService.Lockers.Owners.updateManyForSmartLocker(lockerId, getCardCredentialIdsOnly(lockerOwners)) :
      this.adminApiService.Lockers.Owners.updateManyForDumbLocker(lockerId, getCardHolderIdsOnly(lockerOwners));
  
    // Show loading indicator
    this.isLoadingLockerOwnersSubject.next(true);
    this.adminDialogService.showIsLoading(this.isLoadingLockerOwners$);
  
    apiCall$.pipe(
      finalize(() => {
        this.isLoadingLockerOwnersSubject.next(false);
        this.showSaveButton = false;

        // override initial owners to be updated 
        this.initialLockerOwners = lockerOwners;

        this.adminDialogService.showIsLoading(this.isLoadingLockerOwners$);
        if (isSmartLocker)
        {
          this.adminDialogService.alert("It may take up to 15 minutes for Lock changes to take effect", "Changes saved successfully");
        }
      })
    ).subscribe({
      error: error => {
        console.error('Failed to update locker owners:', error);
        this.adminDialogService.error('Failed to update locker owners.', 'Update Error');
      }
    });
  }

  handleBackClick() {
    if(this.showAssignCardHoldersComponent) // if back is pressed from the search
    {
      this.toggleAssignCardHolders();
    }
    else {
      this.backClicked.emit();
    }
  }

  /* Battery calculations */
  getBatteryLevelColour(voltage: number): string {
    return getBatteryLevelColour(voltage);
  }

  getBatteryIcon(voltage: number): IconDefinition {
    return getBatteryIcon(voltage);
  }

  getBatteryVoltagePercentage(batteryVoltage: number): string {
    return convertBatteryVoltageToPercentage(batteryVoltage);
  }

  // Get locker usage warning (e.g hasn't been used in 1 month)
  getAuditStatus(): number {
    const { lastAuditTime } = this.lockerStatus;

    return getLockerUsageStatus(lastAuditTime);
  }


  /* Lock Communication Status */
  lockHasRecentCommunication() {
    // TODO: [3] Get rid of MomentJS
    const minutesSinceLastCommunicationConsideredToBeOnline = 60;

    return this.lockerStatus.lastCommunication !== undefined
      && this.lockerStatus !== undefined
      && moment.utc(Date.now()).subtract(minutesSinceLastCommunicationConsideredToBeOnline, 'minutes').isBefore(moment.utc(this.lockerStatus.lastCommunication));
  }

  /* Save button */
  updateSaveButtonVisibility() {
    const currentOwners = this.lockerOwnersSubject.value;
    this.showSaveButton = !this.arraysAreEqual(this.initialLockerOwners, currentOwners);
  }

  // compare arrays (could be improved & generic-ified)
  arraysAreEqual(arr1: ICardHolderAndCardCredentialsDTO[], arr2: ICardHolderAndCardCredentialsDTO[]): boolean {
    const set1 = new Set(arr1.map(item => item.cardHolder.id));
    const set2 = new Set(arr2.map(item => item.cardHolder.id));
    if (set1.size !== set2.size) return false;
    for (let id of set1) {
      if (!set2.has(id)) return false;
    }
    return true;
  }


  // Get lock type as string
  getLockTypeName(lockType: LockerSecurityType)
  {
    return lockSecurityTypeToString(lockType);
  }

  getLockerTypeName(lockerBankBehaviour: LockerBankBehaviour)
  {
    return lockerBehaviourTypeToString(lockerBankBehaviour);
  }
}