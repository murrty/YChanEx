namespace YChanEx
{
    partial class LicenseSource
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenseSource));
            this.rtbSrcLc = new System.Windows.Forms.RichTextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbSrcLc
            // 
            this.rtbSrcLc.BackColor = System.Drawing.SystemColors.Control;
            this.rtbSrcLc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbSrcLc.Location = new System.Drawing.Point(12, 13);
            this.rtbSrcLc.Name = "rtbSrcLc";
            this.rtbSrcLc.ReadOnly = true;
            this.rtbSrcLc.Size = new System.Drawing.Size(270, 123);
            this.rtbSrcLc.TabIndex = 0;
            this.rtbSrcLc.Text = resources.GetString("rtbSrcLc.Text");
            this.rtbSrcLc.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbSrcLc_LinkClicked);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(207, 149);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // LicenseSource
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(294, 182);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.rtbSrcLc);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(310, 220);
            this.MinimumSize = new System.Drawing.Size(310, 220);
            this.Name = "LicenseSource";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "License and Sorce";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbSrcLc;
        private System.Windows.Forms.Button btnClose;
    }
}