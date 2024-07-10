using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STSL.SmartLocker.Utils.AzureServiceBus.Configuration;
using STSL.SmartLocker.Utils.AzureServiceBus.Contracts;
using STSL.SmartLocker.Utils.AzureServiceBus.Services;

namespace STSL.SmartLocker.Utils.AzureServiceBus;

public static class ServiceExtensions
{
    public static void ConfigureAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureServiceBusOptions>(configuration.GetSection("AzureServiceBusOptions:AccessControl"));

        services.AddScoped<IAzureServiceBusService, AzureServiceBusService>();
    }
}
