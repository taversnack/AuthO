using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.BluBugMessageConsume.Services;

namespace STSL.SmartLocker.Utils.BluBugMessageConsume
{
    public class BluBugMessageConsumeFunction
    {
        private readonly ConsumeService _consumeService;
        private readonly ILogger<BluBugMessageConsumeFunction> _logger;
        private readonly ServiceBusSender _serviceBusSender;

        public BluBugMessageConsumeFunction(
            ConsumeService consumeService,
            ILogger<BluBugMessageConsumeFunction> logger,
            IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _consumeService = consumeService;
            _logger = logger;
            _serviceBusSender = serviceBusClientFactory.CreateClient("outputSender").CreateSender(Environment.GetEnvironmentVariable("serviceBusQueueOut"));
        }

        [Function(nameof(BluBugMessageConsumeFunction))]
        public async Task Run(
            [ServiceBusTrigger("%serviceBusTopic%", "%serviceBusSubscription%", Connection = "serviceBusConnection")] ServiceBusReceivedMessage queueItem)
        {
            _logger.LogInformation($"Receiving BluBug message: {queueItem.Body}");

            ServiceBusMessage? outputMessage = await _consumeService.ProcessMessageAsync(queueItem.Body);

            if (outputMessage != null)
            {
                await _serviceBusSender.SendMessageAsync(outputMessage);
            }
        }
    }
}
