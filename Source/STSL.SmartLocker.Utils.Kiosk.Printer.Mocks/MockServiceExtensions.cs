using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Mock.Services;

namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Mock
{
    public static class MockServiceExtensions
    {
        public static void ConfigureMock(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<PrinterSettings>(configuration.GetSection("CardPrinter:Mock"));
            services.AddSingleton<IPrinterService, MockPrinterService>();
        }
    }
}
