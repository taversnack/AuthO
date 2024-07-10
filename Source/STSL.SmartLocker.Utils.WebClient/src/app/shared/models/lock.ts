import { DateTimeOffset, EntityId, IUsesGuidId } from "./common";

export type LockSerial = number;

// NOTE: [0] Minor network optimization to avoid strings
// export enum LockOperatingMode {
//   Installation = 0,
//   Shared = 1,
//   Dedicated = 2,
//   Confiscated = 3,
//   Reader = 4,
// }

export enum LockOperatingMode {
  Installation = 'Installation',
  Shared = 'Shared',
  Dedicated = 'Dedicated',
  Confiscated = 'Confiscated',
  Reader = 'Reader',
}

export interface ILockDTO extends IUsesGuidId {
  // readonly id: EntityId; // from IUsesGuidId
  serialNumber: LockSerial;
  installationDateUtc: DateTimeOffset;
  firmwareVersion: string;
  operatingMode: LockOperatingMode;
  lockerId?: EntityId;
}

export interface ICreateLockDTO {
  serialNumber: LockSerial;
  installationDateUtc?: DateTimeOffset;
  firmwareVersion?: string;
  operatingMode?: LockOperatingMode;
  lockerId?: EntityId;
}

export interface IUpdateLockDTO {
  serialNumber: LockSerial;
  installationDateUtc?: DateTimeOffset;
  firmwareVersion?: string;
  operatingMode?: LockOperatingMode;
  lockerId?: EntityId;
}

export interface ILock {
  owner: string;
  batteryLevel: number;
  lastAccess: Date;
  expiry: Date;
}
