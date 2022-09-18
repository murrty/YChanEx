namespace YChanEx;
/// <summary>
/// Enumeration of the various thread statuses available
/// </summary>
public enum ThreadStatus : int {
    /// <summary>
    /// The thread has an unknown status.
    /// </summary>
    NoStatusSet = -1,

    /// <summary>
    /// The thread is waiting for the delay to rescan.
    /// </summary>
    Waiting = 0,
    /// <summary>
    /// The thread us currently scanning for files.
    /// </summary>
    ThreadScanning = 1,
    /// <summary>
    /// The thread is currently downloading files.
    /// </summary>
    ThreadDownloading = 2,
    /// <summary>
    /// The thread was not modified since last scan.
    /// </summary>
    ThreadNotModified = 3,
    /// <summary>
    /// The file from the thread has 404'd.
    /// </summary>
    ThreadFile404 = 4,
    /// <summary>
    /// The thread is not allowed to view the content.
    /// </summary>
    ThreadIsNotAllowed = 5,
    /// <summary>
    /// The thread is reloading into memory.
    /// </summary>
    ThreadReloaded = 6,


    /// <summary>
    /// The thread was alive when it was saved.
    /// </summary>
    ThreadIsAlive = 100,
    /// <summary>
    /// The thread 404'd
    /// </summary>
    ThreadIs404 = 101,
    /// <summary>
    /// The thread was aborted.
    /// </summary>
    ThreadIsAborted = 102,
    /// <summary>
    /// The thread was archived.
    /// </summary>
    ThreadIsArchived = 103,
    /// <summary>
    /// The thread is going to scan soon.
    /// </summary>
    ThreadScanningSoon = 104,

    /// <summary>
    /// The thread is retrying the download.
    /// </summary>
    ThreadRetrying = 666,
    /// <summary>
    /// The thread wasn't downloaded properly.
    /// </summary>
    ThreadImproperlyDownloaded = 777,
    /// <summary>
    /// The thread information wasn't given when the thread download started.
    /// </summary>
    ThreadInfoNotSet = 888,
    /// <summary>
    /// The thread encountered an unknown error.
    /// </summary>
    ThreadUnknownError = 999,
    /// <summary>
    /// Thread is requesting to update the name
    /// </summary>
    ThreadUpdateName = 1111,
}