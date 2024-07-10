using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateLockerBankDTO(Guid LocationId, string Name, string? Description, LockerBankBehaviour Behaviour, TimeSpan? DefaultLeaseDuration);
public sealed record LockerBankDTO(Guid Id, Guid LocationId, string Name, string Description, LockerBankBehaviour Behaviour, /*bool OnlyContainsSmartLocks,*/ TimeSpan? DefaultLeaseDuration, Guid? ReferenceImageId);
public sealed record UpdateLockerBankDTO(Guid LocationId, string Name, string? Description, LockerBankBehaviour Behaviour, TimeSpan? DefaultLeaseDuration);
