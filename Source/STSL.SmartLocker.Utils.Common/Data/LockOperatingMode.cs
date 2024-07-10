using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Common.Data;

// NOTE: [0] Can remove string conversion attribute in production for minor network optimization
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LockOperatingMode
{
    Installation = 0,
    Shared = 1,
    Dedicated = 2,
    Confiscated = 3,
    Reader = 4,
}
