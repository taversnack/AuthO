namespace STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;

// EXEC sp_describe_first_result_set N'slk.ListAuditRecordsForLocker'
public class ListAuditRecordsForLocker_Result
{
    public long RowNum { get; set; }
    public string? AuditCategory { get; set; }
    public string? AuditDescription { get; set; }
    public DateTime AuditTime { get; set; }
    public string? Subject { get; set; }
    public string? Object { get; set; }
    public int LockSerialNumber { get; set; }
    public int AuditType { get; set; }
    public string? SubjectSN { get; set; }
    public string? ObjectSN { get; set; }
}

public class ListAuditRecordsForLocker_Result_Paged
{
    public int RowCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<ListAuditRecordsForLocker_Result> Results { get; set; } = Enumerable.Empty<ListAuditRecordsForLocker_Result>();
}