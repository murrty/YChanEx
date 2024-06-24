#nullable enable
namespace murrty.networking;
using System.Net;
using YChanEx;
internal static class CookieParser {
    public static Cookie? GetCookie(string? value, Uri hostUri) {
        if (value.IsNullEmptyWhitespace()) {
            return null;
        }

        string? cookieName = null;
        string? cookieValue = null;

        // Get the name and value, it's the first value in the string.
        int sepPos = value.IndexOf('=');
        int endPos = value.IndexOf(';');

        // Other optionals
        string cookiePath = "/";
        string cookieDomain = hostUri.Host;
        DateTime? expiresOn = null;
        bool httpOnly = false;
        bool secure = false;

        cookieName = value[..sepPos];
        if (endPos > -1) {
            cookieValue = value[(sepPos + 1)..endPos];

            if (endPos > value.Length - 1) {
                // Check for expire date
                int expirePos = value.IndexOf("expires=", StringComparison.OrdinalIgnoreCase);
                if (expirePos > -1) {

                }
            }
        }
        else {
            cookieValue = value[(sepPos + 1)..];
        }

        Cookie cookie = new(cookieName, cookieValue, cookiePath, cookieDomain) {
            HttpOnly = httpOnly,
            Secure = secure
        };
        if (expiresOn != null) {
            cookie.Expires = expiresOn.Value;
        }
        return cookie;
    }


}
