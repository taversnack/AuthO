using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using STSL.SmartLocker.Utils.DTO.Kiosk;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using System.Net.Http;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Services
{
    public class KioskConnectivityService : AccessTokenBase, IKioskConnectivityService
    {
        public KioskConnectivityService(
            ILogger<KioskConnectivityService> logger,
            IOptions<AppSettings> settings,
            IHttpClientFactory httpClientFactory) :
            base(logger, settings.Value, httpClientFactory)
        { }

        public async Task<KioskResponseDTO> InitializeKioskAsync()
        {
            HttpClient apiClient = await CreateApiClientWithClientCredentialsAsync();
            var response = await apiClient.PutAsync("kiosk/initialize", null);
            var body = await response.Content.ReadAsStringAsync();

            ThrowIfProblems(nameof(InitializeKioskAsync), response, body);

            return JsonConvert.DeserializeObject<KioskResponseDTO>(body);
        }

    }
}
