using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts
{
    public interface IAccessTokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
