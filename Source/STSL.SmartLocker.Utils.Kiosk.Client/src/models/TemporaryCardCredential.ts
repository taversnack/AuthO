import type { CardType, EntityId } from "./Common"

export interface ICreateTemporaryCardCredential {
    serialNumber: string
    hidNumber: string
    cardType: CardType.Temporary  // make readonly and not needed to be intialized
    cardHolderId: EntityId
    cardLabel: string
}

export interface IReturnTemporaryCardCredential {
    serialNumber: string
    hidNumber: string
    cardType: CardType.Temporary  // make readonly and not needed to be intialized
    cardHolderId?: EntityId
    cardLabel: string
}