﻿namespace YChanEx {
    partial class frmDownloader {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (DownloadThread.IsAlive) {
                DownloadThread.Abort();
            }
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tmrScan = new System.Windows.Forms.Timer(this.components);
            this.lbNumberOfFiles = new System.Windows.Forms.Label();
            this.lbTimeToRescan = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lbScanTimer = new System.Windows.Forms.Label();
            this.lbLastModified = new System.Windows.Forms.Label();
            this.ttDownloader = new System.Windows.Forms.ToolTip(this.components);
            this.lbNotModified = new System.Windows.Forms.Label();
            this.btnForce404 = new System.Windows.Forms.Button();
            this.cmThreadActions = new System.Windows.Forms.ContextMenu();
            this.mOpenThreadDownloadFolder = new System.Windows.Forms.MenuItem();
            this.mOpenThreadInBrowser = new System.Windows.Forms.MenuItem();
            this.mCopyThreadID = new System.Windows.Forms.MenuItem();
            this.mCopyThreadURL = new System.Windows.Forms.MenuItem();
            this.btnAbortRetry = new System.Windows.Forms.Button();
            this.lbFileCountSeparator = new System.Windows.Forms.Label();
            this.lbDownloadedFiles = new System.Windows.Forms.Label();
            this.lbTotalFiles = new System.Windows.Forms.Label();
            this.ilStatus = new System.Windows.Forms.ImageList(this.components);
            this.btnOpenFolder = new YChanEx.SplitButton();
            this.lvImages = new YChanEx.VistaListView();
            this.clID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clExt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clHash = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnPauseTimer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tmrScan
            // 
            this.tmrScan.Interval = 1000;
            this.tmrScan.Tick += new System.EventHandler(this.tmrScan_Tick);
            // 
            // lbNumberOfFiles
            // 
            this.lbNumberOfFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbNumberOfFiles.AutoSize = true;
            this.lbNumberOfFiles.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNumberOfFiles.Location = new System.Drawing.Point(12, 255);
            this.lbNumberOfFiles.Name = "lbNumberOfFiles";
            this.lbNumberOfFiles.Size = new System.Drawing.Size(99, 17);
            this.lbNumberOfFiles.TabIndex = 1;
            this.lbNumberOfFiles.Text = "number of files:";
            this.ttDownloader.SetToolTip(this.lbNumberOfFiles, "The total number of files in the thread (scanned)");
            // 
            // lbTimeToRescan
            // 
            this.lbTimeToRescan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbTimeToRescan.AutoSize = true;
            this.lbTimeToRescan.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTimeToRescan.Location = new System.Drawing.Point(12, 293);
            this.lbTimeToRescan.Name = "lbTimeToRescan";
            this.lbTimeToRescan.Size = new System.Drawing.Size(126, 17);
            this.lbTimeToRescan.TabIndex = 2;
            this.lbTimeToRescan.Text = "time until next scan: ";
            this.ttDownloader.SetToolTip(this.lbTimeToRescan, "The time until the next scan will start");
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(405, 285);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lbScanTimer
            // 
            this.lbScanTimer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbScanTimer.AutoSize = true;
            this.lbScanTimer.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbScanTimer.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbScanTimer.Location = new System.Drawing.Point(131, 293);
            this.lbScanTimer.Name = "lbScanTimer";
            this.lbScanTimer.Size = new System.Drawing.Size(110, 17);
            this.lbScanTimer.TabIndex = 5;
            this.lbScanTimer.Text = "scanning thread...";
            this.ttDownloader.SetToolTip(this.lbScanTimer, "haha");
            // 
            // lbLastModified
            // 
            this.lbLastModified.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbLastModified.AutoSize = true;
            this.lbLastModified.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLastModified.Location = new System.Drawing.Point(12, 274);
            this.lbLastModified.Name = "lbLastModified";
            this.lbLastModified.Size = new System.Drawing.Size(123, 17);
            this.lbLastModified.TabIndex = 6;
            this.lbLastModified.Text = "last modified: never";
            this.ttDownloader.SetToolTip(this.lbLastModified, "The last time the thread was updated.\r\nIf the thread hasn\'t been posted to since " +
        "this time, it won\'t download any files.");
            // 
            // lbNotModified
            // 
            this.lbNotModified.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbNotModified.AutoSize = true;
            this.lbNotModified.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNotModified.Location = new System.Drawing.Point(203, 293);
            this.lbNotModified.Name = "lbNotModified";
            this.lbNotModified.Size = new System.Drawing.Size(83, 17);
            this.lbNotModified.TabIndex = 7;
            this.lbNotModified.Text = "not modified";
            this.ttDownloader.SetToolTip(this.lbNotModified, "The thread has not been modified since last download.");
            this.lbNotModified.Visible = false;
            // 
            // btnForce404
            // 
            this.btnForce404.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForce404.Enabled = false;
            this.btnForce404.Location = new System.Drawing.Point(300, 256);
            this.btnForce404.Name = "btnForce404";
            this.btnForce404.Size = new System.Drawing.Size(99, 24);
            this.btnForce404.TabIndex = 8;
            this.btnForce404.Text = "404";
            this.btnForce404.UseVisualStyleBackColor = true;
            this.btnForce404.Visible = false;
            this.btnForce404.Click += new System.EventHandler(this.btnForce404_Click);
            // 
            // cmThreadActions
            // 
            this.cmThreadActions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mOpenThreadDownloadFolder,
            this.mOpenThreadInBrowser,
            this.mCopyThreadID,
            this.mCopyThreadURL});
            // 
            // mOpenThreadDownloadFolder
            // 
            this.mOpenThreadDownloadFolder.Index = 0;
            this.mOpenThreadDownloadFolder.Text = "Open download folder";
            this.mOpenThreadDownloadFolder.Click += new System.EventHandler(this.mOpenThreadDownloadFolder_Click);
            // 
            // mOpenThreadInBrowser
            // 
            this.mOpenThreadInBrowser.Index = 1;
            this.mOpenThreadInBrowser.Text = "Open thread in browser";
            this.mOpenThreadInBrowser.Click += new System.EventHandler(this.mOpenThreadInBrowser_Click);
            // 
            // mCopyThreadID
            // 
            this.mCopyThreadID.Index = 2;
            this.mCopyThreadID.Text = "Copy thread ID";
            this.mCopyThreadID.Click += new System.EventHandler(this.mCopyThreadID_Click);
            // 
            // mCopyThreadURL
            // 
            this.mCopyThreadURL.Index = 3;
            this.mCopyThreadURL.Text = "Copy thread URL";
            this.mCopyThreadURL.Click += new System.EventHandler(this.mCopyThreadURL_Click);
            // 
            // btnAbortRetry
            // 
            this.btnAbortRetry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbortRetry.Location = new System.Drawing.Point(405, 256);
            this.btnAbortRetry.Name = "btnAbortRetry";
            this.btnAbortRetry.Size = new System.Drawing.Size(75, 23);
            this.btnAbortRetry.TabIndex = 10;
            this.btnAbortRetry.Text = "Abort";
            this.btnAbortRetry.UseVisualStyleBackColor = true;
            this.btnAbortRetry.Click += new System.EventHandler(this.btnAbortRetry_Click);
            // 
            // lbFileCountSeparator
            // 
            this.lbFileCountSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbFileCountSeparator.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbFileCountSeparator.Location = new System.Drawing.Point(116, 254);
            this.lbFileCountSeparator.Name = "lbFileCountSeparator";
            this.lbFileCountSeparator.Size = new System.Drawing.Size(64, 17);
            this.lbFileCountSeparator.TabIndex = 11;
            this.lbFileCountSeparator.Text = "/";
            this.lbFileCountSeparator.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbDownloadedFiles
            // 
            this.lbDownloadedFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbDownloadedFiles.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDownloadedFiles.Location = new System.Drawing.Point(108, 255);
            this.lbDownloadedFiles.Name = "lbDownloadedFiles";
            this.lbDownloadedFiles.Size = new System.Drawing.Size(36, 17);
            this.lbDownloadedFiles.TabIndex = 13;
            this.lbDownloadedFiles.Text = "0";
            this.lbDownloadedFiles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbTotalFiles
            // 
            this.lbTotalFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbTotalFiles.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTotalFiles.Location = new System.Drawing.Point(152, 255);
            this.lbTotalFiles.Name = "lbTotalFiles";
            this.lbTotalFiles.Size = new System.Drawing.Size(36, 17);
            this.lbTotalFiles.TabIndex = 14;
            this.lbTotalFiles.Text = "0";
            this.lbTotalFiles.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ilStatus
            // 
            this.ilStatus.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ilStatus.ImageSize = new System.Drawing.Size(16, 16);
            this.ilStatus.TransparentColor = System.Drawing.Color.Fuchsia;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.DropDownContextMenu = this.cmThreadActions;
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOpenFolder.Location = new System.Drawing.Point(300, 285);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(99, 23);
            this.btnOpenFolder.TabIndex = 9;
            this.btnOpenFolder.Text = "Open folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // lvImages
            // 
            this.lvImages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvImages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clID,
            this.clExt,
            this.clFileName,
            this.clHash});
            this.lvImages.EnableVistaView = true;
            this.lvImages.Location = new System.Drawing.Point(12, 12);
            this.lvImages.Name = "lvImages";
            this.lvImages.Size = new System.Drawing.Size(468, 234);
            this.lvImages.TabIndex = 0;
            this.lvImages.UseCompatibleStateImageBehavior = false;
            this.lvImages.View = System.Windows.Forms.View.Details;
            this.lvImages.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvImages_MouseDoubleClick);
            // 
            // clID
            // 
            this.clID.Text = "ID";
            this.clID.Width = 110;
            // 
            // clExt
            // 
            this.clExt.Text = "Ext";
            this.clExt.Width = 46;
            // 
            // clFileName
            // 
            this.clFileName.Text = "Original filename";
            this.clFileName.Width = 201;
            // 
            // clHash
            // 
            this.clHash.Text = "File Hash";
            this.clHash.Width = 106;
            // 
            // btnPauseTimer
            // 
            this.btnPauseTimer.Enabled = false;
            this.btnPauseTimer.Location = new System.Drawing.Point(219, 257);
            this.btnPauseTimer.Name = "btnPauseTimer";
            this.btnPauseTimer.Size = new System.Drawing.Size(75, 23);
            this.btnPauseTimer.TabIndex = 15;
            this.btnPauseTimer.Text = "Pause Tmr";
            this.btnPauseTimer.UseVisualStyleBackColor = true;
            this.btnPauseTimer.Visible = false;
            this.btnPauseTimer.Click += new System.EventHandler(this.btnPauseTimer_Click);
            // 
            // frmDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 320);
            this.Controls.Add(this.btnPauseTimer);
            this.Controls.Add(this.lbTotalFiles);
            this.Controls.Add(this.lbDownloadedFiles);
            this.Controls.Add(this.btnAbortRetry);
            this.Controls.Add(this.lbNotModified);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnForce404);
            this.Controls.Add(this.lbLastModified);
            this.Controls.Add(this.lbScanTimer);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lbTimeToRescan);
            this.Controls.Add(this.lbNumberOfFiles);
            this.Controls.Add(this.lvImages);
            this.Controls.Add(this.lbFileCountSeparator);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::YChanEx.Properties.Resources.YChanEx;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "frmDownloader";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.Text = "unknown chan download";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDownloader_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private VistaListView lvImages;
        private System.Windows.Forms.ColumnHeader clID;
        private System.Windows.Forms.ColumnHeader clExt;
        private System.Windows.Forms.ColumnHeader clFileName;
        private System.Windows.Forms.ColumnHeader clHash;
        private System.Windows.Forms.Timer tmrScan;
        private System.Windows.Forms.Label lbNumberOfFiles;
        private System.Windows.Forms.Label lbTimeToRescan;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lbScanTimer;
        private System.Windows.Forms.Label lbLastModified;
        private System.Windows.Forms.ToolTip ttDownloader;
        private System.Windows.Forms.Label lbNotModified;
        private System.Windows.Forms.Button btnForce404;
        private SplitButton btnOpenFolder;
        private System.Windows.Forms.ContextMenu cmThreadActions;
        private System.Windows.Forms.Button btnAbortRetry;
        private System.Windows.Forms.Label lbFileCountSeparator;
        private System.Windows.Forms.Label lbDownloadedFiles;
        private System.Windows.Forms.Label lbTotalFiles;
        private System.Windows.Forms.ImageList ilStatus;
        private System.Windows.Forms.MenuItem mOpenThreadDownloadFolder;
        private System.Windows.Forms.MenuItem mOpenThreadInBrowser;
        private System.Windows.Forms.MenuItem mCopyThreadID;
        private System.Windows.Forms.MenuItem mCopyThreadURL;
        private System.Windows.Forms.Button btnPauseTimer;

    }
}