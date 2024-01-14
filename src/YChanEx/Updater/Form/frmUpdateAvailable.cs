#nullable enable
namespace YChanEx;
using System.Windows.Forms;
using murrty.updater;
internal sealed partial class frmUpdateAvailable : Form {
    /// <summary>
    /// Whether the "Skip version" button should be disabled, false if it's an automatic check.
    /// </summary>
    public bool BlockSkip { get; init; }

    /// <summary>
    /// The update that is available.
    /// </summary>
    internal GithubData UpdateData { get; }

    public frmUpdateAvailable(GithubData UpdateData) {
        InitializeComponent();
        this.UpdateData = UpdateData;
        lbUpdateAvailableCurrentVersion.Text = $"Current version: {Program.CurrentVersion}";
    }

    private void frmUpdateAvailable_Load(object? sender, EventArgs e) {
        btnUpdateAvailableSkip.Enabled = !BlockSkip;
        if (UpdateData.IsBetaVersion) {
            lbUpdateAvailableHeader.Text = "A beta update is available";
        }
        lbUpdateAvailableUpdateVersion.Text = $"Update version: {UpdateData.Version}";
        txtUpdateAvailableName.Text = UpdateData.VersionHeader ?? "No header provided";
        rtbUpdateAvailableChangelog.Text = UpdateData.VersionDescription ?? "No description provided.";
        lbUpdateSize.Text = $"The new executable size is {HtmlControl.GetSize(UpdateData.ExecutableSize)}";
    }

    private void btnUpdateAvailableSkip_Click(object? sender, EventArgs e) {
        this.DialogResult = DialogResult.Ignore;
    }

    private void btnUpdateAvailableUpdate_Click(object? sender, EventArgs e) {
        this.DialogResult = DialogResult.Yes;
    }

    private void btnUpdateAvailableOk_Click(object? sender, EventArgs e) {
        this.DialogResult = DialogResult.OK;
    }

    private void rtbUpdateAvailableChangelog_KeyPress(object? sender, KeyPressEventArgs e) {
        e.Handled = true;
    }
}
