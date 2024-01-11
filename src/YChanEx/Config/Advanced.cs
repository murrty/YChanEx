namespace YChanEx;

/// <summary>
/// Contains advanced configuration.
/// </summary>
public static class Advanced {
    private const string ConfigName = "Advanced";
    internal const string DefaultUserAgent = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/105.0";

    static Advanced() {
        fUserAgent =
            IniProvider.Read(UserAgent, DefaultUserAgent, ConfigName);

        fDisabledScanWhenOpeningSettings =
            IniProvider.Read(DisableScanWhenOpeningSettings, true, ConfigName);

        fSilenceErrors =
            IniProvider.Read(SilenceErrors, false, ConfigName);
    }

    /// <summary>
    /// The user-agent used in for downloads.
    /// </summary>
    public static string UserAgent {
        get => fUserAgent;
        internal set {
            if (fUserAgent != value) {
                fUserAgent = value;
                IniProvider.Write(UserAgent, ConfigName);
            }
        }
    }
    private static string fUserAgent;

    /// <summary>
    /// Whether the scanner should pause while in the settings.
    /// </summary>
    public static bool DisableScanWhenOpeningSettings {
        get => fDisabledScanWhenOpeningSettings;
        internal set {
            if (fDisabledScanWhenOpeningSettings != value) {
                fDisabledScanWhenOpeningSettings = value;
                IniProvider.Write(DisableScanWhenOpeningSettings, ConfigName);
            }
        }
    }
    private static bool fDisabledScanWhenOpeningSettings;

    /// <summary>
    /// Whether errors should not be displayed.
    /// </summary>
    public static bool SilenceErrors {
        get => fSilenceErrors;
        internal set {
            if (fSilenceErrors != value) {
                fSilenceErrors = value;
                IniProvider.Write(SilenceErrors, ConfigName);
            }
        }
    }
    private static bool fSilenceErrors;

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    public static void Reset() {
        UserAgent = DefaultUserAgent;
        DisableScanWhenOpeningSettings = true;
        SilenceErrors = false;
    }
}