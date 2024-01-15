#nullable enable
namespace YChanEx;
using System;
using System.Net;
using System.Windows.Forms;
public partial class frmAddCookie : Form {
    public SimpleCookie? Cookie { get; set; }

    public frmAddCookie() {
        InitializeComponent();
    }
    public frmAddCookie(SimpleCookie? cookie) : this() {
        if (cookie != null) {
            txtName.Text = cookie.Name.UnlessNull(string.Empty);
            txtValue.Text = cookie.Value.UnlessNull(string.Empty);
            txtPath.Text = cookie.Path.UnlessNull(string.Empty);
            txtDomain.Text = cookie.Domain.UnlessNull(string.Empty);
        }
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
        Cookie = new(txtName.Text, txtValue.Text, txtPath.Text.UnlessNullEmptyWhiteSpace("/"), txtDomain.Text);
        this.DialogResult = DialogResult.OK;
    }
}