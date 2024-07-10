using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerBankAdminConfiguration : EntityBaseConfiguration<LockerBankAdmin>
{
    public override void Configure(EntityTypeBuilder<LockerBankAdmin> builder)
    {
        base.Configure(builder);

        // Unique for combination of TenantId, LockerBankId & CardHolderId, no need for separate PK
        builder.HasKey(x => new { x.TenantId, x.LockerBankId, x.CardHolderId });
    }

}
