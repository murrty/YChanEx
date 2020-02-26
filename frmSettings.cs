using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmSettings : Form {
        public frmSettings() {
            InitializeComponent();
            this.Icon = Properties.Resources.YChanEx;
            LoadSettings();
        }

        private void btnSSave_Click(object sender, EventArgs e) {
            SaveSettings();
        }

        void LoadSettings() {
            txtSavePath.Text = Downloads.Default.DownloadPath;
            numTimer.Value = Downloads.Default.ScannerDelay;
            chkDownloadHTML.Checked = Downloads.Default.SaveHTML;
            chkDownloadThumbnails.Checked = Downloads.Default.SaveThumbnails;
            chkSaveDownloadQueueOnExit.Checked = Downloads.Default.SaveQueueOnExit;
            chkSaveOriginalFileNames.Checked = Downloads.Default.SaveOriginalFilenames;
            chkPreventDuplicates.Checked = Downloads.Default.PreventDuplicates;

            chkShowTrayIcon.Checked = General.Default.ShowTrayIcon;
            chkMinimizeToTray.Checked = General.Default.MinimizeToTray;
            chkShowExitWarning.Checked = General.Default.ShowExitWarning;
            chkEnableUpdates.Checked = General.Default.EnableUpdates;

            txtUserAgent.Text = Advanced.Default.UserAgent;
            chkDisableScannerWhenOpeningSettings.Checked = Advanced.Default.DisableScanWhenOpeningSettings;
            chkSilenceErrors.Checked = Advanced.Default.SilenceErrors;
        }
        void SaveSettings() {
            if (txtSavePath.Text != Downloads.Default.DownloadPath) {
                Downloads.Default.DownloadPath = txtSavePath.Text;
            }
            Downloads.Default.ScannerDelay = (int)numTimer.Value;
            Downloads.Default.SaveHTML = chkDownloadHTML.Checked;
            Downloads.Default.SaveThumbnails = chkDownloadThumbnails.Checked;
            Downloads.Default.SaveQueueOnExit = chkSaveDownloadQueueOnExit.Checked;
            Downloads.Default.SaveOriginalFilenames = chkSaveOriginalFileNames.Checked;
            Downloads.Default.PreventDuplicates = chkPreventDuplicates.Checked;

            General.Default.ShowTrayIcon = chkShowTrayIcon.Checked;
            General.Default.MinimizeToTray = chkMinimizeToTray.Checked;
            General.Default.ShowExitWarning = chkShowExitWarning.Checked;
            General.Default.EnableUpdates = chkEnableUpdates.Checked;

            if (!string.IsNullOrEmpty(txtUserAgent.Text)) {
                Advanced.Default.UserAgent = txtUserAgent.Text;
            }
            else if (Advanced.Default.UserAgent != "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/64.0") {
                Advanced.Default.UserAgent = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/64.0";
            }
            Advanced.Default.DisableScanWhenOpeningSettings = chkDisableScannerWhenOpeningSettings.Checked;
            Advanced.Default.SilenceErrors = chkSilenceErrors.Checked;

            Downloads.Default.Save();
            General.Default.Save();
            Advanced.Default.Save();
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                fbd.Description = "Select a folder to save threads to";
                if (fbd.ShowDialog() == DialogResult.OK) {
                    txtSavePath.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
