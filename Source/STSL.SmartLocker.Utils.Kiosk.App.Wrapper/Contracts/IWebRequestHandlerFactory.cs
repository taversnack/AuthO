namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts
{
    public interface IWebRequestHandlerFactory
    {
        IWebRequestHandler CreateHandler(string type);
    }
}
