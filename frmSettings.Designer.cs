namespace YChanEx {
    partial class frmSettings {
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDownloads = new System.Windows.Forms.TabPage();
            this.chkSaveDownloadQueueOnExit = new System.Windows.Forms.CheckBox();
            this.txtSavePath = new YChanEx.HintTextBox();
            this.chkPreventDuplicates = new System.Windows.Forms.CheckBox();
            this.chkSaveOriginalFileNames = new System.Windows.Forms.CheckBox();
            this.chkDownloadThumbnails = new System.Windows.Forms.CheckBox();
            this.chkDownloadHTML = new System.Windows.Forms.CheckBox();
            this.numTimer = new System.Windows.Forms.NumericUpDown();
            this.lbTimer = new System.Windows.Forms.Label();
            this.lbSavePath = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkMove = new System.Windows.Forms.CheckBox();
            this.tabApplication = new System.Windows.Forms.TabPage();
            this.idk = new System.Windows.Forms.CheckBox();
            this.chkEnableUpdates = new System.Windows.Forms.CheckBox();
            this.chkShowExitWarning = new System.Windows.Forms.CheckBox();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkShowTrayIcon = new System.Windows.Forms.CheckBox();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.btnOpenLocalFiles = new System.Windows.Forms.Button();
            this.chkSilenceErrors = new System.Windows.Forms.CheckBox();
            this.chkDisableScannerWhenOpeningSettings = new System.Windows.Forms.CheckBox();
            this.txtUserAgent = new YChanEx.HintTextBox();
            this.lbUserAgent = new System.Windows.Forms.Label();
            this.tabReset = new System.Windows.Forms.TabPage();
            this.btnUserScript = new System.Windows.Forms.Button();
            this.btnProtocol = new System.Windows.Forms.Button();
            this.btnSCan = new System.Windows.Forms.Button();
            this.btnSSave = new System.Windows.Forms.Button();
            this.ttSettings = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tabDownloads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).BeginInit();
            this.tabApplication.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDownloads);
            this.tabControl1.Controls.Add(this.tabApplication);
            this.tabControl1.Controls.Add(this.tabAdvanced);
            this.tabControl1.Controls.Add(this.tabReset);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(402, 170);
            this.tabControl1.TabIndex = 0;
            // 
            // tabDownloads
            // 
            this.tabDownloads.Controls.Add(this.chkSaveDownloadQueueOnExit);
            this.tabDownloads.Controls.Add(this.txtSavePath);
            this.tabDownloads.Controls.Add(this.chkPreventDuplicates);
            this.tabDownloads.Controls.Add(this.chkSaveOriginalFileNames);
            this.tabDownloads.Controls.Add(this.chkDownloadThumbnails);
            this.tabDownloads.Controls.Add(this.chkDownloadHTML);
            this.tabDownloads.Controls.Add(this.numTimer);
            this.tabDownloads.Controls.Add(this.lbTimer);
            this.tabDownloads.Controls.Add(this.lbSavePath);
            this.tabDownloads.Controls.Add(this.btnBrowse);
            this.tabDownloads.Controls.Add(this.chkMove);
            this.tabDownloads.Location = new System.Drawing.Point(4, 22);
            this.tabDownloads.Name = "tabDownloads";
            this.tabDownloads.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloads.Size = new System.Drawing.Size(394, 144);
            this.tabDownloads.TabIndex = 0;
            this.tabDownloads.Text = "Downloads";
            this.tabDownloads.UseVisualStyleBackColor = true;
            // 
            // chkSaveDownloadQueueOnExit
            // 
            this.chkSaveDownloadQueueOnExit.AutoSize = true;
            this.chkSaveDownloadQueueOnExit.Location = new System.Drawing.Point(11, 112);
            this.chkSaveDownloadQueueOnExit.Name = "chkSaveDownloadQueueOnExit";
            this.chkSaveDownloadQueueOnExit.Size = new System.Drawing.Size(166, 17);
            this.chkSaveDownloadQueueOnExit.TabIndex = 29;
            this.chkSaveDownloadQueueOnExit.Text = "Save download queue on exit";
            this.ttSettings.SetToolTip(this.chkSaveDownloadQueueOnExit, "Saves the download queue on exit.");
            this.chkSaveDownloadQueueOnExit.UseVisualStyleBackColor = true;
            // 
            // txtSavePath
            // 
            this.txtSavePath.Location = new System.Drawing.Point(68, 11);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.ReadOnly = true;
            this.txtSavePath.Size = new System.Drawing.Size(194, 20);
            this.txtSavePath.TabIndex = 28;
            this.txtSavePath.Text = "C:\\";
            this.txtSavePath.TextHint = "C:\\";
            this.ttSettings.SetToolTip(this.txtSavePath, "The directory where downloads will be saved to");
            // 
            // chkPreventDuplicates
            // 
            this.chkPreventDuplicates.AutoSize = true;
            this.chkPreventDuplicates.Location = new System.Drawing.Point(147, 89);
            this.chkPreventDuplicates.Name = "chkPreventDuplicates";
            this.chkPreventDuplicates.Size = new System.Drawing.Size(113, 17);
            this.chkPreventDuplicates.TabIndex = 27;
            this.chkPreventDuplicates.Text = "Prevent duplicates";
            this.ttSettings.SetToolTip(this.chkPreventDuplicates, "Prevents duplicates of original file names.");
            this.chkPreventDuplicates.UseVisualStyleBackColor = true;
            // 
            // chkSaveOriginalFileNames
            // 
            this.chkSaveOriginalFileNames.AutoSize = true;
            this.chkSaveOriginalFileNames.Location = new System.Drawing.Point(147, 66);
            this.chkSaveOriginalFileNames.Name = "chkSaveOriginalFileNames";
            this.chkSaveOriginalFileNames.Size = new System.Drawing.Size(136, 17);
            this.chkSaveOriginalFileNames.TabIndex = 26;
            this.chkSaveOriginalFileNames.Text = "Save original file names";
            this.ttSettings.SetToolTip(this.chkSaveOriginalFileNames, "Saves files as their uploaded file names, instead of generated file IDs");
            this.chkSaveOriginalFileNames.UseVisualStyleBackColor = true;
            // 
            // chkDownloadThumbnails
            // 
            this.chkDownloadThumbnails.AutoSize = true;
            this.chkDownloadThumbnails.Location = new System.Drawing.Point(11, 89);
            this.chkDownloadThumbnails.Name = "chkDownloadThumbnails";
            this.chkDownloadThumbnails.Size = new System.Drawing.Size(130, 17);
            this.chkDownloadThumbnails.TabIndex = 24;
            this.chkDownloadThumbnails.Text = "Download Thumbnails";
            this.ttSettings.SetToolTip(this.chkDownloadThumbnails, "Downloads thumbnails for files in the thumb folder.");
            this.chkDownloadThumbnails.UseVisualStyleBackColor = true;
            // 
            // chkDownloadHTML
            // 
            this.chkDownloadHTML.AutoSize = true;
            this.chkDownloadHTML.Location = new System.Drawing.Point(11, 66);
            this.chkDownloadHTML.Name = "chkDownloadHTML";
            this.chkDownloadHTML.Size = new System.Drawing.Size(106, 17);
            this.chkDownloadHTML.TabIndex = 23;
            this.chkDownloadHTML.Text = "Download HTML";
            this.ttSettings.SetToolTip(this.chkDownloadHTML, "Downloads thread HTML files as Thread.html in the thread folder");
            this.chkDownloadHTML.UseVisualStyleBackColor = true;
            // 
            // numTimer
            // 
            this.numTimer.Location = new System.Drawing.Point(73, 37);
            this.numTimer.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numTimer.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numTimer.Name = "numTimer";
            this.numTimer.Size = new System.Drawing.Size(56, 20);
            this.numTimer.TabIndex = 21;
            this.numTimer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ttSettings.SetToolTip(this.numTimer, "The time (in seconds) that will be delayed before each scan");
            this.numTimer.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lbTimer
            // 
            this.lbTimer.AutoSize = true;
            this.lbTimer.Location = new System.Drawing.Point(8, 39);
            this.lbTimer.Name = "lbTimer";
            this.lbTimer.Size = new System.Drawing.Size(59, 13);
            this.lbTimer.TabIndex = 22;
            this.lbTimer.Text = "Timer (sec)";
            // 
            // lbSavePath
            // 
            this.lbSavePath.AutoSize = true;
            this.lbSavePath.Location = new System.Drawing.Point(5, 14);
            this.lbSavePath.Name = "lbSavePath";
            this.lbSavePath.Size = new System.Drawing.Size(57, 13);
            this.lbSavePath.TabIndex = 20;
            this.lbSavePath.Text = "Save &Path\r\n";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(268, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 20);
            this.btnBrowse.TabIndex = 18;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // chkMove
            // 
            this.chkMove.AutoSize = true;
            this.chkMove.Checked = true;
            this.chkMove.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMove.Location = new System.Drawing.Point(296, 6);
            this.chkMove.Name = "chkMove";
            this.chkMove.Size = new System.Drawing.Size(93, 30);
            this.chkMove.TabIndex = 19;
            this.chkMove.Text = "Move existing \r\ndownloads";
            this.ttSettings.SetToolTip(this.chkMove, "Moves existing downloads to the new directory.");
            this.chkMove.UseVisualStyleBackColor = true;
            // 
            // tabApplication
            // 
            this.tabApplication.Controls.Add(this.idk);
            this.tabApplication.Controls.Add(this.chkEnableUpdates);
            this.tabApplication.Controls.Add(this.chkShowExitWarning);
            this.tabApplication.Controls.Add(this.chkMinimizeToTray);
            this.tabApplication.Controls.Add(this.chkShowTrayIcon);
            this.tabApplication.Location = new System.Drawing.Point(4, 22);
            this.tabApplication.Name = "tabApplication";
            this.tabApplication.Padding = new System.Windows.Forms.Padding(3);
            this.tabApplication.Size = new System.Drawing.Size(394, 144);
            this.tabApplication.TabIndex = 1;
            this.tabApplication.Text = "Application";
            this.tabApplication.UseVisualStyleBackColor = true;
            // 
            // idk
            // 
            this.idk.AutoSize = true;
            this.idk.Location = new System.Drawing.Point(225, 75);
            this.idk.Name = "idk";
            this.idk.Size = new System.Drawing.Size(49, 17);
            this.idk.TabIndex = 31;
            this.idk.Text = "????";
            this.idk.UseVisualStyleBackColor = true;
            // 
            // chkEnableUpdates
            // 
            this.chkEnableUpdates.AutoSize = true;
            this.chkEnableUpdates.Location = new System.Drawing.Point(120, 75);
            this.chkEnableUpdates.Name = "chkEnableUpdates";
            this.chkEnableUpdates.Size = new System.Drawing.Size(99, 17);
            this.chkEnableUpdates.TabIndex = 30;
            this.chkEnableUpdates.Text = "Enable updates";
            this.ttSettings.SetToolTip(this.chkEnableUpdates, "Enables updates for the application");
            this.chkEnableUpdates.UseVisualStyleBackColor = true;
            // 
            // chkShowExitWarning
            // 
            this.chkShowExitWarning.AutoSize = true;
            this.chkShowExitWarning.Location = new System.Drawing.Point(244, 52);
            this.chkShowExitWarning.Name = "chkShowExitWarning";
            this.chkShowExitWarning.Size = new System.Drawing.Size(111, 17);
            this.chkShowExitWarning.TabIndex = 29;
            this.chkShowExitWarning.Text = "Show exit warning";
            this.ttSettings.SetToolTip(this.chkShowExitWarning, "Shows a warning before exiting");
            this.chkShowExitWarning.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(141, 52);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(97, 17);
            this.chkMinimizeToTray.TabIndex = 28;
            this.chkMinimizeToTray.Text = "Minimize to tray";
            this.ttSettings.SetToolTip(this.chkMinimizeToTray, "Minimizes the program to the system\'s tray");
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // chkShowTrayIcon
            // 
            this.chkShowTrayIcon.AutoSize = true;
            this.chkShowTrayIcon.Location = new System.Drawing.Point(40, 52);
            this.chkShowTrayIcon.Name = "chkShowTrayIcon";
            this.chkShowTrayIcon.Size = new System.Drawing.Size(95, 17);
            this.chkShowTrayIcon.TabIndex = 27;
            this.chkShowTrayIcon.Text = "Show tray icon";
            this.ttSettings.SetToolTip(this.chkShowTrayIcon, "Shows the ychanex icon in the system tray");
            this.chkShowTrayIcon.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.btnOpenLocalFiles);
            this.tabAdvanced.Controls.Add(this.chkSilenceErrors);
            this.tabAdvanced.Controls.Add(this.chkDisableScannerWhenOpeningSettings);
            this.tabAdvanced.Controls.Add(this.txtUserAgent);
            this.tabAdvanced.Controls.Add(this.lbUserAgent);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvanced.Size = new System.Drawing.Size(394, 144);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // btnOpenLocalFiles
            // 
            this.btnOpenLocalFiles.Location = new System.Drawing.Point(148, 102);
            this.btnOpenLocalFiles.Name = "btnOpenLocalFiles";
            this.btnOpenLocalFiles.Size = new System.Drawing.Size(98, 24);
            this.btnOpenLocalFiles.TabIndex = 4;
            this.btnOpenLocalFiles.Text = "Open local files";
            this.ttSettings.SetToolTip(this.btnOpenLocalFiles, "Browse locally saved program data.\r\nThis is not your threads directory, unless yo" +
        "u chose to save threads here.");
            this.btnOpenLocalFiles.UseVisualStyleBackColor = true;
            // 
            // chkSilenceErrors
            // 
            this.chkSilenceErrors.AutoSize = true;
            this.chkSilenceErrors.Location = new System.Drawing.Point(261, 72);
            this.chkSilenceErrors.Name = "chkSilenceErrors";
            this.chkSilenceErrors.Size = new System.Drawing.Size(89, 17);
            this.chkSilenceErrors.TabIndex = 3;
            this.chkSilenceErrors.Text = "Silence errors";
            this.ttSettings.SetToolTip(this.chkSilenceErrors, "Silences any errors that may occur.");
            this.chkSilenceErrors.UseVisualStyleBackColor = true;
            // 
            // chkDisableScannerWhenOpeningSettings
            // 
            this.chkDisableScannerWhenOpeningSettings.AutoSize = true;
            this.chkDisableScannerWhenOpeningSettings.Location = new System.Drawing.Point(45, 72);
            this.chkDisableScannerWhenOpeningSettings.Name = "chkDisableScannerWhenOpeningSettings";
            this.chkDisableScannerWhenOpeningSettings.Size = new System.Drawing.Size(210, 17);
            this.chkDisableScannerWhenOpeningSettings.TabIndex = 2;
            this.chkDisableScannerWhenOpeningSettings.Text = "Disable scanner when opening settings";
            this.ttSettings.SetToolTip(this.chkDisableScannerWhenOpeningSettings, "Pauses the scanner timer.\r\nThis does not stop in-progress scans/downloads.");
            this.chkDisableScannerWhenOpeningSettings.UseVisualStyleBackColor = true;
            // 
            // txtUserAgent
            // 
            this.txtUserAgent.Location = new System.Drawing.Point(74, 34);
            this.txtUserAgent.Name = "txtUserAgent";
            this.txtUserAgent.Size = new System.Drawing.Size(312, 20);
            this.txtUserAgent.TabIndex = 1;
            this.txtUserAgent.TextHint = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/64.0";
            this.ttSettings.SetToolTip(this.txtUserAgent, "The user-agent for the webclients to use");
            // 
            // lbUserAgent
            // 
            this.lbUserAgent.AutoSize = true;
            this.lbUserAgent.Location = new System.Drawing.Point(8, 37);
            this.lbUserAgent.Name = "lbUserAgent";
            this.lbUserAgent.Size = new System.Drawing.Size(60, 13);
            this.lbUserAgent.TabIndex = 0;
            this.lbUserAgent.Text = "User-Agent";
            // 
            // tabReset
            // 
            this.tabReset.Location = new System.Drawing.Point(4, 22);
            this.tabReset.Name = "tabReset";
            this.tabReset.Padding = new System.Windows.Forms.Padding(3);
            this.tabReset.Size = new System.Drawing.Size(394, 144);
            this.tabReset.TabIndex = 3;
            this.tabReset.Text = "Reset";
            this.tabReset.UseVisualStyleBackColor = true;
            // 
            // btnUserScript
            // 
            this.btnUserScript.Location = new System.Drawing.Point(12, 176);
            this.btnUserScript.Name = "btnUserScript";
            this.btnUserScript.Size = new System.Drawing.Size(75, 24);
            this.btnUserScript.TabIndex = 25;
            this.btnUserScript.Text = "Userscript";
            this.ttSettings.SetToolTip(this.btnUserScript, "Install the userscript for the application");
            this.btnUserScript.UseVisualStyleBackColor = true;
            // 
            // btnProtocol
            // 
            this.btnProtocol.Enabled = false;
            this.btnProtocol.Location = new System.Drawing.Point(93, 176);
            this.btnProtocol.Name = "btnProtocol";
            this.btnProtocol.Size = new System.Drawing.Size(116, 24);
            this.btnProtocol.TabIndex = 24;
            this.btnProtocol.Text = "Install protocol";
            this.ttSettings.SetToolTip(this.btnProtocol, "Install the protocol for the userscript");
            this.btnProtocol.UseVisualStyleBackColor = true;
            this.btnProtocol.Visible = false;
            // 
            // btnSCan
            // 
            this.btnSCan.Location = new System.Drawing.Point(234, 176);
            this.btnSCan.Name = "btnSCan";
            this.btnSCan.Size = new System.Drawing.Size(75, 24);
            this.btnSCan.TabIndex = 23;
            this.btnSCan.Text = "Cancel";
            this.ttSettings.SetToolTip(this.btnSCan, "Does not save any settings changed");
            this.btnSCan.UseVisualStyleBackColor = true;
            // 
            // btnSSave
            // 
            this.btnSSave.Location = new System.Drawing.Point(315, 176);
            this.btnSSave.Name = "btnSSave";
            this.btnSSave.Size = new System.Drawing.Size(75, 24);
            this.btnSSave.TabIndex = 22;
            this.btnSSave.Text = "Save";
            this.ttSettings.SetToolTip(this.btnSSave, "Saves the settings");
            this.btnSSave.UseVisualStyleBackColor = true;
            this.btnSSave.Click += new System.EventHandler(this.btnSSave_Click);
            // 
            // ttSettings
            // 
            this.ttSettings.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(402, 212);
            this.Controls.Add(this.btnSSave);
            this.Controls.Add(this.btnUserScript);
            this.Controls.Add(this.btnProtocol);
            this.Controls.Add(this.btnSCan);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(410, 242);
            this.MinimumSize = new System.Drawing.Size(410, 242);
            this.Name = "frmSettings";
            this.Text = "ychanex settings";
            this.tabControl1.ResumeLayout(false);
            this.tabDownloads.ResumeLayout(false);
            this.tabDownloads.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).EndInit();
            this.tabApplication.ResumeLayout(false);
            this.tabApplication.PerformLayout();
            this.tabAdvanced.ResumeLayout(false);
            this.tabAdvanced.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDownloads;
        private System.Windows.Forms.NumericUpDown numTimer;
        private System.Windows.Forms.Label lbTimer;
        private System.Windows.Forms.Label lbSavePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkMove;
        private System.Windows.Forms.TabPage tabApplication;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.TabPage tabReset;
        private System.Windows.Forms.Button btnUserScript;
        private System.Windows.Forms.Button btnProtocol;
        private System.Windows.Forms.Button btnSCan;
        private System.Windows.Forms.Button btnSSave;
        private System.Windows.Forms.CheckBox chkPreventDuplicates;
        private System.Windows.Forms.CheckBox chkSaveOriginalFileNames;
        private System.Windows.Forms.CheckBox chkDownloadThumbnails;
        private System.Windows.Forms.CheckBox chkDownloadHTML;
        private HintTextBox txtSavePath;
        private System.Windows.Forms.CheckBox idk;
        private System.Windows.Forms.CheckBox chkEnableUpdates;
        private System.Windows.Forms.CheckBox chkShowExitWarning;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkShowTrayIcon;
        private System.Windows.Forms.ToolTip ttSettings;
        private System.Windows.Forms.CheckBox chkSaveDownloadQueueOnExit;
        private System.Windows.Forms.Label lbUserAgent;
        private System.Windows.Forms.CheckBox chkSilenceErrors;
        private System.Windows.Forms.CheckBox chkDisableScannerWhenOpeningSettings;
        private HintTextBox txtUserAgent;
        private System.Windows.Forms.Button btnOpenLocalFiles;
    }
}