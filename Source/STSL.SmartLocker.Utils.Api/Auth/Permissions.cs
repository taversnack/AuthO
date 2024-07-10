namespace STSL.SmartLocker.Utils.Api.Auth;

internal static class Roles
{
    public const string SuperUser = "super-user";
    public const string Installer = "installer";
    public const string LockerBankAdmin = "locker-bank-admin";
    public const string TenantAdmin = "tenant-admin";
}

internal static class Permissions
{
    public static IEnumerable<string> Policies => new[] {
        Locations,
        LockerBanks,
        Lockers,
        Locks,
        CardHolders,
        CardCredentials,
        LockerBankAdmins,
        LeaseUsers,
    }.SelectMany(x => new[] { Read + x, Maintain + x })
    .Concat(new[] {
        Create + Tenants,
        Read + Tenants,
        Update + Tenants,
        Delete + Tenants,
    });

    // Access types
    private const string Read = "read:";
    private const string Maintain = "maintain:";

    private const string Create = "create:";
    private const string Update = "update:";
    private const string Delete = "delete:";

    // Entity types
    private const string Tenants = "tenants";
    private const string Locations = "locations";
    private const string LockerBanks = "locker-banks";
    private const string LockerBankAdmins = "admins";
    private const string LeaseUsers = "lease-users";
    private const string Lockers = "lockers";
    private const string Locks = "lockers";
    private const string CardHolders = "card-holders";
    private const string CardCredentials = "card-credentials";

    // Fine grained permission policies
    public const string CreateTenants = Create + Tenants;
    public const string ReadTenants = Read + Tenants;
    public const string UpdateTenants = Update + Tenants;
    public const string DeleteTenants = Delete + Tenants;

    public const string CreateLocations = Maintain + Locations;
    public const string ReadLocations = Read + Locations;
    public const string UpdateLocations = Maintain + Locations;
    public const string DeleteLocations = Maintain + Locations;

    public const string CreateLockerBanks = Maintain + LockerBanks;
    public const string ReadLockerBanks = Read + LockerBanks;
    public const string UpdateLockerBanks = Maintain + LockerBanks;
    public const string DeleteLockerBanks = Maintain + LockerBanks;

    public const string CreateLockerBankAdmins = Maintain + LockerBankAdmins;
    public const string ReadLockerBankAdmins = Read + LockerBankAdmins;
    public const string UpdateLockerBankAdmins = Maintain + LockerBankAdmins;
    public const string DeleteLockerBankAdmins = Maintain + LockerBankAdmins;

    public const string CreateLeaseUsers = Maintain + LeaseUsers;
    public const string ReadLeaseUsers = Read + LeaseUsers;
    public const string UpdateLeaseUsers = Maintain + LeaseUsers;
    public const string DeleteLeaseUsers = Maintain + LeaseUsers;

    public const string CreateLockers = Maintain + Lockers;
    public const string ReadLockers = Read + Lockers;
    public const string UpdateLockers = Maintain + Lockers;
    public const string DeleteLockers = Maintain + Lockers;

    public const string CreateLocks = Maintain + Locks;
    public const string ReadLocks = Read + Locks;
    public const string UpdateLocks = Maintain + Locks;
    public const string DeleteLocks = Maintain + Locks;

    public const string CreateCardHolders = Maintain + CardHolders;
    public const string ReadCardHolders = Read + CardHolders;
    public const string UpdateCardHolders = Maintain + CardHolders;
    public const string DeleteCardHolders = Maintain + CardHolders;

    public const string CreateCardCredentials = Maintain + CardCredentials;
    public const string ReadCardCredentials = Read + CardCredentials;
    public const string UpdateCardCredentials = Maintain + CardCredentials;
    public const string DeleteCardCredentials = Maintain + CardCredentials;
}