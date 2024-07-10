namespace STSL.SmartLocker.Utils.Domain.Kiosk
{
    public class KioskCardCredentialCapture
    {

        public long SerialNumber { get; set; }
        public long HIDNumber { get; set; }

        public override string ToString()
        {
            return $"HID: {HIDNumber}, CSN: {SerialNumber}";
        }

        public static implicit operator string(KioskCardCredentialCapture self) { return self.ToString(); }
    }
}
