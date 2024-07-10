using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using System;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers;

internal class DispenseNewCardHandler : IWebRequestHandler
{
    private IPrinterService _printerService;

    public DispenseNewCardHandler(IPrinterService printerWrapper)
    {
        _printerService = printerWrapper;
    }
    public async Task<Response> HandleAsync(object data)
    {
        await Task.CompletedTask;
        try
        {
            var dispenseResult = _printerService.DispenseNewCard();

            return dispenseResult;
        }
        catch (Exception ex)
        {
            return new Response(null, false, ex.ToString());
        }
    }
}
