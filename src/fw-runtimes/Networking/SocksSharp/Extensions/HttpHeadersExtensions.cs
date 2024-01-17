namespace SocksSharp.Extensions;
using System.Net.Http.Headers;
internal static class HttpHeadersExtensions {
    public static string GetHeaderString(this HttpHeaders headers, string key) {
        if (headers == null) {
            throw new ArgumentNullException(nameof(headers));
        }

        if (string.IsNullOrEmpty(key)) {
            throw new ArgumentNullException(nameof(key));
        }

        string value = string.Empty;
        string separator = key.Equals("User-Agent") ? " " : ", ";

        if (headers.TryGetValues(key, out IEnumerable<string> values) && values.Count() > 1) {
            value = string.Join(separator, values.ToArray());
        }

        return value;
    }
}
