#nullable enable
namespace YChanEx;
public enum ThreadState : byte {
    /// <summary>
    /// The thread was alive when it was saved.
    /// </summary>
    ThreadIsAlive = 0x0,
    /// <summary>
    /// The thread 404'd
    /// </summary>
    ThreadIs404 = 0x1,
    /// <summary>
    /// The thread was aborted.
    /// </summary>
    ThreadIsAborted = 0x2,
    /// <summary>
    /// The thread was archived.
    /// </summary>
    ThreadIsArchived = 0x3,
}
