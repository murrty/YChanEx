namespace YChanEx;

/// <summary>
/// Contains download configuration.
/// </summary>
internal static class Downloads {
    private const string ConfigName = "Downloads";

    static Downloads() {
        fSaveThumbnails =
            IniProvider.Read(SaveThumbnails, false, ConfigName);

        fSaveHTML =
            IniProvider.Read(SaveHTML, false, ConfigName);

        fSaveOriginalFilenames =
            IniProvider.Read(SaveOriginalFilenames, false, ConfigName);

        fPreventDuplicates =
            IniProvider.Read(PreventDuplicates, false, ConfigName);

        fDownloadPath =
            IniProvider.Read(DownloadPath,
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + System.IO.Path.DirectorySeparatorChar + "Downloads", ConfigName);

        fScannerDelay =
            IniProvider.Read(ScannerDelay, 60, ConfigName);

        fAllowFileNamesGreaterThan255 =
            IniProvider.Read(AllowFileNamesGreaterThan255, false, ConfigName);

        ffchanWarning =
            IniProvider.Read(fchanWarning, false, ConfigName);

        fCleanThreadHTML =
            IniProvider.Read(CleanThreadHTML, false, ConfigName);

        fAutoRemoveDeadThreads =
            IniProvider.Read(AutoRemoveDeadThreads, false, ConfigName);
    }

    /// <summary>
    /// Whether thumbnails (low-resolution previews) of the images should be downloaded.
    /// </summary>
    public static bool SaveThumbnails {
        get => fSaveThumbnails;
        set {
            if (fSaveThumbnails != value) {
                fSaveThumbnails = value;
                IniProvider.Write(SaveThumbnails, ConfigName);
            }
        }
    }
    private static bool fSaveThumbnails;

    /// <summary>
    /// Whether the HTML should be saved. Some chans support in-house HTML, while others require the chan html to be modified manually.
    /// </summary>
    public static bool SaveHTML {
        get => fSaveHTML;
        set {
            if (fSaveHTML != value) {
                fSaveHTML = value;
                IniProvider.Write(SaveHTML, ConfigName);
            }
        }
    }
    private static bool fSaveHTML;

    /// <summary>
    /// Whether posts should be saved with the original file name.
    /// </summary>
    public static bool SaveOriginalFilenames {
        get => fSaveOriginalFilenames;
        set {
            if (fSaveOriginalFilenames != value) {
                fSaveOriginalFilenames = value;
                IniProvider.Write(SaveOriginalFilenames, ConfigName);
            }
        }
    }
    private static bool fSaveOriginalFilenames;

    /// <summary>
    /// Whether duplicate prevention should be enabled.
    /// </summary>
    public static bool PreventDuplicates {
        get => fPreventDuplicates;
        set {
            if (fPreventDuplicates != value) {
                fPreventDuplicates = value;
                IniProvider.Write(PreventDuplicates, ConfigName);
            }
        }
    }
    private static bool fPreventDuplicates;

    /// <summary>
    /// Where the threads will be downloaded to.
    /// </summary>
    public static string DownloadPath {
        get => fDownloadPath;
        set {
            if (fDownloadPath != value) {
                fDownloadPath = value;
                IniProvider.Write(DownloadPath, ConfigName);
            }
        }
    }
    private static string fDownloadPath;

    /// <summary>
    /// The time in seconds when the scanner should rescan the thread.
    /// </summary>
    public static int ScannerDelay {
        get => fScannerDelay;
        set {
            if (fScannerDelay != value) {
                fScannerDelay = value;
                IniProvider.Write(ScannerDelay, ConfigName);
            }
        }
    }
    private static int fScannerDelay;

    /// <summary>
    /// Whether file names are allowed to exceed 255 length.
    /// </summary>
    public static bool AllowFileNamesGreaterThan255 {
        get => fAllowFileNamesGreaterThan255;
        set {
            if (fAllowFileNamesGreaterThan255 != value) {
                fAllowFileNamesGreaterThan255 = value;
                IniProvider.Write(AllowFileNamesGreaterThan255, ConfigName);
            }
        }
    }
    private static bool fAllowFileNamesGreaterThan255;

    /// <summary>
    /// Whether the fchan warning has been acknowledged.
    /// </summary>
    public static bool fchanWarning {
        get => ffchanWarning;
        set {
            if (ffchanWarning != value) {
                ffchanWarning = value;
                IniProvider.Write(fchanWarning, ConfigName);
            }
        }
    }
    private static bool ffchanWarning;

    /// <summary>
    /// Whether the thread HTML source should be clean and human readable. Adds some size to the HTML as a cost.
    /// </summary>
    public static bool CleanThreadHTML {
        get => fCleanThreadHTML;
        set {
            if (fCleanThreadHTML != value) {
                fCleanThreadHTML = value;
                IniProvider.Write(CleanThreadHTML, ConfigName);
            }
        }
    }
    private static bool fCleanThreadHTML;

    /// <summary>
    /// Whether threads that are 404d (or archived, after scanned and downloaded) will be automatically removed from the queue.
    /// </summary>
    public static bool AutoRemoveDeadThreads {
        get => fAutoRemoveDeadThreads;
        set {
            if (fAutoRemoveDeadThreads != value) {
                fAutoRemoveDeadThreads = value;
                IniProvider.Write(AutoRemoveDeadThreads, ConfigName);
            }
        }
    }
    private static bool fAutoRemoveDeadThreads;

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    public static void Reset() {
        SaveThumbnails = false;
        SaveHTML = false;
        SaveOriginalFilenames = false;
        PreventDuplicates = false;
        DownloadPath = string.Empty;
        ScannerDelay = 60;
        AllowFileNamesGreaterThan255 = false;
        fchanWarning = false;
        CleanThreadHTML = false;
        AutoRemoveDeadThreads = false;
    }
}