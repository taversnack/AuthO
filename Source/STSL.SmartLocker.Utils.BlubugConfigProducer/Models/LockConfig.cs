using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.Models;

internal enum Access
{
    [EnumMember(Value = "RO")]
    ReadOnly,

    [EnumMember(Value = "RW")]
    ReadWrite,
}

internal enum ResultStatus
{
    [EnumMember(Value = "OK")]
    Ok
}

internal interface ILockProperty<T>
{
    static Access Access { get; }
    T? Value { get; init; }
}

internal sealed class ReadOnlyLockProperty<T> : ILockProperty<T>
{
    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadOnly;
    [JsonPropertyName("value")]
    public T? Value { get; init; }
}

internal sealed class ReadOnlyLockProperty : ILockProperty<string>
{
    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadOnly;
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

internal sealed class ReadWriteLockProperty<T> : ILockProperty<T>
{
    public ReadWriteLockProperty(T? target) => (Target, Pending) = (target, null);

    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadWrite;
    [JsonPropertyName("value")]
    public T? Value { get; init; }
    [JsonPropertyName("target")]
    public T? Target { get; set; }
    [JsonPropertyName("pending")]
    public bool? Pending { get; init; } = false;
}
internal sealed class ReadWriteLockProperty : ILockProperty<string>
{
    public ReadWriteLockProperty(string? target) => (Target, Pending) = (target, null);

    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadWrite;
    [JsonPropertyName("value")]
    public string? Value { get; init; }
    [JsonPropertyName("target")]
    public string? Target { get; set; }
    [JsonPropertyName("pending")]
    public bool? Pending { get; init; } = false;
}

internal sealed class LockConfig
{
    [JsonPropertyName("address")]
    public int? Address { get; set; }
    [JsonPropertyName("server_timestamp")]
    public DateTime? ServerTimestamp { get; set; }
    [JsonPropertyName("property")]
    public Property Property { get; set; } = new Property();
    [JsonPropertyName("action")]
    public object? Action { get; set; }
    [JsonPropertyName("channels")]
    public object[]? Channels { get; set; }
    [JsonPropertyName("organisation")]
    public string? Organisation { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

internal sealed class Property
{
    [JsonPropertyName("Version")]
    public ReadOnlyLockProperty<string>? Version { get; set; }

    [JsonPropertyName("Channels")]
    public ReadOnlyLockProperty<int?>? Channels { get; set; }

    [JsonPropertyName("Generation")]
    public ReadOnlyLockProperty<int?>? Generation { get; set; }

    [JsonPropertyName("LogMemorySize")]
    public ReadOnlyLockProperty<int?>? LogMemorySize { get; set; }

    [JsonPropertyName("Serial")]
    public ReadOnlyLockProperty<int?>? Serial { get; set; }

    [JsonPropertyName("TransmitIntervalDirect")]
    public ReadWriteLockProperty<int?>? TransmitIntervalDirect { get; set; }

    [JsonPropertyName("TransmitIntervalIndirect")]
    public ReadWriteLockProperty<int?>? TransmitIntervalIndirect { get; set; }

    [JsonPropertyName("ProhibitMeshing")]
    public ReadWriteLockProperty<bool?>? ProhibitMeshing { get; set; }

    [JsonPropertyName("IndicatorConfiguration")]
    public ReadWriteLockProperty<int?>? IndicatorConfiguration { get; set; }

    [JsonPropertyName("LockerSecurityMode")]
    public ReadWriteLockProperty<int?>? LockOperatingMode { get; set; }

    [JsonPropertyName("LockerSecuritySweep")]
    public ReadWriteLockProperty<int?>? LockSecuritySweep { get; set; }

    [JsonPropertyName("BatteryThresholdAdjust")]
    public ReadWriteLockProperty<int?>? BatteryThresholdAdjust { get; set; }

    // Contains "Cards0" through "Cards19"
    // which should be treated as a [5,3] array of [(16 digit hex string no '0x' prefix)CardSerial, (int)CardType, (unused byte)0]
    [JsonExtensionData]
    public Dictionary<string, object>? Cards { get; set; }
}