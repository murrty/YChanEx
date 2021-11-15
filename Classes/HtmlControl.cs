using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YChanEx {
    class HtmlControl {
        public static string GetHTMLBase(ChanType Chan, ThreadInfo Info) {
            switch (Chan) {
                case ChanType.FourChan:
                    return "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"utf-8\">\r\n" +
                                      "<style> html { -moz-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; } body { background: #1d1f21 none; color: #c5c8c6; font-family: arial, helvetica, sans-serif; font-size: 10pt; margin-left: 0; margin-right: 0; margin-top: 5px; padding-left: 5px; padding-right: 5px; } .CenteredSmall { width: 468px; max-width: 100%; } a, a:visited { color: #81a2be!important; text-decoration: none; } a.replylink:not(:hover), div#absbot a:not(:hover) { color: #81a2be!important; } a:hover { color: #5f89ac!important; } img { border: none; } hr { border: none; border-top: 1px solid #282a2e; height: 0; } div.boardBanner { text-align: center; clear: both; } div.boardBanner>div.boardTitle { font-family: Tahoma, sans-serif; font-size: 28px; font-weight: 700; letter-spacing: -2px; margin-top: 0; } div.boardBanner>div.boardSubtitle { font-size: x-small; } div.post { margin: 5px 0; overflow: hidden; } div.op { display: inline; } div.reply { background-color: #282a2e; border: 1px solid #282a2e; display: table; padding: 2px; } div.post div.postInfo span.nameBlock span.postertrip { color: #c5c8c6; font-weight: 400!important; } div.post div.file .fileThumb { float: left; margin-left: 20px; margin-right: 20px; margin-top: 3px; margin-bottom: 5px; } span.fileThumb { margin-left: 0!important; margin-right: 5px!important; } div.post div.file .fileThumb img { border: none; float: left; } .postblock { background-color: #282a2e; color: #c5c8c6; font-weight: 700; padding: 0 5px; font-size: 10pt; } .reply:target { background: #1d1d21!important; border: 1px solid #111!important; padding: 2px; } .deadlink { text-decoration: line-through; color: #81a2be!important; } .oldpost { color: #c5c8c6; font-weight: 700; } .fileText a { text-decoration: underline; } span.subject{ color:#b294bb;font-weight:700; } blockquote>span.quote { color:#b5bd68; } .center-screen { display: flex; justify-content: center; align-items: center; text-align: center; } </style>\r\n" +
                                      "<title></title>\r\n" +
                                      "</head>\r\n\r\n<body>\r\n<div class=\"boardBanner\">\r\n<div class=\"boardTitle\">" + Info.ThreadBoard + " - " + BoardTitles.FourChan(Info.BoardName, true) + "</div>\r\n<div class=\"boardSubtitle\">" + BoardSubtitles.GetSubtitle(Chan, Info.ThreadBoard) + "</div>\r\n</div>" +
                                      "<hr class=\"CenteredSmall\">";
                case ChanType.EightKun:
                    return "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<meta charset=\"utf-8\">\r\n" +
                        "<style> html { -moz-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; } body { background: #1d1f21 none; color: #c5c8c6; font-family: arial, helvetica, sans-serif; font-size: 10pt; margin-left: 0; margin-right: 0; margin-top: 5px; padding-left: 5px; padding-right: 5px; } .CenteredSmall { width: 468px; max-width: 100%; } a, a:visited { color: #81a2be!important; text-decoration: none; } a.replylink:not(:hover), div#absbot a:not(:hover) { color: #81a2be!important; } a:hover { color: #5f89ac!important; } img { border: none; } hr { border: none; border-top: 1px solid #282a2e; height: 0; } div.boardBanner { text-align: center; clear: both; } div.boardBanner>div.boardTitle { font-family: Tahoma, sans-serif; font-size: 28px; font-weight: 700; letter-spacing: -2px; margin-top: 0; } div.boardBanner>div.boardSubtitle { font-size: x-small; } div.post { margin: 5px 0; overflow: hidden; } div.op { display: inline; } div.reply { background-color: #282a2e; border: 1px solid #282a2e; display: table; padding: 2px; } div.post div.postInfo span.nameBlock span.postertrip { color: #c5c8c6; font-weight: 400!important; } div.post div.file .fileThumb { float: left; margin-left: 20px; margin-right: 20px; margin-top: 3px; margin-bottom: 5px; } span.fileThumb { margin-left: 0!important; margin-right: 5px!important; } div.post div.file .fileThumb img { border: none; float: left; } .postblock { background-color: #282a2e; color: #c5c8c6; font-weight: 700; padding: 0 5px; font-size: 10pt; } .reply:target { background: #1d1d21!important; border: 1px solid #111!important; padding: 2px; } .deadlink { text-decoration: line-through; color: #81a2be!important; } .oldpost { color: #c5c8c6; font-weight: 700; } .fileText a { text-decoration: underline; } span.subject{ color:#b294bb;font-weight:700; } blockquote>span.quote { color:#b5bd68; } .center-screen { display: flex; justify-content: center; align-items: center; text-align: center; } </style>\r\n" +
                        "<title></title>\r\n" +
                        "</head>\r\n\r\n<body>\r\n<div class=\"boardBanner\">\r\n<div class=\"boardTitle\">" + Info.ThreadBoard + "</div>\r\n<div class=\"boardSubtitle\">" + BoardSubtitles.GetSubtitle(Chan, Info.ThreadBoard) + "</div>\r\n</div>" +
                        "<hr class=\"CenteredSmall\">";
                default:
                    return null;
            }
        }

        public static string GetPostForHTML(PostInfo Post, bool OP) {
            string PostHTML = "\r\n";
            string ThumbnailPath;
            if (Downloads.Default.SaveThumbnails) {
                ThumbnailPath = "thumb/" + Post.PostFileID + "s.jpg";
            }
            else {
                ThumbnailPath = Post.PostOutputFileName;
            }

            if (OP) {
                PostHTML += "<div class=\"postContainer opContainer\">\r\n" +
                    "<div id=\"p" + Post.PostID + "\" class=\"post op\">\r\n" +
                    //"<div class=\"file\"><div class=\"fileText\">&nbsp; <a href=\"" + Post.PostOutputFileName + "\" target=\"_blank\">" + Post.PostOriginalName + Post.PostFileExtension + "</a> (" + Post.PostFileSize + " B, " + Post.PostWidth + "x" + Post.PostHeight + ")</div><a class=\"fileThumb\" href=\"" + Post.PostOutputFileName + "\" target=\"_blank\"><img src=\"thumb/" + Post.PostFileID + "s.jpg\" style=\"height: " + Post.PostThumbnailHeight + "px; width: " + Post.PostThumbnailWidth + "px;\"></a></div>\r\n" +
                    "<div class=\"file\"><div class=\"fileText\">&nbsp; <a href=\"" + Post.PostOutputFileName + "\" target=\"_blank\">" + Post.PostOriginalName + Post.PostFileExtension + "</a> (" + Post.PostFileSize + " B, " + Post.PostWidth + "x" + Post.PostHeight + ")</div><a class=\"fileThumb\" href=\"" + Post.PostOutputFileName + "\" target=\"_blank\"><img src=\"" + ThumbnailPath + "\" style=\"height: " + Post.PostThumbnailHeight + "px; width: " + Post.PostThumbnailWidth + "px;\"></a></div>\r\n" +
                    "<div class=\"postInfo desktop\">&nbsp; <span class=\"subject\">" + Post.PostSubject + "</span> <span class=\"nameBlock\"><span class=\"name\">" + Post.PosterName + "</span> </span> <span class=\"dateTime\">" + Post.PostDate + "</span> <span class=\"postNum desktop\"><a href=\"#p" + Post.PostID + "\">No.</a>" + Post.PostID + "</span></div>\r\n" +
                    "<blockquote class=\"postMessage\">" + Post.PostComment + "</blockquote>\r\n</div>\r\n</div>";
            }
            else {
                PostHTML += "<div class=\"postContainer replyContainer\">" +
                    "<div id=\"p" + Post.PostID + "\" class=\"post reply\">" +
                    "<div class=\"postInfo desktop\"><span class=\"nameBlock\"><span class=\"name\">&nbsp;" + Post.PosterName + "</span> </span> <span class=\"dateTime\">" + Post.PostDate + "</span> <span class=\"postNum desktop\"><a href=\"#p" + Post.PostID + "\">No.</a>" + Post.PostID + "&nbsp;</span></div>\r\n";
                if (Post.PostContainsFile) {
                    PostHTML += "<div class=\"file\">\r\n" +
                        //"<div class=\"fileText\">&nbsp; <a href=\"" + Post.PostOutputFileName + "\" target=\"_blank\">" + Post.PostOriginalName + Post.PostFileExtension + "</a> (" + Post.PostFileSize + " B, " + Post.PostWidth + "x" + Post.PostHeight + ")</div><a class=\"fileThumb\" href=\"" + Post.PostOutputFileName + "\" target=\"_blank\"><img src=\"thumb\\" + Post.PostFileID + "s.jpg\" style=\"height: " + Post.PostThumbnailHeight + "px; width: " + Post.PostThumbnailWidth + "px;\"></a>" +
                        "<div class=\"fileText\">&nbsp; <a href=\"" + Post.PostOutputFileName + "\" target=\"_blank\">" + Post.PostOriginalName + Post.PostFileExtension + "</a> (" + Post.PostFileSize + " B, " + Post.PostWidth + "x" + Post.PostHeight + ")</div><a class=\"fileThumb\" href=\"" + Post.PostOutputFileName + "\" target=\"_blank\"><img src=\"" + ThumbnailPath + "\" style=\"height: " + Post.PostThumbnailHeight + "px; width: " + Post.PostThumbnailWidth + "px;\"></a>" +
                        
                        "\r\n</div>\r\n";
                }
                PostHTML += "<blockquote class=\"postMessage\">" + Post.PostComment + "</blockquote>\r\n</div>\r\n</div>\r\n";
            }
            return PostHTML;
        }

        public static string GetHTMLFooter() {
            return "\r\n<hr class=\"CenteredSmall\">\r\n<div>[<a href=\"#top\">Top</a>]<a class=\"center-screen\"href=\"https://github.com/murrty/ychanex\">Ripped by YChanEx</a></div>\r\n</body>\r\n</html>";
        }

        public static string GetReadableTime(double Time) {
            DateTime NewDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            NewDate = NewDate.AddSeconds(Time).ToLocalTime();

            return NewDate.Year + "/" +
                   NewDate.Month + "/" +
                   NewDate.Day +
                   " (" + NewDate.DayOfWeek.ToString().Substring(0, 3) + ") " +
                   NewDate.Hour + ":" +
                   NewDate.Minute + ":" +
                   NewDate.Second;
        }
    }
}
