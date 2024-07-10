using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Kiosk.Models;

public class PrinterDetails
{
    [JsonPropertyName("modelName")]
    public string ModelName { get; set; }
    [JsonPropertyName("port")]
    public string Port { get; set; }
    [JsonPropertyName("address")]
    public string Address { get; set; }
    [JsonPropertyName("printerName")]
    public string PrinterName { get; set; }
}