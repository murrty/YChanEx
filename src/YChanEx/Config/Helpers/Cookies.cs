#nullable enable
namespace YChanEx;
using System.IO;
using System.Net;
internal static class Cookies {
    public static List<SimpleCookie> CookieList;
    private static readonly string CookiesPath;
    static Cookies() {
        CookiesPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "cookies.json";
        if (File.Exists(CookiesPath)) {
            CookieList = File.ReadAllText(CookiesPath).JsonDeserialize<List<SimpleCookie>>() ?? [];
        }
        else {
            CookieList = [];
        }
    }
    public static void AddCookie(SimpleCookie cookie) {
        if (CookieList.Contains(cookie)) {
            return;
        }
        CookieList.Add(cookie);
        File.WriteAllText(CookiesPath, CookieList.JsonSerialize());
    }
    public static void RemoveCookie(SimpleCookie cookie) {
        CookieList.Remove(cookie);
        File.WriteAllText(CookiesPath, CookieList.JsonSerialize());
    }
}