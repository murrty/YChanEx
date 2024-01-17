namespace SocksSharp;
using SocksSharp.Proxy;
internal class Socks4ClientHandler : ProxyClientHandler<Socks4> {
    public Socks4ClientHandler(ProxySettings proxySettings) : base(proxySettings) { }
}
