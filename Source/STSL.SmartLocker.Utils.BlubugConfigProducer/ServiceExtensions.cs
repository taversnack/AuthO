using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Configuration;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Contracts;
using STSL.SmartLocker.Utils.BlubugConfigProducer.Services;
using System.Net;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer;

public static class ServiceExtensions
{
    const string DefaultBlubugServiceOptionsSection = nameof(BlubugServiceOptions);

    public static void AddBlubugServices(this IServiceCollection services)
    {
        services.TryAddTransient<IBlubugService, BlubugService>();
    }

    public static void ConfigureBlubugServices(this IServiceCollection services, IConfiguration configuration, string blubugServiceOptionsSection = DefaultBlubugServiceOptionsSection)
    {
        // Retrieve Blubug config section or throw
        var blubugServiceOptionsConfig = configuration.GetRequiredSection(blubugServiceOptionsSection);

        var lockConfigOptions = blubugServiceOptionsConfig.Get<BlubugServiceOptions>()
            ?? throw new InvalidOperationException($"Could not bind a configuration section named '{blubugServiceOptionsSection}' to an instance of {nameof(BlubugServiceOptions)}");

        // Set up DI IOptions for BlubugServiceOptions
        services.Configure<BlubugServiceOptions>(blubugServiceOptionsConfig);

        // Add and configure named HttpClient to be created through IHttpClientFactory
        services.AddHttpClient(lockConfigOptions.BlubugHttpClientName, client =>
        {
            client.BaseAddress = new Uri(lockConfigOptions.Url);
            client.Timeout = TimeSpan.FromSeconds(lockConfigOptions.ConfigRequestTimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseDefaultCredentials = true,
            Credentials = new NetworkCredential(lockConfigOptions.Organisation, lockConfigOptions.Password),
        });
    }
}
