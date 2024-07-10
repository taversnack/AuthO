namespace STSL.SmartLocker.Utils.AzureServiceBus.Configuration;

public sealed class AzureServiceBusOptions
{
    public string? ConnectionString { get; set; }
    public string? TopicName { get; set; }
    public string? ResponseTopicName { get; set; }
    public string? ResponseSubscriptionName { get; set; }
}