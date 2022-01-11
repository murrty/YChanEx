namespace YChanEx {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
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
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtThreadURL = new System.Windows.Forms.TextBox();
            this.mmMain = new System.Windows.Forms.MainMenu(this.components);
            this.mSettings = new System.Windows.Forms.MenuItem();
            this.mAbout = new System.Windows.Forms.MenuItem();
            this.niTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.changeTray = new System.Windows.Forms.Timer(this.components);
            this.cmThreads = new System.Windows.Forms.ContextMenu();
            this.mStatus = new System.Windows.Forms.MenuItem();
            this.mRetryDownload = new System.Windows.Forms.MenuItem();
            this.mSetCustomName = new System.Windows.Forms.MenuItem();
            this.mThreadsSep = new System.Windows.Forms.MenuItem();
            this.mOpenDownloadFolder = new System.Windows.Forms.MenuItem();
            this.mOpenThreadInBrowser = new System.Windows.Forms.MenuItem();
            this.mCopyThreadURL = new System.Windows.Forms.MenuItem();
            this.mCopyThreadID = new System.Windows.Forms.MenuItem();
            this.mThreadsSep2 = new System.Windows.Forms.MenuItem();
            this.mRemove = new System.Windows.Forms.MenuItem();
            this.cmTray = new System.Windows.Forms.ContextMenu();
            this.mTrayShowYChanEx = new System.Windows.Forms.MenuItem();
            this.mTraySep = new System.Windows.Forms.MenuItem();
            this.mTrayExit = new System.Windows.Forms.MenuItem();
            this.ilIcons = new System.Windows.Forms.ImageList(this.components);
            this.chkCreateThreadInTheBackground = new System.Windows.Forms.CheckBox();
            this.mAddThread = new System.Windows.Forms.MenuItem();
            this.lvThreads = new YChanEx.VistaListView();
            this.clIcon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clThread = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(442, 2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(48, 25);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // txtThreadURL
            // 
            this.txtThreadURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtThreadURL.Location = new System.Drawing.Point(12, 5);
            this.txtThreadURL.Name = "txtThreadURL";
            this.txtThreadURL.Size = new System.Drawing.Size(424, 22);
            this.txtThreadURL.TabIndex = 0;
            this.txtThreadURL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtThreadURL_KeyPress);
            // 
            // mmMain
            // 
            this.mmMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mSettings,
            this.mAbout});
            // 
            // mSettings
            // 
            this.mSettings.Index = 0;
            this.mSettings.Text = "Settings";
            this.mSettings.Click += new System.EventHandler(this.mSettings_Click);
            // 
            // mAbout
            // 
            this.mAbout.Index = 1;
            this.mAbout.Text = "About";
            this.mAbout.Click += new System.EventHandler(this.mAbout_Click);
            // 
            // niTray
            // 
            this.niTray.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.niTray.Icon = global::YChanEx.Properties.Resources.YChanEx;
            this.niTray.Text = "YChanEx";
            this.niTray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niTray_MouseDoubleClick);
            // 
            // changeTray
            // 
            this.changeTray.Interval = 5000;
            this.changeTray.Tick += new System.EventHandler(this.changeTray_Tick);
            // 
            // cmThreads
            // 
            this.cmThreads.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mStatus,
            this.mRetryDownload,
            this.mSetCustomName,
            this.mThreadsSep,
            this.mOpenDownloadFolder,
            this.mOpenThreadInBrowser,
            this.mCopyThreadURL,
            this.mCopyThreadID,
            this.mThreadsSep2,
            this.mRemove});
            this.cmThreads.Popup += new System.EventHandler(this.cmThreads_Popup);
            // 
            // mStatus
            // 
            this.mStatus.Index = 0;
            this.mStatus.Text = "View status(es)";
            this.mStatus.Click += new System.EventHandler(this.mStatus_Click);
            // 
            // mRetryDownload
            // 
            this.mRetryDownload.Index = 1;
            this.mRetryDownload.Text = "Retry download";
            this.mRetryDownload.Click += new System.EventHandler(this.mRetryDownload_Click);
            // 
            // mSetCustomName
            // 
            this.mSetCustomName.Index = 2;
            this.mSetCustomName.Text = "Set custom name";
            this.mSetCustomName.Click += new System.EventHandler(this.mSetCustomName_Click);
            // 
            // mThreadsSep
            // 
            this.mThreadsSep.Index = 3;
            this.mThreadsSep.Text = "-";
            // 
            // mOpenDownloadFolder
            // 
            this.mOpenDownloadFolder.Index = 4;
            this.mOpenDownloadFolder.Text = "Open download folder(s)";
            this.mOpenDownloadFolder.Click += new System.EventHandler(this.mOpenDownloadFolder_Click);
            // 
            // mOpenThreadInBrowser
            // 
            this.mOpenThreadInBrowser.Index = 5;
            this.mOpenThreadInBrowser.Text = "Open thread(s) in browser";
            this.mOpenThreadInBrowser.Click += new System.EventHandler(this.mOpenThreadInBrowser_Click);
            // 
            // mCopyThreadURL
            // 
            this.mCopyThreadURL.Index = 6;
            this.mCopyThreadURL.Text = "Copy thread url(s)";
            this.mCopyThreadURL.Click += new System.EventHandler(this.mCopyThreadURL_Click);
            // 
            // mCopyThreadID
            // 
            this.mCopyThreadID.Index = 7;
            this.mCopyThreadID.Text = "Copy thread id(s)";
            this.mCopyThreadID.Click += new System.EventHandler(this.mCopyThreadID_Click);
            // 
            // mThreadsSep2
            // 
            this.mThreadsSep2.Index = 8;
            this.mThreadsSep2.Text = "-";
            // 
            // mRemove
            // 
            this.mRemove.Index = 9;
            this.mRemove.Text = "Remove thread(s)";
            this.mRemove.Click += new System.EventHandler(this.mRemove_Click);
            // 
            // cmTray
            // 
            this.cmTray.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mTrayShowYChanEx,
            this.mAddThread,
            this.mTraySep,
            this.mTrayExit});
            // 
            // mTrayShowYChanEx
            // 
            this.mTrayShowYChanEx.Index = 0;
            this.mTrayShowYChanEx.Text = "Show YChanex";
            this.mTrayShowYChanEx.Click += new System.EventHandler(this.mTrayShowYChanEx_Click);
            // 
            // mTraySep
            // 
            this.mTraySep.Index = 2;
            this.mTraySep.Text = "-";
            // 
            // mTrayExit
            // 
            this.mTrayExit.Index = 3;
            this.mTrayExit.Text = "Exit";
            this.mTrayExit.Click += new System.EventHandler(this.mTrayExit_Click);
            // 
            // ilIcons
            // 
            this.ilIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.ilIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.ilIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // chkCreateThreadInTheBackground
            // 
            this.chkCreateThreadInTheBackground.AutoSize = true;
            this.chkCreateThreadInTheBackground.Location = new System.Drawing.Point(12, 33);
            this.chkCreateThreadInTheBackground.Name = "chkCreateThreadInTheBackground";
            this.chkCreateThreadInTheBackground.Size = new System.Drawing.Size(194, 17);
            this.chkCreateThreadInTheBackground.TabIndex = 2;
            this.chkCreateThreadInTheBackground.Text = "Create thread in the background";
            this.chkCreateThreadInTheBackground.UseVisualStyleBackColor = true;
            // 
            // mAddThread
            // 
            this.mAddThread.Index = 1;
            this.mAddThread.Text = "Add Thread (Clipboard)";
            this.mAddThread.Click += new System.EventHandler(this.mAddThread_Click);
            // 
            // lvThreads
            // 
            this.lvThreads.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvThreads.AutoArrange = false;
            this.lvThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clIcon,
            this.clStatus,
            this.clThread,
            this.clName});
            this.lvThreads.EnableVistaView = true;
            this.lvThreads.FullRowSelect = true;
            this.lvThreads.HideSelection = false;
            this.lvThreads.Location = new System.Drawing.Point(0, 56);
            this.lvThreads.Name = "lvThreads";
            this.lvThreads.Size = new System.Drawing.Size(502, 193);
            this.lvThreads.SmallImageList = this.ilIcons;
            this.lvThreads.TabIndex = 3;
            this.lvThreads.UseCompatibleStateImageBehavior = false;
            this.lvThreads.View = System.Windows.Forms.View.Details;
            this.lvThreads.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvThreads_MouseDoubleClick);
            // 
            // clIcon
            // 
            this.clIcon.Text = "*";
            this.clIcon.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.clIcon.Width = 24;
            // 
            // clStatus
            // 
            this.clStatus.Text = "Status";
            this.clStatus.Width = 90;
            // 
            // clThread
            // 
            this.clThread.Text = "Threads";
            this.clThread.Width = 90;
            // 
            // clName
            // 
            this.clName.Text = "Name";
            this.clName.Width = 290;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 251);
            this.Controls.Add(this.chkCreateThreadInTheBackground);
            this.Controls.Add(this.lvThreads);
            this.Controls.Add(this.txtThreadURL);
            this.Controls.Add(this.btnAdd);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::YChanEx.Properties.Resources.YChanEx;
            this.Menu = this.mmMain;
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YChanEx";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TextBox txtThreadURL;
        private System.Windows.Forms.MainMenu mmMain;
        private System.Windows.Forms.MenuItem mSettings;
        private System.Windows.Forms.MenuItem mAbout;
        private System.Windows.Forms.NotifyIcon niTray;
        private VistaListView lvThreads;
        private System.Windows.Forms.ColumnHeader clThread;
        private System.Windows.Forms.Timer changeTray;
        private System.Windows.Forms.ColumnHeader clStatus;
        private System.Windows.Forms.ContextMenu cmThreads;
        private System.Windows.Forms.MenuItem mStatus;
        private System.Windows.Forms.MenuItem mRemove;
        private System.Windows.Forms.MenuItem mRetryDownload;
        private System.Windows.Forms.ContextMenu cmTray;
        private System.Windows.Forms.MenuItem mTrayShowYChanEx;
        private System.Windows.Forms.MenuItem mTraySep;
        private System.Windows.Forms.MenuItem mTrayExit;
        private System.Windows.Forms.MenuItem mThreadsSep;
        private System.Windows.Forms.MenuItem mOpenDownloadFolder;
        private System.Windows.Forms.MenuItem mOpenThreadInBrowser;
        private System.Windows.Forms.MenuItem mCopyThreadURL;
        private System.Windows.Forms.MenuItem mCopyThreadID;
        private System.Windows.Forms.MenuItem mThreadsSep2;
        private System.Windows.Forms.ColumnHeader clName;
        private System.Windows.Forms.ImageList ilIcons;
        private System.Windows.Forms.ColumnHeader clIcon;
        private System.Windows.Forms.CheckBox chkCreateThreadInTheBackground;
        private System.Windows.Forms.MenuItem mSetCustomName;
        private System.Windows.Forms.MenuItem mAddThread;
    }
}

