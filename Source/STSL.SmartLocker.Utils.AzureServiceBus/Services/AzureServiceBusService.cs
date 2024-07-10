using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.AzureServiceBus.Configuration;
using STSL.SmartLocker.Utils.AzureServiceBus.Contracts;
using System.Net;
using System.Text.Json;
using STSL.SmartLocker.Utils.DTO.AzureServiceBus;

namespace STSL.SmartLocker.Utils.AzureServiceBus.Services;

public class AzureServiceBusService : AzureServiceBusServiceBase, IAzureServiceBusService
{
    private readonly ILogger _logger;
    public AzureServiceBusService(IOptions<AzureServiceBusOptions> options, ILogger<AzureServiceBusService> logger)
        : base(options) => (_logger) = (logger);

    public async Task<bool> PublishMessageAndGetSuccessResponseAsync<T>(T message, CancellationToken cancellationToken)
    {
        var correlationId = await PublishMessageAsync(message, cancellationToken);

        if (correlationId is null)
        {
            return false;
        }

        var azureResponse = await ReceiveMessageAsync<AzureServiceBusResponseMessage>(correlationId, cancellationToken);

        // Ensure is successful AND response is OK. Maybe this can be removed but safety first
        return (azureResponse?.IsSuccess ?? false) && azureResponse?.StatusCode == HttpStatusCode.OK;
    }

    public async Task<E?> PublishMessageAndGetDataResponseAsync<T, E>(T message, CancellationToken cancellationToken)
    {
        var correlationId = await PublishMessageAsync(message, cancellationToken);

        if (correlationId is null)
        {
            return default;
        }
       
        var azureResponse = await ReceiveMessageAsync<AzureServiceBusDataMessage<E?>>(correlationId, cancellationToken);

        if ((azureResponse?.IsSuccess ?? false) && azureResponse?.StatusCode == HttpStatusCode.OK)
        {
            return azureResponse.Data;
        }

        return default;
    }

    #region Private Functions

    private async Task<string?> PublishMessageAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);

            if (jsonMessage is null)
            {
                LogError($"Failed to serialize message: {message}");
                return null;
            }

            return await base.PublishMessageAsync(jsonMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            LogException(ex, "Exception thrown in PublishMessageAsync");
            throw;
        }
    }

    private async Task<T?> ReceiveMessageAsync<T>(string correlationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await base.ReceiveMessageAsync(correlationId, cancellationToken);
            if (message is null)
            {
                LogError("Failed to receive message from service bus");
                return default;
            }

            return DeserializeMessage<T>(message);
        }
        catch (OperationCanceledException)
        {
            LogError("Operation timed out");
            throw;
        }
        catch (Exception ex)
        {
            LogException(ex, "Exception thrown in ReceiveMessageAsync");
            return default;
        }
    }

    private T? DeserializeMessage<T>(BinaryData message)
    {
        var deserializedMessage = JsonSerializer.Deserialize<T>(message);
        if (deserializedMessage is null)
        {
            LogError($"Failed to deserialize message: {message}");
        }
        return deserializedMessage;
    }

    private void LogError(string errorMessage)
    {
        _logger.LogDebug(errorMessage);
    }

    private void LogException(Exception ex, string contextMessage)
    {
        _logger.LogError(ex, $"{contextMessage}: '{ex.Message}'");
    }

    #endregion Private Functions
}