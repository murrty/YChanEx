namespace SocksSharp;
using SocksSharp.Proxy;
internal class Socks5ClientHandler : ProxyClientHandler<Socks5> {
    public Socks5ClientHandler(ProxySettings proxySettings) : base(proxySettings) { }
}
