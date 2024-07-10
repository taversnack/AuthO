using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.Common.Data;

// NOTE: [0] Can remove string conversion attribute in production for minor network optimization
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortOrder
{
    Ascending,
    Descending,
}