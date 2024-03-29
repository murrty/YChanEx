﻿#nullable enable
namespace YChanEx.Parsers;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class SevenChan {
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
            array[i] = new(PostNodes[i]);
        }

        return array;
    }
    public static async Task<SevenChanPost[]> GenerateAsync(string html) {
        HtmlDocument htDoc = await HtmlDocument.FromHtmlAsync(html, HtmlParseOptions.RemoveEmptyTextNodes | HtmlParseOptions.TrimTextNodes);

        return await Task.Run(() => {
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
                array[i] = new(PostNodes[i]);
            }

            return array;
        });
    }
    public static SevenChanPost[]? TryGenerate(string html) {
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
        return $"/{board}/ - {name} - 7chan";
    }
    public static string GetFullBoardName(string board, bool @override) {
        if (General.UseFullBoardNameForTitle || @override) {
            return board.ToLowerInvariant() switch {
                #region 7chan & Related services
                "7ch" => "Site Discussion",
                "ch7" => "Channel7 & Radio7",
                "irc" => "Internet Relay Circlejerk",
                #endregion

                #region VIP
                "VIP" => "Very Important Posters",
                "civ" => "Civics",
                //"vip6" => "IPv6 for VIP",
                #endregion

                #region Premium Content
                "777" => "weed",
                "b" => "Random",
                "banner" => "Banners",
                "f" => "Flash",
                "gfx" => "Grahpics Manipulation",
                "fail" => "Failure",
                #endregion

                #region SFW
                //"class" => "The Finer Things",
                "co" => "Comics and Cartoons",
                "eh" => "Particularly uninteresting conversation",
                "fit" => "Fitness & Health",
                "halp" => "Technical Support",
                "jew" => "Thrifty Living",
                "lit" => "Literature",
                "phi" => "Philosophy",
                "pr" => "Programming",
                "rnb" => "Rage and Baww",
                "sci" => "Science, Technology, Engineering, and Mathematics",
                "tg" => "Tabletop Games",
                "w" => "Weapons",
                "zom" => "Zombies",
                #endregion

                #region General
                "a" => "Anime & Manga",
                "ai" => "AI-Generated Artwork & Text",
                "grim" => "Cold, Grim & Miserable",
                "hi" => "History and Culture",
                "me" => "Film, Music & Television",
                "rx" => "Drugs",
                "vg" => "Video Games",
                "wp" => "Wallpapers",
                "x" => "Paranormal & Conspiracy",
                #endregion

                #region Porn
                "cake" => "Delicious",
                "cd" => "Crossdressing",
                "d" => "Alternative Hentai",
                "di" => "Sexy Beautiful Traps",
                "elit" => "Erotic Literature",
                "fag" => "Men Discussion",
                "fur" => "Furry",
                "gif" => "Animated GIFs",
                "h" => "Hentai",
                "men" => "Sexy Beautiful Men",
                "pco" => "Porn Comics",
                "s" => "Sexy Beautiful Women",
                "sm" => "Shotacon",
                "ss" => "Straight Shotacon",
                "unf" => "Uniforms",
                //"v" => "The Vineyard",
                #endregion

                _ => $"{board} (Unknown board)"
            };
        }
        return board;
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
