namespace STSL.SmartLocker.Utils.DTO;

public readonly struct LockerAndLockDTO
{
    public LockerDTO Locker { get; init; }
    public LockDTO? Lock { get; init; }
}
