using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using System;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers;

public class ReturnAndProcessTemporaryCardHandler : IWebRequestHandler
{

    private readonly IPrinterService _printerService;

    public ReturnAndProcessTemporaryCardHandler(IPrinterService printerWrapper)
    {
        _printerService = printerWrapper;
    }

    public async Task<Response> HandleAsync(object data)
    {
        await Task.CompletedTask;
        try
        {
            var returnStatus = _printerService.ReturnAndProcessTemporaryCard();
            return returnStatus;
        }
        catch (Exception ex)
        {
            return new Response(null, false, ex.ToString());
        }
    }
}
