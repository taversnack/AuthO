using STSL.SmartLocker.Utils.CLI.Models;
using STSL.SmartLocker.Utils.CLI.Services.Contracts;
using System.Net.Http.Json;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.CLI.Services;

internal sealed class LockerService : ILockerService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly JsonSerializerOptions _defaultSerializerOptions = new();

    static LockerService()
    {
        _defaultSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        //_defaultSerializerOptions.PropertyNameCaseInsensitive = true;
    }

    public LockerService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<LockerConfigJson?> GetLockerConfigJson(LockerSerial lockerId)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.LockersHttpClient);

        return await httpClient.GetFromJsonAsync<LockerConfigJson>($"device_get?address={lockerId}", _defaultSerializerOptions);
    }

    public async Task<LockerConfig?> GetLockerConfig(LockerSerial lockerId)
    {
        var jsonConfig = await GetLockerConfigJson(lockerId);

        return jsonConfig?.ConvertToSimplifiedFormat();
    }

    public async Task<bool> SetLockerConfig(LockerConfig config)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.LockersHttpClient);

        var convertedConfig = LockerConfig.ConvertToEndpointFormat(config);

        var result = await httpClient.PutAsJsonAsync($"device_put?address={config.Id}", convertedConfig, _defaultSerializerOptions);

        return result.IsSuccessStatusCode;
    }

    public async Task<bool> SetupLockers(LockersSetup config)
    {
        foreach (var locker in config.Lockers)
        {
            if (!await SetLockerConfig(locker))
            {
                return false;
            }
        }
        return true;
    }

    public async Task<bool> SetLockerConfig(LockerConfigJson config, LockerSerial lockerId)
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.LockersHttpClient);

        var result = await httpClient.PutAsJsonAsync($"device_put?address={lockerId}", config, _defaultSerializerOptions);

        return result.IsSuccessStatusCode;
    }
}
