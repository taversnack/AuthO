using STSL.SmartLocker.Utils.Domain.Kiosk;

namespace STSL.SmartLocker.Utils.Domain;

public sealed class Location : EntityBaseWithTenant, IUsesGuidId, IHasReferenceImage<LocationReferenceImage>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public ICollection<Kiosks> Kiosks { get; set; }

    // NOTE: As of .NET 6; supplying either format is fine & cross platform compatible though I'd recommend IANA *nix "Europe/London" format.
    // Timezone data must be available on whatever platform the binary is running on
    // e.g. if using on an Alpine Linux docker container, the timezone info / IANA database should be installed
    // as a separate dependency. I suspect this may not be such an issue on Azure but cannot say for certain (citation needed)!
    // Windows
    //public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
    // *nix
    //public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    public Guid? CurrentReferenceImageId { get; set; }

    // navigation
    public List<LockerBank>? LockerBanks { get; set; }

    public LocationReferenceImage? CurrentReferenceImage { get; set; }

    public List<LocationReferenceImage>? ReferenceImages { get; set; }
}
