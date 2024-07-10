namespace STSL.SmartLocker.Utils.DTO.Kiosk;

public sealed record KioskLockerAssignentDTO(Guid Id, Guid? LockerId, Guid? CardHolderId, Guid? TemporaryCardCredentialId, Guid? ReplacedCardCredentialId,  DateTimeOffset AssignmentDate, bool IsTemporaryCardActive);