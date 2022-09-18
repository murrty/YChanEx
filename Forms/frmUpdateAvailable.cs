using System.Windows.Forms;

namespace YChanEx {
    public partial class frmUpdateAvailable : Form {
        public bool BlockSkip { get; set; } = false;

        public frmUpdateAvailable() {
            InitializeComponent();
            txtUpdateAvailableName.Text = UpdateChecker.LastChecked.VersionHeader;
            rtbUpdateAvailableChangelog.Text = UpdateChecker.LastChecked.VersionDescription;
            lbUpdateAvailableUpdateVersion.Text = $"Update version: {UpdateChecker.LastChecked.VersionTag}";
            lbUpdateAvailableCurrentVersion.Text = $"Current version: {Program.CurrentVersion}";
        }

        private void frmUpdateAvailable_Load(object sender, EventArgs e) {
            btnUpdateAvailableSkip.Enabled = !BlockSkip;
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
