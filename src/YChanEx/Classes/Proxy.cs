#nullable enable
namespace murrty.controls;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using YChanEx;
public partial struct Proxy : IEquatable<Proxy>, IComparable<Proxy> {
    public static readonly Proxy Empty = new();

    [System.Diagnostics.CodeAnalysis.StringSyntax("regex")] const string ProxyProtocol = @"^(http|socks4(a)|socks5):\/\/";
    [System.Diagnostics.CodeAnalysis.StringSyntax("regex")] const string IPv4 = @"((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}";
    [System.Diagnostics.CodeAnalysis.StringSyntax("regex")] const string PortVal = @":(([1-9]\d{0,3}|[1-5]\d{4}|6553[0-5]|655[0-2]\d|65[0-4]\d{2}|6[0-4]\d{3}))$";

#if NET7_0_OR_GREATER
    [GeneratedRegex(ProxyProtocol + IPv4 + PortVal, RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex Ipv4StringGenerated();
    private static readonly Regex Ipv4String = Ipv4StringGenerated();
#else
    private static readonly Regex Ipv4String = new(ProxyProtocol + IPv4 + PortVal, RegexOptions.Compiled | RegexOptions.IgnoreCase);
#endif

    private readonly byte[] Bytes = [0x0, 0x0, 0x0, 0x0];
    public readonly byte Section1 { get => Bytes[0]; set => Bytes[0] = value; }
    public readonly byte Section2 { get => Bytes[1]; set => Bytes[1] = value; }
    public readonly byte Section3 { get => Bytes[2]; set => Bytes[2] = value; }
    public readonly byte Section4 { get => Bytes[3]; set => Bytes[3] = value; }

    public string? Domain { readonly get; set; }

    public readonly string IP => $"{Bytes[0]}.{Bytes[1]}.{Section3}.{Bytes[3]}";

    public ushort Port { readonly get; set; }

    public ProxyType ProxyType { get; set; } = ProxyType.HTTP;

    public Proxy() {
        Bytes[0] = 0;
        Bytes[1] = 0;
        Bytes[2] = 0;
        Bytes[3] = 0;
        Port = 0;
        ProxyType = ProxyType.HTTP;
    }
    public Proxy(byte Section1, byte Section2, byte Section3, byte Section4, ushort Port, ProxyType type) {
        if (Section1 == 0) {
            throw new ArgumentException("Section1 cannot be 0.");
        }

        if (Section1 == 255 && Section2 == 255 && Section3 == 255 && Section4 == 255) {
            throw new ArgumentException("The sections cannot all be 255.");
        }

        if (Port == 0) {
            throw new ArgumentException("The port cannot be 0.");
        }

        Bytes[0] = Section1;
        Bytes[1] = Section2;
        Bytes[2] = Section3;
        Bytes[3] = Section4;
        this.Port = Port;
        ProxyType = type;
    }
    public Proxy(Proxy old, ProxyType proxy) {
        Bytes[0] = old.Bytes[0];
        Bytes[1] = old.Bytes[1];
        Bytes[2] = old.Bytes[2];
        Bytes[3] = old.Bytes[3];
        Port = old.Port;
        ProxyType = proxy;
    }
    public Proxy(string Domain, ushort Port, ProxyType proxy) {
        this.Domain = Domain;
        this.Port = Port;
        this.ProxyType = proxy;
    }

    public static Proxy Parse(string s) {
        if (!TryParse(s, out var pr))
            throw new ArgumentException($"'{s}' is not a valid IPv4 proxy.");
        return pr;
    }
    public static bool TryParse(string s, out Proxy ipv4) {
        s = s.ToLowerInvariant().Trim('/');

        // Domain catch.
        if (!Ipv4String.IsMatch(s)) {
            if (s.IndexOf('.') < 1) {
                ipv4 = default;
                return false;
            }

            if (s.StartsWith("socks4://")) {
                ipv4 = new(s, ushort.Parse(s.SubstringAfterLastChar(':')), ProxyType.SOCKS4);
                return true;
            }
            else if (s.StartsWith("socks4a://")) {
                ipv4 = new(s, ushort.Parse(s.SubstringAfterLastChar(':')), ProxyType.SOCKS4A);
                return true;
            }
            else if (s.StartsWith("socks5://")) {
                ipv4 = new(s, ushort.Parse(s.SubstringAfterLastChar(':')), ProxyType.SOCKS5);
                return true;
            }

            ipv4 = default;
            return false;
        }

        string[] ipSplits;
        string portSplit;

        void GetSplits() {
            ipSplits = s.Split('/')[^1].Split('.');
            portSplit = ipSplits[^1].Split(':')[1];
            ipSplits[^1] = ipSplits[^1].Split(':')[0];
        }

        ProxyType proxy;
        if (s.StartsWith("http://")) {
            GetSplits();
            proxy = ProxyType.HTTP;
        }
        else if (s.StartsWith("socks4://")) {
            GetSplits();
            proxy = ProxyType.SOCKS4;
        }
        else if (s.StartsWith("socks4a://")) {
            GetSplits();
            proxy = ProxyType.SOCKS4A;
        }
        else if (s.StartsWith("socks5://")) {
            GetSplits();
            proxy = ProxyType.SOCKS5;
        }
        else {
            ipv4 = default;
            return false;
        }

        // Parsing heaven <3
        if (!byte.TryParse(ipSplits[0], out byte sec1)
        || !byte.TryParse(ipSplits[1], out byte sec2)
        || !byte.TryParse(ipSplits[2], out byte sec3)
        || !byte.TryParse(ipSplits[3], out byte sec4)
        || !ushort.TryParse(portSplit, out ushort port)) {
            ipv4 = default;
            return false;
        }

        if (sec1 == 0) {
            ipv4 = default;
            return false;
        }

        if (sec1 == 255 && sec2 == 255 && sec3 == 255 && sec4 == 255) {
            ipv4 = default;
            return false;
        }

        ipv4 = new(sec1, sec2, sec3, sec4, port, proxy);

        return true;
    }

    public readonly string GetIP(bool IncludePortIfValued) {
        StringBuilder buffer = new();
        if (Domain is not null) {
            buffer.Append(Domain);
        }
        else {
            buffer.Append(Bytes[0]).Append('.').Append(Bytes[1]).Append('.').Append(Bytes[2]).Append('.').Append(Bytes[3]);
        }
        if (IncludePortIfValued && Port > 0) {
            buffer.Append(':').Append(Port);
        }
        return buffer.ToString();
    }
    public readonly string GetReadableIP() {
        return ProxyType switch {
            ProxyType.HTTP => "http://",
            ProxyType.SOCKS4 => "socks4://",
            ProxyType.SOCKS4A => "socks4a://",
            ProxyType.SOCKS5 => "socks5://",
            _ => throw new Exception("Not a valid proxy.")
        } + GetIP(true);
    }
    public readonly IPAddress GetIPAddress() => new(Bytes);

    public readonly bool HasValue => ((Bytes[0] > 0 && (Bytes[1] > 0 || Bytes[2] > 0 || Bytes[3] > 0)) || Domain is not null) && Port > 0;

    public override readonly bool Equals(object? obj) => obj is Proxy pv && Equals(pv);
    public override readonly int GetHashCode() => Bytes[0] >> 24 | Bytes[1] >> 16 | Bytes[2] >> 8 | Bytes[3];
    public override readonly string ToString() {
        StringBuilder sb = new();

        sb.Append(ProxyType switch {
            ProxyType.HTTP => "http",
            ProxyType.SOCKS4 => "socks4",
            ProxyType.SOCKS4A => "socks4a",
            ProxyType.SOCKS5 => "socks5",
            _ => throw new InvalidCastException($"Proxy type '{ProxyType}' is invalid.")
        }).Append("://");

        if (Domain is not null) {
            sb.Append(Domain);
        }
        else {
            sb.Append(Bytes[0]).Append('.')
                .Append(Bytes[1]).Append('.')
                .Append(Bytes[2]).Append('.')
                .Append(Bytes[3]);
        }

        return sb.Append(':')
            .Append(Port)
            .Append('/')
            .ToString();
    }

    public readonly bool Equals(Proxy other) => Equals(this, other);
    public static bool Equals(Proxy a, Proxy b) {
        if (a.Domain is null) {
            if (b.Domain is not null) {
                return false;
            }

            if (a.Bytes[0] != b.Bytes[0]) {
                return false;
            }

            if (a.Bytes[1] != b.Bytes[1]) {
                return false;
            }

            if (a.Bytes[2] != b.Bytes[2]) {
                return false;
            }

            if (a.Bytes[3] != b.Bytes[3]) {
                return false;
            }
        }
        else {
            if (b.Domain is null) {
                return false;
            }
            return a.Domain.Equals(b.Domain);
        }

        if (a.Port != b.Port) {
            return false;
        }

        if (a.ProxyType != b.ProxyType) {
            return false;
        }

        return true;
    }
    public readonly int CompareTo(Proxy other) {
        if (other.Bytes[0] > Bytes[0]) {
            return 1;
        }
        if (other.Bytes[0] < Bytes[0]) {
            return -1;
        }

        if (other.Bytes[1] > Bytes[1]) {
            return 2;
        }
        if (other.Bytes[1] < Bytes[1]) {
            return -2;
        }

        if (other.Bytes[2] > Bytes[2]) {
            return 3;
        }
        if (other.Bytes[2] < Bytes[2]) {
            return -3;
        }

        if (other.Bytes[3] > Bytes[3]) {
            return 4;
        }
        if (other.Bytes[3] < Bytes[3]) {
            return -4;
        }

        if (other.Port > Port) {
            return 5;
        }
        if (other.Port < Port) {
            return -5;
        }

        return 0;

        //return other.Bytes[0] > Bytes[0] ? 1 : other.Bytes[0] < Bytes[0] ? -1 :
        //    other.Bytes[1] > Bytes[1] ? 2 : other.Bytes[1] < Bytes[1] ? -2 :
        //    other.Bytes[2] > Bytes[2] ? 3 : other.Bytes[2] < Bytes[2] ? -3 :
        //    other.Bytes[3] > Bytes[3] ? 4 : other.Bytes[3] < Bytes[3] ? -4 :
        //    other.Port > Port ? 5 : other.Port < Port ? -5 :
        //    0;
    }

    public static bool operator ==(Proxy a, Proxy b) => Equals(a, b);
    public static bool operator !=(Proxy a, Proxy b) => !Equals(a, b);
}
