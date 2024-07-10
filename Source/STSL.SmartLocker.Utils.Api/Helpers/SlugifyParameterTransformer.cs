using System.Text.RegularExpressions;

namespace STSL.SmartLocker.Utils.Api.Helpers;

internal sealed partial class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
        => value is string url ? Slugify().Replace(url, "$1-$2").ToLowerInvariant() : null;

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex Slugify();
}
