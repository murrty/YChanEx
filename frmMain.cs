using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form {
        List<frmDownloader> ThreadDownloadForms = new List<frmDownloader>();
        List<string> ThreadURLs = new List<string>();
        List<int> ThreadAliveStatus = new List<int>();
        bool Show404Icon = false;
        bool ThreadsModified = false;

        public frmMain() {
            InitializeComponent();
            niTray.Icon = Properties.Resources.YChanEx;
            this.Icon = Properties.Resources.YChanEx;
            lvThreads.ContextMenu = cmThreads;
        }

        public void Announce404(string ThreadID, string ThreadBoard, string URL, int ChanType) {
            int ThreadIndex = ThreadURLs.IndexOf(URL);
            ThreadAliveStatus[ThreadIndex] = (int)ThreadStatuses.AliveStatus.Was404;
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
            Show404Icon = true;
            changeTray.Start();
            niTray.ShowBalloonTip(5000);
            ThreadsModified = true;
            GC.Collect();
        }
        public void AnnounceAbort(string URL) {
            int ThreadIndex = ThreadURLs.IndexOf(URL);
            ThreadAliveStatus[ThreadIndex] = (int)ThreadStatuses.AliveStatus.WasAborted;
            ThreadsModified = true;
        }
        public void Un404Thread(string ThreadURL) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            ThreadAliveStatus[ThreadIndex] = (int)ThreadStatuses.AliveStatus.Alive;
            ThreadsModified = true;
        }
        public void SetItemStatus(string URL, string Status) {
            int ItemIndex = ThreadURLs.IndexOf(URL);
            lvThreads.Items[ItemIndex].SubItems[0].Text = Status;
        }
        public bool AddNewThread(string URL, bool ThreadWasSaved = false, int AliveStatus = (int)ThreadStatuses.AliveStatus.Alive) {
            if (Chans.SupportedChan(URL)) {
                if (ThreadURLs.Contains(URL)) {
                    int ThreadURLIndex = ThreadURLs.IndexOf(URL);
                    if (ThreadAliveStatus[ThreadURLIndex] != (int)ThreadStatuses.AliveStatus.Alive) {
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
                    lvi.SubItems[1].Text = URL;
                    lvi.Name = URL;
                    lvThreads.Items.Add(lvi);
                    ThreadURLs.Add(URL);
                    ThreadAliveStatus.Add(AliveStatus);

                    frmDownloader newThread = new frmDownloader();
                    ThreadDownloadForms.Add(newThread);
                    newThread.ChanType = Chans.GetChanType(URL);
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
                    newThread.Name = URL;
                    newThread.ThreadURL = URL;
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
        public bool TryExiting() {
            bool ExitApplication = false;
            if (General.Default.MinimizeInsteadOfExiting) {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
            }
            if (General.Default.ShowExitWarning) {
                switch (MessageBox.Show("You have threads in the queue. Do you really want to exit?", "YChanEx", MessageBoxButtons.YesNoCancel)) {
                    case DialogResult.Yes:
                        if (ThreadsModified) {
                            Chans.SaveThreads(ThreadURLs, ThreadAliveStatus);
                        }
                        ExitApplication = true;
                        break;
                    case DialogResult.No:
                        ExitApplication = true;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }
            else {
                if (ThreadsModified) {
                    Chans.SaveThreads(ThreadURLs, ThreadAliveStatus);
                }
                ExitApplication = true;
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
                ThreadAliveStatus.RemoveAt(i);
                ThreadDownloadForms.RemoveAt(i);
            }

            niTray.Visible = false;

            return true;
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

            if (!TryExiting()) {
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
        private void changeTray_Tick(object sender, EventArgs e) {
            if (Show404Icon) {
                Show404Icon = false;
            }
            else {
                niTray.Icon = Properties.Resources.YChanEx;
                changeTray.Stop();
            }
        }
        private void cmItems_Popup(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                mStatus.Enabled = true;
                if (ThreadAliveStatus[lvThreads.SelectedIndices[0]] != (int)ThreadStatuses.AliveStatus.Alive) {
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
        private void niTray_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (!this.Visible) {
                this.Show();
                this.Activate();
                if (!General.Default.ShowTrayIcon) {
                    niTray.Visible = false;
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
        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Show();
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Activate();
            }
        }
        private void btnAdd_Click(object sender, EventArgs e) {
            if (AddNewThread(txtThreadURL.Text)) {
                txtThreadURL.Clear();
            }
        }


        private void mStatus_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Show();
            }
        }
        private void mRetryDownload_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                if (ThreadAliveStatus[lvThreads.SelectedIndices[0]] != (int)ThreadStatuses.AliveStatus.Alive) {
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
                if (ThreadAliveStatus[SelectedIndex] != (int)ThreadStatuses.AliveStatus.Alive) {
                    ThreadDownloadForms[SelectedIndex].StopDownload();
                }

                if (Saved.Default.DownloadFormSize != ThreadDownloadForms[SelectedIndex].Size) {
                    Saved.Default.DownloadFormSize = ThreadDownloadForms[SelectedIndex].Size;
                }

                ThreadDownloadForms[SelectedIndex].Dispose();
                ThreadDownloadForms.RemoveAt(SelectedIndex);
                ThreadURLs.RemoveAt(SelectedIndex);
                ThreadAliveStatus.RemoveAt(SelectedIndex);
                lvThreads.Items.RemoveAt(SelectedIndex);

                ThreadsModified = true;
            }
        }


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
            if (TryExiting()) {
                Environment.Exit(0);
            }
        }

    }
}