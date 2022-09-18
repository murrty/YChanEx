using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmNewName : Form {
        public frmNewName() {
            InitializeComponent();
            
        }

        private void btnSetName_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void txtNewName_KeyPress(object sender, KeyPressEventArgs e) {
            switch (e.KeyChar) {
                case (char)Keys.Enter:
                    e.Handled = true;
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    break;
            }
        }

        private void btnReset_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
        }
    }
}
