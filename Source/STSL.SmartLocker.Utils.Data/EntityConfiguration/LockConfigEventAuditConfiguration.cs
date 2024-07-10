using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockConfigEventAuditConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<LockConfigEventAudit>
{
    public override void Configure(EntityTypeBuilder<LockConfigEventAudit> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.UpdatedByUserEmail).HasMaxLength(256);
    }
}