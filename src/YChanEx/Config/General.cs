namespace YChanEx;

/// <summary>
/// Contains general program configuration.
/// </summary>
public static class General {
    private const string ConfigName = "General";

    static General() {
        fShowTrayIcon = IniProvider.Read(ShowTrayIcon, false, ConfigName);
        fMinimizeToTray = IniProvider.Read(MinimizeToTray, true, ConfigName);
        fShowExitWarning = IniProvider.Read(ShowExitWarning, false, ConfigName);
        fEnableUpdates = IniProvider.Read(EnableUpdates, true, ConfigName);
        fUseFullBoardNameForTitle = IniProvider.Read(UseFullBoardNameForTitle, true, ConfigName);
        fSaveQueueOnExit = IniProvider.Read(SaveQueueOnExit, true, ConfigName);
        fMinimizeInsteadOfExiting = IniProvider.Read(MinimizeInsteadOfExiting, false, ConfigName);
        fAutoSaveThreads = IniProvider.Read(AutoSaveThreads, true, ConfigName);
        fSaveThreadHistory = IniProvider.Read(SaveThreadHistory, false, ConfigName);
    }

    /// <summary>
    /// Whether the tray icon should be visible (when not minized to tray).
    /// </summary>
    public static bool ShowTrayIcon {
        get => fShowTrayIcon;
        set {
            if (fShowTrayIcon != value) {
                fShowTrayIcon = value;
                IniProvider.Write(ShowTrayIcon, ConfigName);
            }
        }
    }
    private static bool fShowTrayIcon;

    /// <summary>
    /// Whether the program should minimize to the system tray.
    /// </summary>
    public static bool MinimizeToTray {
        get => fMinimizeToTray;
        set {
            if (fMinimizeToTray != value) {
                fMinimizeToTray = value;
                IniProvider.Write(MinimizeToTray, ConfigName);
            }
        }
    }
    private static bool fMinimizeToTray;

    /// <summary>
    /// Whether the exit warning should be displayed when attempting to exit.
    /// </summary>
    public static bool ShowExitWarning {
        get => fShowExitWarning;
        set {
            if (fShowExitWarning != value) {
                fShowExitWarning = value;
                IniProvider.Write(ShowExitWarning, ConfigName);
            }
        }
    }
    private static bool fShowExitWarning;

    /// <summary>
    /// Whether checking for updates is enabled.
    /// </summary>
    public static bool EnableUpdates {
        get => fEnableUpdates;
        set {
            if (fEnableUpdates != value) {
                fEnableUpdates = value;
                IniProvider.Write(EnableUpdates, ConfigName);
            }
        }
    }
    private static bool fEnableUpdates;

    /// <summary>
    /// Whether to use the full board name for the thread title.
    /// </summary>
    public static bool UseFullBoardNameForTitle {
        get => fUseFullBoardNameForTitle;
        set {
            if (fUseFullBoardNameForTitle != value) {
                fUseFullBoardNameForTitle = value;
                IniProvider.Write(UseFullBoardNameForTitle, ConfigName);
            }
        }
    }
    private static bool fUseFullBoardNameForTitle;

    /// <summary>
    /// Whether the thread queue should save on exit.
    /// </summary>
    public static bool SaveQueueOnExit {
        get => fSaveQueueOnExit;
        set {
            if (fSaveQueueOnExit != value) {
                fSaveQueueOnExit = value;
                IniProvider.Write(SaveQueueOnExit, ConfigName);
            }
        }
    }
    private static bool fSaveQueueOnExit;

    /// <summary>
    /// Whether the program should minimize to tray instead of exiting.
    /// </summary>
    public static bool MinimizeInsteadOfExiting {
        get => fMinimizeInsteadOfExiting;
        set {
            if (fMinimizeInsteadOfExiting != value) {
                fMinimizeInsteadOfExiting = value;
                IniProvider.Write(MinimizeInsteadOfExiting, ConfigName);
            }
        }
    }
    private static bool fMinimizeInsteadOfExiting;

    /// <summary>
    /// Whether the threads should automatically save when adding/removing/changing threads.
    /// </summary>
    public static bool AutoSaveThreads {
        get => fAutoSaveThreads;
        set {
            if (fAutoSaveThreads != value) {
                fAutoSaveThreads = value;
                IniProvider.Write(AutoSaveThreads, ConfigName);
            }
        }
    }
    private static bool fAutoSaveThreads;

    /// <summary>
    /// Gets or sets whether to save downloaded thread history.
    /// </summary>
    public static bool SaveThreadHistory {
        get => fSaveThreadHistory;
        set {
            if (fSaveThreadHistory != value) {
                fSaveThreadHistory = value;
                IniProvider.Write(SaveThreadHistory, ConfigName);
            }
        }
    }
    private static bool fSaveThreadHistory;

    public static void Reset() {
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
