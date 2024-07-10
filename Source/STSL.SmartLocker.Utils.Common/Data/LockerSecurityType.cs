namespace STSL.SmartLocker.Utils.Common.Data;

// NOTE: [0] Can remove string conversion attribute in production for minor network optimization
//[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LockerSecurityType
{
    None = 0,
    CombinationLock = 1,
    KeyLock = 2,
    SmartLock = 3
}