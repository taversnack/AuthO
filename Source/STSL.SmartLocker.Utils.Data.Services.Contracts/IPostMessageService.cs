using STSL.SmartLocker.Utils.Kiosk.Models;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts
{
    public interface IPostMessageService
    {
        public void PostMessage(Response message);
    }
}
