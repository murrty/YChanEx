#nullable enable
namespace YChanEx.Parsers;
internal static class FourChan {
    public static string? GetHtmlTitle(ThreadData data) {
        if (data.ThreadName == null) {
            return null;
        }
        return GetHtmlTitle(data.Board, data.ThreadName);
    }
    public static string GetHtmlTitle(string board, string name) {
        return $"/{board}/ - {name} - 4chan";
    }

    public static DateTimeOffset GetPostTime(long timestamp) {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp);
    }
    public static string CalculatePackedBase64Hash(string md5) {
        // 24 character, packed base64 MD5 hash of file
        // Requires a query-string to bypass cloudflare caching which changes the hash.

        byte[] raw_bytes = new byte[16];
        for (int i = 0; i < 32; i += 2) {
            raw_bytes[i / 2] = Convert.ToByte(md5[i..(i + 2)], 16);
        }
        return Convert.ToBase64String(raw_bytes);
    }
}
