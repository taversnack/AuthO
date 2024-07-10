namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;
using System;
using System.Threading.Tasks;

internal class PrepareCardForEncodingHandler : IWebRequestHandler
{
    private IPrinterService _printerService;
    private readonly IHostEnvironment _hostEnvironment;

    public PrepareCardForEncodingHandler(IPrinterService printerWrapper, IHostEnvironment hostEnvironment)
    {
        _printerService = printerWrapper;
        _hostEnvironment = hostEnvironment;
    }
    public async Task<Response> HandleAsync(object data)
    {
        await Task.CompletedTask;
        try
        {
            var primeCardResult = _printerService.PrepareCardForEncoding();
            return primeCardResult;
        }
        catch (Exception ex)
        {
            return new Response(null, false, ex.ToString());
        }

    }
}
