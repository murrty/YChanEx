#nullable enable
namespace YChanEx;
/// <summary>
/// Enumeration of the chan types available
/// </summary>
public enum ChanType : int {
    /// <summary>
    /// No chan was selected to download.
    /// </summary>
    Unsupported = -1,
    /// <summary>
    /// 4chan(nel) was selected to download.
    /// </summary>
    FourChan = 0,
    /// <summary>
    /// 420chan was selected to download.
    /// </summary>
    FourTwentyChan = 1,
    /// <summary>
    /// 7chan was selected to download.
    /// </summary>
    SevenChan = 2,
    /// <summary>
    /// 8chan was selected to download.
    /// </summary>
    EightChan = 3,
    /// <summary>
    /// 8kun was selected to download.
    /// </summary>
    EightKun = 4,
    /// <summary>
    /// fchan was selected to download.
    /// </summary>
    fchan = 5,
    /// <summary>
    /// u18chan was selected to download.
    /// </summary>
    u18chan = 6,
    /// <summary>
    /// An archival chan was selected to download.
    /// </summary>
    FoolFuuka = 7,
}