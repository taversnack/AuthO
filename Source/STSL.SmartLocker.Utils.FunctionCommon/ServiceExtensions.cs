using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace STSL.SmartLocker.Utils.FunctionCommon
{
    public static class ServiceExtensions
    {
        public static IHostBuilder ConfigureSTSLAppSettings(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((hostContext, config) =>
             {
                 // add appsettings.json for logging configuration for our application
                 // (host logging is independent and configured through host.json)
                 config.AddJsonFile("appsettings.json", optional: true);
             });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureSTSLAzureCredential(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddAzureClients(clientBuilder =>
                {
                    clientBuilder.UseCredential(STSLAzureCredential.GetCredential());
                });
            });
            return hostBuilder;
        }

        public static IHostBuilder ConfigureSTSLApplicationInsights(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddApplicationInsightsTelemetryProcessor<CustomTelemetryProcessor>();
                services.AddApplicationInsightsTelemetryWorkerService(options => options.EnableDependencyTrackingTelemetryModule = false);
                services.ConfigureFunctionsApplicationInsights();
            });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureSTSLFunctionLogging(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureLogging((hostingContext, logging) =>
            {
                // remove the default rule that fixes the log level at Warning or above
                logging.Services.Configure<LoggerFilterOptions>(options =>
                {
                    LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                        == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                    if (defaultRule is not null)
                    {
                        options.Rules.Remove(defaultRule);
                    }
                });

                // Make sure the configuration of the appsettings.json file is picked up.
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            });

            return hostBuilder;
        }
    }
}
