using STSL.SmartLocker.Utils.Enrolment.WebApp.Models;

namespace STSL.SmartLocker.Utils.Enrolment.WebApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(CardCredentialCapture capture, string ipAddress);
    }
}
