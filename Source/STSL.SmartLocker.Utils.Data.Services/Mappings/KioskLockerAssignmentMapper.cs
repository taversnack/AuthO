using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain.Kiosk;
using STSL.SmartLocker.Utils.DTO.Kiosk;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class KioskLockerAssignmentMapper : IMapsToDTO<KioskLockerAssignentDTO, KioskLockerAssignment>
{
    public static KioskLockerAssignentDTO ToDTO(KioskLockerAssignment entity) => new
    (
        Id: entity.Id,
        LockerId: entity.LockerId,
        CardHolderId: entity.CardHolderId,
        TemporaryCardCredentialId: entity.TemporaryCardCredentialId,
        ReplacedCardCredentialId: entity.ReplacedCardCredentialId,
        AssignmentDate: entity.AssignmentDate,
        IsTemporaryCardActive: entity.IsTemporaryCardActive
    );
}