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
            this.cmItems = new System.Windows.Forms.ContextMenu();
            this.mStatus = new System.Windows.Forms.MenuItem();
            this.mRetryDownload = new System.Windows.Forms.MenuItem();
            this.mRemove = new System.Windows.Forms.MenuItem();
            this.lvThreads = new YChanEx.VistaListView();
            this.clStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clThread = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmTray = new System.Windows.Forms.ContextMenu();
            this.mTrayShowYChanEx = new System.Windows.Forms.MenuItem();
            this.mTrayExit = new System.Windows.Forms.MenuItem();
            this.mTraySep = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(282, 2);
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
            this.txtThreadURL.Size = new System.Drawing.Size(264, 20);
            this.txtThreadURL.TabIndex = 2;
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
            this.niTray.Text = "YChanEx";
            this.niTray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niTray_MouseDoubleClick);
            // 
            // changeTray
            // 
            this.changeTray.Interval = 5000;
            this.changeTray.Tick += new System.EventHandler(this.changeTray_Tick);
            // 
            // cmItems
            // 
            this.cmItems.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mStatus,
            this.mRetryDownload,
            this.mRemove});
            this.cmItems.Popup += new System.EventHandler(this.cmItems_Popup);
            // 
            // mStatus
            // 
            this.mStatus.Index = 0;
            this.mStatus.Text = "View Status";
            this.mStatus.Click += new System.EventHandler(this.mStatus_Click);
            // 
            // mRetryDownload
            // 
            this.mRetryDownload.Index = 1;
            this.mRetryDownload.Text = "Retry Download";
            this.mRetryDownload.Click += new System.EventHandler(this.mRetryDownload_Click);
            // 
            // mRemove
            // 
            this.mRemove.Index = 2;
            this.mRemove.Text = "Remove";
            this.mRemove.Click += new System.EventHandler(this.mRemove_Click);
            // 
            // lvThreads
            // 
            this.lvThreads.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clStatus,
            this.clThread});
            this.lvThreads.EnableVistaView = true;
            this.lvThreads.FullRowSelect = true;
            this.lvThreads.Location = new System.Drawing.Point(0, 33);
            this.lvThreads.MultiSelect = false;
            this.lvThreads.Name = "lvThreads";
            this.lvThreads.Size = new System.Drawing.Size(342, 178);
            this.lvThreads.TabIndex = 3;
            this.lvThreads.UseCompatibleStateImageBehavior = false;
            this.lvThreads.View = System.Windows.Forms.View.Details;
            this.lvThreads.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvThreads_MouseDoubleClick);
            // 
            // clStatus
            // 
            this.clStatus.Text = "Status";
            this.clStatus.Width = 74;
            // 
            // clThread
            // 
            this.clThread.Text = "Threads";
            this.clThread.Width = 260;
            // 
            // cmTray
            // 
            this.cmTray.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mTrayShowYChanEx,
            this.mTraySep,
            this.mTrayExit});
            // 
            // mTrayShowYChanEx
            // 
            this.mTrayShowYChanEx.Index = 0;
            this.mTrayShowYChanEx.Text = "Show YChanex";
            this.mTrayShowYChanEx.Click += new System.EventHandler(this.mTrayShowYChanEx_Click);
            // 
            // mTrayExit
            // 
            this.mTrayExit.Index = 2;
            this.mTrayExit.Text = "Exit";
            this.mTrayExit.Click += new System.EventHandler(this.mTrayExit_Click);
            // 
            // mTraySep
            // 
            this.mTraySep.Index = 1;
            this.mTraySep.Text = "-";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(342, 211);
            this.Controls.Add(this.lvThreads);
            this.Controls.Add(this.txtThreadURL);
            this.Controls.Add(this.btnAdd);
            this.Menu = this.mmMain;
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YChanEx";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
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
        private System.Windows.Forms.ContextMenu cmItems;
        private System.Windows.Forms.MenuItem mStatus;
        private System.Windows.Forms.MenuItem mRemove;
        private System.Windows.Forms.MenuItem mRetryDownload;
        private System.Windows.Forms.ContextMenu cmTray;
        private System.Windows.Forms.MenuItem mTrayShowYChanEx;
        private System.Windows.Forms.MenuItem mTraySep;
        private System.Windows.Forms.MenuItem mTrayExit;
    }
}

