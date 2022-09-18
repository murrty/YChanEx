namespace YChanEx;

using System.Drawing;

/// <summary>
/// Handles most config handling.
/// </summary>
public sealed class Config {

    internal static readonly Point InvalidPoint = new(-32_000, -32_000);

    #region Main config instance
    public static Config Settings { get; set; } = new();
    #endregion

    #region Sub-config Instances
    public Config_Initialization Initialization { get; set; }
    public Config_General General { get; set; }
    public Config_Downloads Downloads { get; set; }
    public Config_Saved Saved { get; set; }
    public Config_Regex Regex { get; set; }
    public Config_Advanced Advanced { get; set; }
    #endregion

    #region Fields
    internal string SavedThreadsPath { get; set; }
    #endregion

    public Config() {
        Initialization = new();
        General = new();
        Downloads = new();
        Saved = new();
        Regex = new();
        Advanced = new();
        SavedThreadsPath = $"{Environment.CurrentDirectory}\\SavedThreads";
    }

    /// <summary>
    /// Loads the configurations into memory.
    /// </summary>
    internal static void Load() {
        Settings.Initialization.Load();
        Settings.General.Load();
        Settings.Downloads.Load();
        Settings.Saved.Load();
        Settings.Regex.Load();
        Settings.Advanced.Load();
    }

    /// <summary>
    /// Saves the configurations.
    /// </summary>
    internal static void Save() {
        Settings.Initialization.Save();
        Settings.General.Save();
        Settings.Downloads.Save();
        Settings.Saved.Save();
        Settings.Regex.Save();
        Settings.Advanced.Save();
    }

    /// <summary>
    /// Checks if a point is a valid one to use.
    /// </summary>
    /// <param name="input">The <seealso cref="Point"/> value to validate.</param>
    /// <returns>If the input is a valid point.</returns>
    public static bool ValidPoint(Point input) {
        return input.X != -32000 && input.Y != -32000;
    }

    /// <summary>
    /// Checks if a size is a valid one to use.
    /// </summary>
    /// <param name="input">The <seealso cref="Size"/> value to validate.</param>
    /// <returns>If the input is a valid size.</returns>
    public static bool ValidSize(Size input) {
        return input.Width > 0 && input.Height > 0;
    }

}