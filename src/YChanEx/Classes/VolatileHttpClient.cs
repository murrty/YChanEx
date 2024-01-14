#nullable enable
namespace YChanEx;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using murrty.controls;
internal sealed class VolatileHttpClient : HttpClient {
    public HttpMessageHandler Handler;
    internal const int _timeout = 60_000;
    private readonly ProxyData _proxy;
    private readonly bool _useProxy;
    public ulong Iteration;

    //private delegate Task DownloadStream(Stream HttpStream, Stream Output, CancellationToken token);
    private const int DefaultBuffer = 81920;
    private static readonly int ThrottleSize;
    private static readonly int ThrottleBufferSize;

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
        return false;
    }

    internal static async Task WriteToStreamAsync(Stream HttpStream, Stream Output, CancellationToken token) {
        byte[] buffer = new byte[DefaultBuffer];
        int bytesRead;

        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0) {
            await Output.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }
    internal static async Task ThrottledWriteToStreamAsync(Stream HttpStream, Stream Output, CancellationToken token) {
        byte[] buffer = new byte[ThrottleBufferSize];
        int bytesRead;

        using ThrottledStream ThrottledWriter = new(Output, ThrottleSize);
        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) != 0) {
            await ThrottledWriter.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }
}
