using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Html = SoftCircuits.HtmlMonkey;

namespace YChanEx {

    /// <summary>
    /// The form that thread downloads will be relegated to.
    /// </summary>
    public partial class frmDownloader : Form {

        // download the images in the lists.
        // indexes are:
        // 0 = waiting
        // 1 = downloading
        // 2 = completed
        // 3 = error
        // 4 = 404

        #region Variables
        private const string DefaultEmptyFileName = "ychanex-emptyname";
        /// <summary>
        /// The IMainFormInterface to interface with the main form,
        /// used for updating the main form with this forms' threads' status, or name.
        /// </summary>
        private readonly IMainFom MainFormInstance;     // all, the instance of the main for for modifying it
                                                        // when anything major changes in the download form.

        /// <summary>
        /// The ThreadInfo containing all information about this forms' thread.
        /// </summary>
        public ThreadInfo CurrentThread;                // all, the ThreadInfo relating to the current thread.
        public ThreadStatus LastStatus;                 // ???

        /// <summary>
        /// The thread for the thread parser/downloader
        /// </summary>
        private Thread DownloadThread;                  // all, the main download thread.
        /// <summary>
        /// The thread that's used to delay the rescanner.
        /// </summary>
        private Thread TimerIdle;                       // all, the timer idler for when the settings form is open.

        // Mostly-debug
        /// <summary>
        /// Pauses the file downloader 100ms between each file,
        /// used to prevent sending too many requests.
        /// </summary>
        private const bool PauseBetweenFiles = true;    // all, temp pauses between file downloads.
        #endregion

        #region Form Controls
        public frmDownloader(IMainFom MainForm) {
            InitializeComponent();
            MainFormInstance = MainForm;

            Debug.Print("Created download form");

            if (Program.DebugMode) {
                btnForce404.Enabled = true;
                btnForce404.Visible = true;
                btnPauseTimer.Enabled = true;
                btnPauseTimer.Visible = true;
            }
            lvImages.SmallImageList = Program.DownloadImages;

            this.Load += (s, e) => {
                if (Config.ValidPoint(Config.Settings.Saved.DownloadFormLocation)) {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = Config.Settings.Saved.DownloadFormLocation;
                }
                if (Config.ValidSize(Config.Settings.Saved.DownloadFormSize))
                    this.Size = Config.Settings.Saved.DownloadFormSize;
            };
        }
        private void frmDownloader_FormClosing(object sender, FormClosingEventArgs e) {
            Config.Settings.Saved.DownloadFormLocation = this.Location;
            Config.Settings.Saved.DownloadFormSize = this.Size;
            e.Cancel = true;
            this.Hide();
        }
        private void tmrScan_Tick(object sender, EventArgs e) {
            if (Program.SettingsOpen) {
                TimerIdle = new Thread(() => {
                    try {
                        while (Program.SettingsOpen) {
                            Thread.Sleep(1000);
                        }
                        if (this.IsHandleCreated) {
                            this.Invoke((MethodInvoker)delegate {
                                tmrScan.Start();
                            });
                        }
                    }
                    catch (ThreadAbortException) {
                        return;
                    }
                }) {
                    Name = "Idling timer for Settings"
                };
                TimerIdle.Start();
                tmrScan.Stop();
                return;
            }

            switch (CurrentThread.CountdownToNextScan) {
                case 0: {
                    tmrScan.Stop();
                    btnPauseTimer.Enabled = false;
                    ManageThread(ThreadEvent.StartDownload);
                } break;

                case 15: {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanningSoon;
                    MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                    btnPauseTimer.Enabled = true;
                    lbScanTimer.Text = CurrentThread.CountdownToNextScan.ToString();
                    CurrentThread.CountdownToNextScan--;
                } break;

                default: {
                    if (CurrentThread.CountdownToNextScan == CurrentThread.HideModifiedLabelAt) {
                        lbNotModified.Visible = false;
                    }
                    btnPauseTimer.Enabled = true;
                    lbScanTimer.Text = CurrentThread.CountdownToNextScan.ToString();
                    CurrentThread.CountdownToNextScan--;
                } break;
            }
        }
        private void lvImages_MouseDoubleClick(object sender, MouseEventArgs e) {
            for (int i = 0; i < lvImages.SelectedIndices.Count; i++) {
                if (File.Exists(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[i]])) {
                    Process.Start(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[i]]);
                }
            }
        }
        private void lvImages_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Return) {
                for (int i = 0; i < lvImages.SelectedIndices.Count; i++) {
                    if (File.Exists(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[i]])) {
                        Process.Start(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[i]]);
                    }
                }
                e.Handled = true;
            }
        }
        private void btnForce404_Click(object sender, EventArgs e) {
            if (Program.DebugMode) {
                tmrScan.Stop();
                if (DownloadThread != null && DownloadThread.IsAlive) {
                    DownloadThread.Abort();
                }

                CurrentThread.CurrentActivity = ThreadStatus.ThreadIs404;
                btnForce404.Enabled = false;
                ManageThread(ThreadEvent.AfterDownload);
            }
        }
        private void btnAbortRetry_Click(object sender, EventArgs e) {
            switch (CurrentThread.CurrentActivity) {
                case ThreadStatus.ThreadIs404:
                case ThreadStatus.ThreadIsAborted:
                case ThreadStatus.ThreadIsArchived:
                    ManageThread(ThreadEvent.RetryDownload);
                    break;
                default:
                    ManageThread(ThreadEvent.AbortDownload);
                    break;
            }
        }
        private void btnOpenFolder_Click(object sender, EventArgs e) {
            if (CurrentThread.Data.DownloadPath == null) { return; }

            if (Directory.Exists(CurrentThread.Data.DownloadPath)) {
                Process.Start(CurrentThread.Data.DownloadPath);
            }
        }
        private void btnClose_Click(object sender, EventArgs e) {
            this.Hide();
        }

        #region cmThreadActions
        private void mOpenThreadDownloadFolder_Click(object sender, EventArgs e) {
            if (Directory.Exists(CurrentThread.Data.DownloadPath)) {
                Process.Start(CurrentThread.Data.DownloadPath);
            }
        }

        private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
            if (CurrentThread.Data.ThreadURL != null) {
                Process.Start(CurrentThread.Data.ThreadURL);
            }
        }

        private void mCopyThreadID_Click(object sender, EventArgs e) {
            if (CurrentThread.Data.ThreadID != null) {
                Clipboard.SetText(CurrentThread.Data.ThreadID);
            }
        }

        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (CurrentThread.Data.ThreadURL != null) {
                Clipboard.SetText(CurrentThread.Data.ThreadURL);
            }
        }

        private void mCopyThreadApiUrl_Click(object sender, EventArgs e) {
            if (CurrentThread.Data.ThreadURL != null) {
                switch (CurrentThread.Chan) {
                    case ChanType.FourChan:
                    case ChanType.FourTwentyChan:
                    case ChanType.EightChan:
                    case ChanType.EightKun:
                        Clipboard.SetText(Networking.GetAPILink(CurrentThread));
                        break;

                    case ChanType.SevenChan:
                    case ChanType.fchan:
                    case ChanType.u18chan:
                        Clipboard.SetText(CurrentThread.Data.ThreadURL);
                        break;
                }
            }
        }
        #endregion

        #region cmPosts
        private void mOpenImages_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            for (int i = 0; i < lvImages.SelectedIndices.Count; i++) {
                if (File.Exists(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[i]])) {
                    Process.Start(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[i]]);
                }
            }
        }
        private void mRemoveImages_Click(object sender, EventArgs e) {
            for (int Post = lvImages.SelectedIndices.Count - 1; Post >= 0; Post--) {
                RemoveFileFromSystem(lvImages.SelectedIndices[Post]);
            }
        }
        private void mRemoveImagesFromSystem_Click(object sender, EventArgs e) {
            // Only for removing items.
            for (int Post = lvImages.SelectedIndices.Count - 1; Post >= 0; Post--) {
                RemoveFileFromSystem(lvImages.SelectedIndices[Post]);
            }
        }
        private void mRemoveImagesFromThread_Click(object sender, EventArgs e) {
            // Only for removing items.
            for (int Post = lvImages.SelectedIndices.Count - 1; Post >= 0; Post--) {
                RemoveFileFromThread(lvImages.SelectedIndices[Post], CurrentThread.Data.ParsedPostIDs.IndexOf(CurrentThread.Data.ImagePostIDs[Post]));
            }
        }
        private void mRemoveImagesFromBoth_Click(object sender, EventArgs e) {
            // Only for removing items.
            for (int Post = lvImages.SelectedIndices.Count - 1; Post >= 0; Post--) {
                RemoveFileFromSystem(lvImages.SelectedIndices[Post]);
                RemoveFileFromThread(lvImages.SelectedIndices[Post], CurrentThread.Data.ParsedPostIDs.IndexOf(CurrentThread.Data.ImagePostIDs[Post]));
            }
        }

        private void mCopyPostIDs_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            string PostIDBuffer = string.Empty;
            for (int Post = 0; Post < lvImages.SelectedIndices.Count; Post++) {
                PostIDBuffer += CurrentThread.Data.ImagePostIDs[Post] + "\r\n";
            }
        }
        private void mCopyImageIDNames_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            string PostNameIDBuffer = string.Empty;
            for (int Post = 0; Post < lvImages.SelectedIndices.Count; Post++) {
                PostNameIDBuffer += CurrentThread.Data.FileIDs[Post] + "\r\n";
            }
        }
        private void mCopyOriginalFileNames_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            string PostOriginalFileNameBuffer = string.Empty;
            for (int Post = 0; Post < lvImages.SelectedIndices.Count; Post++) {
                PostOriginalFileNameBuffer += CurrentThread.Data.FileOriginalNames[Post] + "\r\n";
            }
        }
        private void mCopyDupeCheckedOriginalFileNames_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            string PostDupeCheckedOriginalFileNameBuffer = string.Empty;
            for (int Post = 0; Post < lvImages.SelectedIndices.Count; Post++) {
                int DupeIndex = CurrentThread.Data.FileNamesDupes.IndexOf(CurrentThread.Data.FileOriginalNames[Post]);
                PostDupeCheckedOriginalFileNameBuffer += CurrentThread.Data.FileNamesDupes[DupeIndex] + "\r\n";
            }
        }
        private void mCopyFileHashes_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            string PostFileHashBuffer = string.Empty;
            for (int Post = 0; Post < lvImages.SelectedIndices.Count; Post++) {
                PostFileHashBuffer += CurrentThread.Data.FileHashes[Post] + "\r\n";
            }
        }

        private void mShowInExplorer_Click(object sender, EventArgs e) {
            // Only for NOT removing items.
            for (int Post = 0; Post < lvImages.SelectedIndices.Count; Post++) {
                Process.Start("explorer.exe", "/select, \"" + CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[lvImages.SelectedIndices[Post]] + "\"");
            }
        }

        private void cmPosts_Popup(object sender, EventArgs e) {
            if (lvImages.SelectedIndices.Count > 0) {
                mOpenImages.Enabled = true;
                mRemoveImages.Enabled = true;
                //mRemoveImagesFromSystem.Enabled = true;
                //mRemoveImagesFromThread.Enabled = true;
                //mRemoveImagesFromBoth.Enabled = true;
                mCopyPostIDs.Enabled = true;
                mCopyImageIDNames.Enabled = true;
                mCopyOriginalFileNames.Enabled = true;
                mCopyDupeCheckedOriginalFileNames.Enabled = true;
                mCopyFileHashes.Enabled = true;
                mShowInExplorer.Enabled = true;
            }
            else {
                mOpenImages.Enabled = false;
                mRemoveImages.Enabled = false;
                //mRemoveImagesFromSystem.Enabled = false;
                //mRemoveImagesFromThread.Enabled = false;
                //mRemoveImagesFromBoth.Enabled = false;
                mCopyPostIDs.Enabled = false;
                mCopyImageIDNames.Enabled = false;
                mCopyOriginalFileNames.Enabled = false;
                mCopyDupeCheckedOriginalFileNames.Enabled = false;
                mCopyFileHashes.Enabled = false;
                mShowInExplorer.Enabled = false;
            }
        }
        #endregion

        #region Debug
        private void btnPauseTimer_Click(object sender, EventArgs e) {
            if (tmrScan.Enabled) {
                tmrScan.Stop();
                btnPauseTimer.Text = "Start Tmr";
            }
            else {
                tmrScan.Start();
                btnPauseTimer.Text = "Pause Tmr";
            }
        }
        #endregion

        #endregion

        #region Custom Thread Methods

        public void ManageThread(ThreadEvent Event) {
            switch (Event) {

                case ThreadEvent.ParseForInfo: {
                    Debug.Print("ParseThreadForInfo called");
                    string[] URLSplit = CurrentThread.Data.ThreadURL.Split('/');
                    string ThreadName = "Unparsed";

                    switch (CurrentThread.Chan) {

                        #region 4chan
                        case ChanType.FourChan: {
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^3];
                            CurrentThread.Data.ThreadID = URLSplit[^1].Split('#')[0];
                            CurrentThread.Data.BoardName = Chans.GetFullBoardName(CurrentThread);

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"4chan thread - {CurrentThread.Data.BoardName} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"4chan thread - {CurrentThread.Data.BoardName} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"4chan thread - {CurrentThread.Data.BoardName} - {CurrentThread.Data.ThreadID}";
                            }

                            CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                            CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\4chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                        #region 420chan
                        case ChanType.FourTwentyChan: {
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^4];
                            CurrentThread.Data.ThreadID = URLSplit[^2].Split('#')[0];

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"420chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"420chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"420chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadID}";
                            }

                            CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                            CurrentThread.Data.DownloadPath =  $"{Config.Settings.Downloads.DownloadPath}\\420chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                        #region 7chan
                        case ChanType.SevenChan: {
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^3];
                            CurrentThread.Data.ThreadID = URLSplit[^1].Split('#')[0].Replace(".html", "");

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"7chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"7chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"7chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadID}";
                            }

                            CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                            CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\7chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                        #region 8chan
                        case ChanType.EightChan: {
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^3];
                            CurrentThread.Data.ThreadID = URLSplit[^1].Split('#')[0].Replace(".html", "").Replace(".json", "");

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"8chan thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"8chan thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"8chan thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadID}";
                            }

                            CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\8chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                        #region 8kun
                        case ChanType.EightKun: {
                            if (Chans.StupidFuckingBoard(ChanType.EightKun, CurrentThread.Data.ThreadURL)) {
                                MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadIs404);
                                this.Dispose();
                                return;
                            }
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^3];
                            CurrentThread.Data.ThreadID = URLSplit[^1].Split('#')[0].Replace(".html", "").Replace(".json", "");

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"8kun thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"8kun thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"8kun thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadID}";
                            }

                            CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\8kun\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                        #region fchan
                        case ChanType.fchan: {
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^3];
                            CurrentThread.Data.ThreadID = URLSplit[^1].Split('#')[0].Replace(".html", "");

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = string.Format("fchan thread - {0} - {1}", Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.CustomThreadName);
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = string.Format("fchan thread - {0} - {1}", Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                            }
                            else {
                                ThreadName = string.Format("fchan thread - {0} - {1}", Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                            }

                            CurrentThread.ThreadCookieContainer = new CookieContainer();
                            CurrentThread.ThreadCookieContainer.Add(new Cookie("disclaimer", "seen") { Domain = "fchan.us" });

                            CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                            CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\fchan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                        #region u18chan
                        case ChanType.u18chan: {
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.Data.BoardName = CurrentThread.Data.ThreadBoard = URLSplit[^3];
                            CurrentThread.Data.ThreadID = URLSplit[^1].Split('#')[0];

                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"u18chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"u18chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"u18chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadID}";
                            }

                            CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                            CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\u18chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                        } break;
                        #endregion

                    }

                    this.Text = ThreadName;
                    CurrentThread.UpdateJsonPath();

                    if (CurrentThread.Data.DownloadPath != null) {
                        btnOpenFolder.Enabled = true;
                    }

                } break;

                case ThreadEvent.StartDownload: {
                    switch (CurrentThread.Chan) {
                        case ChanType.FourChan: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\4chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\4chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            Set4chanThread();
                        } break;
                        case ChanType.FourTwentyChan: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\420chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\420chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            Set420chanThread();
                        } break;
                        case ChanType.SevenChan: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\7chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\7chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            Set7chanThread();
                        } break;
                        case ChanType.EightChan: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\8chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\8chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            Set8chanThread();
                        } break;
                        case ChanType.EightKun: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\8kun\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\8kun\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            Set8kunThread();
                        } break;
                        case ChanType.fchan: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\fchan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\fchan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            SetFchanThread();
                        } break;
                        case ChanType.u18chan: {
                            if (CurrentThread.Data.DownloadPath != $"{Config.Settings.Downloads.DownloadPath}\\u18chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}") {
                                CurrentThread.Data.DownloadPath = $"{Config.Settings.Downloads.DownloadPath}\\u18chan\\{CurrentThread.Data.ThreadBoard}\\{CurrentThread.Data.ThreadID}";
                            }
                            Setu18ChanThread();
                        }
                        break;

                        default: {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.NoStatusSet);
                            return;
                        }
                    }

                    if (CurrentThread.Data.DownloadPath != null) {
                        btnOpenFolder.Enabled = true;
                    }

                    CurrentThread.HideModifiedLabelAt = Config.Settings.Downloads.ScannerDelay - 10;
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;
                    MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadScanning);
                    lbScanTimer.Text = "scanning now...";
                    DownloadThread.Start();
                } break;

                case ThreadEvent.AfterDownload: {
                    switch (CurrentThread.CurrentActivity) {

                        case ThreadStatus.ThreadIsAborted: {
                            lbScanTimer.Text = "Aborted";
                            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                            this.Icon = Properties.Resources.YChanEx404;

                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                            btnAbortRetry.Text = "Retry";
                        } break;

                        case ThreadStatus.ThreadIs404: {
                            if (Config.Settings.Downloads.AutoRemoveDeadThreads) {
                                MainFormInstance.ThreadKilled(CurrentThread);
                                return;
                            }
                            else {
                                lbScanTimer.Text = "404'd";
                                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                                this.Icon = Properties.Resources.YChanEx404;

                                MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                                btnAbortRetry.Text = "Retry";
                            }
                        } break;

                        case ThreadStatus.ThreadFile404: {
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            CurrentThread.FileWas404 = true;
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                            CurrentThread.CountdownToNextScan = Config.Settings.Downloads.ScannerDelay - 1;
                            lvImages.Items[CurrentThread.Data.DownloadedImagesCount].ImageIndex = 3;
                            if (CurrentThread.RetryCountFor404 == 4) {
                                CurrentThread.RetryCountFor404 = 0;
                                CurrentThread.FileWas404 = true;
                                CurrentThread.Data.DownloadedImagesCount++;
                                lbScanTimer.Text = "File 404, skipping";
                            }
                            else {
                                CurrentThread.RetryCountFor404++;
                                lbScanTimer.Text = "File 404, retrying";
                            }
                            tmrScan.Start();
                        } break;

                        case ThreadStatus.ThreadIsArchived: {
                            if (Config.Settings.Downloads.AutoRemoveDeadThreads) {
                                MainFormInstance.ThreadKilled(CurrentThread);
                                return;
                            }
                            else {
                                lbScanTimer.Text = "Archived";
                                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                                this.Icon = Properties.Resources.YChanEx404;

                                MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                                btnAbortRetry.Text = "Rescan";
                                CurrentThread.ThreadModified = true;
                            }
                        } break;

                        case ThreadStatus.ThreadDownloading:
                        case ThreadStatus.Waiting:
                        case ThreadStatus.ThreadNotModified: {
                            lbNotModified.Visible = CurrentThread.CurrentActivity == ThreadStatus.ThreadNotModified;
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                            CurrentThread.CountdownToNextScan = Config.Settings.Downloads.ScannerDelay - 1;
                            lbScanTimer.Text = "soon (tm)";
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            tmrScan.Start();
                        } break;

                        case ThreadStatus.ThreadImproperlyDownloaded: {
                            lbScanTimer.Text = "Bad download";
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                            CurrentThread.CountdownToNextScan = Config.Settings.Downloads.ScannerDelay - 1;
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            tmrScan.Start();
                        } break;

                        case ThreadStatus.ThreadIsNotAllowed: {
                            // ??
                        } break;

                        case ThreadStatus.ThreadInfoNotSet: {
                            // ??
                        } break;

                    }
                } break;

                case ThreadEvent.AbortDownload: {
                    Debug.Print("AbortDownload called");
                    tmrScan.Stop();
                    if (DownloadThread != null && DownloadThread.IsAlive) {
                        DownloadThread.Abort();
                    }
                    this.Icon = Properties.Resources.YChanEx404;
                    lbScanTimer.Text = "Aborted";
                    lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);

                    btnAbortRetry.Text = "Retry";
                    lbNotModified.Visible = false;
                    if (Program.DebugMode) {
                        btnForce404.Enabled = false;
                    }
                } break;

                case ThreadEvent.RetryDownload: {
                    Debug.Print("RetryDownload called");
                    this.Icon = Properties.Resources.YChanEx;
                    lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadRetrying;
                    btnAbortRetry.Text = "Abort";
                    if (Program.DebugMode) {
                        btnForce404.Enabled = true;
                    }

                    MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, CurrentThread.CurrentActivity);
                    lbScanTimer.Text = "scanning now...";
                    btnAbortRetry.Text = "Abort";
                    tmrScan.Stop();
                    ManageThread(ThreadEvent.StartDownload);
                } break;

                case ThreadEvent.AbortForClosing: {
                    if (DownloadThread != null && DownloadThread.IsAlive) {
                        DownloadThread.Abort();
                    }
                } break;

                case ThreadEvent.ReloadThread: {
                    if (Config.Settings.Downloads.AutoRemoveDeadThreads && CurrentThread.CurrentActivity switch {
                        ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsArchived or _ when CurrentThread.Data.ThreadArchived => true,
                        _ => false
                    }) {
                        MainFormInstance.ThreadKilled(CurrentThread);
                        return;
                    }

                    string ThreadName = "Unparsed";

                    switch (CurrentThread.Chan) {
                        case ChanType.FourChan: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"4chan thread - {CurrentThread.Data.BoardName} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"4chan thread - {CurrentThread.Data.BoardName} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"4chan thread - {CurrentThread.Data.BoardName} - {CurrentThread.Data.ThreadID}";
                            }
                        } break;
                        case ChanType.FourTwentyChan: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"420chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"420chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"420chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadID}";
                            }
                        } break;
                        case ChanType.SevenChan: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"7chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"7chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"7chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadID}";
                            }
                        } break;
                        case ChanType.EightChan: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"8chan thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"8chan thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"8chan thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadID}";
                            }
                        } break;
                        case ChanType.EightKun: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"8kun thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"8kun thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"8kun thread - {CurrentThread.Data.ThreadBoard} - {CurrentThread.Data.ThreadID}";
                            }
                        } break;
                        case ChanType.fchan: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = string.Format("fchan thread - {0} - {1}", Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.CustomThreadName);
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = string.Format("fchan thread - {0} - {1}", Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                            }
                            else {
                                ThreadName = string.Format("fchan thread - {0} - {1}", Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                            }
                        } break;
                        case ChanType.u18chan: {
                            if (CurrentThread.Data.SetCustomName) {
                                ThreadName = $"u18chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.CustomThreadName}";
                            }
                            else if (CurrentThread.Data.RetrievedThreadName) {
                                ThreadName = $"u18chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadName}";
                            }
                            else {
                                ThreadName = $"u18chan thread - {Chans.GetFullBoardName(CurrentThread)} - {CurrentThread.Data.ThreadID}";
                            }
                        } break;

                        default: {
                            ManageThread(ThreadEvent.ParseForInfo);
                        } break;
                    }

                    this.Text = ThreadName;
                    CurrentThread.UpdateJsonPath();

                    if (CurrentThread.Data.DownloadPath != null)
                        btnOpenFolder.Enabled = true;

                    if (CurrentThread.Data.FileIDs.Count > 0) {
                        for (int i = 0; i < CurrentThread.Data.FileIDs.Count; i++) {
                            ListViewItem lvi = new();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = CurrentThread.Data.FileIDs[i];
                            lvi.SubItems[0].Text = CurrentThread.Data.FileIDs[i];
                            lvi.SubItems[1].Text = CurrentThread.Data.FileExtensions[i].Trim('.');
                            lvi.SubItems[2].Text = CurrentThread.Data.FileOriginalNames[i];
                            switch (CurrentThread.Chan) {
                                case ChanType.FourChan:
                                case ChanType.EightChan:
                                case ChanType.EightKun: {
                                    lvi.SubItems[3].Text = CurrentThread.Data.FileHashes[i];
                                }
                                break;
                            }
                            lvi.ImageIndex = CurrentThread.Data.FileStatus[i] switch {
                                FileDownloadStatus.Downloaded  =>
                                    File.Exists($"{CurrentThread.Data.DownloadPath}\\{CurrentThread.Data.FileNames[i]}") ? 5 : 6,
                                FileDownloadStatus.Error => 3,
                                FileDownloadStatus.FileNotFound => 4,
                                _ => 0
                            };
                            this.Invoke((MethodInvoker)delegate {
                                lvImages.Items.Add(lvi);
                            });
                        }
                        CurrentThread.ThreadHTML = HtmlControl.RebuildHTML(CurrentThread);
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbPostsParsed.Text = $"posts parsed: {CurrentThread.Data.ParsedPostIDs.Count}";
                        lbLastModified.Text = $"last modified: {CurrentThread.Data.LastModified}";
                    }
                    if (CurrentThread.Data.FileIDs.Count > 0) {
                    }
                } break;

            }
        }

        public void RemoveFileFromSystem(int FileIndex) {
            if (File.Exists(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[FileIndex])) {
                File.Delete(CurrentThread.Data.DownloadPath + "\\" + CurrentThread.Data.FileNames[FileIndex]);
            }
            if (File.Exists(CurrentThread.Data.DownloadPath + "\\thumb\\" + CurrentThread.Data.ThumbnailNames[FileIndex])) {
                File.Delete(CurrentThread.Data.DownloadPath + "\\thumb\\" + CurrentThread.Data.ThumbnailNames[FileIndex]);
            }
            CurrentThread.Data.DownloadedImagesCount--;
            lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
            lvImages.Items[FileIndex].ImageIndex = 0;
            CurrentThread.Data.FileStatus[FileIndex] = FileDownloadStatus.Undownloaded;
        }

        public void RemoveFileFromThread(int FileIndex, int PostIndex) {
            CurrentThread.Data.ThreadImagesCount--;
            if (CurrentThread.Data.FileNamesDupes.Contains(CurrentThread.Data.FileOriginalNames[FileIndex])) {
                int DupeIndex = CurrentThread.Data.FileNamesDupes.IndexOf(CurrentThread.Data.FileOriginalNames[FileIndex]);
                //CurrentThread.Data.FileNamesDupes.RemoveAt(DupeIndex);
                if (CurrentThread.Data.FileNamesDupesCount[DupeIndex] > 1) {
                    CurrentThread.Data.FileNamesDupesCount[DupeIndex]--;
                }
                else {
                    CurrentThread.Data.FileNamesDupesCount.RemoveAt(DupeIndex);
                }
            }
            CurrentThread.Data.FileIDs.RemoveAt(FileIndex);
            CurrentThread.Data.ImagePostIDs.RemoveAt(FileIndex);
            CurrentThread.Data.FileExtensions.RemoveAt(FileIndex);
            CurrentThread.Data.FileOriginalNames.RemoveAt(FileIndex);
            CurrentThread.Data.FileHashes.RemoveAt(FileIndex);
            CurrentThread.Data.ImageFiles.RemoveAt(FileIndex);
            CurrentThread.Data.ThumbnailFiles.RemoveAt(FileIndex);
            CurrentThread.Data.FileNames.RemoveAt(FileIndex);
            CurrentThread.Data.ThumbnailNames.RemoveAt(FileIndex);
            CurrentThread.Data.FileStatus.RemoveAt(FileIndex);

            CurrentThread.Data.ThreadPostsCount--;
            CurrentThread.Data.ParsedPostIDs.RemoveAt(PostIndex);
            lvImages.Items.RemoveAt(FileIndex);

            lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
            lbPostsParsed.Text = "posts parsed: " + CurrentThread.Data.ParsedPostIDs.Count.ToString();
        }

        public void UpdateThreadName(bool ApplyToMainForm = false) {
            string ThreadNameBuffer = "unknown thread - {0} - {1}";
            switch (CurrentThread.Chan) {
                case ChanType.FourChan:
                    ThreadNameBuffer = "4chan thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                    }
                    break;
                case ChanType.FourTwentyChan:
                    ThreadNameBuffer = "420chan thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                    }
                    break;
                case ChanType.SevenChan:
                    ThreadNameBuffer = "7chan thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                    }
                    break;
                case ChanType.EightChan:
                    ThreadNameBuffer = "8chan thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, CurrentThread.Data.ThreadBoard, CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, CurrentThread.Data.ThreadBoard, CurrentThread.Data.ThreadID);
                    }
                    break;
                case ChanType.EightKun:
                    ThreadNameBuffer = "8kun thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, CurrentThread.Data.ThreadBoard, CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, CurrentThread.Data.ThreadBoard, CurrentThread.Data.ThreadID);
                    }
                    break;
                case ChanType.fchan:
                    ThreadNameBuffer = "fchan thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                    }
                    break;
                case ChanType.u18chan:
                    ThreadNameBuffer = "u18chan thread - {0} - {1}";
                    if (CurrentThread.Data.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.Data.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(CurrentThread), CurrentThread.Data.ThreadID);
                    }
                    break;
                default:
                    this.Text = string.Format(ThreadNameBuffer, CurrentThread.Data.ThreadBoard, CurrentThread.Data.ThreadID);
                    return;
            }
        }

        #endregion

        #region Shared Chan Logic

        /// <summary>
        /// Retrieve the HTML of a given ThreadURL for parsing, or aesthetic.
        /// </summary>
        /// <param name="URL">The URL of the page to download the HTML source.</param>
        /// <param name="HTML">The out string of the HTML.</param>
        /// <param name="SkipModifiedSince">Use WebClient to recieve HTML instead of HttpWebRequest.</param>
        /// <returns>The HTML of a given Thread</returns>
        private bool GetThreadHTML(string URL, out string HTML, bool SkipModifiedSince = false) {
            try {
                HTML = string.Empty;
                using murrty.classcontrols.ExtendedWebClient wc = new(CurrentThread.ThreadCookieContainer, murrty.classcontrols.HttpMethod.GET);
                if (!SkipModifiedSince) {
                    wc.IfModifiedSince = CurrentThread.Data.LastModified;
                }
                wc.UserAgent = Config.Settings.Advanced.UserAgent;
                HTML = wc.DownloadString(URL);
                return true;
            }
            catch { throw; }
        }

        /// <summary>
        /// Retrieves the JSON of a the thread.
        /// </summary>
        /// <param name="URL">The URL of the api page that will be downloaded.</param>
        /// <param name="JsonString">The output string of the entire JSON that was downloaded.</param>
        /// <returns>True if the json is not null, empty, or whitespace; otherwise, false.</returns>
        private bool GetThreadJSON(string URL, out string JsonString) {
            JsonString = null;
            try {
                using murrty.classcontrols.ExtendedWebClient wc = new(CurrentThread.ThreadCookieContainer, murrty.classcontrols.HttpMethod.GET, CurrentThread.Data.LastModified);
                wc.UserAgent = Config.Settings.Advanced.UserAgent;
                JsonString = wc.DownloadString(URL);
                CurrentThread.Data.LastModified = wc.ResourceLastModified;
                return !string.IsNullOrWhiteSpace(JsonString);
            }
            catch { throw; }
        }

        /// <summary>
        /// Downloads all files in the <see cref="CurrentThread"/>.
        /// <para>Requires <see cref="ThreadInfo.ImageFiles"/>, <see cref="ThreadInfo.ThumbnailFiles"/>, <see cref="ThreadInfo.FileNames"/>, <see cref="ThreadInfo.ThumbnailNames"/>, and <see cref="ThreadInfo.FileStatus"/> to be populated in the <see cref="CurrentThread"/>.</para>
        /// </summary>
        private void DownloadThreadFiles() {
            try {
                if (CurrentThread.AddedNewPosts) {
                    // Set the ThreadStatus to ThreadDownloading
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    if (Config.Settings.General.AutoSaveThreads) {
                        CurrentThread.SaveThread();
                        CurrentThread.ThreadModified = false;
                    }

                    // Create the directories, WebClient doesn't create them.
                    if (!Directory.Exists(CurrentThread.Data.DownloadPath)) {
                        Directory.CreateDirectory(CurrentThread.Data.DownloadPath);
                    }
                    if (Config.Settings.Downloads.SaveThumbnails && !Directory.Exists($"{CurrentThread.Data.DownloadPath}\\thumb")) {
                        Directory.CreateDirectory($"{CurrentThread.Data.DownloadPath}\\thumb");
                    }

                    // Create the webclient instance to download the files.
                    using murrty.classcontrols.ExtendedWebClient wc = new();
                    wc.Method = murrty.classcontrols.HttpMethod.GET;
                    wc.UserAgent = Config.Settings.Advanced.UserAgent;
                    wc.CookieContainer = CurrentThread.ThreadCookieContainer = new();

                    // Variables to assist during downloads
                    string FullFilePath = ""; // The absolute path of the file.
                    string FullThumbnailPath = ""; // The absolute path of the thumbnail.
                    string CurrentURL = "";
                    for (int ImageFilesIndex = 0; ImageFilesIndex < CurrentThread.Data.ImageFiles.Count; ImageFilesIndex++) {
                        //if (Program.DebugMode) break;
                        // safety net, check for null in the list
                        // continue the downloads if it is null.
                        if (CurrentThread.Data.FileStatus[ImageFilesIndex] != FileDownloadStatus.Downloaded && CurrentThread.Data.FileStatus[ImageFilesIndex] != FileDownloadStatus.FileNotFound && CurrentThread.Data.ImageFiles[ImageFilesIndex] is not null) {
                            // Variables to help with the current file
                            bool Downloaded = false; // If the file finished downloading.
                            bool DownloadingThumbnail = false; // If the thumbnail is being downloaded. If it 404's, it's not a huge problem.
                            int DownloadAttempt = 0; // The attempts it took to download the file.

                            // Set the icon in the list to "Downloading".
                            this.Invoke((MethodInvoker)delegate {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            });

                            // Create the file paths, and check if they conform to the 255 length limit.
                            // May throw exceptions if the DownloadPath is ~255 in length itself, but they chose this path.
                            FullFilePath = $"{CurrentThread.Data.DownloadPath}\\{CurrentThread.Data.FileNames[ImageFilesIndex]}";
                            if (FullFilePath.Length > 255 && !Config.Settings.Downloads.AllowFileNamesGreaterThan255) {

                                FileHandler.StripFileNameAndExtension(CurrentThread.Data.FileNames[ImageFilesIndex], out string NewFileName, out string FileExt);
                                while (FullFilePath.Length > 255) {
                                    NewFileName = NewFileName[..^1];
                                    FullFilePath = $"{CurrentThread.Data.DownloadPath}\\{NewFileName}.{FileExt}";
                                }
                                File.WriteAllText($"{CurrentThread.Data.DownloadPath}\\{NewFileName}.txt", CurrentThread.Data.FileNames[ImageFilesIndex]);
                                FullFilePath = $"{CurrentThread.Data.DownloadPath}\\{NewFileName}.{FileExt}";

                            }
                            if (Config.Settings.Downloads.SaveThumbnails) {
                                FullThumbnailPath = $"{CurrentThread.Data.DownloadPath}\\thumb\\{CurrentThread.Data.ThumbnailNames[ImageFilesIndex]}";
                                if (FullThumbnailPath.Length > 255 && !Config.Settings.Downloads.AllowFileNamesGreaterThan255) {

                                    FileHandler.StripFileNameAndExtension(CurrentThread.Data.ThumbnailNames[ImageFilesIndex], out string NewFileName, out string FileExt);
                                    while (FullThumbnailPath.Length > 255) {
                                        NewFileName = NewFileName[..^1];
                                        FullThumbnailPath = $"{CurrentThread.Data.DownloadPath}\\thumb\\{NewFileName}.{FileExt}";
                                    }
                                    File.WriteAllText($"{CurrentThread.Data.DownloadPath}\\thumb\\{NewFileName}.txt", CurrentThread.Data.ThumbnailNames[ImageFilesIndex]);
                                    FullThumbnailPath = $"{CurrentThread.Data.DownloadPath}\\thumb\\{NewFileName}.{FileExt}";
                                }
                            }

                            // Main download loop to try to download the file.
                            do {
                                CurrentURL = CurrentThread.Data.ImageFiles[ImageFilesIndex];
                                try {
                                    if (CurrentThread.Chan == ChanType.EightChan) {
                                        if (!File.Exists(FullFilePath)) {
                                            wc.Headers.Add("Accept", "image/avif,image/webp,*/*");
                                            wc.Headers.Add("Referer", $"{CurrentThread.Data.ThreadURL}");
                                            wc.DownloadFile(CurrentURL, FullFilePath);
                                        }
                                        Downloaded = true;
                                        if (Config.Settings.Downloads.SaveThumbnails && !File.Exists(FullThumbnailPath)) {
                                            DownloadingThumbnail = true;
                                            CurrentURL = CurrentThread.Data.ThumbnailFiles[ImageFilesIndex];
                                            wc.Headers.Add("Accept", "image/avif,image/webp,*/*");
                                            wc.Headers.Add("Referer", $"{CurrentThread.Data.ThreadURL}");
                                            wc.DownloadFile(CurrentURL, FullThumbnailPath);
                                        }
                                        DownloadingThumbnail = false;
                                    }
                                    else {
                                        if (!File.Exists(FullFilePath)) {
                                            wc.DownloadFile(CurrentURL, FullFilePath);
                                        }
                                        Downloaded = true;
                                        if (Config.Settings.Downloads.SaveThumbnails && !File.Exists(FullThumbnailPath)) {
                                            DownloadingThumbnail = true;
                                            CurrentURL = CurrentThread.Data.ThumbnailFiles[ImageFilesIndex];
                                            wc.DownloadFile(CurrentURL, FullThumbnailPath);
                                        }
                                        DownloadingThumbnail = false;
                                    }
                                }
                                catch (WebException WebEx) {
                                    // check if the result was 404, and if it was break out of the loop and set the file to 404.
                                    if (!DownloadingThumbnail) {
                                        if (WebEx.Response is HttpWebResponse Response && Response.StatusCode == HttpStatusCode.NotFound) {
                                            CurrentThread.Data.FileStatus[ImageFilesIndex] = FileDownloadStatus.FileNotFound;
                                            CurrentThread.ThreadModified = true;
                                            this.Invoke((MethodInvoker)delegate {
                                                lvImages.Items[ImageFilesIndex].ImageIndex = 4;
                                            });
                                        }
                                        else {
                                            CurrentThread.Data.FileStatus[ImageFilesIndex] = FileDownloadStatus.Error;
                                            CurrentThread.ThreadModified = true;
                                            this.Invoke((MethodInvoker)delegate {
                                                lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                                            });
                                            if (CurrentThread.Chan != ChanType.EightKun) {
                                                HandleWebException(WebEx, CurrentURL);
                                            }
                                        }
                                        break;
                                    }
                                    else {
                                        HandleWebException(WebEx, CurrentURL);
                                    }
                                    DownloadAttempt++;
                                }
                                catch (Exception ex) {
                                    murrty.classes.Log.ReportException(ex);
                                    DownloadAttempt++;
                                }
                            } while (!Downloaded && DownloadAttempt < 5);

                            // Check if the file was actually downloaded, then continue.
                            // Otherwise mark it as not downloaded.
                            if (Downloaded) {
                                CurrentThread.Data.DownloadedImagesCount++;
                                CurrentThread.Data.FileStatus[ImageFilesIndex] = FileDownloadStatus.Downloaded;
                                CurrentThread.ThreadModified = true;
                                this.Invoke((MethodInvoker)delegate {
                                    lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                                });
                            }

                            // Sleep for 100ms.
                            if (PauseBetweenFiles) { Thread.Sleep(100); }
                        }
                    }
                    if (Config.Settings.Downloads.SaveHTML) {
                        File.WriteAllText(CurrentThread.Data.DownloadPath + "\\Thread.html", CurrentThread.ThreadHTML + HtmlControl.GetHTMLFooter(CurrentThread));
                    }
                    CurrentThread.AddedNewPosts = false;
                }
            }
            catch { throw; }
            finally { CurrentThread.DownloadingFiles = false; }
            
        }

        private string GenerateFileName(string FileID, string OriginalName, string Extension) {
            if (Config.Settings.Downloads.SaveOriginalFilenames) {
                OriginalName = !string.IsNullOrWhiteSpace(OriginalName) ? OriginalName : DefaultEmptyFileName;

                // check for duplicates, and set the prefix to "(1)" if there is duplicates
                // (the space is intentional)
                if (Config.Settings.Downloads.PreventDuplicates) {
                    string FileNamePrefix = ""; // the prefix for the file
                    string FileNameSuffix = ""; // the suffix for the file
                    if (CurrentThread.Data.FileOriginalNames.Contains(OriginalName + Extension)) {
                        if (CurrentThread.Data.FileNamesDupes.Contains(OriginalName + Extension)) {
                            int DupeNameIndex = CurrentThread.Data.FileNamesDupes.LastIndexOf(OriginalName + Extension);
                            CurrentThread.Data.FileNamesDupesCount[DupeNameIndex]++;
                            FileNamePrefix = $" (duplicate {CurrentThread.Data.FileNamesDupesCount[DupeNameIndex]})";
                        }
                        else {
                            CurrentThread.Data.FileNamesDupes.Add(OriginalName + Extension);
                            CurrentThread.Data.FileNamesDupesCount.Add(1);
                            FileNameSuffix = " (duplicate)";
                        }
                    }
                    // replace any invalid file name characters.
                    // some linux nerds can have invalid windows file names as file names
                    // so we gotta filter them.
                    OriginalName = FileNamePrefix + FileHandler.ReplaceIllegalCharacters(OriginalName) + FileNameSuffix;
                }

                return OriginalName;
            }
            else return FileID;
        }

        private void HandleWebException(WebException WebEx, string CurrentURL) {
            if (WebEx.Response is HttpWebResponse Response) {
                switch (Response.StatusCode) {
                    case HttpStatusCode.NotModified: {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                    } break;

                    case HttpStatusCode.NotFound: {
                        if (CurrentThread.DownloadingFiles) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadFile404;
                        }
                        else {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadIs404;
                        }
                    } break;

                    case HttpStatusCode.Forbidden: {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadIsNotAllowed;
                    } break;

                    default: {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        murrty.classes.Log.ReportException(WebEx, CurrentURL);
                    } break;
                }
            }
            else {
                CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                murrty.classes.Log.ReportException(WebEx, CurrentURL);
            }
        }

        #endregion

        // Known issues:  (2022-01-25)

        // None so far.

        #region 4chan Download Logic: Completed. (2022-08-16)

        private void Set4chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = $"https://i.4cdn.org/{CurrentThread.Data.ThreadBoard}/";
                string ThreadJSON = null;
                string CurrentURL = null;

                #region Try block

                try {

                    #region API Download Logic
                    // Check the thread board and id for null value
                    // Can't really parse the API without them.
                    if (string.IsNullOrWhiteSpace(CurrentThread.Data.ThreadBoard) || string.IsNullOrWhiteSpace(CurrentThread.Data.ThreadID)) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        ManageThread(ThreadEvent.AfterDownload);
                        return;
                    }

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                    CurrentURL = Networking.GetAPILink(CurrentThread);

                    if (!GetThreadJSON(CurrentURL, out ThreadJSON)) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        return;
                    }

                    // Serialize the json data into a class object.
                    FourChanThread ThreadData = ThreadJSON.JsonDeserialize<FourChanThread>();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData.posts.Length == 0) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        return;
                    }

                    // Reset CurrentURL to thread url.
                    // Because it's currently scanning
                    CurrentURL = CurrentThread.Data.ThreadURL;
                    #endregion

                    #region API Parsing Logic
                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!CurrentThread.Data.RetrievedThreadName && !CurrentThread.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just fakken use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.posts[0].sub,
                            Comment: ThreadData.posts[0].com,
                            FallbackName: CurrentThread.Data.ThreadID
                        );
                        CurrentThread.Data.ThreadName = NewName;
                        CurrentThread.Data.RetrievedThreadName = true;
                        CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(
                            "<title></title>",
                            $"<title> /{CurrentThread.Data.ThreadBoard}/ - {NewName} - 4chan</title>");
                        CurrentThread.Data.HtmlThreadNameSet = true;
                        this.Invoke((MethodInvoker)delegate () {
                            UpdateThreadName(true);
                        });
                    }

                    // check for archive flag in the post.
                    CurrentThread.Data.ThreadArchived = ThreadData.posts[0].archived ?? false;

                    // Create new variables for the next loop.
                    PostData CurrentPost;       // post info for SaveHTML

                    // Start counting through the posts.
                    // Intentionally start at index 0 so it can check for posts that
                    // would have been skipped if a post gets deleted.
                    FourChanThread.Post Post;
                    for (int PostIndex = 0; PostIndex < ThreadData.posts.Length; PostIndex++) {
                        // Set the temporary post to the looped index post.
                        Post = ThreadData.posts[PostIndex];
                        // Also checks if the parsed post ids contains the current post number,
                        // and parses it if it does NOT contain it.
                        // Useful for posts that got skipped on accident or an issue arose from
                        // parsing it.
                        string PostID = Post.no.ToString();
                        if (!CurrentThread.Data.ParsedPostIDs.Contains(PostID)) {
                            CurrentPost = new();

                            // Checks if there's a file in the post.
                            // If not, it'll skip straight to the SaveHTML portion.
                            if (Post.tim != null) {
                                string FileID = Post.tim.ToString(); // The unique file id (NOT POST ID)
                                string OriginalFileName = Post.filename; // the original file name
                                string FileExtension = Post.ext; // the file extension
                                string ImageFile = $"{FileBaseURL}{Post.tim}{Post.ext}"; // the full url of the file
                                string ThumbnailFile = $"{FileBaseURL}{FileID}s.jpg"; // the full url of the thumbnail file
                                string FileHash = Post.md5; // the hash of the file
                                string FileName = GenerateFileName(FileID, OriginalFileName, FileExtension);

                                // Adds the regular data to the lists.
                                CurrentThread.Data.ThumbnailNames.Add(FileID + "s.jpg");
                                CurrentThread.Data.ImagePostIDs.Add(PostID);
                                CurrentThread.Data.FileIDs.Add(FileID);
                                CurrentThread.Data.FileExtensions.Add(FileExtension);
                                CurrentThread.Data.ImageFiles.Add(ImageFile);
                                CurrentThread.Data.ThumbnailFiles.Add(ThumbnailFile);
                                CurrentThread.Data.FileHashes.Add(FileHash);
                                CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);

                                // the generated name
                                // add the original name after the dupe check or everything'll be considered dupes.
                                CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                CurrentThread.Data.FileOriginalNames.Add(OriginalFileName + FileExtension);

                                // add more information for the files,
                                // add info to the postinfo if SaveHTML is enabled
                                // the info is only available in this bracket, so it's gotta be
                                // added here, at the end.
                                CurrentPost.Files.Add(new() {
                                    ID = FileID,
                                    OriginalName = OriginalFileName,
                                    GeneratedName = FileName + FileExtension,
                                    Extension = FileExtension,
                                    Dimensions = new(Post.w ?? 0, Post.h ?? 0),
                                    ThumbnailDimensions = new(Post.tn_w ?? 0, Post.tn_h ?? 0),
                                    Size = Post.fsize ?? 0,
                                    Spoiled = Post.spoiler is not null && Post.spoiler == 1
                                });

                                // add a new listviewitem to the listview for this image.
                                ListViewItem lvi = new();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = FileID;
                                lvi.SubItems[0].Text = FileID;
                                lvi.SubItems[1].Text = FileExtension.Trim('.');
                                lvi.SubItems[2].Text = OriginalFileName + FileExtension;
                                lvi.SubItems[3].Text = FileHash;
                                lvi.ImageIndex = 0;
                                this.Invoke((MethodInvoker)delegate {
                                    lvImages.Items.Add(lvi);
                                });

                                CurrentThread.Data.ThreadImagesCount++;
                                CurrentThread.Data.ThreadPostsCount++;
                            }

                            // save post info if SaveHTML is enabled.
                            // this is for every post regardless if there's an image or not.
                            CurrentPost.PostID = PostID;
                            //CurrentPost.PostDate = xmlPostNow[0].InnerText;
                            CurrentPost.PostDate = HtmlControl.GetReadableTime(Post.time);
                            CurrentPost.PosterName = Post.name;
                            CurrentPost.PosterTripcode = Post.trip;
                            CurrentPost.PosterID = Post.id;
                            CurrentPost.SpecialPosterName = Post.capcode;

                            // checks if there's a subject in the post, and adds it.
                            // otherwise, it'll just add a empty string.
                            CurrentPost.PostSubject = Post.sub ?? string.Empty;

                            // checks if there's a comment, and adds it.
                            // otherwise, it'll just add a empty string.
                            CurrentPost.PostMessage = Post.com ?? string.Empty;

                            // First post flag.
                            CurrentPost.FirstPost = PostIndex == 0;

                            // add the new post to the html.
                            CurrentThread.ThreadHTML += HtmlControl.GetPostHtmlData(CurrentPost, CurrentThread);

                            CurrentThread.Data.ParsedPostIDs.Add(PostID);
                            CurrentThread.Data.Posts.Add(CurrentPost);
                            CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                        }
                    }

                    // update the form totals and status.
                    this.Invoke((MethodInvoker)delegate () {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbPostsParsed.Text = "posts parsed: " + CurrentThread.Data.ParsedPostIDs.Count.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;

                }

                #endregion

                #region Catch Logic

                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }

                #endregion

                #region Finally block

                finally {
                    if (CurrentThread.Data.ThreadArchived)
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadIsArchived;

                    this?.BeginInvoke(() => {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }

                #endregion

            }) {
                Name = $"4chan thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        private static bool Generate4chanMD5(string InputFile, string InputFileHash) {
            // Attempts to convert existing file to 4chan's hash type
            try {
                if (!File.Exists(InputFile)) {
                    return false;
                }
                string OutputHash = null;
                using (System.Security.Cryptography.MD5 FileMD5 = System.Security.Cryptography.MD5.Create())
                using (FileStream FileStream = File.OpenRead(InputFile)) {
                    var FileHash = FileMD5.ComputeHash(FileStream);
                    Thread.Sleep(50);
                    OutputHash = BitConverter.ToString(FileHash).Replace("-", string.Empty).ToLowerInvariant();
                }
                byte[] RawByte = new byte[16];
                for (int i = 0; i < 32; i += 2) {
                    RawByte[i / 2] = Convert.ToByte(OutputHash.Substring(i, 2), 16);
                }

                OutputHash = Convert.ToBase64String(RawByte);

                return OutputHash == InputFileHash;
            }
            catch (Exception ex) {
                murrty.classes.Log.ReportException(ex);
                return false;
            }
        }

        #endregion

        #region 420chan Download Logic: Completed. (2022-01-25)

        private void Set420chanThread() {
            // 420chan doesn't have original filenames anymore?

            DownloadThread = new Thread(() => {
                string FileBaseURL = $"https://boards.420chan.org/{CurrentThread.Data.ThreadBoard}/src/";
                string ThumbnailBaseUrl = $"https://boards.420chan.org/{CurrentThread.Data.ThreadBoard}/thumb/";
                string ThreadJSON = null;
                string CurrentURL = null;

                #region Try block

                try {

                    #region API Download Logic
                    if (CurrentThread.Data.ThreadBoard == null || CurrentThread.Data.ThreadID == null) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        ManageThread(ThreadEvent.AfterDownload);
                        return;
                    }

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                    CurrentURL = Networking.GetAPILink(CurrentThread);

                    if (!GetThreadJSON(CurrentURL, out ThreadJSON)) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        return;
                    }

                    // Serialize the json data into a class object.
                    FourTwentyChanThread ThreadData = new();
                    ThreadData = ThreadJSON.JsonDeserialize<FourTwentyChanThread>();

                    CurrentURL = CurrentThread.Data.ThreadURL;
                    #endregion

                    #region API Parsing Logic
                    XmlDocument xmlDoc = new();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlPost = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                    XmlNodeList xmlPostNumber = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/no");
                    XmlNodeList xmlArchived = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/closed");

                    if (!CurrentThread.Data.HtmlThreadNameSet) {
                        if (!CurrentThread.Data.RetrievedThreadName) {
                            string NewName = FileHandler.GetShortThreadName(
                                Subtitle: xmlPost[0].SelectNodes("sub").Count > 0 ? xmlPost[0].SelectNodes("sub")[0].InnerText : null,
                                Comment: xmlPost[0].SelectNodes("com").Count > 0 ? xmlPost[0].SelectNodes("com")[0].InnerText : null,
                                FallbackName: CurrentThread.Data.ThreadID
                            );
                            CurrentThread.Data.ThreadName = NewName;
                            CurrentThread.Data.RetrievedThreadName = true;
                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace("<title></title>", $"<title> /{CurrentThread.Data.ThreadBoard}/ - {NewName} - 420chan</title>");
                            this.Invoke((MethodInvoker)delegate () {
                                UpdateThreadName(true);
                            });
                        }
                        else {
                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace("<title></title>", $"<title> /{CurrentThread.Data.ThreadBoard}/ - {CurrentThread.Data.ThreadID} - 420chan</title>");
                        }
                        CurrentThread.Data.HtmlThreadNameSet = true;
                    }

                    for (int PostIndex = 0; PostIndex < xmlPost.Count; PostIndex++) {
                        if (!CurrentThread.Data.ParsedPostIDs.Contains(xmlPostNumber[PostIndex].InnerText)) {
                            PostData CurrentPost = new();
                            XmlNodeList xmlFileName = xmlPost[PostIndex].SelectNodes("filename"); // original file name
                            XmlNodeList xmlExt = xmlPost[PostIndex].SelectNodes("ext"); // file extension
                            if (xmlFileName.Count > 0) {
                                XmlNodeList xmlPostWidth = xmlPost[PostIndex].SelectNodes("w");
                                XmlNodeList xmlPostHeight = xmlPost[PostIndex].SelectNodes("h");
                                XmlNodeList xmlThumbnailWidth = xmlPost[PostIndex].SelectNodes("tn_w");
                                XmlNodeList xmlThumbnailHeight = xmlPost[PostIndex].SelectNodes("tn_h");
                                XmlNodeList xmlFileSize = xmlPost[PostIndex].SelectNodes("fsize");

                                string FileID = xmlFileName[0].InnerText;
                                string OriginalFileName = xmlFileName[0].InnerText;
                                string FileExtension = xmlExt[0].InnerText;
                                string ImageFile = FileBaseURL + xmlFileName[0].InnerText + xmlExt[0].InnerText;
                                string ThumbnailFile = FileBaseURL + FileID + "s.jpg";

                                CurrentThread.Data.FileIDs.Add(FileID);
                                CurrentThread.Data.FileExtensions.Add(FileExtension);
                                CurrentThread.Data.ThumbnailFiles.Add(ThumbnailFile);
                                CurrentThread.Data.ImageFiles.Add(ImageFile);
                                CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);

                                string FileNameToReplace = FileID;
                                string FileName = FileID;
                                if (Config.Settings.Downloads.SaveOriginalFilenames) {
                                    FileName = OriginalFileName;
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";

                                    if (Config.Settings.Downloads.PreventDuplicates) {
                                        if (CurrentThread.Data.FileOriginalNames.Contains(FileName)) {
                                            if (CurrentThread.Data.FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = CurrentThread.Data.FileNamesDupes.IndexOf(FileName);
                                                CurrentThread.Data.FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNamePrefix = "(" + CurrentThread.Data.FileNamesDupesCount[DupeNameIndex].ToString() + ") ";
                                            }
                                            else {
                                                CurrentThread.Data.FileNamesDupes.Add(FileName);
                                                CurrentThread.Data.FileNamesDupesCount.Add(1);
                                                FileNamePrefix = "(1) ";
                                            }
                                        }
                                    }

                                    FileName = FileHandler.ReplaceIllegalCharacters(FileName);
                                    FileNameToReplace = FileNamePrefix + FileName + FileNameSuffix;
                                    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                }

                                CurrentThread.Data.FileOriginalNames.Add(OriginalFileName);
                                CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                CurrentThread.Data.ThumbnailNames.Add(FileID + "s.jpg");
                                CurrentThread.Data.ImagePostIDs.Add(xmlPostNumber[PostIndex].InnerText);
                                CurrentPost.Files.Add(new() {
                                    ID = FileID,
                                    OriginalName = OriginalFileName,
                                    Extension = FileExtension,
                                    GeneratedName = FileName + FileExtension,
                                    Dimensions = new(
                                        int.TryParse(xmlPostWidth[0].InnerText, out int Width) ? Width : 0,
                                        int.TryParse(xmlPostHeight[0].InnerText, out int Height) ? Height : 0
                                    ),
                                    ThumbnailDimensions = new(
                                        int.TryParse(xmlThumbnailWidth[0].InnerText, out int ThumbWidth) ? ThumbWidth : 125,
                                        int.TryParse(xmlThumbnailHeight[0].InnerText, out int ThumbHeight) ? ThumbHeight : 125
                                    ),
                                    Size = long.TryParse(xmlFileSize[0].InnerText, out long Size) ? Size : 0,
                                    Spoiled = false,
                                });

                                string OldHTMLLinks = null;
                                if (Config.Settings.Downloads.SaveThumbnails) {
                                    OldHTMLLinks = "//i.4cdn.org/" + CurrentThread.Data.ThreadBoard + "/" + FileID + "s.jpg";
                                    CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + FileID + "s.jpg");
                                }

                                OldHTMLLinks = "//i.4cdn.org/" + CurrentThread.Data.ThreadBoard + "/" + FileID;
                                string OldHTMLLinks2 = "//is2.4chan.org/" + CurrentThread.Data.ThreadBoard + "/" + FileID;
                                if (Config.Settings.Downloads.SaveOriginalFilenames) {
                                    CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks, FileNameToReplace);
                                    CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks2, FileNameToReplace);
                                }
                                else {
                                    CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks, FileID);
                                    CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks2, FileID);
                                }

                                ListViewItem lvi = new();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = FileID;
                                lvi.SubItems[0].Text = FileID;
                                lvi.SubItems[1].Text = FileExtension.Trim('.');
                                lvi.SubItems[2].Text = OriginalFileName;
                                lvi.ImageIndex = 0;
                                this.Invoke((MethodInvoker)delegate {
                                    lvImages.Items.Add(lvi);
                                });

                                CurrentThread.Data.ThreadImagesCount++;
                                CurrentThread.Data.ThreadPostsCount++;
                            }

                            if (Config.Settings.Downloads.SaveHTML) {
                                XmlNodeList xmlPostTime = xmlPost[PostIndex].SelectNodes("time");
                                XmlNodeList xmlPostName = xmlPost[PostIndex].SelectNodes("name");
                                XmlNodeList xmlPostSubject = xmlPost[PostIndex].SelectNodes("sub");
                                XmlNodeList xmlPostComment = xmlPost[PostIndex].SelectNodes("com");
                                CurrentPost.PostID = xmlPostNumber[PostIndex].InnerText;
                                CurrentPost.PostDate = HtmlControl.GetReadableTime(Convert.ToDouble(xmlPostTime[0].InnerText));
                                CurrentPost.PosterName = xmlPostName[0].InnerText;
                                CurrentPost.PostSubject = xmlPostSubject.Count > 0 ? xmlPostSubject[0].InnerText : string.Empty;
                                CurrentPost.PostMessage = xmlPostComment.Count > 0 ? xmlPostComment[0].InnerText : string.Empty;
                                CurrentPost.FirstPost = PostIndex == 0;
                                CurrentThread.ThreadHTML += HtmlControl.GetPostHtmlData(CurrentPost, CurrentThread);
                            }

                            CurrentThread.Data.ParsedPostIDs.Add(xmlPostNumber[PostIndex].InnerText);
                            CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                        }
                    }

                    CurrentThread.Data.ThreadArchived = xmlArchived.Count > 0;

                    this.Invoke((MethodInvoker)delegate () {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbPostsParsed.Text = "posts parsed: " + CurrentThread.Data.ParsedPostIDs.Count.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }

                #endregion

                #region Catch block

                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }

                #endregion

                #region Finally block
                finally {
                    if (CurrentThread.Data.ThreadArchived) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    }
                    this.Invoke((MethodInvoker)delegate () {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }

                #endregion

            }) {
                Name = $"420chan thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        #endregion

        #region 7chan Download Logic: Works. (2022-01-25)

        // Notes:
        // Posts with extra files can only contain 4 files.

        // Needs:
        // Original file names?
        // HTML?

        private void Set7chanThread() {
            DownloadThread = new Thread(() => {
                string ThreadHTML = null;
                string CurrentURL = null;

                #region Try block
                try {

                    #region HTML Download Logic
                    if (CurrentThread.Data.ThreadBoard == null || CurrentThread.Data.ThreadID == null) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        return;
                    }

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                    for (int TryCount = 0; TryCount < 5; TryCount++) {
                        CurrentURL = CurrentThread.Data.ThreadURL;

                        if (!GetThreadHTML(CurrentURL, out ThreadHTML)) {
                            if (TryCount == 5) {
                                CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                                return;
                            }
                            Thread.Sleep(5000);
                        }
                        else {
                            break;
                        }
                    }

                    // Rudimentary check, but you never know.
                    if (ThreadHTML == CurrentThread.LastThreadHTML) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                        return;
                    }

                    CurrentThread.LastThreadHTML = ThreadHTML;
                    #endregion

                    #region HTML Parsing Logic
                    Html.HtmlDocument ThreadDocument = Html.HtmlDocument.FromHtml(ThreadHTML);
                    List<Html.HtmlElementNode> Posts = new(ThreadDocument.Find("div[class:=\"post\"][id:=\"^([0-9]+)\"]"));

                    // Just to make sure it truley was modified.
                    if (Posts.Count == CurrentThread.Data.ParsedPostIDs.Count) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                        return;
                    }

                    Html.HtmlElementNode CurrentPost;
                    Html.HtmlDocument CurrentPostDocument;
                    PostData NewPost;
                    string PostID;
                    string SingleLinedNode;
                    for (int PostIndex = 0; PostIndex < Posts.Count; PostIndex++) {
                        CurrentPost = Posts[PostIndex];
                        PostID = CurrentPost.Attributes["id"].Value;
                        if (!CurrentThread.Data.ParsedPostIDs.Contains(PostID)) {
                            NewPost = new();
                            NewPost.FirstPost = PostIndex == 0;
                            NewPost.PostID = PostID;

                            // We're parsing the inner HTML of the post.
                            SingleLinedNode = CurrentPost.InnerHtml.Replace("\r\n", "\n").Replace("\n", "");
                            CurrentPostDocument = Html.HtmlDocument.FromHtml(SingleLinedNode);

                            List<Html.HtmlElementNode> InnerNodes;
                            if ((InnerNodes = new(CurrentPostDocument.Find("span[class:=\"postername\"]"))).Count > 0) {
                                NewPost.PosterName = InnerNodes[0].Text.Trim();
                            }
                            if ((InnerNodes = new(CurrentPostDocument.Find("span[class:=\"postertrip\"]"))).Count > 0) {
                                NewPost.PosterTripcode = InnerNodes[0].Text.Trim();
                            }
                            if ((InnerNodes = new(CurrentPostDocument.Find("span[class:=\"subject\"]"))).Count > 0) {
                                NewPost.PostSubject = InnerNodes[0].Text.Trim();
                            }
                            if ((InnerNodes = new(CurrentPostDocument.Find("p[class:=\"message\"]"))).Count > 0) {
                                NewPost.PostMessage = SevenChanThread.TranslateMessage(InnerNodes[0].InnerHtml.Trim(), CurrentThread);
                            }
                            NewPost.PostDate = Chans.FindRegex(
                                SingleLinedNode,
                                "\\<span class=\"postername\"\\>.*?\\<\\/span\\>",
                                "\\<span class=\"reflink\"\\>"
                            );
                            NewPost.PosterID = Chans.FindRegex(
                                SingleLinedNode,
                                "\\<\\/span\\>ID: ",
                                "\\<span class=\"extrabtns\""
                            );

                            if ((InnerNodes = new(CurrentPostDocument.Find("p[class:=\"file_size\"]"))).Count > 0) {
                                Html.HtmlDocument FileDocument = Html.HtmlDocument.FromHtml(InnerNodes[0].InnerHtml);
                                PostData.FileData NewFile = new();

                                string FileURL = FileDocument.Find("a[id:=\"expandimg_(.*?)\"]").ElementAt(0).Attributes["href"].Value;
                                string FileID = FileURL.Split('/')[^1];
                                string FileExtension = Path.GetExtension(FileID);
                                FileID = Path.GetFileNameWithoutExtension(FileID);
                                string FileName;
                                string OriginalFileName;

                                NewFile.ID = FileID;
                                NewFile.Extension = FileExtension;

                                string FileInformation = InnerNodes[0].Text.Replace("\r\n", "\n").Replace("\n", "");
                                FileInformation = FileInformation[(FileInformation.IndexOf('(') + 1)..^1];
                                string[] FileInformationSplit = FileInformation.Split(',');
                                NewFile.CustomSize = FileInformationSplit[0][^2..];
                                string[] FileDimensions = FileInformationSplit[1].Trim().ToLower().Split('x');
                                NewFile.Dimensions = new(int.Parse(FileDimensions[0]), int.Parse(FileDimensions[1]));
                                OriginalFileName = NewFile.OriginalName = Path.GetFileNameWithoutExtension(string.Join(",", FileInformationSplit.Skip(2))).Trim();

                                List<Html.HtmlElementNode> ThumbData = new(CurrentPostDocument.Find("img[class:=\"thumb\"]"));
                                NewFile.ThumbnailDimensions = new(int.Parse(ThumbData[0].Attributes["width"].Value), int.Parse(ThumbData[0].Attributes["height"].Value));

                                FileName = GenerateFileName(FileID, OriginalFileName, FileExtension);
                                NewFile.GeneratedName = FileName + FileExtension;

                                NewFile.Spoiled = false;
                                NewPost.Files.Add(NewFile);
                                CurrentThread.Data.ThumbnailNames.Add(FileID + "s.jpg");
                                CurrentThread.Data.ImagePostIDs.Add(PostID);
                                CurrentThread.Data.FileIDs.Add(FileID);
                                CurrentThread.Data.FileExtensions.Add(FileExtension);
                                CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);
                                CurrentThread.Data.ThumbnailFiles.Add(ThumbData[0].Attributes["src"].Value);
                                CurrentThread.Data.ImageFiles.Add(FileURL);
                                CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                CurrentThread.Data.FileOriginalNames.Add(OriginalFileName + FileExtension);

                                ListViewItem lvi = new();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = FileID;
                                lvi.SubItems[0].Text = FileID;
                                lvi.SubItems[1].Text = FileExtension.Trim('.');
                                lvi.SubItems[2].Text = OriginalFileName + FileExtension;
                                lvi.ImageIndex = 0;
                                this.Invoke((MethodInvoker)delegate {
                                    lvImages.Items.Add(lvi);
                                });

                                CurrentThread.Data.ThreadImagesCount++;
                                CurrentThread.Data.ThreadPostsCount++;
                            }
                            else if ((InnerNodes = new(CurrentPostDocument.Find("div[style:=\"float:left\"]"))).Count > 0) {
                                Html.HtmlDocument FileDocument;
                                for (int CurrentFile = 0; CurrentFile < InnerNodes.Count; CurrentFile++) {
                                    PostData.FileData NewFile = new();
                                    FileDocument = Html.HtmlDocument.FromHtml(InnerNodes[CurrentFile].InnerHtml);

                                    string FileURL = FileDocument.Find("span[class:=\"multithumb(first)?\"] > a[id:=\"expandimg_(.*?)\"]").ElementAt(0).Attributes["href"].Value;
                                    string FileID = FileURL.Split('/')[^1];
                                    string FileExtension = Path.GetExtension(FileID);
                                    FileID = Path.GetFileNameWithoutExtension(FileID);
                                    string FileName;
                                    string OriginalFileName;

                                    NewFile.ID = FileID;
                                    NewFile.Extension = FileExtension;

                                    // Parse the thumbnail data first, because file data is alongside it.
                                    List<Html.HtmlElementNode> ThumbnailData = new(FileDocument.Find($"span[id:=\"thumb_{NewPost.PostID}-{CurrentFile}\"] > img[class:=\"multithumb(first)?\"]"));
                                    NewFile.ThumbnailDimensions = new(int.Parse(ThumbnailData[0].Attributes["width"].Value), int.Parse(ThumbnailData[0].Attributes["height"].Value));

                                    string FileInformation = ThumbnailData[0].Attributes["title"].Value;
                                    FileInformation = FileInformation[(FileInformation.IndexOf("(") + 1)..^1];
                                    string[] FileInformationSplit = FileInformation.Split(',');
                                    NewFile.Size = FileInformationSplit[0].ToLower()[^2..] switch {
                                        "mb" => 0,
                                        "kb" => 0,
                                        _ => 0
                                    };
                                    string[] FileDimensions = FileInformationSplit[1].Trim().ToLower().Split('x');
                                    NewFile.Dimensions = new(int.Parse(FileDimensions[0]), int.Parse(FileDimensions[1]));
                                    OriginalFileName = NewFile.OriginalName = Path.GetFileNameWithoutExtension(string.Join(",", FileInformationSplit.Skip(2))).Trim();

                                    FileName = GenerateFileName(FileID, OriginalFileName, FileExtension);
                                    NewFile.GeneratedName = FileName + FileExtension;

                                    NewFile.Spoiled = false;
                                    NewPost.Files.Add(NewFile);
                                    CurrentThread.Data.ThumbnailNames.Add(FileID + "s.jpg");
                                    CurrentThread.Data.ImagePostIDs.Add(PostID);
                                    CurrentThread.Data.FileIDs.Add(FileID);
                                    CurrentThread.Data.FileExtensions.Add(FileExtension);
                                    CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);
                                    CurrentThread.Data.ThumbnailFiles.Add(ThumbnailData[0].Attributes["src"].Value);
                                    CurrentThread.Data.ImageFiles.Add(FileURL);
                                    CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                    CurrentThread.Data.FileOriginalNames.Add(OriginalFileName + FileExtension);

                                    ListViewItem lvi = new();
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.Name = FileID;
                                    lvi.SubItems[0].Text = FileID;
                                    lvi.SubItems[1].Text = FileExtension.Trim('.');
                                    lvi.SubItems[2].Text = OriginalFileName + FileExtension;
                                    lvi.ImageIndex = 0;
                                    this.Invoke((MethodInvoker)delegate {
                                        lvImages.Items.Add(lvi);
                                    });

                                    CurrentThread.Data.ThreadImagesCount++;
                                    CurrentThread.Data.ThreadPostsCount++;
                                }
                            }

                            CurrentThread.ThreadHTML += HtmlControl.GetPostHtmlData(NewPost, CurrentThread);

                            CurrentThread.Data.ParsedPostIDs.Add(PostID);
                            CurrentThread.Data.Posts.Add(NewPost);
                            CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                        }
                    }

                    this.Invoke((MethodInvoker)delegate {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #endregion

                #region Catch block
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }
                #endregion

                #region Finally block
                finally {
                    this.Invoke((MethodInvoker)delegate () {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
                #endregion

            }) {
                Name = $"7chan thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        #endregion

        #region 8chan Download Logic: Works. (2022-08-18)

        // Needs:

        private void Set8chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://8chan.moe";
                string ThreadJSON = null;
                string CurrentURL = null;

                #region Try block
                try {

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                    #region API Download Logic
                    if (CurrentThread.Data.ThreadBoard == null || CurrentThread.Data.ThreadID == null) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        ManageThread(ThreadEvent.AfterDownload);
                        return;
                    }

                    CurrentURL = Networking.GetAPILink(CurrentThread);
                    if (!GetThreadJSON(CurrentURL, out ThreadJSON)) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        return;
                    }

                    // Serialize the json data into a class object.
                    EightChanThread ThreadData = ThreadJSON.JsonDeserialize<EightChanThread>();

                    // Reset CurrentURL to thread url.
                    // Because it's currently scanning
                    CurrentURL = CurrentThread.Data.ThreadURL;
                    #endregion

                    #region API Parsing logic
                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!CurrentThread.Data.RetrievedThreadName && !CurrentThread.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just fakken use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.subject,
                            Comment: ThreadData.message,
                            FallbackName: CurrentThread.Data.ThreadID
                        );
                        CurrentThread.Data.ThreadName = NewName;
                        CurrentThread.Data.RetrievedThreadName = true;
                        CurrentThread.Data.HtmlThreadNameSet = true;
                        this.Invoke((MethodInvoker)delegate () {
                            UpdateThreadName(true);
                        });
                    }

                    if (!CurrentThread.Data.RetrievedBoardName) {
                        CurrentThread.Data.BoardName = ThreadData.boardName;
                        CurrentThread.Data.BoardSubtitle = ThreadData.boardDescription;
                        CurrentThread.Data.RetrievedBoardName = CurrentThread.Data.BoardName != null;
                    }

                    if (!CurrentThread.ThreadHTMLPrepared) {
                        CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                        CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace("<title></title>", "<title> /" + CurrentThread.Data.ThreadBoard + "/ - " + CurrentThread.Data.ThreadName + " - 8chan</title>");
                        CurrentThread.ThreadHTMLPrepared = true;
                    }

                    // ThreadID is the post id too, strangely enough.
                    string PostID = ThreadData.threadId.ToString();
                    PostData CurrentPost;

                    // check for archive flag in the post.
                    CurrentThread.Data.ThreadArchived = ThreadData.archived ?? false;

                    // The first post is separate from the subsequent posts.
                    #region First post
                    if (!CurrentThread.Data.ParsedPostIDs.Contains(PostID)) {
                        CurrentPost = new();

                        CurrentPost.FirstPost = true;
                        CurrentPost.PostID = PostID;
                        try {
                            CurrentPost.PostDate = HtmlControl.GetReadableTime(EightChanThread.GetDateTime(ThreadData.creation));
                        }
                        catch {
                            CurrentPost.PostDate = ThreadData.creation;
                        }
                        CurrentPost.PosterName = ThreadData.name ?? "Anonymous";
                        CurrentPost.PostSubject = ThreadData.subject ?? string.Empty;
                        CurrentPost.PostMessage = EightChanThread.CleanseMessage(ThreadData.markdown, CurrentThread);

                        string FileID = PostID;
                        for (int ImageIndex = 0; ImageIndex < ThreadData.files.Length; ImageIndex++) {
                            if (ImageIndex > 0) {
                                FileID += "-" + (ImageIndex + 1).ToString();
                            }
                            string FileUrl = ThreadData.files[ImageIndex].path;
                            string FileHash = FileUrl[8..^4];
                            string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[^1];
                            string OriginalFileName = ThreadData.files[ImageIndex].originalName;
                            string FileName = GenerateFileName(FileUrl[8..^4], OriginalFileName, FileExtension);

                            CurrentThread.Data.FileExtensions.Add(FileExtension);
                            CurrentThread.Data.FileIDs.Add(FileID);
                            CurrentThread.Data.FileHashes.Add(FileHash);
                            CurrentThread.Data.ImageFiles.Add(FileBaseURL + FileUrl);
                            CurrentThread.Data.ThumbnailFiles.Add(FileBaseURL + ThreadData.files[ImageIndex].thumb);
                            CurrentThread.Data.ThumbnailNames.Add(ThreadData.files[ImageIndex].thumb[8..]);
                            CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);
                            CurrentThread.Data.FileOriginalNames.Add(OriginalFileName);
                            CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                            CurrentPost.Files.Add(new() {
                                ID = FileID,
                                OriginalName = OriginalFileName,
                                Extension = FileExtension,
                                GeneratedName = FileName + FileExtension,
                                Dimensions = new(ThreadData.files[ImageIndex].width, ThreadData.files[ImageIndex].height),
                                ThumbnailDimensions = HtmlControl.ResizeToThumbnail(ThreadData.files[ImageIndex].width, ThreadData.files[ImageIndex].height, true),
                                Size = ThreadData.files[ImageIndex].size
                            });

                            CurrentThread.Data.ThreadImagesCount++;

                            ListViewItem lvi = new();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = ThreadData.files[ImageIndex].path;
                            lvi.SubItems[0].Text = FileID;
                            lvi.SubItems[1].Text = FileExtension.Trim('.');
                            lvi.SubItems[2].Text = Path.GetFileNameWithoutExtension(ThreadData.files[ImageIndex].originalName);
                            lvi.SubItems[3].Text = FileHash;
                            lvi.ImageIndex = 0;
                            this.Invoke((MethodInvoker)delegate {
                                lvImages.Items.Add(lvi);
                            });
                        }

                        CurrentThread.ThreadHTML += HtmlControl.GetPostHtmlData(CurrentPost, CurrentThread);
                        CurrentThread.Data.ParsedPostIDs.Add(PostID);
                        CurrentThread.Data.ThreadPostsCount++;
                        CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                    }
                    #endregion

                    // And this part isn't as fun.
                    #region Subsequent posts
                    if (ThreadData.posts.Length > 0) {
                        EightChanThread.Post ReplyPost;
                        for (int CurrentReply = 0; CurrentReply < ThreadData.posts.Length; CurrentReply++) {
                            ReplyPost = ThreadData.posts[CurrentReply];
                            PostID = ReplyPost.postId.ToString();
                            if (!CurrentThread.Data.ParsedPostIDs.Contains(PostID)) {
                                CurrentPost = new();

                                CurrentPost.FirstPost = false;
                                CurrentPost.PostID = PostID;
                                try {
                                    CurrentPost.PostDate = HtmlControl.GetReadableTime(EightChanThread.GetDateTime(ReplyPost.creation));
                                }
                                catch {
                                    CurrentPost.PostDate = ReplyPost.creation;
                                }
                                CurrentPost.PosterName = ReplyPost.name ?? "Anonymous";
                                CurrentPost.PostSubject = ReplyPost.subject ?? string.Empty;
                                CurrentPost.PostMessage = EightChanThread.CleanseMessage(ReplyPost.markdown, CurrentThread);

                                if (ReplyPost.files.Length > 0) {
                                    string FileID = PostID;
                                    for (int ImageIndex = 0; ImageIndex < ReplyPost.files.Length; ImageIndex++) {
                                        if (ImageIndex > 0) {
                                            FileID += "-" + (ImageIndex + 1).ToString();
                                        }
                                        string FileUrl = ReplyPost.files[ImageIndex].path;
                                        string FileHash = FileUrl[8..^4];
                                        string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[^1];
                                        string OriginalFileName = ReplyPost.files[ImageIndex].originalName;
                                        string FileName = GenerateFileName(FileUrl[8..^4], OriginalFileName, FileExtension);

                                        CurrentThread.Data.FileExtensions.Add(FileExtension);
                                        CurrentThread.Data.FileIDs.Add(FileID);
                                        CurrentThread.Data.FileHashes.Add(FileHash);
                                        CurrentThread.Data.ImageFiles.Add(FileBaseURL + FileUrl);
                                        CurrentThread.Data.ThumbnailFiles.Add(FileBaseURL + ReplyPost.files[ImageIndex].thumb);
                                        CurrentThread.Data.ThumbnailNames.Add(ReplyPost.files[ImageIndex].thumb[8..]);
                                        CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);
                                        CurrentThread.Data.FileOriginalNames.Add(OriginalFileName);
                                        CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                        CurrentPost.Files.Add(new() {
                                            ID = FileID,
                                            OriginalName = OriginalFileName,
                                            Extension = FileExtension,
                                            GeneratedName = FileName + FileExtension,
                                            Dimensions = new(ThreadData.files[ImageIndex].width, ThreadData.files[ImageIndex].height),
                                            ThumbnailDimensions = HtmlControl.ResizeToThumbnail(ThreadData.files[ImageIndex].width, ThreadData.files[ImageIndex].height, false),
                                            Size = ReplyPost.files[ImageIndex].size
                                        });

                                        CurrentThread.Data.ThreadImagesCount++;

                                        ListViewItem lvi = new();
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.Name = ReplyPost.files[ImageIndex].path;
                                        lvi.SubItems[0].Text = FileID;
                                        lvi.SubItems[1].Text = FileExtension.Trim('.');
                                        lvi.SubItems[2].Text = Path.GetFileNameWithoutExtension(ReplyPost.files[ImageIndex].originalName);
                                        lvi.SubItems[3].Text = FileHash;
                                        lvi.ImageIndex = 0;
                                        this.Invoke((MethodInvoker)delegate {
                                            lvImages.Items.Add(lvi);
                                        });
                                    }
                                }

                                CurrentThread.ThreadHTML += HtmlControl.GetPostHtmlData(CurrentPost, CurrentThread);
                                CurrentThread.Data.ParsedPostIDs.Add(PostID);
                                CurrentThread.Data.ThreadPostsCount++;
                                CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                            }
                        }
                    }
                    #endregion

                    this.Invoke((MethodInvoker)delegate {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #endregion

                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    Debug.Print(CurrentURL);
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }
                #endregion

                #region Finally block
                finally {
                    this.Invoke((MethodInvoker)delegate () {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
                #endregion

            }) {
                Name = $"8chan thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        #endregion

        #region 8kun Download Logic: Works. (2022-01-25)

        // Needs:
        // HTML

        private void Set8kunThread() {
            DownloadThread = new Thread(() => {
                //string FileBaseURL_fpath = "https://media.8kun.top/file_store/";
                //string ThumbnailFileBaseURL_fpath = "https://media.8kun.top/file_store/thumb/";
                //string FileBaseURL = "https://media.8kun.top/{0}/src/{1}";
                //string ThumbnailFileBaseURL = "https://media.8kun.top/{0}/thumb/{1}.jpg";
                string FileBaseURL_fpath = "https://media.128ducks.com/file_store/";
                string ThumbnailFileBaseURL_fpath = "https://media.128ducks.com/file_store/thumb/";
                string FileBaseURL = "https://media.8kun.top/{0}/src/{1}";
                string ThumbnailFileBaseURL = "https://media.8kun.top/{0}/thumb/{1}.jpg";
                string ThreadJSON = null;
                string CurrentURL = null;

                #region Try block
                try {
                    // If the previous file wasn't 404, continue parsing.
                    // Otherwise, try downloading the file again.
                    // It will retry 5 times before skipping the file.

                    #region API Download Logic
                    // Check the thread board and id for null value
                    // Can't really parse the API without them.
                    if (CurrentThread.Data.ThreadBoard == null || CurrentThread.Data.ThreadID == null) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        ManageThread(ThreadEvent.AfterDownload);
                        return;
                    }

                    // Download the thread json from the api.
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;
                    CurrentURL = Networking.GetAPILink(CurrentThread);
                    if (!GetThreadJSON(CurrentURL, out ThreadJSON)) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        return;
                    }

                    // Reset CurrentURL to thread url.
                    // Because it's currently scanning
                    CurrentURL = CurrentThread.Data.ThreadURL;
                    #endregion

                    #region API Parsing Logic
                    EightKunThread Thread = ThreadJSON.JsonDeserialize<EightKunThread>();

                    // If the xmlPost count is less than 1, then it's not properly downloaded!
                    // can't really have a thread without posts.
                    if (Thread.posts.Length < 1) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                        return;
                    }

                    if (!CurrentThread.Data.RetrievedBoardName) {
                        var BoardArray = EightKunBoards.GetBoards();
                        EightKunBoards.Board Board = BoardArray.Where(x => x.uri == CurrentThread.Data.ThreadBoard).FirstOrDefault();

                        if (Board != null) {
                            CurrentThread.Data.BoardName = Board.title;
                            CurrentThread.Data.BoardSubtitle = Board.subtitle;
                        }
                        CurrentThread.Data.RetrievedBoardName = true;
                    }

                    if (!CurrentThread.ThreadHTMLPrepared) {
                        CurrentThread.ThreadHTML = HtmlControl.GetHTMLBase(CurrentThread);
                        CurrentThread.ThreadHTMLPrepared = true;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!CurrentThread.Data.RetrievedThreadName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just fakken use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = Thread.posts[0].com;
                        NewName = $"{Thread.posts[0].sub} - {Thread.posts[0].com}";
                                    //Regex.Replace(xmlPost[0].SelectNodes("com")[0].InnerText, "<.*?>", string.Empty);

                        if (NewName.Length > 64) {
                            NewName = NewName[..64];
                        }
                        CurrentThread.Data.ThreadName = WebUtility.HtmlDecode(NewName);

                        if (NewName.Length == 0) {
                            NewName = CurrentThread.Data.ThreadID;
                            CurrentThread.Data.ThreadName = CurrentThread.Data.ThreadURL;
                        }

                        CurrentThread.Data.RetrievedThreadName = true;
                        CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace("<title></title>", "<title> /" + CurrentThread.Data.ThreadBoard + "/ - " + NewName + " - 8kun</title>");
                        CurrentThread.Data.HtmlThreadNameSet = true;
                        this.Invoke((MethodInvoker)delegate () {
                            UpdateThreadName(true);
                        });
                    }

                    string PostID;
                    EightKunThread.Post Post;
                    // Start counting through the posts.
                    // Intentionally start at index 0 so it can check for posts that
                    // would have been skipped if a post gets deleted.
                    for (int PostIndex = 0; PostIndex < Thread.posts.Length; PostIndex++) {
                        Post = Thread.posts[PostIndex];
                        PostID = Post.no.ToString();
                        // Also checks if the parsed post ids contains the current post number,
                        // and parses it if it does NOT contain it.
                        // Useful for posts that got skipped on accident or an issue arose from
                        // parsing it.
                        if (!CurrentThread.Data.ParsedPostIDs.Contains(PostID)) {
                            PostData CurrentPost = new(); // post info for SaveHTML
                            CurrentPost.FirstPost = PostIndex == 0;

                            #region First image in a post
                            // Checks if there's a file in the post.
                            // If not, it'll skip straight to the SaveHTML portion.
                            if (Post.tim != null) {
                                string FileID = Post.tim; // The unique file id (NOT POST ID)
                                string OriginalFileName = Post.filename; // the original file name
                                string FileExtension = Post.ext; // the file extension
                                string ImageFile; // the full url of the file
                                string ThumbnailFile; // the full url of the thumbnail file
                                string FileHash = Post.md5; // the hash of the file

                                // determine which file path to use
                                switch (Post.fpath) {
                                    default: {
                                        ImageFile = FileBaseURL_fpath + FileID + FileExtension;
                                        ThumbnailFile = ThumbnailFileBaseURL_fpath + FileID + ".jpg";
                                    } break;
                                    case 0: {
                                        ImageFile = string.Format(FileBaseURL, CurrentThread.Data.ThreadBoard, FileID + FileExtension);
                                        ThumbnailFile = string.Format(ThumbnailFileBaseURL, CurrentThread.Data.ThreadBoard, FileID + ".jpg");
                                    } break;
                                }

                                // Add the file information to the lists
                                CurrentThread.Data.FileIDs.Add(FileID);
                                CurrentThread.Data.FileExtensions.Add(FileExtension);
                                CurrentThread.Data.ThumbnailFiles.Add(ThumbnailFile);
                                CurrentThread.Data.ImageFiles.Add(ImageFile);
                                CurrentThread.Data.FileHashes.Add(FileHash);
                                CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);

                                // Generate the file name
                                string FileName = FileID;
                                if (Config.Settings.Downloads.SaveOriginalFilenames) {
                                    FileName = OriginalFileName;
                                    string FileNamePrefix = ""; // the prefix for the file
                                    string FileNameSuffix = ""; // the suffix for the file

                                    // check for duplicates, and set the prefix to "(1)" if there is duplicates
                                    // (the space is intentional)
                                    if (Config.Settings.Downloads.PreventDuplicates) {
                                        if (CurrentThread.Data.FileOriginalNames.Contains(FileName)) {
                                            if (CurrentThread.Data.FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = CurrentThread.Data.FileNamesDupes.IndexOf(FileName);
                                                CurrentThread.Data.FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNamePrefix = "(" + CurrentThread.Data.FileNamesDupesCount[DupeNameIndex].ToString() + ") ";
                                            }
                                            else {
                                                CurrentThread.Data.FileNamesDupes.Add(FileName);
                                                CurrentThread.Data.FileNamesDupesCount.Add(1);
                                                FileNamePrefix = "(1) ";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileHandler.ReplaceIllegalCharacters(FileName) + FileNameSuffix;
                                }

                                // add more information for the files,
                                // the name, orignial name, thumbnail names, and image post ids
                                CurrentThread.Data.FileOriginalNames.Add(OriginalFileName);
                                CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                CurrentThread.Data.ThumbnailNames.Add(FileID + "s.jpg");
                                CurrentThread.Data.ImagePostIDs.Add(PostID);

                                // add info to the postinfo if SaveHTML is enabled
                                // the info is only available in this bracket, so it's gotta be
                                // added here, at the end.
                                CurrentPost.Files.Add(new() {
                                    ID = FileID,
                                    OriginalName = OriginalFileName,
                                    Extension = FileExtension,
                                    GeneratedName = FileName + FileExtension,
                                    Dimensions = new(Post.w ?? 0, Post.h ?? 0),
                                    ThumbnailDimensions = new(Post.tn_w ?? 0, Post.tn_h ?? 0),
                                    Size = Post.fsize ?? 0,
                                    Spoiled = Post.spoiler == 1,
                                });

                                // add a new listviewitem to the listview for this image.
                                ListViewItem lvi = new();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = FileID;
                                lvi.SubItems[0].Text = FileID;
                                lvi.SubItems[1].Text = FileExtension.Trim('.');
                                lvi.SubItems[2].Text = OriginalFileName;
                                lvi.SubItems[3].Text = FileHash;
                                lvi.ImageIndex = 0;
                                this.Invoke((MethodInvoker)delegate {
                                    lvImages.Items.Add(lvi);
                                });

                                CurrentThread.Data.ThreadImagesCount++;
                                CurrentThread.Data.ThreadPostsCount++;
                            }
                            #endregion

                            #region Extra Files
                            if (Post.extra_files is not null && Post.extra_files.Length > 0) {
                                EightKunThread.ExtraFile Extra;
                                for (int ExtraPostIndex = 0; ExtraPostIndex < Post.extra_files.Length; ExtraPostIndex++) {
                                    string ExtraID = $"{PostID}-{ExtraPostIndex + 1}";
                                    Extra = Post.extra_files[ExtraPostIndex];
                                    string FileID = Extra.tim; // The unique file id (NOT POST ID)
                                    string OriginalFileName = Extra.filename; // the original file name
                                    string FileExtension = Extra.ext; // the file extension
                                    string ImageFile; // the full url of the file
                                    string ThumbnailFile; // the full url of the thumbnail file
                                    string FileHash = Extra.md5; // the hash of the file

                                    // determine which file path to use
                                    switch (Extra.fpath) {
                                        default: {
                                            ImageFile = FileBaseURL_fpath + FileID + FileExtension;
                                            ThumbnailFile = ThumbnailFileBaseURL_fpath + FileID + ".jpg";
                                        } break;
                                        case 0: {
                                            ImageFile = string.Format(FileBaseURL, CurrentThread.Data.ThreadBoard, FileID + FileExtension);
                                            ThumbnailFile = string.Format(ThumbnailFileBaseURL, CurrentThread.Data.ThreadBoard, FileID + ".jpg");
                                        } break;
                                    }

                                    // Add the file information to the lists
                                    CurrentThread.Data.FileIDs.Add(FileID);
                                    CurrentThread.Data.FileExtensions.Add(FileExtension);
                                    CurrentThread.Data.ThumbnailFiles.Add(ThumbnailFile);
                                    CurrentThread.Data.ImageFiles.Add(ImageFile);
                                    CurrentThread.Data.FileHashes.Add(FileHash);
                                    CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);

                                    // Generate the file name
                                    string FileName = FileID;
                                    if (Config.Settings.Downloads.SaveOriginalFilenames) {
                                        FileName = OriginalFileName;
                                        string FileNamePrefix = ""; // the prefix for the file
                                        string FileNameSuffix = ""; // the suffix for the file

                                        // check for duplicates, and set the prefix to "(1)" if there is duplicates
                                        // (the space is intentional)
                                        if (Config.Settings.Downloads.PreventDuplicates) {
                                            if (CurrentThread.Data.FileOriginalNames.Contains(FileName)) {
                                                if (CurrentThread.Data.FileNamesDupes.Contains(FileName)) {
                                                    int DupeNameIndex = CurrentThread.Data.FileNamesDupes.IndexOf(FileName);
                                                    CurrentThread.Data.FileNamesDupesCount[DupeNameIndex] += 1;
                                                    FileNamePrefix = "(" + CurrentThread.Data.FileNamesDupesCount[DupeNameIndex].ToString() + ") ";
                                                }
                                                else {
                                                    CurrentThread.Data.FileNamesDupes.Add(FileName);
                                                    CurrentThread.Data.FileNamesDupesCount.Add(1);
                                                    FileNamePrefix = "(1) ";
                                                }
                                            }
                                        }

                                        FileName = FileNamePrefix + FileHandler.ReplaceIllegalCharacters(FileName) + FileNameSuffix;
                                    }

                                    // add more information for the files,
                                    // the name, orignial name, thumbnail names, and image post ids
                                    CurrentThread.Data.FileOriginalNames.Add(OriginalFileName);
                                    CurrentThread.Data.FileNames.Add(FileName + FileExtension);
                                    CurrentThread.Data.ThumbnailNames.Add(FileID + "s.jpg");
                                    CurrentThread.Data.ImagePostIDs.Add(ExtraID);

                                    // add info to the postinfo if SaveHTML is enabled
                                    // the info is only available in this bracket, so it's gotta be
                                    // added here, at the end.
                                    CurrentPost.Files.Add(new() {
                                        ID = FileID,
                                        OriginalName = OriginalFileName,
                                        Extension = FileExtension,
                                        GeneratedName = FileName + FileExtension,
                                        Dimensions = new(Extra.w, Extra.h),
                                        ThumbnailDimensions = new(Extra.tn_w, Extra.tn_h),
                                        Size = Extra.fsize,
                                        Spoiled = Extra.spoiler == 1,
                                    });

                                    // add a new listviewitem to the listview for this image.
                                    ListViewItem lvi = new();
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.Name = FileID;
                                    lvi.SubItems[0].Text = FileID;
                                    lvi.SubItems[1].Text = FileExtension.Trim('.');
                                    lvi.SubItems[2].Text = OriginalFileName;
                                    lvi.SubItems[3].Text = FileHash;
                                    lvi.ImageIndex = 0;
                                    this.Invoke((MethodInvoker)delegate {
                                        lvImages.Items.Add(lvi);
                                    });

                                    CurrentThread.Data.ThreadImagesCount++;

                                }
                            }
                            #endregion

                            CurrentPost.PostID = PostID;
                            CurrentPost.PostDate = HtmlControl.GetReadableTime(Post.time);
                            CurrentPost.PosterName = Post.name ?? "Anonymous";
                            CurrentPost.PostSubject = Post.sub ?? string.Empty;
                            CurrentPost.PostMessage = Post.com is null ? string.Empty : HtmlControl.ConvertHtmlTags(Post.com, ChanType.EightKun);

                            // add the new post to the html.
                            CurrentThread.ThreadHTML += HtmlControl.GetPostHtmlData(CurrentPost, CurrentThread);
                            CurrentThread.Data.ParsedPostIDs.Add(PostID);
                            CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                        }
                    }

                    // check for archive flag in the post.
                    //CurrentThread.Data.ThreadArchived = Thread.posts[0].locked > 0;

                    // update the form totals and status.
                    this.Invoke((MethodInvoker)delegate () {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbPostsParsed.Text = "posts parsed: " + CurrentThread.Data.ParsedPostIDs.Count.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #endregion

                #region Catch block
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }
                #endregion

                #region Finally block
                finally {
                    if (CurrentThread.Data.ThreadArchived) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    }
                    this.Invoke((MethodInvoker)delegate () {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
                #endregion

            }) {
                Name = $"8kun thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        #endregion

        #region fchan Download Logic: Works, very poorly. (2022-01-25)

        /* here's some information.
         * fchan is parsed using html, but even then it's inconsistent.
         * some file names have underscores after the file ID, or a period.
         * do people use fchan?
         * the main problem is, the regex will find the right lines, but I can only
         * guess the substrings to make it work... but it just sometimes doesn't work.
         * Unless fchan can implement an API, I'm not going to be updating this.
         * It will sometimes work, it sometimes won't.
         * That's all. I'm tired of fchan's HTML guessing game.
         */

        private void SetFchanThread() {
            DownloadThread = new Thread(() => {
                string BaseURL = "http://fchan.us/";
                string ThreadHTML = null;
                string CurrentURL = null;

                #region Try block
                try {

                    #region HTML Download Logic
                    if (CurrentThread.Data.ThreadBoard == null || CurrentThread.Data.ThreadID == null) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        return;
                    }

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                    CurrentURL = CurrentThread.Data.ThreadURL;
                    for (int TryCount = 0; TryCount < 5; TryCount++) {
                        if (!GetThreadHTML(CurrentURL, out ThreadHTML)) {
                            if (TryCount == 5) {
                                CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                                return;
                            }
                            Thread.Sleep(5000);
                        }
                        else {
                            break;
                        }
                    }

                    if (ThreadHTML == CurrentThread.LastThreadHTML) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                        return;
                    }

                    CurrentThread.LastThreadHTML = ThreadHTML;
                    #endregion

                    #region HTML Parsing logic
                    MatchCollection NameMatches = new Regex(ChanRegex.fchanNames).Matches(ThreadHTML);
                    MatchCollection PostIDMatches = new Regex(Chans.DefaultRegex.fchanIDs).Matches(ThreadHTML);
                    for (int PostMatchesIndex = 0; PostMatchesIndex < NameMatches.Count; PostMatchesIndex++) {
                        string IDMatch = PostIDMatches[PostMatchesIndex].Value;
                        string PostID = IDMatch[12..^7];
                        if (!CurrentThread.Data.FileIDs.Contains(PostID)) {
                            string NameMatch = NameMatches[PostMatchesIndex].Value;
                            string FileMatch = NameMatch[..NameMatch.IndexOf("\" rel=\"")];
                            int IndexOfFullFileName = NameMatch.IndexOf('>') + 1;

                            string FullFileName = FileMatch[5..];                       // file name saved on fchan
                            string FileExtension = "." + FullFileName.Split('.')[^1];   // file extension
                            string FileName = FullFileName[..^FileExtension.Length];    // file name w/o ext
                                                                                                                        //string OriginalFileName = NameMatch.Substring(IndexOfFullFileName);                   // original file name
                                                                                                                        //OriginalFileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);

                            CurrentThread.Data.FileIDs.Add(PostID);
                            CurrentThread.Data.FileOriginalNames.Add(PostID);
                            //CurrentThread.PostOriginalNames.Add(OriginalFileName);
                            CurrentThread.Data.FileExtensions.Add(FileExtension);
                            CurrentThread.Data.ImageFiles.Add(BaseURL + "/src/" + FullFileName.Trim('/'));
                            CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);

                            // I hate fchan, holy god I hate it so.
                            // Why can't they have regular locations for original file names
                            // killing myself.

                            //if (Config.Settings.Downloads.SaveCurrentThread.PostOriginalNames) {
                            //    FileName = OriginalFileName;
                            //    string FileNamePrefix = "";
                            //    string FileNameSuffix = "";

                            //    for (int IllegalCharacterIndex = 0; IllegalCharacterIndex < Chans.InvalidFileCharacters.Length; IllegalCharacterIndex++) {
                            //        FileName = FileName.Replace(Chans.InvalidFileCharacters[IllegalCharacterIndex], "_");
                            //    }

                            //    if (Config.Settings.Downloads.PreventDuplicates) {
                            //        if (FileNames.Contains(FileName)) {
                            //            if (FileNamesDupes.Contains(FileName)) {
                            //                int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                            //                FileNamesDupesCount[DupeNameIndex] += 1;
                            //                FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                            //            }
                            //            else {
                            //                FileNamesDupes.Add(FileName);
                            //                FileNamesDupesCount.Add(1);
                            //                FileNameSuffix = " (dupe 1)";
                            //            }
                            //        }
                            //    }

                            //    FileName = FileNamePrefix + FileName + FileNameSuffix;
                            //}

                            CurrentThread.Data.FileNames.Add(FileName + FileExtension);

                            if (Config.Settings.Downloads.SaveThumbnails) {
                                // trim the board name length + 14 for the image generated information before the 
                                string ThumbnailName = FullFileName[..(CurrentThread.Data.ThreadBoard.Length + 14)] + "s";
                                ThumbnailName += FullFileName[(CurrentThread.Data.ThreadBoard.Length + 14)..];
                                string ThumbnailLink = BaseURL + CurrentThread.Data.ThreadBoard + "/thumb/" + ThumbnailName[..^FileExtension.Length].Trim('/');
                                CurrentThread.Data.ThumbnailNames.Add(ThumbnailName);
                                CurrentThread.Data.ThumbnailFiles.Add(ThumbnailLink + ".jpg");

                                if (Config.Settings.Downloads.SaveHTML) {
                                    ThreadHTML = ThreadHTML.Replace("src=\"/" + CurrentThread.Data.ThreadBoard + "/thumb/" + ThumbnailName, "src=\"thumb/" + ThumbnailName);
                                }
                            }

                            if (Config.Settings.Downloads.SaveHTML) {
                                ThreadHTML = ThreadHTML.Replace("/src/" + CurrentThread.Data.ThreadBoard + "/" + FullFileName, FileName);
                            }

                            CurrentThread.Data.ThreadPostsCount++;

                            ListViewItem lvi = new();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = PostID;
                            lvi.SubItems[0].Text = PostID;
                            lvi.SubItems[1].Text = FileExtension;
                            lvi.SubItems[2].Text = FileName;
                            lvi.ImageIndex = 0;
                            this.Invoke((MethodInvoker)delegate {
                                lvImages.Items.Add(lvi);
                            });
                            CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                        }
                    }

                    this.Invoke((MethodInvoker)delegate {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #endregion

                #region Catch block
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }
                #endregion

                #region Finally block
                finally {
                    this.Invoke((MethodInvoker)delegate () {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
                #endregion

            }) {
                Name = $"fchan thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        #endregion

        #region u18chan Download Logic: Works. (2022-01-25)

        // Needs:
        // Fixed HTML replacement
        // HTML?

        private void Setu18ChanThread() {
            DownloadThread = new Thread(() => {
                string ThreadHTML = null;
                string CurrentURL = null;

                #region Try block
                try {

                    #region HTML Download Logic
                    if (CurrentThread.Data.ThreadBoard == null || CurrentThread.Data.ThreadID == null) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                        ManageThread(ThreadEvent.AfterDownload);
                        return;
                    }

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                    for (int TryCount = 0; TryCount < 5; TryCount++) {
                        CurrentURL = CurrentThread.Data.ThreadURL;

                        if (!GetThreadHTML(CurrentURL, out ThreadHTML)) {
                            if (TryCount == 5) {
                                CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                                return;
                            }
                            Thread.Sleep(5000);
                        }
                        else {
                            break;
                        }
                    }

                    if (ThreadHTML == CurrentThread.LastThreadHTML) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                        return;
                    }

                    CurrentThread.LastThreadHTML = ThreadHTML;
                    #endregion

                    #region HTML Parsing logic
                    // "why parse html using regex?"
                    // i dunno lol. works, don't it?
                    MatchCollection PostMatches = new Regex(ChanRegex.u18chanPosts).Matches(ThreadHTML);

                    if (PostMatches.Count == CurrentThread.Data.ParsedPostIDs.Count) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                        return;
                    }

                    for (int PostMatchesIndex = 0; PostMatchesIndex < PostMatches.Count; PostMatchesIndex++) {
                        if (PostMatches[PostMatchesIndex] != null) {
                            string MatchValue = PostMatches[PostMatchesIndex].Value;
                            int IndexOfTag = MatchValue.IndexOf('<');
                            string PostID = MatchValue[(IndexOfTag + 14)..][..8].Trim('_');
                            if (!CurrentThread.Data.FileIDs.Contains(PostID)) {
                                IndexOfTag = MatchValue.IndexOf('>');
                                string FileLink = MatchValue[..(IndexOfTag - 1)];

                                string FileName = FileLink.Split('/')[^1];
                                string FileExtension = "." + FileName.Split('.')[^1];
                                FileName = FileName[..^FileExtension.Length];

                                CurrentThread.Data.FileIDs.Add(PostID);
                                CurrentThread.Data.FileOriginalNames.Add(FileName);
                                CurrentThread.Data.FileExtensions.Add(FileExtension);
                                CurrentThread.Data.ImageFiles.Add(FileLink);
                                CurrentThread.Data.FileStatus.Add(FileDownloadStatus.Undownloaded);

                                if (Config.Settings.Downloads.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";

                                    do {
                                        FileName = FileName[0..^8];
                                    } while (FileName.EndsWith("_u18chan"));

                                    if (Config.Settings.Downloads.PreventDuplicates) {
                                        if (CurrentThread.Data.FileNames.Contains(FileName)) {
                                            if (CurrentThread.Data.FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = CurrentThread.Data.FileNamesDupes.IndexOf(FileName);
                                                CurrentThread.Data.FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNameSuffix = " (dupe " + CurrentThread.Data.FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                            }
                                            else {
                                                CurrentThread.Data.FileNamesDupes.Add(FileName);
                                                CurrentThread.Data.FileNamesDupesCount.Add(1);
                                                FileNameSuffix = " (dupe 1)";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileHandler.ReplaceIllegalCharacters(FileName) + FileNameSuffix;
                                }

                                CurrentThread.Data.FileNames.Add(FileName + FileExtension);

                                if (Config.Settings.Downloads.SaveThumbnails) {
                                    string ThumbnailName = FileName + "s";
                                    string ThumbnailLink = FileLink[0..^12] + "s_u18chan" + FileExtension;
                                    CurrentThread.Data.ThumbnailNames.Add(ThumbnailName + FileExtension);
                                    CurrentThread.Data.ThumbnailFiles.Add(ThumbnailLink);

                                    if (Config.Settings.Downloads.SaveHTML) {
                                        ThreadHTML = ThreadHTML.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=", "src=\"");
                                        ThreadHTML = ThreadHTML.Replace(ThumbnailLink, "thumb/" + ThumbnailLink.Split('/')[^1]);
                                    }
                                }

                                if (Config.Settings.Downloads.SaveHTML) {
                                    ThreadHTML = ThreadHTML.Replace(FileLink, FileName + FileExtension);
                                }

                                ListViewItem lvi = new();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = PostID;
                                lvi.SubItems[0].Text = PostID;
                                lvi.SubItems[1].Text = FileExtension;
                                lvi.SubItems[2].Text = FileName;
                                lvi.ImageIndex = 0;
                                this.Invoke((MethodInvoker)delegate {
                                    lvImages.Items.Add(lvi);
                                });

                                CurrentThread.Data.ThreadImagesCount++;
                                CurrentThread.Data.ThreadPostsCount++;
                                CurrentThread.AddedNewPosts = CurrentThread.ThreadModified = true;
                            }
                        }
                    }

                    this.Invoke((MethodInvoker)delegate {
                        lbNumberOfFiles.Text = $"number of files:  {CurrentThread.Data.DownloadedImagesCount} / {CurrentThread.Data.ThreadImagesCount}";
                        lbLastModified.Text = "last modified: " + CurrentThread.Data.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadIndex, ThreadStatus.ThreadDownloading);
                    });
                    #endregion

                    #region Download logic
                    //CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    //CurrentThread.DownloadingFiles = true;

                    //for (int ImageFilesIndex = CurrentThread.Data.DownloadedImagesCount; ImageFilesIndex < CurrentThread.Data.ImageFiles.Count; ImageFilesIndex++) {
                    //    this.Invoke((MethodInvoker)delegate {
                    //        lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                    //    });
                    //    CurrentURL = CurrentThread.Data.ImageFiles[ImageFilesIndex];

                    //    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                    //    if (Networking.DownloadFile(new(CurrentThread.Data.ImageFiles[ImageFilesIndex]), CurrentThread.Data.DownloadPath, CurrentThread.Data.FileNames[ImageFilesIndex])) {
                    //        CurrentThread.Data.DownloadedImagesCount++;

                    //        if (Config.Settings.Downloads.SaveThumbnails) {
                    //            CurrentURL = CurrentThread.Data.ThumbnailFiles[ImageFilesIndex];
                    //            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                    //            Networking.DownloadFile(new(CurrentThread.Data.ThumbnailFiles[ImageFilesIndex]), CurrentThread.Data.DownloadPath + "\\thumb", CurrentThread.Data.ThumbnailNames[ImageFilesIndex]);
                    //        }

                    //        this.Invoke((MethodInvoker)delegate {
                    //            lbDownloadedFiles.Text = CurrentThread.Data.DownloadedImagesCount.ToString();
                    //            lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                    //        });
                    //    }
                    //    else {
                    //        this.Invoke((MethodInvoker)delegate {
                    //            lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                    //        });
                    //    }

                    //    if (PauseBetweenFiles) { Thread.Sleep(100); }
                    //}

                    //if (Config.Settings.Downloads.SaveHTML) {
                    //    File.WriteAllText(CurrentThread.Data.DownloadPath + "\\Thread.html", ThreadHTML);
                    //}
                    //CurrentThread.DownloadingFiles = false;
                    #endregion

                    DownloadThreadFiles();
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #endregion

                #region Catch block
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                }
                #endregion

                #region Finally block
                finally {
                    this.Invoke((MethodInvoker)delegate () {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
                #endregion

            }) {
                Name = $"u18chan thread /{CurrentThread.Data.ThreadBoard}/{CurrentThread.Data.ThreadID}"
            };
        }

        #endregion

    }
}