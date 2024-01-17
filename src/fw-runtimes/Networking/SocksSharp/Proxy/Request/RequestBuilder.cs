namespace SocksSharp.Proxy.Request;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using SocksSharp.Extensions;
internal class RequestBuilder {
    private readonly string newLine = "\r\n";

    private readonly HttpRequestMessage request;
    private readonly CookieContainer cookies;

    public RequestBuilder(HttpRequestMessage request) : this(request, null) { }

    public RequestBuilder(HttpRequestMessage request, CookieContainer cookies) {
        this.request = request;
        this.cookies = cookies;
    }

    public byte[] BuildStartingLine() {
        var uri = request.RequestUri;

        var startingLine = $"{request.Method.Method} {uri.PathAndQuery} HTTP/{request.Version}" + newLine;

        if (string.IsNullOrEmpty(request.Headers.Host)) {
            startingLine += "Host: " + uri.Host + newLine;
        }

        return ToByteArray(startingLine);
    }

    public byte[] BuildHeaders(bool hasContent) {
        var headers = GetHeaders(request.Headers);
        if (hasContent) {
            var contentHeaders = GetHeaders(request.Content.Headers);
            headers = !string.IsNullOrWhiteSpace(headers) ? string.Join(newLine, headers, contentHeaders) : contentHeaders;
        }

        return ToByteArray(headers + newLine + newLine);
    }

    private string GetHeaders(HttpHeaders headers) {
        List<string> headersList = [];

        foreach (var header in headers) {
            string headerKeyAndValue = string.Empty;

            if (header.Value is string[] values && values.Length < 2) {
                if (values.Length > 0 && !string.IsNullOrEmpty(values[0])) {
                    headerKeyAndValue = header.Key + ": " + values[0];
                }
            }
            else {
                string headerValue = headers.GetHeaderString(header.Key);
                if (!string.IsNullOrEmpty(headerValue)) {
                    headerKeyAndValue = header.Key + ": " + headerValue;
                }
            }

            if (!string.IsNullOrEmpty(headerKeyAndValue)) {
                headersList.Add(headerKeyAndValue);
            }
        }

        if (headers is HttpContentHeaders && !headersList.Contains("Content-Length")) {
            var content = headers as HttpContentHeaders;
            if (content.ContentLength > 0) {
                headersList.Add($"Content-Length: {content.ContentLength}");
            }
        }

        if (cookies != null) {
            var cookiesCollection = cookies.GetCookies(request.RequestUri);
            var rawCookies = "Cookie: ";

            foreach (var cookie in cookiesCollection) {
                rawCookies += cookie + "; ";
            }

            if (cookiesCollection.Count > 0) {
                headersList.Add(rawCookies);
            }
        }

        return string.Join("\r\n", [.. headersList]);
    }

    private byte[] ToByteArray(string data) {
        return Encoding.ASCII.GetBytes(data);
    }
}