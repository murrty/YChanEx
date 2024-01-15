namespace SocksSharp;
using SocksSharp.Proxy;
internal class Socks5ProxyClientHandler : ProxyClientHandler<Socks5> {
    public Socks5ProxyClientHandler(ProxySettings proxySettings) : base(proxySettings) { }
}
