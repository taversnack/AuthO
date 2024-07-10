using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using System;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers
{
    public class RejectCardHandler : IWebRequestHandler
    {
        private readonly IPrinterService _printerService;

        public RejectCardHandler(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        public async Task<Response> HandleAsync(object data)
        {
            await Task.CompletedTask;
            try
            {
                var response = _printerService.RejectCard();

                if (response.Success)
                {
                    return response;
                }
                else
                {
                    return new Response(null, false, response.Message);
                }
            }
            catch (Exception ex)
            {
                return new Response(null, false, ex.ToString());
            }
        }
    }
}
