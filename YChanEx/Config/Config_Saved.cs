namespace YChanEx;

using System.Drawing;

/// <summary>
/// Contains saved info configuration.
/// </summary>
public sealed class Config_Saved {

    private const string ConfigName = "Saved";

    #region Fields
    /// <summary>
    /// The last Point of the main form.
    /// </summary>
    public Point MainFormLocation { get; set; }
    /// <summary>
    /// The last Size of the main form.
    /// </summary>
    public Size MainFormSize { get; set; }
    /// <summary>
    /// The last Point of the main form.
    /// </summary>
    public Point DownloadFormLocation { get; set; }
    /// <summary>
    /// The last Size of the download form.
    /// </summary>
    public Size DownloadFormSize { get; set; }
    /// <summary>
    /// Whether the thread should be created in the background, and not show a window when added.
    /// </summary>
    public bool CreateThreadInTheBackground { get; set; }
    /// <summary>
    /// The joined-string of the main forms' column sizes.
    /// </summary>
    public string MainFormColumnSizes { get; set; }
    public Point LogFormLocation { get; set; }
    public Size LogFormSize { get; set; }

    private Point fMainFormLocation;
    private Size fMainFormSize;
    private Point fDownloadFormLocation;
    private Size fDownloadFormSize;
    private bool fCreateThreadInTheBackground;
    private string fMainFormColumnSizes;
    private Point fLogFormLocation;
    private Size fLogFormSize;
    #endregion

    public void Load() {
        fMainFormLocation = MainFormLocation = IniProvider.Read(MainFormLocation, Config.InvalidPoint, ConfigName);
        fMainFormSize = MainFormSize = IniProvider.Read(MainFormSize, Size.Empty, ConfigName);
        fDownloadFormLocation = DownloadFormLocation = IniProvider.Read(DownloadFormLocation, Config.InvalidPoint, ConfigName);
        fDownloadFormSize = DownloadFormSize = IniProvider.Read(DownloadFormSize, Size.Empty, ConfigName);
        fCreateThreadInTheBackground = CreateThreadInTheBackground = IniProvider.Read(CreateThreadInTheBackground, false, ConfigName);
        fMainFormColumnSizes = MainFormColumnSizes = IniProvider.Read(MainFormColumnSizes, string.Empty, ConfigName);
        fLogFormLocation = LogFormLocation = IniProvider.Read(LogFormLocation, Config.InvalidPoint, ConfigName);
        fLogFormSize = LogFormSize = IniProvider.Read(LogFormSize, Size.Empty, ConfigName);
    }

    public void Save() {
        if (MainFormLocation != fMainFormLocation)
            fMainFormLocation = IniProvider.Write(MainFormLocation, ConfigName);
        if (MainFormSize != fMainFormSize)
            fMainFormSize = IniProvider.Write(MainFormSize, ConfigName);
        if (DownloadFormLocation != fDownloadFormLocation)
            fDownloadFormLocation = IniProvider.Write(DownloadFormLocation, ConfigName);
        if (DownloadFormSize != fDownloadFormSize)
            fDownloadFormSize = IniProvider.Write(DownloadFormSize, ConfigName);
        if (CreateThreadInTheBackground != fCreateThreadInTheBackground)
            fCreateThreadInTheBackground = IniProvider.Write(CreateThreadInTheBackground, ConfigName);
        if (MainFormColumnSizes != fMainFormColumnSizes)
            fMainFormColumnSizes = IniProvider.Write(MainFormColumnSizes, ConfigName);
        if (LogFormLocation != fLogFormLocation)
            fLogFormLocation = IniProvider.Write(LogFormLocation, ConfigName);
        if (LogFormSize != fLogFormSize)
            fLogFormSize = IniProvider.Write(LogFormSize, ConfigName);
    }

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    internal void Reset() {
        MainFormLocation = Config.InvalidPoint;
        MainFormSize = Size.Empty;
        DownloadFormLocation = Config.InvalidPoint;
        DownloadFormSize = Size.Empty;
        CreateThreadInTheBackground = false;
        MainFormColumnSizes = string.Empty;
    }

}