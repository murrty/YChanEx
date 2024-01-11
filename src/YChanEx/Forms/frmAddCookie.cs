namespace YChanEx;

using System.Net;
using System.Windows.Forms;

public partial class frmAddCookie : Form {
    public Cookie Cookie { get; set; }
    public frmAddCookie() {
        InitializeComponent();
    }

    private void btnCancel_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Cancel;
    }

    private void btnAdd_Click(object sender, EventArgs e) {
        if (string.IsNullOrWhiteSpace(txtName.Text)) {
            txtName.Focus();
            System.Media.SystemSounds.Asterisk.Play();
            return;
        }
        if (string.IsNullOrWhiteSpace(txtValue.Text)) {
            txtValue.Focus();
            System.Media.SystemSounds.Asterisk.Play();
            return;
        }
        if (string.IsNullOrWhiteSpace(txtDomain.Text)) {
            txtDomain.Focus();
            System.Media.SystemSounds.Asterisk.Play();
            return;
        }
        Cookie = new(txtName.Text, txtValue.Text, txtPath.Text, txtDomain.Text);
        this.DialogResult = DialogResult.OK;
    }
}