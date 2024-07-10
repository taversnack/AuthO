using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class LockerBank : EntityBaseWithTenant, IUsesGuidId, IHasReferenceImage<LockerBankReferenceImage>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public LockerBankBehaviour Behaviour { get; set; }
    public TimeSpan? DefaultLeaseDuration { get; set; }
    //public bool OnlyContainsSmartLocks { get; set; }
    public Guid? CurrentReferenceImageId { get; set; }
    public Guid LocationId { get; set; }

    // navigation
    public Location? Location { get; set; }
    public List<Locker> Lockers { get; } = new();
    public LockerBankReferenceImage? CurrentReferenceImage { get; set; }
    public List<LockerBankReferenceImage>? ReferenceImages { get; set; }

    /// <summary>
    /// Used to keep a track of the actual users who the UserCardCredentials belong to.
    /// These are many to many CardHolders who are assigned to a bank of shift lockers.
    /// </summary>
    public List<LockerBankLeaseUser> LeaseUsers { get; } = new();
    public List<LockerBankUserCardCredential> UserCardCredentials { get; } = new();
    /// <summary>
    /// A separate list of card credentials that should only be master, security sweep & welcome cards.
    /// Allows the LeaseUsers / UserCardCredentials to be managed without affecting the special cards.
    /// </summary>
    public List<LockerBankSpecialCardCredential> SpecialCardCredentials { get; } = new();
    public List<LockerBankAdmin> Admins { get; } = new();
}

public sealed class StringifiedLockerBankBehaviour
{
    public required LockerBankBehaviour Value { get; init; }
    public required string Name { get; init; }
}