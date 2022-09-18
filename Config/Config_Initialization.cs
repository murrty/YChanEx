namespace YChanEx;

/// <summary>
/// Contains initialization program configuration.
/// </summary>
public sealed class Config_Initialization {

    private const string ConfigName = "Initialization";

    #region Fields
    /// <summary>
    /// Whether the program has ran for the first time.
    /// </summary>
    public bool FirstTime { get; set; }
    /// <summary>
    /// The decimal version of the skipped update version.
    /// </summary>
    public Version SkippedVersion { get; set; }

    private bool fFirstTime;
    private Version fSkippedVersion;
    #endregion

    public void Load() {
        fFirstTime = FirstTime = IniProvider.Read(FirstTime, true, ConfigName);
        fSkippedVersion = SkippedVersion = IniProvider.Read(SkippedVersion, Version.Empty, ConfigName);
    }

    public void Save() {
        if (FirstTime != fFirstTime)
            IniProvider.Write(FirstTime, ConfigName);
        if (SkippedVersion != fSkippedVersion)
            IniProvider.Write(SkippedVersion, ConfigName);
    }

    public void Reset() {
        SkippedVersion = Version.Empty;
    }

}