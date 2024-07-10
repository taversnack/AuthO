using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.AzureServiceBus.Configuration;
using System.Net;

namespace STSL.SmartLocker.Utils.AzureServiceBus.Services;

public abstract class AzureServiceBusServiceBase : IAsyncDisposable
{
    protected readonly AzureServiceBusOptions _options;
    private readonly Lazy<ServiceBusClient> _lazyClient;
    private readonly Lazy<ServiceBusSender> _lazySender;
    private readonly Lazy<ServiceBusReceiver> _lazyReceiver;

    public AzureServiceBusServiceBase(IOptions<AzureServiceBusOptions> options)
    {
        _options = options.Value;
        
        // Lazy Initialization
        _lazyClient = new Lazy<ServiceBusClient>(() => new ServiceBusClient(_options.ConnectionString));
        _lazySender = new Lazy<ServiceBusSender>(() => _lazyClient.Value.CreateSender(_options.TopicName));
        _lazyReceiver = new Lazy<ServiceBusReceiver>(() => _lazyClient.Value.CreateReceiver(_options.ResponseTopicName, _options.ResponseSubscriptionName));
    }

    protected ServiceBusClient Client => _lazyClient.Value;
    protected ServiceBusSender Sender => _lazySender.Value;
    protected ServiceBusReceiver Receiver => _lazyReceiver.Value;

    protected async Task<string> PublishMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            // Set the correlationId so we can listen for a response to this message specifically
            var correlationId = Guid.NewGuid().ToString();

            ServiceBusMessage sbMessage = new(message)
            {
                CorrelationId = correlationId
            };
            
            await Sender.SendMessageAsync(sbMessage, cancellationToken);
            return correlationId;
        }
        catch (Exception)
        {
            throw;
        }
    }

    protected async Task<BinaryData?> ReceiveMessageAsync(string correlationId, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var message in Receiver.ReceiveMessagesAsync(cancellationToken))
                {
                    if (message is not null && message.CorrelationId == correlationId)
                    {
                        await Receiver.CompleteMessageAsync(message, cancellationToken);
                        return message.Body;
                    }
                    else if (message != null)
                    {
                        // If the message is not the one we're looking for, abandon it
                        await Receiver.AbandonMessageAsync(message, null, cancellationToken);
                    }              
                }
            }

            // If the cancellationToken is triggered, it means we are no longer listening for messages
            throw new OperationCanceledException("Operation was canceled before the response message was received.");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_lazySender.IsValueCreated)
        {
            await _lazySender.Value.CloseAsync();
        }
        if (_lazyReceiver.IsValueCreated)
        {
            await _lazyReceiver.Value.CloseAsync();
        }
        if(_lazyClient.IsValueCreated)
        {
            await _lazyClient.Value.DisposeAsync();
        }
    }
}
