namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockerBankAdmin : EntityBaseWithTenant
{
    public Guid LockerBankId { get; set; }
    public Guid CardHolderId { get; set; }

    // navigation
    public LockerBank? LockerBank { get; set; }
    // It may make sense in time to make Admin an entity type of it's own or to make a User table
    // and make Admin & CardHolder both reference a User.
    public CardHolder? CardHolder { get; set; }
}
