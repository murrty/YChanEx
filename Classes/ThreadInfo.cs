namespace YChanEx;

using System.Runtime.Serialization;
using System.Net;

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
    public bool DownloadingFiles { get; set; } = false;
    /// <summary>
    /// Determines if scanning should be skipped and the file should be attempted to be redownloaded.
    /// <para>Used by all chans.</para>
    /// </summary>
    public bool FileWas404 { get; set; } = false;
    /// <summary>
    /// Used to keep track of the redownload attemps.
    /// <para>Used by all chans.</para>
    /// </summary>
    public int RetryCountFor404 { get; set; } = 0;
    /// <summary>
    /// Whether the thread was modified.
    /// </summary>
    public bool ThreadModified { get; set; } = false;
    /// <summary>
    /// Whether new files have been added.
    /// </summary>
    public bool AddedNewPosts { get; set; } = false;

    /// <summary>
    /// The countdown to the next scan.
    /// </summary>
    public int CountdownToNextScan { get; set; } = 0;
    /// <summary>
    /// The time that the "not modified" label should be hidden at.
    /// <para>It is determined by subracting 10 from CountdownToNextScan.</para>
    /// </summary>
    public int HideModifiedLabelAt { get; set; } = 0;

    /// <summary>
    /// The status of the thread, used to determine what's happening in the thread.
    /// <seealso cref="ThreadStatus"/>
    /// </summary>
    public ThreadStatus CurrentActivity { get; set; } = ThreadStatus.NoStatusSet;
    /// <summary>
    /// Determines if the ThreadInfo has been set.
    /// </summary>
    public bool ThreadInfoSet { get; set; } = false;

    /// <summary>
    /// The HTML of the thread, custom built for YOUR CONVENIENCE.
    /// </summary>
    public string ThreadHTML { get; set; } = null;
    /// <summary>
    /// Whether the ThreadHTML has been prepared. Used by 8chan/8kun.
    /// </summary>
    public bool ThreadHTMLPrepared { get; set; } = false;
    /// <summary>
    /// The HTML source of the thread that has been scanned.
    /// <para>This is used on the threads that do not make use of LastModified by comparing old HTML to new HTML.</para>
    /// <para>It'd be easier if they just supported If-Last-Modified, but here we are.</para>
    /// </summary>
    public string LastThreadHTML { get; set; } = null;
    /// <summary>
    /// The cookie container that contains cookies to connect to websites that require them.
    /// <para>Currently, only fchan requires this.</para>
    /// </summary>
    public CookieContainer ThreadCookieContainer { get; set; } = null;

    public ThreadInfo(ThreadData Info, ChanType Type) {
        Chan = Type;
        Data = Info;
    }

    public ThreadInfo(ChanType Type) {
        Chan = Type;
        Data = new();
    }

    public void UpdateJsonPath() {
        string NewFile = $"{Config.Settings.SavedThreadsPath}\\{ThreadIndex + 1:D4}-{this.Chan switch {
            ChanType.FourChan => "4chan-",
            ChanType.FourTwentyChan => "420chan-",
            ChanType.SevenChan => "7chan-",
            ChanType.EightChan => "8chan-",
            ChanType.EightKun => "8kun-",
            ChanType.fchan => "fchan-",
            ChanType.u18chan => "u18chan-",
            _ => throw new InvalidOperationException(nameof(ChanType))
        }}{this.Data.ThreadBoard}-{this.Data.ThreadID}.thread.json";

        if (SavedThreadJson != NewFile) {
            if (System.IO.File.Exists(SavedThreadJson))
                System.IO.File.Move(SavedThreadJson, NewFile);

            SavedThreadJson = NewFile;
        }
    }
}

public sealed class ThreadData {

    /// <summary>
    /// The overall status of the thread. (Alive/404/Aborted/Archived).
    /// <seealso cref="ThreadStatus"/>
    /// </summary>
    [DataMember]
    public ThreadStatus OverallStatus = ThreadStatus.NoStatusSet;

    /// <summary>
    /// The URL of the thread that will be downloaded.
    /// </summary>
    [DataMember]
    public string ThreadURL { get; set; } = null;
    /// <summary>
    /// The ID of the thread being downloaded.
    /// </summary>
    [DataMember]
    public string ThreadID { get; set; } = null;
    /// <summary>
    /// The board that the thread that's being downloaded is on.
    /// </summary>
    [DataMember]
    public string ThreadBoard { get; set; } = null;
    /// <summary>
    /// The path that the thread will be downloaded to.
    /// </summary>
    [DataMember]
    public string DownloadPath { get; set; } = null;

    /// <summary>
    /// The list of post IDs that have been parsed.
    /// </summary>
    [DataMember]
    public List<string> ParsedPostIDs { get; set; } = new();
    /// <summary>
    /// The list of file IDs displayed on the form.
    /// </summary>
    [DataMember]
    public List<string> FileIDs { get; set; } = new();
    /// <summary>
    /// Contains the post ID of the image. Used for tracking the post in the parsed posts list.
    /// </summary>
    [DataMember]
    public List<string> ImagePostIDs { get; set; } = new();
    /// <summary>
    /// The list of file extensions displayed on the form.
    /// </summary>
    [DataMember]
    public List<string> FileExtensions { get; set; } = new();
    /// <summary>
    /// The list of original file names displayed on the form.
    /// </summary>
    [DataMember]
    public List<string> FileOriginalNames { get; set; } = new();
    /// <summary>
    /// The list of File Hashes displayed on the form.
    /// </summary>
    [DataMember]
    public List<string> FileHashes { get; set; } = new();

    /// <summary>
    /// The list of image files that will be downloaded.
    /// </summary>
    [DataMember]
    public List<string> ImageFiles { get; set; } = new();
    /// <summary>
    /// The list of thumbnail files that will be downloaded.
    /// </summary>
    [DataMember]
    public List<string> ThumbnailFiles { get; set; } = new();
    /// <summary>
    /// The list of file names (including extension) that will be set to the file that gets downloaded.
    /// </summary>
    [DataMember]
    public List<string> FileNames { get; set; } = new();
    /// <summary>
    /// The list of thumbnail file names that will be set to the file that gets downloaded.
    /// </summary>
    [DataMember]
    public List<string> ThumbnailNames { get; set; } = new();
    /// <summary>
    /// The list of files that are duplicated in a thread.
    /// <para>If a file exists with the name, it'll be added here.</para>
    /// </summary>
    [DataMember]
    public List<string> FileNamesDupes { get; set; } = new();
    /// <summary>
    /// The list of the count of duped files.
    /// <para>If a file exists with the name, the amount that appears will be added here.</para>
    /// </summary>
    [DataMember]
    public List<int> FileNamesDupesCount { get; set; } = new();
    /// <summary>
    /// The list of the <see cref="FileDownloadStatus"/> of the file.
    /// </summary>
    [DataMember]
    public List<FileDownloadStatus> FileStatus { get; set; } = new();

    /// <summary>
    /// The list of posts for saving HTML.
    /// </summary>
    [DataMember]
    public List<PostData> Posts { get; set; } = new();
    /// <summary>
    /// Counts the images in the thread when they were added.
    /// <para>Used by all chans.</para>
    /// </summary>
    [DataMember]
    public int ThreadImagesCount { get; set; } = 0;
    /// <summary>
    /// Counts how many images were downloaded, for the form.
    /// <para>Used by all chans.</para>
    /// </summary>
    [DataMember]
    public int DownloadedImagesCount { get; set; } = 0;
    /// <summary>
    /// The amount of posts counted up to.
    /// <para>Used by 8chan and 8kun.</para>
    /// </summary>
    [DataMember]
    public int ThreadPostsCount { get; set; } = 0;

    /// <summary>
    /// The last modified date, used as a header when trying to download json or html information.
    /// <para>Uses "If-Modified-Since" header on HttpWebRequests to prevent overload.</para>
    /// <para>All download logic includes this, but only a few actually make use of it.</para>
    /// </summary>
    [DataMember]
    public DateTime LastModified { get; set; } = default;
    /// <summary>
    /// Determines if the thread is archived.
    /// </summary>
    [DataMember]
    public bool ThreadArchived { get; set; } = false;
    /// <summary>
    /// The board name of the thread retrieved from <seealso cref="BoardTitles"/>.
    /// <para>Chans that support user-made boards requires the name to be parsed from HTML.</para>
    /// </summary>
    [DataMember]
    public string BoardName { get; set; } = null;
    /// <summary>
    /// Determines if the board name has been retrieved, either from HTML or <seealso cref="BoardTitles"/>.
    /// <para>Prevents unneccessary parsing.</para>
    /// </summary>
    [DataMember]
    public bool RetrievedBoardName { get; set; } = false;
    /// <summary>
    /// The name of the thread, given by OP of the Thread.
    /// <para>Used for easier thread identification.</para>
    /// </summary>
    [DataMember]
    public string ThreadName { get; set; } = null;
    /// <summary>
    /// Determines if the thread name has been retrieved from the current thread, either through API or HTML parsing.
    /// <para>Used for easier thread identification.</para>
    /// </summary>
    [DataMember]
    public bool RetrievedThreadName { get; set; } = false;
    /// <summary>
    /// Determines if a custom name was set for the thread.
    /// </summary>
    [DataMember]
    public bool SetCustomName { get; set; } = false;
    /// <summary>
    /// The user-set name for the thread. Only appears in the main form listview, and the thread form text.
    /// </summary>
    [DataMember]
    public string CustomThreadName { get; set; } = null;
    /// <summary>
    /// Determines if the thread name was set into the HTML that will be saved.
    /// </summary>
    [DataMember]
    public bool HtmlThreadNameSet { get; set; } = false;
    /// <summary>
    /// The subtitle of the board used by certain chans.
    /// </summary>
    [DataMember]
    public string BoardSubtitle { get; set; } = null;

}

/// <summary>
/// Contains information about the post.
/// <para>Only used if SaveHTML is enabled.</para>
/// </summary>
public sealed class PostData {

    /// <summary>
    /// Whether the post is the first post in the thread.
    /// </summary>
    public bool FirstPost { get; set; } = false;

    /// <summary>
    /// The ID of the post.
    /// </summary>
    public string PostID { get; set; } = null;
    /// <summary>
    /// The date of the post.
    /// </summary>
    public string PostDate { get; set; } = null;
    /// <summary>
    /// The displayed name of the poser.
    /// </summary>
    public string PosterName { get; set; } = null;
    /// <summary>
    /// The tripcode of the post.
    /// </summary>
    public string PosterTripcode { get; set; } = null;
    /// <summary>
    /// The extra name of the post, usually it's "Mod" or "Admin" or something.
    /// </summary>
    public string SpecialPosterName { get; set; } = null;
    /// <summary>
    /// The ID of the user on boards that have pseudo-identification.
    /// </summary>
    public string PosterID { get; set; } = null;
    /// <summary>
    /// The subject of the post (Usually the OP)
    /// </summary>
    public string PostSubject { get; set; } = null;
    /// <summary>
    /// The comment on the post.
    /// </summary>
    public string PostMessage { get; set; } = null;

    /// <summary>
    /// Contains a list of all files in the post.
    /// </summary>
    public List<FileData> Files { get; set; } = new();

    /// <summary>
    /// Contains information about the files in a post.
    /// </summary>
    public sealed class FileData {
        /// <summary>
        /// Gets or sets the ID of the file.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Gets or sets the original name of the file.
        /// </summary>
        public string OriginalName { get; set; }
        /// <summary>
        /// Gets or sets the extension of the file.
        /// </summary>
        public string Extension { get; set; }
        /// <summary>
        /// Gets or sets the generated file name to be used for the file.
        /// </summary>
        public string GeneratedName { get; set; }
        /// <summary>
        /// Gets or sets the hash of the file.
        /// </summary>
        //public string Hash { get; set; }
        /// <summary>
        /// Gets or sets the width and height of the file.
        /// </summary>
        public System.Drawing.Size Dimensions { get; set; }
        /// <summary>
        /// Gets or sets the width and height of the thumbnail.
        /// </summary>
        public System.Drawing.Size ThumbnailDimensions { get; set; }
        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// Gets or sets the custom size of the image. This takes precident over <see cref="Size"/>.
        /// </summary>
        public string CustomSize { get; set; }
        /// <summary>
        /// Gets or sets whether the file is hidden or not.
        /// </summary>
        public bool Spoiled { get; set; }
        /// <summary>
        /// Gets or sets the status of the file, downloaded, errored, etc.
        /// </summary>
        //public FileDownloadStatus Status { get; set; } = FileDownloadStatus.Undownloaded;
    }
}