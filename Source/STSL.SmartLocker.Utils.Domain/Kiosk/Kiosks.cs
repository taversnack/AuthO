using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STSL.SmartLocker.Utils.Domain.Kiosk
{
    public sealed class Kiosks
    {
        [Key]
        public Guid KioskId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public Guid TenantId { get; set; }
        public Guid? LocationId { get; set; }
        public Location Location { get; set; }
    }
}
