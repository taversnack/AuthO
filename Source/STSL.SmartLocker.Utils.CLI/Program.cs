using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using STSL.SmartLocker.Utils.CLI.Services;
using STSL.SmartLocker.Utils.CLI.Services.Contracts;
using System.Net;

namespace STSL.SmartLocker.Utils.CLI;

internal sealed class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
                config.AddUserSecrets("87b340c3-7588-42f1-a1bc-301e6b1e7b47");
                config.Build();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<CLIService>();

                services.AddTransient<ILockerService, LockerService>();

                // This could just be a static class with static constructor that takes in args I suppose
                services.AddSingleton<IApplicationArgs, ApplicationArgs>(s => new ApplicationArgs(args));

                EndpointOptions options = new();
                context.Configuration.GetSection(nameof(EndpointOptions)).Bind(options);

                services.AddHttpClient(Constants.LockersHttpClient, client =>
                {
                    client.BaseAddress = new Uri(options.Url);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential(options.Organisation, options.Password),
                });
            });
        //.ConfigureLogging((context, logging) =>
        //{
        //    // TODO: Setup more advanced logging options for command output
        //});

        await host.RunConsoleAsync();
    }
}