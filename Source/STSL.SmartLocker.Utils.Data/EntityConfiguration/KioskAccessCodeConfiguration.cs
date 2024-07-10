using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.Domain.Kiosk;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class KioskAccessCodeConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<KioskAccessCode>
{
    public override void Configure(EntityTypeBuilder<KioskAccessCode> builder)
    {
        base.Configure(builder);
        
        builder.Property(x => x.HasBeenUsed).HasDefaultValue(false);
    }
}
