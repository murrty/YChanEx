#nullable enable
namespace YChanEx;
using SoftCircuits.HtmlMonkey;
/// <summary>
/// Contains usability methods governing local files.
/// </summary>
internal static class FileHandler {
    /// <summary>
    /// The Dictionary of illegal file name characters.
    /// </summary>
    private static readonly Dictionary<string, string> IllegalCharacters = new() {
        { "\\", "_" },
        { "/",  "_" },
        { ":",  "_" },
        { "*",  "_" },
        { "?",  "_" },
        { "\"", "_" },
        { "<",  "_" },
        { ">",  "_" },
        { "|",  "_" }
    };

    /// <summary>
    /// Replaces the illegal file name characters in a string.
    /// </summary>
    /// <param name="Input">The string to replace bad characters.</param>
    /// <returns>The string with the illegal characters filtered out.</returns>
    public static string ReplaceIllegalCharacters(string Input) {
        return IllegalCharacters.Aggregate(Input, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
    }

    /// <summary>
    /// Replaces the illegal file name characters in a string.
    /// </summary>
    /// <param name="Input">The string to replace bad characters.</param>
    /// <param name="ReplacementCharacter">The <see cref="string"/> replacement character to replace it with.</param>
    /// <returns>The string with the illegal characters filtered out.</returns>
    public static string ReplaceIllegalCharacters(string Input, string ReplacementCharacter) {
        return IllegalCharacters.Aggregate(Input, (current, replacement) => current.Replace(replacement.Key, ReplacementCharacter));
    }

    /// <summary>
    /// Generates a short 64-char thread name for the title and main form for easier identification.
    /// </summary>
    /// <param name="Subtitle">The subtitle of the post, IE the main thing about it. Optional.</param>
    /// <param name="Comment">The main post text, containing the message in the post.</param>
    /// <param name="FallbackName">The fallback name that will be used if the new name isn't a usable name.</param>
    /// <returns>If either the subtitle or comment contain text, returns them either grouped or solo; otherwise, the thread ID.</returns>
    public static string GetShortThreadName(string? Subtitle, string? Comment, string FallbackName) {
        string NewName = string.Empty;

        if (string.IsNullOrEmpty(FallbackName)) {
            FallbackName = "No fallback name";
        }

        if (Subtitle is not null) {
            NewName = Subtitle = Subtitle.Trim();
        }

        if (Comment is not null) {
            NewName += (Subtitle?.Length > 0 && Comment.Length > 0 ? " - " : "") + Comment.Trim();
        }

        if (NewName.Length > 0) {
            NewName = NewName
                .Replace("<br><br>", " ") // New lines
                .Replace("<br>", " ")
                .Replace("<wbr>", "") // Weird inserts between URLs
                .Replace("<span class=\"quote\">", "") // >implying text xd
                .Replace("</span>", "") // close of >implying text xd
                                        //.Replace("&gt;", ">")  // These are fixed by WebUtility.HtmlDecode.
                                        //.Replace("&lt;", "<")  // But I'm keeping them commented.
                                        //.Replace("&amp;", "&") // Just in case.
                .Replace("</a>", "") // the end of any quote-link urls.
                .Replace("\n", " ")
                .Trim(); // Cleans up any trailing spaces, new-line and the windows \n, too.

            NewName = System.Text.RegularExpressions.Regex.Replace(NewName, "<a href=\\\"(.*?)\\\" class=\\\"quotelink\\\">", "");
            NewName = HtmlUtility.Decode(NewName); //Uri.UnescapeDataString(NewName);

            if (NewName.Length > 64) {
                NewName = NewName[..64];
            }

            NewName = NewName.Trim();
        }

        return NewName.Length > 0 ? NewName : FallbackName;
    }
}