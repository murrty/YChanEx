#nullable enable
namespace YChanEx.Parsers;
using System.IO;
using System.Net.Http;
using System.Threading;
using murrty.classes;
using YChanEx.Posts;
internal static class EightChan {
    public static Dictionary<string, EightChanBoard> BoardSubtitles = [];

    public static async Task<EightChanBoard?> GetBoardAsync(string boardId, HttpClient DownloadClient, CancellationToken token) {
        string CacheDir = Path.Combine(Downloads.DownloadPath, "8chan");
        string CacheFile = Path.Combine(CacheDir, "boardcache.json");
        if (BoardSubtitles.Count < 1) {
            if (File.Exists(CacheFile)) {
                try {
                    var Deserialized = File.ReadAllText(CacheFile).JsonDeserialize<Dictionary<string, EightChanBoard>?>()!;
                    if (Deserialized != null) {
                        BoardSubtitles = Deserialized;
                        if (BoardSubtitles.TryGetValue(boardId, out var board) && board != null) {
                            return board;
                        }
                    }
                    Log.Warn("Could not find board in board cache, re-downloading...");
                }
                catch {
                    Log.Warn("Could not load cache file, re-downloading...");
                }
            }
        }
        else if (BoardSubtitles.TryGetValue(boardId, out var board) && board != null) {
            return board;
        }

        HttpRequestMessage Request = new(HttpMethod.Get, "https://8chan.moe/" + boardId + "/");
        Request.Headers.Referrer = new Uri("https://8chan.moe/");

        using var Response = await Networking.GetResponseAsync(Request, DownloadClient, token);
        if (Response == null) {
            Log.Warn("Could not get board info.");
            return null;
        }

        var BoardString = await Networking.GetStringAsync(Response, token);
        var Board = EightChanBoard.ExtractBoard(BoardString);
        if (Board == null) {
            return null;
        }

        Board.BoardId = boardId;
        BoardSubtitles[boardId] = Board;
        Directory.CreateDirectory(CacheDir);
        File.WriteAllText(CacheFile, BoardSubtitles.JsonSerialize());
        return Board;
    }

    public static DateTimeOffset GetPostTime(EightChanThread Thread) {
        return PostTimeInternal(Thread.creation!);
    }
    public static DateTimeOffset GetPostTime(EightChanPost Post) {
        return PostTimeInternal(Post.creation!);
    }
    public static DateTimeOffset GetPostTime(string str) {
        return PostTimeInternal(str);
    }
    private static DateTimeOffset PostTimeInternal(string creation) {
        return System.Text.RegularExpressions.Regex.IsMatch(creation, "^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}.[0-9]{3}Z$") ?
            DateTimeOffset.ParseExact(creation, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal) :
            default;
    }

    public static string CleanMessage(EightChanThread Thread, ThreadInfo ThreadInfo) {
        return CleanMessageInternal(Thread.markdown, ThreadInfo);
    }
    public static string CleanMessage(EightChanPost Post, ThreadInfo ThreadInfo) {
        return CleanMessageInternal(Post.markdown, ThreadInfo);
    }
    private static string CleanMessageInternal(string? message, ThreadInfo Thread) {
        if (string.IsNullOrWhiteSpace(message)) {
            return string.Empty;
        }

        return message!
            .Replace("\n", "<br />")
            .Replace($"<a class=\"quoteLink\" href=\"/{Thread.Data.Board}/res/{Thread.Data.Id}.html#",
                "<a class=\"quoteLink\" href=\"#p");
    }

    public static ulong[]? GetQuotedPosts(EightChanThread Thread) {
        return GetQuotedPosts(Thread.markdown);
    }
    public static ulong[]? GetQuotedPosts(EightChanPost Post) {
        return GetQuotedPosts(Post.markdown);
    }
    private static ulong[]? GetQuotedPosts(string? markdown) {
        if (markdown.IsNullEmptyWhitespace()) {
            return null;
        }

        var Matches = Parsers.Helpers.ParsersShared.RepliesHtmlRegex.Matches(markdown);
        if (Matches.Count < 1) {
            return null;
        }

        return Matches
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(x => x.Value[(x.Value.LastIndexOf('#') + 1)..^1])
            .Select(ulong.Parse)
            .Distinct()
            .ToArray();
    }
}
