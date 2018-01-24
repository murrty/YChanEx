using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YChanEx {
    public partial class frmUpdateInfo : Form {
        public frmUpdateInfo() {
            InitializeComponent();

            btnUpdate.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;
        }

        private void btnCancel_Click(object sender, EventArgs e) { }

        private void btnUpdate_Click(object sender, EventArgs e) { }

        private void frmUpdateInfo_Load(object sender, EventArgs e) {
            rtbNotice.Text = Updater.getCriticalInformation();
        }
    }
}
