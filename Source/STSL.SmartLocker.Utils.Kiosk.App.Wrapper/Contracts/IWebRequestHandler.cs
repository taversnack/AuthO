using STSL.SmartLocker.Utils.Kiosk.Models;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;

public interface IWebRequestHandler
{
    Task<Response> HandleAsync(object data=null);
}
