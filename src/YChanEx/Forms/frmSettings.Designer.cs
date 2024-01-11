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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSettings));
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tabDownloads = new System.Windows.Forms.TabPage();
            this.chkUseProxy = new System.Windows.Forms.CheckBox();
            this.txtProxy = new murrty.controls.ExtendedTextBox();
            this.chkAutoRemoveDeadThreads = new System.Windows.Forms.CheckBox();
            this.chkCleanThreadHTML = new System.Windows.Forms.CheckBox();
            this.chkAllowFileNamesGreaterThan255 = new System.Windows.Forms.CheckBox();
            this.txtSavePath = new murrty.controls.ExtendedTextBox();
            this.chkPreventDuplicates = new System.Windows.Forms.CheckBox();
            this.chkSaveOriginalFileNames = new System.Windows.Forms.CheckBox();
            this.chkDownloadThumbnails = new System.Windows.Forms.CheckBox();
            this.chkDownloadHTML = new System.Windows.Forms.CheckBox();
            this.numTimer = new System.Windows.Forms.NumericUpDown();
            this.lbTimer = new System.Windows.Forms.Label();
            this.lbSavePath = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkMoveExistingDownloads = new System.Windows.Forms.CheckBox();
            this.lbScanDelaySeconds = new System.Windows.Forms.Label();
            this.tabApplication = new System.Windows.Forms.TabPage();
            this.chkSaveDownloadHistory = new System.Windows.Forms.CheckBox();
            this.chkMinimizeInsteadOfExiting = new System.Windows.Forms.CheckBox();
            this.chkUseFullBoardNameForTitle = new System.Windows.Forms.CheckBox();
            this.chkSaveDownloadQueueOnExit = new System.Windows.Forms.CheckBox();
            this.chkEnableUpdates = new System.Windows.Forms.CheckBox();
            this.chkShowExitWarning = new System.Windows.Forms.CheckBox();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkShowTrayIcon = new System.Windows.Forms.CheckBox();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.btnOpenLocalFiles = new System.Windows.Forms.Button();
            this.chkSilenceErrors = new System.Windows.Forms.CheckBox();
            this.chkDisableScannerWhenOpeningSettings = new System.Windows.Forms.CheckBox();
            this.txtUserAgent = new murrty.controls.ExtendedTextBox();
            this.lbUserAgent = new System.Windows.Forms.Label();
            this.tabCookies = new System.Windows.Forms.TabPage();
            this.btnRemoveCookie = new System.Windows.Forms.Button();
            this.btnAddCookie = new System.Windows.Forms.Button();
            this.lvCookies = new System.Windows.Forms.ListView();
            this.chCookieName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCookieValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCookiePath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCookieDomain = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabReset = new System.Windows.Forms.TabPage();
            this.chkEnableSettingsReset = new System.Windows.Forms.CheckBox();
            this.chkResetRegexSettings = new System.Windows.Forms.CheckBox();
            this.chkResetAdvancedSettings = new System.Windows.Forms.CheckBox();
            this.chkResetApplicationSettings = new System.Windows.Forms.CheckBox();
            this.chkResetDownloadSettings = new System.Windows.Forms.CheckBox();
            this.btnResetSettings = new System.Windows.Forms.Button();
            this.btnUserScript = new System.Windows.Forms.Button();
            this.btnProtocol = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.ttSettings = new System.Windows.Forms.ToolTip(this.components);
            this.tcMain.SuspendLayout();
            this.tabDownloads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).BeginInit();
            this.tabApplication.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.tabCookies.SuspendLayout();
            this.tabReset.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tabDownloads);
            this.tcMain.Controls.Add(this.tabApplication);
            this.tcMain.Controls.Add(this.tabAdvanced);
            this.tcMain.Controls.Add(this.tabCookies);
            this.tcMain.Controls.Add(this.tabReset);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(394, 170);
            this.tcMain.TabIndex = 0;
            // 
            // tabDownloads
            // 
            this.tabDownloads.Controls.Add(this.chkUseProxy);
            this.tabDownloads.Controls.Add(this.txtProxy);
            this.tabDownloads.Controls.Add(this.chkAutoRemoveDeadThreads);
            this.tabDownloads.Controls.Add(this.chkCleanThreadHTML);
            this.tabDownloads.Controls.Add(this.chkAllowFileNamesGreaterThan255);
            this.tabDownloads.Controls.Add(this.txtSavePath);
            this.tabDownloads.Controls.Add(this.chkPreventDuplicates);
            this.tabDownloads.Controls.Add(this.chkSaveOriginalFileNames);
            this.tabDownloads.Controls.Add(this.chkDownloadThumbnails);
            this.tabDownloads.Controls.Add(this.chkDownloadHTML);
            this.tabDownloads.Controls.Add(this.numTimer);
            this.tabDownloads.Controls.Add(this.lbTimer);
            this.tabDownloads.Controls.Add(this.lbSavePath);
            this.tabDownloads.Controls.Add(this.btnBrowse);
            this.tabDownloads.Controls.Add(this.chkMoveExistingDownloads);
            this.tabDownloads.Controls.Add(this.lbScanDelaySeconds);
            this.tabDownloads.Location = new System.Drawing.Point(4, 22);
            this.tabDownloads.Name = "tabDownloads";
            this.tabDownloads.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloads.Size = new System.Drawing.Size(386, 144);
            this.tabDownloads.TabIndex = 0;
            this.tabDownloads.Text = "Downloads";
            this.tabDownloads.UseVisualStyleBackColor = true;
            // 
            // chkUseProxy
            // 
            this.chkUseProxy.AutoSize = true;
            this.chkUseProxy.Location = new System.Drawing.Point(178, 39);
            this.chkUseProxy.Name = "chkUseProxy";
            this.chkUseProxy.Size = new System.Drawing.Size(51, 17);
            this.chkUseProxy.TabIndex = 38;
            this.chkUseProxy.Text = "Proxy";
            this.ttSettings.SetToolTip(this.chkUseProxy, "Whether the proxy provided will be used.\r\n\r\nDefault: Off");
            this.chkUseProxy.UseVisualStyleBackColor = true;
            // 
            // txtProxy
            // 
            this.txtProxy.ButtonAlignment = murrty.controls.ButtonAlignment.Left;
            this.txtProxy.ButtonCursor = System.Windows.Forms.Cursors.Default;
            this.txtProxy.ButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProxy.ButtonImageIndex = -1;
            this.txtProxy.ButtonImageKey = "";
            this.txtProxy.ButtonSize = new System.Drawing.Size(22, 19);
            this.txtProxy.ButtonText = "";
            this.txtProxy.ButtonTextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtProxy.Location = new System.Drawing.Point(231, 37);
            this.txtProxy.Name = "txtProxy";
            this.txtProxy.Size = new System.Drawing.Size(143, 20);
            this.txtProxy.TabIndex = 37;
            this.txtProxy.TextHint = "socks5://127.0.0.1:12345";
            this.ttSettings.SetToolTip(this.txtProxy, "The proxy that will be used to connect.\r\n\r\nEnter a value such as \"socks5://127.0." +
        "0.1:12345\"\r\nAn easier input setting will be added in the future");
            // 
            // chkAutoRemoveDeadThreads
            // 
            this.chkAutoRemoveDeadThreads.AutoSize = true;
            this.chkAutoRemoveDeadThreads.Location = new System.Drawing.Point(194, 112);
            this.chkAutoRemoveDeadThreads.Name = "chkAutoRemoveDeadThreads";
            this.chkAutoRemoveDeadThreads.Size = new System.Drawing.Size(150, 17);
            this.chkAutoRemoveDeadThreads.TabIndex = 35;
            this.chkAutoRemoveDeadThreads.Text = "Auto-remove dead threads";
            this.ttSettings.SetToolTip(this.chkAutoRemoveDeadThreads, "Automatically removes any threads that 404 (or archived after scanning and downlo" +
        "ad) from the queue\r\n\r\nDefault: Off");
            this.chkAutoRemoveDeadThreads.UseVisualStyleBackColor = true;
            // 
            // chkCleanThreadHTML
            // 
            this.chkCleanThreadHTML.AutoSize = true;
            this.chkCleanThreadHTML.Location = new System.Drawing.Point(265, 66);
            this.chkCleanThreadHTML.Name = "chkCleanThreadHTML";
            this.chkCleanThreadHTML.Size = new System.Drawing.Size(85, 17);
            this.chkCleanThreadHTML.TabIndex = 34;
            this.chkCleanThreadHTML.Text = "Clean HTML";
            this.ttSettings.SetToolTip(this.chkCleanThreadHTML, "The HTML that gets downloaded will be clean and readable, if you read the raw HTM" +
        "L\r\n\r\nDefault: Off");
            this.chkCleanThreadHTML.UseVisualStyleBackColor = true;
            // 
            // chkAllowFileNamesGreaterThan255
            // 
            this.chkAllowFileNamesGreaterThan255.AutoSize = true;
            this.chkAllowFileNamesGreaterThan255.Location = new System.Drawing.Point(11, 112);
            this.chkAllowFileNamesGreaterThan255.Name = "chkAllowFileNamesGreaterThan255";
            this.chkAllowFileNamesGreaterThan255.Size = new System.Drawing.Size(177, 17);
            this.chkAllowFileNamesGreaterThan255.TabIndex = 30;
            this.chkAllowFileNamesGreaterThan255.Text = "Allow file names with high length";
            this.ttSettings.SetToolTip(this.chkAllowFileNamesGreaterThan255, resources.GetString("chkAllowFileNamesGreaterThan255.ToolTip"));
            this.chkAllowFileNamesGreaterThan255.UseVisualStyleBackColor = true;
            // 
            // txtSavePath
            // 
            this.txtSavePath.ButtonAlignment = murrty.controls.ButtonAlignment.Left;
            this.txtSavePath.ButtonCursor = System.Windows.Forms.Cursors.Default;
            this.txtSavePath.ButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSavePath.ButtonImageIndex = -1;
            this.txtSavePath.ButtonImageKey = "";
            this.txtSavePath.ButtonSize = new System.Drawing.Size(22, 19);
            this.txtSavePath.ButtonText = "";
            this.txtSavePath.ButtonTextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.chkPreventDuplicates.Location = new System.Drawing.Point(11, 89);
            this.chkPreventDuplicates.Name = "chkPreventDuplicates";
            this.chkPreventDuplicates.Size = new System.Drawing.Size(113, 17);
            this.chkPreventDuplicates.TabIndex = 27;
            this.chkPreventDuplicates.Text = "Prevent duplicates";
            this.ttSettings.SetToolTip(this.chkPreventDuplicates, "Prevents duplicates of original file names by adding the index of the file\r\nto th" +
        "e prefix of the file name.\r\n\r\nExample: FileName (Duplicate).jpg, FileName (Dupli" +
        "cate 2).jpg...\r\n\r\nDefault: Off");
            this.chkPreventDuplicates.UseVisualStyleBackColor = true;
            // 
            // chkSaveOriginalFileNames
            // 
            this.chkSaveOriginalFileNames.AutoSize = true;
            this.chkSaveOriginalFileNames.Location = new System.Drawing.Point(11, 66);
            this.chkSaveOriginalFileNames.Name = "chkSaveOriginalFileNames";
            this.chkSaveOriginalFileNames.Size = new System.Drawing.Size(136, 17);
            this.chkSaveOriginalFileNames.TabIndex = 26;
            this.chkSaveOriginalFileNames.Text = "Save original file names";
            this.ttSettings.SetToolTip(this.chkSaveOriginalFileNames, "Saves files as their uploaded file names, instead of generated file IDs\r\n\r\nDefaul" +
        "t: Off");
            this.chkSaveOriginalFileNames.UseVisualStyleBackColor = true;
            // 
            // chkDownloadThumbnails
            // 
            this.chkDownloadThumbnails.AutoSize = true;
            this.chkDownloadThumbnails.Location = new System.Drawing.Point(153, 89);
            this.chkDownloadThumbnails.Name = "chkDownloadThumbnails";
            this.chkDownloadThumbnails.Size = new System.Drawing.Size(126, 17);
            this.chkDownloadThumbnails.TabIndex = 24;
            this.chkDownloadThumbnails.Text = "Download thumbnails";
            this.ttSettings.SetToolTip(this.chkDownloadThumbnails, "Downloads thumbnails for files in the thumb folder\r\n\r\nThis is a good option to in" +
        "clude with Download HTML, but may take\r\nup more disk space for the image thumbna" +
        "ils\r\n\r\nDefault: Off");
            this.chkDownloadThumbnails.UseVisualStyleBackColor = true;
            // 
            // chkDownloadHTML
            // 
            this.chkDownloadHTML.AutoSize = true;
            this.chkDownloadHTML.Location = new System.Drawing.Point(153, 66);
            this.chkDownloadHTML.Name = "chkDownloadHTML";
            this.chkDownloadHTML.Size = new System.Drawing.Size(106, 17);
            this.chkDownloadHTML.TabIndex = 23;
            this.chkDownloadHTML.Text = "Download HTML";
            this.ttSettings.SetToolTip(this.chkDownloadHTML, "Saves HTML of thread data after images download\r\n\r\nDefault: Off");
            this.chkDownloadHTML.UseVisualStyleBackColor = true;
            // 
            // numTimer
            // 
            this.numTimer.Location = new System.Drawing.Point(68, 37);
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
            this.ttSettings.SetToolTip(this.numTimer, "Rescans threads after the specified time (in seconds)\r\n(Note: some sites don\'t li" +
        "ke it if their API is requested more than once in 60 seconds, and may impose lim" +
        "its)\r\n\r\nRecommended: 60");
            this.numTimer.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lbTimer
            // 
            this.lbTimer.AutoSize = true;
            this.lbTimer.Location = new System.Drawing.Point(5, 39);
            this.lbTimer.Name = "lbTimer";
            this.lbTimer.Size = new System.Drawing.Size(60, 13);
            this.lbTimer.TabIndex = 22;
            this.lbTimer.Text = "Scan delay";
            // 
            // lbSavePath
            // 
            this.lbSavePath.AutoSize = true;
            this.lbSavePath.Location = new System.Drawing.Point(5, 14);
            this.lbSavePath.Name = "lbSavePath";
            this.lbSavePath.Size = new System.Drawing.Size(56, 13);
            this.lbSavePath.TabIndex = 20;
            this.lbSavePath.Text = "Save &path\r\n";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(268, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 20);
            this.btnBrowse.TabIndex = 18;
            this.btnBrowse.Text = "...";
            this.ttSettings.SetToolTip(this.btnBrowse, "Browses for a new directory for threads to be saved at.");
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // chkMoveExistingDownloads
            // 
            this.chkMoveExistingDownloads.AutoSize = true;
            this.chkMoveExistingDownloads.Checked = true;
            this.chkMoveExistingDownloads.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMoveExistingDownloads.Location = new System.Drawing.Point(296, 6);
            this.chkMoveExistingDownloads.Name = "chkMoveExistingDownloads";
            this.chkMoveExistingDownloads.Size = new System.Drawing.Size(93, 30);
            this.chkMoveExistingDownloads.TabIndex = 19;
            this.chkMoveExistingDownloads.Text = "Move existing \r\ndownloads";
            this.ttSettings.SetToolTip(this.chkMoveExistingDownloads, "Moves existing downloads to the new directory.");
            this.chkMoveExistingDownloads.UseVisualStyleBackColor = true;
            // 
            // lbScanDelaySeconds
            // 
            this.lbScanDelaySeconds.AutoSize = true;
            this.lbScanDelaySeconds.Location = new System.Drawing.Point(126, 39);
            this.lbScanDelaySeconds.Name = "lbScanDelaySeconds";
            this.lbScanDelaySeconds.Size = new System.Drawing.Size(47, 13);
            this.lbScanDelaySeconds.TabIndex = 31;
            this.lbScanDelaySeconds.Text = "seconds";
            // 
            // tabApplication
            // 
            this.tabApplication.Controls.Add(this.chkSaveDownloadHistory);
            this.tabApplication.Controls.Add(this.chkMinimizeInsteadOfExiting);
            this.tabApplication.Controls.Add(this.chkUseFullBoardNameForTitle);
            this.tabApplication.Controls.Add(this.chkSaveDownloadQueueOnExit);
            this.tabApplication.Controls.Add(this.chkEnableUpdates);
            this.tabApplication.Controls.Add(this.chkShowExitWarning);
            this.tabApplication.Controls.Add(this.chkMinimizeToTray);
            this.tabApplication.Controls.Add(this.chkShowTrayIcon);
            this.tabApplication.Location = new System.Drawing.Point(4, 22);
            this.tabApplication.Name = "tabApplication";
            this.tabApplication.Padding = new System.Windows.Forms.Padding(3);
            this.tabApplication.Size = new System.Drawing.Size(386, 144);
            this.tabApplication.TabIndex = 1;
            this.tabApplication.Text = "Application";
            this.tabApplication.UseVisualStyleBackColor = true;
            // 
            // chkSaveDownloadHistory
            // 
            this.chkSaveDownloadHistory.AutoSize = true;
            this.chkSaveDownloadHistory.Location = new System.Drawing.Point(213, 98);
            this.chkSaveDownloadHistory.Name = "chkSaveDownloadHistory";
            this.chkSaveDownloadHistory.Size = new System.Drawing.Size(132, 17);
            this.chkSaveDownloadHistory.TabIndex = 33;
            this.chkSaveDownloadHistory.Text = "Save download history";
            this.ttSettings.SetToolTip(this.chkSaveDownloadHistory, "Saves the history of threads added to the queue\r\n\r\nDefault: Off");
            this.chkSaveDownloadHistory.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeInsteadOfExiting
            // 
            this.chkMinimizeInsteadOfExiting.AutoSize = true;
            this.chkMinimizeInsteadOfExiting.Location = new System.Drawing.Point(40, 52);
            this.chkMinimizeInsteadOfExiting.Name = "chkMinimizeInsteadOfExiting";
            this.chkMinimizeInsteadOfExiting.Size = new System.Drawing.Size(188, 17);
            this.chkMinimizeInsteadOfExiting.TabIndex = 32;
            this.chkMinimizeInsteadOfExiting.Text = "Hide the program instead of exiting";
            this.ttSettings.SetToolTip(this.chkMinimizeInsteadOfExiting, "When exiting the program, it\'ll minimize to the tray instead of exit.\r\nYou\'ll hav" +
        "e to exit using the tray icon.\r\n\r\nDefault: Off");
            this.chkMinimizeInsteadOfExiting.UseVisualStyleBackColor = true;
            // 
            // chkUseFullBoardNameForTitle
            // 
            this.chkUseFullBoardNameForTitle.AutoSize = true;
            this.chkUseFullBoardNameForTitle.Location = new System.Drawing.Point(166, 75);
            this.chkUseFullBoardNameForTitle.Name = "chkUseFullBoardNameForTitle";
            this.chkUseFullBoardNameForTitle.Size = new System.Drawing.Size(159, 17);
            this.chkUseFullBoardNameForTitle.TabIndex = 31;
            this.chkUseFullBoardNameForTitle.Text = "Use full board names in titles";
            this.ttSettings.SetToolTip(this.chkUseFullBoardNameForTitle, resources.GetString("chkUseFullBoardNameForTitle.ToolTip"));
            this.chkUseFullBoardNameForTitle.UseVisualStyleBackColor = true;
            // 
            // chkSaveDownloadQueueOnExit
            // 
            this.chkSaveDownloadQueueOnExit.AutoSize = true;
            this.chkSaveDownloadQueueOnExit.Location = new System.Drawing.Point(41, 98);
            this.chkSaveDownloadQueueOnExit.Name = "chkSaveDownloadQueueOnExit";
            this.chkSaveDownloadQueueOnExit.Size = new System.Drawing.Size(166, 17);
            this.chkSaveDownloadQueueOnExit.TabIndex = 29;
            this.chkSaveDownloadQueueOnExit.Text = "Save download queue on exit";
            this.ttSettings.SetToolTip(this.chkSaveDownloadQueueOnExit, "Saves the download queue to an xml file on the application\'s exit.\r\n\r\nDefault: On" +
        "");
            this.chkSaveDownloadQueueOnExit.UseVisualStyleBackColor = true;
            // 
            // chkEnableUpdates
            // 
            this.chkEnableUpdates.AutoSize = true;
            this.chkEnableUpdates.Location = new System.Drawing.Point(61, 75);
            this.chkEnableUpdates.Name = "chkEnableUpdates";
            this.chkEnableUpdates.Size = new System.Drawing.Size(99, 17);
            this.chkEnableUpdates.TabIndex = 30;
            this.chkEnableUpdates.Text = "Enable updates";
            this.ttSettings.SetToolTip(this.chkEnableUpdates, "Enables checking for updates for this application.\r\n\r\nDefault: On");
            this.chkEnableUpdates.UseVisualStyleBackColor = true;
            // 
            // chkShowExitWarning
            // 
            this.chkShowExitWarning.AutoSize = true;
            this.chkShowExitWarning.Location = new System.Drawing.Point(235, 52);
            this.chkShowExitWarning.Name = "chkShowExitWarning";
            this.chkShowExitWarning.Size = new System.Drawing.Size(111, 17);
            this.chkShowExitWarning.TabIndex = 29;
            this.chkShowExitWarning.Text = "Show exit warning";
            this.ttSettings.SetToolTip(this.chkShowExitWarning, "Shows a warning to minimize instead of exiting if there are threads\r\ncurrently in" +
        " the download queue.\r\n\r\nDefault: Off");
            this.chkShowExitWarning.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(195, 29);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(97, 17);
            this.chkMinimizeToTray.TabIndex = 28;
            this.chkMinimizeToTray.Text = "Minimize to tray";
            this.ttSettings.SetToolTip(this.chkMinimizeToTray, "Minimizes the program to the system\'s tray\r\n\r\nDefault: On");
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // chkShowTrayIcon
            // 
            this.chkShowTrayIcon.AutoSize = true;
            this.chkShowTrayIcon.Location = new System.Drawing.Point(94, 29);
            this.chkShowTrayIcon.Name = "chkShowTrayIcon";
            this.chkShowTrayIcon.Size = new System.Drawing.Size(95, 17);
            this.chkShowTrayIcon.TabIndex = 27;
            this.chkShowTrayIcon.Text = "Show tray icon";
            this.ttSettings.SetToolTip(this.chkShowTrayIcon, "Shows the YChanEx icon in the system tray\r\n\r\nDefault: Off");
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
            this.tabAdvanced.Size = new System.Drawing.Size(386, 144);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // btnOpenLocalFiles
            // 
            this.btnOpenLocalFiles.Location = new System.Drawing.Point(148, 94);
            this.btnOpenLocalFiles.Name = "btnOpenLocalFiles";
            this.btnOpenLocalFiles.Size = new System.Drawing.Size(98, 24);
            this.btnOpenLocalFiles.TabIndex = 4;
            this.btnOpenLocalFiles.Text = "Open local files";
            this.ttSettings.SetToolTip(this.btnOpenLocalFiles, "Browse locally saved program data.\r\nThis is not your threads directory, unless yo" +
        "u chose to save threads here.");
            this.btnOpenLocalFiles.UseVisualStyleBackColor = true;
            this.btnOpenLocalFiles.Click += new System.EventHandler(this.btnOpenLocalFiles_Click);
            // 
            // chkSilenceErrors
            // 
            this.chkSilenceErrors.AutoSize = true;
            this.chkSilenceErrors.Location = new System.Drawing.Point(261, 64);
            this.chkSilenceErrors.Name = "chkSilenceErrors";
            this.chkSilenceErrors.Size = new System.Drawing.Size(89, 17);
            this.chkSilenceErrors.TabIndex = 3;
            this.chkSilenceErrors.Text = "Silence errors";
            this.ttSettings.SetToolTip(this.chkSilenceErrors, resources.GetString("chkSilenceErrors.ToolTip"));
            this.chkSilenceErrors.UseVisualStyleBackColor = true;
            // 
            // chkDisableScannerWhenOpeningSettings
            // 
            this.chkDisableScannerWhenOpeningSettings.AutoSize = true;
            this.chkDisableScannerWhenOpeningSettings.Location = new System.Drawing.Point(45, 64);
            this.chkDisableScannerWhenOpeningSettings.Name = "chkDisableScannerWhenOpeningSettings";
            this.chkDisableScannerWhenOpeningSettings.Size = new System.Drawing.Size(210, 17);
            this.chkDisableScannerWhenOpeningSettings.TabIndex = 2;
            this.chkDisableScannerWhenOpeningSettings.Text = "Disable scanner when opening settings";
            this.ttSettings.SetToolTip(this.chkDisableScannerWhenOpeningSettings, resources.GetString("chkDisableScannerWhenOpeningSettings.ToolTip"));
            this.chkDisableScannerWhenOpeningSettings.UseVisualStyleBackColor = true;
            // 
            // txtUserAgent
            // 
            this.txtUserAgent.ButtonAlignment = murrty.controls.ButtonAlignment.Left;
            this.txtUserAgent.ButtonCursor = System.Windows.Forms.Cursors.Default;
            this.txtUserAgent.ButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserAgent.ButtonImageIndex = -1;
            this.txtUserAgent.ButtonImageKey = "";
            this.txtUserAgent.ButtonSize = new System.Drawing.Size(22, 19);
            this.txtUserAgent.ButtonText = "";
            this.txtUserAgent.ButtonTextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtUserAgent.Location = new System.Drawing.Point(74, 26);
            this.txtUserAgent.Name = "txtUserAgent";
            this.txtUserAgent.Size = new System.Drawing.Size(302, 20);
            this.txtUserAgent.TabIndex = 1;
            this.txtUserAgent.TextHint = "Mozilla/5.0 (X11; Linux i686; rv:64.0) Gecko/20100101 Firefox/84.0";
            this.ttSettings.SetToolTip(this.txtUserAgent, "The user-agent for the webclients to use");
            // 
            // lbUserAgent
            // 
            this.lbUserAgent.AutoSize = true;
            this.lbUserAgent.Location = new System.Drawing.Point(8, 29);
            this.lbUserAgent.Name = "lbUserAgent";
            this.lbUserAgent.Size = new System.Drawing.Size(60, 13);
            this.lbUserAgent.TabIndex = 0;
            this.lbUserAgent.Text = "User-Agent";
            // 
            // tabCookies
            // 
            this.tabCookies.Controls.Add(this.btnRemoveCookie);
            this.tabCookies.Controls.Add(this.btnAddCookie);
            this.tabCookies.Controls.Add(this.lvCookies);
            this.tabCookies.Location = new System.Drawing.Point(4, 22);
            this.tabCookies.Name = "tabCookies";
            this.tabCookies.Padding = new System.Windows.Forms.Padding(3);
            this.tabCookies.Size = new System.Drawing.Size(386, 144);
            this.tabCookies.TabIndex = 5;
            this.tabCookies.Text = "Cookies";
            this.tabCookies.UseVisualStyleBackColor = true;
            // 
            // btnRemoveCookie
            // 
            this.btnRemoveCookie.Location = new System.Drawing.Point(222, 115);
            this.btnRemoveCookie.Name = "btnRemoveCookie";
            this.btnRemoveCookie.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveCookie.TabIndex = 2;
            this.btnRemoveCookie.Text = "Remove";
            this.btnRemoveCookie.UseVisualStyleBackColor = true;
            // 
            // btnAddCookie
            // 
            this.btnAddCookie.Location = new System.Drawing.Point(303, 115);
            this.btnAddCookie.Name = "btnAddCookie";
            this.btnAddCookie.Size = new System.Drawing.Size(75, 23);
            this.btnAddCookie.TabIndex = 1;
            this.btnAddCookie.Text = "Add...";
            this.btnAddCookie.UseVisualStyleBackColor = true;
            this.btnAddCookie.Click += new System.EventHandler(this.btnAddCookie_Click);
            // 
            // lvCookies
            // 
            this.lvCookies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCookies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chCookieName,
            this.chCookieValue,
            this.chCookiePath,
            this.chCookieDomain});
            this.lvCookies.HideSelection = false;
            this.lvCookies.Location = new System.Drawing.Point(6, 6);
            this.lvCookies.Name = "lvCookies";
            this.lvCookies.Size = new System.Drawing.Size(374, 107);
            this.lvCookies.TabIndex = 0;
            this.lvCookies.UseCompatibleStateImageBehavior = false;
            this.lvCookies.View = System.Windows.Forms.View.Details;
            // 
            // chCookieName
            // 
            this.chCookieName.Text = "Name";
            this.chCookieName.Width = 102;
            // 
            // chCookieValue
            // 
            this.chCookieValue.Text = "Value";
            this.chCookieValue.Width = 108;
            // 
            // chCookiePath
            // 
            this.chCookiePath.Text = "Path";
            // 
            // chCookieDomain
            // 
            this.chCookieDomain.Text = "Domain";
            this.chCookieDomain.Width = 99;
            // 
            // tabReset
            // 
            this.tabReset.Controls.Add(this.chkEnableSettingsReset);
            this.tabReset.Controls.Add(this.chkResetRegexSettings);
            this.tabReset.Controls.Add(this.chkResetAdvancedSettings);
            this.tabReset.Controls.Add(this.chkResetApplicationSettings);
            this.tabReset.Controls.Add(this.chkResetDownloadSettings);
            this.tabReset.Controls.Add(this.btnResetSettings);
            this.tabReset.Location = new System.Drawing.Point(4, 22);
            this.tabReset.Name = "tabReset";
            this.tabReset.Padding = new System.Windows.Forms.Padding(3);
            this.tabReset.Size = new System.Drawing.Size(386, 144);
            this.tabReset.TabIndex = 3;
            this.tabReset.Text = "Reset";
            this.tabReset.UseVisualStyleBackColor = true;
            // 
            // chkEnableSettingsReset
            // 
            this.chkEnableSettingsReset.AutoSize = true;
            this.chkEnableSettingsReset.Location = new System.Drawing.Point(278, 115);
            this.chkEnableSettingsReset.Name = "chkEnableSettingsReset";
            this.chkEnableSettingsReset.Size = new System.Drawing.Size(61, 17);
            this.chkEnableSettingsReset.TabIndex = 5;
            this.chkEnableSettingsReset.Text = "I\'m sure";
            this.ttSettings.SetToolTip(this.chkEnableSettingsReset, "Enables the Reset Selection Options button.\r\nThis is a safety net.");
            this.chkEnableSettingsReset.UseVisualStyleBackColor = true;
            this.chkEnableSettingsReset.CheckedChanged += new System.EventHandler(this.chkEnableSettingsReset_CheckedChanged);
            // 
            // chkResetRegexSettings
            // 
            this.chkResetRegexSettings.AutoSize = true;
            this.chkResetRegexSettings.Location = new System.Drawing.Point(118, 88);
            this.chkResetRegexSettings.Name = "chkResetRegexSettings";
            this.chkResetRegexSettings.Size = new System.Drawing.Size(128, 17);
            this.chkResetRegexSettings.TabIndex = 4;
            this.chkResetRegexSettings.Text = "Reset Regex Settings";
            this.ttSettings.SetToolTip(this.chkResetRegexSettings, "Enabling this will reset all settings in the Regex tab.");
            this.chkResetRegexSettings.UseVisualStyleBackColor = true;
            // 
            // chkResetAdvancedSettings
            // 
            this.chkResetAdvancedSettings.AutoSize = true;
            this.chkResetAdvancedSettings.Location = new System.Drawing.Point(118, 65);
            this.chkResetAdvancedSettings.Name = "chkResetAdvancedSettings";
            this.chkResetAdvancedSettings.Size = new System.Drawing.Size(146, 17);
            this.chkResetAdvancedSettings.TabIndex = 3;
            this.chkResetAdvancedSettings.Text = "Reset Advanced Settings";
            this.ttSettings.SetToolTip(this.chkResetAdvancedSettings, "Enabling this will reset all settings in the Advanced tab.");
            this.chkResetAdvancedSettings.UseVisualStyleBackColor = true;
            // 
            // chkResetApplicationSettings
            // 
            this.chkResetApplicationSettings.AutoSize = true;
            this.chkResetApplicationSettings.Location = new System.Drawing.Point(118, 42);
            this.chkResetApplicationSettings.Name = "chkResetApplicationSettings";
            this.chkResetApplicationSettings.Size = new System.Drawing.Size(149, 17);
            this.chkResetApplicationSettings.TabIndex = 2;
            this.chkResetApplicationSettings.Text = "Reset Application Settings";
            this.ttSettings.SetToolTip(this.chkResetApplicationSettings, "Enabling this will reset all settings in the Application tab.");
            this.chkResetApplicationSettings.UseVisualStyleBackColor = true;
            // 
            // chkResetDownloadSettings
            // 
            this.chkResetDownloadSettings.AutoSize = true;
            this.chkResetDownloadSettings.Location = new System.Drawing.Point(118, 19);
            this.chkResetDownloadSettings.Name = "chkResetDownloadSettings";
            this.chkResetDownloadSettings.Size = new System.Drawing.Size(145, 17);
            this.chkResetDownloadSettings.TabIndex = 1;
            this.chkResetDownloadSettings.Text = "Reset Download Settings";
            this.ttSettings.SetToolTip(this.chkResetDownloadSettings, "Enabling this will reset all settings in the Downloads tab.");
            this.chkResetDownloadSettings.UseVisualStyleBackColor = true;
            // 
            // btnResetSettings
            // 
            this.btnResetSettings.Enabled = false;
            this.btnResetSettings.Location = new System.Drawing.Point(114, 111);
            this.btnResetSettings.Name = "btnResetSettings";
            this.btnResetSettings.Size = new System.Drawing.Size(158, 23);
            this.btnResetSettings.TabIndex = 0;
            this.btnResetSettings.Text = "Reset Selected Options";
            this.ttSettings.SetToolTip(this.btnResetSettings, "Completely and unapologetically resets the program\'s options.\r\n\r\nMay cause slight" +
        ", if not major, irritation on resetting when you\r\ndidn\'t actually mean to do it." +
        "");
            this.btnResetSettings.UseVisualStyleBackColor = true;
            this.btnResetSettings.Click += new System.EventHandler(this.btnResetSettings_Click);
            // 
            // btnUserScript
            // 
            this.btnUserScript.Location = new System.Drawing.Point(12, 173);
            this.btnUserScript.Name = "btnUserScript";
            this.btnUserScript.Size = new System.Drawing.Size(75, 24);
            this.btnUserScript.TabIndex = 25;
            this.btnUserScript.Text = "Userscript";
            this.ttSettings.SetToolTip(this.btnUserScript, "Install the userscript for the application");
            this.btnUserScript.UseVisualStyleBackColor = true;
            this.btnUserScript.Click += new System.EventHandler(this.btnUserScript_Click);
            // 
            // btnProtocol
            // 
            this.btnProtocol.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnProtocol.Location = new System.Drawing.Point(93, 173);
            this.btnProtocol.Name = "btnProtocol";
            this.btnProtocol.Size = new System.Drawing.Size(116, 24);
            this.btnProtocol.TabIndex = 24;
            this.btnProtocol.Text = "Install protocol";
            this.ttSettings.SetToolTip(this.btnProtocol, "Install the protocol for the userscript");
            this.btnProtocol.UseVisualStyleBackColor = true;
            this.btnProtocol.Visible = false;
            this.btnProtocol.Click += new System.EventHandler(this.btnProtocol_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(307, 173);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 24);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "&Cancel";
            this.ttSettings.SetToolTip(this.btnCancel, "Does not save any settings changed.\r\n");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(226, 173);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 24);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "&Save";
            this.ttSettings.SetToolTip(this.btnSave, "Saves the settings.");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ttSettings
            // 
            this.ttSettings.AutoPopDelay = 25000;
            this.ttSettings.InitialDelay = 500;
            this.ttSettings.ReshowDelay = 100;
            this.ttSettings.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ttSettings.ToolTipTitle = "Information:";
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(394, 207);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnUserScript);
            this.Controls.Add(this.btnProtocol);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tcMain);
            this.Icon = global::YChanEx.Properties.Resources.ProgramIcon;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(410, 242);
            this.MinimumSize = new System.Drawing.Size(410, 242);
            this.Name = "frmSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "YChanEx settings";
            this.tcMain.ResumeLayout(false);
            this.tabDownloads.ResumeLayout(false);
            this.tabDownloads.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).EndInit();
            this.tabApplication.ResumeLayout(false);
            this.tabApplication.PerformLayout();
            this.tabAdvanced.ResumeLayout(false);
            this.tabAdvanced.PerformLayout();
            this.tabCookies.ResumeLayout(false);
            this.tabReset.ResumeLayout(false);
            this.tabReset.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tabDownloads;
        private System.Windows.Forms.NumericUpDown numTimer;
        private System.Windows.Forms.Label lbTimer;
        private System.Windows.Forms.Label lbSavePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkMoveExistingDownloads;
        private System.Windows.Forms.TabPage tabApplication;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.TabPage tabReset;
        private System.Windows.Forms.Button btnUserScript;
        private System.Windows.Forms.Button btnProtocol;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkPreventDuplicates;
        private System.Windows.Forms.CheckBox chkSaveOriginalFileNames;
        private System.Windows.Forms.CheckBox chkDownloadThumbnails;
        private System.Windows.Forms.CheckBox chkDownloadHTML;
        private murrty.controls.ExtendedTextBox txtSavePath;
        private System.Windows.Forms.CheckBox chkUseFullBoardNameForTitle;
        private System.Windows.Forms.CheckBox chkEnableUpdates;
        private System.Windows.Forms.CheckBox chkShowExitWarning;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkShowTrayIcon;
        private System.Windows.Forms.ToolTip ttSettings;
        private System.Windows.Forms.CheckBox chkSaveDownloadQueueOnExit;
        private System.Windows.Forms.Label lbUserAgent;
        private System.Windows.Forms.CheckBox chkSilenceErrors;
        private System.Windows.Forms.CheckBox chkDisableScannerWhenOpeningSettings;
        private murrty.controls.ExtendedTextBox txtUserAgent;
        private System.Windows.Forms.Button btnOpenLocalFiles;
        private System.Windows.Forms.CheckBox chkAllowFileNamesGreaterThan255;
        private System.Windows.Forms.CheckBox chkMinimizeInsteadOfExiting;
        private System.Windows.Forms.CheckBox chkResetRegexSettings;
        private System.Windows.Forms.CheckBox chkResetAdvancedSettings;
        private System.Windows.Forms.CheckBox chkResetApplicationSettings;
        private System.Windows.Forms.CheckBox chkResetDownloadSettings;
        private System.Windows.Forms.Button btnResetSettings;
        private System.Windows.Forms.CheckBox chkEnableSettingsReset;
        private System.Windows.Forms.Label lbScanDelaySeconds;
        private System.Windows.Forms.CheckBox chkCleanThreadHTML;
        private System.Windows.Forms.CheckBox chkSaveDownloadHistory;
        private System.Windows.Forms.CheckBox chkAutoRemoveDeadThreads;
        private System.Windows.Forms.TabPage tabCookies;
        private System.Windows.Forms.ListView lvCookies;
        private System.Windows.Forms.Button btnRemoveCookie;
        private System.Windows.Forms.Button btnAddCookie;
        private System.Windows.Forms.ColumnHeader chCookieName;
        private System.Windows.Forms.ColumnHeader chCookieValue;
        private System.Windows.Forms.ColumnHeader chCookiePath;
        private System.Windows.Forms.ColumnHeader chCookieDomain;
        private murrty.controls.ExtendedTextBox txtProxy;
        private System.Windows.Forms.CheckBox chkUseProxy;
    }
}