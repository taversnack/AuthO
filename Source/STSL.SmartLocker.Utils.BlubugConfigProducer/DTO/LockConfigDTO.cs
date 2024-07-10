using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;

public readonly struct LockConfigDTO
{
    public readonly LockConfigSnapshotDTO Value { get; init; }
    public readonly LockConfigSnapshotDTO? Pending { get; init; }
}

public sealed class LockConfigSnapshotDTO
{
    public LockOperatingMode? LockOperatingMode { get; set; }
    public int? LockSecuritySweep { get; set; }
    public List<LockConfigCardDTO>? Cards { get; set; }
}