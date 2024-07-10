using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Kiosk.Models.Printer
{
    public class EjectAndMove
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }
    }
}
