#nullable enable
namespace YChanEx;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
public partial class frmMain : Form, IMainFom {
    #region Variables
    private readonly List<frmDownloader> Threads = [];  // The list of Thread download forms
    private readonly List<string> ThreadURLs = [];      // The list of Thread URLs

    private Thread? UpdateCheck;
    private Thread? ThreadLoader;   // The Thread for reloading saved threads

    private bool Icon404WasShown;   // Determines if the 404 icon has been shown on the tray.
    private bool TrayExit;          // Whether the tray option to exit was used.
    #endregion

    #region Usability methods
    /// <summary>
    /// Sets the thread status from another thread handle to change the status on the main form.
    /// </summary>
    /// <param name="ThreadIndex">The index of the thread in the list.</param>
    /// <param name="Status">The new custom status to be set onto it.</param>
    public void SetItemStatus(int ThreadIndex, ThreadStatus Status) {
        switch (Status) {
            case ThreadStatus.Waiting: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Finished scan";
            } break;
            case ThreadStatus.ThreadScanning: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Scanning";
            } break;
            case ThreadStatus.ThreadDownloading: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Downloading";
            } break;
            case ThreadStatus.ThreadNotModified: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " No new posts";
            } break;
            case ThreadStatus.ThreadIsNotAllowed: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Fobridden (403)";
            } break;
            case ThreadStatus.ThreadReloaded: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Reloaded";
            } break;

            case ThreadStatus.ThreadIs404: {
                Threads[ThreadIndex].ThreadInfo.Data.ThreadState = ThreadState.ThreadIs404;
                Threads[ThreadIndex].ThreadInfo.ForceSaveThread();
                niTray.BalloonTipText = $"{Threads[ThreadIndex].ThreadInfo.Data.Id} on /{Threads[ThreadIndex].ThreadInfo.Data.Board}/ has 404'd";
                niTray.BalloonTipTitle = Threads[ThreadIndex].ThreadInfo.Chan switch {
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
                niTray.Icon = Properties.Resources.ProgramIcon_Dead;
                Icon404WasShown = true;
                changeTray.Start();
                niTray.ShowBalloonTip(5000);
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "404'd";
            } break;
            case ThreadStatus.ThreadIsAborted: {
                Threads[ThreadIndex].ThreadInfo.Data.ThreadState = ThreadState.ThreadIsAborted;
                Threads[ThreadIndex].ThreadInfo.ForceSaveThread();
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Aborted";
            } break;
            case ThreadStatus.ThreadIsArchived: {
                Threads[ThreadIndex].ThreadInfo.Data.ThreadState = ThreadState.ThreadIsArchived;
                Threads[ThreadIndex].ThreadInfo.ForceSaveThread();
                niTray.BalloonTipText = $"{Threads[ThreadIndex].ThreadInfo.Data.Id} on /{Threads[ThreadIndex].ThreadInfo.Data.Board}/ has been archived.";
                niTray.BalloonTipTitle = Threads[ThreadIndex].ThreadInfo.Chan switch {
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
                niTray.Icon = Properties.Resources.ProgramIcon_Dead;
                Icon404WasShown = true;
                changeTray.Start();
                niTray.ShowBalloonTip(5000);
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = "Archived";
            } break;
            case ThreadStatus.ThreadScanningSoon: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Scanning soon";
            } break;
            case ThreadStatus.ThreadUpdateName: {
                lvThreads.Items[ThreadIndex].SubItems[clName.Index].Text = Threads[ThreadIndex].ThreadInfo.Data.RetrievedThreadName ? Threads[ThreadIndex].ThreadInfo.Data.ThreadName : Threads[ThreadIndex].ThreadInfo.Data.Url;
            } break;

            case ThreadStatus.ThreadInfoNotSet: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Info not set";
            } break;
            case ThreadStatus.ThreadRetrying: {
                Threads[ThreadIndex].ThreadInfo.Data.ThreadState = ThreadState.ThreadIsAlive;
                Threads[ThreadIndex].ThreadInfo.ForceSaveThread();
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Retrying";
            } break;
            case ThreadStatus.ThreadImproperlyDownloaded: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Bad download";
            } break;
            case ThreadStatus.NoThreadPosts: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " No thread posts";
            } break;
            case ThreadStatus.FailedToParseThreadHtml: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Could not parse html";
            } break;
            case ThreadStatus.ThreadUnknownError: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Unknown thread error";
            } break;

            default: {
                lvThreads.Items[ThreadIndex].SubItems[clStatus.Index].Text = " Unknown state";
            } break;
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
                    if (General.SaveQueueOnExit) {
                        var SavedThreads = ProgramSettings.LoadThreads();
                        if (SavedThreads.Count > 0) {
                            for (int i = 0; i < SavedThreads.Count; i++) {
                                ListViewItem lvi = new() {
                                    Name = SavedThreads[i].Url,
                                    Text = "waiting for load..."
                                };
                                this.Invoke(() => lvThreads.Items.Add(lvi));
                            }
                            for (int i = 0; i < SavedThreads.Count; i++) {
                                try {
                                    var Thread = SavedThreads[i];
                                    this.Invoke(() => LoadSavedThread(Thread, Thread.FilePath));
                                }
                                catch (Exception ex) {
                                    murrty.classes.Log.ReportException(ex);
                                }
                                Thread.Sleep(500);
                            }
                        }
                    }

                    if (Arguments.Argv.Length > 0) {
                        for (int i = 0; i < Arguments.Argv.Length; i++) {
                            this.Invoke(() => AddNewThread(Arguments.Argv[i], true));
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
        List<ThreadInfo> Data = [];
        if (ThreadURLs.Count > 0) {
            for (int i = 0; i < ThreadURLs.Count; i++) {
                Data.Add(Threads[i].ThreadInfo);
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
        if (ThreadURL.StartsWith("ychanex:")) {
            ThreadURL = ThreadURL[8..];
        }
        if (ThreadURL.StartsWith("view-source:")) {
            ThreadURL = ThreadURL[12..];
        }

        ThreadURL = Networking.CleanURL(ThreadURL);

        if (!Chans.SupportedChan(ThreadURL, out ChanType ReceivedType)) {
            if (FromTrayOrArgument) {
                niTray.BalloonTipText = $"The url is not valid: {ThreadURL}";
                niTray.BalloonTipIcon = ToolTipIcon.Error;
                niTray.ShowBalloonTip(5000);
            }
            return false;
        }

        if (ThreadURLs.Contains(ThreadURL)) {
            int ThreadURLIndex = ThreadURLs.IndexOf(ThreadURL);
            if (Threads[ThreadURLIndex].ThreadInfo.Data.ThreadState != ThreadState.ThreadIsAlive) {
                Threads[ThreadURLIndex].ManageThread(ThreadEvent.RetryDownload);
                niTray.BalloonTipTitle = "Already in queue";
                niTray.BalloonTipText = $"Restarting {ThreadURL}";
                niTray.BalloonTipIcon = ToolTipIcon.Info;
            }
            else {
                niTray.BalloonTipTitle = "Already in queue";
                niTray.BalloonTipText = $"{ThreadURL} is already loaded and in queue.";
                niTray.BalloonTipIcon = ToolTipIcon.Info;
            }
            niTray.ShowBalloonTip(5000);
            return true;
        }
        else {
            switch (ReceivedType) {
                case ChanType.EightKun:
                case ChanType.EightChan: {
                    ThreadURL = ThreadURL.Replace(".json", ".html");
                } break;

                case ChanType.fchan: {
                    ThreadURL = ThreadURL.Replace("https://", "http://");
                } break;

                case ChanType.Unsupported:
                    return false;
            }

            ThreadInfo NewInfo = new(ReceivedType);
            NewInfo.Data.Url = ThreadURL;
            NewInfo.ThreadIndex = ThreadURLs.Count;
            if (ReceivedType == ChanType.FoolFuuka) {
                NewInfo.Data.UrlHost = Networking.GetHost(ThreadURL);
            }
            ThreadURLs.Add(ThreadURL);

            frmDownloader newThread = new(this, NewInfo) {
                Name = ThreadURL,
            };
            newThread.ManageThread(ThreadEvent.ParseForInfo);
            newThread.Show();

            ListViewItem lvi = new() {
                Name = ThreadURL
            };
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
            lvi.ImageIndex = (int)NewInfo.Chan;
            lvi.SubItems[clStatus.Index].Text = " Creating";
            lvi.SubItems[clThread.Index].Text = $"/{NewInfo.Data.Board}/ - {NewInfo.Data.Id}";
            lvi.SubItems[clName.Index].Text = ThreadURL;

            lvThreads.Items.Add(lvi);

            Threads.Add(newThread);

            if (chkCreateThreadInTheBackground.Checked) {
                newThread.Hide();
            }

            if (FromTrayOrArgument) {
                newThread.Hide();
                niTray.BalloonTipTitle = "Thread added to queue";
                niTray.BalloonTipText = $"Added /{NewInfo.Data.Board}/{NewInfo.Data.Id}/ to the queue";
                niTray.BalloonTipIcon = ToolTipIcon.Info;
                niTray.ShowBalloonTip(5000);
            }

            newThread.Opacity = 100;
            newThread.ShowInTaskbar = true;

            newThread.ManageThread(ThreadEvent.StartDownload);
            if (General.AutoSaveThreads) {
                SaveThreads();
            }

            if (General.SaveThreadHistory && !DownloadHistory.Contains(ReceivedType, ThreadURL)) {
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
                    case ChanType.FoolFuuka: {
                        tvHistory.Nodes[7].Nodes.Add(ThreadURL, ThreadURL);
                    }
                    break;
                }
                DownloadHistory.Save();
            }

            niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";
            return true;
        }
    }

    /// <summary>
    /// Adds a new thread to the queue via saved threads.
    /// </summary>
    /// <param name="Info"></param>
    /// <returns></returns>
    private bool LoadSavedThread(ThreadData Info, string InfoPath) {
        Info.Url = Networking.CleanURL(Info.Url);

        if (!Chans.SupportedChan(Info.Url, out ChanType ReceivedType) || ThreadURLs.Contains(Info.Url)) {
            return false;
        }

        if (ReceivedType == ChanType.Unsupported) {
            return false;
        }
        if (ReceivedType == ChanType.fchan) {
            Info.Url = Info.Url.Replace("https://", "http://");
        }

        ThreadURLs.Add(Info.Url);
        ThreadInfo NewInfo = new(Info, ReceivedType) {
            ThreadIndex = ThreadURLs.Count - 1,
            SavedThreadJson = InfoPath,
            ThreadReloaded = true,
        };
        NewInfo.UpdateJsonPath();
        frmDownloader newThread = new(this, NewInfo) {
            Name = Info.Url,
        };
        newThread.Show();
        newThread.Hide();
        newThread.ManageThread(ThreadEvent.ReloadThread);

        //ListViewItem lvi = new() {
        //    Name = Info.Url
        //};

        ListViewItem lvi = lvThreads.Items[Threads.Count];

        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

        lvi.ImageIndex = (int)NewInfo.Chan;
        lvi.SubItems[clStatus.Index].Text = " Creating";
        lvi.SubItems[clThread.Index].Text = $"/{NewInfo.Data.Board}/ - {NewInfo.Data.Id}";
        //lvThreads.Items.Add(lvi);

        newThread.Opacity = 100;
        newThread.ShowInTaskbar = true;

        Threads.Add(newThread);
        if (Info.SetCustomName) {
            lvi.SubItems[clName.Index].Text = Info.CustomThreadName;
        }
        else if (Info.RetrievedThreadName) {
            lvi.SubItems[clName.Index].Text = Info.ThreadName;
        }
        else {
            lvi.SubItems[clName.Index].Text = Info.Url;
        }

        newThread.ManageThread(ThreadEvent.StartDownload);
        niTray.Text = "YChanEx - " + Threads.Count + " threads";
        return true;
    }

    /// <summary>
    /// Removes a thread from the queue.
    /// </summary>
    /// <param name="Thread">The thread information that will be removed..</param>
    /// <returns>True if the thread was removed; otherwise, false.</returns>
    private bool RemoveThread(ThreadInfo Thread) {
        if (Thread.ThreadIndex > -1 && Thread.ThreadIndex < ThreadURLs.Count) {
            Saved.DownloadFormSize = Threads[Thread.ThreadIndex].Size;

            if (Threads[Thread.ThreadIndex].ThreadInfo.Data.ThreadState == ThreadState.ThreadIsAlive) {
                Threads[Thread.ThreadIndex].ManageThread(ThreadEvent.AbortDownload, true);
            }

            if (System.IO.File.Exists(Thread.SavedThreadJson)) {
                System.IO.File.Delete(Thread.SavedThreadJson);
            }

            //Threads[Thread.ThreadIndex].Dispose();
            Threads.RemoveAt(Thread.ThreadIndex);
            ThreadURLs.RemoveAt(Thread.ThreadIndex);
            lvThreads.Items.RemoveAt(Thread.ThreadIndex);

            if (Threads.Count > 0) {
                for (int i = Thread.ThreadIndex; i < Threads.Count; i++) {
                    Threads[i].ThreadInfo.ThreadIndex--;
                    Threads[i].ThreadInfo.UpdateJsonPath();
                }
            }

            niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";

            return true;
        }
        return false;
    }

    /// <summary>
    /// The method to determine if the application should exit, while also performing last-second saving for threads and form location/size.
    /// </summary>
    /// <returns>True, if the application is allowed to exit. False, if the application isn't allowed to exit.</returns>
    private bool ApplicationCanExit(bool tray) {
        if (this.Visible && General.MinimizeInsteadOfExiting && !tray) {
            this.WindowState = FormWindowState.Normal;
            this.Hide();
            niTray.Visible = true;
            return false;
        }

        if (General.SaveQueueOnExit && (!Program.DebugMode || Program.LoadThreadsInDebug)) {
            SaveThreads();
        }

        if (ThreadLoader?.IsAlive == true) {
            ThreadLoader.Abort();
        }

        if (UpdateCheck?.IsAlive == true) {
            UpdateCheck.Abort();
        }

        Saved.MainFormLocation = this.Location;
        Saved.MainFormSize = this.Size;
        Saved.CreateThreadInTheBackground = chkCreateThreadInTheBackground.Checked;
        Saved.MainFormColumnSizes = lvThreads.GetColumnWidths();

        for (int i = 0; i < Threads.Count; i++) {
            Threads[i].ManageThread(ThreadEvent.AbortForClosing);
            Threads[i].Dispose();
        }
        ThreadURLs.Clear();
        Threads.Clear();
        niTray.Visible = false;
        TrayExit = tray;

        return true;
    }
    #endregion

    #region Form Controls
    [System.Diagnostics.DebuggerStepThrough]
    protected override void WndProc(ref Message m) {
        switch (m.Msg) {
            case CopyData.WM_COPYDATA when CopyData.TryGetArray(m.LParam, CopyData.ID_ARGS, out var argv): {
                for (int i = 0; i < argv.Length; i++) {
                    AddNewThread(argv[i], true);
                }
            } break;

            case CopyData.WM_SHOWMAINFORM: {
                if (this.WindowState != FormWindowState.Normal) {
                    this.WindowState = FormWindowState.Normal;
                }
                this.Show();
                this.Activate();
                niTray.Visible = General.ShowTrayIcon;
                m.Result = IntPtr.Zero;
            } break;

            case 0x0010: { // WM_CLOSE
                niTray.Visible = false;
                base.WndProc(ref m);
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
        ilIcons.Images.Add(Properties.Resources._foolfuuka);
        lvThreads.SmallImageList = ilIcons;
        lvThreads.ContextMenu = cmThreads;

#if !DEBUG
        tpDebug.Dispose();
#endif
    }
    private void frmMain_Load(object sender, EventArgs e) {
        if (General.ShowTrayIcon) {
            niTray.Visible = true;
        }
        niTray.ContextMenu = cmTray;

        if (Config.ValidSize(Saved.MainFormSize)) {
            this.Size = Saved.MainFormSize;
        }
        if (Config.ValidPoint(Saved.MainFormLocation)) {
            this.Location = Saved.MainFormLocation;
        }
        if (!string.IsNullOrEmpty(Saved.MainFormColumnSizes)) {
            lvThreads.SetColumnWidths(Saved.MainFormColumnSizes);
        }

        if (DownloadHistory.Count > 0) {
            for (int i = 0; i < DownloadHistory.Data.FourChanHistory.Count; i++) {
                string url = DownloadHistory.Data.FourChanHistory[i];
                tvHistory.Nodes[0].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.FourTwentyChanHistory.Count; i++) {
                string url = DownloadHistory.Data.FourTwentyChanHistory[i];
                tvHistory.Nodes[1].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.SevenChanHistory.Count; i++) {
                string url = DownloadHistory.Data.SevenChanHistory[i];
                tvHistory.Nodes[2].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.EightChanHistory.Count; i++) {
                string url = DownloadHistory.Data.EightChanHistory[i];
                tvHistory.Nodes[3].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.EightKunHistory.Count; i++) {
                string url = DownloadHistory.Data.EightKunHistory[i];
                tvHistory.Nodes[4].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.FchanHistory.Count; i++) {
                string url = DownloadHistory.Data.FchanHistory[i];
                tvHistory.Nodes[5].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.u18chanHistory.Count; i++) {
                string url = DownloadHistory.Data.u18chanHistory[i];
                tvHistory.Nodes[6].Nodes.Add(url, url);
            }
            for (int i = 0; i < DownloadHistory.Data.FoolFuukaHistory.Count; i++) {
                string url = DownloadHistory.Data.FoolFuukaHistory[i];
                tvHistory.Nodes[7].Nodes.Add(url, url);
            }
        }

        chkCreateThreadInTheBackground.Checked = Saved.CreateThreadInTheBackground;

        if (General.EnableUpdates && !Program.DebugMode) {
            UpdateCheck = new(() => UpdateChecker.CheckForUpdate(false)) {
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
        if (!TrayExit) {
            if (General.MinimizeInsteadOfExiting && this.Visible) {
                this.Hide();
                niTray.Visible = true;
                e.Cancel = true;
            }
            else if (!ApplicationCanExit(false)) {
                e.Cancel = true;
            }
        }
    }
    private void frmMain_SizeChanged(object sender, EventArgs e) {
        if (this.WindowState == FormWindowState.Minimized && General.MinimizeToTray) {
            this.WindowState = FormWindowState.Normal;
            this.Hide();
            niTray.Visible = true;
        }
    }
    private void mSettings_Click(object sender, EventArgs e) {
        using frmSettings Settings = new();
        Settings.ShowDialog();
        Program.SettingsOpen = false;
        niTray.Visible = General.ShowTrayIcon;

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
            frmDownloader DownloadForm = Threads[lvThreads.SelectedIndices[0]];
            DownloadForm.Show();
            DownloadForm.WindowState = FormWindowState.Normal;
            DownloadForm.Activate();
        }
    }
    private void cmThreads_Popup(object sender, EventArgs e) {
        if (lvThreads.SelectedIndices.Count > 0) {
            mStatus.Enabled = true;

            if (lvThreads.SelectedIndices.Count == 1) {
                mSetCustomName.Enabled = true;
                mRetryDownload.Enabled = Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.ThreadState != ThreadState.ThreadIsAlive;
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
            niTray.Icon = Properties.Resources.ProgramIcon;
            changeTray.Stop();
        }
    }
    private void niTray_MouseDoubleClick(object sender, MouseEventArgs e) {
        if (!this.Visible) {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.Activate();
            if (!General.ShowTrayIcon) {
                niTray.Visible = false;
            }
        }
    }
    #endregion

    #region cmThreads Controls
    private void mStatus_Click(object sender, EventArgs e) {
        if (lvThreads.SelectedIndices.Count > 0) {
            for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                frmDownloader DownloadForm = Threads[lvThreads.SelectedIndices[CurrentThread]];
                DownloadForm.Show();
                DownloadForm.WindowState = FormWindowState.Normal;
                DownloadForm.Activate();
            }
        }
    }
    private void mRetryDownload_Click(object sender, EventArgs e) {
        if (lvThreads.SelectedItems.Count == 1) {
            if (Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.ThreadState != ThreadState.ThreadIsAlive) {
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
                    Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.SetCustomName = true;
                    Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.CustomThreadName = newName.txtNewName.Text;
                    break;
                case DialogResult.No:
                    lvThreads.Items[lvThreads.SelectedIndices[0]].SubItems[clName.Index].Text = Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.ThreadName;
                    Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.SetCustomName = false;
                    Threads[lvThreads.SelectedIndices[0]].ThreadInfo.Data.CustomThreadName = null;
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
                string FoundThreadDownloadPath = Threads[lvThreads.SelectedIndices[CurrentThread]].ThreadInfo.DownloadPath;
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
                ThreadIDBuffer += Threads[lvThreads.SelectedIndices[CurrentThread]].ThreadInfo.Data.Id;
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
                RemoveThread(Threads[lvThreads.SelectedIndices[CurrentThread]].ThreadInfo);
            }

            if (General.AutoSaveThreads) {
                SaveThreads();
            }

            niTray.Text = "YChanEx - " + lvThreads.Items.Count + " threads";
        }
    }
    #endregion

    #region cmTray Controls
    private void mTrayShowYChanEx_Click(object sender, EventArgs e) {
        if (!this.Visible) {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.Activate();
            if (!General.ShowTrayIcon) {
                niTray.Visible = false;
            }
        }
    }
    private void mAddThread_Click(object sender, EventArgs e) {
        if (Clipboard.ContainsText()) {
            AddNewThread(Clipboard.GetText(), true);
        }
    }
    private void mTrayExit_Click(object sender, EventArgs e) {
        if (ApplicationCanExit(true)) {
            this.Close();
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
