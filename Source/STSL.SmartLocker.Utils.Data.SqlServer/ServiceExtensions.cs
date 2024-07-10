using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.SqlServer.Contexts;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures;
using System.Reflection;

namespace STSL.SmartLocker.Utils.Data.SqlServer;

public static class ServiceExtensions
{
    private const string DefaultConnectionStringName = "SmartLockerDatabase";

    public static void AddSqlServerDatabase(this IServiceCollection services, IConfiguration configuration, string connectionStringName = DefaultConnectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName) ?? throw new ArgumentException($"No connection string could be found with name: {connectionStringName}", nameof(connectionStringName));

        services.AddSqlServerDatabase(connectionString);
    }

    public static void AddSqlServerDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<SmartLockerDbContext>(options => options.SetDatabaseOptions(connectionString));
        services.AddDbContext<SmartLockerDbContext>(options => options.SetDatabaseOptions(connectionString), optionsLifetime: ServiceLifetime.Singleton);
        services.AddDbContext<SmartLockerSqlServerDbContext>(options => options.SetDatabaseOptions(connectionString), optionsLifetime: ServiceLifetime.Singleton);
        services.TryAddTransient<StoredProcedureRepository>();

        services.TryAddDatabaseExceptionHandler();
    }

    private static void SetDatabaseOptions(this DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        });
    }

    private static void TryAddDatabaseExceptionHandler(this IServiceCollection services)
    {
        services.TryAddSingleton<IDatabaseExceptionHandler, SqlServerExceptionHandler>();
    }
}
