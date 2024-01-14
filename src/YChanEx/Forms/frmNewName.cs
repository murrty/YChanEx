#nullable enable
namespace YChanEx;
using System;
using System.Windows.Forms;
public partial class frmNewName : Form {
    public string SetName { get; private set; }

    public frmNewName(string currentName) {
        InitializeComponent();
        txtNewName.Text = currentName;
        SetName = currentName;
    }

    private void txtNewName_KeyDown(object sender, KeyEventArgs e) {
       if (e.KeyCode == Keys.Return) {
            e.Handled = e.SuppressKeyPress = true;
            if (txtNewName.Text.IsNullEmptyWhitespace()) {
                System.Media.SystemSounds.Exclamation.Play();
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
    private void btnSetName_Click(object sender, EventArgs e) {
        if (txtNewName.Text.IsNullEmptyWhitespace()) {
            txtNewName.Focus();
            System.Media.SystemSounds.Exclamation.Play();
            return;
        }
        SetName = txtNewName.Text;
        this.DialogResult = DialogResult.OK;
    }
    private void btnCancel_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Cancel;
    }
    private void btnReset_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.No;
    }
}
