using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerLeaseConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<LockerLease>
{
    public override void Configure(EntityTypeBuilder<LockerLease> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.CardCredential).WithMany().OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CardHolder).WithMany(x => x.LockerLeaseHistory).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Locker).WithMany(x => x.LeaseHistory).HasForeignKey(x => x.LockerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Lock).WithMany().OnDelete(DeleteBehavior.SetNull);
    }
}
