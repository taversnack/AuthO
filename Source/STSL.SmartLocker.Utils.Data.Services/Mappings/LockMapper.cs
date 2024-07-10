using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockMapper : IMapsToDTO<LockDTO, Lock>
{
    public static LockDTO ToDTO(Lock entity) => new
    (
        Id: entity.Id,
        SerialNumber: entity.SerialNumber,
        InstallationDateUtc: entity.InstallationDateUtc,
        FirmwareVersion: entity.FirmwareVersion,
        OperatingMode: entity.OperatingMode,
        LockerId: entity.LockerId
    );
}

internal sealed class CreateLockMapper : IMapsToEntity<CreateLockDTO, Lock>
{
    public static Lock ToEntity(CreateLockDTO dto)
    {
        var newLock = new Lock
        {
            SerialNumber = new(dto.SerialNumber),
        };

        newLock.InstallationDateUtc = dto.InstallationDateUtc ?? newLock.InstallationDateUtc;
        newLock.FirmwareVersion = dto.FirmwareVersion ?? newLock.FirmwareVersion;
        newLock.OperatingMode = dto.OperatingMode ?? newLock.OperatingMode;
        newLock.LockerId = dto.LockerId ?? newLock.LockerId;

        return newLock;
    }
}

internal sealed class UpdateLockMapper : IMapsToEntity<UpdateLockDTO, Lock>
{
    public static Lock ToEntity(UpdateLockDTO dto)
    {
        var newLock = new Lock
        {
            SerialNumber = new(dto.SerialNumber),
        };

        newLock.InstallationDateUtc = dto.InstallationDateUtc ?? newLock.InstallationDateUtc;
        newLock.FirmwareVersion = dto.FirmwareVersion ?? newLock.FirmwareVersion;
        newLock.OperatingMode = dto.OperatingMode ?? newLock.OperatingMode;
        newLock.LockerId = dto.LockerId ?? newLock.LockerId;

        return newLock;
    }
}
