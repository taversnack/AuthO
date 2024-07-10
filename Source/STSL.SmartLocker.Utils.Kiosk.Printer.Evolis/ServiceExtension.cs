using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk;
using STSL.SmartLocker.Utils.Data.Services.Kiosk;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Services;

namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis;

public static class ServiceExtension
{

    public static IServiceCollection ConfigureEvolis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PrinterSettings>(configuration.GetSection("PrinterSettings"));
        services.AddSingleton<IEncodeDecode, EncodeDecode>();
        services.AddSingleton<IPrinterService, PrinterService>();
        services.AddSingleton<IEmailService, EmailService>();
        return services;
    }
}