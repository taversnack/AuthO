import { EntityId } from "./common";

// TODO: [1] -DP Fill in these DTOs
export interface ICreateLockerBankAdminDTO {
  lockerBankId?: EntityId;
  cardHolderId: EntityId;
}

export interface ILockerBankAdminDTO {
  lockerBankId: EntityId;
  cardHolderId: EntityId;
}

export interface IUpdateLockerBankAdminDTO {
  lockerBankId?: EntityId;
  cardHolderId: EntityId;
}
