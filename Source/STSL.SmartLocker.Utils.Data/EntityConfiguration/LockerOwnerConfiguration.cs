using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerOwnerConfiguration : EntityBaseConfiguration<LockerOwner>
{
    public override void Configure(EntityTypeBuilder<LockerOwner> builder)
    {
        base.Configure(builder);

        // Unique for combination of TenantId, LockerId & CardHolderId, no need for separate PK
        builder.HasKey(x => new { x.TenantId, x.LockerId, x.CardHolderId });
    }
}
