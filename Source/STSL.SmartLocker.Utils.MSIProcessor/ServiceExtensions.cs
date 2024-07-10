using Microsoft.Extensions.DependencyInjection;
using STSL.SmartLocker.Utils.MSIProcessor.Models;
using STSL.SmartLocker.Utils.MSIProcessor.Services;
using STSL.SmartLocker.Utils.Data.SqlServer;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace STSL.SmartLocker.Utils.MSIProcessor;

internal static class ServiceExtensions
{
    public static void ConfigureEmailServices(this IServiceCollection services, IConfiguration config)
    {
        var emailServiceConnectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");

        var emailAddresses = new List<EmailAddressRecipients>();
        config.GetSection("EmailRecipientAddresses").Bind(emailAddresses);

        // Binding to Azure Email Address Struct
        var recipients  = emailAddresses.Select(cr => new EmailAddress(cr.Address)).ToList();

        var emailServiceOptions = new EmailServiceOptions
        {
            SenderAddress = Environment.GetEnvironmentVariable("EmailSenderAddress"),
            RecipientAddresses = recipients
        };

        services.AddSingleton(Options.Create(emailServiceOptions));

        services.AddSingleton(new EmailClient(emailServiceConnectionString));

        services.AddSingleton<EmailService>();
    }

    public static void ConfigureDatabaseServices(this IServiceCollection services)
    {
        var databaseConnectionString = Environment.GetEnvironmentVariable("SmartLockerDatabase") ?? throw new InvalidOperationException("No SmartLockerDatabase environment variable found.");

        services.AddSqlServerDatabase(databaseConnectionString);

        services.AddSingleton<DatabaseService>();
    }
}