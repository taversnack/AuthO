using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class CardCredentialConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<CardCredential>
{
    public override void Configure(EntityTypeBuilder<CardCredential> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.SerialNumber).HasMaxLength(16).IsFixedLength().IsUnicode(false);
        builder.Property(x => x.HidNumber).HasMaxLength(32);

        builder.Property(x => x.CardLabel).HasMaxLength(256);

        builder.HasIndex(x => x.SerialNumber).IsUnique();
        builder.HasIndex(x => x.HidNumber).IsUnique();

        // NOTE: See todo comment in CardCredential domain entity class
        //builder.HasOne(x => x.CardHolder).WithMany().OnDelete(DeleteBehavior.SetNull);
    }
}
