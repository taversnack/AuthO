import { IUsesGuidId } from "./common";

export type Base64EncodedImage = string;

export interface ITenantDTO extends IUsesGuidId {
  name: string;
  logo?: Base64EncodedImage;
  logoMimeType?: string;
  cardHolderAliasSingular?: string;
  cardHolderAliasPlural?: string;
  cardHolderUniqueIdentifierAlias?: string;
  helpPortalUrl?: string;
  allowLockUpdates?: boolean;
}

export interface ICreateTenantDTO {
  name: string;
  logo?: Base64EncodedImage;
  logoMimeType?: string;
  cardHolderAliasSingular?: string;
  cardHolderAliasPlural?: string;
  cardHolderUniqueIdentifierAlias?: string;
  helpPortalUrl?: string;
}

export interface IUpdateTenantDTO {
  name: string;
  logo?: Base64EncodedImage;
  logoMimeType?: string;
  cardHolderAliasSingular?: string;
  cardHolderAliasPlural?: string;
  cardHolderUniqueIdentifierAlias?: string;
  helpPortalUrl?: string;
}
