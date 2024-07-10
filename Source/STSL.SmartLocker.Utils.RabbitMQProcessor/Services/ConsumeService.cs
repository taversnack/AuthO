using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Messages;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.RabbitMQProcessor.Services
{
    public class ConsumeService
    {
        private readonly ILogger _logger;

        public ConsumeService(ILogger<ConsumeService> logger)
            => (_logger) = (logger);

        public ServiceBusMessage[] ProcessMessage(string message)
        {
            var outputMessages = new List<ServiceBusMessage>();
            try
            {
                var msgs = JsonSerializer.Deserialize<BluBugMessage[]>(message);

                if (msgs is null)
                {
                    _logger.LogError("Failed to deserialize message {message}", message);
                }
                else
                {
                    foreach (BluBugMessage m in msgs)
                    {
                        // forward the message on to the output service bus queue
                        var sbm = new ServiceBusMessage(JsonSerializer.Serialize(m))
                        {
                            Subject = m.OriginAddress.ToString()
                        };
                        sbm.ApplicationProperties.Add("OriginAddress", m.OriginAddress);
                        sbm.ApplicationProperties.Add("BoundaryAddress", m.BoundaryAddress);
                        if (m.AuditTypeCode.HasValue)
                        {
                            sbm.ApplicationProperties.Add("AuditTypeCode", m.AuditTypeCode.Value.ToString());
                        }
                        outputMessages.Add(sbm);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in ProcessMessageAsync: '{exception}' whilst processing message {message}", ex.Message, message);
            }
            return [.. outputMessages];
        }

    }
}
