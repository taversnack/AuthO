using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.CLI.Models;

internal enum LockerSecurityMode : byte
{
    Installation = 0,
    Shared = 1,
    Dedicated = 2,
    Confiscated = 3,
    Reader = 4,
}

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

internal interface ILockerProperty<T>
{
    static Access Access { get; }
    T? Value { get; init; }

    //public T? Target { get; }
    //public bool? Pending { get; }
}

internal class LockerPropertyResponse
{
    public ResultStatus Result { get; init; }
}

internal class LockerConfigResponse
{
    [JsonPropertyName("address")]
    public LockerSerial Address { get; init; }
    [JsonPropertyName("property")]
    public Dictionary<string, LockerPropertyResponse>? Property { get; init; }
    [JsonPropertyName("status")]
    public ResultStatus Status { get; init; }
}

internal class ReadOnlyLockerProperty<T> : ILockerProperty<T>
{
    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadOnly;
    [JsonPropertyName("value")]
    public T? Value { get; init; }

    //public T? Target { get; }
    //public bool? Pending { get; }
}

internal class ReadOnlyLockerProperty : ILockerProperty<string>
{
    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadOnly;
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    //public T? Target { get; }
    //public bool? Pending { get; }
}

internal class ReadWriteLockerProperty<T> : ILockerProperty<T>
{
    public ReadWriteLockerProperty(T? target) => (Target, Pending) = (target, null);

    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadWrite;
    [JsonPropertyName("value")]
    public T? Value { get; init; }
    [JsonPropertyName("target")]
    public T? Target { get; set; }
    [JsonPropertyName("pending")]
    public bool? Pending { get; init; } = false;
}
internal class ReadWriteLockerProperty : ILockerProperty<string>
{
    public ReadWriteLockerProperty(string? target) => (Target, Pending) = (target, null);

    [JsonPropertyName("access")]
    public static Access Access { get; } = Access.ReadWrite;
    [JsonPropertyName("value")]
    public string? Value { get; init; }
    [JsonPropertyName("target")]
    public string? Target { get; set; }
    [JsonPropertyName("pending")]
    public bool? Pending { get; init; } = false;
}

internal sealed class LockerConfigJson
{
    [JsonPropertyName("address")]
    public LockerSerial? Address { get; set; }
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
    public ReadOnlyLockerProperty<string>? Version { get; set; }
    [JsonPropertyName("Channels")]
    public ReadOnlyLockerProperty<int?>? Channels { get; set; }
    [JsonPropertyName("Generation")]
    public ReadOnlyLockerProperty<int?>? Generation { get; set; }
    [JsonPropertyName("LogMemorySize")]
    public ReadOnlyLockerProperty<int?>? LogMemorySize { get; set; }
    [JsonPropertyName("Serial")]
    public ReadOnlyLockerProperty<int?>? Serial { get; set; }
    [JsonPropertyName("TransmitIntervalDirect")]
    public ReadWriteLockerProperty<int?>? TransmitIntervalDirect { get; set; }
    [JsonPropertyName("TransmitIntervalIndirect")]
    public ReadWriteLockerProperty<int?>? TransmitIntervalIndirect { get; set; }
    [JsonPropertyName("ProhibitMeshing")]
    public ReadWriteLockerProperty<bool?>? ProhibitMeshing { get; set; }
    [JsonPropertyName("IndicatorConfiguration")]
    public ReadWriteLockerProperty<int?>? IndicatorConfiguration { get; set; }
    [JsonPropertyName("LockerSecurityMode")]
    public ReadWriteLockerProperty<LockerSecurityMode?>? LockerSecurityMode { get; set; }
    [JsonPropertyName("LockerSecuritySweep")]
    public ReadWriteLockerProperty<int?>? LockerSecuritySweep { get; set; }

    // Combine all into [JsonExtensionData] Dictionary
    // in any case these do not need to be jagged arrays,
    // they should be multidimensional object[,]
    [JsonPropertyName("Cards0")]
    public ReadWriteLockerProperty<object[][]>? Cards0 { get; set; }
    [JsonPropertyName("Cards1")]
    public ReadWriteLockerProperty<object[][]>? Cards1 { get; set; }
    [JsonPropertyName("Cards2")]
    public ReadWriteLockerProperty<object[][]>? Cards2 { get; set; }
    [JsonPropertyName("Cards3")]
    public ReadWriteLockerProperty<object[][]>? Cards3 { get; set; }
    [JsonPropertyName("Cards4")]
    public ReadWriteLockerProperty<object[][]>? Cards4 { get; set; }
    [JsonPropertyName("Cards5")]
    public ReadWriteLockerProperty<object[][]>? Cards5 { get; set; }
    [JsonPropertyName("Cards6")]
    public ReadWriteLockerProperty<object[][]>? Cards6 { get; set; }
    [JsonPropertyName("Cards7")]
    public ReadWriteLockerProperty<object[][]>? Cards7 { get; set; }
    [JsonPropertyName("Cards8")]
    public ReadWriteLockerProperty<object[][]>? Cards8 { get; set; }
    [JsonPropertyName("Cards9")]
    public ReadWriteLockerProperty<object[][]>? Cards9 { get; set; }
    [JsonPropertyName("Cards10")]
    public ReadWriteLockerProperty<object[][]>? Cards10 { get; set; }
    [JsonPropertyName("Cards11")]
    public ReadWriteLockerProperty<object[][]>? Cards11 { get; set; }
    [JsonPropertyName("Cards12")]
    public ReadWriteLockerProperty<object[][]>? Cards12 { get; set; }
    [JsonPropertyName("Cards13")]
    public ReadWriteLockerProperty<object[][]>? Cards13 { get; set; }
    [JsonPropertyName("Cards14")]
    public ReadWriteLockerProperty<object[][]>? Cards14 { get; set; }
    [JsonPropertyName("Cards15")]
    public ReadWriteLockerProperty<object[][]>? Cards15 { get; set; }
    [JsonPropertyName("Cards16")]
    public ReadWriteLockerProperty<object[][]>? Cards16 { get; set; }
    [JsonPropertyName("Cards17")]
    public ReadWriteLockerProperty<object[][]>? Cards17 { get; set; }
    [JsonPropertyName("Cards18")]
    public ReadWriteLockerProperty<object[][]>? Cards18 { get; set; }
    [JsonPropertyName("Cards19")]
    public ReadWriteLockerProperty<object[][]>? Cards19 { get; set; }

    public void SetDefaults()
    {
        LockerSecurityMode = new ReadWriteLockerProperty<LockerSecurityMode?>(Models.LockerSecurityMode.Installation);
        LockerSecuritySweep = new ReadWriteLockerProperty<int?>(0);

        var cardValue = new object[5][];
        for (var i = 0; i < Constants.LockerConfigCardBlockMaxCount; i++)
        {
            cardValue[i] = new object[3];
            cardValue[i][0] = Constants.EmptyCardString;
            cardValue[i][1] = 0;
            cardValue[i][2] = 0;
        }

        Cards0 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards1 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards2 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards3 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards4 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards5 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards6 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards7 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards8 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards9 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards10 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards11 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards12 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards13 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards14 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards15 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards16 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards17 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards18 = new ReadWriteLockerProperty<object[][]>(cardValue);
        Cards19 = new ReadWriteLockerProperty<object[][]>(cardValue);
    }

    public static Property OverwriteWith(Property toBeOverwritten, Property takesPrecedent)
    {
        Type propertyType = typeof(Property);

        var properties = propertyType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

        foreach (var property in properties)
        {
            var value = property.GetValue(takesPrecedent);
            if (value is not null)
                property.SetValue(toBeOverwritten, value);
        }

        return toBeOverwritten;
    }

    public static Property WithDefaults()
    {
        Property property = new();

        property.SetDefaults();

        return property;
    }
}