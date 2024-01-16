#nullable enable
namespace YChanEx.Parsers;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class U18Chan {
    private static readonly Selector FirstPostSelector = Selector.ParseSelector("div[id=\"FirstPost\"]");
    private static readonly Selector RepliesSelector = Selector.ParseSelector("table[class=\"ReplyBoxTable\"]");

    public static U18ChanPost[] Generate(string html) {
        HtmlDocument htDoc = HtmlDocument.FromHtml(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);
        var FirstPostNode = htDoc.Find(FirstPostSelector)
            .FirstOrDefault();

        if (FirstPostNode == null) {
            throw new NullReferenceException($"Could not find '{nameof(FirstPostNode)}'.");
        }

        U18ChanPost FirstPost = new(FirstPostNode, true);

        var ReplyNodes = htDoc.Find(RepliesSelector)
            .ToArray();

        if (ReplyNodes.Length == 0) {
            return [ FirstPost ];
        }

        var array = new U18ChanPost[ReplyNodes.Length + 1];
        array[0] = FirstPost;
        for (int i = 0; i < ReplyNodes.Length; i++) {
            array[i + 1] = new(ReplyNodes[i], false);
        }

        return array;
    }
    public static async Task<U18ChanPost[]?> GenerateAsync(string html) {
        HtmlDocument htDoc = await HtmlDocument.FromHtmlAsync(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);

        return await Task.Run(() => {
            var FirstPostNode = htDoc.Find(FirstPostSelector)
                .FirstOrDefault();

            if (FirstPostNode == null) {
                throw new NullReferenceException($"Could not find '{nameof(FirstPostNode)}'.");
            }

            var FirstPost = new U18ChanPost(FirstPostNode, true);

            var ReplyNodes = htDoc.Find(RepliesSelector)
                .ToArray();

            if (ReplyNodes.Length == 0) {
                return [ FirstPost ];
            }

            var array = new U18ChanPost[ReplyNodes.Length + 1];
            array[0] = FirstPost;
            for (int i = 0; i < ReplyNodes.Length; i++) {
                array[i + 1] = new(ReplyNodes[i], false);
            }

            return array;
        });
    }
    public static U18ChanPost[]? TryGenerate(string html) {
        try {
            return Generate(html);
        }
        catch {
            return null;
        }
    }

    public static string? GetHtmlTitle(ThreadData data) {
        if (data.ThreadName == null) {
            return null;
        }
        return GetHtmlTitle(data.Board, data.ThreadName);
    }
    public static string GetHtmlTitle(string board, string name) {
        return $"/{board}/ - {name} - U-18Chan";
    }

    internal static long ConvertSizeToBytes(string size) {
        if (size.EndsWith("kb", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^2], out var dbl);
            return (long)Math.Round(dbl * 1000, MidpointRounding.ToEven);
        }

        if (size.EndsWith("mb", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^2], out var dbl);
            return (long)Math.Round(dbl * 1000 * 1000, MidpointRounding.ToEven);
        }

        if (size.EndsWith("gb", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^2], out var dbl);
            return (long)Math.Round(dbl * 1000 * 1000 * 1000, MidpointRounding.ToEven);
        }

        if (size.EndsWith("b", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^1], out var dbl);
            return (long)Math.Round(dbl, MidpointRounding.ToEven);
        }

        throw new ArgumentException("Invalid size format");
    }
    internal static DateTimeOffset ConvertTimestampToDateTime(string timestamp) {
        //YYYY/mm/DD hh:MM:ss
        string[] splits = timestamp.Split(' ');

        // YYYY/mm/DD
        string[] splits2 = splits[0].Split('/');
        int year = int.Parse(splits2[0]);
        int month = int.Parse(splits2[1]);
        int day = int.Parse(splits2[2]);

        // hh:MM:ss
        splits2 = splits[1].Split(':');
        int hour = int.Parse(splits2[0]);
        int minute = int.Parse(splits2[1]);
        int second = int.Parse(splits2[2]);

        // UTC -6:00
        return new(year, month, day, hour, minute, second, new TimeSpan(-6, 0, 0));

        //return DateTime.Parse(str);
    }
    internal static Size ConvertDimensionsToSize(string size) {
        string[] Dimensions = size.Split('x', 'X');
        return new Size(int.Parse(Dimensions[0]),
            int.Parse(Dimensions[1]));
    }
    internal static Size ConvertThumbnailAttributesToSize(HtmlAttribute Attribute) {
        static string GetSize(string d) {
            return d[(d.IndexOf(':') + 2)..^2];
        }
        string[] Style = Attribute.Value!.Split([';'], StringSplitOptions.RemoveEmptyEntries);
        return new Size(int.Parse(GetSize(Style[0])),
            int.Parse(GetSize(Style[1])));
    }
    internal static string? GetMessage(HtmlElementNode? node) {
        if (node != null) {
            if (node.Children[^1] is HtmlElementNode cnode && cnode.TagName.Equals("br", StringComparison.OrdinalIgnoreCase)) {
                node.Children.RemoveAt(node.Children.Count - 1);
            }
            for (int i = 0; i < node.Children.Count; i++) {
                if (node.Children[i] is HtmlElementNode element) {
                    CleanMessageNode(element);
                }
            }
            if (!node.InnerHtml.IsNullEmptyWhitespace()) {
                return node.InnerHtml;
            }
        }
        return null;
    }
    private static void CleanMessageNode(HtmlElementNode node) {
        if (node.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)) {
            node.Attributes.Remove("onclick");
            node.Attributes.Remove("onmouseover");
            node.Attributes.Remove("onmouseout");
            node.Attributes.Remove("class");
            var newHref = node.Attributes["href"]?.Value;
            if (newHref != null) {
                node.Attributes["href"]!.Value = "#p" + newHref[(newHref.LastIndexOf('#') + 1)..];
                node.Attributes.Add(new HtmlAttribute("class", "quotelink"));
            }
        }

        if (node.Children.Count > 0) {
            for (int i = 0; i < node.Children.Count; i++) {
                if (node.Children[i] is HtmlElementNode element) {
                    CleanMessageNode(element);
                }
            }
        }
    }
}
