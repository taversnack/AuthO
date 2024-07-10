using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerCardCredentialConfiguration : EntityBaseConfiguration<LockerCardCredential>
{
    public override void Configure(EntityTypeBuilder<LockerCardCredential> builder)
    {
        base.Configure(builder);

        // Unique for combination of TenantId, LockerId & CardCredentialId, no need for separate PK
        builder.HasKey(x => new { x.TenantId, x.LockerId, x.CardCredentialId });
    }
}
