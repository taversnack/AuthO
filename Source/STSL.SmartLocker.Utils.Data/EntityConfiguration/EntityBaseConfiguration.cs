using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal abstract class EntityBaseConfiguration<T> : IEntityTypeConfiguration<T> where T : EntityBase
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ConfigureBaseEntity();
    }
}

internal abstract class EntityBaseConfigurationWithIdMappedToEntityId<T> : IEntityTypeConfiguration<T> where T : EntityBaseWithTenant, IUsesGuidId
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ConfigureBaseEntity();

        builder.HasOne(x => x.Tenant).WithMany().OnDelete(DeleteBehavior.Restrict);

        // NOTE: [0] Redesign to use the tenantId as part of composite PK?
        // .NET Guid is true UUID, which has infinitesimally small collistion chance.
        // We may at some future point prefer autoincremented unsigned ints for
        // storage efficiency, possibly performance if using Hi/Lo optimization
        // (Guid may be better avoiding roundtrip but lacks sequentiality if required),
        // simpler sequentiality, legibility etc.
        // Depending on needs; may be worth using composite.
        //builder.HasKey(x => new { x.TenantId, x.Id });

        builder.Property(x => x.Id).HasColumnName($"{typeof(T).Name}Id");
    }
}

file static class EntityBaseConfigurationExtensions
{
    // Configuration that is for all EntityBase regardless of specialisations
    public static void ConfigureBaseEntity<T>(this EntityTypeBuilder<T> builder) where T : EntityBase
    {
        // Index on TenantId
        builder.HasIndex(x => x.TenantId);
    }
}