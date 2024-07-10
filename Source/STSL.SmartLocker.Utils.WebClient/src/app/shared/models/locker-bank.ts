import { EntityId, IHasReferenceImage, IUsesGuidId } from "./common";

// NOTE: [0] Minor network optimization to avoid strings
// export enum LockerBankBehaviour
// {
//   Unset,
//   Permanent,
//   Temporary,
// }

export enum LockerBankBehaviour
{
  Unset,
  Permanent = 'Permanent',
  Temporary = 'Temporary',
}

export interface ILockerBankDTO extends IUsesGuidId, IHasReferenceImage {
  // readonly id: EntityId; // from IUsesGuidId
  locationId: EntityId;
  name: string;
  description: string;
  behaviour: LockerBankBehaviour;
  // onlyContainsSmartLocks: boolean;
}

export interface ICreateLockerBankDTO {
  locationId: EntityId;
  name: string;
  description?: string;
  behaviour: LockerBankBehaviour;
}

export interface IUpdateLockerBankDTO {
  locationId: EntityId;
  name: string;
  description?: string;
  behaviour: LockerBankBehaviour;
}
