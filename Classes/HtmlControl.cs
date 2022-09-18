using System;

namespace YChanEx {
    class HtmlControl {

        private const string HtmlInit_Clean = $"<!DOCTYPE html>\r\n<html>\r\n    <head>\r\n        <meta charset=\"utf - 8\">\r\n        <style>\r\n            html {{\r\n                -moz-text-size-adjust: 100%;\r\n                -webkit-text-size-adjust: 100%;\r\n                -ms-text-size-adjust: 100%;\r\n            }}\r\n            \r\n            body {{\r\n                background: #1d1f21 none;\r\n                color: #c5c8c6;\r\n                font-family: arial, helvetica, sans-serif;\r\n                font-size: 10pt;\r\n                margin-left: 0;\r\n                margin-right: 0;\r\n                margin-top: 5px;\r\n                padding-left: 5px;\r\n                padding-right: 5px;\r\n            }}\r\n            \r\n            .CenteredSmall {{\r\n                width: 468px;\r\n                max-width: 100%;\r\n            }}\r\n            \r\n            a, a:visited {{\r\n                color: #81a2be!important;\r\n                text-decoration: none;\r\n            }}\r\n            \r\n            a.replylink:not(:hover), div#absbot a:not(:hover) {{\r\n                color: #81a2be!important;\r\n            }}\r\n            \r\n            a:hover {{\r\n                color: #5f89ac!important;\r\n            }}\r\n            \r\n            img {{\r\n                border: none;\r\n            }}\r\n            \r\n            hr {{\r\n                border: none;\r\n                border-top: 1px solid #282a2e;\r\n                height: 0;\r\n            }}\r\n            \r\n            div.boardBanner {{\r\n                text-align: center; clear: both;\r\n            }}\r\n            \r\n            div.boardBanner>div.boardTitle {{\r\n                font-family: Tahoma, sans-serif;\r\n                font-size: 28px;\r\n                font-weight: 700;\r\n                letter-spacing: -2px;\r\n                margin-top: 0;\r\n            }}\r\n            \r\n            div.boardBanner>div.boardSubtitle {{\r\n                font-size: x-small;\r\n            }}\r\n            \r\n            div.post {{\r\n                margin: 5px 0;\r\n                overflow: hidden;\r\n            }}\r\n            \r\n            div.op {{\r\n                display: inline;\r\n            }}\r\n            \r\n            div.reply {{\r\n                background-color: #282a2e;\r\n                border: 1px solid #282a2e;\r\n                display: table;\r\n                padding: 2px;\r\n            }}\r\n            \r\n            div.post div.postInfo span.nameBlock span.postertrip {{\r\n                color: #c5c8c6;\r\n                font-weight: 400!important;\r\n            }}\r\n            \r\n            div.post div.file .fileThumb {{\r\n                float: left;\r\n                margin-left: 20px;\r\n                margin-right: 20px;\r\n                margin-top: 3px;\r\n                margin-bottom: 5px;\r\n            }}\r\n            \r\n            span.fileThumb {{\r\n                margin-left: 0!important;\r\n                margin-right: 5px!important;\r\n            }}\r\n            \r\n            div.post div.file .fileThumb img {{\r\n                border: none; float: left;\r\n            }}\r\n            \r\n            .postblock {{\r\n                background-color: #282a2e;\r\n                color: #c5c8c6;\r\n                font-weight: 700;\r\n                padding: 0 5px;\r\n                font-size: 10pt;\r\n            }}\r\n            \r\n            .reply:target {{\r\n                background: #1d1d21!important;\r\n                border: 1px solid #111!important;\r\n                padding: 2px;\r\n            }}\r\n            \r\n            .deadlink {{\r\n                text-decoration: line-through;\r\n                color: #81a2be!important;\r\n            }}\r\n            \r\n            .oldpost {{\r\n                color: #c5c8c6;\r\n                font-weight: 700;\r\n            }}\r\n            \r\n            .fileText a {{\r\n                text-decoration: underline;\r\n            }}\r\n            \r\n            span.subject{{\r\n                color:#b294bb;\r\n                font-weight:700;\r\n            }}\r\n            \r\n            lockquote>span.quote {{\r\n                color:#b5bd68;\r\n            }}\r\n            \r\n            .center-screen {{\r\n                display: flex;\r\n                justify-content: center;\r\n                align-items: center;\r\n                text-align: center;\r\n            }}\r\n        </style>\r\n        <title></title>\r\n    </head>\r\n\r\n    <body>\r\n        <div class=\"boardBanner\">\r\n            <div class=\"boardTitle\">";
        private const string HtmlInit_Dirty = $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style> html {{ -moz-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }} body {{ background: #1d1f21 none; color: #c5c8c6; font-family: arial, helvetica, sans-serif; font-size: 10pt; margin-left: 0; margin-right: 0; margin-top: 5px; padding-left: 5px; padding-right: 5px; }} .CenteredSmall {{ width: 468px; max-width: 100%; }} a, a:visited {{ color: #81a2be!important; text-decoration: none; }} a.replylink:not(:hover), div#absbot a:not(:hover) {{ color: #81a2be!important; }} a:hover {{ color: #5f89ac!important; }} img {{ border: none; }} hr {{ border: none; border-top: 1px solid #282a2e; height: 0; }} div.boardBanner {{ text-align: center; clear: both; }} div.boardBanner>div.boardTitle {{ font-family: Tahoma, sans-serif; font-size: 28px; font-weight: 700; letter-spacing: -2px; margin-top: 0; }} div.boardBanner>div.boardSubtitle {{ font-size: x-small; }} div.post {{ margin: 5px 0; overflow: hidden; }} div.op {{ display: inline; }} div.reply {{ background-color: #282a2e; border: 1px solid #282a2e; display: table; padding: 2px; }} div.post div.postInfo span.nameBlock span.postertrip {{ color: #c5c8c6; font-weight: 400!important; }} div.post div.file .fileThumb {{ float: left; margin-left: 20px; margin-right: 20px; margin-top: 3px; margin-bottom: 5px; }} span.fileThumb {{ margin-left: 0!important; margin-right: 5px!important; }} div.post div.file .fileThumb img {{ border: none; float: left; }} .postblock {{ background-color: #282a2e; color: #c5c8c6; font-weight: 700; padding: 0 5px; font-size: 10pt; }} .reply:target {{ background: #1d1d21!important; border: 1px solid #111!important; padding: 2px; }} .deadlink {{ text-decoration: line-through; color: #81a2be!important; }} .oldpost {{ color: #c5c8c6; font-weight: 700; }} .fileText a {{ text-decoration: underline; }} span.subject{{ color:#b294bb;font-weight:700; }} blockquote>span.quote {{ color:#b5bd68; }} .center-screen {{ display: flex; justify-content: center; align-items: center; text-align: center; }} </style><title></title></head><body><div class=\"boardBanner\"><div class=\"boardTitle\">";

        public static string GetHTMLBase(ThreadInfo Info, bool CleanHTML) {
            if (CleanHTML) {
                return Info.Chan switch {
                    ChanType.FourChan => $"{HtmlInit_Clean}{Info.ThreadBoard} - {Chans.GetFullBoardName(Info.Chan, Info.ThreadBoard, true)}</div>\r\n            <div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div>\r\n        </div>\r\n\r\n        <hr class=\"CenteredSmall\" />",

                    ChanType.FourTwentyChan => $"{HtmlInit_Clean}{Info.ThreadBoard} - {Chans.GetFullBoardName(Info.Chan, Info.ThreadBoard, true)}</div>\r\n            <div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div>\r\n        </div>\r\n\r\n        <hr class=\"CenteredSmall\" />",

                    ChanType.SevenChan => $"{HtmlInit_Clean}{Info.ThreadBoard} - {Chans.GetFullBoardName(Info.Chan, Info.ThreadBoard, true)}</div>\r\n            <div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div>\r\n        </div>\r\n\r\n        <hr class=\"CenteredSmall\" />",

                    ChanType.EightKun => $"{HtmlInit_Clean}{Info.ThreadBoard}</div>\r\n            <div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div>\r\n        </div>\r\n\r\n        <hr class=\"CenteredSmall\" />",

                    _ => $"{HtmlInit_Clean}{Info.ThreadBoard}</div>\r\n            <div class=\"boardSubtitle\">{Info.ThreadBoard}</div>\r\n        </div>\r\n\r\n        <hr class=\"CenteredSmall\" />\r\n\r\n        <br/><center>This *chan isn't supported by the updated HTML archiver. This may be a program-sided issue.</center><br/>",
                };
            }
            else {
                return Info.Chan switch {
                    ChanType.FourChan => $"{HtmlInit_Dirty}{Info.ThreadBoard} - {Chans.GetFullBoardName(Info.Chan, Info.ThreadBoard, true)}</div><div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div></div><hr class=\"CenteredSmall\" />",

                    ChanType.FourTwentyChan => $"{HtmlInit_Dirty}{Info.ThreadBoard} - {Chans.GetFullBoardName(Info.Chan, Info.ThreadBoard, true)}</div><div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div></div><hr class=\"CenteredSmall\" />",

                    ChanType.SevenChan => $"{HtmlInit_Dirty}{Info.ThreadBoard} - {Chans.GetFullBoardName(Info.Chan, Info.ThreadBoard, true)}</div><div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div></div><hr class=\"CenteredSmall\" />",

                    ChanType.EightKun => $"{HtmlInit_Dirty}{Info.ThreadBoard}</div><div class=\"boardSubtitle\">{Chans.GetBoardSubtitle(Info.Chan, Info.ThreadBoard)}</div></div><hr class=\"CenteredSmall\" />",

                    _ => $"{HtmlInit_Dirty}{Info.ThreadBoard}</div><div class=\"boardSubtitle\">{Info.ThreadBoard}</div></div><hr class=\"CenteredSmall\" /><br/><center>This *chan isn't supported by the updated HTML archiver. This may be a program-sided issue.</center><br/>",
                };
            }
        }

        public static string GetPostForHTML(PostInfo Post, bool OP, bool CleanHTML) {
            string PostHTML = CleanHTML ? "\r\n\r\n" : "";
            string ThumbnailPath = Config.Settings.Downloads.SaveThumbnails ? "thumb/" + Post.PostFileID + "s.jpg" : Post.PostOutputFileName;
            if (CleanHTML) {
                if (OP) {
                    PostHTML += $"        <div class=\"postContainer opContainer\">\r\n            <div id=\"p{Post.PostID}\" class=\"post op\">\r\n                <div class=\"file\">\r\n                    <div class=\"fileText\">&nbsp; \r\n                        <a href=\"{Post.PostOutputFileName}\" target=\"_blank\">{Post.PostOriginalName}{Post.PostFileExtension}</a> ({Post.PostFileSize} B, {Post.PostWidth}x{Post.PostHeight})\r\n                    </div>\r\n                    <a class=\"fileThumb\" href=\"{Post.PostOutputFileName}\" target=\"_blank\"><img src=\"{ThumbnailPath}\" style=\"height: {Post.PostThumbnailHeight}px; width: {Post.PostThumbnailWidth}px;\"></a>\r\n                </div>\r\n            <div class=\"postInfo desktop\">&nbsp; <span class=\"subject\">{Post.PostSubject}</span> <span class=\"nameBlock\"><span class=\"name\" style=\"font-weight: bold; \">{Post.PosterName}</span> </span> <span class=\"dateTime\">{Post.PostDate}</span> <span class=\"postNum desktop\"><a href=\"#p{ Post.PostID}\">No.</a>{Post.PostID}</span></div>\r\n            <blockquote class=\"postMessage\">{Post.PostComment}</blockquote>\r\n            </div>\r\n        </div>";
                }
                else {
                    PostHTML += $"        <div class=\"postContainer replyContainer\">\r\n            <div id=\"p{Post.PostID}\" class=\"post reply\">\r\n                <div class=\"postInfo desktop\">\r\n                    <span class=\"nameBlock\"><span class=\"name\" style=\"font-weight: bold; \">&nbsp;{Post.PosterName}</span> </span> <span class=\"dateTime\">{Post.PostDate}</span> <span class=\"postNum desktop\"><a href=\"#p{Post.PostID}\">No.</a>{Post.PostID}&nbsp;</span>\r\n                </div>\r\n";

                    if (Post.PostContainsFile) {
                        PostHTML += $"                <div class=\"file\">\r\n                    <div class=\"fileText\">&nbsp; <a href=\"{Post.PostOutputFileName}\" target=\"_blank\">{Post.PostOriginalName}{Post.PostFileExtension}</a> ({Post.PostFileSize} B, {Post.PostWidth}x{Post.PostHeight})</div>\r\n                    <a class=\"fileThumb\" href=\"{Post.PostOutputFileName}\" target=\"_blank\"><img src=\"{ThumbnailPath}\" style=\"height: {Post.PostThumbnailHeight}px; width: {Post.PostThumbnailWidth}px;\"/></a>\r\n                </div>\r\n";
                    }

                    PostHTML += $"                <blockquote class=\"postMessage\">{Post.PostComment}</blockquote>\r\n            </div>\r\n        </div>";
                }
            }
            else {
                if (OP) {
                    PostHTML += $"<div class=\"postContainer opContainer\"><div id=\"p{Post.PostID}\" class=\"post op\"><div class=\"file\"><div class=\"fileText\">&nbsp; <a href=\"{Post.PostOutputFileName}\" target=\"_blank\">{Post.PostOriginalName}{Post.PostFileExtension}</a> ({Post.PostFileSize} B, {Post.PostWidth}x{Post.PostHeight})</div><a class=\"fileThumb\" href=\"{Post.PostOutputFileName}\" target=\"_blank\"><img src=\"{ThumbnailPath}\" style=\"height: {Post.PostThumbnailHeight}px; width: {Post.PostThumbnailWidth}px;\"></a></div><div class=\"postInfo desktop\">&nbsp; <span class=\"subject\">{Post.PostSubject}</span> <span class=\"nameBlock\"><span class=\"name\" style=\"font-weight: bold; \">{Post.PosterName}</span> </span> <span class=\"dateTime\">{Post.PostDate}</span> <span class=\"postNum desktop\"><a href=\"#p{Post.PostID}\">No.</a>{Post.PostID}</span></div><blockquote class=\"postMessage\">{Post.PostComment}</blockquote></div></div>";
                }
                else {
                    PostHTML += $"<div class=\"postContainer replyContainer\"><div id=\"p{Post.PostID}\" class=\"post reply\"><div class=\"postInfo desktop\"><span class=\"nameBlock\"><span class=\"name\" style=\"font-weight: bold; \">&nbsp;{Post.PosterName}</span> </span> <span class=\"dateTime\">{Post.PostDate}</span> <span class=\"postNum desktop\"><a href=\"#p{Post.PostID}\">No.</a>{Post.PostID}&nbsp;</span></div>";

                    if (Post.PostContainsFile) {
                        PostHTML += $"<div class=\"file\"><div class=\"fileText\">&nbsp; <a href=\"{Post.PostOutputFileName}\" target=\"_blank\">{Post.PostOriginalName}{Post.PostFileExtension}</a> ({Post.PostFileSize} B, {Post.PostWidth}x{Post.PostHeight})</div><a class=\"fileThumb\" href=\"{Post.PostOutputFileName}\" target=\"_blank\"><img src=\"{ThumbnailPath}\" style=\"height: {Post.PostThumbnailHeight}px; width: {Post.PostThumbnailWidth}px;\"/></a></div>";
                    }

                    PostHTML += $"<blockquote class=\"postMessage\">{Post.PostComment}</blockquote></div></div>";
                }
            }
            return PostHTML;
        }

        public static string GetHTMLFooter(ThreadInfo Info, bool CleanHTML) {
            return CleanHTML ?
                $"\r\n\r\n        <hr class=\"CenteredSmall\" />\r\n\r\n        <div>\r\n            [<a href=\"#top\">Top</a>]\r\n            [<a href=\"{Info.ThreadURL}\">Original thread</a>]\r\n            <a class=\"center-screen\" href=\"https://github.com/murrty/ychanex\">Ripped by YChanEx</a>\r\n        </div>\r\n    </body>\r\n</html>"
            :
                $"<hr class=\"CenteredSmall\"><div>[<a href=\"#top\">Top</a>][<a href=\"{Info.ThreadURL}\">Original thread</a>]<a class=\"center-screen\"href=\"https://github.com/murrty/ychanex\">Ripped by YChanEx</a></div></body></html>";
        }

        public static string GetReadableTime(double Time) {
            DateTime NewDate = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            NewDate = NewDate.AddSeconds(Time).ToLocalTime();
            return $"{NewDate.Year}/{NewDate.Month}/{NewDate.Day} ({NewDate.DayOfWeek.ToString()[..3]}) {NewDate.Hour}:{NewDate.Minute}:{NewDate.Second}";
        }

    }
}
