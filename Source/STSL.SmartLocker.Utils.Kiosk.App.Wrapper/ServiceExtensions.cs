using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AppSettings>().Bind(configuration.GetSection(AppSettings.Name));
        return services;
    }
    public static void ConfigureHostConfiguration(IConfigurationBuilder config)
    {
        config.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!);
        config.AddEnvironmentVariables(prefix: "DOTNET_");
    }
}
