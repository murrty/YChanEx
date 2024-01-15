namespace SocksSharp;
using SocksSharp.Proxy;
internal class Socks4ProxyClientHandler : ProxyClientHandler<Socks4> {
    public Socks4ProxyClientHandler(ProxySettings proxySettings) : base(proxySettings) { }
}
