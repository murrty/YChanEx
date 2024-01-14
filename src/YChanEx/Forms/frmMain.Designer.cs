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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("4chan");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("420chan");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("7chan");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("8chan");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("8kun");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("fchan");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("u18chan");
            System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("foolfuuka");
            this.btnAdd = new System.Windows.Forms.Button();
            this.txtThreadURL = new System.Windows.Forms.TextBox();
            this.mmMain = new System.Windows.Forms.MainMenu(this.components);
            this.mSettings = new System.Windows.Forms.MenuItem();
            this.mLog = new System.Windows.Forms.MenuItem();
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
            this.mAddThread = new System.Windows.Forms.MenuItem();
            this.mTraySep = new System.Windows.Forms.MenuItem();
            this.mTrayExit = new System.Windows.Forms.MenuItem();
            this.ilIcons = new System.Windows.Forms.ImageList(this.components);
            this.chkCreateThreadInTheBackground = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpCurrentQueue = new System.Windows.Forms.TabPage();
            this.lvThreads = new murrty.controls.ExtendedListView();
            this.clStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clThread = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tpHistory = new System.Windows.Forms.TabPage();
            this.tvHistory = new System.Windows.Forms.TreeView();
            this.btnHistoryClear = new System.Windows.Forms.Button();
            this.btnHistoryRemove = new System.Windows.Forms.Button();
            this.btnHistoryRedownload = new System.Windows.Forms.Button();
            this.lbThreadHistoryHint = new System.Windows.Forms.Label();
            this.tpDebug = new System.Windows.Forms.TabPage();
            this.pnUpper = new System.Windows.Forms.Panel();
            this.tabControl1.SuspendLayout();
            this.tpCurrentQueue.SuspendLayout();
            this.tpHistory.SuspendLayout();
            this.pnUpper.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(394, 2);
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
            this.txtThreadURL.Size = new System.Drawing.Size(376, 22);
            this.txtThreadURL.TabIndex = 0;
            this.txtThreadURL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtThreadURL_KeyPress);
            // 
            // mmMain
            // 
            this.mmMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mSettings,
            this.mLog,
            this.mAbout});
            // 
            // mSettings
            // 
            this.mSettings.Index = 0;
            this.mSettings.Text = "Settings";
            this.mSettings.Click += new System.EventHandler(this.mSettings_Click);
            // 
            // mLog
            // 
            this.mLog.Index = 1;
            this.mLog.Text = "Log";
            this.mLog.Click += new System.EventHandler(this.mLog_Click);
            // 
            // mAbout
            // 
            this.mAbout.Index = 2;
            this.mAbout.Text = "About";
            this.mAbout.Click += new System.EventHandler(this.mAbout_Click);
            // 
            // niTray
            // 
            this.niTray.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.niTray.Icon = global::YChanEx.Properties.Resources.ProgramIcon;
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
            // mAddThread
            // 
            this.mAddThread.Index = 1;
            this.mAddThread.Text = "Add Thread (Clipboard)";
            this.mAddThread.Click += new System.EventHandler(this.mAddThread_Click);
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
            this.chkCreateThreadInTheBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCreateThreadInTheBackground.AutoSize = true;
            this.chkCreateThreadInTheBackground.Location = new System.Drawing.Point(258, 32);
            this.chkCreateThreadInTheBackground.Name = "chkCreateThreadInTheBackground";
            this.chkCreateThreadInTheBackground.Size = new System.Drawing.Size(194, 17);
            this.chkCreateThreadInTheBackground.TabIndex = 2;
            this.chkCreateThreadInTheBackground.Text = "Create thread in the background";
            this.chkCreateThreadInTheBackground.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpCurrentQueue);
            this.tabControl1.Controls.Add(this.tpHistory);
            this.tabControl1.Controls.Add(this.tpDebug);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 33);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(454, 261);
            this.tabControl1.TabIndex = 4;
            // 
            // tpCurrentQueue
            // 
            this.tpCurrentQueue.Controls.Add(this.lvThreads);
            this.tpCurrentQueue.Location = new System.Drawing.Point(4, 22);
            this.tpCurrentQueue.Name = "tpCurrentQueue";
            this.tpCurrentQueue.Padding = new System.Windows.Forms.Padding(3);
            this.tpCurrentQueue.Size = new System.Drawing.Size(446, 235);
            this.tpCurrentQueue.TabIndex = 0;
            this.tpCurrentQueue.Text = "Queue";
            this.tpCurrentQueue.UseVisualStyleBackColor = true;
            // 
            // lvThreads
            // 
            this.lvThreads.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvThreads.AutoArrange = false;
            this.lvThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clStatus,
            this.clThread,
            this.clName});
            this.lvThreads.FullRowSelect = true;
            this.lvThreads.HideSelection = false;
            this.lvThreads.Location = new System.Drawing.Point(6, 6);
            this.lvThreads.Name = "lvThreads";
            this.lvThreads.Size = new System.Drawing.Size(434, 223);
            this.lvThreads.SmallImageList = this.ilIcons;
            this.lvThreads.TabIndex = 3;
            this.lvThreads.UseCompatibleStateImageBehavior = false;
            this.lvThreads.View = System.Windows.Forms.View.Details;
            this.lvThreads.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvThreads_MouseDoubleClick);
            // 
            // clStatus
            // 
            this.clStatus.Text = "Status";
            this.clStatus.Width = 106;
            // 
            // clThread
            // 
            this.clThread.Text = "Threads";
            this.clThread.Width = 118;
            // 
            // clName
            // 
            this.clName.Text = "Name";
            this.clName.Width = 204;
            // 
            // tpHistory
            // 
            this.tpHistory.Controls.Add(this.tvHistory);
            this.tpHistory.Controls.Add(this.btnHistoryClear);
            this.tpHistory.Controls.Add(this.btnHistoryRemove);
            this.tpHistory.Controls.Add(this.btnHistoryRedownload);
            this.tpHistory.Controls.Add(this.lbThreadHistoryHint);
            this.tpHistory.Location = new System.Drawing.Point(4, 22);
            this.tpHistory.Name = "tpHistory";
            this.tpHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tpHistory.Size = new System.Drawing.Size(446, 235);
            this.tpHistory.TabIndex = 1;
            this.tpHistory.Text = "Thread history";
            this.tpHistory.UseVisualStyleBackColor = true;
            // 
            // tvHistory
            // 
            this.tvHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvHistory.Location = new System.Drawing.Point(6, 29);
            this.tvHistory.Name = "tvHistory";
            treeNode1.Name = "node4chan";
            treeNode1.Text = "4chan";
            treeNode2.Name = "node420chan";
            treeNode2.Text = "420chan";
            treeNode3.Name = "node7chan";
            treeNode3.Text = "7chan";
            treeNode4.Name = "node8chan";
            treeNode4.Text = "8chan";
            treeNode5.Name = "node8kun";
            treeNode5.Text = "8kun";
            treeNode6.Name = "nodefchan";
            treeNode6.Text = "fchan";
            treeNode7.Name = "nodeu18chan";
            treeNode7.Text = "u18chan";
            treeNode8.Name = "foolfuuka";
            treeNode8.Text = "foolfuuka";
            this.tvHistory.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7,
            treeNode8});
            this.tvHistory.Size = new System.Drawing.Size(342, 200);
            this.tvHistory.TabIndex = 5;
            this.tvHistory.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvHistory_AfterSelect);
            // 
            // btnHistoryClear
            // 
            this.btnHistoryClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistoryClear.Enabled = false;
            this.btnHistoryClear.Location = new System.Drawing.Point(354, 246);
            this.btnHistoryClear.Name = "btnHistoryClear";
            this.btnHistoryClear.Size = new System.Drawing.Size(84, 23);
            this.btnHistoryClear.TabIndex = 4;
            this.btnHistoryClear.Text = "Remove all";
            this.btnHistoryClear.UseVisualStyleBackColor = true;
            this.btnHistoryClear.Click += new System.EventHandler(this.btnHistoryClear_Click);
            // 
            // btnHistoryRemove
            // 
            this.btnHistoryRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistoryRemove.Enabled = false;
            this.btnHistoryRemove.Location = new System.Drawing.Point(354, 56);
            this.btnHistoryRemove.Name = "btnHistoryRemove";
            this.btnHistoryRemove.Size = new System.Drawing.Size(86, 23);
            this.btnHistoryRemove.TabIndex = 3;
            this.btnHistoryRemove.Text = "Remove";
            this.btnHistoryRemove.UseVisualStyleBackColor = true;
            this.btnHistoryRemove.Click += new System.EventHandler(this.btnHistoryRemove_Click);
            // 
            // btnHistoryRedownload
            // 
            this.btnHistoryRedownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHistoryRedownload.Enabled = false;
            this.btnHistoryRedownload.Location = new System.Drawing.Point(354, 27);
            this.btnHistoryRedownload.Name = "btnHistoryRedownload";
            this.btnHistoryRedownload.Size = new System.Drawing.Size(86, 23);
            this.btnHistoryRedownload.TabIndex = 2;
            this.btnHistoryRedownload.Text = "Redownload";
            this.btnHistoryRedownload.UseVisualStyleBackColor = true;
            this.btnHistoryRedownload.Click += new System.EventHandler(this.btnHistoryRedownload_Click);
            // 
            // lbThreadHistoryHint
            // 
            this.lbThreadHistoryHint.AutoSize = true;
            this.lbThreadHistoryHint.Location = new System.Drawing.Point(4, 7);
            this.lbThreadHistoryHint.Name = "lbThreadHistoryHint";
            this.lbThreadHistoryHint.Size = new System.Drawing.Size(352, 13);
            this.lbThreadHistoryHint.TabIndex = 0;
            this.lbThreadHistoryHint.Text = "When the option is enabled, threads downloaded will appear here";
            // 
            // tpDebug
            // 
            this.tpDebug.Location = new System.Drawing.Point(4, 22);
            this.tpDebug.Name = "tpDebug";
            this.tpDebug.Padding = new System.Windows.Forms.Padding(3);
            this.tpDebug.Size = new System.Drawing.Size(326, 156);
            this.tpDebug.TabIndex = 2;
            this.tpDebug.Text = "Debug";
            this.tpDebug.UseVisualStyleBackColor = true;
            // 
            // pnUpper
            // 
            this.pnUpper.Controls.Add(this.txtThreadURL);
            this.pnUpper.Controls.Add(this.btnAdd);
            this.pnUpper.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnUpper.Location = new System.Drawing.Point(0, 0);
            this.pnUpper.Name = "pnUpper";
            this.pnUpper.Size = new System.Drawing.Size(454, 33);
            this.pnUpper.TabIndex = 5;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 294);
            this.Controls.Add(this.chkCreateThreadInTheBackground);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pnUpper);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::YChanEx.Properties.Resources.ProgramIcon;
            this.Menu = this.mmMain;
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YChanEx";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.tabControl1.ResumeLayout(false);
            this.tpCurrentQueue.ResumeLayout(false);
            this.tpHistory.ResumeLayout(false);
            this.tpHistory.PerformLayout();
            this.pnUpper.ResumeLayout(false);
            this.pnUpper.PerformLayout();
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
        private murrty.controls.ExtendedListView lvThreads;
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
        private System.Windows.Forms.CheckBox chkCreateThreadInTheBackground;
        private System.Windows.Forms.MenuItem mSetCustomName;
        private System.Windows.Forms.MenuItem mAddThread;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpCurrentQueue;
        private System.Windows.Forms.TabPage tpHistory;
        private System.Windows.Forms.TabPage tpDebug;
        private System.Windows.Forms.Panel pnUpper;
        private System.Windows.Forms.Label lbThreadHistoryHint;
        private System.Windows.Forms.Button btnHistoryClear;
        private System.Windows.Forms.Button btnHistoryRemove;
        private System.Windows.Forms.Button btnHistoryRedownload;
        private System.Windows.Forms.TreeView tvHistory;
        private System.Windows.Forms.MenuItem mLog;
    }
}

