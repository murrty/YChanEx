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
        private ChanType _ChanID;
        private string _ThreadURL;
        private string _ThreadID;
        private string _ThreadBoard;

        private ThreadStatus _Status;
        private bool _ThreadInfoSet;

        private DateTime _LastModified;
        private string _LastThreadHTML;
        private CookieContainer _ThreadCookieContainer;

        private string _BoardName;
        private bool _RetrievedBoardName;
        private string _ThreadName;
        private bool _RetrievedThreadName;

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
        /// Gets or sets the bool BoardName of the ThreadInfo, used to hold the name of the board for chans that allow custom boards.
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
        /// <para>Not implemented.</para>
        /// </summary>
        public string ThreadName {
            get { return _ThreadName; }
            set { _ThreadName = value; }
        }
        /// <summary>
        /// Gets or sets the RetrievedThreadName of the ThreadInfo, used to determine of the ThreadName was retrieved.
        /// <para>Not implemented.</para>
        /// </summary>
        public bool RetrievedThreadName {
            get { return _RetrievedThreadName; }
            set { _RetrievedThreadName = value; }
        }
    }

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
