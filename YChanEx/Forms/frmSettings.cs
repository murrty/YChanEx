using System.Windows.Forms;

namespace YChanEx {
    public partial class frmSettings : Form {

        #region Variables
        private string fchanFiles = string.Empty;
        private string fchanIDs = string.Empty;
        private string u18chanPosts = string.Empty;
        #endregion

        public frmSettings() {
            InitializeComponent();
            Program.SettingsOpen = true;
            LoadSettings();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            SaveSettings();
            this.DialogResult = DialogResult.OK;
        }

        private void LoadSettings() {
            txtSavePath.Text = Config.Settings.Downloads.DownloadPath;
            numTimer.Value = Config.Settings.Downloads.ScannerDelay;
            chkSaveOriginalFileNames.Checked = Config.Settings.Downloads.SaveOriginalFilenames;
            chkPreventDuplicates.Checked = Config.Settings.Downloads.PreventDuplicates;
            chkAllowFileNamesGreaterThan255.Checked = Config.Settings.Downloads.AllowFileNamesGreaterThan255;
            chkDownloadHTML.Checked = Config.Settings.Downloads.SaveHTML;
            chkCleanThreadHTML.Checked = Config.Settings.Downloads.CleanThreadHTML;
            chkDownloadThumbnails.Checked = Config.Settings.Downloads.SaveThumbnails;
            chkAutoRemoveDeadThreads.Checked = Config.Settings.Downloads.AutoRemoveDeadThreads;

            chkShowTrayIcon.Checked = Config.Settings.General.ShowTrayIcon;
            chkMinimizeToTray.Checked = Config.Settings.General.MinimizeToTray;
            chkMinimizeInsteadOfExiting.Checked = Config.Settings.General.MinimizeInsteadOfExiting;
            chkShowExitWarning.Checked = Config.Settings.General.ShowExitWarning;
            chkEnableUpdates.Checked = Config.Settings.General.EnableUpdates;
            chkUseFullBoardNameForTitle.Checked = Config.Settings.General.UseFullBoardNameForTitle;
            chkSaveDownloadQueueOnExit.Checked = Config.Settings.General.SaveQueueOnExit;
            chkSaveDownloadHistory.Checked = Config.Settings.General.SaveThreadHistory;

            txtUserAgent.Text = Config.Settings.Advanced.UserAgent;
            chkDisableScannerWhenOpeningSettings.Checked = Config.Settings.Advanced.DisableScanWhenOpeningSettings;
            chkSilenceErrors.Checked = Config.Settings.Advanced.SilenceErrors;

            if (!SystemRegistry.ProtocolExists) {
                if (!Program.IsAdmin) {
                    btnProtocol.Text = " " + btnProtocol.Text;
                    NativeMethods.SendMessage(btnProtocol.Handle, NativeMethods.BCM_SETSHIELD, IntPtr.Zero, (IntPtr)2);
                }
                btnProtocol.Visible = true;
            }
            else {
                btnProtocol.Visible = false;
                btnProtocol.Enabled = false;
            }
        }

        private void SaveSettings() {
            Config.Settings.Downloads.DownloadPath = txtSavePath.Text;
            Config.Settings.Downloads.ScannerDelay = (int)numTimer.Value;
            Config.Settings.Downloads.SaveOriginalFilenames = chkSaveOriginalFileNames.Checked;
            Config.Settings.Downloads.PreventDuplicates = chkPreventDuplicates.Checked;
            Config.Settings.Downloads.AllowFileNamesGreaterThan255 = chkAllowFileNamesGreaterThan255.Checked;
            Config.Settings.Downloads.SaveHTML = chkDownloadHTML.Checked;
            Config.Settings.Downloads.CleanThreadHTML = chkCleanThreadHTML.Checked;
            Config.Settings.Downloads.SaveThumbnails = chkDownloadThumbnails.Checked;
            Config.Settings.Downloads.AutoRemoveDeadThreads = chkAutoRemoveDeadThreads.Checked;

            Config.Settings.General.ShowTrayIcon = chkShowTrayIcon.Checked;
            Config.Settings.General.MinimizeToTray = chkMinimizeToTray.Checked;
            Config.Settings.General.MinimizeInsteadOfExiting = chkMinimizeInsteadOfExiting.Checked;
            Config.Settings.General.ShowExitWarning = chkShowExitWarning.Checked;
            Config.Settings.General.EnableUpdates = chkEnableUpdates.Checked;
            Config.Settings.General.UseFullBoardNameForTitle = chkUseFullBoardNameForTitle.Checked;
            Config.Settings.General.SaveQueueOnExit = chkSaveDownloadQueueOnExit.Checked;
            Config.Settings.General.SaveThreadHistory = chkSaveDownloadHistory.Checked;

            Config.Settings.Advanced.UserAgent = string.IsNullOrWhiteSpace(txtUserAgent.Text) ? Config_Advanced.DefaultUserAgent : txtUserAgent.Text;
            Config.Settings.Advanced.DisableScanWhenOpeningSettings = chkDisableScannerWhenOpeningSettings.Checked;
            Config.Settings.Advanced.SilenceErrors = chkSilenceErrors.Checked;

            Config.Settings.Regex.fchanFiles = fchanFiles;
            Config.Settings.Regex.fchanIDs = fchanIDs;
            Config.Settings.Regex.u18chanPosts = u18chanPosts;

            Config.Save();
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            using BetterFolderBrowserNS.BetterFolderBrowser fbd = new();
            fbd.Title = "Select a folder to save threads to...";
            fbd.RootFolder = Config.Settings.Downloads.DownloadPath;
            if (fbd.ShowDialog() == DialogResult.OK) {
                if (chkMoveExistingDownloads.Checked) {
                    if (!System.IO.Directory.Exists(fbd.SelectedPath)) {
                        System.IO.Directory.CreateDirectory(fbd.SelectedPath);
                    }
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

        private void btnCancel_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOpenLocalFiles_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(Environment.CurrentDirectory);
        }

        private void lvRegex_SelectedIndexChanged(object sender, EventArgs e) {
            if (lvRegex.SelectedItems.Count > 0) {
                switch (lvRegex.SelectedItems[0].Index) {
                    case 0:
                        txtRegex.Text = Config.Settings.Regex.fchanFiles;
                        txtRegex.TextHint = Chans.DefaultRegex.fchanFiles;
                        lbRegexHint.Text = "This is a file pattern for fchan, it parses raw HTML for image links.";
                        break;
                    case 1:
                        txtRegex.Text = Config.Settings.Regex.fchanIDs;
                        txtRegex.TextHint = Chans.DefaultRegex.fchanFiles;
                        lbRegexHint.Text = "This is a file name pattern for fchan, it parses raw HTML for post IDs.";
                        break;
                    case 2:
                        txtRegex.Text = Config.Settings.Regex.u18chanPosts;
                        txtRegex.TextHint = Chans.DefaultRegex.u18chanPosts;
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
                        fchanFiles = txtRegex.Text;
                        break;
                    case 2:
                        fchanIDs = txtRegex.Text;
                        break;
                    case 3:
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
                Config.Settings.Downloads.Reset();
            }
            if (chkResetApplicationSettings.Checked) {
                Config.Settings.General.Reset();
            }
            if (chkResetAdvancedSettings.Checked) {
                Config.Settings.Advanced.Reset();
            }
            if (chkResetRegexSettings.Checked) {
                Config.Settings.Regex.Reset();
            }

            LoadSettings();
            chkEnableSettingsReset.Checked = false;
            btnResetSettings.Enabled = false;
        }

        private void btnUserScript_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://raw.githubusercontent.com/murrty/YChanEx/master/Resources/YChanEx.user.js");
        }

        private void btnProtocol_Click(object sender, EventArgs e) {
            if (MessageBox.Show($"Setting the protocol will allow webbrowsers to send URLs to this program using the \"ychanex:\" protocol. It's recommended to save this program in a static location. Do you want to point the protocol to \"{Program.FullApplicationPath}\"?", "YChanEx", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                using System.Diagnostics.Process InstallProtocol = new() {
                    StartInfo = new() {
                        Arguments = "--protocol",
                        FileName = Program.FullApplicationPath,
                        Verb = "runas",
                        WorkingDirectory = System.IO.Path.GetDirectoryName(Program.FullApplicationPath)
                    }
                };
                InstallProtocol.Start();
            }
        }

    }
}
