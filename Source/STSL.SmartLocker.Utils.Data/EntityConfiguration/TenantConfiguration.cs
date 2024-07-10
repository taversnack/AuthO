using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class TenantConfiguration : EntityBaseConfiguration<Tenant>
{
    public override void Configure(EntityTypeBuilder<Tenant> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name).HasMaxLength(256);
        builder.Property(x => x.CardHolderAliasSingular).HasMaxLength(256);
        builder.Property(x => x.CardHolderAliasPlural).HasMaxLength(256);
        builder.Property(x => x.CardHolderUniqueIdentifierAlias).HasMaxLength(256).HasDefaultValue("Unique Identifier");
        builder.Property(x => x.HelpPortalUrl).HasMaxLength(1024);
        builder.Property(x => x.LogoMimeType).HasMaxLength(256);
        // 2 Megabytes MB
        builder.Property(x => x.Logo).HasMaxLength(1024 * 1024 * 2);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
