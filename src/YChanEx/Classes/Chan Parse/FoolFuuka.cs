#nullable enable
namespace YChanEx.Parsers;
using System.Drawing;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class FoolFuuka {
    private static readonly Selector FirstPostSelector = Selector.ParseSelector("article[id:=\"\\d+\"][data-board=]");
    private static readonly Selector RepliesSelector = Selector.ParseSelector("aside[class=posts] > article > div[class=\"post_wrapper\"]");

    // Tested: 2.2.0: b4k, desuarchive, archived (to an extent)

    public static FoolFuukaPost[] Generate(string html) {
        HtmlDocument htDoc = HtmlDocument.FromHtml(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);
        var FirstPostNode = htDoc.Find(FirstPostSelector)
            .FirstOrDefault();

        if (FirstPostNode == null) {
            throw new NullReferenceException($"Could not find '{nameof(FirstPostNode)}'.");
        }

        FoolFuukaPost FirstPost = new(FirstPostNode);

        var ReplyNodes = htDoc.Find(RepliesSelector)
            .ToArray();

        if (ReplyNodes.Length == 0) {
            return [FirstPost];
        }

        var array = new FoolFuukaPost[ReplyNodes.Length + 1];
        array[0] = FirstPost;
        for (int i = 0; i < ReplyNodes.Length; i++) {
            array[i + 1] = new(ReplyNodes[i]);
        }

        return array;
    }
    public static async Task<FoolFuukaPost[]> GenerateAsync(string html) {
        HtmlDocument htDoc = await HtmlDocument.FromHtmlAsync(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);

        return await Task.Run(() => {
            var FirstPostNode = htDoc.Find(FirstPostSelector)
                .FirstOrDefault();

            if (FirstPostNode == null) {
                throw new NullReferenceException($"Could not find '{nameof(FirstPostNode)}'.");
            }

            FoolFuukaPost FirstPost = new(FirstPostNode);
            var ReplyNodes = htDoc.Find(RepliesSelector)
                .ToArray();

            if (ReplyNodes.Length == 0) {
                return [ FirstPost ];
            }

            var array = new FoolFuukaPost[ReplyNodes.Length + 1];
            array[0] = FirstPost;
            for (int i = 0; i < ReplyNodes.Length; i++) {
                array[i + 1] = new(ReplyNodes[i]);
            }

            return array;
        });
    }

    internal static long ConvertSizeToBytes(string size) {
        if (size.EndsWith("kib", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^3], out var dbl);
            return (long)Math.Round(dbl * 1024, MidpointRounding.ToEven);
        }

        if (size.EndsWith("mib", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^3], out var dbl);
            return (long)Math.Round(dbl * 1024 * 1024, MidpointRounding.ToEven);
        }

        if (size.EndsWith("gib", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^3], out var dbl);
            return (long)Math.Round(dbl * 1024 * 1024 * 1024, MidpointRounding.ToEven);
        }

        if (size.EndsWith("b", StringComparison.OrdinalIgnoreCase)) {
            double.TryParse(size[..^1], out var dbl);
            return (long)Math.Round(dbl, MidpointRounding.ToEven);
        }

        throw new ArgumentException("Invalid size format");
    }
    internal static DateTimeOffset ConvertTimestampToDateTime(string timestamp) {
        return DateTimeOffset.Parse(timestamp);
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
        if (node?.Children.Count > 0) {
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
        if (node.TagName.Equals("span", StringComparison.OrdinalIgnoreCase)) {
            if (node.Attributes.Contains("class", "greentext", StringComparison.InvariantCultureIgnoreCase)) {
                if (node.Children.Count > 0) {
                    if (node.Children[0] is HtmlElementNode c) {
                        node = c;
                        if (node.Attributes.TryGetValue("data-post", out var postId)) {
                            node.Attributes.Clear();
                            node.Attributes.Add("class", "quotelink");
                            node.Attributes.Add("href", "#p" + postId.Value);
                        }
                    }
                    else {
                        node.Attributes.Remove("class");
                        node.Attributes.Add("class", "quote");
                    }
                }
            }
            else if (node.Attributes.ContainsWithValue("class", "spoiler", StringComparison.InvariantCultureIgnoreCase)) {
                node.TagName = "s";
                node.Attributes.Clear();
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
