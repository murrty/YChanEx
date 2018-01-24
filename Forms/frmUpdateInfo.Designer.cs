namespace YChanEx
{
    partial class frmUpdateInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbNotice = new System.Windows.Forms.Label();
            this.rtbNotice = new System.Windows.Forms.RichTextBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbCritNotice = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbNotice
            // 
            this.lbNotice.AutoSize = true;
            this.lbNotice.Location = new System.Drawing.Point(66, 12);
            this.lbNotice.Name = "lbNotice";
            this.lbNotice.Size = new System.Drawing.Size(128, 13);
            this.lbNotice.TabIndex = 0;
            this.lbNotice.Text = "Critical update information";
            // 
            // rtbNotice
            // 
            this.rtbNotice.Location = new System.Drawing.Point(12, 37);
            this.rtbNotice.Name = "rtbNotice";
            this.rtbNotice.ReadOnly = true;
            this.rtbNotice.Size = new System.Drawing.Size(236, 170);
            this.rtbNotice.TabIndex = 1;
            this.rtbNotice.Text = "";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(165, 239);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(83, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update now";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(84, 239);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Update later";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbCritNotice
            // 
            this.lbCritNotice.AutoSize = true;
            this.lbCritNotice.Location = new System.Drawing.Point(30, 219);
            this.lbCritNotice.Name = "lbCritNotice";
            this.lbCritNotice.Size = new System.Drawing.Size(200, 13);
            this.lbCritNotice.TabIndex = 4;
            this.lbCritNotice.Text = "It\'s highly recommended that you update.";
            // 
            // frmUpdateInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 270);
            this.Controls.Add(this.lbCritNotice);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.rtbNotice);
            this.Controls.Add(this.lbNotice);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmUpdateInfo";
            this.Text = "Critical update notice";
            this.Load += new System.EventHandler(this.frmUpdateInfo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbNotice;
        private System.Windows.Forms.RichTextBox rtbNotice;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbCritNotice;
    }
}