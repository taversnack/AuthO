using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockerBankAdminMapper : IMapsToDTO<LockerBankAdminDTO, LockerBankAdmin>
{
    public static LockerBankAdminDTO ToDTO(LockerBankAdmin entity)
        => new(LockerBankId: entity.LockerBankId, CardHolderId: entity.CardHolderId);
}

internal sealed class CreateLockerBankAdminMapper : IMapsToEntity<CreateLockerBankAdminDTO, LockerBankAdmin>
{
    public static LockerBankAdmin ToEntity(CreateLockerBankAdminDTO dto)
        => new() { LockerBankId = dto.LockerBankId, CardHolderId = dto.CardHolderId };
}

internal sealed class UpdateLockerBankAdminMapper : IMapsToEntity<UpdateLockerBankAdminDTO, LockerBankAdmin>
{
    public static LockerBankAdmin ToEntity(UpdateLockerBankAdminDTO dto)
        => new() { LockerBankId = dto.LockerBankId, CardHolderId = dto.CardHolderId };
}