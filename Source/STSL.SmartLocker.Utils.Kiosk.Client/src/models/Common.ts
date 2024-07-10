// GUID
export type EntityId = string

export interface IUsesGuidId {
  readonly id: EntityId
}

export enum CardType
{
  Temporary = 'Temporary'
}
