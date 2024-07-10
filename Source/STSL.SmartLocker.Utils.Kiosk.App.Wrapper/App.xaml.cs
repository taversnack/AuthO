using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers.HandlerFactory;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Services;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.ViewModel;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Mock;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    private static IConfiguration _configuration;

    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureHostConfiguration(ServiceExtensions.ConfigureHostConfiguration)
        .ConfigureAppConfiguration((context, config) =>
        {
            _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<App>()
            .Build();
            config.Sources.Clear();

            config.AddConfiguration(_configuration);
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            logging.AddNLog("nlog.config");
        })
        .ConfigureServices((context, services) =>
        {
            services.AddLogging();
            services.Configure<AppSettings>(_configuration.GetSection(nameof(AppSettings)));
            services.Configure<PrinterSettings>(_configuration.GetSection(nameof(PrinterSettings)));
            services.AddHttpClient("auth");


            services.AddSingleton<IAccessTokenService, AccessTokenService>();
            services.AddSingleton<IWebRequestHandlerFactory, WebRequestHandlerFactory>();
            services.AddSingleton<IKioskConnectivityService, KioskConnectivityService>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IPostMessageService, PostMessageService>();
            services.AddHostedService<ApplicationHostService>();
            services.ConfigureOptions(_configuration);

            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            if (appSettings.IsTestEnvironment)
            {
                services.ConfigureMock(_configuration);
            }
            else
            {
                services.ConfigureEvolis(_configuration);
            }
        })
        .Build();

    /// <summary>
    /// Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null"/>.</returns>
    public static T GetService<T>()
        where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }
    /// <summary>
    /// Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null"/>.</returns>
    public static T GetRequiredService<T>()
        where T : class
    {
        return (T)_host.Services.GetRequiredService(typeof(T));
    }
    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();
    }

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }
}
