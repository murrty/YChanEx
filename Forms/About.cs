using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YChanEx {
    public partial class About : Form {

        public About() { InitializeComponent(); }

        private void About_Shown(object sender, EventArgs e) { lbVersion.Text = "v" + Properties.Settings.Default.currentVersion.ToString(); }

        private void llbCheckForUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            decimal cV = Updater.getCloudVersion();

            if (Updater.isUpdateAvailable(cV)) {
                if (Updater.isUpdateCritical()) {
                    frmUpdateInfo uInfo = new frmUpdateInfo();
                    if (uInfo.ShowDialog() == System.Windows.Forms.DialogResult.Yes) {
                        Updater.createUpdaterStub();
                        Updater.runUpdater(cV);
                        return;
                    }
                    uInfo.Close();
                    uInfo.Dispose();
                } else {
                    if (MessageBox.Show("An update is available. \nNew verison: " + cV.ToString() + " | Your version: " + Properties.Settings.Default.currentVersion.ToString() + "\n\nWould you like to update?", "YChanEx", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Updater.createUpdaterStub();
                        Updater.runUpdater(cV);
                        return;
                    }
                }
            }
            else { 
                MessageBox.Show("No update is available at this time.");
            }
        }

        private void pbIcon_Click(object sender, EventArgs e) { Process.Start("https://github.com/murrty/ychanex/"); }

    }
}
