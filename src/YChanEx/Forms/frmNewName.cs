namespace YChanEx;
using System.Windows.Forms;
public partial class frmNewName : Form {
    public frmNewName() {
        InitializeComponent();
    }

    private void btnSetName_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.OK;
    }

    private void btnCancel_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Cancel;
    }

    [System.Diagnostics.DebuggerStepThrough]
    private void txtNewName_KeyPress(object sender, KeyPressEventArgs e) {
        switch (e.KeyChar) {
            case (char)Keys.Enter:
                e.Handled = true;
                this.DialogResult = DialogResult.OK;
                break;
        }
    }

    private void btnReset_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.No;
    }
}
