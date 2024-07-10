using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Data.SqlServer.Views;

// EXEC sp_help N'slk.LockersWithStatus'
public class LockersWithStatus_View
{
    public Guid TenantId { get; set; }
    public Guid LocationId { get; set; }
    public Guid LockerBankId { get; set; }
    public Guid LockerId { get; set; }
    public Guid? LockId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? ServiceTag { get; set; }
    public LockerSecurityType SecurityType { get; set; }
    public string? AssignedTo { get; set; }
    public Guid? AssignedToCardHolderId { get; set; }
    public string? AssignedToUniqueIdentifier { get; set; }
    public int? AssignedToManyCount { get; set; }
    public int? LockSerialNumber { get; set; }
    public string? LockFirmwareVersion { get; set; }
    public LockOperatingMode? LockOperatingMode { get; set; }
    public decimal? BatteryVoltage { get; set; }
    public byte? LastAudit { get; set; }
    public string? LastAuditCategory { get; set; }
    public string? LastAuditDescription { get; set; }
    public DateTime? LastAuditTime { get; set; }
    public Guid? LastAuditSubjectId { get; set; }
    public Guid? LastAuditObjectId { get; set; }
    public string? LastAuditSubjectUniqueIdentifier { get; set; }
    public string? LastAuditObjectUniqueIdentifier { get; set; }
    public string? LastAuditSubject { get; set; }
    public string? LastAuditObject { get; set; }
    public string? LastAuditSubjectSN { get; set; }
    public string? LastAuditObjectSN { get; set; }
    public DateTime? LastCommunication { get; set; }
    public int? BoundaryAddress { get; set; }
}
