namespace YChanEx; 
internal static class Extensions {
    public static bool Contains<T>(this IList<T> list, T value, out int Index) {
        for (int i = 0; i < list.Count; i++) {
            if (list[i].Equals(value)) {
                Index = i;
                return true;
            }
        }
        Index = -1;
        return false;
    }
    public static bool IsNullEmptyWhitespace(this string value) {
        if (value is null || value.Length == 0)
            return true;

        value = value.ReplaceWhitespace("");
        if (value.Length == 0)
            return true;

        while (value[^1] == ' ')
            value = value[..^1];

        while (value[0] == ' ')
            value = value[1..];

        return value.Length == 0;
    }
    public static string ReplaceWhitespace(this string str, string replacement = " ") => System.Text.RegularExpressions.Regex.Replace(str, @"\s+", replacement, System.Text.RegularExpressions.RegexOptions.Compiled);
}