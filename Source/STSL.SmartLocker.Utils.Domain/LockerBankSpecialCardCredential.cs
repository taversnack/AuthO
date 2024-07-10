using STSL.SmartLocker.Utils.Domain.Interfaces;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockerBankSpecialCardCredential : EntityBaseWithTenant, ILockerBankCardCredential
{
    public Guid LockerBankId { get; set; }
    public Guid CardCredentialId { get; set; }

    // navigation
    public LockerBank? LockerBank { get; set; }
    public CardCredential? CardCredential { get; set; }
}
