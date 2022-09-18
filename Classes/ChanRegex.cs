namespace YChanEx;

/// <summary>
/// The Regex strings for detecting the chans.
/// </summary>
internal class ChanRegex {
    public static string fchanNames =>
        string.IsNullOrWhiteSpace(Config.Settings.Regex.fchanIDs) ? Chans.DefaultRegex.fchanFiles : Config.Settings.Regex.fchanIDs;
    public static string fchanIDs =>
        string.IsNullOrWhiteSpace(Config.Settings.Regex.fchanIDs) ? Chans.DefaultRegex.fchanIDs : Config.Settings.Regex.fchanIDs;
    public static string u18chanPosts =>
        string.IsNullOrWhiteSpace(Config.Settings.Regex.u18chanPosts) ? Chans.DefaultRegex.u18chanPosts : Config.Settings.Regex.u18chanPosts;
}