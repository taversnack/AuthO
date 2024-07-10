using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Data.SqlServer.Views;

namespace STSL.SmartLocker.Utils.Data.SqlServer.EntityConfiguration;

internal sealed class LockersWithStatus_View_Configuration : IEntityTypeConfiguration<LockersWithStatus_View>
{
    public void Configure(EntityTypeBuilder<LockersWithStatus_View> builder)
    {
        builder.ToView("LockersWithStatus");
        builder.HasKey(e => e.LockerId);
        builder.Property(e => e.BatteryVoltage).HasColumnType("decimal(4,3)");
    }
}
