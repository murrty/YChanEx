using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form {
        #region Variables
        private List<frmDownloader> ThreadDownloadForms = new List<frmDownloader>();    // The list of Thread download forms
        private List<string> ThreadURLs = new List<string>();                           // The list of Thread URLs
        private List<int> ThreadAliveStatuses = new List<int>();                        // the list of Thread statuses
        private bool Icon404WasShown = false;   // Determines if the 404 icon has been shown on the tray.
        private bool ThreadsModified = false;   // Determines if the threads lists were modified to resave them.
        #endregion

        #region Usbility methods
        /// <summary>
        /// Announces the 404 to the main form handle, which will pop up a notification in the tray.
        /// </summary>
        /// <param name="ThreadID">The ID of the thread that will be displayed in the notification.</param>
        /// <param name="ThreadBoard">The board of the thread that will be displayed in the notification.</param>
        /// <param name="ThreadURL">The URL of the thread that will be used to find indexes in the lists to set 404 status.</param>
        /// <param name="ChanType">The int value of the *chan to determine what *chan the thread was on.</param>
        public void Announce404(string ThreadID, string ThreadBoard, string ThreadURL, int ChanType) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            ThreadAliveStatuses[ThreadIndex] = (int)ThreadStatuses.AliveStatus.Was404;
            niTray.BalloonTipText = ThreadID + " on /" + ThreadBoard + "/ has 404'd";
            switch (ChanType) {
                case (int)ChanTypes.Types.FourChan:
                    niTray.BalloonTipTitle = "4chan";
                    break;
                case (int)ChanTypes.Types.FourTwentyChan:
                    niTray.BalloonTipTitle = "420chan";
                    break;
                case (int)ChanTypes.Types.SevenChan:
                    niTray.BalloonTipTitle = "7chan";
                    break;
                case (int)ChanTypes.Types.EightChan:
                    niTray.BalloonTipTitle = "8chan";
                    break;
                case (int)ChanTypes.Types.EightKun:
                    niTray.BalloonTipTitle = "8kun";
                    break;
                case (int)ChanTypes.Types.fchan:
                    niTray.BalloonTipTitle = "fchan";
                    break;
                case (int)ChanTypes.Types.u18chan:
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
            GC.Collect();
        }
        /// <summary>
        /// Announces to the form that the thread was aborted by the user.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread that was aborted to find the index of the status to set aborted status.</param>
        public void AnnounceAbort(string ThreadURL) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            ThreadAliveStatuses[ThreadIndex] = (int)ThreadStatuses.AliveStatus.WasAborted;
            ThreadsModified = true;
        }
        /// <summary>
        /// Restores the alive status of the thread to attempt redownloading if the thread was 404'd or aborted.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread being reset to alive to find the index of the status to set alive status.</param>
        public void Un404Thread(string ThreadURL) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            ThreadAliveStatuses[ThreadIndex] = (int)ThreadStatuses.AliveStatus.Alive;
            ThreadsModified = true;
        }
        /// <summary>
        /// Sets the thread status from another thread handle to change the status on the main form.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread to find the index in the listview.</param>
        /// <param name="NewStatus">The new custom status to be set onto it.</param>
        public void SetItemStatus(string ThreadURL, string NewStatus) {
            int ItemIndex = ThreadURLs.IndexOf(ThreadURL);
            lvThreads.Items[ItemIndex].SubItems[0].Text = NewStatus;
        }
        /// <summary>
        /// Adds a new thread to the queue by setting predetermined statuses.
        /// </summary>
        /// <param name="ThreadURL">The url of the thread that will be added to the queue.</param>
        /// <param name="ThreadWasSaved">The boolean to set if the thread was saved, and if it was it will automatically hide the form. Defaults to false.</param>
        /// <param name="AliveStatus">The int value of the thread status. Defaults to alive.</param>
        /// <returns></returns>
        public bool AddNewThread(string ThreadURL, bool ThreadWasSaved = false, int AliveStatus = (int)ThreadStatuses.AliveStatus.Alive) {
            if (Chans.SupportedChan(ThreadURL)) {
                if (ThreadURLs.Contains(ThreadURL)) {
                    int ThreadURLIndex = ThreadURLs.IndexOf(ThreadURL);
                    if (ThreadAliveStatuses[ThreadURLIndex] != (int)ThreadStatuses.AliveStatus.Alive) {
                        ThreadDownloadForms[lvThreads.SelectedIndices[0]].RetryScanOnFailure();
                    }
                    return true;
                }
                else {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    switch (AliveStatus) {
                        case (int)ThreadStatuses.AliveStatus.Was404:
                            lvi.SubItems[0].Text = ThreadStatuses.Has404;
                            break;
                        case (int)ThreadStatuses.AliveStatus.WasAborted:
                            lvi.SubItems[0].Text = ThreadStatuses.HasAborted;
                            break;
                        default:
                            lvi.SubItems[0].Text = ThreadStatuses.Waiting;
                            break;
                    }
                    lvi.SubItems[1].Text = ThreadURL;
                    lvi.Name = ThreadURL;
                    lvThreads.Items.Add(lvi);
                    ThreadURLs.Add(ThreadURL);
                    ThreadAliveStatuses.Add(AliveStatus);

                    frmDownloader newThread = new frmDownloader();
                    ThreadDownloadForms.Add(newThread);
                    newThread.ChanType = Chans.GetChanType(ThreadURL);
                    if (newThread.ChanType == (int)ChanTypes.Types.fchan) {
                        if (!Downloads.Default.fchanWarning) {
                            MessageBox.Show(
                                "fchan works, but isn't supported. I'm keeping this in for people, but here's your only warning: I will not help with any issues regarding fchan, and they will not be acknowledged.\n\n" +
                                "The reason I'm not going to continue working on fchan is because of all the logic shenanigans I have to do to get files, and even then it's still not perfect for some files.\n\n\n" +
                                "I might fix it and update it later, but I'm not going to touch it anymore. You're on your own with it.\n\n" +
                                "This is the only time this warning will appear.");
                            Downloads.Default.fchanWarning = true;
                            Downloads.Default.Save();
                        }
                    }
                    newThread.Name = ThreadURL;
                    newThread.ThreadURL = ThreadURL;
                    newThread.Show();

                    if (ThreadWasSaved) {
                        if (AliveStatus == (int)ThreadStatuses.AliveStatus.Alive) {
                            newThread.StartDownload();
                        }
                        else {
                            newThread.StartGone(AliveStatus);
                        }

                        newThread.Hide();
                    }
                    else {
                        newThread.StartDownload();
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
                ThreadDownloadForms[i].AbortDownloadForClosing();
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
            niTray.Icon = Properties.Resources.YChanEx;
            this.Icon = Properties.Resources.YChanEx;
            lvThreads.ContextMenu = cmThreads;
        }
        private void frmMain_Load(object sender, EventArgs e) {
            if (General.Default.SaveQueueOnExit && !Program.IsDebug) {
                string[] ThreadArray = Chans.LoadThreads().Split('\n');
                if (ThreadArray != null && ThreadArray.Length > 0) {
                    for (int ThreadArrayIndex = 0; ThreadArrayIndex < ThreadArray.Length; ThreadArrayIndex++) {
                        // assume the thread is alive unless it's confirmed false in the threads.dat file
                        int AliveStatus = 0;
                        string ThreadAtIndex = ThreadArray[ThreadArrayIndex];
                        string URL = ThreadAtIndex.Split('=')[0].Trim(' ');

                        // if the thread.dat contains an equal sign, try to parse it.
                        if (ThreadAtIndex.Contains("=")) {
                            string IsAliveString = ThreadAtIndex.Split('=')[1].ToLower().Trim(' ');
                            if (IsAliveString.ToLower() == "1") { AliveStatus = 1; }
                            else if (IsAliveString.ToLower() == "2") { AliveStatus = 2; }
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
                    ThreadDownloadForms[Thread].ChangeFormTitle();
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
                if (ThreadAliveStatuses[lvThreads.SelectedIndices[0]] != (int)ThreadStatuses.AliveStatus.Alive) {
                    mRetryDownload.Enabled = true;
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
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Show();
            }
        }
        private void mRetryDownload_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                if (ThreadAliveStatuses[lvThreads.SelectedIndices[0]] != (int)ThreadStatuses.AliveStatus.Alive) {
                    ThreadDownloadForms[lvThreads.SelectedIndices[0]].RetryScanOnFailure();
                    ThreadsModified = true;
                    mRetryDownload.Enabled = false;
                }
            }
        }
        private void mOpenDownloadFolder_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                string FoundThreadDownloadPath = ThreadDownloadForms[lvThreads.SelectedIndices[0]].DownloadPath;
                if (!string.IsNullOrEmpty(FoundThreadDownloadPath)) {
                    if (System.IO.Directory.Exists(FoundThreadDownloadPath)) {
                        System.Diagnostics.Process.Start(FoundThreadDownloadPath);
                    }
                }
            }
        }
        private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                System.Diagnostics.Process.Start(ThreadURLs[lvThreads.SelectedIndices[0]]);
            }
        }
        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                Clipboard.SetText(ThreadURLs[lvThreads.SelectedIndices[0]]);
            }
        }
        private void mCopyThreadID_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                string FoundThreadID = ThreadDownloadForms[lvThreads.SelectedIndices[0]].PublicThreadID;
                if (!string.IsNullOrEmpty(FoundThreadID)) {
                    Clipboard.SetText(FoundThreadID);
                }
            }
        }
        private void mRemove_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                int SelectedIndex = lvThreads.SelectedIndices[0];
                if (ThreadAliveStatuses[SelectedIndex] != (int)ThreadStatuses.AliveStatus.Alive) {
                    ThreadDownloadForms[SelectedIndex].StopDownload();
                }

                if (Saved.Default.DownloadFormSize != ThreadDownloadForms[SelectedIndex].Size) {
                    Saved.Default.DownloadFormSize = ThreadDownloadForms[SelectedIndex].Size;
                }

                ThreadDownloadForms[SelectedIndex].Dispose();
                ThreadDownloadForms.RemoveAt(SelectedIndex);
                ThreadURLs.RemoveAt(SelectedIndex);
                ThreadAliveStatuses.RemoveAt(SelectedIndex);
                lvThreads.Items.RemoveAt(SelectedIndex);

                ThreadsModified = true;
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