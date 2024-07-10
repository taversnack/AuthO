namespace STSL.SmartLocker.Utils.AzureServiceBus.Contracts;

public interface IAzureServiceBusService
{
    Task<bool> PublishMessageAndGetSuccessResponseAsync<T>(T message, CancellationToken cancellationToken);
    Task<E?> PublishMessageAndGetDataResponseAsync<T, E>(T message, CancellationToken cancellationToken);
    //Task<string?> PublishMessageAsync<T>(T message, CancellationToken cancellation = default);
    //Task<T?> ReceiveMessageAsync<T>(string correlationId, CancellationToken cancellationToken = default);
}