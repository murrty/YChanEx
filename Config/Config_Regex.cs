namespace YChanEx;

/// <summary>
/// Contains regular expression configuration.
/// </summary>
public sealed class Config_Regex {

    private const string ConfigName = "Regex";

    #region Fields
    /// <summary>
    /// The pattern of fchans' files used in Regular Expression.
    /// </summary>
    public string fchanFiles { get; set; }
    /// <summary>
    /// The pattern of fchans' post IDs used in Regular Expression.
    /// </summary>
    public string fchanIDs { get; set; }
    /// <summary>
    /// The pattern of u18chans' files used in Regular Expression.
    /// </summary>
    public string u18chanFiles { get; set; }
    /// <summary>
    /// The pattern of 8chans' URL used in Regular Expression.
    /// </summary>
    public string u18chanID { get; set; }
    /// <summary>
    /// The pattern of u18chans' post IDs used in Regular Expression.
    /// </summary>
    public string u18chanPosts { get; set; }

    private string ffchanFiles;
    private string ffchanIDs;
    private string fu18chanFiles;
    private string fu18chanID;
    private string fu18chanPosts;
    #endregion

    public void Load() {
        ffchanFiles = fchanFiles = IniProvider.Read(fchanFiles, string.Empty, ConfigName);
        ffchanIDs = fchanIDs = IniProvider.Read(fchanIDs, string.Empty, ConfigName);
        fu18chanFiles = u18chanFiles = IniProvider.Read(u18chanFiles, string.Empty, ConfigName);
        fu18chanID = u18chanID = IniProvider.Read(u18chanID, string.Empty, ConfigName);
        fu18chanPosts = u18chanPosts = IniProvider.Read(u18chanPosts, string.Empty, ConfigName);
    }

    public void Save() {
        if (fchanFiles != ffchanFiles)
            IniProvider.Write(fchanFiles, ConfigName);
        if (fchanIDs != ffchanIDs)
            IniProvider.Write(fchanIDs, ConfigName);
        if (u18chanFiles != fu18chanFiles)
            IniProvider.Write(u18chanFiles, ConfigName);
        if (u18chanID != fu18chanID)
            IniProvider.Write(u18chanID, ConfigName);
        if (u18chanPosts != fu18chanPosts)
            IniProvider.Write(u18chanPosts, ConfigName);
    }

    /// <summary>
    /// Resets the config to defaults.
    /// </summary>
    internal void Reset() {
        fchanFiles = string.Empty;
        fchanIDs = string.Empty;
        u18chanFiles = string.Empty;
        u18chanID = string.Empty;
        u18chanPosts = string.Empty;
    }

}