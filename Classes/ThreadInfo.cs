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
        private ChanType _ChanID = ChanType.None;
        private string _ThreadURL = null;
        private string _ThreadID = null;
        private string _ThreadBoard = null;

        private ThreadStatus _Status = ThreadStatus.UnknownStatus;
        private bool _ThreadInfoSet = false;

        private DateTime _LastModified = default(DateTime);
        private string _LastThreadHTML = null;
        private CookieContainer _ThreadCookieContainer = null;

        private string _BoardName = null;
        private bool _RetrievedBoardName = false;
        private string _ThreadName = null;
        private bool _RetrievedThreadName = false;

        private int _ThreadImagesCount = 0;
        private int _DownloadedImagesCount = 0;
        private int _ExtraFilesImageCount = 0;
        private int _ThreadPostsCount = 0;
        private bool _DownloadingFiles = false;
        private bool _FileWas404 = false;
        private int _RetryCountFor404 = 0;

        /// <summary>
        /// Gets or sets the ChanType Chan of the ThreadInfo, used to determine which *chan is going to be downloaded.
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ChanType Chan {
            get { return _ChanID; }
            set { _ChanID = value; }
        }

        /// <summary>
        /// Gets or sets the string ThreadURL of the ThreadInfo, used to break out the Board and ID for parsing.
        /// </summary>
        public string ThreadURL {
            get { return _ThreadURL; }
            set { _ThreadURL = value; }
        }
        /// <summary>
        /// Gets or sets the string ThreadID of the ThreadInfo, used to determine the specific thread.
        /// </summary>
        public string ThreadID {
            get { return _ThreadID; }
            set { _ThreadID = value; }
        }
        /// <summary>
        /// Gets or sets the string ThreadBoard of the ThreadInfo, used to determine the specific board.
        /// </summary>
        public string ThreadBoard {
            get { return _ThreadBoard; }
            set { _ThreadBoard = value; }
        }

        /// <summary>
        /// Gets or sets the ThreadStatus Status of the ThreadInfo, used to determine what the thread is up to.
        /// <seealso cref="ThreadStatus"/>
        /// </summary>
        public ThreadStatus Status {
            get { return _Status; }
            set { _Status = value; }
        }
        /// <summary>
        /// Gets or sets the bool ThreadInfoSet of the ThreadInfo, used to determine if the ThreadInfo has been set.
        /// </summary>
        public bool ThreadInfoSet {
            get { return _ThreadInfoSet; }
            set { _ThreadInfoSet = value; }
        }

        /// <summary>
        /// Gets or sets the DateTime LastModified of the ThreadInfo, used to determine when the thread was last updated (if supported).
        /// <para>Uses "If-Modified-Since" header on HttpWebRequests to prevent overload.</para>
        /// <para>All download logic includes this, but only a few actually make use of it.</para>
        /// </summary>
        public DateTime LastModified {
            get { return _LastModified; }
            set { _LastModified = value; }
        }
        /// <summary>
        /// Gets or sets the string LastThreadHTML of the ThreadInfo, used to determine if the thread has changed since last scan.
        /// <para>This is used on the threads that do not make use of LastModified by comparing old HTML to new HTML.</para>
        /// <para>It'd be easier if they just supported If-Last-Modified, but here we are.</para>
        /// </summary>
        public string LastThreadHTML {
            get { return _LastThreadHTML; }
            set { _LastThreadHTML = value; }
        }
        /// <summary>
        /// Gets or sets the Cookie ThreadCookie of the ThreadInfo, used to connect to sites that require a cookie to access.
        /// <para>Currently, only fchan requires this.</para>
        /// </summary>
        public CookieContainer ThreadCookie {
            get { return _ThreadCookieContainer; }
            set { _ThreadCookieContainer = value; }
        }

        /// <summary>
        /// Gets or sets the string BoardName of the ThreadInfo, used to hold the name of the board for chans that allow custom boards.
        /// <para>So far, it's 8chan and 8kun.</para>
        /// </summary>
        public string BoardName {
            get { return _BoardName; }
            set { _BoardName = value; }
        }
        /// <summary>
        /// Gets or sets the RetrievedBoardName of the ThreadInfo, used to determine if the BoardName was retrieved.
        /// <para>Prevents unneccessary parsing.</para>
        /// </summary>
        public bool RetrievedBoardName {
            get { return _RetrievedBoardName; }
            set { _RetrievedBoardName = value; }
        }
        /// <summary>
        /// Gets or sets the ThreadName of the ThreadInfo, used to hold the name of the thread.
        /// <para>Used for easier thread identification.</para>
        /// </summary>
        public string ThreadName {
            get { return _ThreadName; }
            set { _ThreadName = value; }
        }
        /// <summary>
        /// Gets or sets the RetrievedThreadName of the ThreadInfo, used to determine of the ThreadName was retrieved.
        /// <para>Used for easier thread identification.</para>
        /// </summary>
        public bool RetrievedThreadName {
            get { return _RetrievedThreadName; }
            set { _RetrievedThreadName = value; }
        }

        /// <summary>
        /// Gets or sets the int ThreadImagesCount of the thread, used to count the images in the thread.
        /// <para>Used by all chans.</para>
        /// </summary>
        public int ThreadImagesCount {
            get { return _ThreadImagesCount; }
            set { _ThreadImagesCount = value; }
        }
        /// <summary>
        /// Gets or sets the int DownloadImagesCount of the thread, used the count the downloaded images.
        /// <para>Used by all chans.</para>
        /// </summary>
        public int DownloadedImagesCount {
            get { return _DownloadedImagesCount; }
            set { _DownloadedImagesCount = value; }
        }
        /// <summary>
        /// Gets or sets the ExtraFilesImageCount of the thread, used to count the extra images in the thread.
        /// <para>Used by 8kun.</para>
        /// </summary>
        public int ExtraFilesImageCount {
            get { return _ExtraFilesImageCount; }
            set { _ExtraFilesImageCount = value; }
        }
        /// <summary>
        /// Gets or sets the int ThreadPostsCount of the thread, used to count the posts parsed.
        /// <para>Used by 8chan and 8kun.</para>
        /// </summary>
        public int ThreadPostsCount {
            get { return _ThreadPostsCount; }
            set { _ThreadPostsCount = value; }
        }
        /// <summary>
        /// Gets or sets bool DownloadingFiles of the thread, to determine if a thread is currently downloading images.
        /// <para>Used by all chans.</para>
        /// </summary>
        public bool DownloadingFiles {
            get { return _DownloadingFiles; }
            set { _DownloadingFiles = value; }
        }
        /// <summary>
        /// Gets or sets the bool FileWas404 to determine if the file should be attempted to be redownloaded.
        /// <para>Used by all chans.</para>
        /// </summary>
        public bool FileWas404 {
            get { return _FileWas404; }
            set { _FileWas404 = value; }
        }
        /// <summary>
        /// Gets or sets the int RetryCountFor404 to keep track of the redownload attemps.
        /// <para>Used by all chans.</para>
        /// </summary>
        public int RetryCountFor404 {
            get { return _RetryCountFor404; }
            set { _RetryCountFor404 = value; }
        }
    }

    /// <summary>
    /// Skeleton of the threads that have been saved by the application.
    /// </summary>
    public sealed class SavedThreadInfo {
        private string _ThreadURL;
        private ThreadStatus _Status;
        private string _ThreadName;
        private bool _CustomName;

        public string ThreadURL {
            get { return _ThreadURL; }
            set { _ThreadURL = value; }
        }
        public ThreadStatus Status {
            get { return _Status; }
            set { _Status = value; }
        }

        public string ThreadName {
            get { return _ThreadName; }
            set { _ThreadName = value; }
        }
        public bool CustomName {
            get { return _CustomName; }
            set { _CustomName = value; }
        }
    }
}
