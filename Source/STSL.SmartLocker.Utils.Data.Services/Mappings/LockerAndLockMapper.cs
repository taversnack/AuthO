using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockerAndLockMapper : IMapsToDTO<LockerAndLockDTO, Locker> //, IMapsToDTO<LockerAndLockDTO, Lock>
{
    public static LockerAndLockDTO ToDTO(Locker entity) => new()
    {
        Locker = LockerMapper.ToDTO(entity),
        Lock = entity.Lock == null ? null : LockMapper.ToDTO(entity.Lock),
    };

    //public static LockerAndLockDTO ToDTO(Lock entity) => new()
    //{
    //    Locker = LockerMapper.ToDTO(entity.Locker ?? throw new Exception("Tried to map the LockerAndLockDTO but failed as lock does not have an associated Locker")),
    //    Lock = LockMapper.ToDTO(entity)
    //};
}
