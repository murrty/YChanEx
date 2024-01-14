#nullable enable
namespace YChanEx;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using murrty.controls;
internal sealed class VolatileHttpClient : HttpClient {
    private const int DefaultBuffer = 81920;
    /// <summary>
    /// Represents the lowest timeout possible, 5 seconds (in milliseconds).
    /// </summary>
    internal const int LowestTimeout = 5_000,
    /// <summary>
    /// Represents the default timeout, 1 minute (in milliseconds).
    /// </summary>
        DefaultTimeout = 60_000,
    /// <summary>
    /// Represents the highest timeout possible, 30 minutes (in milliseconds).
    /// </summary>
        HighestTimeout = 1_800_000;

    internal delegate Task DownloadStream(Stream HttpStream, Stream Output, CancellationToken token);
    private readonly int ThrottleSize;
    private readonly int ThrottleBufferSize;

    private readonly ProxyData _proxy;
    private readonly bool _useProxy;
    private readonly bool _throttle;
    private readonly int _throttleSize;
    private readonly int _timeout;
    public HttpMessageHandler Handler;
    public ulong Iteration;

    public VolatileHttpClient(HttpMessageHandler NewHandler) : base(NewHandler) {
        this.Handler = NewHandler;
        this._proxy = Initialization.Proxy;
        this._useProxy = Initialization.UseProxy;
        this._timeout = Initialization.Timeout;

        if (Initialization.UseThrottling) {
            this._throttle = false;
            this._throttleSize = 0;
            this.WriteStreamAsync = WriteToStreamAsync;
        }
        else {
            this._throttle = true;
            this._throttleSize = Initialization.ThrottleSize;
            this.ThrottleBufferSize = Math.Min(_throttleSize, DefaultBuffer);
            this.WriteStreamAsync = ThrottledWriteToStreamAsync;
        }

        this.DefaultRequestHeaders.Accept.Add(new("*/*"));
        //this.DefaultRequestHeaders.AcceptEncoding.Add(new("br"));
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

        if (this._timeout != Initialization.Timeout) {
            return true;
        }

        return false;
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
        byte[] buffer = new byte[ThrottleBufferSize];
        int bytesRead;

        using ThrottledStream ThrottledWriter = new(Output, ThrottleSize);
        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) != 0) {
            await ThrottledWriter.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }
}
