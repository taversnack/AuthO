import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { faBatteryFull, faBatteryThreeQuarters, faBatteryHalf, faBatteryQuarter, faBatteryEmpty } from "@fortawesome/free-solid-svg-icons";
import * as moment from "moment";
import { DateTime } from "src/app/shared/models/common";
import { LockerBankBehaviour } from "../models/location-locker-bank-summary";
import { LockerSecurityType } from "src/app/shared/models/locker";


/* TODO: Improve these functions maybe */
export function getBatteryLevelColour(voltage: number): string {
    if (voltage >= 3.6) return '#57C4AD';
    else if (voltage > 3.45 && voltage < 3.6) return '#EDA247';
    else return '#DB4352';
}

export function getBatteryIcon(voltage: number): IconDefinition {
    if (voltage >= 4.532) return faBatteryFull; // 95% + 
    else if (voltage < 4.532 && voltage >= 4.110) return faBatteryThreeQuarters; // 80 - 60%
    ///else if (voltage < 4.298 && voltage >= 4.110) return 'battery_5_bar'; // 60 - 79%
    else if (voltage < 4.110 && voltage >= 3.954) return faBatteryHalf; // 40 - 59%
    else if (voltage < 3.954 && voltage >= 3.580) return faBatteryHalf; // 20 - 49%
    else if (voltage < 3.580 && voltage >= 3.300) return faBatteryQuarter; // 1 - 10%
    else return faBatteryEmpty; // battery dead
}

  
export function convertBatteryVoltageToPercentage(voltage: number): string {
    // Coefficients
    const c1 = 188.7230365;
    const c2 = -3836.230;
    const c3 = 30973.71818;
    const c4 = -124152.295;
    const c5 = 247116.5623;
    const c6 = -195479.5779;
  
    // Apply formula
    const percentage = c1 * Math.pow(voltage, 5) + c2 * Math.pow(voltage, 4) + c3 * Math.pow(voltage, 3) + c4 * Math.pow(voltage, 2) + c5 * voltage + c6;
    
    // Round the result and ensure it's within 0-100%
    const roundedPercentage = Math.min(Math.max(Math.round(percentage), 0), 100);
    return `${roundedPercentage}%`;
}
  

// Returns a number indicating the status of the locker usage.
  // 0: Audit overdue (hasn't been used in 1 month),
  // 1: lastAuditTime is null/undefined,
  // 2: No warning
export function getLockerUsageStatus(date?: DateTime): number {
    
    if (date === null || date === undefined) {
      return 1; // Indicates the locker is newly assigned or audit data not available.
    }

    // Normalize lastAuditTime to a Date object
    let normalizedLastAuditDate: Date;

    if (date instanceof Date) {
      normalizedLastAuditDate = date;

    } else if (moment.isMoment(date)) {
      normalizedLastAuditDate = date.toDate();
    } else {
      normalizedLastAuditDate = new Date(date);
    }

    const oneMonthAgo = new Date();
    oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);

    return normalizedLastAuditDate < oneMonthAgo ? 0 : 2;
}


export const lockerBehaviourTypeToString = (lockSecurityType: LockerBankBehaviour): string => {
    switch(lockSecurityType) {
      case LockerBankBehaviour.Unset: return 'None';
      case LockerBankBehaviour.Permanent: return 'Permanent';
      case LockerBankBehaviour.Temporary: return 'Temporary';
      default: return '';
    }
  };
  