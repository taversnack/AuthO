﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace STSL.SmartLocker.Utils.Reporting.Data.Deploy
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);

            await host.RunConsoleAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builder) => builder.AddUserSecrets<Program>())
                .UseStartup<Startup>();
    }
}