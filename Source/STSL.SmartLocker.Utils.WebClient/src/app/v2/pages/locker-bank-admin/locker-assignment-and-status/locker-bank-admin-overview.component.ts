import { animate, style, transition, trigger } from '@angular/animations';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { BehaviorSubject, Subject, map } from 'rxjs';
import { AdminApiService } from 'src/app/admin/services/admin-api.service';
import { IPagingResponse } from 'src/app/shared/models/api';
import { LockerBankBehaviour } from 'src/app/shared/models/locker-bank';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import { ILockerBankAdminLocationSummaryDTO, ILockerBankAdminLockerBankSummaryDTO, ILockerBankEvent } from 'src/app/v2/shared/models/location-locker-bank-summary';


/*export const fadeInAnimation = trigger('fadeInAnimation', [
  transition(':enter', [
    style({ opacity: 0 }),  // initial state
    animate('300ms', style({ opacity: 1 }))
  ])
]);*/ // Sample animation on page load

@Component({
  selector: 'locker-bank-admin-overview',
  templateUrl: './locker-bank-admin-overview.component.html',
  styleUrls: ['locker-bank-admin-overview.component.css']
  //animations: [fadeInAnimation]
})
export class LockerBankAdminOverviewComponent implements OnInit {

  // Services
  private readonly adminApiService = inject(AdminApiService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  // Variables
  private readonly lockerBankSummariesSubject = new BehaviorSubject<IPagingResponse<ILockerBankAdminLocationSummaryDTO> | undefined>(undefined);
  
  readonly lockerBankSummaries$ = this.lockerBankSummariesSubject.asObservable().pipe(
    map(response => response?.results || [])
  );

  // Loader
  private readonly isLoadingLockerBankSummariesSubject = new BehaviorSubject<boolean>(false);
  readonly isLoadingLockerBankSummaries$ = this.isLoadingLockerBankSummariesSubject.asObservable();

  // Navigation
  selectedLockerBankSummary: ILockerBankAdminLockerBankSummaryDTO | null = null;
  selectedLockerBankLocationName: string | null = null;
  selectedLockerSummary: ILockerStatusDTO | null = null;

  ngOnInit(): void {
    this.fetchData(); // make sure this is completed / awaited
  }

  fetchData(): void {
    this.isLoadingLockerBankSummariesSubject.next(true);

    this.adminApiService.LockerBankAdmins.LockerBankSummaries()
        .subscribe({
          next: (response) => {
            this.lockerBankSummariesSubject.next({ ...response });
            this.isLoadingLockerBankSummariesSubject.next(false);
          },
          error: () => {
            this.isLoadingLockerBankSummariesSubject.next(false);
          }
        });
  }

  handleSummaryCardClicked(lockerBankEvent: ILockerBankEvent): void {

    this.selectedLockerBankLocationName = lockerBankEvent.locationName;
    this.selectedLockerBankSummary = lockerBankEvent.lockerBankSummary;
  }

  handleLockerCardClicked(lockerStatus: ILockerStatusDTO): void {
    this.selectedLockerSummary = lockerStatus
  }

  clearSelection(): void {
    this.selectedLockerBankSummary = null;
    this.selectedLockerBankLocationName = null;
  }

  clearLockerSelection(): void {
    this.selectedLockerSummary = null;
  }
}