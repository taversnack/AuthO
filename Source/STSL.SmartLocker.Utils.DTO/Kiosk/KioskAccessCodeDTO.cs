namespace STSL.SmartLocker.Utils.DTO.Kiosk;

public sealed record KioskAccessCodeDTO(Guid Id, string AccessCode, bool HasBeenUsed, DateTime ExpiryDate, Guid? CardHolderId);

public sealed record CreateKioskAccessCodeDTO(string Email, DateTime RequestDateTime);

public sealed record AccessCodeDTO(string Code);
