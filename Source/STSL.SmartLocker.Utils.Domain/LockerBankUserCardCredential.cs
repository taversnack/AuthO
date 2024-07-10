using STSL.SmartLocker.Utils.Domain.Interfaces;

namespace STSL.SmartLocker.Utils.Domain;

/// <summary>
/// For use with shift locker banks & 'biscuit cupboard' locker banks
/// </summary>
public sealed class LockerBankUserCardCredential : EntityBaseWithTenant, ILockerBankCardCredential
{
    public Guid LockerBankId { get; set; }
    public Guid CardCredentialId { get; set; }

    // navigation
    public LockerBank? LockerBank { get; set; }
    public CardCredential? CardCredential { get; set; }
}
