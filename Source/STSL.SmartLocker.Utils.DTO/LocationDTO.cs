namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateLocationDTO(string Name, string? Description);
public sealed record LocationDTO(Guid Id, string Name, string Description, Guid? ReferenceImageId);
public sealed record UpdateLocationDTO(string Name, string? Description);
