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

    public static string RemoveWhitespace(this string str) {
        return ReplaceWhitespace(str, string.Empty);
    }
    public static string ReplaceWhitespace(this string str, string replacement = " ") {
        return System.Text.RegularExpressions.Regex.Replace(str, @"\s+", replacement, System.Text.RegularExpressions.RegexOptions.Compiled);
    }

    public static string UnlessNull([MaybeNull] this string? value, string other) {
        return value ?? other;
    }
    public static string UnlessNullEmpty([MaybeNull] this string? value, string other) {
        if (value is null || value.Length < 1) {
            return other;
        }
        return value;
    }
    public static string UnlessNullEmptyWhiteSpace([MaybeNull] this string? value, string other) {
        if (value.IsNullEmptyWhitespace()) {
            return other;
        }
        return value;
    }
    public static string FirstNonNullEmptyWhiteSpace(this string?[] values, bool ReturnNullOnNone = false) {
        for (int i = 0; i < values.Length; i++) {
            string? v = values[i];
            if (!v.IsNullEmptyWhitespace()) {
                return v;
            }
        }
        if (!ReturnNullOnNone) {
            throw new NullReferenceException("No non-null, empty, or whitespace values found.");
        }
        return null!;
    }
}