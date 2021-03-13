using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YChanEx {
    /// <summary>
    /// Skeleton of thread downloads, used as a maintainer of thread information such as thread URL, ID, board, status.
    /// </summary>
    public sealed class ThreadInfo {
        protected ChanType _ChanID;
        protected string _ThreadURL;
        protected string _ThreadID;
        protected string _ThreadBoard;
        protected ThreadStatus _Status;

        public ChanType Chan {
            get { return _ChanID; }
            set { _ChanID = value; }
        }
        public string ThreadURL {
            get { return _ThreadURL; }
            set { _ThreadURL = value; }
        }
        public string ThreadID {
            get { return _ThreadID; }
            set { _ThreadID = value; }
        }
        public string ThreadBoard {
            get { return _ThreadBoard; }
            set { _ThreadBoard = value; }
        }

        public ThreadStatus Status {
            get { return _Status; }
            set { _Status = value; }
        }
    }
}
