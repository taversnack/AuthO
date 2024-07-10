using Microsoft.Extensions.Configuration;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper
{
    public class WebClient
    {
        public const string Name = "ContentUrL";
        public string ContentUrl { get; set; }

        public WebClient(IConfiguration configuration)
        {
            ContentUrl = configuration.GetValue(Name, "");
        }
    }
}
