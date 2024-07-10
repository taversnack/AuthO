using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateLockerDTO(Guid LockerBankId, string Label, string? ServiceTag, LockerSecurityType SecurityType);
public sealed record LockerDTO(Guid Id, Guid LockerBankId, string Label, string? ServiceTag, LockerSecurityType SecurityType, DateTimeOffset? AbsoluteLeaseExpiry, CardHolderDTO? CurrentLeaseHolder = null);
public sealed record UpdateLockerDTO(Guid LockerBankId, string Label, string? ServiceTag, LockerSecurityType SecurityType, DateTimeOffset? AbsoluteLeaseExpiry);


public sealed record UpdateLockerWithIdDTO(Guid Id, Guid LockerBankId, string Label, string ServiceTag, LockerSecurityType SecurityType, DateTimeOffset? AbsoluteLeaseExpiry);

public readonly record struct UpdateManyLockersDTO(List<UpdateLockerWithIdDTO> Lockers);
