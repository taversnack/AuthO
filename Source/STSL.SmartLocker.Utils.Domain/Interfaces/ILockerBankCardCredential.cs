namespace STSL.SmartLocker.Utils.Domain.Interfaces;

public interface ILockerBankCardCredential
{
    Guid LockerBankId { get; set; }
    Guid CardCredentialId { get; set; }

    LockerBank? LockerBank { get; set; }
    CardCredential? CardCredential { get; set; }
}
