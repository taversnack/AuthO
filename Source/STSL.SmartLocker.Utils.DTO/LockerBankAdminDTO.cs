namespace STSL.SmartLocker.Utils.DTO;

public sealed record CreateLockerBankAdminDTO(Guid LockerBankId, Guid CardHolderId);
public sealed record LockerBankAdminDTO(Guid LockerBankId, Guid CardHolderId);
public sealed record UpdateLockerBankAdminDTO(Guid LockerBankId, Guid CardHolderId);

