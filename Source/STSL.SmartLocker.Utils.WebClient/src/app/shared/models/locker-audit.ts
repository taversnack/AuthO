import { DateTime } from "./common";
import { LockSerial } from "./lock";

export interface ILockerAuditDTO
{
  rowNum: number;
  auditCategory: string;
  auditDescription: string;
  auditTime: DateTime;
  subject: string;
  object: string;
  lockSerialNumber: LockSerial;
  auditType: number;
  subjectSN: string;
  objectSN: string;
}
