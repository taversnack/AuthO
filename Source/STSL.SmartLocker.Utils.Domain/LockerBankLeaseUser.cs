namespace STSL.SmartLocker.Utils.Domain;

// TODO: Maybe rename to LeaseHolder ? minor point though
/// <summary>
/// For use with temporary locker banks. 
/// </summary>
public sealed class LockerBankLeaseUser : EntityBaseWithTenant
{
    public Guid LockerBankId { get; set; }
    public Guid CardHolderId { get; set; }

    // navigation
    public LockerBank? LockerBank { get; set; }
    public CardHolder? CardHolder { get; set; }
}