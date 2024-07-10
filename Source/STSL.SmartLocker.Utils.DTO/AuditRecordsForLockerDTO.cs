namespace STSL.SmartLocker.Utils.DTO;

public class AuditRecordsForLockerDTO
{
    // Not sure what RowNum is
    public long RowNum { get; set; }
    // Could this be an enum? Limits the possible range of values
    // gives the property more context.
    public string AuditCategory { get; set; } = string.Empty;
    public string AuditDescription { get; set; } = string.Empty;
    public DateTime AuditTime { get; set; }
    public string? Subject { get; set; }
    public string? Object { get; set; }
    //public LockSerial LockSerialNumber { get; set; }
    public int LockSerialNumber { get; set; }
    // Could this be an enum? Limits the possible range of values
    // gives the property more context.
    public int AuditType { get; set; }
    // SN is presumably SerialNumber? 
    public string? SubjectSN { get; set; }
    public string? ObjectSN { get; set; }
}
