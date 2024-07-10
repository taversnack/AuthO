using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerBankUserCardCredentialConfiguration : EntityBaseConfiguration<LockerBankUserCardCredential>
{
    public override void Configure(EntityTypeBuilder<LockerBankUserCardCredential> builder)
    {
        base.Configure(builder);

        // Unique for combination of TenantId, LockerBankId & CardCredentialId, no need for separate PK
        builder.HasKey(x => new { x.TenantId, x.LockerBankId, x.CardCredentialId });
    }
}
