#nullable enable
namespace YChanEx.Parsers;
using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class SevenChan {
    //var chan7 = HtmlDocument.FromFile("X:\\Temp\\7c.html", HtmlParseOptions.RemoveEmptyTextNodes);
    //var sel = Selector.ParseSelector("div[id:=\"^\\d+$\"][class=\"post\"] > div[class=\"post_header\"]");
    //var nodes7c = chan7.Find(sel).ToArray(); // 56
    //var posts = chan7.Find("div[id:=\"^\\d+$\"][class=\"post\"]")
    //    .ToArray();

    private static readonly Selector PostSelector = Selector.ParseSelector("div[id:=\"^\\d+$\"][class=\"post\"]");

    public static SevenChanPost[] Generate(string html) {
        HtmlDocument htDoc = HtmlDocument.FromHtml(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);
        var PostNodes = htDoc.Find(PostSelector)
            .ToArray();

        if (PostNodes == null) {
            throw new NullReferenceException($"Could not find '{nameof(PostNodes)}'.");
        }

        if (PostNodes.Length == 0) {
            return [];
        }

        var array = new SevenChanPost[PostNodes.Length];
        for (int i = 0; i < PostNodes.Length; i++) {
            //if (i == 14) {
            //    Console.WriteLine();
            //}
            //Console.WriteLine(i);
            array[i] = new(PostNodes[i]);
        }

        return array;
    }
    public static async Task<SevenChanPost[]> GenerateAsync(string html) {
        HtmlDocument htDoc = await HtmlDocument.FromHtmlAsync(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);
        var PostNodes = htDoc.Find(PostSelector)
            .ToArray();

        if (PostNodes == null) {
            throw new NullReferenceException($"Could not find '{nameof(PostNodes)}'.");
        }

        if (PostNodes.Length == 0) {
            return [];
        }

        var array = new SevenChanPost[PostNodes.Length];
        await Task.Run(() => {
            for (int i = 0; i < PostNodes.Length; i++) {
                //if (i == 14) {
                //    Console.WriteLine();
                //}
                //Console.WriteLine(i);
                array[i] = new(PostNodes[i]);
            }
        });

        return array;
    }

    internal static long ConvertSizeToBytes(string size) {
        const int ByteMultiplier = 1024;

        if (size.EndsWith("kb", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^2], out var dbl);
            return (long)Math.Round(dbl * ByteMultiplier, MidpointRounding.ToEven);
        }

        if (size.EndsWith("mb", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^2], out var dbl);
            return (long)Math.Round(dbl * ByteMultiplier * ByteMultiplier, MidpointRounding.ToEven);
        }

        if (size.EndsWith("gb", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^2], out var dbl);
            return (long)Math.Round(dbl * ByteMultiplier * ByteMultiplier * ByteMultiplier, MidpointRounding.ToEven);
        }

        if (size.EndsWith("b", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^1], out var dbl);
            return (long)Math.Round(dbl, MidpointRounding.ToEven);
        }

        throw new ArgumentException("Invalid size format");
    }
    internal static DateTimeOffset ConvertTimestampToDateTime(string timestamp) {
        //YYYY/mm/DD hh:MM:ss
        string[] splits = ("20" + timestamp[..timestamp.IndexOf('(')] + " " + timestamp[(timestamp.LastIndexOf(')') + 1)..]).Split(' ');

        // YYYY/mm/DD
        string[] splits2 = splits[0].Split('/');
        int year = int.Parse(splits2[0]);
        int month = int.Parse(splits2[1]);
        int day = int.Parse(splits2[2]);

        // hh:MM
        splits2 = splits[1].Split(':');
        int hour = int.Parse(splits2[0]);
        int minute = int.Parse(splits2[1]);
        const int second = 0;

        // UTC + 1:00
        return new DateTimeOffset(year, month, day, hour, minute, second, new TimeSpan(1, 0, 0));
        //return DateTime.Parse(str);
    }
    internal static Size ConvertDimensionsToSize(string size) {
        string[] Dimensions = size.Split('x', 'X');
        return new Size(int.Parse(Dimensions[0]),
            int.Parse(Dimensions[1]));
    }
    internal static string? GetMessage(HtmlElementNode? node) {
        if (node?.Children.Count > 0) {
            if (node.Children[^1] is HtmlElementNode cnode && cnode.TagName.Equals("br", StringComparison.OrdinalIgnoreCase)) {
                node.Children.RemoveAt(node.Children.Count - 1);
            }
            for (int i = 0; i < node.Children.Count; i++) {
                if (node.Children[i] is HtmlElementNode enode) {
                    if (enode.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)) {
                        enode.Attributes.Remove("class");
                        var newHref = enode.Attributes["href"]?.Value;
                        if (newHref != null) {
                            enode.Attributes["href"]!.Value = "#p" + newHref[(newHref.LastIndexOf('#') + 1)..];
                            enode.Attributes.Add(new HtmlAttribute("class", "quotelink"));
                        }
                    }
                }
            }
            if (!node.InnerHtml.IsNullEmptyWhitespace()) {
                return node.InnerHtml;
            }
        }
        return null;
    }

    public static string TranslateMessage(string Message, ThreadInfo CurThr) {
        if (!string.IsNullOrWhiteSpace(Message)) {
            while (Message.EndsWith("<br />")) {
                Message = Message[..^6];
            }

            Regex Reply = new($"\\<a href=\"\\/{CurThr.Data.Board}\\/res\\/{CurThr.Data.Id}.html#([0-9]+)\" class=\"ref\\|{CurThr.Data.Board}\\|{CurThr.Data.Id}\\|([0-9]+)\"\\>");

            MatchCollection Matches = Reply.Matches(Message);

            if (Matches.Count > 0) {
                for (int i = 0; i < Matches.Count; i++) {
                    Message = Message
                        .Replace(Matches[i].Value, $"<a href=\"#p{Matches[i].Value[(Matches[i].Value.LastIndexOf("|") + 1)..Matches[i].Value.LastIndexOf("\"")]}\">");
                }
            }

            //Regex Quotes = new(">>([0-9]+)");
            //MatchCollection Matches = Quotes.Matches(Message);

            //if (Matches.Count > 0) {
            //    for (int i = 0; i < Matches.Count; i++) {
            //        Message = Message.Replace(Matches[i].Value, $"<a href=\"#p{Matches[i].Value[2..]}\">{Matches[i].Value}</a><br />");
            //    }
            //}
        }

        return Message;
    }
}
