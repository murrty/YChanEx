using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {

    public partial class frmMain : Form, IMainFom {

        #region Variables
        private readonly List<frmDownloader> Threads = new();   // The list of Thread download forms
        private readonly List<string> ThreadURLs = new();              // The list of Thread URLs

        private Thread UpdateCheck;
        private Thread ThreadLoader;            // The Thread for reloading saved threads

        private bool Icon404WasShown = false;           // Determines if the 404 icon has been shown on the tray.
        #endregion

        #region Usability methods
        /// <summary>
        /// Sets the thread status from another thread handle to change the status on the main form.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread to find the index in the listview.</param>
        /// <param name="Status">The new custom status to be set onto it.</param>
        public void SetItemStatus(int ThreadIndex, ThreadStatus Status) {
            switch (Status) {
                case ThreadStatus.ThreadRetrying:
                    Threads[ThreadIndex].CurrentThread.Data.OverallStatus = ThreadStatus.ThreadIsAlive;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Retrying";
                    break;
                case ThreadStatus.Waiting:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Finished scan";
                    break;
                case ThreadStatus.ThreadScanningSoon: {
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Scanning soon";
                } break;
                case ThreadStatus.ThreadNotModified:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " No new posts";
                    break;
                case ThreadStatus.ThreadScanning:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Scanning";
                    break;
                case ThreadStatus.ThreadDownloading:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Downloading";
                    break;
                case ThreadStatus.ThreadImproperlyDownloaded: {
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Bad download";
                } break;
                case ThreadStatus.ThreadIs404:
                    Threads[ThreadIndex].CurrentThread.Data.OverallStatus = Status;
                    niTray.BalloonTipText = $"{Threads[ThreadIndex].CurrentThread.Data.ThreadID} on /{Threads[ThreadIndex].CurrentThread.Data.ThreadBoard}/ has 404'd";
                    niTray.BalloonTipTitle = Threads[ThreadIndex].CurrentThread.Chan switch {
                        ChanType.FourChan => "4chan",
                        ChanType.FourTwentyChan => "420chan",
                        ChanType.SevenChan => "7chan",
                        ChanType.EightChan => "8chan",
                        ChanType.EightKun => "8kun",
                        ChanType.fchan => "fchan",
                        ChanType.u18chan => "u18chan",
                        _ => "Unknown chan",
                    };
                    if (changeTray.Enabled) {
                        changeTray.Stop();
                    }
                    niTray.Icon = Properties.Resources.YChanEx404;
                    Icon404WasShown = true;
                    changeTray.Start();
                    niTray.ShowBalloonTip(5000);
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "404'd";
                    break;
                case ThreadStatus.ThreadIsAborted:
                    Threads[ThreadIndex].CurrentThread.Data.OverallStatus = Status;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Aborted";
                    break;
                case ThreadStatus.ThreadIsArchived:
                    Threads[ThreadIndex].CurrentThread.Data.OverallStatus = Status;
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Archived";
                    break;
                case ThreadStatus.ThreadReloaded:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Reloaded";
                    break;
                case ThreadStatus.ThreadUpdateName:
                    lvThreads.Items[ThreadIndex].SubItems[clName.Index].Text = Threads[ThreadIndex].CurrentThread.Data.RetrievedThreadName ? Threads[ThreadIndex].CurrentThread.Data.ThreadName : Threads[ThreadIndex].CurrentThread.Data.ThreadURL;
                    break;
                default:
                    lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Unknown?";
                    break;
            }
            GC.Collect();
        }

        /// <summary>
        /// Removes the thread if it was killed (archived or 404)
        /// </summary>
        /// <param name="Thread"></param>
        public void ThreadKilled(ThreadInfo Thread) => RemoveThread(Thread);

        /// <summary>
        /// Checks the threads from the saved queue, and the arguments.
        /// </summary>
        private void CheckThreads() {
            if (!Program.DebugMode || Program.LoadThreadsInDebug) {
                ThreadLoader = new Thread(() => {
                    try {
                        if (Config.Settings.General.SaveQueueOnExit) {
                            var SavedThreads = new List<ThreadData>().LoadThreads(out List<string> FilePaths);
                            if (SavedThreads.Count > 0) {
                                for (int i = 0; i < SavedThreads.Count; i++) {
                                    try {
                                        this.Invoke(() => {
                                            AddNewThread(SavedThreads[i], FilePaths[i]);
                                        });
                                    }
                                    catch (Exception ex) {
                                        murrty.classes.Log.ReportException(ex);
                                    }
                                }
                            }
                        }

                        if (Arguments.URLs.Count > 0) {
                            for (int i = 0; i < Arguments.URLs.Count; i++) {
                                if (Arguments.URLs[i].ToLower().StartsWith("ychanex:")) {
                                    this.Invoke(() => {
                                        AddNewThread(Arguments.URLs[i], true);
                                    });
                                }
                            }
                        }
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception ex) {
                        murrty.classes.Log.ReportException(ex);
                    }
                }) {
                    Name = "Saved threads reloader"
                };
                ThreadLoader.Start();
            }
        }

        /// <summary>
        /// Saves all the current threads.
        /// </summary>
        private void SaveThreads() {
            List<ThreadInfo> Data = new();
            if (ThreadURLs.Count > 0) {
                for (int i = 0; i < ThreadURLs.Count; i++) {
                    Data.Add(Threads[i].CurrentThread);
                }
                Data.SaveThreads();
            }
        }

        /// <summary>
        /// Adds a new thread to the queue via URL.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread that will be added to the queue.</param>
        /// <returns>Boolean based on it's success</returns>
        private bool AddNewThread(string ThreadURL, bool FromTrayOrArgument = false) {
            if (ThreadURL.StartsWith("ychanex:")) { ThreadURL = ThreadURL[8..]; }
            if (ThreadURL.StartsWith("view-source:")) { ThreadURL = ThreadURL[12..]; }

            ThreadURL = Networking.CleanURL(ThreadURL);

            if (Chans.SupportedChan(ThreadURL, out ChanType ReceivedType)) {
                if (ThreadURLs.Contains(ThreadURL)) {
                    int ThreadURLIndex = ThreadURLs.IndexOf(ThreadURL);
                    if (Threads[ThreadURLIndex].CurrentThread.Data.OverallStatus != ThreadStatus.ThreadIsAlive) {
                        Threads[ThreadURLIndex].ManageThread(ThreadEvent.RetryDownload);
                        niTray.BalloonTipTitle = "Already in queue";
                        niTray.BalloonTipText = $"Restarting {ThreadURL}";
                        niTray.BalloonTipIcon = ToolTipIcon.Info;
                    }
                    else {
                        niTray.BalloonTipTitle = "Already in queue";
                        niTray.BalloonTipText = $"{ThreadURL} is already downloading.";
                        niTray.BalloonTipIcon = ToolTipIcon.Info;
                    }
                    niTray.ShowBalloonTip(5000);
                    return true;
                }
                else {
                    switch (ReceivedType) {
                        case ChanType.fchan:
                            if (!Config.Settings.Downloads.fchanWarning) {
                                MessageBox.Show(
                                    "fchan works, but isn't supported. I'm keeping this in for people, but here's your only warning: I will not help with any issues regarding fchan, and they will not be acknowledged.\n\n" +
                                    "The reason I'm not going to continue working on fchan is because of all the logic shenanigans I have to do to get files, and even then it's still not perfect for some files.\n\n\n" +
                                    "I might fix it and update it later, but I'm not going to touch it anymore. You're on your own with it.\n\n" +
                                    "This is the only time this warning will appear.");
                                Config.Settings.Downloads.fchanWarning = true;
                            }
                            break;

                        case ChanType.EightKun:
                        case ChanType.EightChan: {
                            ThreadURL = ThreadURL.Replace($".json", $".html");
                        } break;

                        case ChanType.Unsupported:
                            return false;
                    }

                    ThreadURLs.Add(ThreadURL);
                    ThreadInfo NewInfo = new(ReceivedType);
                    NewInfo.Data.ThreadURL = ThreadURL;
                    NewInfo.Data.OverallStatus = ThreadStatus.ThreadIsAlive;
                    NewInfo.ThreadIndex = ThreadURLs.Count - 1;

                    frmDownloader newThread = new(this);
                    newThread.Name = ThreadURL;
                    newThread.CurrentThread = NewInfo;
                    newThread.ManageThread(ThreadEvent.ParseForInfo);
                    newThread.Show();

                    ListViewItem lvi = new();
                    lvi.Name = ThreadURL;
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.ImageIndex = (int)NewInfo.Chan;
                    lvi.SubItems[clStatus.Index].Text = " Creating";
                    lvi.SubItems[clThread.Index].Text = $"/{NewInfo.Data.ThreadBoard}/ - {NewInfo.Data.ThreadID}";
                    lvi.SubItems[clName.Index].Text = ThreadURL;

                    lvThreads.Items.Add(lvi);

                    Threads.Add(newThread);

                    if (chkCreateThreadInTheBackground.Checked) {
                        newThread.Hide();
                    }

                    if (FromTrayOrArgument) {
                        newThread.Hide();
                        niTray.BalloonTipTitle = "Thread added to queue";
                        niTray.BalloonTipText = $"Added /{NewInfo.Data.ThreadBoard}/{NewInfo.Data.ThreadID}/ to the queue";
                        niTray.BalloonTipIcon = ToolTipIcon.Info;
                        niTray.ShowBalloonTip(5000);
                    }

                    newThread.Opacity = 100;
                    newThread.ShowInTaskbar = true;

                    newThread.ManageThread(ThreadEvent.StartDownload);
                    if (Config.Settings.General.AutoSaveThreads) {
                        SaveThreads();
                    }

                    if (Config.Settings.General.SaveThreadHistory && !DownloadHistory.Contains(ReceivedType, ThreadURL)) {
                        DownloadHistory.Add(ReceivedType, ThreadURL);
                        switch (ReceivedType) {
                            case ChanType.FourChan: {
                                tvHistory.Nodes[0].Nodes.Add(ThreadURL, ThreadURL);
                            } break;

                            case ChanType.FourTwentyChan: {
                                tvHistory.Nodes[1].Nodes.Add(ThreadURL, ThreadURL);
                            } break;

                            case ChanType.SevenChan: {
                                tvHistory.Nodes[2].Nodes.Add(ThreadURL, ThreadURL);
                            } break;

                            case ChanType.EightChan: {
                                tvHistory.Nodes[3].Nodes.Add(ThreadURL, ThreadURL);
                            } break;

                            case ChanType.EightKun: {
                                tvHistory.Nodes[4].Nodes.Add(ThreadURL, ThreadURL);
                            } break;

                            case ChanType.fchan: {
                                tvHistory.Nodes[5].Nodes.Add(ThreadURL, ThreadURL);
                            } break;

                            case ChanType.u18chan: {
                                tvHistory.Nodes[6].Nodes.Add(ThreadURL, ThreadURL);
                            } break;
                        }
                        DownloadHistory.Save();
                    }

                    niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";
                    return true;
                }
            }
            else {
                if (FromTrayOrArgument) {
                    niTray.BalloonTipText = $"The url is not valid: {ThreadURL}";
                    niTray.BalloonTipIcon = ToolTipIcon.Error;
                    niTray.ShowBalloonTip(5000);
                }
                return false;
            }
        }

        /// <summary>
        /// Adds a new thread to the queue via saved threads.
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        private bool AddNewThread(ThreadData Info, string InfoPath) {
            Info.ThreadURL = Networking.CleanURL(Info.ThreadURL);
            if (Chans.SupportedChan(Info.ThreadURL, out ChanType ReceivedType) && !ThreadURLs.Contains(Info.ThreadURL)) {
                switch (ReceivedType) {
                    case ChanType.Unsupported: return false;
                }

                ThreadURLs.Add(Info.ThreadURL);
                ThreadInfo NewInfo = new(Info, ReceivedType) {
                    ThreadIndex = ThreadURLs.Count - 1
                };
                NewInfo.SavedThreadJson = InfoPath;
                NewInfo.UpdateJsonPath();
                frmDownloader newThread = new(this);
                newThread.Name = Info.ThreadURL;
                newThread.CurrentThread = NewInfo;
                newThread.Show();
                newThread.Hide();
                newThread.ManageThread(ThreadEvent.ReloadThread);

                ListViewItem lvi = new() {
                    Name = Info.ThreadURL
                };
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                lvi.ImageIndex = (int)NewInfo.Chan;
                lvi.SubItems[clStatus.Index].Text = " Creating";
                lvi.SubItems[clThread.Index].Text = $"/{NewInfo.Data.ThreadBoard}/ - {NewInfo.Data.ThreadID}";
                lvThreads.Items.Add(lvi);

                newThread.Opacity = 100;
                newThread.ShowInTaskbar = true;

                Threads.Add(newThread);
                if (Info.SetCustomName) {
                    lvi.SubItems[clName.Index].Text = Info.CustomThreadName;
                }
                else {
                    if (Info.RetrievedThreadName) {
                        lvi.SubItems[clName.Index].Text = Info.ThreadName;
                    }
                    else {
                        lvi.SubItems[clName.Index].Text = Info.ThreadURL;
                    }
                }
                newThread.ManageThread(ThreadEvent.StartDownload);

                niTray.Text = "YChanEx - " + Threads.Count + " threads";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a thread from the queue.
        /// </summary>
        /// <param name="ThreadIndex">The index of the thread to remove.</param>
        /// <param name="DontAutoSave">Optional, if the AutoSaveThreads option will be skipped entirely.</param>
        /// <returns>True if the thread was removed; otherwise, false.</returns>
        private bool RemoveThread(ThreadInfo Thread) {
            try {
                if (Thread.ThreadIndex > -1 && Thread.ThreadIndex < ThreadURLs.Count) {

                    Config.Settings.Saved.DownloadFormSize = Threads[Thread.ThreadIndex].Size;

                    if (Threads[Thread.ThreadIndex].CurrentThread.Data.OverallStatus != ThreadStatus.ThreadIsAlive) {
                        Threads[Thread.ThreadIndex].ManageThread(ThreadEvent.AbortDownload);
                    }

                    if (System.IO.File.Exists(Thread.SavedThreadJson))
                        System.IO.File.Delete(Thread.SavedThreadJson);

                    Threads[Thread.ThreadIndex].Dispose();
                    Threads.RemoveAt(Thread.ThreadIndex);
                    ThreadURLs.RemoveAt(Thread.ThreadIndex);
                    lvThreads.Items.RemoveAt(Thread.ThreadIndex);

                    if (Threads.Count > 0) {
                        for (int i = Thread.ThreadIndex; i < Threads.Count; i++) {
                            Threads[i].CurrentThread.ThreadIndex--;
                            Threads[i].CurrentThread.UpdateJsonPath();
                        }
                    }

                    niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";

                    return true;
                }
                else return false;
            }
            catch { throw; }
        }

        /// <summary>
        /// The method to determine if the application should exit, while also performing last-second saving for threads and form location/size.
        /// </summary>
        /// <returns>True, if the application is allowed to exit. False, if the application isn't allowed to exit.</returns>
        private bool ApplicationCanExit() {
            if (this.Visible && Config.Settings.General.MinimizeInsteadOfExiting) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
                niTray.Visible = true;
                return false;
            }

            if (Config.Settings.General.SaveQueueOnExit && (!Program.DebugMode || Program.LoadThreadsInDebug)) {
                SaveThreads();
            }

            if (ThreadLoader is not null && ThreadLoader.IsAlive)
                ThreadLoader.Abort();

            if (UpdateCheck is not null && UpdateCheck.IsAlive)
                UpdateCheck.Abort();

            Config.Settings.Saved.MainFormLocation = this.Location;
            Config.Settings.Saved.MainFormSize = this.Size;
            Config.Settings.Saved.CreateThreadInTheBackground = chkCreateThreadInTheBackground.Checked;
            Config.Settings.Saved.MainFormColumnSizes = lvThreads.GetColumnWidths();

            for (int i = 0; i < Threads.Count; i++) {
                Threads[i].ManageThread(ThreadEvent.AbortForClosing);
                Threads[i].Dispose();
            }
            ThreadURLs.Clear();
            Threads.Clear();
            niTray.Visible = false;

            return true;
        }
        #endregion

        #region Form Controls
        [System.Diagnostics.DebuggerStepThrough]
        protected override void WndProc(ref Message m) {
            switch (m.Msg) {
                case CopyData.WM_COPYDATA: {
                    try {
                        try {
                            var Data = CopyData.GetParam<SentData>(m.LParam);
                            string[] ReceivedArguments = Data.Argument.Split('|');
                            for (int i = 0; i < ReceivedArguments.Length; i++) {
                                AddNewThread(ReceivedArguments[i], true);
                            }
                            m.Result = IntPtr.Zero;
                        }
                        catch {
                            m.Result = (IntPtr)1;
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.ToString());
                    }
                } break;

                case CopyData.WM_SHOWFORM: {
                    if (this.WindowState != FormWindowState.Normal)
                        this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.Activate();
                    niTray.Visible = Config.Settings.General.ShowTrayIcon;
                    m.Result = IntPtr.Zero;
                } break;

                default: {
                    base.WndProc(ref m);
                } break;
            }
        }
        public frmMain() {
            InitializeComponent();
            ilIcons.Images.Add(Properties.Resources._4chan);
            ilIcons.Images.Add(Properties.Resources._420chan);
            ilIcons.Images.Add(Properties.Resources._7chan);
            ilIcons.Images.Add(Properties.Resources._8chan);
            ilIcons.Images.Add(Properties.Resources._8kun);
            ilIcons.Images.Add(Properties.Resources._fchan);
            ilIcons.Images.Add(Properties.Resources._u18chan);
            lvThreads.SmallImageList = ilIcons;
            lvThreads.ContextMenu = cmThreads;

#if !DEBUG
            tpDebug.Dispose();
#endif
        }
        private void frmMain_Load(object sender, EventArgs e) {
            if (Config.Settings.General.ShowTrayIcon && !Program.DebugMode)
                niTray.Visible = true;
            niTray.ContextMenu = cmTray;

            if (Config.ValidSize(Config.Settings.Saved.MainFormSize))
                this.Size = Config.Settings.Saved.MainFormSize;
            if (Config.ValidPoint(Config.Settings.Saved.MainFormLocation))
                this.Location = Config.Settings.Saved.MainFormLocation;
            if (!string.IsNullOrEmpty(Config.Settings.Saved.MainFormColumnSizes))
                lvThreads.SetColumnWidths(Config.Settings.Saved.MainFormColumnSizes);

            if (DownloadHistory.Count > 0) {
                string[] Data = DownloadHistory.History;
                for (int i = 0; i < DownloadHistory.Data.FourChanHistory.Count; i++)
                    tvHistory.Nodes[0].Nodes.Add(DownloadHistory.Data.FourChanHistory[i], DownloadHistory.Data.FourChanHistory[i]);
                for (int i = 0; i < DownloadHistory.Data.FourTwentyChanHistory.Count; i++)
                    tvHistory.Nodes[1].Nodes.Add(DownloadHistory.Data.FourTwentyChanHistory[i], DownloadHistory.Data.FourTwentyChanHistory[i]);
                for (int i = 0; i < DownloadHistory.Data.SevenChanHistory.Count; i++)
                    tvHistory.Nodes[2].Nodes.Add(DownloadHistory.Data.SevenChanHistory[i], DownloadHistory.Data.SevenChanHistory[i]);
                for (int i = 0; i < DownloadHistory.Data.EightChanHistory.Count; i++)
                    tvHistory.Nodes[3].Nodes.Add(DownloadHistory.Data.EightChanHistory[i], DownloadHistory.Data.EightChanHistory[i]);
                for (int i = 0; i < DownloadHistory.Data.EightKunHistory.Count; i++)
                    tvHistory.Nodes[4].Nodes.Add(DownloadHistory.Data.EightKunHistory[i], DownloadHistory.Data.EightKunHistory[i]);
                for (int i = 0; i < DownloadHistory.Data.FchanHistory.Count; i++)
                    tvHistory.Nodes[5].Nodes.Add(DownloadHistory.Data.FchanHistory[i], DownloadHistory.Data.FchanHistory[i]);
                for (int i = 0; i < DownloadHistory.Data.u18chanHistory.Count; i++)
                    tvHistory.Nodes[6].Nodes.Add(DownloadHistory.Data.u18chanHistory[i], DownloadHistory.Data.u18chanHistory[i]);
            }

            chkCreateThreadInTheBackground.Checked = Config.Settings.Saved.CreateThreadInTheBackground;

            if (Config.Settings.General.EnableUpdates && !Program.DebugMode) {
                UpdateCheck = new(() => {
                    UpdateChecker.CheckForUpdate(false);
                }) {
                    IsBackground = true,
                    Name = "Startup update checker"
                };
                UpdateCheck.Start();
            }
        }
        private void frmMain_Shown(object sender, EventArgs e) {
            CheckThreads();
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            if (Config.Settings.General.MinimizeInsteadOfExiting && this.Visible) {
                this.Hide();
                niTray.Visible = true;
                e.Cancel = true;
            }
            else if (!ApplicationCanExit()) {
                e.Cancel = true;
            }
        }
        private void frmMain_SizeChanged(object sender, EventArgs e) {
            if (this.WindowState == FormWindowState.Minimized && Config.Settings.General.MinimizeToTray) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
                niTray.Visible = true;
            }
        }
        private void mSettings_Click(object sender, EventArgs e) {
            frmSettings Settings = new();
            Settings.ShowDialog();
            Settings.Dispose();
            Program.SettingsOpen = false;

            if (Config.Settings.General.ShowTrayIcon) {
                niTray.Visible = true;
            }
            else {
                niTray.Visible = false;
            }

            if (Threads.Count > 0) {
                for (int Thread = 0; Thread < Threads.Count; Thread++) {
                    Threads[Thread].UpdateThreadName();
                }
            }
        }
        private void mAbout_Click(object sender, EventArgs e) {
            frmAbout About = new();
            About.ShowDialog();
        }
        private void txtThreadURL_KeyPress(object sender, KeyPressEventArgs e) {
            switch (e.KeyChar) {
                case (char)Keys.Enter:
                    if (AddNewThread(txtThreadURL.Text)) {
                        txtThreadURL.Clear();
                    }
                    e.Handled = true;
                    break;
            }
        }
        private void btnAdd_Click(object sender, EventArgs e) {
            if (AddNewThread(txtThreadURL.Text)) {
                txtThreadURL.Clear();
            }
        }
        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                Threads[lvThreads.SelectedIndices[0]].Show();
                Threads[lvThreads.SelectedIndices[0]].Activate();
            }
        }
        private void cmThreads_Popup(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                mStatus.Enabled = true;

                if (lvThreads.SelectedIndices.Count == 1) {
                    mSetCustomName.Enabled = true;
                    if (Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.OverallStatus != ThreadStatus.ThreadIsAlive) {
                        mRetryDownload.Enabled = true;
                    }
                    else {
                        mRetryDownload.Enabled = false;
                    }
                }
                else {
                    mRetryDownload.Enabled = false;
                    mSetCustomName.Enabled = false;
                }

                mOpenDownloadFolder.Enabled = true;
                mOpenThreadInBrowser.Enabled = true;
                mCopyThreadURL.Enabled = true;
                mCopyThreadID.Enabled = true;

                mRemove.Enabled = true;
            }
            else {
                mStatus.Enabled = false;
                mRetryDownload.Enabled = false;
                mSetCustomName.Enabled = false;

                mOpenDownloadFolder.Enabled = false;
                mOpenThreadInBrowser.Enabled = false;
                mCopyThreadURL.Enabled = false;
                mCopyThreadID.Enabled = false;

                mRemove.Enabled = false;
            }
        }
        private void changeTray_Tick(object sender, EventArgs e) {
            if (Icon404WasShown) {
                Icon404WasShown = false;
            }
            else {
                niTray.Icon = Properties.Resources.YChanEx;
                changeTray.Stop();
            }
        }
        private void niTray_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (!this.Visible) {
                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.Activate();
                if (!Config.Settings.General.ShowTrayIcon) {
                    niTray.Visible = false;
                }
            }
        }
        #endregion

        #region cmThreads Controls
        private void mStatus_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    Threads[lvThreads.SelectedIndices[CurrentThread]].Show();
                }
            }
        }
        private void mRetryDownload_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedItems.Count == 1) {
                if (Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.OverallStatus != ThreadStatus.ThreadIsAlive) {
                    Threads[lvThreads.SelectedIndices[0]].ManageThread(ThreadEvent.RetryDownload);
                    mRetryDownload.Enabled = false;
                }
            }
        }
        private void mSetCustomName_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                frmNewName newName = new();
                newName.txtNewName.Text = lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text;
                switch (newName.ShowDialog()) {
                    case DialogResult.OK:
                        lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = newName.txtNewName.Text;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.SetCustomName = true;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.CustomThreadName = newName.txtNewName.Text;
                        break;
                    case DialogResult.No:
                        lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.ThreadName;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.SetCustomName = false;
                        Threads[lvThreads.SelectedIndices[0]].CurrentThread.Data.CustomThreadName = null;
                        break;
                }
                newName.Dispose();
            }
            //using (Form newName = new Form())
            //using (TextBox threadName = new TextBox())
            //using (Button cancelName = new Button())
            //using (Button setName = new Button()){
            //    newName.FormBorderStyle = FormBorderStyle.FixedSingle;
            //    newName.MaximizeBox = false;
            //    newName.MinimizeBox = false;
            //    newName.Name = "NewName-" + ThreadDownloadForms[lvThreads.SelectedIndices[0]].ThreadID;
            //    newName.Size = new System.Drawing.Size(300, 130);
            //    newName.StartPosition = FormStartPosition.CenterScreen;
            //    newName.Text = "Set thread name";

            //    threadName.Location = new System.Drawing.Point(12, 20);
            //    threadName.Name = "txtThreadName";
            //    threadName.Size = new System.Drawing.Size(258, 20);
            //    newName.Controls.Add(threadName);

            //    cancelName.Click += (s, arg) => {
            //        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //    };
            //    cancelName.Location = new System.Drawing.Point(114, 58);
            //    cancelName.Name = "btnCancel";
            //    cancelName.Size = new System.Drawing.Size(75, 23);
            //    cancelName.Text = "Cancel";
            //    newName.Controls.Add(cancelName);

            //    setName.Click += (s, arg) => {
            //        this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //    };
            //    setName.Location = new System.Drawing.Point(195, 58);
            //    setName.Name = "btnSetName";
            //    setName.Size = new System.Drawing.Size(75, 23);
            //    setName.Text = "Set name";
            //    newName.Controls.Add(setName);

            //    switch (newName.ShowDialog()) {
            //        case System.Windows.Forms.DialogResult.OK:
            //            lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = threadName.Text;
            //            break;
            //    }
            //}
        }
        private void mOpenDownloadFolder_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    string FoundThreadDownloadPath = Threads[lvThreads.SelectedIndices[CurrentThread]].CurrentThread.Data.DownloadPath;
                    if (!string.IsNullOrEmpty(FoundThreadDownloadPath)) {
                        if (System.IO.Directory.Exists(FoundThreadDownloadPath)) {
                            System.Diagnostics.Process.Start(FoundThreadDownloadPath);
                        }
                    }
                }
            }
        }
        private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    System.Diagnostics.Process.Start(ThreadURLs[lvThreads.SelectedIndices[CurrentThread]]);
                }
            }
        }
        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                string ClipboardBuffer = string.Empty;
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    ClipboardBuffer += ThreadURLs[lvThreads.SelectedIndices[CurrentThread]] + "\r\n";
                }
                ClipboardBuffer = ClipboardBuffer.Trim('\n').Trim('\r');
                Clipboard.SetText(ClipboardBuffer);
            }
        }
        private void mCopyThreadID_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                string ThreadIDBuffer = string.Empty;
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    ThreadIDBuffer += Threads[lvThreads.SelectedIndices[CurrentThread]].CurrentThread.Data.ThreadID;
                }
                ThreadIDBuffer = ThreadIDBuffer.Trim('\n').Trim('\r');
                if (!string.IsNullOrEmpty(ThreadIDBuffer)) {
                    Clipboard.SetText(ThreadIDBuffer);
                }
            }
        }
        private void mRemove_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {

                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    RemoveThread(Threads[lvThreads.SelectedIndices[CurrentThread]].CurrentThread);
                }

                if (Config.Settings.General.AutoSaveThreads) SaveThreads();

                niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";
            }
        }
        #endregion

        #region cmTray Controls
        private void mTrayShowYChanEx_Click(object sender, EventArgs e) {
            if (this.Visible == false) {
                this.WindowState = FormWindowState.Normal;
                this.Show();
                this.Activate();
                if (!Config.Settings.General.ShowTrayIcon) {
                    niTray.Visible = false;
                }
            }
        }
        private void mAddThread_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText() && AddNewThread(Clipboard.GetText(), true)) {

            }
        }
        private void mTrayExit_Click(object sender, EventArgs e) {
            if (ApplicationCanExit()) {
                this.Dispose();
            }
        }
        #endregion

        #region History
        private void tvHistory_AfterSelect(object sender, TreeViewEventArgs e) {
            btnHistoryRedownload.Enabled = btnHistoryRemove.Enabled = tvHistory.SelectedNode.Parent != null;
        }

        private void btnHistoryRedownload_Click(object sender, EventArgs e) {
            if (tvHistory.SelectedNode.Parent != null) {
                AddNewThread(tvHistory.SelectedNode.Name, false);
            }
        }

        private void btnHistoryRemove_Click(object sender, EventArgs e) {
            if (tvHistory.SelectedNode.Parent != null) {
                DownloadHistory.Remove(tvHistory.SelectedNode.Name);
                tvHistory.SelectedNode.Remove();
                DownloadHistory.Save();
            }
        }

        private void btnHistoryClear_Click(object sender, EventArgs e) {
            DownloadHistory.Clear();
            tvHistory.Nodes[0].Nodes.Clear();
            tvHistory.Nodes[1].Nodes.Clear();
            tvHistory.Nodes[2].Nodes.Clear();
            tvHistory.Nodes[3].Nodes.Clear();
            tvHistory.Nodes[4].Nodes.Clear();
            tvHistory.Nodes[5].Nodes.Clear();
            tvHistory.Nodes[6].Nodes.Clear();
        }
        #endregion

    }

}