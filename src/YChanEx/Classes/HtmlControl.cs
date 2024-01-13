namespace YChanEx;

using System.Text;
using YChanEx.Posts;

// TODO: Handle HTML that filters out messages in posts, non-image posts, or both

internal static class HtmlControl {
    private static readonly string[] SizeSuffix =
        [ "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" ];

    private static string Condense(string HTML) {
        if (!Downloads.CleanThreadHTML) {
            return HTML.Replace("\r\n", "\n").Replace("\n", "");
            //string[] Base = HTML.Replace("\r\n", "\n").Split('\n');
            //string buffer = string.Empty;
            //for (int i = 0; i < Base.Length; i++) {
            //    buffer += Base[i].Trim();
            //}
            //return buffer;
        }
        return HTML;
    }

    public static string GetHTMLBase(ThreadInfo Thread) {
        return $$"""
<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8">
        <style>
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
                color: #5f89ac;
                text-decoration: none;
            }

            s > a {
                color: #000;
            }
            
            a:hover {
                color: #82a3bf;
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

            div.tags {
                display: inline-flex;
                margin-left: 12px;
                margin-bottom: 12px;
            }

            div.tag {
                display: flex;
                margin-left: 4px;
                padding: 2px 8px 2px 8px;
                background-color: #353535;
                border: 1px solid #191919;
                border-radius: 4px;
            }
        </style>
        <title></title>
    </head>

    <body>
        <div class="threadBanner">
            <div class="boardTitle">/{{Thread.Data.Board}}/ - {{Chans.GetFullBoardName(Thread, true)}}</div>
            <div class="boardSubtitle">{{Thread.Data.BoardSubtitle}}</div>
            <div class="archiveDisclaimer">{{GetDisclaimer(Thread)}}</div>
        </div>

        <div class="contentControls">
            <div class="upperControls">
                [<a href="#bottom">Bottom</a>]
                [<a href="{{Thread.Data.Url}}">Original thread</a>]
            </div>
            <div class="contentSeparator">
                <hr class="contentSeparator" />
            </div>
        </div>

""";
    }
    public static string GetPostHtmlData(GenericPost Post, ThreadInfo Thread) {
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
            <div id="p{{Post.PostId}}" class="post op">
""";

            if (Post.PostFiles.Count > 0) {
                for (int i = 0; i < Post.PostFiles.Count; i++) {
                    var File = Post.PostFiles[i];

                    HTML += $$"""

                <div class="{{(Post.MultiFilePost ? "multiFile" : "file")}}">
                    <div class="fileText">&nbsp; 
                        <a href="{{File.SavedFile}}" target="_blank">{{File.OriginalFileName}}.{{File.FileExtension}}</a><span class="info"> ({{GetSize(File.FileSize)}}, {{File.FileDimensions.Width}}x{{File.FileDimensions.Height}})</span>
                    </div>
                    <a class="fileThumb{{(File.ThumbnailFileSpoiled ? " fileSpoiler" : "")}}" href="{{File.SavedFile}}" target="_blank">{{GetFile(File)}}</a>
                </div>

""";
                }
            }

            HTML += $$"""
            <div class="postInfo desktop">&nbsp; <span class="subject">{{Post.PostSubject}}</span><span class="nameBlock"><span class="name">{{Post.PosterName}}</span>{{(Post.PosterCapcode is not null ? $"<span class=\"specialname\">##{Post.PosterCapcode}</span>" : "")}}{{(Post.PosterTripcode is not null ? $"<span class=\"tripcode\">{Post.PosterTripcode}</span>" : "")}}{{(Post.PosterId is not null ? $"<span class=\"posterid\">{Post.PosterId}</span>" : "")}}</span><span class="dateTime" title="Unix timestamp: {{Post.PostDate.ToUnixTimeSeconds()}}">{{GetReadableTime(Post.PostDate)}}</span> <span class="postNum desktop"><a href="#p{{Post.PostId}}">No.</a>{{Post.PostId}} <a href="{{Thread.Data.Url}}#p{{Post.PostId}}">Original post</a></span></div>
                <blockquote class="postMessage">{{Post.PostMessage}}</blockquote>{{GetTags(Post.Tags)}}
            </div>
        </div>

""";
        }
        else {
            HTML += $$"""

        <div class="postContainer">
            <div id="p{{Post.PostId}}" class="post reply">
                <div class="postInfo desktop">
                    <span class="nameBlock"><span class="name">&nbsp;{{(!string.IsNullOrWhiteSpace(Post.PostSubject) ? $"<span class=\"subject\">{Post.PostSubject} </span>" : "")}}{{Post.PosterName}}</span>{{(Post.PosterCapcode is not null ? $"<span class=\"specialname\">##{Post.PosterCapcode}</span>" : "")}}{{(Post.PosterTripcode is not null ? $"<span class=\"tripcode\">{Post.PosterTripcode}</span>" : "")}}{{(Post.PosterId is not null ? $"<span class=\"posterid\">{Post.PosterId}</span>" : "")}}</span><span class="dateTime" title="Unix timestamp: {{Post.PostDate.ToUnixTimeSeconds()}}">{{GetReadableTime(Post.PostDate)}}</span> <span class="postNum desktop"><a href="#p{{Post.PostId}}">No.</a>{{Post.PostId}} <a href="{{Thread.Data.Url}}#p{{Post.PostId}}">Original post</a>&nbsp;</span>
                </div>

""";

            if (Post.PostFiles.Count > 0) {
                for (int i = 0; i < Post.PostFiles.Count; i++) {
                    var File = Post.PostFiles[i];

                    HTML += $$"""
            <div class="{{(Post.MultiFilePost ? "multiFile" : "file")}}">
                    <div class="fileText">&nbsp; <a href="{{File.SavedFile}}" target="_blank">{{File.OriginalFileName}}.{{File.FileExtension}}</a><span class="info"> ({{GetSize(File.FileSize)}}, {{File.FileDimensions.Width}}x{{File.FileDimensions.Height}})</span></div>
                    <a class="fileThumb{{(File.ThumbnailFileSpoiled ? " fileSpoiler" : "")}}" href="{{File.SavedFile}}" target="_blank">{{GetFile(File)}}</a>
                </div>

""";
                }
            }

            HTML += $$"""
            <blockquote class="postMessage">{{Post.PostMessage}}</blockquote>
{{(Post.MultiFilePost ? "                <blockquote class=\"postMessage\"><span class=\"ychanexNotice\">This is a multi-file post</span></blockquote>" : "")}}{{GetTags(Post.Tags)}}
            </div>
        </div>

""";
        }

        return Downloads.CleanThreadHTML ? HTML : Condense(HTML);
    }
    public static string GetHTMLFooter(ThreadInfo Thread) {
string HTML = $$"""

        <div class="contentControls">
            <div class="upperControls">
                [<a href="#top">Top</a>]
                [<a href="{{Thread.Data.Url}}">Original thread</a>]
            </div>
            <div class="contentSeparator">
                <hr class="contentSeparator" />
            </div>
            <a class="archiveNote" href="https://github.com/murrty/ychanex">Archived using YChanEx</a>
        </div>
        
        <div class="threadBanner" id="bottom">
            <div class="archiveDisclaimer">{{GetDisclaimer(Thread)}}</div>
        </div>
    </body>
</html>
""";

        return Downloads.CleanThreadHTML ? HTML : Condense(HTML);
    }

    private static string GetTags(string[] tags) {
        if (tags == null || tags.Length < 1) {
            return string.Empty;
        }

        StringBuilder sb = new("\n");
        sb.Append("""
                <div class="tags">
""");
        for (int i = 0; i < tags.Length; i++) {
            if (tags[i].IsNullEmptyWhitespace()) {
                continue;
            }
            sb.Append("""

                    <div class="tag">
                        
""").Append(tags[i]).Append("""

                    </div>
""");
        }
        sb.Append("""
                </div>
""");

        return sb.ToString();
    }
    private static string GetFile(GenericFile File) {
        return File.FileExtension switch {
            ".webm" or "webm" =>
$"""
<video controls {GetVideoThumbnailDimensions(File)}loop=true><source src="{File.SavedFile}" type="video/webm"></video>
""",

            ".mp4" or "mp4" =>
$"""
<video controls {GetVideoThumbnailDimensions(File)}loop=true><source src="{File.SavedFile}" type="video/webm"></video>
""",

            _ =>
$"""
<img src="{(Downloads.SaveThumbnails ? "thumb/" + File.SavedThumbnailFile : File.SavedFile)}" {GetImageThumbnailDimension(File)}/>
""",
        };
    }
    private static string GetImageThumbnailDimension(GenericFile File) {
        if (File.ThumbnailFileDimensions.Width == 0 || File.ThumbnailFileDimensions.Height == 0) {
            return string.Empty;
        }
        return $"style=\"width: {File.ThumbnailFileDimensions.Width}px; height: {File.ThumbnailFileDimensions.Height}px;\" ";
    }
    private static string GetVideoThumbnailDimensions(GenericFile File) {
        if (File.ThumbnailFileDimensions.Width == 0 || File.ThumbnailFileDimensions.Height == 0) {
            return string.Empty;
        }
        return $"width=\"{File.ThumbnailFileDimensions.Width}\" height=\"{File.ThumbnailFileDimensions.Height}\" ";
    }
    private static string GetDisclaimer(ThreadInfo Thread) {
        return Thread.Chan switch {
            //ChanType.EightChan or
            //ChanType.EightKun => "The content archived from this site is highly fucking stupid and is not reviewed by, endorsed by, or reflect the views of YChanEx, the developer, nor contributors.",
            ChanType.FourChan when Thread.Data.Board.Equals("pol", StringComparison.OrdinalIgnoreCase) => "The content archived from this board is actually braindead. YChanEx, developer, and contributors, reject the content archived here.",
            _ => "The content archived here was not previously reviewed by YChanEx (developer or contributors), and is not endorsed by nor reflect the views of YChanEx (developer or contributors)."
        };
    }

    public static string GetSize(long Size, int DecimalPlaces = 2) {
        int DivisionCount = 0;
        decimal Division = Size;
        while (Math.Round(Division, DecimalPlaces) >= 1024) {
            Division /= 1024;
            DivisionCount++;
        }
        return $"{decimal.Round(Division, DecimalPlaces)} {SizeSuffix[DivisionCount]}";
    }
    public static string GetReadableTime(DateTimeOffset Time) {
        Time = Time.ToUniversalTime();
        return $"{Time.Year:D4}/{Time.Month:D2}/{Time.Day:D2} ({Time.DayOfWeek.ToString()[..3]}) {Time.Hour:D2}:{Time.Minute:D2}:{Time.Second:D2} (UTC)";
    }
}