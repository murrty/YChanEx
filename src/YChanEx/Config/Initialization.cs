namespace YChanEx;

/// <summary>
/// Contains initialization program configuration.
/// </summary>
internal static class Initialization {
    static Initialization() {
        fFirstTime = IniProvider.Read(FirstTime, true);
        fSkippedVersion = IniProvider.Read(SkippedVersion, Version.Empty);
        fSkippedBetaVersion = IniProvider.Read(SkippedBetaVersion, Version.Empty);
        fUseProxy = IniProvider.Read(UseProxy, false);
        fProxy = IniProvider.Read(Proxy, ProxyData.Empty);
        fUseThrottling = IniProvider.Read(UseThrottling, false);
        fThrottleSize = IniProvider.Read(ThrottleSize, 1);
    }

    /// <summary>
    /// Whether the program has ran for the first time.
    /// </summary>
    public static bool FirstTime {
        get => fFirstTime;
        set {
            if (fFirstTime != value) {
                fFirstTime = value;
                IniProvider.Write(FirstTime);
            }
        }
    }
    private static bool fFirstTime;

    /// <summary>
    /// The decimal version of the skipped update version.
    /// </summary>
    public static Version SkippedVersion {
        get => fSkippedVersion;
        set {
            if (fSkippedVersion != value) {
                fSkippedVersion = value;
                IniProvider.Write(SkippedVersion);
            }
        }
    }
    private static Version fSkippedVersion;

    /// <summary>
    /// The decimal version of the skipped update version.
    /// </summary>
    public static Version SkippedBetaVersion {
        get => fSkippedBetaVersion;
        set {
            if (fSkippedBetaVersion != value) {
                fSkippedBetaVersion = value;
                IniProvider.Write(SkippedBetaVersion);
            }
        }
    }
    private static Version fSkippedBetaVersion;

    /// <summary>
    /// Whether to use a proxy for downloading.
    /// </summary>
    public static bool UseProxy {
        get => fUseProxy;
        set {
            if (fUseProxy != value) {
                fUseProxy = value;
                IniProvider.Write(UseProxy);
            }
        }
    }
    private static bool fUseProxy;

    /// <summary>
    /// The proxy data that will be used.
    /// </summary>
    public static ProxyData Proxy {
        get => fProxy;
        set {
            if (fProxy != value) {
                fProxy = value;
                IniProvider.Write(Proxy);
            }
        }
    }
    private static ProxyData fProxy;

    /// <summary>
    /// Whether to use throttling to throttle download speed.
    /// </summary>
    public static bool UseThrottling {
        get => fUseThrottling;
        set {
            if (fUseThrottling != value) {
                fUseThrottling = value;
                IniProvider.Write(UseThrottling);
            }
        }
    }
    private static bool fUseThrottling;

    /// <summary>
    /// The throttle size (in bytes per second).
    /// </summary>
    public static int ThrottleSize {
        get => fThrottleSize;
        set {
            if (fThrottleSize != value) {
                fThrottleSize = value;
                IniProvider.Write(ThrottleSize);
            }
        }
    }
    private static int fThrottleSize;

    public static void Reset() {
        SkippedVersion = Version.Empty;
        fSkippedBetaVersion = Version.Empty;
        fUseProxy = false;
        fProxy = ProxyData.Empty;
        fUseThrottling = false;
        fThrottleSize = 1;
    }
}