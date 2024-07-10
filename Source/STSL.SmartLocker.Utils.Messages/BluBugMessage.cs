using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Messages;

public sealed class BluBugMessage
{
    /// <summary>
    /// Serial number of lock.
    /// </summary>
    [JsonPropertyName("Origin/Address")]
    public int OriginAddress { get; set; }

    /// <summary>
    /// Timestamp when data was generated, recorded on the lock.
    /// </summary>
    [JsonPropertyName("Origin/Timestamp")]
    public DateTime? OriginTimestamp { get; set; }

    /// <summary>
    /// Timestamp when data arrived at the server.
    /// </summary>
    [JsonPropertyName("Server/Timestamp")]
    public DateTime ServerTimestamp { get; set; }

    /// <summary>
    /// The network address of a device on the boundary between the Mercury sensor network and Capital cloud network.
    /// This is often a gateway.
    /// </summary>
    [JsonPropertyName("Boundary/Address")]
    public int BoundaryAddress { get; set; }

    /// <summary>
    /// Audit events have a sequence number which allows Cloud Capital to ensure that every event is delivered at least once.
    /// </summary>
    [JsonPropertyName("Reading/Seqno")]
    public long? ReadingSeqno { get; set; }

    /// <summary>
    /// An integer specifying the type of audit event.
    /// </summary>
    [JsonPropertyName("Audit/TypeCode")]
    public int? AuditTypeCode { get; set; }

    /// <summary>
    /// The identity of the card which caused this event.
    /// The identity is 8 bytes expressed in hexadecimal.
    /// </summary>
    [JsonPropertyName("Audit/Subject")]
    public string? AuditSubject { get; set; }

    /// <summary>
    /// The identity of the welcome card when used to welcome a card (in AuditSubject) or
    /// when a master card is used to unlock a shared lock, AuditObject is the identity of the original user card that locked the lock.
    /// The identity is 8 bytes expressed in hexadecimal.
    /// </summary>
    [JsonPropertyName("Audit/Object")]
    public string? AuditObject { get; set; }

    /// <summary>
    /// The battery voltage for the lock
    /// </summary>
    [JsonPropertyName("Reading/BatteryVoltage")]
    public decimal? ReadingBatteryVoltage { get; set; }

    [JsonPropertyName("Reading/Vdd")]
    public decimal? ReadingVdd { get; set; }

    [JsonPropertyName("Origin/Urgent")]
    public bool? OriginUrgent { get; set; }
}