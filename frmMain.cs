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
                case (int)ChanTypes.Types.u18han:
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
        public void SetItemStatus(string URL, string Status) {
            int ItemIndex = ThreadURLs.IndexOf(URL);
            lvThreads.Items[ItemIndex].SubItems[0].Text = Status;
        }
        public bool AddNewThread(string URL) {
            if (Chans.SupportedChan(URL)) {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems[0].Text = ThreadStatuses.Downloading;
                lvi.SubItems[1].Text = URL;
                lvi.Name = URL;
                lvThreads.Items.Add(lvi);
                ThreadURLs.Add(URL);
                ThreadIsGone.Add(false);
                frmDownloader newThread = new frmDownloader();
                newThread.Name = URL;
                newThread.ThreadURL = URL;
                newThread.StartDownload();
                ThreadDownloadForms.Add(newThread);
                newThread.Show();
                return true;
            }
            else {
                return false;
            }
        }

        private void frmMain_Load(object sender, EventArgs e) {
            if (Downloads.Default.SaveQueueOnExit) {
                string ReadThreads = null;
                if (System.IO.File.Exists(Program.ApplicationFilesLocation + "\\threads.dat")) {
                    ReadThreads = System.IO.File.ReadAllText(Program.ApplicationFilesLocation + "\\threads.dat").Trim('\n');
                    if (!string.IsNullOrEmpty(ReadThreads)) {
                        string[] ThreadArray = ReadThreads.Split('\n');
                        for (int ThreadArrayIndex = 0; ThreadArrayIndex < ThreadArray.Length; ThreadArrayIndex++) {
                            AddNewThread(ThreadArray[ThreadArrayIndex]);
                        }
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e) {
            if (AddNewThread(txtThreadURL.Text)) {
                txtThreadURL.Clear();
            }
        }

        private void mSettings_Click(object sender, EventArgs e) {
            frmSettings Settings = new frmSettings();
            Settings.ShowDialog();
            Settings.Dispose();
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

        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                ThreadDownloadForms[lvThreads.SelectedIndices[0]].Show();
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

        private void mAbout_Click(object sender, EventArgs e) {
            frmAbout About = new frmAbout();
            About.ShowDialog();
        }
    }
}