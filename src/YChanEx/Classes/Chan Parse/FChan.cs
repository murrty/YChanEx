﻿#nullable enable
namespace YChanEx.Parsers;
using System.Drawing;
using System.Net;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class FChan {
    public static readonly Cookie AccessCookie = new("disclaimer", "seen", "/", "fchan.us");
    private static readonly Selector ThreadSelector = Selector.ParseSelector("div[id:=\"whole_thread\\d+\"]");
    private static readonly Selector PostsSelector = Selector.ParseSelector("div[id:=\"thread\\d+\"] > table");

    public static FChanPost[] Generate(string html) {
        HtmlDocument htDoc = HtmlDocument.FromHtml(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);
        var ThreadNode = htDoc.FirstOrDefault(ThreadSelector) ??
            throw new ArgumentNullException("Could not find thread node.");

        var PostNodes = ThreadNode.Children.Find(PostsSelector)
            .ToArray();

        if (PostNodes == null) {
            throw new NullReferenceException($"Could not find '{nameof(PostNodes)}'.");
        }

        if (PostNodes.Length == 0) {
            return [];
        }

        // If the children replies are nested, it's an older thread that hasn't been updated.
        bool ChildrenNested = PostNodes[0].Children.FirstOrDefault(FChanPost.ReplyPostSelector) != null;

        var array = new FChanPost[PostNodes.Length];
        array[0] = new(PostNodes[0], ThreadNode, ChildrenNested, true);
        for (int i = 1; i < PostNodes.Length; i++) {
            array[i] = new(PostNodes[i], ThreadNode, ChildrenNested, false);
        }

        return array;
    }
    public static async Task<FChanPost[]> GenerateAsync(string html) {
        HtmlDocument htDoc = await HtmlDocument.FromHtmlAsync(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);
        var ThreadNode = htDoc.FirstOrDefault(ThreadSelector) ??
            throw new ArgumentNullException("Could not find thread node.");

        var PostNodes = ThreadNode.Children.Find(PostsSelector)
            .ToArray();

        if (PostNodes == null) {
            throw new NullReferenceException($"Could not find '{nameof(PostNodes)}'.");
        }

        if (PostNodes.Length == 0) {
            return [];
        }

        // If the children replies are nested, it's an older thread that hasn't been updated.
        bool ChildrenNested = await Task.Run(() => PostNodes[0].Children.FirstOrDefault(FChanPost.ReplyPostSelector)) != null;

        var array = new FChanPost[PostNodes.Length];
        array[0] = new(PostNodes[0], ThreadNode, ChildrenNested, true);
        await Task.Run(() => {
            for (int i = 1; i < PostNodes.Length; i++) {
                array[i] = new(PostNodes[i], ThreadNode, ChildrenNested, false);
            }
        });

        return array;
    }

    internal static long ConvertSizeToBytes(string size) {
        return long.Parse(size[..size.IndexOf(' ')]);
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

        // UTC -4:00
        return new(year, month, day, hour, minute, second, new TimeSpan(-4, 0, 0));

        //return DateTime.Parse(str);
    }
    internal static Size ConvertDimensionsToSize(string size) {
        string[] Dimensions = size.Trim().Split('x', 'X');
        return new Size(int.Parse(Dimensions[0]),
            int.Parse(Dimensions[1]));
    }
    internal static string? GetMessage(HtmlElementNode? node) {
        if (node?.Children.Count > 0) {
            for (int i = 0; i < node.Children.Count; i++) {
                if (node.Children[i] is HtmlElementNode enode) {
                    if (enode.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)) {
                        enode.Attributes.Remove("class");
                        enode.Attributes.Remove("onclick");
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
}