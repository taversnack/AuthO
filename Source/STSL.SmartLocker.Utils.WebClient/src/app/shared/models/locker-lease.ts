import { ICardCredentialDTO } from './card-credential'
import { ICardHolderDTO } from './card-holder'
import { DateTimeOffset, IUsesGuidId } from './common'
import { ILockDTO } from './lock'
import { ILockerDTO } from './locker'
import { LockerBankBehaviour } from './locker-bank'

export interface ILockerLeaseDTO extends IUsesGuidId {
  startedAt?: DateTimeOffset;
  endedAt?: DateTimeOffset;
  lockerBankBehaviour: LockerBankBehaviour ;
  endedByMasterCard: boolean;
  cardCredential?: ICardCredentialDTO;
  cardHolder?: ICardHolderDTO;
  locker?: ILockerDTO;
  lock?: ILockDTO;
}
