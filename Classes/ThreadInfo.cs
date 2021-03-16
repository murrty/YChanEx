using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YChanEx {

    /// <summary>
    /// Skeleton of thread downloads, used as a maintainer of thread information such as thread URL, ID, board, status.
    /// <para>This contains all private information of the thread to reduce clutter on the main class.</para>
    /// </summary>
    public sealed class ThreadInfo {

        /// <summary>
        /// The chan type that will be parsed.
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ChanType Chan = ChanType.None;
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
        /// The list of file ids displayed on the form.
        /// </summary>
        public List<string> FileIDs = new List<string>();
        /// <summary>
        /// The list of file extensions displayed on the form.
        /// </summary>
        public List<string> FileExtensions = new List<string>();
        /// <summary>
        /// The list of original file names displayed on the form.
        /// </summary>
        public List<string> FileOriginalNames = new List<string>();
        /// <summary>
        /// The list of File Hashes displayed on the form.
        /// </summary>
        public List<string> FileHashes = new List<string>();

        /// <summary>
        /// The list of image files that will be downloaded.
        /// </summary>
        public List<string> ImageFiles = new List<string>();
        /// <summary>
        /// The list of thumbnail files that will be downloaded.
        /// </summary>
        public List<string> ThumbnailFiles = new List<string>();
        /// <summary>
        /// The list of file names that will be set to the file that gets downloaded.
        /// </summary>
        public List<string> FileNames = new List<string>();
        /// <summary>
        /// The list of thumbnail file names that will be set to the file that gets downloaded.
        /// </summary>
        public List<string> ThumbnailNames = new List<string>();
        /// <summary>
        /// The list of files that are duplicated in a thread.
        /// <para>If a file exists with the name, it'll be added here.</para>
        /// </summary>
        public List<string> FileNamesDupes = new List<string>();
        /// <summary>
        /// The list of the count of duped files.
        /// <para>If a file exists with the name, the amount that appears will be added here.</para>
        /// </summary>
        public List<int> FileNamesDupesCount = new List<int>();

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
        /// Counts the extra files in a post.
        /// <para>Used by 8kun.</para>
        /// </summary>
        public int ExtraFilesImageCount = 0;
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
        /// The status of the thread, used to determine what's happening.
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ThreadStatus Status = ThreadStatus.UnknownStatus;
        /// <summary>
        /// Determines if the ThreadInfo has been set.
        /// </summary>
        public bool ThreadInfoSet = false;

        /// <summary>
        /// The last modified date, used as a header when trying to download json or html information.
        /// <para>Uses "If-Modified-Since" header on HttpWebRequests to prevent overload.</para>
        /// <para>All download logic includes this, but only a few actually make use of it.</para>
        /// </summary>
        public DateTime LastModified = default(DateTime);
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
        /// The board name of the thread if not supported in <seealso cref="BoardTitles"/>.
        /// <para>So far, it's 8chan and 8kun.</para>
        /// </summary>
        public string BoardName = null;
        /// <summary>
        /// Determines if the board name has been retrieved.
        /// <para>Prevents unneccessary parsing.</para>
        /// </summary>
        public bool RetrievedBoardName = false;
        /// <summary>
        /// The name of the thread, given by OP.
        /// <para>Used for easier thread identification.</para>
        /// </summary>
        public string ThreadName = null;
        /// <summary>
        /// Determines if the thread name has been retrieved.
        /// <para>Used for easier thread identification.</para>
        /// </summary>
        public bool RetrievedThreadName = false;
        /// <summary>
        /// Determines if a custom name was set.
        /// </summary>
        public bool SetCustomName = false;
        /// <summary>
        /// The user-set custom name.
        /// </summary>
        public string CustomName = null;

    }

    /// <summary>
    /// Skeleton of the threads that have been saved by the application.
    /// </summary>
    public sealed class SavedThreadInfo {
        public string ThreadURL = null;
        public ThreadStatus Status = ThreadStatus.UnknownStatus;
        public bool RetrievedThreadName = false;
        public string ThreadName = null;
        public bool SetCustomName = false;
        public string CustomName = null;
    }
}
