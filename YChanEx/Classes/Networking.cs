namespace YChanEx;

using System.IO;
using System.Net;

internal class Networking {
    public static string DownloadString(string InputURL, DateTime ModifiedSince = default) {
        try {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(InputURL);
            Request.UserAgent = Config.Settings.Advanced.UserAgent;
            Request.Method = "GET";
            if (ModifiedSince != default)
                Request.IfModifiedSince = ModifiedSince;
            using HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            using Stream ResponseStream = Response.GetResponseStream();
            using StreamReader Reader = new(ResponseStream);
            return Reader.ReadToEnd();
        }
        catch {
            throw;
        }
    }

    public static string GetAPILink(ThreadInfo Thread) {
        return Thread.Chan switch {
            ChanType.FourChan => $"https://a.4cdn.org/{Thread.Data.ThreadBoard}/thread/{Thread.Data.ThreadID}.json",
            ChanType.FourTwentyChan => $"https://api.420chan.org/{Thread.Data.ThreadBoard}/res/{Thread.Data.ThreadID}.json",
            ChanType.EightChan => $"https://8chan.moe/{Thread.Data.ThreadBoard}/res/{Thread.Data.ThreadID}.json",
            ChanType.EightKun => $"https://8kun.top/{Thread.Data.ThreadBoard}/res/{Thread.Data.ThreadID}.json",
            _ => null
        };
    }

    public static string CleanURL(string URL) {
        if (URL.StartsWith("http://")) {
            URL = URL[7..];
        }
        if (URL.StartsWith("www.")) {
            URL = URL[4..];
        }
        if (!URL.StartsWith("https://")) {
            URL = "https://" + URL;
        }
        return URL;
    }

}