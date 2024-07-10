import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { IconDefinition, faBatteryEmpty, faBatteryFull, faBatteryHalf, faBatteryQuarter, faBatteryThreeQuarters, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import { LockerSecurityType } from 'src/app/shared/models/locker';
import { ILockerStatusDTO } from 'src/app/shared/models/locker-status';
import * as moment from 'moment';
import { convertBatteryVoltageToPercentage, getBatteryIcon, getBatteryLevelColour, getLockerUsageStatus } from 'src/app/v2/shared/utils/functions';

@Component({
  selector: 'locker-summary-card',
  templateUrl: 'locker-summary-card.component.html',
  styleUrls: ['locker-summary-card.component.css']
})
export class LockerSummaryCardComponent {
  @Input() lockerSummary!: ILockerStatusDTO;

   // Card Click Event (Recieved from the card component)
   @Output() cardClicked: EventEmitter<ILockerStatusDTO> = new EventEmitter<ILockerStatusDTO>();

  // Font Awesome
  faBatteryFull = faBatteryFull;
  faBattery75 = faBatteryThreeQuarters;
  faBattery50 = faBatteryHalf;
  faBattery25 = faBatteryQuarter;
  faBatteryEmpty = faBatteryEmpty;
  faExclamationTriangle = faExclamationTriangle;

  readonly LockerSecurityType = LockerSecurityType;
  
  // propogation handler for the onClick function of the locker card.
  onCardClick(lockerSummary: ILockerStatusDTO): void {
    this.cardClicked.emit(lockerSummary);
  }


  lockHasRecentCommunication() {
    // TODO: [3] Get rid of MomentJS
    const minutesSinceLastCommunicationConsideredToBeOnline = 60;

    return this.lockerSummary.lastCommunication !== undefined
      && this.lockerSummary !== undefined
      && moment.utc(Date.now()).subtract(minutesSinceLastCommunicationConsideredToBeOnline, 'minutes').isBefore(moment.utc(this.lockerSummary.lastCommunication));
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
    const { lastAuditTime } = this.lockerSummary;

    return getLockerUsageStatus(lastAuditTime);
  }
}