using STSL.SmartLocker.Utils.Enrolment.WebApp.Models;
using STSL.SmartLocker.Utils.Enrolment.WebApp.Services;

namespace STSL.SmartLocker.Utils.Enrolment.WebApp
{
    public static class ServiceExtensions
    {
        public static void AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection("EmailService"));

            services.AddSingleton<IEmailService, EmailService>();
        }
    }
}
