#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
internal static class Extensions {
    public static bool IsNullEmptyWhitespace([NotNullWhen(false)] this string? value) {
        if (string.IsNullOrEmpty(value))
            return true;

        value = value!.ReplaceWhitespace("");
        if (value.Length == 0)
            return true;

        while (value[^1] == ' ')
            value = value[..^1];

        while (value[0] == ' ')
            value = value[1..];

        return value.Length == 0;
    }
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> a) {
        foreach (T item in enumerable)
            a(item);
    }

    public static string? RemoveFromStart(this string? input, string value) {
        if (!input.IsNullEmptyWhitespace() && input.StartsWith(value)) {
            return input[value.Length..];
        }
        return input;
    }
    public static string? RemoveFromStart(this string? input, string value, StringComparison comparison) {
        if (!input.IsNullEmptyWhitespace() && input.StartsWith(value, comparison)) {
            return input[value.Length..];
        }
        return input;
    }
    public static string? RemoveFromEnd(this string? input, string value) {
        if (!input.IsNullEmptyWhitespace() && input.EndsWith(value)) {
            return input[..^value.Length];
        }
        return input;
    }
    public static string? RemoveFromEnd(this string? input, string value, StringComparison comparison) {
        if (!input.IsNullEmptyWhitespace() && input.EndsWith(value, comparison)) {
            return input[..^value.Length];
        }
        return input;
    }

    public static string ReplaceWhitespace(this string str, string replacement = " ") => System.Text.RegularExpressions.Regex.Replace(str, @"\s+", replacement, System.Text.RegularExpressions.RegexOptions.Compiled);
}