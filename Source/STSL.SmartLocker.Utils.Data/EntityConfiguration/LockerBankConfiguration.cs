using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LockerBankConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<LockerBank>
{
    public override void Configure(EntityTypeBuilder<LockerBank> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(256);

        builder.HasIndex(x => new { x.TenantId, x.LocationId, x.Name }).IsUnique();

        builder.ConfigureEntityWithAReferenceImage<LockerBank, LockerBankReferenceImage>();

    }
}