using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YChanEx {
    public partial class Settings : Form {

        #region Variables
        public bool moveFolders = false;
        #endregion

        #region Settings (Settings_Shown)
        public Settings() {
            InitializeComponent();

            btnSSave.DialogResult = DialogResult.OK;
            btnSCan.DialogResult = DialogResult.Cancel;
        }

        private void Settings_Shown(object sender, EventArgs e) {
            loadSettings();
        }
        #endregion

        #region Custom (loadSettings)
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
        #endregion

        #region Buttons (btnSSave_Click / btnSCan_Click / btnBrowse_Click / btnReset_Click)
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
        #endregion

        #region CheckBoxes (chkHTML_CheckChanged / chkShowTray_CheckedChanged / chkHistory_CheckedChanged / chkDisableErrors_CheckedChanged)
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
        #endregion

        private void chkOriginalNames_CheckedChanged(object sender, EventArgs e) {
            if (chkOriginalNames.Checked)
                chkPreventDupes.Enabled = true;
            else 
                chkPreventDupes.Enabled = false;

            chkPreventDupes.Checked = false;
        }
    }
}
