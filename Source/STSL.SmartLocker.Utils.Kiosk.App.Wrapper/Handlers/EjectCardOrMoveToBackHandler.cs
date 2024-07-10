using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Models.Printer;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using System;
using System.Text.Json;
using System.Threading.Tasks;

internal class EjectCardOrMoveToBackHandler : IWebRequestHandler
{
    private IPrinterService _printerService;
    public EjectCardOrMoveToBackHandler(IPrinterService printerWrapper)
    {
        _printerService = printerWrapper;
    }
    public async Task<Response> HandleAsync(object data)
    {
        await Task.CompletedTask;
        try
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data is JsonElement)
            {
                EjectAndMove request = JsonSerializer.Deserialize<EjectAndMove>(data.ToString());
                var dispenseResult = _printerService.EjectCardOrMoveToBack(request.Action);
                return dispenseResult;
            }
            
            return new Response(null, false, "Invalid Format of Data");

        }
        catch (Exception ex)
        {
            return new Response(null, false, ex.ToString());
        }
    }
}
