using System;
using System.Collections.Generic;
using System.Net;

namespace YChanEx {

    /// <summary>
    /// Enumeration of file download status, that determines the current state of the file.
    /// </summary>
    public enum FileDownloadStatus {
        /// <summary>
        /// No attempt to download the file has occurred yet.
        /// <para>A download attempt will occur.</para>
        /// </summary>
        Undownloaded,
        /// <summary>
        /// The file successfully downloaded.
        /// <para>No download attempt will occur for this file again.</para>
        /// </summary>
        Downloaded,
        /// <summary>
        /// The file was given a 404, and is assumed to be deleted.
        /// <para>No download attempt will occur for this file again.</para>
        /// </summary>
        Error404,
        /// <summary>
        /// The file was not able to be downloaded.
        /// <para>A download attempt will occur.</para>
        /// </summary>
        Error
    }

    /// <summary>
    /// Skeleton of thread downloads, used as a maintainer of thread information such as thread URL, ID, board, status.
    /// <para>This contains all private information of the thread to reduce clutter on the main class.</para>
    /// </summary>
    public sealed class ThreadInfo {

        /// <summary>
        /// The chan type that will be parsed.
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ChanType Chan = ChanType.Unsupported;
        /// <summary>
        /// The URL of the thread that will be downloaded.
        /// </summary>
        public string ThreadURL = null;
        /// <summary>
        /// The ID of the thread being downloaded.
        /// </summary>
        public string ThreadID = null;
        /// <summary>
        /// The board that the thread that's being downloaded is on.
        /// </summary>
        public string ThreadBoard = null;
        /// <summary>
        /// The path that the thread will be downloaded to.
        /// </summary>
        public string DownloadPath = null;
        /// <summary>
        /// The HTML of the thread, custom build for YOUR CONVENIENCE.
        /// </summary>
        public string ThreadHTML = null;

        /// <summary>
        /// The list of post IDs that have been parsed.
        /// </summary>
        public List<string> ParsedPostIDs = new();
        /// <summary>
        /// The list of file IDs displayed on the form.
        /// </summary>
        public List<string> FileIDs = new();
        /// <summary>
        /// Contains the post ID of the image. Used for tracking the post in the parsed posts list.
        /// </summary>
        public List<string> ImagePostIDs = new();
        /// <summary>
        /// The list of file extensions displayed on the form.
        /// </summary>
        public List<string> FileExtensions = new();
        /// <summary>
        /// The list of original file names displayed on the form.
        /// </summary>
        public List<string> FileOriginalNames = new();
        /// <summary>
        /// The list of File Hashes displayed on the form.
        /// </summary>
        public List<string> FileHashes = new();

        /// <summary>
        /// The list of image files that will be downloaded.
        /// </summary>
        public List<string> ImageFiles = new();
        /// <summary>
        /// The list of thumbnail files that will be downloaded.
        /// </summary>
        public List<string> ThumbnailFiles = new();
        /// <summary>
        /// The list of file names that will be set to the file that gets downloaded.
        /// </summary>
        public List<string> FileNames = new();
        /// <summary>
        /// The list of thumbnail file names that will be set to the file that gets downloaded.
        /// </summary>
        public List<string> ThumbnailNames = new();
        /// <summary>
        /// The list of files that are duplicated in a thread.
        /// <para>If a file exists with the name, it'll be added here.</para>
        /// </summary>
        public List<string> FileNamesDupes = new();
        /// <summary>
        /// The list of the count of duped files.
        /// <para>If a file exists with the name, the amount that appears will be added here.</para>
        /// </summary>
        public List<int> FileNamesDupesCount = new();
        /// <summary>
        /// The list of the <see cref="FileDownloadStatus"/> of the file.
        /// </summary>
        public List<FileDownloadStatus> FileStatus = new();

        /// <summary>
        /// Counts the images in the thread when they were added.
        /// <para>Used by all chans.</para>
        /// </summary>
        public int ThreadImagesCount = 0;
        /// <summary>
        /// Counts how many images were downloaded, for the form.
        /// <para>Used by all chans.</para>
        /// </summary>
        public int DownloadedImagesCount = 0;
        /// <summary>
        /// The amount of posts counted up to.
        /// <para>Used by 8chan and 8kun.</para>
        /// </summary>
        public int ThreadPostsCount = 0;
        /// <summary>
        /// Determines if a thread is currently downloading images. Used to set FileWas404 on download 404.
        /// <para>Used by all chans.</para>
        /// </summary>
        public bool DownloadingFiles = false;
        /// <summary>
        /// Determines if scanning should be skipped and the file should be attempted to be redownloaded.
        /// <para>Used by all chans.</para>
        /// </summary>
        public bool FileWas404 = false;
        /// <summary>
        /// Used to keep track of the redownload attemps.
        /// <para>Used by all chans.</para>
        /// </summary>
        public int RetryCountFor404 = 0;

        /// <summary>
        /// The countdown to the next scan.
        /// </summary>
        public int CountdownToNextScan = 0;
        /// <summary>
        /// The time that the "not modified" label should be hidden at.
        /// <para>It is determined by subracting 10 from CountdownToNextScan.</para>
        /// </summary>
        public int HideModifiedLabelAt = 0;

        /// <summary>
        /// The overall status of the thread. (Alive/404/Aborted/Archived).
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ThreadStatus OverallStatus = ThreadStatus.NoStatusSet;
        /// <summary>
        /// The status of the thread, used to determine what's happening in the thread.
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ThreadStatus CurrentActivity = ThreadStatus.NoStatusSet;
        /// <summary>
        /// Determines if the ThreadInfo has been set.
        /// </summary>
        public bool ThreadInfoSet = false;

        /// <summary>
        /// The last modified date, used as a header when trying to download json or html information.
        /// <para>Uses "If-Modified-Since" header on HttpWebRequests to prevent overload.</para>
        /// <para>All download logic includes this, but only a few actually make use of it.</para>
        /// </summary>
        public DateTime LastModified = default;
        /// <summary>
        /// The HTML source of the thread that has been scanned.
        /// <para>This is used on the threads that do not make use of LastModified by comparing old HTML to new HTML.</para>
        /// <para>It'd be easier if they just supported If-Last-Modified, but here we are.</para>
        /// </summary>
        public string LastThreadHTML = null;
        /// <summary>
        /// The cookie container that contains cookies to connect to websites that require them.
        /// <para>Currently, only fchan requires this.</para>
        /// </summary>
        public CookieContainer ThreadCookieContainer = null;
        /// <summary>
        /// Determines if the thread is archived.
        /// </summary>
        public bool ThreadArchived = false;

        /// <summary>
        /// The board name of the thread retrieved from <seealso cref="BoardTitles"/>.
        /// <para>Chans that support user-made boards requires the name to be parsed from HTML.</para>
        /// </summary>
        public string BoardName = null;
        /// <summary>
        /// Determines if the board name has been retrieved, either from HTML or  <seealso cref="BoardTitles"/>.
        /// <para>Prevents unneccessary parsing.</para>
        /// </summary>
        public bool RetrievedBoardName = false;
        /// <summary>
        /// The name of the thread, given by OP of the Thread.
        /// <para>Used for easier thread identification.</para>
        /// </summary>
        public string ThreadName = null;
        /// <summary>
        /// Determines if the thread name has been retrieved from the current thread, either through API or HTML parsing.
        /// <para>Used for easier thread identification.</para>
        /// </summary>
        public bool RetrievedThreadName = false;
        /// <summary>
        /// Determines if a custom name was set for the thread.
        /// </summary>
        public bool SetCustomName = false;
        /// <summary>
        /// The user-set name for the thread. Only appears in the main form listview, and the thread form text.
        /// </summary>
        public string CustomName = null;
        /// <summary>
        /// Determines if the thread name was set into the HTML that will be saved.
        /// </summary>
        public bool HtmlThreadNameSet = false;

        public ThreadInfo(SavedThreadInfo SavedInfo, ChanType NewType) {
            Chan = NewType;
            ThreadURL = SavedInfo.ThreadURL;
            RetrievedThreadName = SavedInfo.RetrievedThreadName;
            ThreadName = SavedInfo.ThreadName;
            SetCustomName = SavedInfo.SetCustomName;
            CustomName = SavedInfo.CustomName;
            OverallStatus = ThreadStatus.ThreadIsAlive;
        }

        public ThreadInfo() { }

    }

    /// <summary>
    /// Skeleton of the threads that have been saved by the application.
    /// </summary>
    public sealed class SavedThreadInfo {
        public string ThreadURL = null;
        public ThreadStatus Status = ThreadStatus.NoStatusSet;
        public bool RetrievedThreadName = false;
        public string ThreadName = null;
        public bool SetCustomName = false;
        public string CustomName = null;
    }

    /// <summary>
    /// Contains information about the post.
    /// <para>Only used if SaveHTML is enabled.</para>
    /// </summary>
    public sealed class PostInfo {
        /// <summary>
        /// The ID of the post.
        /// </summary>
        public string PostID = null;
        /// <summary>
        /// The date of the post.
        /// </summary>
        public string PostDate = null;
        /// <summary>
        /// The displayed name of the poser.
        /// </summary>
        public string PosterName = null;
        /// <summary>
        /// The subject of the post (Usually the OP)
        /// </summary>
        public string PostSubject = null;
        /// <summary>
        /// The comment on the post.
        /// </summary>
        public string PostComment = null;
        /// <summary>
        /// The original file name (if the post contains a image)
        /// </summary>
        public string PostOriginalName = null;
        /// <summary>
        /// The extension of the file (if the post contains a image)
        /// </summary>
        public string PostFileExtension = null;
        /// <summary>
        /// The width of the file (if the post contains a image)
        /// </summary>
        public string PostWidth = null;
        /// <summary>
        /// The height of the file (if the post contains a image)
        /// </summary>
        public string PostHeight = null;
        /// <summary>
        /// The width of the file thumbnail (if the post contains a image)
        /// </summary>
        public string PostThumbnailWidth = null;
        /// <summary>
        /// The Height of the file thumbnail (if the post contains a image)
        /// </summary>
        public string PostThumbnailHeight = null;
        /// <summary>
        /// The unique file ID of the file (if the post contains a image)
        /// </summary>
        public string PostFileID = null;
        /// <summary>
        /// The file size (in bytes) of the file (if the post contains a image)
        /// </summary>
        public string PostFileSize = null;

        /// <summary>
        /// Determines if the post contains a file.
        /// </summary>
        public bool PostContainsFile = false;
        /// <summary>
        /// The file name that is saved through the application.
        /// <para>This may differ if save original file name and/or prevent duplicates is enabled.</para>
        /// </summary>
        public string PostOutputFileName = null;
    }

}
