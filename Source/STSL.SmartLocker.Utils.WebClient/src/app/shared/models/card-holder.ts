import { IUsesGuidId } from "./common";

export const defaultCardHolderAliasSingular = 'Card Holder';
export const defaultCardHolderAliasPlural = 'Card Holders';
export const defaultUniqueIdentifierAlias = 'Unique Identifier';

export interface ICardHolderDTO extends IUsesGuidId {
  // readonly id: EntityId; // from IUsesGuidId
  firstName: string;
  lastName: string;
  email?: string;
  uniqueIdentifier?: string;
  isVerified: boolean;
}

export interface ICreateCardHolderDTO {
  firstName: string;
  lastName: string;
  email?: string;
  uniqueIdentifier?: string;
  isVerified: boolean;
}

export interface IUpdateCardHolderDTO {
  firstName: string;
  lastName: string;
  email?: string;
  uniqueIdentifier?: string;
  isVerified: boolean;
}
