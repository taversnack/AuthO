using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using STSL.SmartLocker.Utils.Data.SqlServer;
using STSL.SmartLocker.Utils.FunctionCommon;
using STSL.SmartLocker.Utils.LockEvents.Contracts;
using STSL.SmartLocker.Utils.LockEvents.Services;

var host = new HostBuilder()
    .ConfigureSTSLAppSettings()
    .ConfigureSTSLAzureCredential()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureSTSLApplicationInsights()
    .ConfigureServices((hostContext, services) =>
    {
        var databaseConnectionString = Environment.GetEnvironmentVariable("SmartLockerDatabase") ?? throw new InvalidOperationException("No SmartLockerDatabase environment variable found.");
        services.AddSqlServerDatabase(databaseConnectionString);

        services.AddScoped<ILockConfigUpdateService, LockConfigUpdateService>();
    })
    .ConfigureSTSLFunctionLogging()
    .Build();

host.Run();
