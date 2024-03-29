﻿#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using murrty.classes;
using murrty.controls;
internal static class Networking {
    private static readonly FieldInfo _domainTable;
    private static readonly FieldInfo _mList;

    public static readonly Cookie[] RequiredCookies;
    public static readonly bool Tls12OrHigher;
    public static CookieContainer CookieContainer;
    public static VolatileHttpClient CachedClient;

    public static VolatileHttpClient LatestClient {
        get {
            if (CachedClient.UpdateRequired()) {
                Log.Debug("Client update required");
                RecreateDownloadClient();
            }
            return CachedClient;
        }
    }

    static Networking() {
        _domainTable = typeof(CookieContainer)
            .GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic);
        _mList = typeof(CookieContainer).Assembly.GetType("System.Net.PathList")
            .GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic);

        RequiredCookies = [
            Parsers.FChan.AccessCookie,
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
        RefreshCookies(false);
        RecreateDownloadClient();

        // 100 connection limit
        //ServicePointManager.DefaultConnectionLimit = 100;

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
                NewHandler = new SocksSharp.Socks4ClientHandler(new SocksSharp.Proxy.ProxySettings() {
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
                NewHandler = new SocksSharp.Socks4aClientHandler(new SocksSharp.Proxy.ProxySettings() {
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
                NewHandler = new SocksSharp.Socks5ClientHandler(new SocksSharp.Proxy.ProxySettings() {
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
    public static void RefreshCookies(bool InvalidateExisting) {
        if (InvalidateExisting) {
            CookieContainer.InvalidateCookies();
        }

        for (int i = 0; i < RequiredCookies.Length; i++) {
            CookieContainer.Add(RequiredCookies[i]);
        }
        if (Cookies.CookieList.Count > 0) {
            for (int i = 0; i < Cookies.CookieList.Count; i++) {
                CookieContainer.Add(Cookies.CookieList[i]);
            }
        }
    }

    public static string CleanURL(string url) {
        // replace the "http" with "https" protocol
        if (url.StartsWith("http://")) {
            url = url[7..];
        }

        // remove "www." since it's inferred by default
        if (url.StartsWith("www.")) {
            url = url[4..];
        }

        // append "https" protocol
        if (!url.StartsWith("https://")) {
            url = "https://" + url;
        }

        // trim the fragment and empty path
        return url.SubstringBeforeLastChar('#').TrimEnd('/');
    }
    public static string GetHost(string url) {
        int sepIndex;

        // remove 'http' protocol
        if (url.StartsWith("https://") || url.StartsWith("http://")) {
            sepIndex = url.IndexOf('/');
            if (sepIndex < 0) {
                return url;
            }
            return url[5..url.IndexOf('/', 8)].TrimStart(':', '/');
        }

        // no 'http' protocol (specifically identified)
        sepIndex = url.IndexOf('/');
        if (sepIndex < 0) {
            return url;
        }

        return url[..url.IndexOf('/')].TrimStart(':', '/');
    }
    public static string GetHostNameOnly(string url) {
        url = GetHost(url);

        string[] urlSplit = url.Split('.');
        // domain.com <- returns 'domain'
        if (urlSplit.Length == 2) {
            return urlSplit[0];
        }

        // example.domain.com       <- returns 'domain'
        // example.other.domain.com <- returns 'domain'
        return urlSplit[^2];
    }

    internal static CookieCollection GetAllCookies(this CookieContainer container) {
        var m_domainTable = (System.Collections.Hashtable)_domainTable.GetValue(container);
        CookieCollection result = [];
        foreach (System.Collections.DictionaryEntry element in m_domainTable) {
            var l = (System.Collections.SortedList)_mList.GetValue(element.Value);
            foreach (var e in l) {
                var cl = (CookieCollection)((System.Collections.DictionaryEntry)e).Value;
                foreach (Cookie fc in cl) {
                    result.Add(fc);
                }
            }
        }
        return result;
    }
    internal static void InvalidateCookies(this CookieContainer container) {
        foreach (System.Collections.DictionaryEntry element in (System.Collections.Hashtable)_domainTable.GetValue(container)) {
            foreach (var e in (System.Collections.SortedList)_mList.GetValue(element.Value)) {
                foreach (Cookie fc in (CookieCollection)((System.Collections.DictionaryEntry)e).Value) {
                    fc.Expired = true;
                }
            }
        }
    }
}
