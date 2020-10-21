using System;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmSettings : Form {
        private string FourChanURL = string.Empty;
        private string FourTwentyChanURL = string.Empty;
        private string SevenChanURL = string.Empty;
        private string SevenChanFiles = string.Empty;
        private string EightChanURL = string.Empty;
        private string EightKunURL = string.Empty;
        private string fchanURL = string.Empty;
        private string fchanFiles = string.Empty;
        private string u18chanURL = string.Empty;
        private string u18chanFiles = string.Empty;

        public frmSettings() {
            InitializeComponent();
            this.Icon = Properties.Resources.YChanEx;
            Program.SettingsOpen = true;
            LoadSettings();
        }

        private void btnSSave_Click(object sender, EventArgs e) {
            SaveSettings();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
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
            chkUseFullBoardNameForTitle.Checked = General.Default.UseFullBoardNameForTitle;

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
            General.Default.UseFullBoardNameForTitle = chkUseFullBoardNameForTitle.Checked;

            if (!string.IsNullOrEmpty(txtUserAgent.Text)) {
                Advanced.Default.UserAgent = txtUserAgent.Text;
            }
            else if (Advanced.Default.UserAgent != "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/64.0") {
                Advanced.Default.UserAgent = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/64.0";
            }
            Advanced.Default.DisableScanWhenOpeningSettings = chkDisableScannerWhenOpeningSettings.Checked;
            Advanced.Default.SilenceErrors = chkSilenceErrors.Checked;

            bool RegexChanged = false;

            if (RegexStrings.Default.FourChanURL != FourChanURL) {
                RegexStrings.Default.FourChanURL = FourChanURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.FourTwentyChanURL != FourTwentyChanURL) {
                RegexStrings.Default.FourTwentyChanURL = FourTwentyChanURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.SevenChanURL != SevenChanURL) {
                RegexStrings.Default.SevenChanURL = SevenChanURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.SevenChanFiles != SevenChanFiles) {
                RegexStrings.Default.SevenChanFiles = SevenChanFiles;
                RegexChanged = true;
            }
            if (RegexStrings.Default.EightChanURL != EightChanURL) {
                RegexStrings.Default.EightChanURL = EightChanURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.EightKunURL != EightKunURL) {
                RegexStrings.Default.EightKunURL = EightKunURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.fchanURL != fchanURL) {
                RegexStrings.Default.fchanURL = fchanURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.fchanFiles != fchanFiles) {
                RegexStrings.Default.fchanFiles = fchanFiles;
                RegexChanged = true;
            }
            if (RegexStrings.Default.u18chanURL != u18chanURL) {
                RegexStrings.Default.u18chanURL = u18chanURL;
                RegexChanged = true;
            }
            if (RegexStrings.Default.u18chanFiles != u18chanFiles) {
                RegexStrings.Default.u18chanFiles = u18chanFiles;
                RegexChanged = true;
            }

            Downloads.Default.Save();
            General.Default.Save();
            Advanced.Default.Save();
            if (RegexChanged) {
                RegexStrings.Default.Save();
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                fbd.Description = "Select a folder to save threads to";
                if (fbd.ShowDialog() == DialogResult.OK) {
                    txtSavePath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnSCan_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void lvRegex_SelectedIndexChanged(object sender, EventArgs e) {
            if (lvRegex.SelectedItems.Count > 0) {
                switch (lvRegex.SelectedItems[0].Index) {
                    case 0:
                        txtRegex.Text = RegexStrings.Default.FourChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.FourChanURL;
                        break;
                    case 1:
                        txtRegex.Text = RegexStrings.Default.FourTwentyChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.FourTwentyChanURL;
                        break;
                    case 2:
                        txtRegex.Text = RegexStrings.Default.SevenChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.SevenChanURL;
                        break;
                    case 3:
                        txtRegex.Text = RegexStrings.Default.SevenChanFiles;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.SevenChanFiles;
                        break;
                    case 4:
                        txtRegex.Text = RegexStrings.Default.EightChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.EightChanURL;
                        break;
                    case 5:
                        txtRegex.Text = RegexStrings.Default.EightKunURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.EightKunURL;
                        break;
                    case 6:
                        txtRegex.Text = RegexStrings.Default.fchanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.fchanURL;
                        break;
                    case 7:
                        txtRegex.Text = RegexStrings.Default.fchanFiles;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.fchanFiles;
                        break;
                    case 8:
                        txtRegex.Text = RegexStrings.Default.u18chanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.u18chanURL;
                        break;
                    case 9:
                        txtRegex.Text = RegexStrings.Default.u18chanFiles;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.u18chanFiles;
                        break;
                }
            }
            else {
                txtRegex.Text = "";
                txtRegex.TextHint = "";
            }
        }

        private void txtRegex_TextChanged(object sender, EventArgs e) {
            if (lvRegex.SelectedItems.Count > 0) {
                switch (lvRegex.SelectedItems[0].Index) {
                    case 0:
                        FourChanURL = txtRegex.Text;
                        break;
                    case 1:
                        FourTwentyChanURL = txtRegex.Text;
                        break;
                    case 2:
                        SevenChanURL = txtRegex.Text;
                        break;
                    case 3:
                        SevenChanFiles = txtRegex.Text;
                        break;
                    case 4:
                        EightChanURL = txtRegex.Text;
                        break;
                    case 5:
                        EightKunURL = txtRegex.Text;
                        break;
                    case 6:
                        fchanURL = txtRegex.Text;
                        break;
                    case 7:
                        fchanFiles = txtRegex.Text;
                        break;
                    case 8:
                        u18chanURL = txtRegex.Text;
                        break;
                    case 9:
                        u18chanFiles = txtRegex.Text;
                        break;
                }
            }
        }
    }
}
