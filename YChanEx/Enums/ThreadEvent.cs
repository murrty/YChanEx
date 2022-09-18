namespace YChanEx; 
/// <summary>
/// Enumeration of the thread events
/// </summary>
public enum ThreadEvent : int {
    /// <summary>
    /// The thread should parse the given information
    /// </summary>
    ParseForInfo = 0,
    /// <summary>
    /// The thread should start to download
    /// </summary>
    StartDownload = 1,
    /// <summary>
    /// The thread should update itself and wait until next download
    /// </summary>
    AfterDownload = 2,
    /// <summary>
    /// The thread should abort because the user requested it
    /// </summary>
    AbortDownload = 3,
    /// <summary>
    /// The thread should retry because the user requested it
    /// </summary>
    RetryDownload = 4,
    /// <summary>
    /// The thread was 404'd or aborted when it was added.
    /// </summary>
    ThreadWasGone = 5,
    ReloadThread = 6,

    /// <summary>
    /// The thread should abort because the application is closing.
    /// </summary>
    AbortForClosing = 999
}