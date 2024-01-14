#nullable enable
namespace YChanEx;
/// <summary>
/// Contains advanced configuration.
/// </summary>
public static class Advanced {
    private const string ConfigName = "Advanced";

    static Advanced() {
        fDisabledScanWhenOpeningSettings =
            IniProvider.Read(DisableScanWhenOpeningSettings, true, ConfigName);

        fSilenceErrors =
            IniProvider.Read(SilenceErrors, false, ConfigName);
    }

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
        DisableScanWhenOpeningSettings = true;
        SilenceErrors = false;
    }
}