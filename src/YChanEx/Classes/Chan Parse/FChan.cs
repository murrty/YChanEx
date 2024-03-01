#nullable enable
namespace YChanEx.Parsers;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class FChan {
    public static readonly System.Net.Cookie AccessCookie = new("disclaimer", "seen", "/", "fchan.us");
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

        return await Task.Run(() => {
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
        });
    }
    public static FChanPost[]? TryGenerate(string html) {
        try {
            return Generate(html);
        }
        catch {
            return null;
        }
    }

    public static string GetOldHistoryName(string Url) {
        if (Url.StartsWith("ychanex:")) {
            Url = Url[8..];
        }
        if (Url.StartsWith("view-source:")) {
            Url = Url[12..];
        }
        Url = Networking.CleanURL(Url);

        string[] URLSplit = Url.Split('/');
        return $"/{URLSplit[^3]}/ - {URLSplit[^1].SubstringBeforeLastChar('.')}";
    }

    public static string? GetHtmlTitle(ThreadData data) {
        if (data.ThreadName == null) {
            return null;
        }
        return GetHtmlTitle(data.Board, data.ThreadName);
    }
    public static string GetHtmlTitle(string board, string name) {
        return $"/{board}/ - {name} - fchan";
    }
    public static string GetFullBoardName(string board, bool @override) {
        if (General.UseFullBoardNameForTitle || @override) {
            return board.ToLowerInvariant() switch {
                #region Normal image boards
                "f" => "female",
                "m" => "male",
                "h" => "herm",
                "s" => "straight",
                "toon" => "toon",
                "a" => "alternative",
                "ah" => "alternative (hard)",
                "c" => "clean",
                #endregion

                #region Specialized image boards
                "artist" => "artist",
                "crit" => "critique",
                "b" => "banners",
                #endregion

                _ => $"{board} (Unknown board)"
            };
        }
        return board;
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
            node.Attributes.Remove("class");
            node.Attributes.Remove("onclick");
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
