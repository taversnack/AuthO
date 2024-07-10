using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class CardHolderConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<CardHolder>
{
    public override void Configure(EntityTypeBuilder<CardHolder> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Email).HasMaxLength(256);

        builder.Property(x => x.FirstName).HasMaxLength(256);
        builder.Property(x => x.LastName).HasMaxLength(256);
        builder.Property(x => x.UniqueIdentifier).HasMaxLength(256);

        builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.UniqueIdentifier }).IsUnique();
    }
}
