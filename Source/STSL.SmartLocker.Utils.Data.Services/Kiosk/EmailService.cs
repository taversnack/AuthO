using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Enum;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk;
using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.Kiosk.Models.Printer;

namespace STSL.SmartLocker.Utils.Data.Services.Kiosk
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private EmailSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task SendNotificationEmailAsync(EmailRequestDTO requestDTO)
        {
            _logger.LogInformation("SendNotificationEmailAsync called with EmailType: {EmailType}, ToEmail: {ToEmail}", requestDTO.EmailType, requestDTO.ToEmail);

            var sender = requestDTO.EmailType switch
            {
                EmailType.OneTimePasswordFromAddress => _emailSettings.OneTimePasswordFromAddress,
                EmailType.DefaultFromAddress => _emailSettings.DefaultFromAddress,
                EmailType.StatusReport => _emailSettings.DefaultFromAddress,
                _ => throw new ArgumentException("Invalid email type specified")
            };
            if (requestDTO.ToEmail == null && requestDTO.EmailType == EmailType.StatusReport)
            {
                requestDTO.ToEmail = _emailSettings.StatusReportsToAddresses;
            }
            var emailContent = new EmailContent(requestDTO.Subject)
            {
                PlainText = requestDTO.PlainText,
                Html = requestDTO.HtmlContent,
            };

            var toRecipients = new List<EmailAddress>() { new EmailAddress(requestDTO.ToEmail) };
            var emailRecipients = new EmailRecipients(toRecipients);

            var connectionString = $"{_emailSettings.EndPoint};{_emailSettings.AccessKey}";

            try
            {
                var emailClient = new EmailClient(connectionString);

                var emailMessage = new EmailMessage(sender, emailRecipients, emailContent);
                var emailMessageResponse = await emailClient.SendAsync(WaitUntil.Completed, emailMessage);
                _logger.LogInformation($"Email Sent. Status = {emailMessageResponse}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email due to an exception.");
                throw;
            }
            _logger.LogInformation("SendNotificationEmailAsync completed");
        }


        public async Task<bool> SendErrorEmailAsync(ErrorType errorType)
        {

            _logger.LogInformation("SendErrorEmailAsync called with errorType: {errorType}", errorType);
            var errorDetails = GetErrorDetails(errorType);

            var emailOptions = new EmailRequestDTO
            {
                Subject = errorDetails.Subject,
                HtmlContent = $"<p>Hi,</p><p>{errorDetails.Message}</p><p>Regards,<br/>Locker Team</p>",
                EmailType = EmailType.StatusReport
            };

            try
            {
                _logger.LogInformation("Before calling SendNotificationEmailAsync in SendErrorEmailAsync for errorType: {errorType}", errorType);
                await SendNotificationEmailAsync(emailOptions);
                _logger.LogInformation("After calling SendNotificationEmailAsync in SendErrorEmailAsync for errorType: {errorType}", errorType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send error email for {errorType}", errorType);
                return false;
            };
        }
        private ErrorDetails GetErrorDetails(ErrorType errorType) => errorType switch
        {
            ErrorType.Ribbon => new ErrorDetails
            {
                Subject = "Ribbon Error in Kiosk",
                Message = "A Ribbon Error has been detected. This issue often involves problems with the ribbon cartridge, which may be incorrectly installed, damaged, or depleted."
            },
            ErrorType.RejectBox => new ErrorDetails
            {
                Subject = "Reject Box Error in Kiosk",
                Message = "A Reject Box Error has been detected. Please check if the reject box is full or open, and address accordingly."
            },
            ErrorType.Mechanical => new ErrorDetails
            {
                Subject = "Mechanical Error in Kiosk",
                Message = "A Mechanical Error has been detected. Please check for any mechanical failures or misalignments."
            },
            ErrorType.Temperature => new ErrorDetails
            {
                Subject = "Temperature Error in Kiosk",
                Message = "A Temperature Error has been detected. Please check the device for overheating components."
            },
            ErrorType.DataHandling => new ErrorDetails
            {
                Subject = "Data Handling Error in Kiosk",
                Message = "A Data Handling Error has been detected. Please check the data processing units for errors."
            },
            ErrorType.Cover => new ErrorDetails
            {
                Subject = "Cover Error in Kiosk",
                Message = "A Cover Error has been detected. Ensure that all covers and access panels are properly closed."
            },
            ErrorType.ClearError => new ErrorDetails
            {
                Subject = "Clear Error in Kiosk",
                Message = "A Clear Error has been detected. Please ensure the clearing mechanisms are functioning properly."
            },
            ErrorType.Other => new ErrorDetails
            {
                Subject = "General Error in Kiosk",
                Message = "A General Error has been detected. Please perform a comprehensive system check."
            },
            _ => throw new ArgumentException("Invalid error type.")
        };
    }
}