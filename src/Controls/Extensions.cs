#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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

    public static string ReplaceFirst(this string str, string oldValue, string replacement) {
        int pos = str.IndexOf(oldValue);
        if (pos < 0) {
            return str;
        }
        return str[..pos] + replacement + str[(pos + oldValue.Length)..];
    }
    public static string ReplaceFirst(this string str, string oldValue, string replacement, int startIndex) {
        int pos = str.IndexOf(oldValue, startIndex);
        if (pos < 0) {
            return str;
        }
        return str[..pos] + replacement + str[(pos + oldValue.Length)..];
    }
    public static string ReplaceLast(this string str, string oldValue, string replacement) {
        int pos = str.LastIndexOf(oldValue);
        if (pos < 0) {
            return str;
        }
        return str[..pos] + replacement + str[(pos + oldValue.Length)..];
    }
    public static string ReplaceLast(this string str, string oldValue, string replacement, int startIndex) {
        int pos = str.LastIndexOf(oldValue, startIndex);
        if (pos < 0) {
            return str;
        }
        return str[..pos] + replacement + str[(pos + oldValue.Length)..];
    }

    /// <summary>
    /// Returns a new string in which all occurrences of a specified string in the current
    /// instance are replaced with another specified string.
    /// </summary>
    /// <param name="str">The string which will have the replacements.</param>
    /// <param name="oldValue">The string to be replaced.</param>
    /// <param name="newValue">The string to replace all occurrences of oldValue</param>
    /// <param name="comparisonType">How the strings will be compared.</param>
    /// <returns>
    /// A string that is equivalent to the current string except that all instances of
    /// oldValue are replaced with newValue. If oldValue is not found in the current
    /// instance, the method returns the current instance unchanged.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string Replace(this string str, string oldValue, string? newValue, StringComparison comparisonType) {
        // Thanks to someone on StackOverflow.
        // Check inputs.
#if NETCOREAPP
        ArgumentNullException.ThrowIfNull(str);
        ArgumentException.ThrowIfNullOrEmpty(oldValue);
#else
        if (str == null) {
            // Same as original .NET C# string.Replace behavior.
            throw new ArgumentNullException(nameof(str));
        }
        if (oldValue is null) {
            // Same as original .NET C# string.Replace behavior.
            throw new ArgumentNullException(nameof(oldValue));
        }
        if (oldValue.Length == 0) {
            // Same as original .NET C# string.Replace behavior.
            throw new ArgumentException("String cannot be of zero length.");
        }
#endif
        if (str.Length == 0) {
            // Same as original .NET C# string.Replace behavior.
            return str;
        }
        if (oldValue == newValue) {
            return str;
        }

        // Prepare string builder for storing the processed string.
        // Note: StringBuilder has a better performance than String by 30-40%.
        StringBuilder resultStringBuilder = new(str.Length);

        // Analyze the replacement: replace or remove.
        bool isReplacementNullOrEmpty = string.IsNullOrWhiteSpace(newValue);

        // Replace all values.
        const int valueNotFound = -1;
        int foundAt;
        int startSearchFromIndex = 0;
        while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound) {
            // Append all characters until the found replacement.
            int charsUntilReplacment = foundAt - startSearchFromIndex;
            bool isNothingToAppend = charsUntilReplacment == 0;
            if (!isNothingToAppend) {
                resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacment);
            }

            // Process the replacement.
            if (!isReplacementNullOrEmpty) {
                resultStringBuilder.Append(newValue);
            }

            // Prepare start index for the next search.
            // This needed to prevent infinite loop, otherwise method always start search 
            // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
            // and comparisonType == "any ignore case" will conquer to replacing:
            // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
            startSearchFromIndex = foundAt + oldValue.Length;
            if (startSearchFromIndex == str.Length) {
                // It is end of the input string: no more space for the next search.
                // The input string ends with a value that has already been replaced. 
                // Therefore, the string builder with the result is complete and no further action is required.
                return resultStringBuilder.ToString();
            }
        }

        // Append the last part to the result.
        int charsUntilStringEnd = str.Length - startSearchFromIndex;
        resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

        return resultStringBuilder.ToString();
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