import { IHasReferenceImage, IUsesGuidId } from "./common";

export interface ILocationDTO extends IUsesGuidId, IHasReferenceImage {
  // readonly id: EntityId; // from IUsesGuidId
  name: string;
  description: string;
}

export interface ICreateLocationDTO {
  name: string;
  description?: string;
}

export interface IUpdateLocationDTO {
  name: string;
  description?: string;
}
