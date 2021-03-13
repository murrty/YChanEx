using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmAbout : Form {
        //Language lang = Language.GetInstance();

        public frmAbout() {
            InitializeComponent();
            pbIcon.Image = Properties.Resources.ychanex32;
            pbIcon.Cursor = new Cursor(NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, (IntPtr)32649)));
        }
        private void frmAbout_Shown(object sender, EventArgs e) {
            lbVersion.Text = "v" + Properties.Settings.Default.AppVersion.ToString();
            if (Program.IsDebug) {
                lbVersion.Text += " (debug)";
            }
            lbBody.Text = lbBody.Text.Replace("{DEBUG}", Properties.Settings.Default.DebugDate);
        }

        private void llbCheckForUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            UpdateChecker.CheckForUpdate(true);
        }
        private void pbIcon_Click(object sender, EventArgs e) {
            Process.Start("https://github.com/murrty/YChanEx/");
        }

        private void llbGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("https://github.com/murrty/YChanEx/");
        }
    }
}
