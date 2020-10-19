namespace YChanEx {
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
            this.lbTotal = new System.Windows.Forms.Label();
            this.lbTimeToRescan = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lbScanTimer = new System.Windows.Forms.Label();
            this.lbLastModified = new System.Windows.Forms.Label();
            this.ttDownloader = new System.Windows.Forms.ToolTip(this.components);
            this.lbNotModified = new System.Windows.Forms.Label();
            this.lvImages = new YChanEx.VistaListView();
            this.clID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clExt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clHash = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnForce404 = new System.Windows.Forms.Button();
            this.btnOpenFolder = new YChanEx.SplitButton();
            this.SuspendLayout();
            // 
            // tmrScan
            // 
            this.tmrScan.Interval = 1000;
            this.tmrScan.Tick += new System.EventHandler(this.tmrScan_Tick);
            // 
            // lbTotal
            // 
            this.lbTotal.AutoSize = true;
            this.lbTotal.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTotal.Location = new System.Drawing.Point(12, 255);
            this.lbTotal.Name = "lbTotal";
            this.lbTotal.Size = new System.Drawing.Size(110, 17);
            this.lbTotal.TabIndex = 1;
            this.lbTotal.Text = "number of files: 0";
            this.ttDownloader.SetToolTip(this.lbTotal, "The total number of files in the thread (scanned)");
            // 
            // lbTimeToRescan
            // 
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
            // lvImages
            // 
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
            this.clID.Width = 98;
            // 
            // clExt
            // 
            this.clExt.Text = "Ext";
            this.clExt.Width = 52;
            // 
            // clFileName
            // 
            this.clFileName.Text = "Original filename";
            this.clFileName.Width = 185;
            // 
            // clHash
            // 
            this.clHash.Text = "MD5 Hash";
            this.clHash.Width = 114;
            // 
            // btnForce404
            // 
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
            // btnOpenFolder
            // 
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOpenFolder.Location = new System.Drawing.Point(300, 285);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(99, 23);
            this.btnOpenFolder.TabIndex = 9;
            this.btnOpenFolder.Text = "Open folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // frmDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 320);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnForce404);
            this.Controls.Add(this.lbLastModified);
            this.Controls.Add(this.lbScanTimer);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lbTimeToRescan);
            this.Controls.Add(this.lbTotal);
            this.Controls.Add(this.lvImages);
            this.Controls.Add(this.lbNotModified);
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "frmDownloader";
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
        private System.Windows.Forms.Label lbTotal;
        private System.Windows.Forms.Label lbTimeToRescan;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lbScanTimer;
        private System.Windows.Forms.Label lbLastModified;
        private System.Windows.Forms.ToolTip ttDownloader;
        private System.Windows.Forms.Label lbNotModified;
        private System.Windows.Forms.Button btnForce404;
        private SplitButton btnOpenFolder;

    }
}