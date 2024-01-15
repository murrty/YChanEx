namespace SocksSharp.Extensions;
using System.Net.Http.Headers;
internal static class HttpHeadersExtensions {
    public static string GetHeaderString(this HttpHeaders headers, string key) {
        if (headers == null) {
            throw new ArgumentNullException(nameof(headers));
        }

        if (String.IsNullOrEmpty(key)) {
            throw new ArgumentNullException(nameof(key));
        }

        string value = String.Empty;

        headers.TryGetValues(key, out IEnumerable<string> values);
        string separator = key.Equals("User-Agent") ? " " : ", ";

        if (values?.Count() > 1) {
            value = String.Join(separator, values.ToArray());
        }

        return value;
    }
}
