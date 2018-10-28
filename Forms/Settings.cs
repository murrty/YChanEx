using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace YChanEx {
    public partial class Settings : Form {
        #region Variables
        public bool moveFolders = false;
        bool isAdmin = false;
        #endregion

        #region Form Methods
        public Settings() {
            InitializeComponent();

            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey("ychanex\\shell\\open\\command", false);
            if (regKey == null) {
                btnProtocol.Visible = true;
                btnProtocol.Enabled = true;
                if (!(new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator)) {
                    UACShield(btnProtocol);
                }
                else {
                    isAdmin = true;
                }
            }

            btnSSave.DialogResult = DialogResult.OK;
            btnSCan.DialogResult = DialogResult.Cancel;
        }

        private void Settings_Shown(object sender, EventArgs e) {
            loadSettings();
        }
        #endregion

        #region Custom Methods
        private void loadSettings() {
            edtPath.Text = YCSettings.Default.downloadPath;
            edtTimer.Value = (YCSettings.Default.scannerTimer / 1000);
            chkHTML.Checked = YCSettings.Default.htmlDownload;
            chkThumbnails.Checked = YCSettings.Default.downloadThumbnails;
            chkSave.Checked = YCSettings.Default.saveOnClose;
            chkShowTray.Checked = YCSettings.Default.trayIcon;
            chkMinimizeToTray.Checked = YCSettings.Default.minimizeToTray;
            chkWarn.Checked = YCSettings.Default.warnOnClose;
            chkUpdates.Checked = YCSettings.Default.updaterEnabled;
            chkHistory.Checked = YCSettings.Default.saveHistory;
            cb404.SelectedIndex = YCSettings.Default.threadDeadAction;
            chkOriginalNames.Checked = YCSettings.Default.originalName;
            chkSaveDate.Checked = YCSettings.Default.saveDate;
            chkLogErrors.Checked = YCSettings.Default.logErrorFiles;
            chkPreventDupes.Checked = YCSettings.Default.preventDupes;

            txtUserAgent.Text = Adv.Default.UserAgent;
            chkDisableScan.Checked = Adv.Default.settingsDisableScan;
            chkDisableErrors.Checked = Adv.Default.disableErrors;

            if (YCSettings.Default.firstStart) {
                chkMove.Checked = false;
                chkMove.Enabled = false;
            }
        }
        private void saveSettings() {
            if (YCSettings.Default.firstStart != false)
                YCSettings.Default.firstStart = false;
            if (YCSettings.Default.downloadPath != edtPath.Text)
                YCSettings.Default.downloadPath = edtPath.Text;
            if (YCSettings.Default.scannerTimer != (int)edtTimer.Value * 1000)
                YCSettings.Default.scannerTimer = (int)edtTimer.Value * 1000;
            if (YCSettings.Default.htmlDownload != chkHTML.Checked)
                YCSettings.Default.htmlDownload = chkHTML.Checked;
            if (YCSettings.Default.downloadThumbnails != chkThumbnails.Checked)
                YCSettings.Default.downloadThumbnails = chkThumbnails.Checked;
            if (YCSettings.Default.saveOnClose != chkSave.Checked)
                YCSettings.Default.saveOnClose = chkSave.Checked;
            if (YCSettings.Default.trayIcon != chkShowTray.Checked)
                YCSettings.Default.trayIcon = chkShowTray.Checked;
            if (YCSettings.Default.minimizeToTray != chkMinimizeToTray.Checked)
                YCSettings.Default.minimizeToTray = chkMinimizeToTray.Checked;
            if (YCSettings.Default.warnOnClose != chkWarn.Checked)
                YCSettings.Default.warnOnClose = chkWarn.Checked;
            if (YCSettings.Default.updaterEnabled != chkUpdates.Checked)
                YCSettings.Default.updaterEnabled = chkUpdates.Checked;
            if (YCSettings.Default.saveHistory != chkHistory.Checked)
                YCSettings.Default.saveHistory = chkHistory.Checked;
            if (YCSettings.Default.threadDeadAction != cb404.SelectedIndex)
                YCSettings.Default.threadDeadAction = cb404.SelectedIndex;
            if (YCSettings.Default.originalName != chkOriginalNames.Checked)
                YCSettings.Default.originalName = chkOriginalNames.Checked;
            if (YCSettings.Default.saveDate != chkSaveDate.Checked)
                YCSettings.Default.saveDate = chkSaveDate.Checked;
            if (YCSettings.Default.logErrorFiles != chkLogErrors.Checked)
                YCSettings.Default.logErrorFiles = chkLogErrors.Checked;
            if (YCSettings.Default.preventDupes != chkPreventDupes.Checked)
                YCSettings.Default.preventDupes = chkPreventDupes.Checked;

            if (Adv.Default.UserAgent != txtUserAgent.Text)
                Adv.Default.UserAgent = txtUserAgent.Text;
            if (Adv.Default.settingsDisableScan != chkDisableScan.Checked)
                Adv.Default.settingsDisableScan = chkDisableScan.Checked;
            if (Adv.Default.disableErrors != chkDisableErrors.Checked)
                Adv.Default.disableErrors = chkDisableErrors.Checked;

            YCSettings.Default.Save();
            Adv.Default.Save();
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        public static void UACShield(Button btn) {
            const Int32 BCM_SETSHIELD = 0x160C;
            btn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            SendMessage(btn.Handle, BCM_SETSHIELD, 0, 1);
        }
        #endregion

        #region Buttons
        private void btnSSave_Click(object sender, EventArgs e) {
            if (edtPath.Text != "") {

                if (edtPath.Text != YCSettings.Default.downloadPath && YCSettings.Default.firstStart == false)
                    if (chkMove.Checked)
                        moveFolders = true;

                saveSettings();
            }
        }
        private void btnSCan_Click(object sender, EventArgs e) { }
        private void btnBrowse_Click(object sender, EventArgs e) {
            FolderBrowserDialog FolD = new FolderBrowserDialog {
                Description = "Select folder to save downloaded files.",
                SelectedPath = @"C:\"
            };

            if (FolD.ShowDialog() == DialogResult.OK)
                edtPath.Text = FolD.SelectedPath;
        }
        private void btnReset_Click(object sender, EventArgs e) {
            if (rbRegular.Checked) {
                if (MessageBox.Show("This will reset your regular settings (Downloads & Application). Continue?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    YCSettings.Default.Reset();
                    YCSettings.Default.Save();
                    loadSettings();
                }
            }
            else if (rbAdvanced.Checked) {
                if (MessageBox.Show("This will only reset the advanced settings. Continue?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    Adv.Default.Reset();
                    Adv.Default.Save();
                    loadSettings();
                }
            }
            else if (rbAll.Checked) {
                if (MessageBox.Show("This will reset ALL your settings to default. Continue?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    YCSettings.Default.Reset();
                    YCSettings.Default.Save();
                    Adv.Default.Reset();
                    Adv.Default.Save();
                    loadSettings();
                }
            }
        }

        private void btnUserScript_Click(object sender, EventArgs e) {
            if (MessageBox.Show("This program supports a userscript to add a download button to the *chan boards, would you like to install it? You do need to install the protocol before this will fully function.", "YChanEx", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                Process.Start("https://github.com/murrty/YChanEx/raw/master/Plugin/YChanEx.user.js");
            }
        }
        private void btnProtocol_Click(object sender, EventArgs e) {
            if (!isAdmin) {
                if (MessageBox.Show("This task requires re-running as administrator. Restart elevated?", "YChanEx", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    startInfo.Arguments = "installProtocol";
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                return;
            }
            else {
                Controller.installProtocol();
            }
        }
        #endregion

        #region CheckBoxes
        private void chkHTML_CheckedChanged(object sender, EventArgs e) {
            if (chkHTML.Checked) {
                chkThumbnails.Enabled = true;
            }
            else {
                chkThumbnails.Enabled = false;
                chkThumbnails.Checked = false;
            }
        }
        private void chkShowTray_CheckedChanged(object sender, EventArgs e) {
            if (chkShowTray.Checked) {
                chkMinimizeToTray.Enabled = true;
            }
            else {
                chkMinimizeToTray.Enabled = false;
                chkMinimizeToTray.Checked = false;
            }
        }
        private void chkHistory_CheckedChanged(object sender, EventArgs e) {
            chkSaveDate.Enabled = chkHistory.Checked;
        }
        private void chkDisableErrors_CheckedChanged(object sender, EventArgs e) {
            chkLogErrors.Enabled = !chkDisableErrors.Checked;
        }
        private void chkOriginalNames_CheckedChanged(object sender, EventArgs e) {
            if (chkOriginalNames.Checked)
                chkPreventDupes.Enabled = true;
            else
                chkPreventDupes.Enabled = false;

            chkPreventDupes.Checked = false;
        }
        #endregion

        private void btnLocal_Click(object sender, EventArgs e) {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\YChanEx"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\YChanEx");

            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\YChanEx");
        }
    }
}
