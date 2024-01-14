#nullable enable
namespace YChanEx;
public enum ProxyType : byte {
    None = 0x0,
    HTTP = 0x1,
    SOCKS4 = 0x2,
    SOCKS4A = 0x3,
    SOCKS5 = 0x4,
}
