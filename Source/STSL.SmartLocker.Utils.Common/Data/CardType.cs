using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Common.Data;

// NOTE: [0] Can remove string conversion attribute in production for minor network optimization
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardType
{
    Unregistered = 0,
    User = 1,
    Welcome = 2,
    Master = 3,
    Security = 4,
    Tag = 5,
    Temporary = 6,
}