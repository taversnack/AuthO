using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockerMapper : IMapsToDTO<LockerDTO, Locker>
{
    public static LockerDTO ToDTO(Locker entity) => new
    (
        Id: entity.Id,
        LockerBankId: entity.LockerBankId,
        Label: entity.Label,
        ServiceTag: entity.ServiceTag ?? string.Empty,
        SecurityType: entity.SecurityType,
        AbsoluteLeaseExpiry: entity.AbsoluteLeaseExpiry,
        CurrentLeaseHolder: entity.CurrentLease?.CardHolder is null ? null : CardHolderMapper.ToDTO(entity.CurrentLease.CardHolder)

    // In case of use with CardCredentialAndCardHolderDTO
    //CurrentLease: entity.CurrentLease?.CardCredential is null ? null : new()
    //{
    //    CardHolder = entity.CurrentLease.CardHolder is null ? null : CardHolderMapper.ToDTO(entity.CurrentLease.CardHolder),
    //    CardCredential = CardCredentialMapper.ToDTO(entity.CurrentLease.CardCredential)
    //}
    );
}

internal sealed class CreateLockerMapper : IMapsToEntity<CreateLockerDTO, Locker>
{
    public static Locker ToEntity(CreateLockerDTO dto) => new()
    {
        LockerBankId = dto.LockerBankId,
        Label = dto.Label,
        ServiceTag = dto.ServiceTag,
        SecurityType = dto.SecurityType,
    };
}

internal sealed class UpdateLockerMapper : IMapsToEntity<UpdateLockerDTO, Locker>, IMapsToEntity<UpdateLockerWithIdDTO, Locker>
{
    public static Locker ToEntity(UpdateLockerDTO dto) => new()
    {
        LockerBankId = dto.LockerBankId,
        Label = dto.Label,
        ServiceTag = dto.ServiceTag,
        SecurityType = dto.SecurityType,
    };

    public static Locker ToEntity(UpdateLockerWithIdDTO dto) => new()
    {
        //Id = dto.Id,
        LockerBankId = dto.LockerBankId,
        Label = dto.Label,
        ServiceTag = dto.ServiceTag,
        SecurityType = dto.SecurityType,
    };
}
