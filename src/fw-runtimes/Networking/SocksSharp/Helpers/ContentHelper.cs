namespace SocksSharp.Helpers;
internal static class ContentHelper {
    public static bool IsContentHeader(string name) {
        //https://github.com/dotnet/corefx/blob/3e72ee5971db5d0bd46606fa672969adde29e307/src/System.Net.Http/src/System/Net/Http/Headers/KnownHeaders.cs
        string[] contentHeaders = [
            "Last-Modified",
            "Expires",
            "Content-Type",
            "Content-Range",
            "Content-MD5",
            "Content-Location",
            "Content-Length",
            "Content-Language",
            "Content-Encoding",
            "Allow"
        ];

        bool isContent = false;
        for (int i = 0; i < contentHeaders.Length; i++) {
            string header = contentHeaders[i];
            isContent = isContent || header.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        return isContent;
    }
}