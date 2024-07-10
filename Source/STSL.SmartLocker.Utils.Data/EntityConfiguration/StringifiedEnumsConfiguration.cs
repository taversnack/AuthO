using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class StringifiedLockerBankBehaviorConfiguration : IEntityTypeConfiguration<StringifiedLockerBankBehaviour>
{
    public void Configure(EntityTypeBuilder<StringifiedLockerBankBehaviour> builder)
    {
        builder.HasKey(x => x.Value);

        builder.HasData(Enum.GetValues<LockerBankBehaviour>().Select(x => new StringifiedLockerBankBehaviour
        {
            Name = Enum.GetName(x) ?? x.ToString(),
            Value = x,
        }));
    }
}

internal sealed class StringifiedLockOperatingModeConfiguration : IEntityTypeConfiguration<StringifiedLockOperatingMode>
{
    public void Configure(EntityTypeBuilder<StringifiedLockOperatingMode> builder)
    {
        builder.HasKey(x => x.Value);

        builder.HasData(Enum.GetValues<LockOperatingMode>().Select(x => new StringifiedLockOperatingMode
        {
            Name = Enum.GetName(x) ?? x.ToString(),
            Value = x,
        }));
    }
}
