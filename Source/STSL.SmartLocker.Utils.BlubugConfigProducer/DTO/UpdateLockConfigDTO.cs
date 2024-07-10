using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;

public sealed class UpdateLockConfigDTO
{
    public List<LockConfigCardDTO>? Cards { get; set; }
    public bool? SecuritySweepIsActive { get; set; }
    public LockOperatingMode? OperatingMode { get; set; }

    public bool ReplaceAllCardBlocks { get; set; } = true;
    public LockConfigUpdateProperties UpdateProperties { get; set; } = LockConfigUpdateProperties.All;
}

// TODO: Eventually only update properties that need to be updated to save resources.
// Would be good to find out from Wireless Measurements what if any detriment there is of sending
// unneeded changes. Does this make it to the lock and reduce battery longevity? Is there some diffing
// that prevents this at the level of blubug, gateway etc. Would be useful to understand this in more detail
public enum LockConfigUpdateProperties
{
    Default = 0,
    OperatingMode = 1 << 0,
    SecuritySweep = 1 << 1,
    Cards = 1 << 2,

    CardsAndOperatingMode = Cards | OperatingMode,

    All = int.MaxValue,
}