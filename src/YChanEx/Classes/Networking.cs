#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using murrty.classes;
using murrty.controls;
internal static class Networking {
    public static readonly Cookie[] RequiredCookies;
    public static readonly bool Tls12OrHigher;
    public static CookieContainer CookieContainer;
    public static VolatileHttpClient CachedClient;

    public static VolatileHttpClient LatestClient {
        get {
            if (CachedClient.UpdateRequired()) {
                RecreateDownloadClient();
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
        CookieContainer = new();
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
                    AllowAutoRedirect = true,
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
                    AllowAutoRedirect = true,
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
                    AllowAutoRedirect = true,
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
                    AllowAutoRedirect = true,
                };
            } break;
            default: {
                NewHandler = new HttpClientHandler() {
                    UseCookies = true,
                    CookieContainer = CookieContainer,
                    AllowAutoRedirect = true,
                };
            } break;
        }

        VolatileHttpClient NewClient = new(NewHandler);
        CachedClient = NewClient;
    }
    public static void RefreshCookies() {
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

        //using var response = await DownloadClient.GetResponseAsync(new HttpRequestMessage(HttpMethod.Get, ""), default);
        //string? re = await response?.Content.ReadAsStringAsync();
    }
}
