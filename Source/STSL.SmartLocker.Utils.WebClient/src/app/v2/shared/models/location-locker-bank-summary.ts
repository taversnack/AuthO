import { IHasReferenceImage, IUsesGuidId } from "src/app/shared/models/common";

export enum LockerBankBehaviour
{
  Unset,
  Permanent = 'Permanent',
  Temporary = 'Temporary',
};

export interface ILockerBankAdminLocationSummaryDTO extends IUsesGuidId, IHasReferenceImage {
    locationName: string;
    locationDescription?: string;
    lockerBankSummaries: ILockerBankAdminLockerBankSummaryDTO[];
};

export interface ILockerBankAdminLockerBankSummaryDTO extends IUsesGuidId, IHasReferenceImage {
    name: string;
    locationName: string;
    description?: string;
    behaviour: LockerBankBehaviour;
    lockerCount: number; 
    vacancyCount: number; 
    usage: string;
    hasWarnings: boolean;
};


/* Event handler for emitting location name as well as DTO */
export interface ILockerBankEvent {
  lockerBankSummary: ILockerBankAdminLockerBankSummaryDTO;
  locationName: string;
}