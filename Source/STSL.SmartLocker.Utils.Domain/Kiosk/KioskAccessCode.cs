namespace STSL.SmartLocker.Utils.Domain.Kiosk;

public class KioskAccessCode : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public required string AccessCode { get; set; }
    public bool HasBeenUsed { get; set; } = false;
    public DateTime ExpiryDate { get; set; }
    public Guid CardHolderId { get; set; }
    public CardHolder? CardHolder { get; set; }
}