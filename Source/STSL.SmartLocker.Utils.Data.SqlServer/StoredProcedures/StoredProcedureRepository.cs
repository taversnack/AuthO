using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Data.SqlServer.Contexts;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;
using System.Data;

namespace STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures;

public class StoredProcedureRepository
{
    private readonly SmartLockerSqlServerDbContext _dbContext;
    private readonly ILogger<StoredProcedureRepository> _logger;

    public StoredProcedureRepository(SmartLockerSqlServerDbContext dbContext, ILogger<StoredProcedureRepository> logger) =>
        (_dbContext, _logger) = (dbContext, logger);

    public async Task<List<ListAuditRecordsForLocker_Result>> SP_ListAuditRecordsForLocker(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
    {
        // return first n audit records (as specified in the stored procedure default parameter values)
        return await _dbContext.ListAuditRecordsForLocker_Results
            .FromSql($"EXECUTE slk.ListAuditRecordsForLocker @tenantId={tenantId}, @lockerId={lockerId}")
            .ToListAsync(cancellationToken);
    }

    public async Task<ListAuditRecordsForLocker_Result_Paged> SP_ListAuditRecordsForLocker_Paged(Guid tenantId, Guid lockerId, int pageNumber = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        var tenantIdParam = new SqlParameter("@tenantId", tenantId);
        var lockerIdParam = new SqlParameter("@lockerId", lockerId);

        var pageNumberParam = new SqlParameter("@pageNumber", pageNumber);
        var pageSizeParam = new SqlParameter("@pageSize", pageSize);
        var rowCountParam = new SqlParameter { ParameterName = "@rowCount", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };

        var results = await _dbContext.ListAuditRecordsForLocker_Results
            .FromSqlRaw("EXECUTE slk.ListAuditRecordsForLocker @tenantId, @lockerId, @rowCount OUTPUT, @pageNumber, @pageSize", tenantIdParam, lockerIdParam, rowCountParam, pageNumberParam, pageSizeParam)
            .ToListAsync(cancellationToken);

        return new ListAuditRecordsForLocker_Result_Paged
        {
            RowCount = (int)rowCountParam.Value,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Results = results
        };
    }

    public async Task<List<ProcessMSIData_Result>> SP_ProcessMSIData(Guid tenantId, string json, CancellationToken cancellationToken = default)
    {
        var tenantIdParam = new SqlParameter("@tenantId", tenantId);
        var jsonParam = new SqlParameter("@json", json);

        var results = await _dbContext.ProcessMSIOutput_Results
            .FromSqlRaw("EXECUTE slk.ProcessMSIData @tenantId, @json", tenantIdParam, jsonParam)
            .ToListAsync(cancellationToken);

        return results;
    }
}
