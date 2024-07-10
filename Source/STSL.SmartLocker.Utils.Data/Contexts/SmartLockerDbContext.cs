using Microsoft.EntityFrameworkCore;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.Domain.Kiosk;

namespace STSL.SmartLocker.Utils.Data.Contexts;

public class SmartLockerDbContext : DbContext
{
    // "slk" is short for "smart locker"
    public static readonly string DefaultSchema = "slk";

    public SmartLockerDbContext() { }

    public SmartLockerDbContext(DbContextOptions<SmartLockerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Note: currently the db context assumes a relational database with schemas
        // this line could be moved out into implementation specific project
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartLockerDbContext).Assembly);
    }

    #region Auditing Overrides
    /*
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.UpdateAuditableEntitiesOnSave();
        return await base.SaveChangesAsync(cancellationToken);
    }
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.UpdateAuditableEntitiesOnSave();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges()
    {
        ChangeTracker.UpdateAuditableEntitiesOnSave();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.UpdateAuditableEntitiesOnSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    */
    #endregion Auditing Overrides

    #region Entity DbSets

    // Primary entities
    public virtual DbSet<Tenant> Tenants { get => Set<Tenant>(); }
    public virtual DbSet<Location> Locations { get => Set<Location>(); }
    public virtual DbSet<LockerBank> LockerBanks { get => Set<LockerBank>(); }
    public virtual DbSet<Locker> Lockers { get => Set<Locker>(); }
    public virtual DbSet<Lock> Locks { get => Set<Lock>(); }
    public virtual DbSet<CardHolder> CardHolders { get => Set<CardHolder>(); }
    public virtual DbSet<CardCredential> CardCredentials { get => Set<CardCredential>(); }
    public virtual DbSet<LockerLease> LockerLeases { get => Set<LockerLease>(); }
    public virtual DbSet<KioskAccessCode> KioskAccessCodes { get => Set<KioskAccessCode>(); }
    public virtual DbSet<KioskLockerAssignment> KioskLockerAssignments { get => Set<KioskLockerAssignment>(); }
    public virtual DbSet<Kiosks> Kiosks { get => Set<Kiosks>(); }
    public virtual DbSet<LockConfigEventAudit> LockConfigEventAudits { get => Set<LockConfigEventAudit>(); }

    // Association entities
    public virtual DbSet<LockerCardCredential> LockerCardCredentials { get => Set<LockerCardCredential>(); }
    public virtual DbSet<LockerBankUserCardCredential> LockerBankUserCardCredentials { get => Set<LockerBankUserCardCredential>(); }
    public virtual DbSet<LockerBankSpecialCardCredential> LockerBankSpecialCardCredentials { get => Set<LockerBankSpecialCardCredential>(); }
    public virtual DbSet<LockerBankLeaseUser> LockerBankLeaseUsers { get => Set<LockerBankLeaseUser>(); }
    public virtual DbSet<LockerOwner> LockerOwners { get => Set<LockerOwner>(); }
    public virtual DbSet<LockerBankAdmin> LockerBankAdmins { get => Set<LockerBankAdmin>(); }
    

    #endregion Entity DbSets
}