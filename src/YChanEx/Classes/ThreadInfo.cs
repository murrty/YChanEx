#nullable enable
namespace YChanEx;

using System.Drawing.Drawing2D;
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
    public ChanType Chan => this.Data.ChanType;
    /// <summary>
    /// The status of the thread, used to determine what's happening in the thread.
    /// <seealso cref="ThreadStatus"/>
    /// </summary>
    public ThreadStatus CurrentActivity { get; set; }
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
                ChanType.FourChan => $"https://a.4cdn.org/{this.Data.Board}/thread/{this.Data.Id}.json",
                ChanType.FourTwentyChan => $"https://api.420chan.org/{this.Data.Board}/res/{this.Data.Id}.json",
                ChanType.EightChan => $"https://8chan.moe/{this.Data.Board}/res/{this.Data.Id}.json",
                ChanType.EightKun => $"https://8kun.top/{this.Data.Board}/res/{this.Data.Id}.json",
                ChanType.FoolFuuka => $"https://{this.Data.UrlHost}/_/api/chan/thread?board={this.Data.Board}&num={this.Data.Id}",
                _ => this.Data.Url
            };
        }
    }
    /// <summary>
    /// Gets the string that this thread is represented by in the log.
    /// </summary>
    public string ThreadLogDisplay { get; set; }
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
        Data.Parent = this;
        this.Data = Data;
        this.ThreadIndex = -1;
        this.SavedThreadJson = string.Empty;
        this.DownloadPath = Downloads.DownloadPath;
        this.ThreadTopHtml = string.Empty;
        this.ThreadBottomHtml = string.Empty;
        this.ThreadLogDisplay = Data.ChanType switch {
            ChanType.FourChan => $"4chan /{Data.Board}/ thread {Data.Id}",
            ChanType.SevenChan => $"7chan /{Data.Board}/ thread {Data.Id}",
            ChanType.EightChan => $"8chan /{Data.Board}/ thread {Data.Id}",
            ChanType.EightKun => $"8kun /{Data.Board}/ thread {Data.Id}",
            ChanType.fchan => $"fchan /{Data.Board}/ thread {Data.Id}",
            ChanType.u18chan => $"u18chan /{Data.Board}/ thread {Data.Id}",
            ChanType.FoolFuuka => $"{Data.UrlHost} /{Data.Board}/ thread {Data.Id}",
            _ => "Unknown chan!!!"
        };
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
            if (File.Exists(this.SavedThreadJson)) {
                File.Move(this.SavedThreadJson, NewFile);
            }

            this.SavedThreadJson = NewFile;
        }
    }
    public void SaveHtml() {
        Directory.CreateDirectory(this.DownloadPath);
        string HtmlFile = Path.Combine(this.DownloadPath, "Thread.html");
        using FileStream fs = new(HtmlFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        using StreamWriter Writer = new(fs, Encoding.UTF8);
        Writer.Write(this.ThreadTopHtml);
        for (int i = 0; i < this.Data.ThreadPosts.Count; i++) {
            HtmlControl.WritePostHtmlData(this.Data.ThreadPosts[i], this, Writer);
        }
        Writer.Write(this.ThreadBottomHtml);
        Writer.Flush();
    }
    public void CheckQuotes() {
        for (int i = 0; i < this.Data.ThreadPosts.Count; i++) {
            var CurrentPost = this.Data.ThreadPosts[i];
            CurrentPost.QuotedBy = this.Data.ThreadPosts
                .Where(x => x.Quotes?.Contains(CurrentPost.PostId) == true)
                .Select(x => x.PostId)
                .ToArray();
        }
    }
}
