using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STSL.SmartLocker.Utils.Reporting.Data.Deploy.Services;

namespace STSL.SmartLocker.Utils.Reporting.Data.Deploy
{
    public class Startup
    {
        IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

            services.AddHostedService<DeployDatabaseService>();
        }
    }
}
