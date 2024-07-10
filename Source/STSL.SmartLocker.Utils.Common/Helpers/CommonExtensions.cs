using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace STSL.SmartLocker.Utils.Common.Helpers;

public static partial class CommonExtensions
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? enumerable) => enumerable is null || !enumerable.Any();
    public static bool RemoveFirst<T>([NotNullWhen(true)] this List<T> list, Predicate<T> predicate)
    {
        var matchedIndex = list.FindIndex(predicate);
        if(matchedIndex != -1)
        {
            list.RemoveAt(matchedIndex);
            return true;
        }
        return false;
    }
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    public static bool IsValidHexadecimal(this string hexString, bool allowStartingWith0x = false)
        => allowStartingWith0x && hexString.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ?
            HexadecimalRegex().IsMatch(hexString[2..]) :
            HexadecimalRegex().IsMatch(hexString);

    [GeneratedRegex("^[0-9a-fA-F]$")]
    private static partial Regex HexadecimalRegex();

    public static long ReverseHexStringEndiannessAndConvertToLong(this string hexString)
    {
        var reversedHexString = Enumerable
            .Range(0, hexString.Length / 2)
            .Select(i => hexString.Substring(i * 2, 2))
            .Reverse()
            .Aggregate(string.Empty, (current, next) => current + next);

        return Convert.ToInt64(reversedHexString, 16);
    }
}
