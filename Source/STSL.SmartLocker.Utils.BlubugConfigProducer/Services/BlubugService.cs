using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Configuration;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Contracts;
using STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Mappings;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Models;
using STSL.SmartLocker.Utils.Common.Data;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.Services;

public sealed class BlubugService : IBlubugService
{
    private static readonly JsonSerializerOptions _serializerOptions = new();

    static BlubugService()
    {
        _serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BlubugService> _logger;
    private readonly BlubugServiceOptions _options;

    public BlubugService(
        IHttpClientFactory httpClientFactory,
        ILogger<BlubugService> logger,
        IOptions<BlubugServiceOptions> blubugServiceOptions)
        => (_httpClientFactory, _logger, _options)
        = (httpClientFactory, logger, blubugServiceOptions.Value);

    public async Task<bool> UpdateLockConfigAsync(LockSerial lockSerial, UpdateLockConfigDTO lockConfigDTO, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient(_options.BlubugHttpClientName)
            ?? throw new InvalidOperationException("Could not create named http client: " + _options.BlubugHttpClientName);

        try
        {
            var lockConfigInBlubugFormat = lockConfigDTO.ToBlubugFormat();

            _logger.LogInformation("Sending JSON to Blubug for lock serial: {lockSerial}\n{json}", lockSerial.Value, JsonSerializer.Serialize(lockConfigInBlubugFormat, _serializerOptions));

            using var result = await httpClient.PutAsJsonAsync("device_put?address=" + lockSerial.Value, lockConfigInBlubugFormat, _serializerOptions, cancellationToken);

            return result.IsSuccessStatusCode;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException tex)
        {
            _logger.LogInformation("Timed out attempting to put lock config for lock serial: {lockSerial}", lockSerial.Value);
            return false;
        }
    }

    public async Task<LockConfigDTO?> GetLockConfigAsync(LockSerial lockSerial, CancellationToken cancellationToken = default)
    {
        using var httpClient = _httpClientFactory.CreateClient(_options.BlubugHttpClientName)
            ?? throw new InvalidOperationException("Could not create named http client: " + _options.BlubugHttpClientName);

        var lockConfig = await httpClient.GetFromJsonAsync<LockConfig>("device_get?address=" + lockSerial.Value, _serializerOptions, cancellationToken);

        return lockConfig?.ToDTO();
    }
}
