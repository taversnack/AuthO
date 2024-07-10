import { Moment } from "moment";

// TODO: Value Object Id, avoid primitive obsession & improve type safety / reduce bugs
// export interface IBaseEntity {
//   readonly id: EntityId<IBaseEntity>;
// }

// export type DefaultEntityIdType = string;

// export interface EntityId<Entity extends IBaseEntity, EntityIdType extends number | string = DefaultEntityIdType> {
//   readonly value: EntityIdType;
// }

// GUID
export type EntityId = string;

export type DateTime = Date | string | Moment;
export type DateTimeOffset = Date | string | Moment;

export interface IUsesGuidId {
  readonly id: EntityId;
}

export interface IHasReferenceImage {
  readonly referenceImageId?: EntityId;
}

export const NewlineCharacter = /\r\n|\n/;

export const DomainConstants = {
  MaxUserCardCredentialsPerLocker: 90,
  MaxSpecialCardCredentialsPerLocker: 10,
}
