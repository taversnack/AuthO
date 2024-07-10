namespace STSL.SmartLocker.Utils.Domain;

public sealed class CardHolder : EntityBaseWithTenant, IUsesGuidId
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? UniqueIdentifier { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsTerminated { get; set; } = false;
    public DateTime? TerminationDate { get; set; }

    // navigation
    public List<CardCredential>? CardCredentials { get; set; }
    public List<Locker>? CurrentlyLeasedLockers { get; set; }
    public List<LockerLease>? LockerLeaseHistory { get; set; }
}