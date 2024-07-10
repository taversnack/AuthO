import { ICardCredentialDTO } from "./card-credential";
import { ICardHolderDTO } from "./card-holder";
import { EntityId } from './common';

export interface ICardCredentialAndCardHolderDTO {
  cardHolder?: ICardHolderDTO;
  cardCredential: ICardCredentialDTO;
}

export interface ICardHolderAndCardCredentialsDTO {
  cardHolder: ICardHolderDTO;
  cardCredentials?: ICardCredentialDTO[];
}

export interface IUpdateManyCardCredentialAndCardHoldersDTO {
  cardHolderIds?: EntityId[];
  cardCredentialIds?: EntityId[];
}
