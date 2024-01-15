#nullable enable
namespace YChanEx;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using murrty.controls;
internal sealed class VolatileHttpClient : HttpClient {
    private const int DefaultBuffer = 81920;
    /// <summary>
    /// Represents the lowest timeout possible, 5 seconds (in milliseconds).
    /// </summary>
    internal const int LowestTimeout = 5_000;
    /// <summary>
    /// Represents the default timeout, 1 minute (in milliseconds).
    /// </summary>
    internal const int DefaultTimeout = 60_000;
    /// <summary>
    /// Represents the highest timeout possible, 30 minutes (in milliseconds).
    /// </summary>
    internal const int HighestTimeout = 1_800_000;

    internal delegate Task DownloadStream(Stream HttpStream, Stream Output, CancellationToken token);

    private readonly Proxy _proxy;
    private readonly bool _useProxy;
    private readonly bool _throttle;
    private readonly int _throttleSize;
    private readonly int _throttleBuffer;
    private readonly int _timeout;
    public HttpMessageHandler SetHandler;

    public VolatileHttpClient(HttpMessageHandler NewHandler) : base(NewHandler) {
        this.SetHandler = NewHandler;
        this._proxy = Initialization.Proxy;
        this._useProxy = Initialization.UseProxy;
        this._timeout = Initialization.Timeout;

        if (Initialization.UseThrottling) {
            this._throttle = true;
            this._throttleSize = Initialization.ThrottleSize;
            this._throttleBuffer = Math.Min(_throttleSize, DefaultBuffer);
            this.WriteStreamAsync = ThrottledWriteToStreamAsync;
        }
        else {
            this._throttle = false;
            this._throttleSize = 0;
            this.WriteStreamAsync = WriteToStreamAsync;
        }

        this.DefaultRequestHeaders.Accept.Add(new("*/*"));
        this.DefaultRequestHeaders.AcceptEncoding.Add(new("br"));
        this.DefaultRequestHeaders.AcceptEncoding.Add(new("gzip"));
        this.DefaultRequestHeaders.AcceptEncoding.Add(new("deflate"));
        this.DefaultRequestHeaders.AcceptLanguage.Add(new("*"));
        this.DefaultRequestHeaders.ConnectionClose = false;
        this.DefaultRequestHeaders.UserAgent.ParseAdd(Program.UserAgent);
        this.Timeout = new(0, 0, 0, 0, _timeout);
    }
    public bool UpdateRequired() {
        if (Initialization.UseProxy) {
            if (Initialization.UseProxy != _useProxy) {
                return true;
            }
            if (_proxy != Initialization.Proxy) {
                return true;
            }
        }
        else if (_useProxy) {
            return true;
        }

        if (Initialization.UseThrottling) {
            if (_throttleSize != Initialization.ThrottleSize) {
                return true;
            }
        }
        else if (_throttle) {
            return true;
        }

        return this._timeout != Initialization.Timeout;
    }

    public DownloadStream WriteStreamAsync { get; }
    internal async Task WriteToStreamAsync(Stream HttpStream, Stream Output, CancellationToken token) {
        byte[] buffer = new byte[DefaultBuffer];
        int bytesRead;

        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0) {
            await Output.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }
    internal async Task ThrottledWriteToStreamAsync(Stream HttpStream, Stream Output, CancellationToken token) {
        byte[] buffer = new byte[_throttleBuffer];
        int bytesRead;

        using ThrottledStream ThrottledWriter = new(Output, _throttleSize);
        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) != 0) {
            await ThrottledWriter.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }

    public async Task<HttpResponseMessage?> GetResponseAsync(HttpRequestMessage request, CancellationToken token) {
        HttpResponseMessage? Response = null;
        int Retries = 0;
        while (true) {
            try {
                Response = await SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                    .ConfigureAwait(false);

                if (!Response.IsSuccessStatusCode) {
#if !NET6_0_OR_GREATER // Auto-redirect, for 308+ on framework.
                    if (((int)Response.StatusCode is > 304 and < 400) && Response.Headers.Location is not null) {
                        request.RequestUri = Response.Headers.Location;
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        return await GetResponseAsync(request, token)
                            .ConfigureAwait(false);
                    }
#endif // Auto-redirect, for 308+ on framework.

                    if ((int)Response.StatusCode > 499 && (++Retries) < 5) {
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        continue;
                    }
                }
            }
            catch { }
            break;
        }

        return Response;
    }
    public async Task<string> GetStringAsync(HttpResponseMessage Response, CancellationToken token) {
        using Stream Content = await Response.Content.ReadAsStreamAsync();
        using MemoryStream Destination = new();

        await WriteStreamAsync(Content, Destination, token);
        await Destination.FlushAsync();

        byte[] Bytes;
        switch (Response.Content.Headers.ContentEncoding.FirstOrDefault()) {
            case "br": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Brotli(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            case "gzip": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.GZip(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            }
            break;
            case "deflate": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Deflate(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            default: {
                Bytes = Destination.ToArray();
                Destination.Close();
            } break;
        }

        return (Response.Content.Headers.ContentType.CharSet ?? "utf-8").ToLowerInvariant() switch {
            "ascii" => Encoding.ASCII.GetString(Bytes),
            "utf-7" => Encoding.UTF7.GetString(Bytes),
            "utf-32" => Encoding.UTF32.GetString(Bytes),
            "utf-16" or "unicode" => Encoding.Unicode.GetString(Bytes),
            "utf-16-be" or "utf-16be" or "unicode-be" or "unicodebe" => Encoding.BigEndianUnicode.GetString(Bytes),
            _ => Encoding.UTF8.GetString(Bytes),
        };
    }
}
