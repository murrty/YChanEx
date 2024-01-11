namespace YChanEx;
using System.Text.RegularExpressions;
internal readonly struct ProxyData {
    public static readonly ProxyData Empty = new();
    private static readonly System.Text.RegularExpressions.Regex Ipv4String = new(
        @"^((http|socks4|socks4a|socks5):\/\/)?(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})(:\d{1,5})?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    readonly byte IP_S1;
    readonly byte IP_S2;
    readonly byte IP_S3;
    readonly byte IP_S4;

    public string IP => $"{IP_S1}:{IP_S2}:{IP_S3}:{IP_S4}";
    public string PortString => Port.ToString();
    public ushort Port { get; }
    public byte Type { get; }

    public ProxyData(byte Section1, byte Section2, byte Section3, byte Section4, ushort Port, byte Type) {
        this.IP_S1 = Section1;
        this.IP_S2 = Section2;
        this.IP_S3 = Section3;
        this.IP_S4 = Section4;
        this.Port = Port;
        this.Type = Type;
    }
    public static bool TryParse(string s, out ProxyData proxy) {
        s = s.ToLowerInvariant().Trim('/');
        if (!Ipv4String.IsMatch(s)) {
            proxy = default;
            return false;
        }

        string[] ipSplits;
        string portSplit;

        void GetSplits() {
            ipSplits = s.Split('/')[^1].Split('.');
            portSplit = ipSplits[^1].Split(':')[1];
            ipSplits[^1] = ipSplits[^1].Split(':')[0];
        }
        byte type;
        if (s.StartsWith("http://")) {
            GetSplits();
            type = 0;
        }
        else if (s.StartsWith("socks4://")) {
            GetSplits();
            type = 1;
        }
        // This the same as above, just SOCKS5 instead.
        else if (s.StartsWith("socks5://")) {
            GetSplits();
            type = 2;
        }
        else {
            proxy = default;
            return false;
        }

        // Parsing heaven <3
        if (!byte.TryParse(ipSplits[0], out byte sec1)
        || !byte.TryParse(ipSplits[1], out byte sec2)
        || !byte.TryParse(ipSplits[2], out byte sec3)
        || !byte.TryParse(ipSplits[3], out byte sec4)
        || !ushort.TryParse(portSplit, out ushort port)) {
            proxy = default;
            return false;
        }

        proxy = new(sec1, sec2, sec3, sec4, port, type);
        return true;
    }
    public readonly string GetReadableIp() {
        return $"{Type switch {
            1 => "socks4",
            2 => "socks4a",
            3 => "socks5",
            _ => "http"
        }}://{IP_S1}.{IP_S2}.{IP_S3}.{IP_S4}:{Port}";
    }

    public readonly bool HasValue => IP_S1 > 1 && Type < 4 && Port > 0;
    public override readonly bool Equals(object obj) => obj is ProxyData pv && Equals(pv);
    public override readonly int GetHashCode() => IP_S1 >> 24 | IP_S2 >> 16 | IP_S3 >> 8 | IP_S4;
    public static bool operator ==(ProxyData a, ProxyData b) => Equals(a, b);
    public static bool operator !=(ProxyData a, ProxyData b) => !Equals(a, b);
}