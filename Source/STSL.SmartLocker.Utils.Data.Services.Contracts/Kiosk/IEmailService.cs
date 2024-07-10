using STSL.SmartLocker.Utils.Common.Enum;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk
{
    public interface IEmailService
    {
        Task SendNotificationEmailAsync(EmailRequestDTO requestDTO);
        Task<bool> SendErrorEmailAsync(ErrorType errorType);
    }
}
