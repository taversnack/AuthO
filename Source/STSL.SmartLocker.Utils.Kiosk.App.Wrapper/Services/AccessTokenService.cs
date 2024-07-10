using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using System.Net.Http;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Services;

public class AccessTokenService : AccessTokenBase, IAccessTokenService
{
    public AccessTokenService(
          ILogger<AccessTokenService> logger,
          IOptions<AppSettings> settings,
          IHttpClientFactory httpClientFactory) :
          base(logger, settings.Value, httpClientFactory)
    { }

    public async Task<string> GetAccessTokenAsync()
    {
        string token = await GetClientCredentialsAccessTokenAsync();

        return token;
    }
}
