#nullable enable
namespace YChanEx;
/// <summary>
/// Contains initialization program configuration.
/// </summary>
internal static class Initialization {
    static Initialization() {
        fFirstTime = IniProvider.Read(FirstTime, true);
        fCheckForUpdates = IniProvider.Read(CheckForUpdates, true);
        fCheckForBetaUpdates = IniProvider.Read(CheckForBetaUpdates, false);
        fSkippedVersion = IniProvider.Read(SkippedVersion, Version.Empty);
        fSkippedBetaVersion = IniProvider.Read(SkippedBetaVersion, Version.Empty);
        fUseProxy = IniProvider.Read(UseProxy, false);
        fProxy = IniProvider.Read(Proxy, Proxy.Empty);
        fUseThrottling = IniProvider.Read(UseThrottling, false);
        fThrottleSize = IniProvider.Read(ThrottleSize, 1);
        fTimeout = IniProvider.Read(Timeout, VolatileHttpClient.DefaultTimeout);
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
    /// Whether the program should check for updates.
    /// <para/>
    /// Default: <see langword="false"/>
    /// </summary>
    public static bool CheckForUpdates {
        get => fCheckForUpdates;
        internal set {
            if (fCheckForUpdates != value) {
                fCheckForUpdates = value;
                IniProvider.Write(CheckForUpdates);
            }
        }
    }
    private static bool fCheckForUpdates;

    /// <summary>
    /// Whether to check for beta versions of the program.
    /// <para/>
    /// Default: <see langword="false"/>
    /// </summary>
    public static bool CheckForBetaUpdates {
        get => fCheckForBetaUpdates;
        internal set {
            if (fCheckForBetaUpdates != value) {
                fCheckForBetaUpdates = value;
                IniProvider.Write(CheckForBetaUpdates);
            }
        }
    }
    private static bool fCheckForBetaUpdates;

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
    public static Proxy Proxy {
        get => fProxy;
        set {
            if (fProxy != value) {
                fProxy = value;
                IniProvider.Write(Proxy);
            }
        }
    }
    private static Proxy fProxy;

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

    /// <summary>
    /// The timeout of each request per second.
    /// <para/>
    /// Default: <see langword="60,000"/>
    /// </summary>
    public static int Timeout {
        get => fTimeout;
        internal set {
            value = value.Clamp(VolatileHttpClient.LowestTimeout, VolatileHttpClient.HighestTimeout);
            if (fTimeout != value) {
                fTimeout = value;
                IniProvider.Write(Timeout);
            }
        }
    }
    private static int fTimeout;

    public static void Reset() {
        SkippedVersion = Version.Empty;
        fSkippedBetaVersion = Version.Empty;
        fUseProxy = false;
        fProxy = Proxy.Empty;
        fUseThrottling = false;
        fThrottleSize = 1;
    }
}