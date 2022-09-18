namespace YChanEx;

/// <summary>
/// Contains general program configuration.
/// </summary>
public sealed class Config_General {

    private const string ConfigName = "General";

    #region Fields
    /// <summary>
    /// Whether the tray icon should be visible (when not minized to tray).
    /// </summary>
    public bool ShowTrayIcon { get; set; }
    /// <summary>
    /// Whether the program should minimize to the system tray.
    /// </summary>
    public bool MinimizeToTray { get; set; }
    /// <summary>
    /// Whether the exit warning should be displayed when attempting to exit.
    /// </summary>
    public bool ShowExitWarning { get; set; }
    /// <summary>
    /// Whether checking for updates is enabled.
    /// </summary>
    public bool EnableUpdates { get; set; }
    /// <summary>
    /// Whether to use the full board name for the thread title.
    /// </summary>
    public bool UseFullBoardNameForTitle { get; set; }
    /// <summary>
    /// Whether the thread queue should save on exit.
    /// </summary>
    public bool SaveQueueOnExit { get; set; }
    /// <summary>
    /// Whether the program should minimize to tray instead of exiting.
    /// </summary>
    public bool MinimizeInsteadOfExiting { get; set; }
    /// <summary>
    /// Whether the threads should automatically save when adding/removing/changing threads.
    /// </summary>
    public bool AutoSaveThreads { get; set; }
    /// <summary>
    /// Gets or sets whether to save downloaded thread history.
    /// </summary>
    public bool SaveThreadHistory { get; set; }

    private bool fShowTrayIcon;
    private bool fMinimizeToTray;
    private bool fShowExitWarning;
    private bool fEnableUpdates;
    private bool fUseFullBoardNameForTitle;
    private bool fSaveQueueOnExit;
    private bool fMinimizeInsteadOfExiting;
    private bool fAutoSaveThreads;
    private bool fSaveThreadHistory;
    #endregion

    public void Load() {
        fShowTrayIcon = ShowTrayIcon = IniProvider.Read(ShowTrayIcon, false, ConfigName);
        fMinimizeToTray = MinimizeToTray = IniProvider.Read(MinimizeToTray, true, ConfigName);
        fShowExitWarning = ShowExitWarning = IniProvider.Read(ShowExitWarning, false, ConfigName);
        fEnableUpdates = EnableUpdates = IniProvider.Read(EnableUpdates, true, ConfigName);
        fUseFullBoardNameForTitle = UseFullBoardNameForTitle = IniProvider.Read(UseFullBoardNameForTitle, true, ConfigName);
        fSaveQueueOnExit = SaveQueueOnExit = IniProvider.Read(SaveQueueOnExit, true, ConfigName);
        fMinimizeInsteadOfExiting = MinimizeInsteadOfExiting = IniProvider.Read(MinimizeInsteadOfExiting, false, ConfigName);
        fAutoSaveThreads = AutoSaveThreads = IniProvider.Read(AutoSaveThreads, true, ConfigName);
        fSaveThreadHistory = SaveThreadHistory = IniProvider.Read(SaveThreadHistory, false, ConfigName);
    }

    public void Save() {
        if (ShowTrayIcon != fShowTrayIcon)
            IniProvider.Write(ShowTrayIcon, ConfigName);
        if (MinimizeToTray != fMinimizeToTray)
            IniProvider.Write(MinimizeToTray, ConfigName);
        if (ShowExitWarning != fShowExitWarning)
            IniProvider.Write(ShowExitWarning, ConfigName);
        if (EnableUpdates != fEnableUpdates)
            IniProvider.Write(EnableUpdates, ConfigName);
        if (UseFullBoardNameForTitle != fUseFullBoardNameForTitle)
            IniProvider.Write(UseFullBoardNameForTitle, ConfigName);
        if (SaveQueueOnExit != fSaveQueueOnExit)
            IniProvider.Write(SaveQueueOnExit, ConfigName);
        if (MinimizeInsteadOfExiting != fMinimizeInsteadOfExiting)
            IniProvider.Write(MinimizeInsteadOfExiting, ConfigName);
        if (AutoSaveThreads != fAutoSaveThreads)
            IniProvider.Write(AutoSaveThreads, ConfigName);
        if (SaveThreadHistory != fSaveThreadHistory)
            IniProvider.Write(SaveThreadHistory, ConfigName);
    }

    public void Reset() {
        ShowTrayIcon = false;
        MinimizeToTray = true;
        ShowExitWarning = false;
        EnableUpdates = true;
        UseFullBoardNameForTitle = false;
        SaveQueueOnExit = true;
        MinimizeInsteadOfExiting = false;
        AutoSaveThreads = true;
        SaveThreadHistory = false;
    }

}