namespace YChanEx;
using System.IO;
using System.Net;
internal static class Cookies {
    public static List<Cookie> CookieList;
    private static readonly string CookiesPath;
    static Cookies() {
        CookiesPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "cookies.json";
        if (File.Exists(CookiesPath)) {
            CookieList = File.ReadAllText(CookiesPath).JsonDeserialize<List<Cookie>>();
        }
        else {
            CookieList = [];
        }
    }
    public static void AddCookie(Cookie cookie) {
        if (CookieList.Contains(cookie)) {
            return;
        }
        CookieList.Add(cookie);
        File.WriteAllText(CookiesPath, CookieList.JsonSerialize());
    }
    public static void RemoveCookie(Cookie cookie) {
        CookieList.Remove(cookie);
        File.WriteAllText(CookiesPath, CookieList.JsonSerialize());
    }
}