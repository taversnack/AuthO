import { ICardHolderDTO } from './card-holder';
import { DateTime, EntityId, IUsesGuidId } from "./common";

export enum LockerSecurityType
{
  None = 0,
  CombinationLock = 1,
  KeyLock = 2,
  SmartLock = 3
};

export const lockSecurityTypeToString = (lockSecurityType: LockerSecurityType): string => {
  switch(lockSecurityType) {
    case LockerSecurityType.None: return 'None';
    case LockerSecurityType.CombinationLock: return 'Combination Lock';
    case LockerSecurityType.KeyLock: return 'Key Lock';
    case LockerSecurityType.SmartLock: return 'Smart Lock';
    default: return '';
  }
};

// export enum LockerSecurityType
// {
//   None = 'None',
//   CombinationLock = 'CombinationLock',
//   KeyLock = 'KeyLock',
//   SmartLock = 'SmartLock'
// };

export interface ILockerDTO extends IUsesGuidId {
  // readonly id: EntityId; // from IUsesGuidId
  lockerBankId: EntityId;
  label: string;
  serviceTag?: string;
  securityType: LockerSecurityType;
  absoluteLeaseExpiry?: DateTime;
  currentLeaseHolder?: ICardHolderDTO;
}

export interface ICreateLockerDTO {
  lockerBankId: EntityId;
  label: string;
  serviceTag?: string;
  securityType: LockerSecurityType;
  absoluteLeaseExpiry?: DateTime;
}

export interface IUpdateLockerDTO {
  lockerBankId: EntityId;
  label: string;
  serviceTag?: string;
  securityType: LockerSecurityType;
  absoluteLeaseExpiry?: DateTime;
}
