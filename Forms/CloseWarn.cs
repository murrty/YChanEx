using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YChanEx
{
    public partial class CloseWarn : Form
    {

        #region Variables
        public bool saveonclose;
        public bool dontshowagain;
        #endregion

        #region Form
        public CloseWarn()
        {
            InitializeComponent();

            btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnNoClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
        private void CloseWarn_Load(object sender, EventArgs e)
        {
            saveonclose = YCSettings.Default.saveOnClose;
            this.chkSave.Checked = YCSettings.Default.saveOnClose;
        }
        #endregion
        #region Buttons
        private void BtnClose_Click(object sender, EventArgs e)
        {
            // this.DialogResult = DialogResult.OK;
        }
        private void BtnNoClose_Click(object sender, EventArgs e)
        {
            // this.Close();
        }
        #endregion
        #region CheckBoxes
        private void ChkWarning_CheckedChanged(object sender, EventArgs e)
        {
            if (chkWarning.Checked)
                YCSettings.Default.warnOnClose = true;
            else
                YCSettings.Default.warnOnClose = false;
        }
        private void ChkSave_CheckedChanged(object sender, EventArgs e)
        {
            if (YCSettings.Default.saveOnClose != chkSave.Checked)
                YCSettings.Default.saveOnClose = chkSave.Checked;
        }
        #endregion
    }
}
