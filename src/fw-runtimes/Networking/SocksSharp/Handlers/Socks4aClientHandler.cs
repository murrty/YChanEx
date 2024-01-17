namespace SocksSharp;
using SocksSharp.Proxy;
internal class Socks4aClientHandler : ProxyClientHandler<Socks4a> {
    public Socks4aClientHandler(ProxySettings proxySettings) : base(proxySettings) { }
}
