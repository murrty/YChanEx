using System.Windows.Forms;

namespace YChanEx {

    public partial class frmUpdateAvailable : Form {

        /// <summary>
        /// The update that is available.
        /// </summary>
        internal GithubData UpdateData { get; } = null;

        public frmUpdateAvailable(GithubData UpdateData, bool BlockSkip = false) {
            InitializeComponent();
            this.UpdateData = UpdateData;
            this.Text = "YChanEx update";
            lbUpdateAvailableHeader.Text = "A YChanEx update is available";
            lbUpdateAvailableCurrentVersion.Text = $"Current version: {Program.CurrentVersion}";
            lbUpdateAvailableUpdateVersion.Text = $"New version: {UpdateData.Version}";
            lbUpdateAvailableChangelog.Text = "Changelog:";
            txtUpdateAvailableName.Text = UpdateData.VersionHeader;
            rtbUpdateAvailableChangelog.Text = UpdateData.VersionDescription;
            lbUpdateSize.Text = $"The new executable size is {HtmlControl.GetSize(UpdateData.GetExecutableSize())}";
            btnUpdateAvailableUpdate.Text = "Update";
            btnUpdateAvailableSkip.Text = "Skip version";
            btnUpdateAvailableOk.Text = "OK";

            btnUpdateAvailableSkip.Enabled = !BlockSkip;
            this.Shown += (s, e) => lbUpdateAvailableHeader.Focus();
        }

        private void btnUpdateAvailableSkip_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Ignore;
        }

        private void btnUpdateAvailableUpdate_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Yes;
        }

        private void btnUpdateAvailableOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
        }
    }
}
