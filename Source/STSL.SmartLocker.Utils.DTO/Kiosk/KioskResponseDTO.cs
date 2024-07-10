namespace STSL.SmartLocker.Utils.DTO.Kiosk
{
    public class KioskResponseDTO
    {
        public Guid TenantId { get; set; }
        public Guid KioskId { get; set; }
        public string KioskName { get; set; }
        public Guid LocationId { get; set; }
    }
}
