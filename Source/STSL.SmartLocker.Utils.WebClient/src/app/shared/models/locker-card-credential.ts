import { EntityId } from "./common";

export interface ICreateLockerCardCredentialDTO {
  lockerId?: EntityId;
  cardCredentialId: EntityId;
}
