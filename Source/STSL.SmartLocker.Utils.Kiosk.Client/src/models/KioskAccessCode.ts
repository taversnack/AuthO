import type { EntityId, IUsesGuidId } from "./Common";

export interface IKioskAccessCode extends IUsesGuidId {
    accessCode: string
    hasBeenUsed: boolean
    expiryDate: Date
    cardHolderId: EntityId
}


export interface IAccessCode {
    code: string
}