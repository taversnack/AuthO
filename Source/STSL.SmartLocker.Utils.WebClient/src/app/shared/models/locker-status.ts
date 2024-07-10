import { DateTime, EntityId } from "./common";
import { LockOperatingMode } from "./lock";
import { LockerSecurityType } from './locker';

export interface ILockerStatusDTO
{
  lockerId: EntityId;
  lockId?: EntityId;
  label: string;
  serviceTag: string;
  securityType: LockerSecurityType;
  assignedTo?: string;
  assignedToCardHolderId?: EntityId;
  assignedToUniqueIdentifier?: string;
  assignedToManyCount?: number;
  lockSerialNumber?: number;
  lockFirmwareVersion?: string;
  lockOperatingMode?: LockOperatingMode;
  batteryVoltage?: number;
  lastAudit?: number;
  lastAuditCategory?: string;
  lastAuditDescription?: string;
  lastAuditTime?: DateTime;
  lastAuditSubjectId?: EntityId;
  lastAuditObjectId?: EntityId;
  lastAuditSubjectUniqueIdentifier?: string;
  lastAuditObjectUniqueIdentifier?: string;
  lastAuditSubject?: string;
  lastAuditObject?: string;
  lastAuditSubjectSN?: string;
  lastAuditObjectSN?: string;
  lastCommunication?: DateTime;
  boundaryAddress?: number;
}
