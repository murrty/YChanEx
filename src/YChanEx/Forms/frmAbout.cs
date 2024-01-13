namespace YChanEx;
using System;
using System.Diagnostics;
using System.Windows.Forms;
public partial class frmAbout : Form {
    private const string BodyText = """
ychanex by murrty
build date {0}

Shrim heals me

do it for likulau
""";
    public frmAbout() {
        InitializeComponent();
        pbIcon.Image = Properties.Resources.AboutImage;
        pbIcon.Cursor = new Cursor(NativeMethods.LoadCursor(0, NativeMethods.IDC_HAND));
        lbVersion.Text = $"v{Program.CurrentVersion}{(Program.DebugMode ? " (deubg)" : "")}";
        lbBody.Text = string.Format(BodyText, Properties.Resources.BuildDate);
    }

    private void llbCheckForUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        UpdateChecker.CheckForUpdate(false, true);
    }

    private void pbIcon_Click(object sender, EventArgs e) {
        Process.Start(Program.GithubPage);
    }

    private void llbGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        Process.Start(Program.GithubPage);
    }
}