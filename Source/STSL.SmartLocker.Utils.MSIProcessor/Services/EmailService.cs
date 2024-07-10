using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.MSIProcessor.Models;

namespace STSL.SmartLocker.Utils.MSIProcessor.Services;

public class EmailService
{
    private readonly EmailClient _emailClient;
    private readonly EmailServiceOptions _options;
    private readonly ILogger _logger;

    public EmailService(
        EmailClient emailClient,
        IOptions<EmailServiceOptions> options,
        ILogger<EmailService> logger)
    {
        _emailClient = emailClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(byte[] attachmentData)
    {
        // Get date string to use for filename and email subject
        string date = DateTime.Now.ToString("dd-MM-yy");

        // Subject & Body
        var emailContent = GetEmailContent(date);

        // Format .csv Attachment
        var attachment = GetEmailAttachment(attachmentData, date);

        var recipients = new EmailRecipients(_options.RecipientAddresses);

        var emailMessage = new EmailMessage(
            senderAddress: _options.SenderAddress,
            recipients: recipients,
            content: emailContent);

        emailMessage.Attachments.Add(attachment);

        try
        {
            await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);
            _logger.LogInformation("Email Sent successfully");
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Email send operation failed with error: '{exception}", ex.Message);
        }
    }

    private static EmailContent GetEmailContent(string date) 
        => new($"MSI Processing Report - {date}") { Html = GetHTMLTemplate() };

    private static EmailAttachment GetEmailAttachment(byte[] data, string date) 
        => new($"MSIOutputReport - {date}.csv", "text/csv", new(data));

    private static string GetHTMLTemplate()
    {
        return @"
        <html>
            <head>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                    }
                    .container {
                        padding: 20px;
                    }
                    .footer {
                        margin-top: 20px;
                        font-size: small;
                        text-align: center;
                    }
                    ul {
                        margin: 10px 0;
                        padding-left: 20px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <p>Hi,</p>
        
                    <p>Please see attached report for the latest MSI output. This report contains staff members that require actioning due to any of the following reasons:</p>
        
                    <ul>
                        <li>They have been marked as terminated</li>
                        <li>They could not be created due to a duplicated email address</li>
                        <li>They have been assigned a new ObjectID</li>
                    </ul>
                    
                    <p>For Staff with a new ObjectID, please update the their record on our system with the new ObjectID recorded in the report.</p>
        
                    <p>Best wishes,<br>
                    STSL Team</p>
                </div>
            </body>
        </html>";
    }
}
