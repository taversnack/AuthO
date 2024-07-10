using System.Net;

namespace STSL.SmartLocker.Utils.DTO.AzureServiceBus;

public class AzureServiceBusResponseMessage
{
    public bool IsSuccess { get; set; }

    public HttpStatusCode StatusCode { get; set; }

    public string? ErrorMessage { get; set; }
}

public sealed class AzureServiceBusDataMessage<T> : AzureServiceBusResponseMessage
{
    public T? Data { get; set; }
}