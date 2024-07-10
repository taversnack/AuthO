namespace STSL.SmartLocker.Utils.BlubugConfigProducer.Configuration;

public sealed class BlubugServiceOptions
{
    public string Url { get; set; } = "https://api.blubug.net/";
    public string Organisation { get; set; } = "organisation-stsl";
    public string Password { get; set; } = string.Empty;
    public string BlubugHttpClientName { get; set; } = "BlubugHttpClient";
    public int ConfigRequestTimeoutSeconds { get; set; } = 90;
}
