namespace YChanEx;

using System.Drawing;

/// <summary>
/// Contains saved info configuration.
/// </summary>
public static class Saved {
    private const string ConfigName = "Saved";

    static Saved() {
        fMainFormLocation = IniProvider.Read(MainFormLocation, Config.InvalidPoint, ConfigName);
        fMainFormSize = IniProvider.Read(MainFormSize, Size.Empty, ConfigName);
        fDownloadFormLocation = IniProvider.Read(DownloadFormLocation, Config.InvalidPoint, ConfigName);
        fDownloadFormSize = IniProvider.Read(DownloadFormSize, Size.Empty, ConfigName);
        fCreateThreadInTheBackground = IniProvider.Read(CreateThreadInTheBackground, false, ConfigName);
        fMainFormColumnSizes = IniProvider.Read(MainFormColumnSizes, string.Empty, ConfigName);
        fLogFormLocation = IniProvider.Read(LogFormLocation, Config.InvalidPoint, ConfigName);
        fLogFormSize = IniProvider.Read(LogFormSize, Size.Empty, ConfigName);
    }

    /// <summary>
    /// The last Point of the main form.
    /// </summary>
    public static Point MainFormLocation {
        get => fMainFormLocation;
        set {
            if (fMainFormLocation != value) {
                fMainFormLocation = value;
                IniProvider.Write(MainFormLocation, ConfigName);
            }
        }
    }
    private static Point fMainFormLocation;

    /// <summary>
    /// The last Size of the main form.
    /// </summary>
    public static Size MainFormSize {
        get => fMainFormSize;
        set {
            if (fMainFormSize != value) {
                fMainFormSize = value;
                IniProvider.Write(MainFormSize, ConfigName);
            }
        }
    }
    private static Size fMainFormSize;

    /// <summary>
    /// The last Point of the main form.
    /// </summary>
    public static Point DownloadFormLocation {
        get => fDownloadFormLocation;
        set {
            if (fDownloadFormLocation != value) {
                fDownloadFormLocation = value;
                IniProvider.Write(DownloadFormLocation, ConfigName);
            }
        }
    }
    private static Point fDownloadFormLocation;

    /// <summary>
    /// The last Size of the download form.
    /// </summary>
    public static Size DownloadFormSize {
        get => fDownloadFormSize;
        set {
            if (fDownloadFormSize != value) {
                fDownloadFormSize = value;
                IniProvider.Write(DownloadFormSize, ConfigName);
            }
        }
    }
    private static Size fDownloadFormSize;

    /// <summary>
    /// Whether the thread should be created in the background, and not show a window when added.
    /// </summary>
    public static bool CreateThreadInTheBackground {
        get => fCreateThreadInTheBackground;
        set {
            if (fCreateThreadInTheBackground != value) {
                fCreateThreadInTheBackground = value;
                IniProvider.Write(CreateThreadInTheBackground, ConfigName);
            }
        }
    }
    private static bool fCreateThreadInTheBackground;

    /// <summary>
    /// The joined-string of the main forms' column sizes.
    /// </summary>
    public static string MainFormColumnSizes {
        get => fMainFormColumnSizes;
        set {
            if (fMainFormColumnSizes != value) {
                fMainFormColumnSizes = value;
                IniProvider.Write(MainFormColumnSizes, ConfigName);
            }
        }
    }
    private static string fMainFormColumnSizes;

    public static Point LogFormLocation {
        get => fLogFormLocation;
        set {
            if (fLogFormLocation != value) {
                fLogFormLocation = value;
                IniProvider.Write(LogFormLocation, ConfigName);
            }
        }
    }
    private static Point fLogFormLocation;

    public static Size LogFormSize {
        get => fLogFormSize;
        set {
            if (fLogFormSize != value) {
                fLogFormSize = value;
                IniProvider.Write(LogFormSize, ConfigName);
            }
        }
    }
    private static Size fLogFormSize;

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    internal static void Reset() {
        MainFormLocation = Config.InvalidPoint;
        MainFormSize = Size.Empty;
        DownloadFormLocation = Config.InvalidPoint;
        DownloadFormSize = Size.Empty;
        CreateThreadInTheBackground = false;
        MainFormColumnSizes = string.Empty;
    }
}