using Microsoft.Extensions.Hosting;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers.HandlerFactory;

public partial class WebRequestHandlerFactory : IWebRequestHandlerFactory
{
    private readonly IPrinterService _printerWrapper;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IHostEnvironment _hostEnvironment;

    public WebRequestHandlerFactory(IPrinterService printerWrapper, IAccessTokenService accessTokenService, IHostEnvironment hostEnvironment)
    {
        _printerWrapper = printerWrapper;
        _accessTokenService = accessTokenService;
        _hostEnvironment = hostEnvironment;
    }
    public IWebRequestHandler CreateHandler(string name)
    {
        return name switch
        {
            "DispenseCard" => new DispenseNewCardHandler(_printerWrapper),
            "ReadyCard" => new PrepareCardForEncodingHandler(_printerWrapper, _hostEnvironment),
            "ReturnTemporaryCard" => new ReturnAndProcessTemporaryCardHandler(_printerWrapper),
            "EjectOrMove" => new EjectCardOrMoveToBackHandler(_printerWrapper),
            "ApiAccessToken" => new GetApiAccessTokenHandler(_accessTokenService),
            "RejectCard" => new RejectCardHandler(_printerWrapper),
            _ => throw new HandlerNotFoundException($"Handler {name} not found"),
        };
    }
}
