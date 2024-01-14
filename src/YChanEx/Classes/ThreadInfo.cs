#nullable enable
namespace YChanEx;
using System.IO;
using System.Net;
using System.Text;
/// <summary>
/// Skeleton of thread downloads, used as a maintainer of thread information such as thread URL, ID, board, status.
/// <para>This contains all private information of the thread to reduce clutter on the main class.</para>
/// </summary>
public sealed class ThreadInfo {
    /// <summary>
    /// Contains all the data in the thread, such as posts, links, and lists.
    /// </summary>
    public ThreadData Data { get; }

    /// <summary>
    /// The chan type that will be parsed.
    /// <seealso cref="ThreadStatus"/>
    /// </summary>
    public ChanType Chan => Data.ChanType;
    /// <summary>
    /// The index of the thread in the queue.
    /// </summary>
    public int ThreadIndex { get; set; }
    /// <summary>
    /// The string to the json file for the thread if it is saved.
    /// </summary>
    public string SavedThreadJson { get; set; }
    /// <summary>
    /// The path that the thread will be downloaded to.
    /// </summary>
    public string DownloadPath { get; set; }

    /// <summary>
    /// Determines if a thread is currently downloading images. Used to set FileWas404 on download 404.
    /// <para>Used by all chans.</para>
    /// </summary>
    public bool DownloadingFiles { get; set; }
    /// <summary>
    /// Determines if scanning should be skipped and the file should be attempted to be redownloaded.
    /// <para>Used by all chans.</para>
    /// </summary>
    public bool FileWas404 { get; set; }
    /// <summary>
    /// Used to keep track of the redownload attemps.
    /// <para>Used by all chans.</para>
    /// </summary>
    public int RetryCountFor404 { get; set; }
    /// <summary>
    /// Whether the thread was modified.
    /// </summary>
    public bool ThreadModified { get; set; }
    /// <summary>
    /// Whether new files have been added.
    /// </summary>
    public bool AddedNewPosts { get; set; }
    /// <summary>
    /// Whether the thread was reloaded.
    /// </summary>
    public bool ThreadReloaded { get; set; }

    /// <summary>
    /// The countdown to the next scan.
    /// </summary>
    public int CountdownToNextScan { get; set; }
    /// <summary>
    /// The time that the "not modified" label should be hidden at.
    /// <para>It is determined by subracting 10 from CountdownToNextScan.</para>
    /// </summary>
    public int HideModifiedLabelAt { get; set; }

    /// <summary>
    /// The status of the thread, used to determine what's happening in the thread.
    /// <seealso cref="ThreadStatus"/>
    /// </summary>
    public ThreadStatus CurrentActivity { get; set; }
    /// <summary>
    /// The status code from the previous httprequest.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// The upper portion of the thread HTML, above the posts, custom built for YOUR CONVENIENCE.
    /// </summary>
    public string ThreadTopHtml { get; set; }
    /// <summary>
    /// The lower portion of the thread HTML, below the posts, custom built for YOUR CONVENIENCE.
    /// </summary>
    public string ThreadBottomHtml { get; set; }
    /// <summary>
    /// The URI of the ThreadUrl.
    /// </summary>
    public Uri? ThreadUri { get; set; }
    /// <summary>
    /// The API link for the current thread, if it has an API link.
    /// </summary>
    public string? ApiLink {
        get {
            return this.Chan switch {
                ChanType.FourChan => $"https://a.4cdn.org/{Data.Board}/thread/{Data.Id}.json",
                ChanType.FourTwentyChan => $"https://api.420chan.org/{Data.Board}/res/{Data.Id}.json",
                ChanType.EightChan => $"https://8chan.moe/{Data.Board}/res/{Data.Id}.json",
                ChanType.EightKun => $"https://8kun.top/{Data.Board}/res/{Data.Id}.json",
                ChanType.FoolFuuka => $"https://{Data.UrlHost}/_/api/chan/thread?board={Data.Board}&num={Data.Id}",
                _ => null
            };
        }
    }
    /// <summary>
    /// Whether the HTML file exists.
    /// </summary>
    public bool HtmlExists {
        get {
            string HtmlFile = Path.Combine(DownloadPath, "Thread.html");
            return File.Exists(HtmlFile);
        }
    }

    public ThreadInfo(ThreadData Data) {
        this.Data = Data;
        this.ThreadIndex = -1;
        this.SavedThreadJson = string.Empty;
        this.DownloadPath = Downloads.DownloadPath;
        this.ThreadTopHtml = string.Empty;
        this.ThreadBottomHtml = string.Empty;
    }

    public void UpdateJsonPath() {
        string NewFile = $"{Program.SavedThreadsPath}\\{this.ThreadIndex + 1:D4}-{this.Chan switch {
            ChanType.FourChan => "4chan-",
            ChanType.FourTwentyChan => "420chan-",
            ChanType.SevenChan => "7chan-",
            ChanType.EightChan => "8chan-",
            ChanType.EightKun => "8kun-",
            ChanType.fchan => "fchan-",
            ChanType.u18chan => "u18chan-",
            ChanType.FoolFuuka => this.Data.UrlHost + "-",
            _ => throw new InvalidOperationException(nameof(ChanType))
        }}{this.Data.Board}-{this.Data.Id}.thread.json";

        if (this.SavedThreadJson != NewFile) {
            if (File.Exists(SavedThreadJson)) {
                File.Move(SavedThreadJson, NewFile);
            }

            this.SavedThreadJson = NewFile;
        }
    }
    public void SaveHtml() {
        string HtmlFile = Path.Combine(DownloadPath, "Thread.html");
        using FileStream fs = new(HtmlFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        using StreamWriter Writer = new(fs, Encoding.UTF8);
        Writer.Write(ThreadTopHtml);
        for (int i = 0; i < Data.ThreadPosts.Count; i++) {
            HtmlControl.WritePostHtmlData(Data.ThreadPosts[i], this, Writer);
        }
        Writer.Write(ThreadBottomHtml);
        Writer.Flush();
    }
    public void CheckQuotes() {
        for (int i = 0; i < Data.ThreadPosts.Count; i++) {
            var CurrentPost = Data.ThreadPosts[i];
            CurrentPost.QuotedBy = Data.ThreadPosts
                .Where(x => x.Quotes?.Contains(CurrentPost.PostId) == true)
                .Select(x => x.PostId)
                .ToArray();
        }
    }
}
