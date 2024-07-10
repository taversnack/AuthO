namespace STSL.SmartLocker.Utils.Enrolment.WebApp.Models
{
    public class CardCredentialCapture
    {
        public long CSN { get; set; }
        public long MifareHID { get; set; }

        public override string ToString()
        {
            return $"HID: {MifareHID}, CSN: {CSN}";
        }

        public static implicit operator string(CardCredentialCapture self) { return self.ToString(); }
    }
}
