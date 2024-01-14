#nullable enable
namespace YChanEx;
public interface IMainFom {
    void SetItemStatus(int ThreadIndex, ThreadStatus Status);
    void ThreadKilled(ThreadInfo Thread);
}