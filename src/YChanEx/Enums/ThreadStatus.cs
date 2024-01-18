#nullable enable
namespace YChanEx;
/// <summary>
/// Enumeration of the various thread statuses available
/// </summary>
public enum ThreadStatus : ushort {
    /// <summary>
    /// The thread has an unknown status.
    /// </summary>
    NoStatusSet = 0,

    /// <summary>
    /// The thread is waiting for the delay to rescan.
    /// </summary>
    Waiting = 1,
    /// <summary>
    /// The thread us currently scanning for files.
    /// </summary>
    ThreadScanning = 2,
    /// <summary>
    /// The thread is currently downloading files.
    /// </summary>
    ThreadDownloading = 3,
    /// <summary>
    /// The thread was not modified since last scan.
    /// </summary>
    ThreadNotModified = 4,
    /// <summary>
    /// The file from the thread has 404'd.
    /// </summary>
    ThreadFile404 = 5,
    /// <summary>
    /// The thread is not allowed to view the content.
    /// </summary>
    ThreadIsNotAllowed = 6,
    /// <summary>
    /// The thread is reloading into memory.
    /// </summary>
    ThreadReloaded = 7,

    /// <summary>
    /// The thread 404'd
    /// </summary>
    ThreadIs404 = 100,
    /// <summary>
    /// The thread was aborted.
    /// </summary>
    ThreadIsAborted = 101,
    /// <summary>
    /// The thread was archived.
    /// </summary>
    ThreadIsArchived = 102,
    /// <summary>
    /// The thread is going to scan soon.
    /// </summary>
    ThreadScanningSoon = 103,
    /// <summary>
    /// Thread is requesting to update the name
    /// </summary>
    ThreadUpdateName = 104,

    /// <summary>
    /// The thread information wasn't given when the thread download started.
    /// </summary>
    ThreadInfoNotSet = 200,
    /// <summary>
    /// The thread is retrying the download.
    /// </summary>
    ThreadRetrying = 201,
    /// <summary>
    /// The thread wasn't downloaded properly.
    /// </summary>
    ThreadImproperlyDownloaded = 202,
    /// <summary>
    /// The thread does not have any posts.
    /// </summary>
    NoThreadPosts = 203,
    /// <summary>
    /// The thread could not be parseed.
    /// </summary>
    FailedToParseThreadHtml = 204,
    /// <summary>
    /// The thread encountered an unknown error.
    /// </summary>
    ThreadUnknownError = 205,
    /// <summary>
    /// The thread encountered an exception that wasn't handled.
    /// </summary>
    ThreadUnhandledException = 206,
}