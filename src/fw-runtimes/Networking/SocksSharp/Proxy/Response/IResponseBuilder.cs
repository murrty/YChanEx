namespace SocksSharp.Proxy.Response;
using System.IO;
using System.Net.Http;
using System.Threading;
public interface IResponseBuilder {
    int ReceiveTimeout { get; set; }

    Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, Stream stream);

    Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request, Stream stream, CancellationToken cancellationToken);
}