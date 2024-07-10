import { ICreateCardCredentialDTO } from './card-credential';
import { ICreateCardHolderDTO } from './card-holder';
import { EntityId } from './common';

export interface ICreateCardHolderAndCardCredentialDTO
{
  cardHolder: ICreateCardHolderDTO;
  cardCredential: ICreateCardCredentialDTO;
}

export interface ICreateBulkCardHolderAndCardCredentialsDTO
{
  cardHolderAndCardCredentials: ICreateCardHolderAndCardCredentialDTO[];
}

export interface IMoveLockersToLockerBankDTO
{
  origin?: EntityId;
  destination: EntityId;
  lockerIds: EntityId[];
}
