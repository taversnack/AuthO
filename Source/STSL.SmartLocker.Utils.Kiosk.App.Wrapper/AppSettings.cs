namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper;

public class AppSettings
{
    public const string Name = "AppSettings";
    public bool IsTestEnvironment { get; set; }
    public int StatusDelayTime { get; set; }
    public class AuthorizationClass
    {
        public string AuthServer { get; set; }
        public string Audience { get; set; }

        public class ClientCredentialsClass
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string Scope { get; set; }
        }
        public ClientCredentialsClass ClientCredentials { get; set; }
    };
    public AuthorizationClass Authorization { get; set; }

    public class GeneralSettingsClass
    {
        public bool TrustedDevice { get; set; }
        public bool Development { get; set; }
    };
    public GeneralSettingsClass GeneralSettings { get; set; }
}
