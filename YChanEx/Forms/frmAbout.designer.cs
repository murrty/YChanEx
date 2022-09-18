﻿namespace YChanEx
{
    partial class frmAbout
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
        /// Initializes the form components.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        private void InitializeComponent()
        {
            this.lbVersion = new System.Windows.Forms.Label();
            this.lbHeader = new System.Windows.Forms.Label();
            this.lbBody = new System.Windows.Forms.Label();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.llbCheckForUpdates = new murrty.controls.ExtendedLinkLabel();
            this.llbGithub = new murrty.controls.ExtendedLinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lbVersion
            // 
            this.lbVersion.AutoSize = true;
            this.lbVersion.Location = new System.Drawing.Point(172, 14);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(28, 13);
            this.lbVersion.TabIndex = 0;
            this.lbVersion.Text = "v0.0";
            // 
            // lbHeader
            // 
            this.lbHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbHeader.Location = new System.Drawing.Point(12, 8);
            this.lbHeader.Name = "lbHeader";
            this.lbHeader.Size = new System.Drawing.Size(240, 20);
            this.lbHeader.TabIndex = 2;
            this.lbHeader.Text = "YChanEx";
            this.lbHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbBody
            // 
            this.lbBody.Location = new System.Drawing.Point(12, 36);
            this.lbBody.Name = "lbBody";
            this.lbBody.Size = new System.Drawing.Size(240, 79);
            this.lbBody.TabIndex = 3;
            this.lbBody.Text = "ychanex by murrty\r\ndebug date {DEBUG}\r\n\r\n\r\n\r\ndo it for likulau";
            this.lbBody.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pbIcon
            // 
            this.pbIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbIcon.InitialImage = null;
            this.pbIcon.Location = new System.Drawing.Point(58, 3);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(32, 32);
            this.pbIcon.TabIndex = 4;
            this.pbIcon.TabStop = false;
            this.pbIcon.Click += new System.EventHandler(this.pbIcon_Click);
            // 
            // llbCheckForUpdates
            // 
            this.llbCheckForUpdates.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.llbCheckForUpdates.AutoSize = true;
            this.llbCheckForUpdates.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.llbCheckForUpdates.Location = new System.Drawing.Point(85, 120);
            this.llbCheckForUpdates.Name = "llbCheckForUpdates";
            this.llbCheckForUpdates.Size = new System.Drawing.Size(94, 13);
            this.llbCheckForUpdates.TabIndex = 6;
            this.llbCheckForUpdates.TabStop = true;
            this.llbCheckForUpdates.Text = "Check for updates";
            this.llbCheckForUpdates.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))));
            this.llbCheckForUpdates.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbCheckForUpdates_LinkClicked);
            // 
            // llbGithub
            // 
            this.llbGithub.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.llbGithub.AutoSize = true;
            this.llbGithub.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.llbGithub.Location = new System.Drawing.Point(222, 120);
            this.llbGithub.Name = "llbGithub";
            this.llbGithub.Size = new System.Drawing.Size(38, 13);
            this.llbGithub.TabIndex = 7;
            this.llbGithub.TabStop = true;
            this.llbGithub.Text = "Github";
            this.llbGithub.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))));
            this.llbGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbGithub_LinkClicked);
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(262, 143);
            this.Controls.Add(this.pbIcon);
            this.Controls.Add(this.lbBody);
            this.Controls.Add(this.lbVersion);
            this.Controls.Add(this.lbHeader);
            this.Controls.Add(this.llbCheckForUpdates);
            this.Controls.Add(this.llbGithub);
            this.Icon = global::YChanEx.Properties.Resources.YChanEx;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(280, 180);
            this.MinimumSize = new System.Drawing.Size(280, 180);
            this.Name = "frmAbout";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About YChanEx";
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.Label lbHeader;
        private System.Windows.Forms.Label lbBody;
        private System.Windows.Forms.PictureBox pbIcon;
        private murrty.controls.ExtendedLinkLabel llbCheckForUpdates;
        private murrty.controls.ExtendedLinkLabel llbGithub;
    }
}