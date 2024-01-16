#nullable enable
namespace YChanEx;
public interface IMainFom {
    /// <summary>
    /// Sets the thread status from another thread handle to change the status on the main form.
    /// </summary>
    /// <param name="Thread">The thread that was 404'd, archived, or aborted.</param>
    /// <param name="Status">The new custom status to be set onto it.</param>
    void SetItemStatus(ThreadInfo Thread, ThreadStatus Status);
    /// <summary>
    /// Removes the thread if it was killed (archived or 404)
    /// </summary>
    /// <param name="Thread">The thread that was 404'd, archived, or aborted.</param>
    void ThreadKilled(ThreadInfo Thread);
    /// <summary>
    /// Adds a thread to the history.
    /// </summary>
    /// <param name="Thread">The thread that will be added.</param>
    void AddToHistory(PreviousThread Thread);
}
