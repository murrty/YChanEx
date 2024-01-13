﻿namespace YChanEx;

using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using YChanEx.Posts;
/// <summary>
/// Skeleton of thread downloads, used as a maintainer of thread information such as thread URL, ID, board, status.
/// <para>This contains all private information of the thread to reduce clutter on the main class.</para>
/// </summary>
public sealed class ThreadInfo {
    /// <summary>
    /// Contains all the data in the thread, such as posts, links, and lists.
    /// </summary>
    public ThreadData Data = new();

    /// <summary>
    /// The chan type that will be parsed.
    /// <seealso cref="ThreadStatus"/>
    /// </summary>
    public ChanType Chan = ChanType.Unsupported;
    /// <summary>
    /// The index of the thread in the queue.
    /// </summary>
    public int ThreadIndex { get; set; } = -1;
    /// <summary>
    /// The string to the json file for the thread if it is saved.
    /// </summary>
    public string SavedThreadJson { get; set; } = string.Empty;

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
    public ThreadStatus CurrentActivity { get; set; } = ThreadStatus.NoStatusSet;
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
    public Uri ThreadUri { get; set; }
    /// <summary>
    /// The API link for the current thread, if it has an API link.
    /// </summary>
    public string ApiLink {
        get {
            return Chan switch {
                ChanType.FourChan => $"https://a.4cdn.org/{Data.Board}/thread/{Data.Id}.json",
                ChanType.FourTwentyChan => $"https://api.420chan.org/{Data.Board}/res/{Data.Id}.json",
                ChanType.EightChan => $"https://8chan.moe/{Data.Board}/res/{Data.Id}.json",
                ChanType.EightKun => $"https://8kun.top/{Data.Board}/res/{Data.Id}.json",
                _ => null
            };
        }
    }
    /// <summary>
    /// Whether the HTML file exists.
    /// </summary>
    public bool HtmlExists {
        get {
            string HtmlFile = Path.Combine(Data.DownloadPath, "Thread.html");
            return File.Exists(HtmlFile);
        }
    }

    public ThreadInfo(ThreadData Data, ChanType Chan) {
        this.Chan = Chan;
        this.Data = Data;
    }

    public ThreadInfo(ChanType Chan) {
        this.Chan = Chan;
        this.Data = new();
    }

    public void UpdateJsonPath() {
        string NewFile = $"{Program.SavedThreadsPath}\\{ThreadIndex + 1:D4}-{this.Chan switch {
            ChanType.FourChan => "4chan-",
            ChanType.FourTwentyChan => "420chan-",
            ChanType.SevenChan => "7chan-",
            ChanType.EightChan => "8chan-",
            ChanType.EightKun => "8kun-",
            ChanType.fchan => "fchan-",
            ChanType.u18chan => "u18chan-",
            _ => throw new InvalidOperationException(nameof(ChanType))
        }}{this.Data.Board}-{this.Data.Id}.thread.json";

        if (SavedThreadJson != NewFile) {
            if (System.IO.File.Exists(SavedThreadJson))
                System.IO.File.Move(SavedThreadJson, NewFile);

            SavedThreadJson = NewFile;
        }
    }
    public void SaveHtml() {
        string HtmlFile = Path.Combine(Data.DownloadPath, "Thread.html");
        using FileStream fs = new(HtmlFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        using StreamWriter Writer = new(fs, Encoding.UTF8);
        Writer.Write(ThreadTopHtml);
        for (int i = 0; i < Data.ThreadPosts.Count; i++) {
            Writer.Write(Data.ThreadPosts[i].PostHtml);
        }
        Writer.Write(ThreadBottomHtml);
        Writer.Flush();
    }
}

public sealed class ThreadData {
    /// <summary>
    /// The overall status of the thread. (Alive/404/Aborted/Archived).
    /// <seealso cref="YChanEx.ThreadStatus"/>
    /// </summary>
    [DataMember]
    public ThreadState ThreadState { get; set; }

    /// <summary>
    /// The ID of the thread being downloaded.
    /// </summary>
    [DataMember]
    public string Id { get; set; }
    /// <summary>
    /// The board that the thread that's being downloaded is on. Generally an acronym or a single letter.
    /// </summary>
    [DataMember]
    public string Board { get; set; }
    /// <summary>
    /// The URL of the thread that will be downloaded.
    /// </summary>
    [DataMember]
    public string Url { get; set; }
    /// <summary>
    /// The path that the thread will be downloaded to.
    /// </summary>
    [DataMember]
    public string DownloadPath { get; set; }

    /// <summary>
    /// The list of post IDs that have been parsed.
    /// </summary>
    [DataMember]
    public List<ulong> ParsedPostIds { get; set; } = [];
    /// <summary>
    /// The posts in the thread.
    /// </summary>
    [DataMember]
    public List<GenericPost> ThreadPosts { get; set; } = [];
    /// <summary>
    /// Dictionary of duplicate names.
    /// </summary>
    [DataMember]
    public Dictionary<string, int> DuplicateNames { get; set; } = [];
    /// <summary>
    /// The estimated total size of the thread.
    /// </summary>
    [DataMember]
    public long EstimatedSize { get; set; }

    /// <summary>
    /// Counts the images in the thread when they were added.
    /// <para>Used by all chans.</para>
    /// </summary>
    [DataMember]
    public int ThreadImagesCount { get; set; }
    /// <summary>
    /// Counts how many images were downloaded, for the form.
    /// <para>Used by all chans.</para>
    /// </summary>
    [DataMember]
    public int DownloadedImagesCount { get; set; }
    /// <summary>
    /// The amount of posts counted up to.
    /// <para>Used by 8chan and 8kun.</para>
    /// </summary>
    [DataMember]
    public int ThreadPostsCount { get; set; }

    /// <summary>
    /// The last modified date, used as a header when trying to download json or html information.
    /// <para>Uses "If-Modified-Since" header on HttpWebRequests to prevent overload.</para>
    /// <para>All download logic includes this, but only a few actually make use of it.</para>
    /// </summary>
    [DataMember]
    public DateTimeOffset? LastModified { get; set; }
    /// <summary>
    /// Determines if the thread is archived.
    /// </summary>
    [DataMember]
    public bool ThreadArchived { get; set; }
    /// <summary>
    /// The board name of the thread retrieved from <seealso cref="BoardTitles"/>.
    /// <para>Chans that support user-made boards requires the name to be parsed from HTML.</para>
    /// </summary>
    [DataMember]
    public string BoardName { get; set; }
    /// <summary>
    /// Determines if the board name has been retrieved, either from HTML or <seealso cref="BoardTitles"/>.
    /// <para>Prevents unneccessary parsing.</para>
    /// </summary>
    [DataMember]
    public bool RetrievedBoardName { get; set; }
    /// <summary>
    /// The name of the thread, given by OP of the Thread.
    /// <para>Used for easier thread identification.</para>
    /// </summary>
    [DataMember]
    public string ThreadName { get; set; }
    /// <summary>
    /// Determines if the thread name has been retrieved from the current thread, either through API or HTML parsing.
    /// <para>Used for easier thread identification.</para>
    /// </summary>
    [DataMember]
    public bool RetrievedThreadName { get; set; }
    /// <summary>
    /// Determines if a custom name was set for the thread.
    /// </summary>
    [DataMember]
    public bool SetCustomName { get; set; }
    /// <summary>
    /// The user-set name for the thread. Only appears in the main form listview, and the thread form text.
    /// </summary>
    [DataMember]
    public string CustomThreadName { get; set; }
    /// <summary>
    /// Determines if the thread name was set into the HTML that will be saved.
    /// </summary>
    [DataMember]
    public bool HtmlThreadNameSet { get; set; }
    /// <summary>
    /// The subtitle of the board used by certain chans.
    /// </summary>
    [DataMember]
    public string BoardSubtitle { get; set; }

    /// <summary>
    /// The thread name that is displayed on the download form.
    /// </summary>
    [IgnoreDataMember]
    public string DownloadFormThreadNameDisplay {
        get {
            if (SetCustomName) {
                return CustomThreadName;
            }
            if (RetrievedThreadName) {
                return ThreadName;
            }
            return Id;
        }
    }
}