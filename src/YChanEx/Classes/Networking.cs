namespace YChanEx;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using murrty.controls;

internal static class Networking {
    public static string DownloadString(string InputURL, DateTime ModifiedSince = default) {
        try {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(InputURL);
            Request.UserAgent = Advanced.UserAgent;
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
            ChanType.FourChan => $"https://a.4cdn.org/{Thread.Data.Board}/thread/{Thread.Data.Id}.json",
            ChanType.FourTwentyChan => $"https://api.420chan.org/{Thread.Data.Board}/res/{Thread.Data.Id}.json",
            ChanType.EightChan => $"https://8chan.moe/{Thread.Data.Board}/res/{Thread.Data.Id}.json",
            ChanType.EightKun => $"https://8kun.top/{Thread.Data.Board}/res/{Thread.Data.Id}.json",
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
        return URL.Split('#')[0];
    }

    public static async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, HttpClient downloadClient, CancellationToken token) {
        HttpResponseMessage Response;
        int Retries = 0;

        while (true) {
            try {
                Response = await downloadClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                    .ConfigureAwait(false);

                if (!Response.IsSuccessStatusCode) {
#if !NET6_0_OR_GREATER // Auto-redirect, for 308+ on framework.
                    if (((int)Response.StatusCode is > 304 and < 400) && Response.Headers.Location is not null) {
                        request.RequestUri = Response.Headers.Location;
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        return await GetResponseAsync(request, downloadClient, token)
                            .ConfigureAwait(false);
                    }
#endif // Auto-redirect, for 308+ on framework.

                    if ((int)Response.StatusCode > 499 && (++Retries) < 5) {
                        continue;
                    }

                    return null;
                }
            }
            catch {
                return null;
            }

            break;
        }

        return Response;
    }
    public static async Task<string> GetStringAsync(HttpResponseMessage response, CancellationToken token) {
        using var Content = await response.Content.ReadAsStreamAsync();
        using MemoryStream Destination = new();

        byte[] buffer = new byte[81920];
        int bytesRead;

        while ((bytesRead = await Content.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0) {
            await Destination.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }

        byte[] Bytes;
        switch (response.Content.Headers.ContentEncoding.FirstOrDefault()) {
            case "br": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Brotli(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            case "gzip": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.GZip(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            case "deflate": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Deflate(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            default: {
                Bytes = Destination.ToArray();
                Destination.Close();
            } break;
        }

        return (response.Content.Headers.ContentType.CharSet ?? "utf-8").ToLowerInvariant() switch {
            "ascii" => Encoding.ASCII.GetString(Bytes),
            "utf-7" => Encoding.UTF7.GetString(Bytes),
            "utf-32" => Encoding.UTF32.GetString(Bytes),
            "utf-16" or "unicode" => Encoding.Unicode.GetString(Bytes),
            "utf-16-be" or "utf-16be" or "unicode-be" or "unicodebe" => Encoding.BigEndianUnicode.GetString(Bytes),
            _ => Encoding.UTF8.GetString(Bytes),
        };
    }
}
