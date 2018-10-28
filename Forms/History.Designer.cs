namespace YChanEx
{
    partial class History
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(History));
            this.btnClose = new System.Windows.Forms.Button();
            this.lbHistory = new System.Windows.Forms.ListBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.mHistory = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mOpenLink = new System.Windows.Forms.MenuItem();
            this.mOpenArchive = new System.Windows.Forms.MenuItem();
            this.mShowInDownloads = new System.Windows.Forms.MenuItem();
            this.mCopy = new System.Windows.Forms.MenuItem();
            this.mCopyLink = new System.Windows.Forms.MenuItem();
            this.mCopyID = new System.Windows.Forms.MenuItem();
            this.mSep = new System.Windows.Forms.MenuItem();
            this.mRemove = new System.Windows.Forms.MenuItem();
            this.btnOpenDownloads = new System.Windows.Forms.Button();
            this.cbSite = new System.Windows.Forms.ComboBox();
            this.chkSorted = new System.Windows.Forms.CheckBox();
            this.lbSite = new System.Windows.Forms.Label();
            this.lbBoard = new System.Windows.Forms.Label();
            this.cbBoard = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(313, 219);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(69, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lbHistory
            // 
            this.lbHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbHistory.FormattingEnabled = true;
            this.lbHistory.HorizontalScrollbar = true;
            this.lbHistory.Location = new System.Drawing.Point(12, 38);
            this.lbHistory.Name = "lbHistory";
            this.lbHistory.Size = new System.Drawing.Size(370, 173);
            this.lbHistory.TabIndex = 1;
            this.lbHistory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbHistory_MouseDown);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(232, 219);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear History";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // mHistory
            // 
            this.mHistory.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.mCopy,
            this.mSep,
            this.mRemove});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mOpenLink,
            this.mOpenArchive,
            this.mShowInDownloads});
            this.menuItem1.Text = "Open...";
            // 
            // mOpenLink
            // 
            this.mOpenLink.Index = 0;
            this.mOpenLink.Text = "Open link";
            this.mOpenLink.Click += new System.EventHandler(this.mOpenLink_Click);
            // 
            // mOpenArchive
            // 
            this.mOpenArchive.Enabled = false;
            this.mOpenArchive.Index = 1;
            this.mOpenArchive.Text = "Open archive link";
            this.mOpenArchive.Click += new System.EventHandler(this.mOpenArchive_Click);
            // 
            // mShowInDownloads
            // 
            this.mShowInDownloads.Index = 2;
            this.mShowInDownloads.Text = "Open in downloads";
            this.mShowInDownloads.Click += new System.EventHandler(this.mShowInDownloads_Click);
            // 
            // mCopy
            // 
            this.mCopy.Index = 1;
            this.mCopy.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mCopyLink,
            this.mCopyID});
            this.mCopy.Text = "Copy...";
            // 
            // mCopyLink
            // 
            this.mCopyLink.Index = 0;
            this.mCopyLink.Text = "Link";
            this.mCopyLink.Click += new System.EventHandler(this.mCopyLink_Click);
            // 
            // mCopyID
            // 
            this.mCopyID.Index = 1;
            this.mCopyID.Text = "Thread ID";
            this.mCopyID.Click += new System.EventHandler(this.mCopyID_Click);
            // 
            // mSep
            // 
            this.mSep.Index = 2;
            this.mSep.Text = "-";
            // 
            // mRemove
            // 
            this.mRemove.Index = 3;
            this.mRemove.Text = "Remove";
            this.mRemove.Click += new System.EventHandler(this.mRemove_Click);
            // 
            // btnOpenDownloads
            // 
            this.btnOpenDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpenDownloads.Location = new System.Drawing.Point(12, 219);
            this.btnOpenDownloads.Name = "btnOpenDownloads";
            this.btnOpenDownloads.Size = new System.Drawing.Size(110, 23);
            this.btnOpenDownloads.TabIndex = 2;
            this.btnOpenDownloads.Text = "Open Downloads";
            this.btnOpenDownloads.UseVisualStyleBackColor = true;
            this.btnOpenDownloads.Click += new System.EventHandler(this.btnOpenDownloads_Click);
            // 
            // cbSite
            // 
            this.cbSite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSite.FormattingEnabled = true;
            this.cbSite.Items.AddRange(new object[] {
            "4chan",
            "420chan",
            "7chan",
            "8chan",
            "fchan",
            "u-18chan"});
            this.cbSite.Location = new System.Drawing.Point(40, 11);
            this.cbSite.Name = "cbSite";
            this.cbSite.Size = new System.Drawing.Size(72, 21);
            this.cbSite.TabIndex = 0;
            this.cbSite.SelectedIndexChanged += new System.EventHandler(this.cbSite_SelectedIndexChanged);
            // 
            // chkSorted
            // 
            this.chkSorted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSorted.AutoSize = true;
            this.chkSorted.Location = new System.Drawing.Point(316, 12);
            this.chkSorted.Name = "chkSorted";
            this.chkSorted.Size = new System.Drawing.Size(59, 17);
            this.chkSorted.TabIndex = 6;
            this.chkSorted.Text = "Sort list";
            this.chkSorted.UseVisualStyleBackColor = true;
            this.chkSorted.CheckedChanged += new System.EventHandler(this.chkSorted_CheckedChanged);
            // 
            // lbSite
            // 
            this.lbSite.AutoSize = true;
            this.lbSite.Location = new System.Drawing.Point(9, 14);
            this.lbSite.Name = "lbSite";
            this.lbSite.Size = new System.Drawing.Size(25, 13);
            this.lbSite.TabIndex = 7;
            this.lbSite.Text = "Site";
            // 
            // lbBoard
            // 
            this.lbBoard.AutoSize = true;
            this.lbBoard.Location = new System.Drawing.Point(118, 14);
            this.lbBoard.Name = "lbBoard";
            this.lbBoard.Size = new System.Drawing.Size(35, 13);
            this.lbBoard.TabIndex = 8;
            this.lbBoard.Text = "Board";
            // 
            // cbBoard
            // 
            this.cbBoard.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBoard.FormattingEnabled = true;
            this.cbBoard.Items.AddRange(new object[] {
            "All boards"});
            this.cbBoard.Location = new System.Drawing.Point(159, 11);
            this.cbBoard.Name = "cbBoard";
            this.cbBoard.Size = new System.Drawing.Size(76, 21);
            this.cbBoard.TabIndex = 9;
            this.cbBoard.SelectedIndexChanged += new System.EventHandler(this.cbBoard_SelectedIndexChanged);
            // 
            // History
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 262);
            this.Controls.Add(this.cbBoard);
            this.Controls.Add(this.lbBoard);
            this.Controls.Add(this.lbSite);
            this.Controls.Add(this.chkSorted);
            this.Controls.Add(this.cbSite);
            this.Controls.Add(this.btnOpenDownloads);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.lbHistory);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(410, 292);
            this.Name = "History";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thread History";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.History_FormClosing);
            this.Shown += new System.EventHandler(this.History_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListBox lbHistory;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ContextMenu mHistory;
        private System.Windows.Forms.MenuItem mRemove;
        private System.Windows.Forms.MenuItem mCopy;
        private System.Windows.Forms.MenuItem mCopyLink;
        private System.Windows.Forms.MenuItem mCopyID;
        private System.Windows.Forms.MenuItem mShowInDownloads;
        private System.Windows.Forms.MenuItem mSep;
        private System.Windows.Forms.Button btnOpenDownloads;
        private System.Windows.Forms.ComboBox cbSite;
        private System.Windows.Forms.CheckBox chkSorted;
        private System.Windows.Forms.Label lbSite;
        private System.Windows.Forms.Label lbBoard;
        private System.Windows.Forms.ComboBox cbBoard;
        private System.Windows.Forms.MenuItem mOpenArchive;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem mOpenLink;

    }
}