﻿#nullable enable
namespace YChanEx.Parsers;
using System.IO;
using System.Net.Http;
using System.Threading;
using murrty.classes;
using YChanEx.Posts;
internal static class EightKun {
    private const string BoardsUrl = "https://8kun.top/boards.json";
    private static EightKunBoard[] Boards = [];

    public static DateTimeOffset GetPostTime(EightKunPost post) {
        return DateTimeOffset.FromUnixTimeSeconds(post.time);
    }
    public static string CleanMessage(string? message) {
        if (message.IsNullEmptyWhitespace()) {
            return string.Empty;
        }

        System.Text.RegularExpressions.Regex RegEx = new("<p class=\\\"body-line ltr quote\\\">.*?<\\/p>");
        System.Text.RegularExpressions.Match RegexMatch = RegEx.Match(message);
        bool QuoteMatch = RegexMatch.Success;
        while (QuoteMatch) {
            string ReplacementString = RegexMatch.Value
                .Replace("<p class=\"body-line ltr quote\">", "<span class=\"quote\">")[..^4] + "</span><br />";
            message = message.Replace(RegexMatch.Value, ReplacementString);

            RegexMatch = RegEx.Match(message);
            QuoteMatch = RegexMatch.Success;
        }
        message = System.Text.RegularExpressions.Regex.Replace(message, "", "");

        message = message
            .Replace("<p class=\"body-line ltr \">", "")
            .Replace("</p>", "<br />")
            .Replace("<p class=\"body-line empty \"/>", "")
            .Replace("<p class=\"body-line empty \">", "")
            .TrimEnd();

        while (message.EndsWith("<br />")) {
            message = message[..^6].TrimEnd();
        }

        return message;
    }

    public static async Task<EightKunBoard?> GetBoardAsync(ThreadInfo Thread, HttpClient DownloadClient, CancellationToken token) {
        string CacheFile = Path.Combine(Downloads.DownloadPath, "8kun", "boardcache.json");
        if (File.Exists(CacheFile)) {
            try {
                var Deserialized = File.ReadAllText(CacheFile).JsonDeserialize<EightKunBoard[]?>()!;
                if (Deserialized != null) {
                    Boards = Deserialized;
                    EightKunBoard? FoundBoard = GetBoard(Thread);
                    if (FoundBoard != null) {
                        return FoundBoard;
                    }
                }
                Log.Write("Could not find board in board cache, re-downloading...");
            }
            catch {
                Log.Write("Could not load cache file, re-downloading...");
            }
        }

        HttpRequestMessage Request = new(HttpMethod.Get, BoardsUrl);
        using var Response = await Networking.GetResponseAsync(Request, DownloadClient, token);
        if (Response == null) {
            Log.Write("Could not get board info.");
            return null;
        }

        var BoardsString = await Networking.GetStringAsync(Response, token);
        var NewBoards = BoardsString.JsonDeserialize<EightKunBoard[]>();

        if (NewBoards == null || NewBoards.Length < 1) {
            Log.Write("Could not update boards cache.");
            return null;
        }

        NewBoards = FilterBoards(NewBoards);
        File.WriteAllText(CacheFile, NewBoards.JsonSerialize());
        Boards = NewBoards;
        return GetBoard(Thread);
    }
    private static EightKunBoard? GetBoard(ThreadInfo Thread) {
        return Boards?.Length > 0 ?
            Array.Find(Boards, x => x.uri?.Equals(Thread.Data.Board, StringComparison.InvariantCultureIgnoreCase) == true) :
            null;
    }
    private static EightKunBoard[] FilterBoards(EightKunBoard[] Boards) {
        // seethe
        return Boards.Where(x =>
            x.uri?.Equals("qresearch", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("qnotables", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("pnd", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("midnightriders", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("qrb", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("philogeometric", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("qsocial", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("qrnews", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("thestorm", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("patriotsfight", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("projectdcomms", StringComparison.InvariantCultureIgnoreCase) != true &&
            x.uri?.Equals("greatawakening", StringComparison.InvariantCultureIgnoreCase) != true)
        .ToArray();
    }
}
