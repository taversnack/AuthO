import { ICreateLockDTO, ILockDTO } from "./lock";
import { ICreateLockerDTO, ILockerDTO } from "./locker";

export interface ILockerAndLockDTO {
  locker: ILockerDTO;
  lock?: ILockDTO;
}

export interface ICreateLockerAndLockDTO {
  locker: ICreateLockerDTO;
  lock: ICreateLockDTO;
}
