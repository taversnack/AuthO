using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Messages;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.BluBugMessageConsume.Services
{
    public class ConsumeService
    {
        private readonly ILogger _logger;
        private readonly DatabaseService _databaseService;

        public ConsumeService(
            ILogger<ConsumeService> logger,
            DatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        public async Task<ServiceBusMessage?> ProcessMessageAsync(BinaryData message)
        {
            ServiceBusMessage? outputMessage = null;
            try
            {
                var msg = JsonSerializer.Deserialize<BluBugMessage>(message);

                if (msg == null)
                {
                    _logger.LogError("Failed to deserialize message {message}", message);
                }
                else
                {
                    LogBlugMessage(msg);
                    bool isDuplicate = await _databaseService.ProcessMessageInDatabaseAsync(msg);

                    if (!isDuplicate && ForwardForFurtherProcessing(msg))
                    {
                        // forward the message on to the output service bus queue
                        outputMessage = new ServiceBusMessage(JsonSerializer.Serialize(msg));
                        outputMessage.Subject = msg.OriginAddress.ToString();
                        outputMessage.ApplicationProperties.Add("OriginAddress", msg.OriginAddress);
                        outputMessage.ApplicationProperties.Add("BoundaryAddress", msg.BoundaryAddress);
                        if (msg.AuditTypeCode.HasValue)
                        {
                            outputMessage.ApplicationProperties.Add("AuditTypeCode", msg.AuditTypeCode.Value.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in ProcessMessageAsync: '{exception}' whilst processing message {message}", ex.Message, message);
            }
            return outputMessage;
        }

        private bool ForwardForFurtherProcessing(BluBugMessage message)
        {
            // only want to forward messages with and audit type
            // (e.g. voltage messages don't have an audit type, so these are filtered out here)
            if (!message.AuditTypeCode.HasValue)
            {
                return false;
            }

            int auditTypeCode = message.AuditTypeCode.Value;

            switch (auditTypeCode)
            {
                case 80:
                case 81:
                    // don't forward keep alives
                    return false;
                default:
                    // for now send everything else through
                    return true;
            }
        }

        private void LogBlugMessage(BluBugMessage m)
        {
            if (m.AuditTypeCode.HasValue)
            {
                if (m.AuditSubject != null)
                {
                    if (m.AuditObject != null)
                    {
                        _logger.LogDebug("Received for lock {lock}, audit code {audit_code}, audit subject {subject}, audit object {audit_object}", m.OriginAddress, m.AuditTypeCode, m.AuditSubject, m.AuditObject);
                    }
                    else
                    {
                        _logger.LogDebug("Received for lock {lock}, audit code {audit_code}, audit subject {audit_subject}", m.OriginAddress, m.AuditTypeCode, m.AuditSubject);
                    }
                }
                else
                {
                    _logger.LogDebug("Received for lock {lock}, audit code {audit_code}", m.OriginAddress, m.AuditTypeCode);
                }
            }
            else
            {
                if (m.ReadingBatteryVoltage.HasValue)
                {
                    _logger.LogDebug("Received for lock {lock}, battery voltage {battery_voltage}", m.OriginAddress, m.ReadingBatteryVoltage);
                }
                else
                {
                    _logger.LogDebug("Received a message with origin address {origin_address}. boundary address {boundary_address}, reading Vdd {reading_vdd}", m.OriginAddress, m.BoundaryAddress, m.ReadingVdd);
                }
            }
        }
    }
}
