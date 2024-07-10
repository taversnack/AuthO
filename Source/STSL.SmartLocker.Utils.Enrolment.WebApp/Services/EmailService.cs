using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Enrolment.WebApp.Models;
using System.Collections.Concurrent;

namespace STSL.SmartLocker.Utils.Enrolment.WebApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailOptions _emailOptions;

        private ConcurrentDictionary<string, CardCredentialCapture> _recentSentCaptures = new ConcurrentDictionary<string, CardCredentialCapture>();
        private DateTime _lastConcurrentDictionaryFlush = DateTime.UtcNow;

        private const double ConcurrentDictionaryRetainMinutes = 60; // flush every 60 minutes
        private const int ConcurrentDictionaryMaxCount = 1000; // if we have over 1000 cached entries, then we may be under attack

        public EmailService(ILogger<EmailService> logger, IOptions<EmailOptions> emailOptions)
        {
            _logger = logger;
            _emailOptions = emailOptions.Value;
        }

        public async Task SendEmailAsync(CardCredentialCapture capture, string ipAddress)
        {
            // empty our cache of captures sent on a periodic basis
            var diff = DateTime.UtcNow - _lastConcurrentDictionaryFlush;
            if ((DateTime.UtcNow - _lastConcurrentDictionaryFlush).TotalMinutes > ConcurrentDictionaryRetainMinutes)
            {
                _recentSentCaptures.Clear();
                _lastConcurrentDictionaryFlush = DateTime.UtcNow;
            }

            // don't send captures that have recently been sent
            if (_recentSentCaptures.ContainsKey(capture))
            {
                _logger.LogInformation("NOT sending email with data {capture} as recently sent", capture);
                return;
            }

            // if the cache has grown massive, then don't send anything - we are probably under attack!
            if (_recentSentCaptures.Count >= ConcurrentDictionaryMaxCount)
            {
                _logger.LogWarning("NOT sending email - excessive number of entries in the recent sent cache");
                return;
            }

            string connectionString = _emailOptions.ConnectionString;
            EmailClient emailClient = new EmailClient(connectionString);

            var htmlContent = $@"
<html>
    <head>
        <style>
            th {{
                text-align:right;
            }}
        </style>
    </head>
    <body>
        <h2>New Enrolment Card Swipe</h2>
        <h3>Generated at {TimeNow} from IP address {ipAddress}</h3>
        <p>This email message is sent from the STSL locker enrolment utility.</p>
        <table>
            <tr>
                <th>HID</th>
                <td>{capture.MifareHID}</td>
            </tr>
            <tr>
                <th>CSN</th>
                <td>{capture.CSN}</td>
            </tr>
        </table>
    </body>
</html>";

            var emailContent = new EmailContent("New Enrolment Card Swipe")
            {
                Html = htmlContent
            };

            var emailRecipients = new EmailRecipients(_emailOptions.Recipients.Select(e => new EmailAddress(e)));

            var emailMessage = new EmailMessage(
                senderAddress: _emailOptions.Sender,
                emailRecipients,
                emailContent);

            try
            {
                _logger.LogInformation("Sending email with data {capture}", capture);
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    WaitUntil.Started, emailMessage);

                _recentSentCaptures.TryAdd(capture, capture);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Email send operation failed with error code: '{ErrorCode}', message: '{Message}'", ex.ErrorCode, ex.Message);
            }
        }

        private DateTime TimeNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
    }
}
