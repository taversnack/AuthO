using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.SqlServer.Contexts;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;
using STSL.SmartLocker.Utils.Data.SqlServer.Views;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.SqlServer;

public sealed class MartService : IMartService
{
    private readonly SmartLockerSqlServerDbContext _dbContext;
    private readonly StoredProcedureRepository _storedProcedureRepository;
    private readonly ILogger<MartService> _logger;

    public MartService(
        SmartLockerSqlServerDbContext dbContext,
        StoredProcedureRepository storedProcedureRepository,
        ILogger<MartService> logger)
        => (_dbContext, _storedProcedureRepository, _logger)
        = (dbContext, storedProcedureRepository, logger);

    // Unused - Remove
    public async Task<List<LockerStatusDTO>> ListLockersWithStatusForLockerBankAsync(Guid tenantId, Guid lockerBankId, CancellationToken cancellationToken = default)
    {
        // TODO: Add exception handling, filtering, sorting
        var query = from locker in _dbContext.LockersWithStatus_View
                    where
                        locker.TenantId == tenantId
                        && locker.LockerBankId == lockerBankId
                    orderby locker.Label
                    select MapLockerWithStatusViewToDTO(locker);

        return await query.ToListAsync(cancellationToken);
    }

    // Unused - Remove
    public async Task<List<AuditRecordsForLockerDTO>> ListAuditRecordsForLockerAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
    {
        // Simple version without paging (uses stored procedure default maximum rows returned).
        //var results = await _storedProcedureRepository.SP_ListAuditRecordsForLocker(tenantId, lockerId, cancellationToken);

        // The stored procedure supports paging - example here returns the first page of audits (most recent first). Page size set to 20.
        // Therefore this returns the most recent 20 audits.
        var results = await _storedProcedureRepository.SP_ListAuditRecordsForLocker_Paged(tenantId, lockerId, 1, 20, cancellationToken);

        return results.Results.Select(MapAuditRecordsForLockerToDTO).ToList();
    }

    public async Task<IPagingResponse<AuditRecordsForLockerDTO>> GetPagedAuditRecordsForLockerAsync(Guid tenantId, Guid lockerId, IPagingRequest pagingRequest, CancellationToken cancellationToken = default)
    {
        var results = await _storedProcedureRepository.SP_ListAuditRecordsForLocker_Paged(tenantId, lockerId, pagingRequest.PageIndex + 1, pagingRequest.RecordsPerPage, cancellationToken);

        var mappedResults = results.Results.Select(MapAuditRecordsForLockerToDTO).ToList();

        return new PagingResponse<AuditRecordsForLockerDTO>
        {
            PageIndex = pagingRequest.PageIndex,
            RecordsPerPage = pagingRequest.RecordsPerPage,
            RecordCount = mappedResults.Count,
            TotalRecords = results.RowCount,
            TotalPages = (results.RowCount + pagingRequest.RecordsPerPage - 1) / pagingRequest.RecordsPerPage,
            Results = mappedResults
        };
    }

    public async Task<IReadOnlyList<LockerStatusDTO>> GetManyLockersWithStatusForLockerBankAsync(Guid tenantId, Guid lockerBankId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await _dbContext.LockersWithStatus_View
            .Where(x => x.TenantId == tenantId && x.LockerBankId == lockerBankId)
            .FilterAndSort(filter, sort)
            .Select(x => MapLockerWithStatusViewToDTO(x))
            .ToListAsync();

    private static LockerStatusDTO MapLockerWithStatusViewToDTO(LockersWithStatus_View locker)
        => new()
        {
            LockerId = locker.LockerId,
            LockId = locker.LockId,
            Label = locker.Label,
            ServiceTag = locker.ServiceTag,
            AssignedTo = locker.AssignedTo,
            SecurityType = locker.SecurityType,
            AssignedToCardHolderId = locker.AssignedToCardHolderId,
            AssignedToUniqueIdentifier = locker.AssignedToUniqueIdentifier,
            AssignedToManyCount = locker.AssignedToManyCount,
            LockSerialNumber = locker.LockSerialNumber,
            LockFirmwareVersion = locker.LockFirmwareVersion,
            LockOperatingMode = locker.LockOperatingMode,
            BatteryVoltage = locker.BatteryVoltage,
            LastAudit = locker.LastAudit,
            LastAuditCategory = locker.LastAuditCategory,
            LastAuditDescription = locker.LastAuditDescription,
            LastAuditTime = locker.LastAuditTime,
            LastAuditSubjectId = locker.LastAuditSubjectId,
            LastAuditObjectId = locker.LastAuditObjectId,
            LastAuditSubjectUniqueIdentifier = locker.LastAuditSubjectUniqueIdentifier,
            LastAuditObjectUniqueIdentifier = locker.LastAuditObjectUniqueIdentifier,
            LastAuditSubject = locker.LastAuditSubject,
            LastAuditObject = locker.LastAuditObject,
            LastAuditSubjectSN = locker.LastAuditSubjectSN,
            LastAuditObjectSN = locker.LastAuditObjectSN,
            LastCommunication = locker.LastCommunication
        };

    private static AuditRecordsForLockerDTO MapAuditRecordsForLockerToDTO(ListAuditRecordsForLocker_Result auditRecord)
        => new()
        {
            RowNum = auditRecord.RowNum,
            AuditCategory = auditRecord.AuditCategory ?? string.Empty,
            AuditDescription = auditRecord.AuditDescription ?? string.Empty,
            AuditTime = auditRecord.AuditTime,
            Subject = auditRecord.Subject,
            Object = auditRecord.Object,
            LockSerialNumber = auditRecord.LockSerialNumber,
            AuditType = auditRecord.AuditType,
            SubjectSN = auditRecord.SubjectSN,
            ObjectSN = auditRecord.ObjectSN
        };
}

internal static class LockerStatusExtensions
{
    public static IQueryable<LockersWithStatus_View> FilterAndSort(this IQueryable<LockersWithStatus_View> all, IFilteredRequest? filter = null, ISortedRequest? sort = null)
    {
        // TODO: Add other filterable / sortable properties
        const string LockerLabel = "label";
        const string LockerServiceTag = "servicetag";
        const string LockSerialNumber = "lockserialnumber";
        const string AssignedTo = "assignedto";
        const string Vacant = "vacant"; // un-assigned lockers
        const string Alerts = "alerts"; // lockers with action needs (Battery change, smart lock not used in 3 months) TODO: expand warnings?
        const string Allocated = "allocated";
        const string Battery = "battery";
        const string LastAuditTime = "lastaudittime";
        const string LastCommunication = "lastcommunication";

        if (filter is not null)// && !string.IsNullOrWhiteSpace(filter.FilterValue)
        {
            var filterLockerLabel = filter.IsNullOrEmptyOrHasFilterProperty(LockerLabel);
            var filterLockerServiceTag = filter.IsNullOrEmptyOrHasFilterProperty(LockerServiceTag);
            var filterAssignedTo = filter.IsNullOrEmptyOrHasFilterProperty(AssignedTo);
            var filterVacant = filter.IsNullOrEmptyOrHasFilterProperty(Vacant);
            var filterAllocated = filter.IsNullOrEmptyOrHasFilterProperty(Allocated);

            var filterAlerts = filter.IsNullOrEmptyOrHasFilterProperty(Alerts);

            all = all.Where(x =>
                (filterLockerLabel && x.Label.Contains(filter.FilterValue)) ||
                (filterLockerServiceTag && x.ServiceTag.Contains(filter.FilterValue)) ||
                (filterAssignedTo && x.AssignedTo != null && x.AssignedTo.Contains(filter.FilterValue)) ||
                (filterVacant && x.AssignedTo == null) || // vacant lockers
                (filterAllocated && x.AssignedTo != null) || // allocated lockers
                (filterAlerts && // TODO: Improve this alerts filtering
                    (
                        (x.BatteryVoltage != null && x.BatteryVoltage <= 3.400m) ||
                        (x.LastAuditTime != null && x.LastAuditTime <= DateTime.Now.AddMonths(-1))
                    )
                ) 
            );
        }

        sort ??= new SortedRequest();

        all = sort.SortBy?.ToLowerInvariant() switch
        {
            LockerServiceTag => sort.UseSorting(all, x => x.ServiceTag),
            LockSerialNumber => sort.UseSorting(all, x => x.LockSerialNumber),
            AssignedTo => sort.UseSorting(all, x => x.AssignedTo),
            Battery => sort.UseSorting(all, x => x.BatteryVoltage),
            LastAuditTime => sort.UseSorting(all, x => x.LastAuditTime),
            LastCommunication => sort.UseSorting(all, x => x.LastCommunication),
            // Sort by locker label as default
            _ => sort.UseSorting(all, x => x.Label),
        };

        return all;
    }
}
