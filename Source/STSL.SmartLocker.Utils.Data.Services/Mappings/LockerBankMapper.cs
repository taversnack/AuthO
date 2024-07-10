using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockerBankMapper : IMapsToDTO<LockerBankDTO, LockerBank>
{
    public static LockerBankDTO ToDTO(LockerBank entity) => new
    (
        Id: entity.Id,
        LocationId: entity.LocationId,
        Name: entity.Name,
        Description: entity.Description,
        Behaviour: entity.Behaviour,
        //OnlyContainsSmartLocks: entity.OnlyContainsSmartLocks,
        DefaultLeaseDuration: entity.DefaultLeaseDuration,
        ReferenceImageId: entity.CurrentReferenceImageId
    );
}

internal sealed class CreateLockerBankMapper : IMapsToEntity<CreateLockerBankDTO, LockerBank>
{
    public static LockerBank ToEntity(CreateLockerBankDTO dto)
    {
        var lockerBank = new LockerBank
        {
            LocationId = dto.LocationId,
            Name = dto.Name,
            Behaviour = dto.Behaviour,
        };

        lockerBank.Description = dto.Description ?? lockerBank.Description;
        lockerBank.DefaultLeaseDuration = dto.DefaultLeaseDuration ?? lockerBank.DefaultLeaseDuration;

        return lockerBank;
    }
}

internal sealed class UpdateLockerBankMapper : IMapsToUpdatedEntity<UpdateLockerBankDTO, LockerBank>
{
    public static LockerBank ToEntity(UpdateLockerBankDTO dto, LockerBank entity)
    {
        var lockerBank = new LockerBank
        {
            LocationId = dto.LocationId,
            Name = dto.Name,
            Behaviour = dto.Behaviour,
            CurrentReferenceImageId = entity.CurrentReferenceImageId
        };

        lockerBank.Description = dto.Description ?? lockerBank.Description;
        lockerBank.DefaultLeaseDuration = dto.DefaultLeaseDuration ?? lockerBank.DefaultLeaseDuration;

        return lockerBank;
    }
}
