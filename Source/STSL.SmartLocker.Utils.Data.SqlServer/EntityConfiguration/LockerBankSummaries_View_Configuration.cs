using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Data.SqlServer.Views;

namespace STSL.SmartLocker.Utils.Data.SqlServer.EntityConfiguration;

internal sealed class LockerBankSummaries_View_Configuration : IEntityTypeConfiguration<LockerBankAdminSummaries_View>
{
    public void Configure(EntityTypeBuilder<LockerBankAdminSummaries_View> builder)
    {
        builder.ToView("LockerBankSummaries");
        builder.HasKey(e => e.LockerBankId);
    }
}