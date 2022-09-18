namespace YChanEx;

internal static class HtmlControl {

    private const int MaximumThumbnailSize = 150;
    private const int MaximumOpThumbnailSize = 250;

    private const string CSS = """
            html {
                -moz-text-size-adjust: 100%;
                -webkit-text-size-adjust: 100%;
                -ms-text-size-adjust: 100%;
            }
            
            body {
                background: #202020 none;
                color: #c5c8c6;
                font-family: arial, helvetica, sans-serif;
                font-size: 10pt;
                margin-left: 0;
                margin-right: 0;
                margin-top: 5px;
                padding-left: 5px;
                padding-right: 5px;
            }
            
            a {
                color: #5f89ac!important;
                text-decoration: none;
            }
            
            a:hover {
                color: #82a3bf!important;
            }

            s {
                background-color: #000;
                color: #000;
                text-decoration: none;
            }

            s:hover {
                color: #fff;
            }
            
            img {
                border: none;
            }
            
            div.threadBanner {
                text-align: center;
            }
            
            div.threadBanner>div.boardTitle {
                font-family: Tahoma, sans-serif;
                font-size: 28px;
                font-weight: 700;
                letter-spacing: -2px;
                margin-top: -4px;
            }
            
            div.threadBanner>div.boardSubtitle {
                font-family: Tahoma, sans-serif;
                font-size: 14px;
                font-weight: 700;
                margin-top: 0;
            }
            
            div.threadBanner>div.archiveDisclaimer {
                margin-top: 4px;
                font-size: x-small;
            }
            
            div.post {
                margin: 4px 0;
            }
            
            div.reply {
                background-color: #252525;
                border: 1px solid #191919;
                display: table;
                padding: 2px;
                padding-top: 5px;
            }
            
            div.post div.file .fileThumb {
                float: left;
                margin-left: 6px;
                margin-right: 10px;
                margin-top: 3px;
                margin-bottom: 6px;
            }
            
            div.post div.file .fileThumb img {
                border: none;
                float: left;
            }
            
            .reply:target {
                background: #1d1d1d!important;
                border: 1px solid #111!important;
                padding: 2px;
            }

            .fileText a {
                display: inline-block;
                text-decoration: none;
                max-width: 160px;
                white-space:nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
                border-bottom: 1px solid;
            }

            .fileText a:hover {
                text-overflow:unset;
                max-width: 100%;
            }

            div.reply .fileText a {
                margin-bottom: -3px;
            }

            div.op .fileText a {
                margin-bottom: -1px;
            }

            span.info {
                display: inline-block;
                padding-left: 4px;
            }

            div.reply div.multiFile span.info {
                padding-right: 6px;
            }

            span.subject{
                color:#ba89c9;
                font-weight:700;
            }
            
            span.name {
                font-weight: bold;
                padding-left: 5px;
            }

            span.quote {
                color: #69a669;
            }

            span.tripcode {
                font-size: 11px;
            }

            span.posterid {
                margin-left: 5px;
                background-color: rgb(100, 27, 113);
                border-radius: 4px;
                padding-left: 2px;
                padding-right: 2px;
            }

            span.dateTime {
                margin-left: 6px;
            }

            span.ychanexNotice {
                color: #5b5b5b;
                font-style: italic;
                font-size: 12px;
            }

            div.post div.file .fileSpoiler img,
            div.post div.multiFile .fileSpoiler img {
                filter: blur(6px);
                -webkit-filter: blur(6px);
                -moz-filter: blur(6px);
                -o-filter: blur(6px);
                -ms-filter: blur(6px);
                /*transition: filter 0.15s;*/
            }

            div.post div.file .fileSpoiler img:hover,
            div.post div.multiFile .fileSpoiler img:hover {
                filter: blur(0px);
                -webkit-filter: blur(0px);
                -moz-filter: blur(0px);
                -o-filter: blur(0px);
                -ms-filter: blur(0px);
                /*transition: filter 0.15s;*/
            }

            div.multiFile {
                display: inline-block;
                vertical-align: top;
                padding-right: 7px;
            }

            div.multiFile .fileThumb img {
                margin-left: 6px;
                margin-top: 3px;
            }

            div.contentControls > div.contentSeparator {
                justify-content: center;
            }

            hr.contentSeparator {
                margin: 0 auto;
                border: none;
                border-top: 1px solid #303030;
                width: 500px;
                max-width: 100%;
                margin-top: -7px;
                padding-bottom: 11px;
            }

            .archiveNote {
                display: flex;
                justify-content: center;
                margin-top: -7px;
            }

            div.reply div.postInfo, div.reply div.fileText,
            div.file div.postInfo {
                margin-left: -2px;
            }

            div.op div.postInfo, div.op div.fileText {
                margin-left: -7px;
            }
""";

    private const string HtmlBase = """
<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8">
        <style>
{0}
        </style>
        <title></title>
    </head>

    <body>
        <div class="threadBanner">
            <div class="boardTitle">/{1}/ - {2}</div>
            <div class="boardSubtitle">{3}</div>
            <div class="archiveDisclaimer">{4}</div>
        </div>

        <div class="contentControls">
            <div class="upperControls">
                [<a href="#bottom">Bottom</a>]
                [<a href="{5}">Original thread</a>]
            </div>
            <div class="contentSeparator">
                <hr class="contentSeparator" />
            </div>
        </div>

""";

    private static string Condense(string HTML) {
        if (!Config.Settings.Downloads.CleanThreadHTML) {
            string[] Base = HTML.Replace("\r\n", "\n").Split('\n');
            string buffer = string.Empty;
            for (int i = 0; i < Base.Length; i++) {
                buffer += Base[i].Trim();
            }
            return buffer;
        }
        return HTML;
    }

    public static string RebuildHTML(ThreadInfo Thread) {
        string HTML = GetHTMLBase(Thread);
        if (Thread.Data.Posts.Count > 0) {
            for (int i = 0; i < Thread.Data.Posts.Count; i++) {
                HTML += GetPostHtmlData(Thread.Data.Posts[i], Thread);
            }
        }
        return HTML;
    }

    public static string GetHTMLBase(ThreadInfo Thread) {
        return Thread.Chan switch {
            ChanType.FourChan or
            ChanType.FourTwentyChan or
            ChanType.SevenChan or
            ChanType.EightChan or
            ChanType.EightKun =>
                Condense(string.Format(
                    HtmlBase,
                    CSS,
                    Thread.Data.ThreadBoard,
                    Chans.GetFullBoardName(Thread, true),
                    Thread.Data.BoardSubtitle,
                    Thread.Chan switch {
                        ChanType.EightChan or
                        ChanType.EightKun => "The content archived from this site is highly fucking stupid and is not reviewed by, endorsed by, or reflect the views of YChanEx.",
                        _ => "The content archived here was not previously reviewed by YChanEx, and is not endorsed by or reflect the views of YChanEx."
                    },
                    Thread.Data.ThreadURL)),

            _ => Condense(
$$"""
{{Condense(HtmlBase)}}{{Thread.Data.ThreadBoard}}</div>
            <div class=\"boardSubtitle\">{{Thread.Data.ThreadBoard}}</div>
        </div>

        <hr class=\"CenteredSmall\" />

        <br/><center>This *chan isn't supported by the updated HTML archiver. This may be a program-sided issue.</center><br/>
""")
        };
    }

    public static string GetPostHtmlData(PostData Post, ThreadInfo Thread) {
        string HTML = string.Empty;

        //if (Post.PostThumbnailHeight > MaximumThumbnailSize) {
        //    decimal rnd = (Post.FirstPost ? MaximumOpThumbnailSize : MaximumThumbnailSize) / (decimal)Post.PostThumbnailHeight;
        //    Post.PostThumbnailWidth = (int)Math.Round(Post.PostThumbnailWidth * rnd);
        //    Post.PostThumbnailHeight = (int)Math.Round(Post.PostThumbnailHeight * rnd);
        //}
        //else if (Post.PostThumbnailWidth > MaximumThumbnailSize) {
        //    decimal rnd = (Post.FirstPost ? MaximumOpThumbnailSize : MaximumThumbnailSize) / (decimal)Post.PostThumbnailWidth;
        //    Post.PostThumbnailWidth = (int)Math.Round(Post.PostThumbnailWidth * rnd);
        //    Post.PostThumbnailHeight = (int)Math.Round(Post.PostThumbnailHeight * rnd);
        //}

        if (Post.FirstPost) {
HTML += $$"""

        <div class="postContainer">
            <div id="p{{Post.PostID}}" class="post op">
""";

            if (Post.Files.Count > 0) {
                for (int i = 0; i < Post.Files.Count; i++) {
HTML += $$"""

                <div class="{{(Post.Files.Count > 1 ? "multiFile" : "file")}}">
                    <div class="fileText">&nbsp; 
                        <a href="{{Post.Files[i].GeneratedName}}" target="_blank">{{Post.Files[i].OriginalName}}{{Post.Files[i].Extension}}</a><span class="info"> ({{Post.Files[i].CustomSize ?? GetSize(Post.Files[i].Size)}}, {{Post.Files[i].Dimensions.Width}}x{{Post.Files[i].Dimensions.Height}})</span>
                    </div>
                    <a class="fileThumb{{(Post.Files[i].Spoiled ? " fileSpoiler" : "")}}" href="{{Post.Files[i].GeneratedName}}" target="_blank">{{GetFile(Post.Files[i])}}</a>
                </div>

""";
                }
            }

HTML += $$"""
            <div class="postInfo desktop">&nbsp; <span class="subject">{{Post.PostSubject}}</span><span class="nameBlock"><span class="name">{{Post.PosterName}}</span>{{(Post.SpecialPosterName is not null ? $"<span class=\"specialname\">##{Post.SpecialPosterName}</span>" : "")}}{{(Post.PosterTripcode is not null ? $"<span class=\"tripcode\">{Post.PosterTripcode}</span>" : "")}}{{(Post.PosterID is not null ? $"<span class=\"posterid\">{Post.PosterID}</span>" : "")}}</span><span class="dateTime">{{Post.PostDate}}</span> <span class="postNum desktop"><a href="#p{{Post.PostID}}">No.</a>{{Post.PostID}} <a href="{{Thread.Data.ThreadURL}}#p{{Post.PostID}}">Original post</a></span></div>
                <blockquote class="postMessage">{{Post.PostMessage}}</blockquote>
            </div>
        </div>

""";
        }
        else {
HTML += $$"""

        <div class="postContainer">
            <div id="p{{Post.PostID}}" class="post reply">
                <div class="postInfo desktop">
                    <span class="nameBlock"><span class="name">&nbsp;{{(!string.IsNullOrWhiteSpace(Post.PostSubject) ? $"<span class=\"subject\">{Post.PostSubject} </span>" : "")}}{{Post.PosterName}}</span>{{(Post.SpecialPosterName is not null ? $"<span class=\"specialname\">##{Post.SpecialPosterName}</span>" : "")}}{{(Post.PosterTripcode is not null ? $"<span class=\"tripcode\">{Post.PosterTripcode}</span>" : "")}}{{(Post.PosterID is not null ? $"<span class=\"posterid\">{Post.PosterID}</span>" : "")}}</span><span class="dateTime">{{Post.PostDate}}</span> <span class="postNum desktop"><a href="#p{{Post.PostID}}">No.</a>{{Post.PostID}} <a href="{{Thread.Data.ThreadURL}}#p{{Post.PostID}}">Original post</a>&nbsp;</span>
                </div>

""";

            if (Post.Files.Count > 0) {
                for (int i = 0; i < Post.Files.Count; i++) {
HTML += $$"""
            <div class="{{(Post.Files.Count > 1 ? "multiFile" : "file")}}">
                    <div class="fileText">&nbsp; <a href="{{Post.Files[i].GeneratedName}}" target="_blank">{{Post.Files[i].OriginalName}}{{Post.Files[i].Extension}}</a><span class="info"> ({{Post.Files[i].CustomSize ?? GetSize(Post.Files[i].Size)}}, {{Post.Files[i].Dimensions.Width}}x{{Post.Files[i].Dimensions.Height}})</span></div>
                    <a class="fileThumb{{(Post.Files[i].Spoiled ? " fileSpoiler" : "")}}" href="{{Post.Files[i].GeneratedName}}" target="_blank">{{GetFile(Post.Files[i])}}</a>
                </div>

""";
                }
            }

HTML += $$"""
            <blockquote class="postMessage">{{Post.PostMessage}}</blockquote>
{{(Post.Files.Count > 1 ? "                <blockquote class=\"postMessage\"><span class=\"ychanexNotice\">This is a multi-file post</span></blockquote>" : "")}}
            </div>
        </div>

""";

        }
        return Config.Settings.Downloads.CleanThreadHTML ? HTML : Condense(HTML);
    }

    public static string GetHTMLFooter(ThreadInfo Thread) {
string HTML = $$"""

        <div class="contentControls">
            <div class="upperControls">
                [<a href="#top">Top</a>]
                [<a href="{{Thread.Data.ThreadURL}}">Original thread</a>]
            </div>
            <div class="contentSeparator">
                <hr class="contentSeparator" />
            </div>
            <a class="archiveNote" href="https://github.com/murrty/ychanex">Archived using YChanEx</a>
        </div>
        
        <div class="threadBanner" id="bottom">
            <div class="archiveDisclaimer">The content archived here was not previously reviewed by YChanEx, and is not endorsed by or reflect the views of YChanEx.</div>
        </div>
    </body>
</html>
""";

        return Config.Settings.Downloads.CleanThreadHTML ? HTML : Condense(HTML);
    }

    public static string GetFile(PostData.FileData File) {
        return File.Extension switch {

            ".webm" or "webm" =>
$"""
<video controls width="{File.ThumbnailDimensions.Width}" height="{File.ThumbnailDimensions.Height}" loop=true><source src="{File.GeneratedName}" type="video/webm"></video>
""",

            ".mp4" or "mp4" =>
$"""
<video controls width="{File.ThumbnailDimensions.Width}" height="{File.ThumbnailDimensions.Height}" loop=true><source src="{File.GeneratedName}" type="video/webm"></video>
""",

            _ =>
$"""
<img src="{(Config.Settings.Downloads.SaveThumbnails ? "thumb/" + File.ID + "s.jpg" : File.GeneratedName)}" style="width: {File.ThumbnailDimensions.Width}px; height: {File.ThumbnailDimensions.Height}px;" />
""",


        };
    }

    private static readonly string[] SizeSuffix =
        { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

    public static string GetSize(long Size, int DecimalPlaces = 2) {
        int DivisionCount = 0;
        decimal Division = Size;
        while (Math.Round(Division, DecimalPlaces) >= 1024) {
            Division /= 1024;
            DivisionCount++;
        }
        return $"{decimal.Round(Division, DecimalPlaces)} {SizeSuffix[DivisionCount]}";
    }

    public static string GetReadableTime(DateTime Time) {
        return $"{Time.Year:#0000}/{Time.Month:#00}/{Time.Day:#00} ({Time.DayOfWeek.ToString()[..3]}) {Time.Hour:#00}:{Time.Minute:#00}:{Time.Second:#00}";
    }

    public static string GetReadableTime(double Time) {
        DateTime NewDate = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        NewDate = NewDate.AddSeconds(Time).ToLocalTime();
        return GetReadableTime(NewDate);
    }

    public static string GetReadableTime(long Time) {
        DateTime NewDate = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        NewDate = NewDate.AddSeconds(Time).ToLocalTime();
        return GetReadableTime(NewDate);
    }

    public static System.Drawing.Size ResizeToThumbnail(int Width, int Height, bool FirstPost) {
        if (Height > MaximumThumbnailSize) {
            decimal rnd = (FirstPost ? MaximumOpThumbnailSize : MaximumThumbnailSize) / (decimal)Height;
            Width = (int)Math.Round(Width * rnd);
            Height = (int)Math.Round(Height * rnd);
        }
        else if (Width > MaximumThumbnailSize) {
            decimal rnd = (FirstPost ? MaximumOpThumbnailSize : MaximumThumbnailSize) / (decimal)Width;
            Width = (int)Math.Round(Width * rnd);
            Height = (int)Math.Round(Height * rnd);
        }
        return new(Width, Height);
    }

    public static string ConvertHtmlTags(string input, ChanType type) {
        // <[a-zA-Z\/].*?>
        switch (type) {
            case ChanType.EightKun: {
                System.Text.RegularExpressions.Regex RegEx = new("<p class=\\\"body-line ltr quote\\\">.*?<\\/p>");
                System.Text.RegularExpressions.Match RegexMatch;
                RegexMatch = RegEx.Match(input);
                bool QuoteMatch = RegexMatch.Success;
                while (QuoteMatch) {
                    string ReplacementString = RegexMatch.Value
                        .Replace("<p class=\"body-line ltr quote\">", "<span class=\"quote\">")[..^4] + "</span><br />";
                    input = input.Replace(RegexMatch.Value, ReplacementString);

                    RegexMatch = RegEx.Match(input);
                    QuoteMatch = RegexMatch.Success;
                }
                input = System.Text.RegularExpressions.Regex.Replace(input, "", "");

                input = input
                    .Replace("<p class=\"body-line ltr \">", "")
                    .Replace("</p>", "<br />")
                    .Replace("<p class=\"body-line empty \"/>", "")
                    .Replace("<p class=\"body-line empty \">", "")
                    .TrimEnd();

                while (input.EndsWith("<br />")) {
                    input = input[..^6].TrimEnd();
                }

            } break;
        }
        return input;
    }

}