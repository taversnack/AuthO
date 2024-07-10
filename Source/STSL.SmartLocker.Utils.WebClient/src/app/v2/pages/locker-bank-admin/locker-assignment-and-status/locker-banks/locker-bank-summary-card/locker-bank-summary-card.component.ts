import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ILockerBankAdminLockerBankSummaryDTO } from 'src/app/v2/shared/models/location-locker-bank-summary';

@Component({
  selector: 'locker-bank-summary-card',
  templateUrl: 'locker-bank-summary-card.component.html',
  styleUrls: ['locker-bank-summary-card.component.css']
})
export class LockerBankSummaryCardComponent {
  // Params
  @Input() lockerBankSummary!: ILockerBankAdminLockerBankSummaryDTO;

  // Card Click Event
  @Output() cardClicked: EventEmitter<ILockerBankAdminLockerBankSummaryDTO> = new EventEmitter<ILockerBankAdminLockerBankSummaryDTO>();

  // send locker bank to base component
  onCardClick(): void {
    this.cardClicked.emit(this.lockerBankSummary);
  }
}