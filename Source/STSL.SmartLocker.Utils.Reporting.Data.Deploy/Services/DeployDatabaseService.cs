using DbUp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Reporting.Data.Deploy.Utils;

namespace STSL.SmartLocker.Utils.Reporting.Data.Deploy.Services
{
    internal class DeployDatabaseService : IHostedService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private readonly AppSettings _settings;

        public DeployDatabaseService(
            IHostApplicationLifetime applicationLifetime,
            ILogger<DeployDatabaseService> logger,
            IOptions<AppSettings> settings)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _settings = settings.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                DeployDatabase();
                _applicationLifetime.StopApplication();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in StartAsync: '{exception}'", ex.Message);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void DeployDatabase()
        {
            var connectionString = _settings.DatabaseConnectionString;

            if (connectionString == null)
            {
                throw new ApplicationException("No database connection string set");
            }

#if DEBUG
            _logger.LogInformation("Debug - ensure database exists");
            EnsureDatabase.For.SqlDatabase(connectionString);
#endif
            try
            {
                _logger.LogInformation("Deploy schema scripts");
                DbUpUtils.UpgradeDatabase(connectionString, ScriptFileType.Schema);

                _logger.LogInformation("Deploy function scripts");
                DbUpUtils.UpgradeDatabaseNoJournal(connectionString, ScriptFileType.Function);

                _logger.LogInformation("Deploy view scripts");
                DbUpUtils.UpgradeDatabaseNoJournal(connectionString, ScriptFileType.View);

                _logger.LogInformation("Deploy stored procedure scripts");
                DbUpUtils.UpgradeDatabaseNoJournal(connectionString, ScriptFileType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in DeployDatabase: '{exception}'", ex.Message);
            }
        }
    }
}
