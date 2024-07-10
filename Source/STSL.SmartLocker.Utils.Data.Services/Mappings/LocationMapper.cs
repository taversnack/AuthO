using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LocationMapper : IMapsToDTO<LocationDTO, Location>
{
    public static LocationDTO ToDTO(Location entity) => new
    (
        Id: entity.Id,
        Name: entity.Name,
        Description: entity.Description,
        ReferenceImageId: entity.CurrentReferenceImageId
    );
}

internal sealed class CreateLocationMapper : IMapsToEntity<CreateLocationDTO, Location>
{
    public static Location ToEntity(CreateLocationDTO dto)
    {
        var location = new Location
        {
            Name = dto.Name,
        };

        location.Description = dto.Description ?? location.Description;

        return location;
    }
}

internal sealed class UpdateLocationMapper : IMapsToUpdatedEntity<UpdateLocationDTO, Location>
{
    public static Location ToEntity(UpdateLocationDTO dto, Location entity)
    {
        var location = new Location
        {
            Name = dto.Name,
            CurrentReferenceImageId = entity.CurrentReferenceImageId
        };

        location.Description = dto.Description ?? location.Description;

        return location;
    }
}
