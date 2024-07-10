using STSL.SmartLocker.Utils.DTO.Kiosk;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts
{
    public interface IKioskConnectivityService
    {
        Task<KioskResponseDTO> InitializeKioskAsync();
    }
}
