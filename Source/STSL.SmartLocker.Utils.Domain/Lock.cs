using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class Lock : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public required LockSerial SerialNumber { get; init; }
    public DateTimeOffset InstallationDateUtc { get; set; } = DateTimeOffset.UtcNow;
    public string FirmwareVersion { get; set; } = string.Empty;
    public LockOperatingMode OperatingMode { get; set; } = LockOperatingMode.Installation;

    public Guid? LockerId { get; set; }

    // navigation
    public Locker? Locker { get; set; }
}

public sealed class StringifiedLockOperatingMode
{
    public required LockOperatingMode Value { get; init; }
    public required string Name { get; init; }
}