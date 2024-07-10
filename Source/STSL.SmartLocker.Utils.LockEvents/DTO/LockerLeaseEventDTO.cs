using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.LockEvents.DTO;

public sealed record LockerLeasedEventDTO(LockSerial LockSerial, string CardSerial, DateTimeOffset ServerTimeStamp, DateTimeOffset? LockTimeStamp);

public sealed record LockerLeaseEndedEventDTO(LockSerial LockSerial, string CardSerial, DateTimeOffset ServerTimeStamp, DateTimeOffset? LockTimeStamp);
public sealed record LockerLeaseEndedByBrokenOpenEventDTO(LockSerial LockSerial, DateTimeOffset ServerTimeStamp, DateTimeOffset? LockTimeStamp);
public sealed record LockerLeaseEndedByMasterCardEventDTO(LockSerial LockSerial, string LeaseCardSerial, string MasterCardSerial, DateTimeOffset ServerTimeStamp, DateTimeOffset? LockTimeStamp);