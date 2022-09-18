namespace YChanEx;

/// <summary>
/// Contains download configuration.
/// </summary>
public sealed class Config_Downloads {

    private const string ConfigName = "Downloads";

    #region Fields
    /// <summary>
    /// Whether thumbnails (low-resolution previews) of the images should be downloaded.
    /// </summary>
    public bool SaveThumbnails { get; set; }
    /// <summary>
    /// Whether the HTML should be saved. Some chans support in-house HTML, while others require the chan html to be modified manually.
    /// </summary>
    public bool SaveHTML { get; set; }
    /// <summary>
    /// Whether posts should be saved with the original file name.
    /// </summary>
    public bool SaveOriginalFilenames { get; set; }
    /// <summary>
    /// Whether duplicate prevention should be enabled.
    /// </summary>
    public bool PreventDuplicates { get; set; }
    /// <summary>
    /// Where the threads will be downloaded to.
    /// </summary>
    public string DownloadPath { get; set; }
    /// <summary>
    /// The time in seconds when the scanner should rescan the thread.
    /// </summary>
    public int ScannerDelay { get; set; }
    /// <summary>
    /// Whether file names are allowed to exceed 255 length.
    /// </summary>
    public bool AllowFileNamesGreaterThan255 { get; set; }
    /// <summary>
    /// Whether the fchan warning has been acknowledged.
    /// </summary>
    public bool fchanWarning { get; set; }
    /// <summary>
    /// Whether the thread HTML source should be clean and human readable. Adds some size to the HTML as a cost.
    /// </summary>
    public bool CleanThreadHTML { get; set; }
    /// <summary>
    /// Whether threads that are 404d (or archived, after scanned and downloaded) will be automatically removed from the queue.
    /// </summary>
    public bool AutoRemoveDeadThreads { get; set; }

    private bool fSaveThumbnails;
    private bool fSaveHTML;
    private bool fSaveOriginalFilenames;
    private bool fPreventDuplicates;
    private string fDownloadPath;
    private int fScannerDelay;
    private bool fAllowFileNamesGreaterThan255;
    private bool ffchanWarning;
    private bool fCleanThreadHTML;
    private bool fAutoRemoveDeadThreads;
    #endregion

    public void Load() {
        fSaveThumbnails = SaveThumbnails = IniProvider.Read(SaveThumbnails, false, ConfigName);
        fSaveHTML = SaveHTML = IniProvider.Read(SaveHTML, false, ConfigName);
        fSaveOriginalFilenames = SaveOriginalFilenames = IniProvider.Read(SaveOriginalFilenames, false, ConfigName);
        fPreventDuplicates = PreventDuplicates = IniProvider.Read(PreventDuplicates, false, ConfigName);
        fDownloadPath = DownloadPath = IniProvider.Read(DownloadPath, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads", ConfigName);
        fScannerDelay = ScannerDelay = IniProvider.Read(ScannerDelay, 60, ConfigName);
        fAllowFileNamesGreaterThan255 = AllowFileNamesGreaterThan255 = IniProvider.Read(AllowFileNamesGreaterThan255, false, ConfigName);
        ffchanWarning = fchanWarning = IniProvider.Read(fchanWarning, false, ConfigName);
        fCleanThreadHTML = CleanThreadHTML = IniProvider.Read(CleanThreadHTML, false, ConfigName);
        fAutoRemoveDeadThreads = AutoRemoveDeadThreads = IniProvider.Read(AutoRemoveDeadThreads, false, ConfigName);
    }

    public void Save() {
        if (SaveThumbnails != fSaveThumbnails)
            fSaveThumbnails = IniProvider.Write(SaveThumbnails, ConfigName);
        if (SaveHTML != fSaveHTML)
            fSaveHTML = IniProvider.Write(SaveHTML, ConfigName);
        if (SaveOriginalFilenames != fSaveOriginalFilenames)
            fSaveOriginalFilenames = IniProvider.Write(SaveOriginalFilenames, ConfigName);
        if (PreventDuplicates != fPreventDuplicates)
            fPreventDuplicates = IniProvider.Write(PreventDuplicates, ConfigName);
        if (DownloadPath != fDownloadPath)
            fDownloadPath = IniProvider.Write(DownloadPath, ConfigName);
        if (ScannerDelay != fScannerDelay)
            fScannerDelay = IniProvider.Write(ScannerDelay, ConfigName);
        if (AllowFileNamesGreaterThan255 != fAllowFileNamesGreaterThan255)
            fAllowFileNamesGreaterThan255 = IniProvider.Write(AllowFileNamesGreaterThan255, ConfigName);
        if (fchanWarning != ffchanWarning)
            ffchanWarning = IniProvider.Write(fchanWarning, ConfigName);
        if (CleanThreadHTML != fCleanThreadHTML)
            fCleanThreadHTML = IniProvider.Write(CleanThreadHTML, ConfigName);
        if (AutoRemoveDeadThreads != fAutoRemoveDeadThreads)
            fAutoRemoveDeadThreads = IniProvider.Write(AutoRemoveDeadThreads, ConfigName);
    }

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    public void Reset() {
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