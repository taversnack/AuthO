using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.Domain.Kiosk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Data.EntityConfiguration;

internal sealed class KioskLockerAssignmentConfiguration : EntityBaseConfigurationWithIdMappedToEntityId<KioskLockerAssignment>
{

    public override void Configure(EntityTypeBuilder<KioskLockerAssignment> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.ReplacedCardCredential).WithMany().OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CardHolder).WithMany().OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Locker).WithMany().OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.IsTemporaryCardActive).HasDefaultValue(false);
    }
}
