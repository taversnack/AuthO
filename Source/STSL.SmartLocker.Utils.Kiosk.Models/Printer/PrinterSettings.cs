namespace STSL.SmartLocker.Utils.Kiosk.Models;

public class PrinterSettings
{
    public const string Name = "PrinterSettings";
    public string ServerAddress { get; set; }
    public string ServerPort { get; set; }
    public string PrinterModel { get; set; }
    public string PrinterName { get; set; }
}