#nullable enable
namespace YChanEx;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using murrty.controls;
internal sealed class VolatileHttpClient : HttpClient {
    private const int DefaultBuffer = 81920;
    internal const int _timeout = 60_000;

    internal delegate Task DownloadStream(Stream HttpStream, Stream Output, CancellationToken token);
    private readonly int ThrottleSize;
    private readonly int ThrottleBufferSize;

    private readonly ProxyData _proxy;
    private readonly bool _useProxy;
    private readonly bool _throttle;
    private readonly int _throttleSize;
    public HttpMessageHandler Handler;
    public ulong Iteration;

    public VolatileHttpClient(HttpMessageHandler NewHandler) : base(NewHandler) {
        this.Handler = NewHandler;
        this._proxy = Initialization.Proxy;
        this._useProxy = Initialization.UseProxy;

        this.DefaultRequestHeaders.Accept.Add(new("*/*"));
        //this.DefaultRequestHeaders.AcceptEncoding.Add(new("br"));
        this.DefaultRequestHeaders.AcceptEncoding.Add(new("gzip"));
        this.DefaultRequestHeaders.AcceptEncoding.Add(new("deflate"));
        this.DefaultRequestHeaders.AcceptLanguage.Add(new("*"));
        this.DefaultRequestHeaders.ConnectionClose = false;
        this.DefaultRequestHeaders.UserAgent.ParseAdd(Program.UserAgent);
        this.Timeout = new(0, 0, 0, 0, _timeout);

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
