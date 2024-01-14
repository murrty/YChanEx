#nullable enable
namespace YChanEx;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using murrty.classes;

public partial class frmAbout : Form {
    private const string BodyText = """
ychanex by murrty
build date {0}

Shrim heals me

do it for likulau
""";
    private Task UpdateTask = Task.CompletedTask;

    public frmAbout() {
        InitializeComponent();
        pbIcon.Image = Properties.Resources.AboutImage;
        pbIcon.Cursor = new Cursor(NativeMethods.LoadCursor(0, NativeMethods.IDC_HAND));
        lbVersion.Text = $"v{Program.CurrentVersion}{(Program.DebugMode ? " (deubg)" : "")}";
        lbBody.Text = string.Format(BodyText, Properties.Resources.BuildDate);
    }

    private async Task CheckUpdate() {
        try {
            var result = await Updater.CheckForUpdate(true);
            if (result == null) {
                Log.Warn("could not get update.");
                MessageBox.Show("Could not find update.");
                return;
            }

            if (result == true) {
                Updater.ShowUpdateForm(false);
            }
            else {
                MessageBox.Show("No update is available.");
            }
        }
        catch {
            Log.Warn("could not get update.");
            MessageBox.Show("Could not find update.");
        }
    }
    private void llbCheckForUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        if (!UpdateTask.IsCompleted) {
            return;
        }
        UpdateTask = CheckUpdate();
    }
    private void pbIcon_Click(object sender, EventArgs e) {
        Process.Start(Program.GithubPage);
    }
    private void llbGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
        Process.Start(Program.GithubPage);
    }
}