#nullable enable
namespace murrty.networking;
using System.Net.Http;
using System.Threading;
using YChanEx;
internal static class DelegateHandlerHelpers {
    public static CancellationTokenSource? CreateToken(HttpRequestMessage request, TimeSpan DefaultTimeout, CancellationToken cancellationToken) {
        TimeSpan Timeout;

#if NET5_0_OR_GREATER
        if (request.Options.TryGetValue(RequestMessage.TimeoutKey, out TimeSpan? time) && time.HasValue) {
            Timeout = time.Value;
        }
        else
#endif
        if (request.Properties?.TryGetValue(RequestMessage.TimeoutKey, out var tsp) is true && tsp is TimeSpan time) {
            Timeout = time;
        }
        else {
            Timeout = DefaultTimeout;
        }

        if (Timeout == TimeSpan.Zero) {
            return null;
        }

        CancellationTokenSource Token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Token.CancelAfter(Timeout);
        return Token;
    }
}
