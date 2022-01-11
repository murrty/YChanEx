using System;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmSettings : Form {
        #region Variables for regex strings
        private string FourChanURL = string.Empty;
        private string FourTwentyChanURL = string.Empty;
        private string SevenChanURL = string.Empty;
        private string SevenChanPosts = string.Empty;
        private string EightChanURL = string.Empty;
        private string EightKunURL = string.Empty;
        private string fchanURL = string.Empty;
        private string fchanFiles = string.Empty;
        private string fchanIDs = string.Empty;
        private string u18chanURL = string.Empty;
        private string u18chanPosts = string.Empty;
        #endregion

        public frmSettings() {
            InitializeComponent();
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
            chkSaveOriginalFileNames.Checked = Downloads.Default.SaveOriginalFilenames;
            chkPreventDuplicates.Checked = Downloads.Default.PreventDuplicates;
            chkAllowFileNamesGreaterThan255.Checked = Downloads.Default.AllowFileNamesGreaterThan255;
            chkDownloadHTML.Checked = Downloads.Default.SaveHTML;
            chkDownloadThumbnails.Checked = Downloads.Default.SaveThumbnails;
            chkRetrieveThreadName.Checked = Downloads.Default.UseThreadName;

            chkShowTrayIcon.Checked = General.Default.ShowTrayIcon;
            chkMinimizeToTray.Checked = General.Default.MinimizeToTray;
            chkShowExitWarning.Checked = General.Default.ShowExitWarning;
            chkEnableUpdates.Checked = General.Default.EnableUpdates;
            chkUseFullBoardNameForTitle.Checked = General.Default.UseFullBoardNameForTitle;
            chkSaveDownloadQueueOnExit.Checked = General.Default.SaveQueueOnExit;

            txtUserAgent.Text = Advanced.Default.UserAgent;
            chkDisableScannerWhenOpeningSettings.Checked = Advanced.Default.DisableScanWhenOpeningSettings;
            chkSilenceErrors.Checked = Advanced.Default.SilenceErrors;
        }
        void SaveSettings() {
            if (txtSavePath.Text != Downloads.Default.DownloadPath) {
                Downloads.Default.DownloadPath = txtSavePath.Text;
            }
            Downloads.Default.ScannerDelay = (int)numTimer.Value;
            Downloads.Default.SaveOriginalFilenames = chkSaveOriginalFileNames.Checked;
            Downloads.Default.PreventDuplicates = chkPreventDuplicates.Checked;
            Downloads.Default.AllowFileNamesGreaterThan255 = chkAllowFileNamesGreaterThan255.Checked;
            Downloads.Default.SaveHTML = chkDownloadHTML.Checked;
            Downloads.Default.SaveThumbnails = chkDownloadThumbnails.Checked;
            Downloads.Default.UseThreadName = chkRetrieveThreadName.Checked;

            General.Default.ShowTrayIcon = chkShowTrayIcon.Checked;
            General.Default.MinimizeToTray = chkMinimizeToTray.Checked;
            General.Default.ShowExitWarning = chkShowExitWarning.Checked;
            General.Default.EnableUpdates = chkEnableUpdates.Checked;
            General.Default.UseFullBoardNameForTitle = chkUseFullBoardNameForTitle.Checked;
            General.Default.SaveQueueOnExit = chkSaveDownloadQueueOnExit.Checked;

            if (!string.IsNullOrEmpty(txtUserAgent.Text)) {
                Advanced.Default.UserAgent = txtUserAgent.Text;
            }
            else if (Advanced.Default.UserAgent != "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/84.0") {
                Advanced.Default.UserAgent = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/84.0";
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
            if (RegexStrings.Default.SevenChanPosts != SevenChanPosts) {
                RegexStrings.Default.SevenChanPosts = SevenChanPosts;
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
            if (RegexStrings.Default.u18chanPosts != u18chanPosts) {
                RegexStrings.Default.u18chanPosts = u18chanPosts;
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
                    if (chkMoveExistingDownloads.Checked) {
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\4chan")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\4chan", fbd.SelectedPath + "\\4chan");
                        }
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\420chan")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\420chan", fbd.SelectedPath + "\\420chan");
                        }
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\7chan")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\7chan", fbd.SelectedPath + "\\7chan");
                        }
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\8chan")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\8chan", fbd.SelectedPath + "\\8chan");
                        }
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\8kun")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\8kun", fbd.SelectedPath + "\\8kun");
                        }
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\fchan")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\fchan", fbd.SelectedPath + "\\fchan");
                        }
                        if (System.IO.Directory.Exists(txtSavePath.Text + "\\u18chan")) {
                            System.IO.Directory.Move(txtSavePath.Text + "\\u18chan", fbd.SelectedPath + "\\u18chan");
                        }
                    }
                    txtSavePath.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnSCan_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOpenLocalFiles_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(Program.ApplicationFilesLocation);
        }

        private void lvRegex_SelectedIndexChanged(object sender, EventArgs e) {
            if (lvRegex.SelectedItems.Count > 0) {
                switch (lvRegex.SelectedItems[0].Index) {
                    case 0:
                        txtRegex.Text = RegexStrings.Default.FourChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.FourChanURL;
                        lbRegexHint.Text = "This is the URL pattern of a 4chan thread, used to detect if it's a real 4chan link.";
                        break;
                    case 1:
                        txtRegex.Text = RegexStrings.Default.FourTwentyChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.FourTwentyChanURL;
                        lbRegexHint.Text = "This is the URL pattern of a 420chan thread, used to detect if it's a real 420chan link.";
                        break;
                    case 2:
                        txtRegex.Text = RegexStrings.Default.SevenChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.SevenChanURL;
                        lbRegexHint.Text = "This is the URL pattern of a 7chan thread, used to detect if it's a real 4chan link.";
                        break;
                    case 3:
                        txtRegex.Text = RegexStrings.Default.SevenChanPosts;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.SevenChanPosts;
                        lbRegexHint.Text = "This is a post pattern for 7chan, it parses raw HTML for image links and post IDs.";
                        break;
                    case 4:
                        txtRegex.Text = RegexStrings.Default.EightChanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.EightChanURL;
                        lbRegexHint.Text = "This is the URL pattern of a 8chan thread, used to detect if it's a real 4chan link.";
                        break;
                    case 5:
                        txtRegex.Text = RegexStrings.Default.EightKunURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.EightKunURL;
                        lbRegexHint.Text = "This is the URL pattern of a 8kun thread, used to detect if it's a real 4chan link.";
                        break;
                    case 6:
                        txtRegex.Text = RegexStrings.Default.fchanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.fchanURL;
                        lbRegexHint.Text = "This is the URL pattern of a fchan thread, used to detect if it's a real fchan link.";
                        break;
                    case 7:
                        txtRegex.Text = RegexStrings.Default.fchanFiles;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.fchanFiles;
                        lbRegexHint.Text = "This is a file pattern for fchan, it parses raw HTML for image links.";
                        break;
                    case 8:
                        txtRegex.Text = RegexStrings.Default.fchanIDs;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.fchanFiles;
                        lbRegexHint.Text = "This is a file name pattern for fchan, it parses raw HTML for post IDs.";
                        break;
                    case 9:
                        txtRegex.Text = RegexStrings.Default.u18chanURL;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.u18chanURL;
                        lbRegexHint.Text = "This is the URL pattern of a u18chan thread, used to detect if it's a real u18chan link.";
                        break;
                    case 10:
                        txtRegex.Text = RegexStrings.Default.u18chanPosts;
                        txtRegex.TextHint = ChanRegex.DefaultRegex.u18chanPosts;
                        lbRegexHint.Text = "This is a post pattern for u18chanchan, it parses raw HTML for image links and post IDs.";
                        break;
                    default:
                        txtRegex.Text = "Unknown Regex Pattern";
                        txtRegex.TextHint = "Unknown Regex Pattern";
                        lbRegexHint.Text = "An error occured somewhere, and a pattern wasn't properly selected.";
                        break;
                }
            }
            else {
                txtRegex.Text = "";
                txtRegex.TextHint = "";
                lbRegexHint.Text = "Select a pattern to the left if you want to change them.\nIf you are unsure what you're doing, don't do anything here.";
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
                        SevenChanPosts = txtRegex.Text;
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
                        fchanIDs = txtRegex.Text;
                        break;
                    case 9:
                        u18chanURL = txtRegex.Text;
                        break;
                    case 10:
                        u18chanPosts = txtRegex.Text;
                        break;
                }
            }
        }

        private void chkEnableSettingsReset_CheckedChanged(object sender, EventArgs e) {
            btnResetSettings.Enabled = chkEnableSettingsReset.Checked;
        }

        private void btnResetSettings_Click(object sender, EventArgs e) {
            if (chkResetDownloadSettings.Checked) {
                Downloads.Default.Reset();
                Downloads.Default.Save();
                Downloads.Default.Reload();
            }
            if (chkResetApplicationSettings.Checked) {
                General.Default.Reset();
                General.Default.Save();
                General.Default.Reload();
            }
            if (chkResetAdvancedSettings.Checked) {
                Advanced.Default.Reset();
                Advanced.Default.Save();
                Advanced.Default.Reload();
            }
            if (chkResetRegexSettings.Checked) {
                RegexStrings.Default.Reset();
                RegexStrings.Default.Save();
                RegexStrings.Default.Reload();
            }

            LoadSettings();
            chkEnableSettingsReset.Checked = false;
            btnResetSettings.Enabled = false;
        }

        private void btnUserScript_Click(object sender, EventArgs e) {

        }
    }
}
