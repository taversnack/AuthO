using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerBankSpecialCardCredentialConfiguration : EntityBaseConfiguration<LockerBankSpecialCardCredential>
{
    public override void Configure(EntityTypeBuilder<LockerBankSpecialCardCredential> builder)
    {
        base.Configure(builder);

        // Unique for combination of TenantId, LockerBankId & CardCredentialId, no need for separate PK
        builder.HasKey(x => new { x.TenantId, x.LockerBankId, x.CardCredentialId });
    }
}
