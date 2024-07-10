using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateLockDTO(
    int SerialNumber,
    DateTimeOffset? InstallationDateUtc = null,
    string? FirmwareVersion = null,
    LockOperatingMode? OperatingMode = null,
    Guid? LockerId = null,
    bool OverrideExistingLockerLockPair = false);

public sealed record LockDTO(
    Guid Id,
    int SerialNumber,
    DateTimeOffset InstallationDateUtc,
    string FirmwareVersion,
    LockOperatingMode OperatingMode,
    Guid? LockerId = null);
public sealed record UpdateLockDTO(
    int SerialNumber,
    DateTimeOffset? InstallationDateUtc = null,
    string? FirmwareVersion = null,
    LockOperatingMode? OperatingMode = null,
    Guid? LockerId = null,
    bool OverrideExistingLockerLockPair = false);
