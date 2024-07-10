using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using STSL.SmartLocker.Utils.FunctionCommon;
using STSL.SmartLocker.Utils.MSIProcessor;
using STSL.SmartLocker.Utils.MSIProcessor.Services;

var host = new HostBuilder()
    .ConfigureSTSLAppSettings()
    .ConfigureSTSLAzureCredential()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureSTSLApplicationInsights()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<ConsumeService>();
        services.ConfigureDatabaseServices();
        services.ConfigureEmailServices(hostContext.Configuration);
    })
    .ConfigureSTSLFunctionLogging()
    .Build();

host.Run();
