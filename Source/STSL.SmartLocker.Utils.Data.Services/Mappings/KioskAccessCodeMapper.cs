using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain.Kiosk;
using STSL.SmartLocker.Utils.DTO.Kiosk;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class KioskAccessCodeMapper : IMapsToDTO<KioskAccessCodeDTO, KioskAccessCode>
{
    public static KioskAccessCodeDTO ToDTO(KioskAccessCode entity) => new
    (
        Id: entity.Id,
        AccessCode: entity.AccessCode,
        HasBeenUsed: entity.HasBeenUsed,
        ExpiryDate: entity.ExpiryDate,
        CardHolderId: entity.CardHolderId
    );
}
