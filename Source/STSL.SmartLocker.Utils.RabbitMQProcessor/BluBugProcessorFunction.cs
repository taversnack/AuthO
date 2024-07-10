using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.RabbitMQProcessor.Services;

namespace STSL.SmartLocker.Utils.RabbitMQProcessor
{
    public class BluBugProcessorFunction
    {
        private readonly ConsumeService _consumeService;
        private readonly ILogger<BluBugProcessorFunction> _logger;
        private readonly ServiceBusSender _serviceBusSender;

        public BluBugProcessorFunction(
            ConsumeService consumeService,
            ILogger<BluBugProcessorFunction> logger,
            IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _consumeService = consumeService;
            _logger = logger;
            _serviceBusSender = serviceBusClientFactory.CreateClient("outputSender").CreateSender(Environment.GetEnvironmentVariable("serviceBusQueue"));
        }

        [Function(nameof(BluBugProcessorFunction))]
        public async Task Run(
            [RabbitMQTrigger("%rabbitMQQueue%", ConnectionStringSetting = "rabbitMQConnection")] string queueItem)
        {
            _logger.LogInformation($"BluBug message received: {queueItem}");

              ServiceBusMessage[] outputMessages = _consumeService.ProcessMessage(queueItem);

            foreach (ServiceBusMessage message in outputMessages)
            {
                await _serviceBusSender.SendMessageAsync(message);
            }
        }
    }
}
