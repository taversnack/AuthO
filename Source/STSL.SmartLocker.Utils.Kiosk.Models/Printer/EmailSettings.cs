namespace STSL.SmartLocker.Utils.Kiosk.Models.Printer
{
    public class EmailSettings
    {
        public const string Name = "EmailSettings";
        public string StatusReportsToAddresses { get; set; }
        public string OneTimePasswordFromAddress { get; set; }
        public string DefaultFromAddress { get; set; }
        public string EndPoint { get; set; }
        public string AccessKey { get; set; }

    }
}
