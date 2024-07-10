import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Observable } from 'rxjs';
import { ILockerBankAdminLocationSummaryDTO, ILockerBankAdminLockerBankSummaryDTO, ILockerBankEvent } from 'src/app/v2/shared/models/location-locker-bank-summary';


@Component({
  selector: 'locker-bank-overview',
  templateUrl: './locker-bank-overview.component.html',
  styleUrls: ['locker-bank-overview.component.css']
})
export class LockerBankOverviewComponent {
  
  // Params
  @Input() lockerBankSummaries!: Observable<ILockerBankAdminLocationSummaryDTO[]>;
  
  // Card Click Event (Recieved from the Locker card component)
  @Output() summaryCardClicked: EventEmitter<ILockerBankEvent> = new EventEmitter<ILockerBankEvent>();

  // Toggle variable for expanding / collapsing panels
  expandedState: { [key: string]: boolean } = {};

  // For expanding / collapsing the Material expansion panel
  toggleLocation(locationId: string): void {
    this.expandedState[locationId] = !this.expandedState[locationId];
  }

  // propogation handler for the onClick function of the locker bank card.
  handleCardClick(lockerBankSummary: ILockerBankAdminLockerBankSummaryDTO, locationName: string): void {
    this.summaryCardClicked.emit({lockerBankSummary, locationName});
  }
}