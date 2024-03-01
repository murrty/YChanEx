#nullable enable
#define ENABLE_8KUN
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
/// <summary>
/// This class contains methods for verification and minor usability.
/// </summary>
internal static class Chans {
    private static readonly Regex FourChanRegex = new(@"^https:\/\/(boards\.)?4chan(nel)?\.org\/[a-zA-Z0-9]+\/thread\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex SevenChanRegex = new(@"^https:\/\/7chan\.org\/[a-zA-Z0-9]+\/res\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex EightChanRegex = new(@"^https:\/\/8chan\.(moe|se|cc)\/[a-zA-Z0-9]+\/res\/\d+\.(html|json)", RegexOptions.IgnoreCase);
#if ENABLE_8KUN && !RELEASE
    private static readonly Regex EightKunRegex = new(@"^https:\/\/8kun\.top\/(?!(qresearch)|(qnotables)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening))[a-zA-Z0-9]+\/res\/\d+\.(html|json)", RegexOptions.IgnoreCase);
#endif
    private static readonly Regex FChanRegex = new(@"^https:\/\/fchan\.us\/[a-zA-Z0-9]+\/res\/\d+\.(html)", RegexOptions.IgnoreCase);
    private static readonly Regex U18ChanRegex = new(@"^https:\/\/u18chan\.com\/(board\/u18chan\/)?[a-zA-Z0-9]+\/topic\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex FoolFuukaRegex = new(@"^https:\/\/((arch\.b4k\.co)|((www\.)?(archived\.moe)|(desuarchive\.org)|(thebarchive\.com)))\/[a-zA-Z0-9_]+\/thread\/\d+", RegexOptions.IgnoreCase);

    /// <summary>
    /// Tries to verify a chan URL.
    /// </summary>
    /// <param name="Url">The Url that will be scanned.</param>
    /// <param name="ThreadData">The thread data associated with the chan, if it's valid and can be used.</param>
    /// <returns><see langword="true"/> if the chan url is valid and can be use; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetThreadData([NotNullWhen(true)] string? Url, [NotNullWhen(true)] out ThreadData? ThreadData) {
        if (Url.IsNullEmptyWhitespace()) {
            ThreadData = null;
            return false;
        }

        if (Url.StartsWith("ychanex:")) {
            Url = Url[8..];
        }
        if (Url.StartsWith("view-source:")) {
            Url = Url[12..];
        }
        Url = Networking.CleanURL(Url);

        if (FourChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.FourChan);
            return true;
        }

        if (SevenChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.SevenChan);
            return true;
        }

        if (EightChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1].SubstringBeforeLastChar('.'),
                ThreadBoard: URLSplit[^3],
                Url: Url.Replace(".json", ".html", StringComparison.OrdinalIgnoreCase),
                Type: ChanType.EightChan);
            return true;
        }

        //8kun is dead, this check disables it from being used. May re-enable in the future.
#if ENABLE_8KUN && !RELEASE
        if (EightKunRegex.IsMatch(Url)) {
            if (StupidFuckingBoard(ChanType.EightKun, Url)) {
                //Log.Write("This program doesn't support archiving boards with content that is considered highly fucking stupid.");
                System.Windows.Forms.MessageBox.Show("This program doesn't support archiving boards with content that is considered highly fucking stupid.", "YChanEx");
                ThreadData = null;
                return false;
            }

            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1].SubstringBeforeLastChar('.'),
                ThreadBoard: URLSplit[^3],
                Url: Url.Replace(".json", ".html", StringComparison.OrdinalIgnoreCase),
                Type: ChanType.EightKun);
            return true;
        }
#endif

        if (FChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: "http" + Url[5..],
                Type: ChanType.fchan);
            return true;
        }
        if (U18ChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.u18chan);
            return true;
        }

        if (FoolFuukaRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.FoolFuuka);
            return true;
        }

        ThreadData = null;
        return false;
    }
    /// <summary>
    /// Whether the input <paramref name="Data"/> is supported by the program.
    /// </summary>
    /// <param name="Data">The data to reverify.</param>
    /// <returns></returns>
    public static bool ReverifyThreadData(ThreadData Data) {
        if (FourChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.FourChan;
            return true;
        }
        if (SevenChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.SevenChan;
            return true;
        }
        if (EightChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.EightChan;
            return true;
        }

        /* 8kun is dead, this check disables it from being used. May re-enable in the future.
        if (EightKunRegex.IsMatch(Data.Url)) {
            if (StupidFuckingBoard(ChanType.EightKun, Data.Url)) {
                //Log.Write("This program doesn't support archiving boards with content that is considered highly fucking stupid.");
                System.Windows.Forms.MessageBox.Show("This program doesn't support archiving boards with content that is considered highly fucking stupid.", "YChanEx");
                return false;
            }
            Data.ChanType = ChanType.EightKun;
            return true;
        }
        */

        if (FChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.fchan;
            return true;
        }
        if (U18ChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.u18chan;
            return true;
        }

        if (FoolFuukaRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.FoolFuuka;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the full chan title from the board id.
    /// </summary>
    /// <param name="Thread">Thread info to derive the board name from.</param>
    /// <param name="OverrideOrDescription">If it should override the settings check for HTML, or if it's obtaining the info from the description</param>
    /// <returns>The string value of the title. If none is parse, it'll return the input board.</returns>
    public static string GetFullBoardName(ThreadInfo Thread, bool OverrideOrDescription = false) {
        return Thread.Chan switch {
            ChanType.FourChan => Parsers.FourChan.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            ChanType.FourTwentyChan => Parsers.FourTwentyChan.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            ChanType.SevenChan => Parsers.SevenChan.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            ChanType.EightChan => Parsers.EightChan.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            ChanType.EightKun => Parsers.EightKun.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            ChanType.fchan => Parsers.FChan.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            ChanType.u18chan => Parsers.U18Chan.GetFullBoardName(Thread.Data.Board, OverrideOrDescription),
            _ => Thread.Data.Board
        };
    }

    [System.Diagnostics.DebuggerStepThrough]
    public static bool StupidFuckingBoard(ChanType Chan, string URL) {
        return Chan switch {
            ChanType.EightKun => Regex.IsMatch(URL,
                "(qresearch)|(qnotables)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening)"),
            _ => false
        };
    }

    internal static string? SearchMatch(string input, string prefix, string suffix) {
        Match M = new Regex($"(?<={prefix}).*?(?={suffix})").Match(input);
        if (!M.Success) {
            return null;
        }
        return M.Value;
    }
    internal static string? SearchMatch(string input, string prefix, string suffix, bool escape) {
        Match M = escape ?
            new Regex($"(?<={Regex.Escape(prefix)}).*?(?={Regex.Escape(suffix)})").Match(input) :
            new Regex($"(?<={prefix}).*?(?={suffix})").Match(input);
        if (!M.Success) {
            return null;
        }
        return M.Value;
    }
    internal static string[] SearchMatches(string input, string prefix, string suffix) {
        MatchCollection M = new Regex($"(?<={prefix}).*?(?={suffix})").Matches(input);
        if (M.Count < 1) {
            return [];
        }
        return M.Cast<Match>()
            .Select(x => x.Value)
            .ToArray();
    }
    internal static string[] SearchMatches(string input, string prefix, string suffix, bool escape) {
        MatchCollection M = escape ?
            new Regex($"(?<={Regex.Escape(prefix)}).*?(?={Regex.Escape(suffix)})").Matches(input) :
            new Regex($"(?<={prefix}).*?(?={suffix})").Matches(input);
        if (M.Count < 1) {
            return [];
        }
        return M.Cast<Match>()
            .Select(x => x.Value)
            .ToArray();
    }
}
