using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record UpdateLockerBankLockersDTO(Guid? LockerBankId, LockOperatingMode? LockOperatingMode);