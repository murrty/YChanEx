using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmMain : Form {
        List<frmDownloader> Threads = new List<frmDownloader>();
        List<string> ThreadURLS = new List<string>();
        bool Show404 = false;

        public frmMain() {
            InitializeComponent();
            niTray.Icon = Properties.Resources.YChanEx;
            this.Icon = Properties.Resources.YChanEx;
        }
        public void Announce404(string ThreadID, string ThreadBoard, string URL) {
            int ThreadIndex = ThreadURLS.IndexOf(URL);
            niTray.BalloonTipText = ThreadID + " on board " + ThreadBoard + " has 404'd";
            niTray.BalloonTipTitle = "404";
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
            if (!Chans.SupportedChan(txtThreadURL.Text)) {
                return;
            }
            ListViewItem lvi = new ListViewItem();
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
            lvi.SubItems[0].Text = ThreadStatuses.Downloading;
            lvi.SubItems[1].Text = txtThreadURL.Text;
            lvi.Name = txtThreadURL.Text;
            lvThreads.Items.Add(lvi);
            ThreadURLS.Add(txtThreadURL.Text);
            frmDownloader newThread = new frmDownloader();
            newThread.Name = txtThreadURL.Text;
            newThread.ThreadURL = txtThreadURL.Text;
            newThread.StartDownload();
            Threads.Add(newThread);
            newThread.Show();
            //newThread.Hide();
            txtThreadURL.Clear();
        }

        private void lbThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            //int ClickedIndex = lbThreads.IndexFromPoint(e.Location);
            //if (ClickedIndex != ListBox.NoMatches) {
            //    Threads[ClickedIndex].Show();
            //    Threads[ClickedIndex].Opacity = 100;
            //    Threads[ClickedIndex].ShowInTaskbar = true;
            //}
        }

        private void mSettings_Click(object sender, EventArgs e) {
            frmSettings Settings = new frmSettings();
            Settings.ShowDialog();
            Settings.Dispose();
        }

        private void changeTray_Tick(object sender, EventArgs e) {
            if (!Show404) {
                niTray.Icon = Properties.Resources.YChanEx;
                Show404 = false;
                changeTray.Stop();
            }
        }

        private void lvThreads_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lvThreads.SelectedItems.Count > 0) {
                Threads[lvThreads.SelectedIndices[0]].Show();
            }
        }
    }
}