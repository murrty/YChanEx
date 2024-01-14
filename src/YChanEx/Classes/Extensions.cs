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
        foreach (T item in enumerable) {
            a(item);
        }
    }
    public static void For<T>(this IList<T> values, Action<T> a) {
        for (int i = 0; i < values.Count; i++) {
            a(values[i]);
        }
    }
    public static void For<T>(this T[] values, Action<T> a) {
        for (int i = 0; i < values.Length; i++) {
            a(values[i]);
        }
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

    public static string Format(this string value, params object[] vals) {
        return string.Format(value, vals);
    }
    public static string[] SplitRemoveEmpty(this string value, params char[] separator) {
        return value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string SubstringBeforeChar(this string input, char ch) {
        for (int i = 0; i < input.Length; i++) {
            if (input[i] == ch)
                return input[..i];
        }
        return input;
    }
    public static string SubstringAfterChar(this string input, char ch) {
        for (int i = 0; i < input.Length; i++) {
            if (input[i] == ch)
                return input[(i + 1)..];
        }
        return input;
    }
    public static string SubstringBeforeLastChar(this string input, char ch) {
        for (int i = input.Length - 1; i > -1; i--) {
            if (input[i] == ch)
                return input[..i];
        }
        return input;
    }
    public static string SubstringAfterLastChar(this string input, char ch) {
        for (int i = input.Length - 1; i > -1; i--) {
            if (input[i] == ch)
                return input[(i + 1)..];
        }
        return input;
    }
    public static string SubstringBeforeChar(this string input, char ch, out int index) {
        for (int i = 0; i < input.Length; i++) {
            if (input[i] == ch) {
                index = i;
                return input[..i];
            }
        }
        index = -1;
        return input;
    }
    public static string SubstringAfterChar(this string input, char ch, out int index) {
        for (int i = 0; i < input.Length; i++) {
            if (input[i] == ch) {
                index = i;
                return input[(i + 1)..];
            }
        }
        index = -1;
        return input;
    }
    public static string SubstringBeforeLastChar(this string input, char ch, out int index) {
        for (int i = input.Length - 1; i > -1; i--) {
            if (input[i] == ch) {
                index = i;
                return input[..i];
            }
        }
        index = -1;
        return input;
    }
    public static string SubstringAfterLastChar(this string input, char ch, out int index) {
        for (int i = input.Length - 1; i > -1; i--) {
            if (input[i] == ch) {
                index = i;
                return input[(i + 1)..];
            }
        }
        index = -1;
        return input;
    }

    public static int Clamp(this int val, int min, int max) {
        if (val < min) {
            return min;
        }
        if (val > max) {
            return max;
        }
        else {
            return val;
        }
    }
    public static long Clamp(this long val, long min, long max) {
        if (val < min) {
            return min;
        }
        if (val > max) {
            return max;
        }
        else {
            return val;
        }
    }
}