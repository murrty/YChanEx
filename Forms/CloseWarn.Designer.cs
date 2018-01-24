namespace YChanEx
{
    partial class CloseWarn
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CloseWarn));
            this.lblText = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnNoClose = new System.Windows.Forms.Button();
            this.chkWarning = new System.Windows.Forms.CheckBox();
            this.chkSave = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblText
            // 
            this.lblText.AutoSize = true;
            this.lblText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblText.Location = new System.Drawing.Point(18, 11);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(364, 16);
            this.lblText.TabIndex = 4;
            this.lblText.Text = "Are you sure you want to exit? Threads are still downloading.";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(230, 68);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // btnNoClose
            // 
            this.btnNoClose.Location = new System.Drawing.Point(311, 68);
            this.btnNoClose.Name = "btnNoClose";
            this.btnNoClose.Size = new System.Drawing.Size(75, 23);
            this.btnNoClose.TabIndex = 3;
            this.btnNoClose.Text = "Cancel";
            this.btnNoClose.UseVisualStyleBackColor = true;
            this.btnNoClose.Click += new System.EventHandler(this.BtnNoClose_Click);
            // 
            // chkWarning
            // 
            this.chkWarning.AutoSize = true;
            this.chkWarning.Location = new System.Drawing.Point(57, 70);
            this.chkWarning.Name = "chkWarning";
            this.chkWarning.Size = new System.Drawing.Size(167, 17);
            this.chkWarning.TabIndex = 1;
            this.chkWarning.Text = "Don\'t show this warning again";
            this.chkWarning.UseVisualStyleBackColor = true;
            this.chkWarning.CheckedChanged += new System.EventHandler(this.ChkWarning_CheckedChanged);
            // 
            // chkSave
            // 
            this.chkSave.AutoSize = true;
            this.chkSave.Location = new System.Drawing.Point(84, 38);
            this.chkSave.Name = "chkSave";
            this.chkSave.Size = new System.Drawing.Size(232, 17);
            this.chkSave.TabIndex = 0;
            this.chkSave.Text = "Save threads and load them on next startup";
            this.chkSave.UseVisualStyleBackColor = true;
            this.chkSave.CheckedChanged += new System.EventHandler(this.ChkSave_CheckedChanged);
            // 
            // CloseWarn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(400, 102);
            this.ControlBox = false;
            this.Controls.Add(this.chkSave);
            this.Controls.Add(this.chkWarning);
            this.Controls.Add(this.btnNoClose);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(416, 140);
            this.MinimumSize = new System.Drawing.Size(416, 140);
            this.Name = "CloseWarn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Closing YChanEx";
            this.Load += new System.EventHandler(this.CloseWarn_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnNoClose;
        private System.Windows.Forms.CheckBox chkWarning;
        private System.Windows.Forms.CheckBox chkSave;
    }
}