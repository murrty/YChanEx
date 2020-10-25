using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form {
        List<frmDownloader> ThreadDownloadForms = new List<frmDownloader>();
        List<string> ThreadURLs = new List<string>();
        List<bool> ThreadIsGone = new List<bool>();
        bool Show404Icon = false;

        public frmMain() {
            InitializeComponent();
            niTray.Icon = Properties.Resources.YChanEx;
            this.Icon = Properties.Resources.YChanEx;
            lvThreads.ContextMenu = cmItems;
        }

        public void Announce404(string ThreadID, string ThreadBoard, string URL, int Chan) {
            int ThreadIndex = ThreadURLs.IndexOf(URL);
            ThreadIsGone[ThreadIndex] = true;
            niTray.BalloonTipText = ThreadID + " on /" + ThreadBoard + "/ has 404'd";
            switch (Chan) {
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
            GC.Collect();
        }
        public void AnnounceAbort(string URL) {
            int ThreadIndex = ThreadURLs.IndexOf(URL);
            ThreadIsGone[ThreadIndex] = true;
        }
        public void Un404Thread(string ThreadURL) {
            int ThreadIndex = ThreadURLs.IndexOf(ThreadURL);
            ThreadIsGone[ThreadIndex] = false;
        }
        public void SetItemStatus(string URL, string Status) {
            int ItemIndex = ThreadURLs.IndexOf(URL);
            lvThreads.Items[ItemIndex].SubItems[0].Text = Status;
        }
        public bool AddNewThread(string URL, bool IsHidden = false) {
            if (Chans.SupportedChan(URL)) {
                if (ThreadURLs.Contains(URL)) {
                    int ThreadURLIndex = ThreadURLs.IndexOf(URL);
                    if (ThreadIsGone[ThreadURLIndex]) {
                        ThreadDownloadForms[lvThreads.SelectedIndices[0]].RetryScanOnFailure();
                    }
                    return true;
                }
                else {
                    ListViewItem lvi = new ListViewItem();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems[0].Text = ThreadStatuses.Downloading;
                    lvi.SubItems[1].Text = URL;
                    lvi.Name = URL;
                    lvThreads.Items.Add(lvi);
                    ThreadURLs.Add(URL);
                    ThreadIsGone.Add(false);
                    frmDownloader newThread = new frmDownloader();
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
                    newThread.StartDownload();
                    ThreadDownloadForms.Add(newThread);
                    newThread.Show();
                    if (IsHidden) {
                        newThread.Hide();
                    }
                    newThread.Opacity = 100;
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
            else if (ThreadDownloadForms.Count > 0) {
                if (General.Default.ShowExitWarning) {
                    switch (MessageBox.Show("You have threads in the queue. Do you really want to exit?", "YChanEx", MessageBoxButtons.YesNo)) {
                        case DialogResult.Yes:
                            Chans.SaveThreads(ThreadURLs);
                            ExitApplication = true;
                            break;
                        case DialogResult.No:
                            return false;
                    }
                }
                else {
                    Chans.SaveThreads(ThreadURLs);
                    ExitApplication = true;
                }
            }
            else {
                ExitApplication = true;
            }

            if (!ExitApplication) {
                return false;
            }

            for (int i = 0; i < ThreadDownloadForms.Count; i++) {
                ThreadDownloadForms[i].AbortDownloadForClosing();
                ThreadDownloadForms[i].Dispose();
                ThreadURLs.RemoveAt(i);
                ThreadIsGone.RemoveAt(i);
                ThreadDownloadForms.RemoveAt(i);
            }

            niTray.Visible = false;

            return true;
        }


        private void frmMain_Load(object sender, EventArgs e) {
            if (General.Default.SaveQueueOnExit && !Program.IsDebug) {
                string[] ThreadArray = Chans.LoadThreads();
                if (ThreadArray != null && ThreadArray.Length > 0) {
                    for (int ThreadArrayIndex = 0; ThreadArrayIndex < ThreadArray.Length; ThreadArrayIndex++) {
                        AddNewThread(ThreadArray[ThreadArrayIndex], true);
                    }
                }
            }
            if (General.Default.ShowTrayIcon) {
                niTray.Visible = true;
            }
            niTray.ContextMenu = cmTray;
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
                mRetryDownload.Enabled = ThreadIsGone[lvThreads.SelectedIndices[0]];
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
        private void mRemove_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                int SelectedIndex = lvThreads.SelectedIndices[0];
                if (!ThreadIsGone[SelectedIndex]) {
                    ThreadDownloadForms[SelectedIndex].StopDownload();
                }
                ThreadDownloadForms[SelectedIndex].Dispose();
                ThreadDownloadForms.RemoveAt(SelectedIndex);
                ThreadURLs.RemoveAt(SelectedIndex);
                ThreadIsGone.RemoveAt(SelectedIndex);
                lvThreads.Items.RemoveAt(SelectedIndex);
            }
        }
        private void mRetryDownload_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                if (ThreadIsGone[lvThreads.SelectedIndices[0]]) {
                    ThreadDownloadForms[lvThreads.SelectedIndices[0]].RetryScanOnFailure();
                }
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