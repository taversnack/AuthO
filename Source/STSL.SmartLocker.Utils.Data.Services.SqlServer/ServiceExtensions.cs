using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using STSL.SmartLocker.Utils.Data.Services.Contracts;

namespace STSL.SmartLocker.Utils.Data.Services.SqlServer;

public static class ServiceExtensions
{
    public static void AddSqlServerApplicationServices(this IServiceCollection services)
    {
        services.TryAddTransient<IMartService, MartService>();
    }
}