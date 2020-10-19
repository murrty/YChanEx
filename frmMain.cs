using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form {
        List<frmDownloader> Threads = new List<frmDownloader>();
        List<string> ThreadURLS = new List<string>();
        List<bool> ThreadIsGone = new List<bool>();
        bool Show404 = false;

        public frmMain() {
            InitializeComponent();
            niTray.Icon = Properties.Resources.YChanEx;
            this.Icon = Properties.Resources.YChanEx;
            lvThreads.ContextMenu = cmItems;
        }
        public void Announce404(string ThreadID, string ThreadBoard, string URL, int Chan) {
            int ThreadIndex = ThreadURLS.IndexOf(URL);
            ThreadIsGone[ThreadIndex] = true;
            niTray.BalloonTipText = ThreadID + " on /" + ThreadBoard + "/ has 404'd";
            switch (Chan) {
                case (int)ChanTypes.Types.fourChan:
                    niTray.BalloonTipTitle = "4chan";
                    break;
                case (int)ChanTypes.Types.fourTwentyChan:
                    niTray.BalloonTipTitle = "420chan";
                    break;
                case (int)ChanTypes.Types.sevenChan:
                    niTray.BalloonTipTitle = "7chan";
                    break;
                case (int)ChanTypes.Types.eightChan:
                    niTray.BalloonTipTitle = "8chan";
                    break;
                case (int)ChanTypes.Types.eightKun:
                    niTray.BalloonTipTitle = "8kun";
                    break;
                case (int)ChanTypes.Types.fchan:
                    niTray.BalloonTipTitle = "fchan";
                    break;
                case (int)ChanTypes.Types.uEighteenChan:
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
            Show404 = true;
            changeTray.Start();
            niTray.ShowBalloonTip(5000);
            GC.Collect();
        }
        public void AnnounceAbort(string URL) {
            int ThreadIndex = ThreadURLS.IndexOf(URL);
            ThreadIsGone[ThreadIndex] = true;
        }
        public void SetItemImage(string URL, int ImageIndex) {
            int ItemIndex = ThreadURLS.IndexOf(URL);
            lvThreads.Items[ItemIndex].ImageIndex = ImageIndex;
        }
        public void SetItemStatus(string URL, string Status) {
            int ItemIndex = ThreadURLS.IndexOf(URL);
            lvThreads.Items[ItemIndex].SubItems[0].Text = Status;
        }

        private void frmMain_Load(object sender, EventArgs e) {

        }

        private void btnAdd_Click(object sender, EventArgs e) {
            if (Chans.SupportedChan(txtThreadURL.Text)) {
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems[0].Text = ThreadStatuses.Downloading;
                lvi.SubItems[1].Text = txtThreadURL.Text;
                lvi.Name = txtThreadURL.Text;
                lvThreads.Items.Add(lvi);
                ThreadURLS.Add(txtThreadURL.Text);
                ThreadIsGone.Add(false);
                frmDownloader newThread = new frmDownloader();
                newThread.Name = txtThreadURL.Text;
                newThread.ThreadURL = txtThreadURL.Text;
                newThread.StartDownload();
                Threads.Add(newThread);
                newThread.Show();
                txtThreadURL.Clear();
            }
        }

        private void mSettings_Click(object sender, EventArgs e) {
            frmSettings Settings = new frmSettings();
            Settings.ShowDialog();
            Settings.Dispose();
        }

        private void changeTray_Tick(object sender, EventArgs e) {
            if (Show404) {
                Show404 = false;
            }
            else {
                niTray.Icon = Properties.Resources.YChanEx;
                changeTray.Stop();
            }
        }

        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                Threads[lvThreads.SelectedIndices[0]].Show();
            }
        }

        private void mStatus_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                Threads[lvThreads.SelectedIndices[0]].Show();
            }
        }

        private void mRemove_Click(object sender, EventArgs e) {
            if (lvThreads.SelectedIndices.Count > 0) {
                int SelectedIndex = lvThreads.SelectedIndices[0];
                if (!ThreadIsGone[SelectedIndex]) {
                    Threads[SelectedIndex].StopDownload();
                }
                Threads[SelectedIndex].Dispose();
                Threads.RemoveAt(SelectedIndex);
                ThreadURLS.RemoveAt(SelectedIndex);
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