using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Data.ValueConverters;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<Lock>
{
    public override void Configure(EntityTypeBuilder<Lock> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.FirmwareVersion).HasMaxLength(256);

        builder.Property(x => x.SerialNumber).HasConversion<LockSerialConverter>();

        builder.HasIndex(x => x.SerialNumber).IsUnique();

        builder.HasOne(x => x.Locker).WithOne(x => x.Lock).OnDelete(DeleteBehavior.SetNull);
    }
}
