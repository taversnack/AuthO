namespace STSL.SmartLocker.Utils.Domain.Kiosk;

public sealed class KioskLockerAssignment : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }

    public Guid? LockerId { get; set; }

    public Guid? CardHolderId { get; set; }

    public Guid? TemporaryCardCredentialId { get; set; }

    public Guid? ReplacedCardCredentialId { get; set; }

    public DateTimeOffset AssignmentDate { get; set; }

    public bool IsTemporaryCardActive { get; set; } = false;

    // navigation
    public CardHolder? CardHolder { get; set; }
    public Locker? Locker { get; set; }

    public CardCredential? ReplacedCardCredential { get; set; }
}