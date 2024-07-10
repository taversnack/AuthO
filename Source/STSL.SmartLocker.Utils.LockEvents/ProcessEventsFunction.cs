using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.LockEvents.Contracts;
using STSL.SmartLocker.Utils.Messages;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.LockEvents
{
    public class ProcessEventsFunction
    {
        private readonly ILockConfigUpdateService _configUpdateService;
        private readonly ILogger<ProcessEventsFunction> _logger;

        public ProcessEventsFunction(ILockConfigUpdateService configUpdateService, ILogger<ProcessEventsFunction> logger)
            => (_configUpdateService, _logger) = (configUpdateService, logger);

        [Function(nameof(ProcessEventsFunction))]
        public async Task Run([ServiceBusTrigger("%serviceBusTopic%", "%serviceBusSubscription%", Connection = "serviceBusConnection")] ServiceBusReceivedMessage message)
        {
             var parsedMessage = JsonSerializer.Deserialize<BluBugMessage>(message.Body);

            if (parsedMessage is null)
            {
                _logger.LogError("Could not deserialize message:\n{message}", message);
                return;
            }

            await _configUpdateService.HandleUpdatesFromParsedMessageAsync(parsedMessage);

            _logger.LogInformation("Processed lock event from origin: {originAddress}", parsedMessage.OriginAddress);
        }
    }
}
