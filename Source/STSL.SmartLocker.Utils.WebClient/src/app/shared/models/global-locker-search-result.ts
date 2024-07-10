import { IPagingResponse } from './api';
import { ILocationDTO } from './location';
import { ILockDTO } from './lock';
import { ILockerDTO } from './locker';
import { ILockerAndLockDTO } from './locker-and-lock';
import { ILockerBankDTO } from './locker-bank';

export interface IGlobalLockerSearchResultDTO {
  locations: ILocationDTO[];
  lockerBanks: ILockerBankDTO[];
  lockerAndLocks: IPagingResponse<ILockerAndLockDTO>;
}

export interface ILockerWithLocationLockerBankLock {
  location?: ILocationDTO;
  lockerBank?: ILockerBankDTO;
  locker: ILockerDTO;
  lock?: ILockDTO;
}
