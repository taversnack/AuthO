using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class LocationReferenceImageConfiguration : EntityBaseConfiguration<LocationReferenceImage>
{
    public override void Configure(EntityTypeBuilder<LocationReferenceImage> builder)
    {
        base.Configure(builder);

        builder.ConfigureReferenceImage();

        builder.ToTable("LocationReferenceImages");
    }
}

internal sealed class LockerBankReferenceImageConfiguration : EntityBaseConfiguration<LockerBankReferenceImage>
{
    public override void Configure(EntityTypeBuilder<LockerBankReferenceImage> builder)
    {
        base.Configure(builder);

        builder.ConfigureReferenceImage();

        builder.ToTable("LockerBankReferenceImages");
    }
}

internal static class ReferenceImageConfigurationExtensions
{
    public static void ConfigureReferenceImage<T>(this EntityTypeBuilder<T> builder) where T : EntityBaseWithTenant, IUsesGuidId, IEntityReferenceImage
    {
        builder.OwnsOne(x => x.MetaData, x =>
        {
            x.Property(x => x.FileName).HasMaxLength(512);
            x.Property(x => x.MimeType).HasMaxLength(256);
            x.Property(x => x.UploadedByCardHolderEmail).HasMaxLength(256);
        });

        builder.Ignore(x => x.EntityId);
    }

    public static void ConfigureEntityWithAReferenceImage<T, E>(this EntityTypeBuilder<T> builder) where T : EntityBaseWithTenant, IUsesGuidId, IHasReferenceImage<E> where E : EntityBaseWithTenant, IUsesGuidId, IEntityReferenceImage
    {
        builder.HasOne(x => x.CurrentReferenceImage).WithOne().HasForeignKey<T>(x => x.CurrentReferenceImageId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.ReferenceImages).WithOne().OnDelete(DeleteBehavior.SetNull);
    }
}
