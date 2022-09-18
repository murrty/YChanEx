namespace YChanEx;

/// <summary>
/// Contains advanced configuration.
/// </summary>
public sealed class Config_Advanced {

    private const string ConfigName = "Advanced";
    internal const string DefaultUserAgent = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/101.0";

    #region Fields
    /// <summary>
    /// The user-agent used in webclients.
    /// </summary>
    public string UserAgent { get; internal set; }
    /// <summary>
    /// Whether the scanner should pause while in the settings.
    /// </summary>
    public bool DisableScanWhenOpeningSettings { get; internal set; } 
    /// <summary>
    /// Whether errors should not be displayed.
    /// </summary>
    public bool SilenceErrors { get; internal set; }

    private string fUserAgent = DefaultUserAgent;
    private bool fDisabledScanWhenOpeningSettings = true;
    private bool fSilenceErrors = false;
    #endregion

    public void Load() {
        fUserAgent = UserAgent = IniProvider.Read(UserAgent, DefaultUserAgent, ConfigName);
        fDisabledScanWhenOpeningSettings = DisableScanWhenOpeningSettings = IniProvider.Read(DisableScanWhenOpeningSettings, true, ConfigName);
        fSilenceErrors = SilenceErrors = IniProvider.Read(SilenceErrors, false, ConfigName);
    }

    public void Save() {
        if (UserAgent != fUserAgent)
            IniProvider.Write(UserAgent, ConfigName);
        if (DisableScanWhenOpeningSettings != fDisabledScanWhenOpeningSettings)
            IniProvider.Write(DisableScanWhenOpeningSettings, ConfigName);
        if (SilenceErrors != fSilenceErrors)
            IniProvider.Write(SilenceErrors, ConfigName);
    }

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    public void Reset() {
        UserAgent = DefaultUserAgent;
        DisableScanWhenOpeningSettings = true;
        SilenceErrors = false;
    }

}