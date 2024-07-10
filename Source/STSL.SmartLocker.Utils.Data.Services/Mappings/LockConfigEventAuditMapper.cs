using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockConfigEventAuditMapper : IMapsToDTO<LockConfigEventAuditDTO, LockConfigEventAudit>
{
    public static LockConfigEventAuditDTO ToDTO(LockConfigEventAudit entity) => new
    (
        Id: entity.Id,
        EventType: entity.EventType,
        UpdatedByUserEmail: entity.UpdatedByUserEmail,
        EntityId: entity.EntityId,
        CreatedAtUTC: entity.CreatedAtUTC,
        AdditionalDescription: entity.AdditionalDescription
    );
}

internal sealed class CreateLockConfigEventAuditMapper : IMapsToEntity<CreateLockConfigEventAuditDTO, LockConfigEventAudit>
{
    public static LockConfigEventAudit ToEntity(CreateLockConfigEventAuditDTO dto)
    {
        var lockConfigEventAudit = new LockConfigEventAudit
        {
            EventType = dto.EventType,
            UpdatedByUserEmail = dto.UpdatedByUserEmail,
            EntityId = dto.EntityId,
            AdditionalDescription = dto.AdditionalDescription,
        };

        lockConfigEventAudit.CreatedAtUTC = dto.CreatedAtUTC ?? lockConfigEventAudit.CreatedAtUTC;

        return lockConfigEventAudit;
    }
}