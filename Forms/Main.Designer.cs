namespace YChanEx {
    partial class frmMain {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tcApp = new System.Windows.Forms.TabControl();
            this.tpThreads = new System.Windows.Forms.TabPage();
            this.lbThreads = new System.Windows.Forms.ListBox();
            this.tpBoard = new System.Windows.Forms.TabPage();
            this.lbBoards = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.edtURL = new System.Windows.Forms.TextBox();
            this.nfTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuBar = new System.Windows.Forms.MainMenu(this.components);
            this.mFile = new System.Windows.Forms.MenuItem();
            this.mReload = new System.Windows.Forms.MenuItem();
            this.mSave = new System.Windows.Forms.MenuItem();
            this.mFileSep = new System.Windows.Forms.MenuItem();
            this.mUpdates = new System.Windows.Forms.MenuItem();
            this.mHistory = new System.Windows.Forms.MenuItem();
            this.mSettings = new System.Windows.Forms.MenuItem();
            this.mHelp = new System.Windows.Forms.MenuItem();
            this.mLicenseAndSource = new System.Windows.Forms.MenuItem();
            this.mHelpSep = new System.Windows.Forms.MenuItem();
            this.mAbout = new System.Windows.Forms.MenuItem();
            this.mUpdateAvailable = new System.Windows.Forms.MenuItem();
            this.mDebug = new System.Windows.Forms.MenuItem();
            this.mDebugTitle = new System.Windows.Forms.MenuItem();
            this.mDebugID = new System.Windows.Forms.MenuItem();
            this.mTray = new System.Windows.Forms.ContextMenu();
            this.mTrayShow = new System.Windows.Forms.MenuItem();
            this.mTraySep1 = new System.Windows.Forms.MenuItem();
            this.mTrayOpen = new System.Windows.Forms.MenuItem();
            this.mDownload = new System.Windows.Forms.MenuItem();
            this.mTrayClipboard = new System.Windows.Forms.MenuItem();
            this.mTraySep2 = new System.Windows.Forms.MenuItem();
            this.mTrayExit = new System.Windows.Forms.MenuItem();
            this.mThreads = new System.Windows.Forms.ContextMenu();
            this.mThreadsOpenF = new System.Windows.Forms.MenuItem();
            this.mThreadsOpenB = new System.Windows.Forms.MenuItem();
            this.mThreadsCopyL = new System.Windows.Forms.MenuItem();
            this.mThreadsSep = new System.Windows.Forms.MenuItem();
            this.mThreadsRemove = new System.Windows.Forms.MenuItem();
            this.mBoards = new System.Windows.Forms.ContextMenu();
            this.mBoardsOpenF = new System.Windows.Forms.MenuItem();
            this.mBoardsOpenB = new System.Windows.Forms.MenuItem();
            this.mBoardsCopyL = new System.Windows.Forms.MenuItem();
            this.mBoardsSep = new System.Windows.Forms.MenuItem();
            this.mBoardsRemove = new System.Windows.Forms.MenuItem();
            this.tcApp.SuspendLayout();
            this.tpThreads.SuspendLayout();
            this.tpBoard.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcApp
            // 
            this.tcApp.Controls.Add(this.tpThreads);
            this.tcApp.Controls.Add(this.tpBoard);
            this.tcApp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcApp.Location = new System.Drawing.Point(0, 0);
            this.tcApp.Name = "tcApp";
            this.tcApp.SelectedIndex = 0;
            this.tcApp.Size = new System.Drawing.Size(382, 230);
            this.tcApp.TabIndex = 0;
            // 
            // tpThreads
            // 
            this.tpThreads.Controls.Add(this.lbThreads);
            this.tpThreads.Location = new System.Drawing.Point(4, 22);
            this.tpThreads.Name = "tpThreads";
            this.tpThreads.Padding = new System.Windows.Forms.Padding(3);
            this.tpThreads.Size = new System.Drawing.Size(374, 204);
            this.tpThreads.TabIndex = 0;
            this.tpThreads.Text = "Threads";
            this.tpThreads.UseVisualStyleBackColor = true;
            // 
            // lbThreads
            // 
            this.lbThreads.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbThreads.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbThreads.FormattingEnabled = true;
            this.lbThreads.Location = new System.Drawing.Point(3, 3);
            this.lbThreads.Name = "lbThreads";
            this.lbThreads.Size = new System.Drawing.Size(368, 198);
            this.lbThreads.TabIndex = 3;
            this.lbThreads.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbThreads_MouseDown);
            // 
            // tpBoard
            // 
            this.tpBoard.Controls.Add(this.lbBoards);
            this.tpBoard.Location = new System.Drawing.Point(4, 22);
            this.tpBoard.Name = "tpBoard";
            this.tpBoard.Padding = new System.Windows.Forms.Padding(3);
            this.tpBoard.Size = new System.Drawing.Size(374, 204);
            this.tpBoard.TabIndex = 1;
            this.tpBoard.Text = "Boards";
            this.tpBoard.UseVisualStyleBackColor = true;
            // 
            // lbBoards
            // 
            this.lbBoards.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbBoards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbBoards.FormattingEnabled = true;
            this.lbBoards.Location = new System.Drawing.Point(3, 3);
            this.lbBoards.Name = "lbBoards";
            this.lbBoards.Size = new System.Drawing.Size(368, 198);
            this.lbBoards.TabIndex = 4;
            this.lbBoards.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbBoards_MouseDown);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(314, -1);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(59, 21);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // edtURL
            // 
            this.edtURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtURL.Location = new System.Drawing.Point(109, 0);
            this.edtURL.Name = "edtURL";
            this.edtURL.Size = new System.Drawing.Size(203, 20);
            this.edtURL.TabIndex = 1;
            this.edtURL.Enter += new System.EventHandler(this.edtURL_Enter);
            this.edtURL.Leave += new System.EventHandler(this.edtURL_Leave);
            // 
            // nfTray
            // 
            this.nfTray.Icon = ((System.Drawing.Icon)(resources.GetObject("nfTray.Icon")));
            this.nfTray.Text = "YchanEx\r\nDouble click to show/hide";
            this.nfTray.Visible = true;
            this.nfTray.BalloonTipClicked += new System.EventHandler(this.nfTray_BalloonTipClicked);
            this.nfTray.BalloonTipClosed += new System.EventHandler(this.nfTray_BalloonTipClosed);
            this.nfTray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.nfTray_MouseDoubleClick);
            this.nfTray.MouseMove += new System.Windows.Forms.MouseEventHandler(this.nfTray_MouseMove);
            // 
            // MenuBar
            // 
            this.MenuBar.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mFile,
            this.mHistory,
            this.mSettings,
            this.mHelp,
            this.mUpdateAvailable,
            this.mDebug});
            // 
            // mFile
            // 
            this.mFile.Index = 0;
            this.mFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mReload,
            this.mSave,
            this.mFileSep,
            this.mUpdates});
            this.mFile.Text = "File";
            this.mFile.Visible = false;
            // 
            // mReload
            // 
            this.mReload.Index = 0;
            this.mReload.Text = "Reload threads";
            this.mReload.Click += new System.EventHandler(this.mReload_Click);
            // 
            // mSave
            // 
            this.mSave.Index = 1;
            this.mSave.Text = "Save history";
            this.mSave.Click += new System.EventHandler(this.mSave_Click);
            // 
            // mFileSep
            // 
            this.mFileSep.Index = 2;
            this.mFileSep.Text = "-";
            // 
            // mUpdates
            // 
            this.mUpdates.Index = 3;
            this.mUpdates.Text = "Check for updates";
            this.mUpdates.Click += new System.EventHandler(this.mUpdates_Click);
            // 
            // mHistory
            // 
            this.mHistory.Index = 1;
            this.mHistory.Text = "History";
            this.mHistory.Click += new System.EventHandler(this.mHistory_Click);
            // 
            // mSettings
            // 
            this.mSettings.Index = 2;
            this.mSettings.Text = "Settings";
            this.mSettings.Click += new System.EventHandler(this.mSettings_Click);
            // 
            // mHelp
            // 
            this.mHelp.Index = 3;
            this.mHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mLicenseAndSource,
            this.mHelpSep,
            this.mAbout});
            this.mHelp.Text = "Help";
            // 
            // mLicenseAndSource
            // 
            this.mLicenseAndSource.Index = 0;
            this.mLicenseAndSource.Text = "License && Source";
            this.mLicenseAndSource.Click += new System.EventHandler(this.mLicenseAndSource_Click);
            // 
            // mHelpSep
            // 
            this.mHelpSep.Index = 1;
            this.mHelpSep.Text = "-";
            // 
            // mAbout
            // 
            this.mAbout.Index = 2;
            this.mAbout.Text = "About";
            this.mAbout.Click += new System.EventHandler(this.mAbout_Click);
            // 
            // mUpdateAvailable
            // 
            this.mUpdateAvailable.Enabled = false;
            this.mUpdateAvailable.Index = 4;
            this.mUpdateAvailable.Text = "Update available";
            this.mUpdateAvailable.Visible = false;
            this.mUpdateAvailable.Click += new System.EventHandler(this.mUpdateAvailable_Click);
            // 
            // mDebug
            // 
            this.mDebug.Enabled = false;
            this.mDebug.Index = 5;
            this.mDebug.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mDebugTitle,
            this.mDebugID});
            this.mDebug.Text = "Debug";
            this.mDebug.Visible = false;
            // 
            // mDebugTitle
            // 
            this.mDebugTitle.Index = 0;
            this.mDebugTitle.Text = "Get Thread Title from Clipboard";
            this.mDebugTitle.Click += new System.EventHandler(this.mDebugTitle_Click);
            // 
            // mDebugID
            // 
            this.mDebugID.Index = 1;
            this.mDebugID.Text = "Get Thread ID";
            this.mDebugID.Click += new System.EventHandler(this.mDebugID_Click);
            // 
            // mTray
            // 
            this.mTray.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mTrayShow,
            this.mTraySep1,
            this.mTrayOpen,
            this.mDownload,
            this.mTraySep2,
            this.mTrayExit});
            // 
            // mTrayShow
            // 
            this.mTrayShow.Index = 0;
            this.mTrayShow.Text = "Hide";
            this.mTrayShow.Click += new System.EventHandler(this.mTrayShow_Click);
            // 
            // mTraySep1
            // 
            this.mTraySep1.Index = 1;
            this.mTraySep1.Text = "-";
            // 
            // mTrayOpen
            // 
            this.mTrayOpen.Index = 2;
            this.mTrayOpen.Text = "Open Downloads";
            this.mTrayOpen.Click += new System.EventHandler(this.mTrayOpen_Click);
            // 
            // mDownload
            // 
            this.mDownload.Index = 3;
            this.mDownload.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mTrayClipboard});
            this.mDownload.Text = "Download Thread";
            // 
            // mTrayClipboard
            // 
            this.mTrayClipboard.Index = 0;
            this.mTrayClipboard.Text = "From Clipboard";
            this.mTrayClipboard.Click += new System.EventHandler(this.mTrayClipboard_Click);
            // 
            // mTraySep2
            // 
            this.mTraySep2.Index = 4;
            this.mTraySep2.Text = "-";
            // 
            // mTrayExit
            // 
            this.mTrayExit.Index = 5;
            this.mTrayExit.Text = "Exit";
            this.mTrayExit.Click += new System.EventHandler(this.mTrayExit_Click);
            // 
            // mThreads
            // 
            this.mThreads.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mThreadsOpenF,
            this.mThreadsOpenB,
            this.mThreadsCopyL,
            this.mThreadsSep,
            this.mThreadsRemove});
            // 
            // mThreadsOpenF
            // 
            this.mThreadsOpenF.Index = 0;
            this.mThreadsOpenF.Text = "Open Folder";
            this.mThreadsOpenF.Click += new System.EventHandler(this.mThreadsOpenF_Click);
            // 
            // mThreadsOpenB
            // 
            this.mThreadsOpenB.Index = 1;
            this.mThreadsOpenB.Text = "Open in Browser";
            this.mThreadsOpenB.Click += new System.EventHandler(this.mThreadsOpenB_Click);
            // 
            // mThreadsCopyL
            // 
            this.mThreadsCopyL.Index = 2;
            this.mThreadsCopyL.Text = "Copy Link";
            this.mThreadsCopyL.Click += new System.EventHandler(this.mThreadsCopyL_Click);
            // 
            // mThreadsSep
            // 
            this.mThreadsSep.Index = 3;
            this.mThreadsSep.Text = "-";
            // 
            // mThreadsRemove
            // 
            this.mThreadsRemove.Index = 4;
            this.mThreadsRemove.Text = "Remove";
            this.mThreadsRemove.Click += new System.EventHandler(this.mThreadsRemove_Click);
            // 
            // mBoards
            // 
            this.mBoards.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mBoardsOpenF,
            this.mBoardsOpenB,
            this.mBoardsCopyL,
            this.mBoardsSep,
            this.mBoardsRemove});
            // 
            // mBoardsOpenF
            // 
            this.mBoardsOpenF.Index = 0;
            this.mBoardsOpenF.Text = "Open Folder";
            this.mBoardsOpenF.Click += new System.EventHandler(this.mBoardsOpenF_Click);
            // 
            // mBoardsOpenB
            // 
            this.mBoardsOpenB.Index = 1;
            this.mBoardsOpenB.Text = "Open in Browser";
            this.mBoardsOpenB.Click += new System.EventHandler(this.mBoardsOpenB_Click);
            // 
            // mBoardsCopyL
            // 
            this.mBoardsCopyL.Index = 2;
            this.mBoardsCopyL.Text = "Copy Link";
            this.mBoardsCopyL.Click += new System.EventHandler(this.mBoardsCopyL_Click);
            // 
            // mBoardsSep
            // 
            this.mBoardsSep.Index = 3;
            this.mBoardsSep.Text = "-";
            // 
            // mBoardsRemove
            // 
            this.mBoardsRemove.Index = 4;
            this.mBoardsRemove.Text = "Remove";
            this.mBoardsRemove.Click += new System.EventHandler(this.mBoardsRemove_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 230);
            this.Controls.Add(this.edtURL);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.tcApp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.MenuBar;
            this.MinimumSize = new System.Drawing.Size(390, 260);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YChanEx";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.tcApp.ResumeLayout(false);
            this.tpThreads.ResumeLayout(false);
            this.tpBoard.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TabControl tcApp;
        private System.Windows.Forms.TabPage tpThreads;
        private System.Windows.Forms.TabPage tpBoard;
        private System.Windows.Forms.ListBox lbThreads;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.TextBox edtURL;
        private System.Windows.Forms.ListBox lbBoards;
        private System.Windows.Forms.NotifyIcon nfTray;
        private System.Windows.Forms.MainMenu MenuBar;
        private System.Windows.Forms.MenuItem mSettings;
        private System.Windows.Forms.MenuItem mAbout;
        private System.Windows.Forms.ContextMenu mTray;
        private System.Windows.Forms.MenuItem mTrayShow;
        private System.Windows.Forms.MenuItem mTrayOpen;
        private System.Windows.Forms.MenuItem mTraySep2;
        private System.Windows.Forms.MenuItem mTrayExit;
        private System.Windows.Forms.ContextMenu mThreads;
        private System.Windows.Forms.MenuItem mThreadsOpenF;
        private System.Windows.Forms.MenuItem mThreadsOpenB;
        private System.Windows.Forms.MenuItem mThreadsSep;
        private System.Windows.Forms.MenuItem mThreadsRemove;
        private System.Windows.Forms.ContextMenu mBoards;
        private System.Windows.Forms.MenuItem mBoardsOpenF;
        private System.Windows.Forms.MenuItem mBoardsOpenB;
        private System.Windows.Forms.MenuItem mBoardsSep;
        private System.Windows.Forms.MenuItem mBoardsRemove;
        private System.Windows.Forms.MenuItem mHelp;
        private System.Windows.Forms.MenuItem mLicenseAndSource;
        private System.Windows.Forms.MenuItem mHelpSep;
        private System.Windows.Forms.MenuItem mDownload;
        private System.Windows.Forms.MenuItem mTrayClipboard;
        private System.Windows.Forms.MenuItem mTraySep1;
        private System.Windows.Forms.MenuItem mThreadsCopyL;
        private System.Windows.Forms.MenuItem mBoardsCopyL;
        private System.Windows.Forms.MenuItem mDebug;
        private System.Windows.Forms.MenuItem mDebugTitle;
        private System.Windows.Forms.MenuItem mDebugID;
        private System.Windows.Forms.MenuItem mHistory;
        private System.Windows.Forms.MenuItem mFile;
        private System.Windows.Forms.MenuItem mReload;
        private System.Windows.Forms.MenuItem mSave;
        private System.Windows.Forms.MenuItem mFileSep;
        private System.Windows.Forms.MenuItem mUpdates;
        private System.Windows.Forms.MenuItem mUpdateAvailable;
    }
}

