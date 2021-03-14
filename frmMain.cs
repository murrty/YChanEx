using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form {

        #region Variables
        private List<frmDownloader> ThreadDownloadForms = new List<frmDownloader>();    // The list of Thread download forms
        private List<string> ThreadURLs = new List<string>();                           // The list of Thread URLs
        private List<ThreadStatus> ThreadAliveStatuses = new List<ThreadStatus>();      // the list of Thread statuses

        private bool Icon404WasShown = false;   // Determines if the 404 icon has been shown on the tray.
        private bool ThreadsModified = false;   // Determines if the threads lists were modified to resave them.
        #endregion

        #region Usability methods
        /// <summary>
        /// Sets the thread status from another thread handle to change the status on the main form.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread to find the index in the listview.</param>
        /// <param name="NewStatus">The new custom status to be set onto it.</param>
        public void SetItemStatus(string ThreadURL, ThreadStatus Status, ThreadInfo UpdThread = default(ThreadInfo)) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            switch (Status) {
                case ThreadStatus.ThreadRetrying:
                    ThreadAliveStatuses[ThreadIndex] = ThreadStatus.ThreadIsAlive;
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Retrying";
                    break;
                case ThreadStatus.Waiting:
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Waiting";
                    break;
                case ThreadStatus.ThreadNotModified:
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Not Modified";
                    break;
                case ThreadStatus.ThreadScanning:
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Scanning";
                    break;
                case ThreadStatus.ThreadDownloading:
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Downloading";
                    break;
                case ThreadStatus.ThreadIs404:
                    ThreadAliveStatuses[ThreadIndex] = ThreadStatus.ThreadIs404;
                    niTray.BalloonTipText = UpdThread.ThreadID + " on /" + UpdThread.ThreadBoard + "/ has 404'd";
                    switch (UpdThread.Chan) {
                        case ChanType.FourChan:
                            niTray.BalloonTipTitle = "4chan";
                            break;
                        case ChanType.FourTwentyChan:
                            niTray.BalloonTipTitle = "420chan";
                            break;
                        case ChanType.SevenChan:
                            niTray.BalloonTipTitle = "7chan";
                            break;
                        case ChanType.EightChan:
                            niTray.BalloonTipTitle = "8chan";
                            break;
                        case ChanType.EightKun:
                            niTray.BalloonTipTitle = "8kun";
                            break;
                        case ChanType.fchan:
                            niTray.BalloonTipTitle = "fchan";
                            break;
                        case ChanType.u18chan:
                            niTray.BalloonTipTitle = "u18chan";
                            break;
                        default:
                            niTray.BalloonTipTitle = "Thread 404";
                            break;
                    }
                    if (changeTray.Enabled) {
                        changeTray.Stop();
                    }
                    niTray.Icon = Properties.Resources.YChanEx404;
                    Icon404WasShown = true;
                    changeTray.Start();
                    niTray.ShowBalloonTip(5000);
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "404'd";
                    break;
                case ThreadStatus.ThreadIsAborted:
                    ThreadAliveStatuses[ThreadIndex] = ThreadStatus.ThreadIsAborted;
                    ThreadsModified = true;
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Aborted";
                    break;
                case ThreadStatus.ThreadReloaded:
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Reloaded";
                    break;
                case ThreadStatus.ThreadUpdateName:
                    if (UpdThread.RetrievedThreadName) {
                        lvThreads.Items[ThreadIndex].SubItems[3].Text = UpdThread.ThreadName;
                    }
                    else {
                        lvThreads.Items[ThreadIndex].SubItems[3].Text = ThreadURL;
                    }
                    break;
                default:
                    lvThreads.Items[ThreadIndex].SubItems[1].Text = "Unknown?";
                    break;
            }
            GC.Collect();
        }
        /// <summary>
        /// Adds a new thread to the queue by setting predetermined statuses.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread that will be added to the queue.</param>
        /// <param name="ThreadWasSaved">The boolean to set if the thread was saved, and if it was it will automatically hide the form. Defaults to false.</param>
        /// <param name="AliveStatus">The int value of the thread status. Defaults to alive.</param>
        /// <returns></returns>
        public bool AddNewThread(string ThreadURL, bool ThreadWasSaved = false, ThreadStatus AliveStatus = ThreadStatus.ThreadIsAlive) {
            if (ThreadURL.StartsWith("view-source:")) { ThreadURL = ThreadURL.Substring(12); }

            if (Chans.SupportedChan(ThreadURL)) {
                if (ThreadURLs.Contains(ThreadURL)) {
                    int ThreadURLIndex = ThreadURLs.IndexOf(ThreadURL);
                    if (ThreadAliveStatuses[ThreadURLIndex] != ThreadStatus.ThreadIsAlive) {
                        ThreadDownloadForms[lvThreads.SelectedIndices[0]].ManageThread(ThreadEvent.RetryDownload);
                    }
                    return true;
                }
                else {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    switch (AliveStatus) {
                        case ThreadStatus.ThreadIs404:
                            lvi.SubItems[1].Text = "404'd";
                            break;
                        case ThreadStatus.ThreadIsAborted:
                            lvi.SubItems[1].Text = "Aborted";
                            break;
                        default:
                            lvi.SubItems[1].Text = "Waiting";
                            break;
                    }
                    lvi.SubItems[3].Text = ThreadURL;
                    lvi.Name = ThreadURL;
                    ThreadURLs.Add(ThreadURL);
                    ThreadAliveStatuses.Add(AliveStatus);

                    frmDownloader newThread = new frmDownloader();
                    ThreadDownloadForms.Add(newThread);
                    newThread.Chan = Chans.GetChanType(ThreadURL);
                    switch (newThread.Chan) {
                        case ChanType.fchan:
                            if (!Downloads.Default.fchanWarning) {
                                MessageBox.Show(
                                    "fchan works, but isn't supported. I'm keeping this in for people, but here's your only warning: I will not help with any issues regarding fchan, and they will not be acknowledged.\n\n" +
                                    "The reason I'm not going to continue working on fchan is because of all the logic shenanigans I have to do to get files, and even then it's still not perfect for some files.\n\n\n" +
                                    "I might fix it and update it later, but I'm not going to touch it anymore. You're on your own with it.\n\n" +
                                    "This is the only time this warning will appear.");
                                Downloads.Default.fchanWarning = true;
                                Downloads.Default.Save();
                            }
                            break;
                    }
                    lvi.ImageIndex = (int)newThread.Chan;
                    lvThreads.Items.Add(lvi);

                    newThread.Name = ThreadURL;
                    newThread.ThreadURL = ThreadURL;
                    newThread.ManageThread(ThreadEvent.ParseForInfo);
                    lvi.SubItems[2].Text = "/" + newThread.ThreadBoard + "/" + newThread.ThreadID;
                    newThread.Show();

                    if (ThreadWasSaved) {
                        if (AliveStatus == ThreadStatus.ThreadIsAlive) {
                            newThread.ManageThread(ThreadEvent.StartDownload);
                        }
                        else {
                            switch (AliveStatus) {
                                case ThreadStatus.ThreadIs404:
                                    newThread.LastStatus = ThreadStatus.ThreadIs404;
                                    break;
                                case ThreadStatus.ThreadIsAborted:
                                    newThread.LastStatus = ThreadStatus.ThreadIsAborted;
                                    break;
                            }
                            newThread.ManageThread(ThreadEvent.ThreadWasGone);
                        }

                        newThread.Hide();
                    }
                    else {
                        newThread.ManageThread(ThreadEvent.StartDownload);
                        ThreadsModified = true;
                    }

                    newThread.Opacity = 100;
                    newThread.ShowInTaskbar = true;
                    return true;
                }
            }
            else {
                return false;
            }
        }
        /// <summary>
        /// The method to determine if the application should exit, while also performing last-second saving for threads and form location/size.
        /// </summary>
        /// <returns>True, if the application is allowed to exit. False, if the application isn't allowed to exit.</returns>
        public bool ApplicationShouldExit() {
            bool ExitApplication = true;

            if (General.Default.MinimizeInsteadOfExiting) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
                ExitApplication = false;
                return false;
            }

            if (lvThreads.Items.Count > 0) {
                if (General.Default.ShowExitWarning) {
                    switch (MessageBox.Show("You have threads in the queue. Do you want to save them for later?", "YChanEx", MessageBoxButtons.YesNoCancel)) {
                        case DialogResult.Yes:
                            if (ThreadsModified) {
                                Chans.SaveThreads(ThreadURLs, ThreadAliveStatuses);
                            }
                            break;
                        case DialogResult.Cancel:
                            ExitApplication = false;
                            return false;
                    }
                }
                else if (General.Default.SaveQueueOnExit) {
                    if (ThreadsModified) {
                        Chans.SaveThreads(ThreadURLs, ThreadAliveStatuses);
                    }
                }
            }

            if (!ExitApplication) {
                return false;
            }

            Saved.Default.MainFormLocation = this.Location;
            Saved.Default.MainFormSize = this.Size;
            Saved.Default.Save();

            for (int i = 0; i < ThreadDownloadForms.Count; i++) {
                ThreadDownloadForms[i].ManageThread(ThreadEvent.AbortForClosing);
                ThreadDownloadForms[i].Dispose();
                ThreadURLs.RemoveAt(i);
                ThreadAliveStatuses.RemoveAt(i);
                ThreadDownloadForms.RemoveAt(i);
            }

            niTray.Visible = false;

            return true;
        }
        #endregion

        #region Form Controls
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
        }
        private void frmMain_Load(object sender, EventArgs e) {
            if (General.Default.SaveQueueOnExit && !Program.IsDebug) {
                string[] ThreadArray = Chans.LoadThreads().Split('\n');
                if (ThreadArray != null && ThreadArray.Length > 0) {
                    for (int ThreadArrayIndex = 0; ThreadArrayIndex < ThreadArray.Length; ThreadArrayIndex++) {
                        // assume the thread is alive unless it's confirmed false in the threads.dat file
                        ThreadStatus AliveStatus = ThreadStatus.ThreadReloaded;
                        string ThreadAtIndex = ThreadArray[ThreadArrayIndex];
                        string URL = ThreadAtIndex.Split('=')[0].Trim(' ');

                        // if the thread.dat contains an equal sign, try to parse it.
                        if (ThreadAtIndex.Contains("=")) {
                            string IsAliveString = ThreadAtIndex.Split('=')[1].ToLower().Trim(' ');
                            switch (IsAliveString.ToLower()) {
                                case "1":
                                    AliveStatus = ThreadStatus.ThreadIs404;
                                    break;
                                case "2":
                                    AliveStatus = ThreadStatus.ThreadIsAborted;
                                    break;
                            }
                        }

                        AddNewThread(URL, true, AliveStatus);
                    }
                }
            }
            if (General.Default.ShowTrayIcon) {
                niTray.Visible = true;
            }
            niTray.ContextMenu = cmTray;

            if (Saved.Default.MainFormLocation != default(System.Drawing.Point)) {
                this.Location = Saved.Default.MainFormLocation;
            }
            if (Saved.Default.MainFormSize != default(System.Drawing.Size)) {
                this.Size = Saved.Default.MainFormSize;
            }

            UpdateChecker.CheckForUpdate();
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            if (General.Default.MinimizeInsteadOfExiting) {
                this.Hide();
                niTray.Visible = true;
                e.Cancel = true;
            }

            if (!ApplicationShouldExit()) {
                e.Cancel = true;
            }
        }
        private void frmMain_SizeChanged(object sender, EventArgs e) {
            if (General.Default.MinimizeToTray && this.WindowState == FormWindowState.Minimized) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
                if (!niTray.Visible) {
                    niTray.Visible = true;
                }
            }
        }
        private void mSettings_Click(object sender, EventArgs e) {
            frmSettings Settings = new frmSettings();
            Settings.ShowDialog();
            Settings.Dispose();
            Program.SettingsOpen = false;

            if (General.Default.ShowTrayIcon) {
                niTray.Visible = true;
            }
            else {
                niTray.Visible = false;
            }

            if (ThreadDownloadForms.Count > 0) {
                for (int Thread = 0; Thread < ThreadDownloadForms.Count; Thread++) {
                    ThreadDownloadForms[Thread].UpdateThreadName();
                }
            }
        }
        private void mAbout_Click(object sender, EventArgs e) {
            frmAbout About = new frmAbout();
            About.ShowDialog();
        }
        private void btnAdd_Click(object sender, EventArgs e) {
            if (AddNewThread(txtThreadURL.Text)) {
                txtThreadURL.Clear();
            }
        }
        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Show();
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Activate();
            }
        }
        private void cmThreads_Popup(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                mStatus.Enabled = true;

                if (lvThreads.SelectedIndices.Count == 1) {
                    if (ThreadAliveStatuses[lvThreads.SelectedIndices[0]] != ThreadStatus.ThreadIsAlive) {
                        mRetryDownload.Enabled = true;
                    }
                    else {
                        mRetryDownload.Enabled = false;
                    }
                }
                else {
                    mRetryDownload.Enabled = false;
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
                this.Show();
                this.Activate();
                if (!General.Default.ShowTrayIcon) {
                    niTray.Visible = false;
                }
            }
        }
        #endregion

        #region cmThreads Controls
        private void mStatus_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    ThreadDownloadForms[lvThreads.SelectedIndices[CurrentThread]].Show();
                }
            }
        }
        private void mRetryDownload_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedItems.Count == 1) {
                if (ThreadAliveStatuses[lvThreads.SelectedIndices[0]] != ThreadStatus.ThreadIsAlive) {
                    ThreadDownloadForms[lvThreads.SelectedIndices[0]].ManageThread(ThreadEvent.RetryDownload);
                    ThreadsModified = true;
                    mRetryDownload.Enabled = false;
                }
            }
        }
        private void mOpenDownloadFolder_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    string FoundThreadDownloadPath = ThreadDownloadForms[lvThreads.SelectedIndices[CurrentThread]].DownloadPath;
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
                    ThreadIDBuffer += ThreadDownloadForms[lvThreads.SelectedIndices[CurrentThread]].ThreadID;
                }
                ThreadIDBuffer = ThreadIDBuffer.Trim('\n').Trim('\r');
                if (!string.IsNullOrEmpty(ThreadIDBuffer)) {
                    Clipboard.SetText(ThreadIDBuffer);
                }
            }
        }
        private void mRemove_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                if (Saved.Default.DownloadFormSize != ThreadDownloadForms[0].Size) {
                    Saved.Default.DownloadFormSize = ThreadDownloadForms[0].Size;
                }

                for (int CurrentThread = lvThreads.SelectedIndices.Count - 1; CurrentThread >= 0; CurrentThread--) {
                    int CurrentIndex = lvThreads.SelectedIndices[CurrentThread];
                    if (ThreadAliveStatuses[CurrentIndex] != ThreadStatus.ThreadIsAlive) {
                        ThreadDownloadForms[CurrentIndex].ManageThread(ThreadEvent.AbortDownload);
                    }


                    ThreadDownloadForms[CurrentIndex].Dispose();
                    ThreadDownloadForms.RemoveAt(CurrentIndex);
                    ThreadURLs.RemoveAt(CurrentIndex);
                    ThreadAliveStatuses.RemoveAt(CurrentIndex);
                    lvThreads.Items.RemoveAt(CurrentIndex);
                    ThreadsModified = true;
                }
            }
        }
        #endregion

        #region cmTray Controls
        private void mTrayShowYChanEx_Click(object sender, EventArgs e) {
            if (this.Visible == false) {
                this.Show();
                this.Activate();
                if (!General.Default.ShowTrayIcon) {
                    niTray.Visible = false;
                }
            }
        }
        private void mTrayExit_Click(object sender, EventArgs e) {
            if (ApplicationShouldExit()) {
                Environment.Exit(0);
            }
        }
        #endregion
    }
}