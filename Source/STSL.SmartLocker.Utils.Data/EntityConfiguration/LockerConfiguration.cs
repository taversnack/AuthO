using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<Locker>
{
    public override void Configure(EntityTypeBuilder<Locker> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Label).HasMaxLength(256);
        builder.Property(x => x.ServiceTag).HasMaxLength(32);

        builder.HasOne(x => x.CurrentLease).WithOne().HasForeignKey<Locker>(x => x.CurrentLeaseId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.LeaseHistory).WithOne(x => x.Locker).HasForeignKey(x => x.LockerId);

        builder.HasIndex(x => new { x.TenantId, x.LockerBankId, x.Label }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.ServiceTag }).IsUnique();
    }
}
