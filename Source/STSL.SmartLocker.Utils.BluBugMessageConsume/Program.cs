﻿using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using STSL.SmartLocker.Utils.BluBugMessageConsume.Services;
using STSL.SmartLocker.Utils.FunctionCommon;

var host = new HostBuilder()
    .ConfigureSTSLAppSettings()
    .ConfigureSTSLAzureCredential()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureSTSLApplicationInsights()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddServiceBusClientWithNamespace(Environment.GetEnvironmentVariable("serviceBusConnection__fullyQualifiedNamespace"))
                .WithName("outputSender");
        });

        services.AddSingleton<ConsumeService>();
        services.AddSingleton<DatabaseService>();
    })
    .ConfigureSTSLFunctionLogging()
    .Build();

host.Run();