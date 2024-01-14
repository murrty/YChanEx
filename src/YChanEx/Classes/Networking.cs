#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using murrty.controls;
internal static class Networking {
    private static ulong Iteration;

    public static readonly Cookie[] RequiredCookies;
    public static readonly bool Tls12OrHigher;
    public static CookieContainer CookieContainer;
    public static VolatileHttpClient CachedClient;

    public static VolatileHttpClient LatestClient {
        get {
            if (CachedClient.UpdateRequired()) {
                RecreateDownloadClient();
                Iteration++;
                CachedClient.Iteration = Iteration;
            }
            return CachedClient;
        }
    }

    static Networking() {
        RequiredCookies = [
            new Cookie("disclaimer", "seen", "/", "fchan.us"),
            new Cookie("disclaimer1",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer2",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer3",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer4",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer5",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer6",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer7",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer8",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer9",  "1", "/", "8chan.moe"),
            new Cookie("disclaimer10", "1", "/", "8chan.moe"),
            new Cookie("disclaimer11", "1", "/", "8chan.moe"),
            new Cookie("disclaimer12", "1", "/", "8chan.moe"),
            new Cookie("disclaimer13", "1", "/", "8chan.moe"),
            new Cookie("disclaimer14", "1", "/", "8chan.moe"),
            new Cookie("disclaimer15", "1", "/", "8chan.moe"),
            new Cookie("disclaimer16", "1", "/", "8chan.moe"),
            new Cookie("disclaimer17", "1", "/", "8chan.moe"),
            new Cookie("disclaimer18", "1", "/", "8chan.moe"),
            new Cookie("disclaimer19", "1", "/", "8chan.moe"),
            new Cookie("disclaimer20", "1", "/", "8chan.moe")];
        RefreshCookies();
        RecreateDownloadClient();

        // 100 connection limit
        ServicePointManager.DefaultConnectionLimit = 100;

        // Try to load TLS 1.3 (Win 11+)
        // It will use 1.2 if 1.3 is not available, regardless of if the first try works.
        //Tls12OrHigher = true;
        try {
#if NETCOREAPP
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13
#else
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288
#endif
                                                 | SecurityProtocolType.Tls12 // 3072
                                                 | SecurityProtocolType.Tls11 // 768
                                                 | SecurityProtocolType.Tls; // 192
            Tls12OrHigher = true;
        }
        catch (NotSupportedException) {
            try {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 // 3072
                                                     | SecurityProtocolType.Tls11 // 768
                                                     | SecurityProtocolType.Tls; // 192
                Tls12OrHigher = true;
            }
            catch (NotSupportedException) {
                //Tls12OrHigher = false;
                try {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 // 768
                                                         | SecurityProtocolType.Tls; // 192
                    Tls12OrHigher = false;
                }
                catch (NotSupportedException) {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // 192
                    Tls12OrHigher = false;
                }
            }
        }
    }

    [MemberNotNull(nameof(CachedClient))]
    public static void RecreateDownloadClient() {
        HttpMessageHandler NewHandler;
        switch (Initialization.Proxy.ProxyType) {
            case ProxyType.HTTP when Initialization.UseProxy: {
                NewHandler = new HttpClientHandler() {
                    Proxy = new WebProxy(Initialization.Proxy.IP, Initialization.Proxy.Port),
                    UseProxy = true,
                    UseCookies = true,
                    CookieContainer = CookieContainer,
                };
            } break;
            case ProxyType.SOCKS4 when Initialization.UseProxy: {
                NewHandler = new SocksSharp.Socks4ProxyClientHandler(new SocksSharp.Proxy.ProxySettings() {
                    Host = Initialization.Proxy.IP,
                    Port = Initialization.Proxy.Port,
                    ConnectTimeout = VolatileHttpClient.DefaultTimeout,
                    ReadWriteTimeOut = VolatileHttpClient.DefaultTimeout,
                }) {
                    UseCookies = true,
                    CookieContainer = CookieContainer,
                };
            } break;
            case ProxyType.SOCKS4A when Initialization.UseProxy: {
                NewHandler = new SocksSharp.Socks4aProxyClientHandler(new SocksSharp.Proxy.ProxySettings() {
                    Host = Initialization.Proxy.Domain ?? Initialization.Proxy.IP,
                    Port = Initialization.Proxy.Port,
                    ConnectTimeout = VolatileHttpClient.DefaultTimeout,
                    ReadWriteTimeOut = VolatileHttpClient.DefaultTimeout,
                }) {
                    UseCookies = true,
                    CookieContainer = CookieContainer,
                };
            } break;
            case ProxyType.SOCKS5 when Initialization.UseProxy: {
                NewHandler = new SocksSharp.Socks5ProxyClientHandler(new SocksSharp.Proxy.ProxySettings() {
                    Host = Initialization.Proxy.Domain ?? Initialization.Proxy.IP,
                    Port = Initialization.Proxy.Port,
                    ConnectTimeout = VolatileHttpClient.DefaultTimeout,
                    ReadWriteTimeOut = VolatileHttpClient.DefaultTimeout,
                }) {
                    UseCookies = true,
                    CookieContainer = CookieContainer,
                };
            } break;
            default: {
                NewHandler = new HttpClientHandler() {
                    UseCookies = true,
                    CookieContainer = CookieContainer,
                };
            } break;
        }

        VolatileHttpClient NewClient = new(NewHandler);
        CachedClient = NewClient;
    }

    [MemberNotNull(nameof(CookieContainer))]
    public static void RefreshCookies() {
        CookieContainer = new();
        for (int i = 0; i < RequiredCookies.Length; i++) {
            CookieContainer.Add(RequiredCookies[i]);
        }
        if (Cookies.CookieList.Count > 0) {
            for (int i = 0; i < Cookies.CookieList.Count; i++) {
                CookieContainer.Add(Cookies.CookieList[i]);
            }
        }
    }

    public static string DownloadString(string url, DateTime ModifiedSince = default) {
        try {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
            Request.UserAgent = Program.UserAgent;
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
    public static string CleanURL(string url) {
        if (url.StartsWith("http://")) {
            url = url[7..];
        }
        if (url.StartsWith("www.")) {
            url = url[4..];
        }
        if (!url.StartsWith("https://")) {
            url = "https://" + url;
        }
        return url.SubstringBeforeLastChar('#').TrimEnd('/');
    }
    public static string GetHost(string url) {
        if (url.StartsWith("https://") || url.StartsWith("http://")) {
            return url[5..url.IndexOf('/', 8)].TrimStart(':', '/');
        }
        return url[..url.IndexOf('/')].TrimStart(':', '/');
    }

    public static async Task Test() {
        HttpClientHandler DownloadClientHandler = new() {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        HttpClient DownloadClient = new(DownloadClientHandler);
        DownloadClient.DefaultRequestHeaders.Accept.Add(new("*/*"));
        //DownloadClient.DefaultRequestHeaders.AcceptEncoding.Add(new("br"));
        DownloadClient.DefaultRequestHeaders.AcceptEncoding.Add(new("gzip"));
        DownloadClient.DefaultRequestHeaders.AcceptEncoding.Add(new("deflate"));
        DownloadClient.DefaultRequestHeaders.AcceptLanguage.Add(new("*"));
        DownloadClient.DefaultRequestHeaders.ConnectionClose = false;
        DownloadClient.DefaultRequestHeaders.UserAgent.ParseAdd(Program.UserAgent);

        await Task.Delay(1);

        //using var response = await GetResponseAsync(new HttpRequestMessage(HttpMethod.Get, ""), DownloadClient, default);

        //string? re = await response?.Content.ReadAsStringAsync();
    }

    public static Task<HttpResponseMessage?> GetResponseAsync(HttpRequestMessage request, CancellationToken token) {
        return GetResponseAsync(request, LatestClient, token);
    }
    public static async Task<HttpResponseMessage?> GetResponseAsync(HttpRequestMessage request, HttpClient DownnloadClient, CancellationToken token) {
        HttpResponseMessage? Response = null;
        int Retries = 0;

        while (true) {
            try {
                Response = await DownnloadClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                    .ConfigureAwait(false);

                if (!Response.IsSuccessStatusCode) {
#if !NET6_0_OR_GREATER // Auto-redirect, for 308+ on framework.
                    if (((int)Response.StatusCode is > 304 and < 400) && Response.Headers.Location is not null) {
                        request.RequestUri = Response.Headers.Location;
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        return await GetResponseAsync(request, DownnloadClient, token)
                            .ConfigureAwait(false);
                    }
#endif // Auto-redirect, for 308+ on framework.

                    if ((int)Response.StatusCode > 499 && (++Retries) < 5) {
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        continue;
                    }

                    Response.Dispose();
                    return null;
                }
            }
            catch {
                Response?.Dispose();
                return null;
            }

            break;
        }

        return Response;
    }
    public static async Task<string> GetStringAsync(HttpResponseMessage Response, CancellationToken token) {
        using Stream Content = await Response.Content.ReadAsStreamAsync();
        using MemoryStream Destination = new();

        await CachedClient.WriteStreamAsync(Content, Destination, token);
        await Destination.FlushAsync();

        byte[] Bytes;
        switch (Response.Content.Headers.ContentEncoding.FirstOrDefault()) {
            case "br": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Brotli(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            }
            break;
            case "gzip": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.GZip(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            }
            break;
            case "deflate": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Deflate(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            }
            break;
            default: {
                Bytes = Destination.ToArray();
                Destination.Close();
            }
            break;
        }

        return (Response.Content.Headers.ContentType.CharSet ?? "utf-8").ToLowerInvariant() switch {
            "ascii" => Encoding.ASCII.GetString(Bytes),
            "utf-7" => Encoding.UTF7.GetString(Bytes),
            "utf-32" => Encoding.UTF32.GetString(Bytes),
            "utf-16" or "unicode" => Encoding.Unicode.GetString(Bytes),
            "utf-16-be" or "utf-16be" or "unicode-be" or "unicodebe" => Encoding.BigEndianUnicode.GetString(Bytes),
            _ => Encoding.UTF8.GetString(Bytes),
        };
    }
}
