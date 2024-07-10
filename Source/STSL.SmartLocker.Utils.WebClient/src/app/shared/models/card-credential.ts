import { EntityId, IUsesGuidId } from "./common";

// NOTE: [0] Minor network optimization to avoid strings
// export enum CardType
// {
//   Unregistered = 0,
//   User = 1,
//   Welcome = 2,
//   Master = 3,
//   Security = 4,
//   Tag = 5,
// }

export enum CardType
{
  Unregistered = 'Unregistered',
  User = 'User',
  Welcome = 'Welcome',
  Master = 'Master',
  Security = 'Security',
  Tag = 'Tag',
}

export interface ICardCredentialDTO extends IUsesGuidId {
  // readonly id: EntityId; // from IUsesGuidId
  serialNumber?: string;
  hidNumber: string;
  cardType: CardType;
  cardLabel?: string;
  cardHolderId?: EntityId;
}

export interface ICreateCardCredentialDTO {
  serialNumber?: string;
  hidNumber: string;
  cardType: CardType;
  cardLabel?: string;
  cardHolderId?: EntityId;
}

export interface IUpdateCardCredentialDTO {
  serialNumber?: string;
  hidNumber: string;
  cardType: CardType;
  cardLabel?: string;
  cardHolderId?: EntityId;
}
