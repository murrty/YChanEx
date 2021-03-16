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
            System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem("4chan URL");
            System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem("420chan URL");
            System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem(new string[] {
            "7chan URL",
            "1"}, -1);
            System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem("7chan Posts");
            System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem("8chan URL");
            System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem("8kun URL");
            System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem("fchan URL");
            System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem("fchan Files");
            System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem("fchan IDs");
            System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem("u18chan URL");
            System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem("u18chan Posts");
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tabDownloads = new System.Windows.Forms.TabPage();
            this.chkRetrieveThreadName = new System.Windows.Forms.CheckBox();
            this.chkAllowFileNamesGreaterThan255 = new System.Windows.Forms.CheckBox();
            this.txtSavePath = new YChanEx.HintTextBox();
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
            this.txtUserAgent = new YChanEx.HintTextBox();
            this.lbUserAgent = new System.Windows.Forms.Label();
            this.tabReset = new System.Windows.Forms.TabPage();
            this.chkEnableSettingsReset = new System.Windows.Forms.CheckBox();
            this.chkResetRegexSettings = new System.Windows.Forms.CheckBox();
            this.chkResetAdvancedSettings = new System.Windows.Forms.CheckBox();
            this.chkResetApplicationSettings = new System.Windows.Forms.CheckBox();
            this.chkResetDownloadSettings = new System.Windows.Forms.CheckBox();
            this.btnResetSettings = new System.Windows.Forms.Button();
            this.tabRegex = new System.Windows.Forms.TabPage();
            this.lbRegexHint = new System.Windows.Forms.Label();
            this.lvRegex = new System.Windows.Forms.ListView();
            this.txtRegex = new YChanEx.HintTextBox();
            this.lbRegexInfo = new System.Windows.Forms.Label();
            this.btnUserScript = new System.Windows.Forms.Button();
            this.btnProtocol = new System.Windows.Forms.Button();
            this.btnSCan = new System.Windows.Forms.Button();
            this.btnSSave = new System.Windows.Forms.Button();
            this.ttSettings = new System.Windows.Forms.ToolTip(this.components);
            this.tcMain.SuspendLayout();
            this.tabDownloads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimer)).BeginInit();
            this.tabApplication.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.tabReset.SuspendLayout();
            this.tabRegex.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tabDownloads);
            this.tcMain.Controls.Add(this.tabApplication);
            this.tcMain.Controls.Add(this.tabAdvanced);
            this.tcMain.Controls.Add(this.tabRegex);
            this.tcMain.Controls.Add(this.tabReset);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(392, 170);
            this.tcMain.TabIndex = 0;
            // 
            // tabDownloads
            // 
            this.tabDownloads.Controls.Add(this.chkRetrieveThreadName);
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
            this.tabDownloads.Size = new System.Drawing.Size(384, 144);
            this.tabDownloads.TabIndex = 0;
            this.tabDownloads.Text = "Downloads";
            this.tabDownloads.UseVisualStyleBackColor = true;
            // 
            // chkRetrieveThreadName
            // 
            this.chkRetrieveThreadName.AutoSize = true;
            this.chkRetrieveThreadName.Location = new System.Drawing.Point(195, 112);
            this.chkRetrieveThreadName.Name = "chkRetrieveThreadName";
            this.chkRetrieveThreadName.Size = new System.Drawing.Size(168, 17);
            this.chkRetrieveThreadName.TabIndex = 33;
            this.chkRetrieveThreadName.Text = "Retrieve and use thread name";
            this.ttSettings.SetToolTip(this.chkRetrieveThreadName, "Uses the OP\'s post to identify the thread in the download form and the main form." +
        "\r\n\r\nSome sites already do this in the HTML title, while others require more pars" +
        "ing.");
            this.chkRetrieveThreadName.UseVisualStyleBackColor = true;
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
        "e prefix of the file name.\r\n\r\nExample: [002] FileName.jpg");
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
            this.ttSettings.SetToolTip(this.chkSaveOriginalFileNames, "Saves files as their uploaded file names, instead of generated file IDs.\r\n\r\nExamp" +
        "le: FileName.jpg");
            this.chkSaveOriginalFileNames.UseVisualStyleBackColor = true;
            // 
            // chkDownloadThumbnails
            // 
            this.chkDownloadThumbnails.AutoSize = true;
            this.chkDownloadThumbnails.Location = new System.Drawing.Point(195, 89);
            this.chkDownloadThumbnails.Name = "chkDownloadThumbnails";
            this.chkDownloadThumbnails.Size = new System.Drawing.Size(126, 17);
            this.chkDownloadThumbnails.TabIndex = 24;
            this.chkDownloadThumbnails.Text = "Download thumbnails";
            this.ttSettings.SetToolTip(this.chkDownloadThumbnails, "Downloads thumbnails for files in the thumb folder.\r\n\r\nThis is a good option to i" +
        "nclude with Download HTML, but may take\r\nup more disk space for the image thumbn" +
        "ails.");
            this.chkDownloadThumbnails.UseVisualStyleBackColor = true;
            // 
            // chkDownloadHTML
            // 
            this.chkDownloadHTML.AutoSize = true;
            this.chkDownloadHTML.Location = new System.Drawing.Point(195, 66);
            this.chkDownloadHTML.Name = "chkDownloadHTML";
            this.chkDownloadHTML.Size = new System.Drawing.Size(106, 17);
            this.chkDownloadHTML.TabIndex = 23;
            this.chkDownloadHTML.Text = "Download HTML";
            this.ttSettings.SetToolTip(this.chkDownloadHTML, "Downloads thread HTML files as Thread.html in the thread folder.");
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
            this.tabApplication.Size = new System.Drawing.Size(384, 144);
            this.tabApplication.TabIndex = 1;
            this.tabApplication.Text = "Application";
            this.tabApplication.UseVisualStyleBackColor = true;
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
        "e to exit using the tray icon.");
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
            this.chkSaveDownloadQueueOnExit.Location = new System.Drawing.Point(109, 98);
            this.chkSaveDownloadQueueOnExit.Name = "chkSaveDownloadQueueOnExit";
            this.chkSaveDownloadQueueOnExit.Size = new System.Drawing.Size(166, 17);
            this.chkSaveDownloadQueueOnExit.TabIndex = 29;
            this.chkSaveDownloadQueueOnExit.Text = "Save download queue on exit";
            this.ttSettings.SetToolTip(this.chkSaveDownloadQueueOnExit, "Saves the download queue to an xml file on the application\'s exit.");
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
            this.ttSettings.SetToolTip(this.chkEnableUpdates, "Enables checking for updates for this application.");
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
        " the download queue.");
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
            this.ttSettings.SetToolTip(this.chkMinimizeToTray, "Minimizes the program to the system\'s tray");
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
            this.tabAdvanced.Size = new System.Drawing.Size(384, 144);
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
            this.ttSettings.SetToolTip(this.chkSilenceErrors, "Does not display any errors that may arise from any problems that may occur.\r\n\r\nN" +
        "ot recommended to enable, but may help if there are an uptick in errors\r\nthat do" +
        "n\'t effect the your regular use.");
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
            this.tabReset.Size = new System.Drawing.Size(384, 144);
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
            // tabRegex
            // 
            this.tabRegex.Controls.Add(this.lbRegexHint);
            this.tabRegex.Controls.Add(this.lvRegex);
            this.tabRegex.Controls.Add(this.txtRegex);
            this.tabRegex.Controls.Add(this.lbRegexInfo);
            this.tabRegex.Location = new System.Drawing.Point(4, 22);
            this.tabRegex.Name = "tabRegex";
            this.tabRegex.Padding = new System.Windows.Forms.Padding(3);
            this.tabRegex.Size = new System.Drawing.Size(384, 144);
            this.tabRegex.TabIndex = 4;
            this.tabRegex.Text = "Regex";
            this.tabRegex.UseVisualStyleBackColor = true;
            // 
            // lbRegexHint
            // 
            this.lbRegexHint.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRegexHint.Location = new System.Drawing.Point(116, 46);
            this.lbRegexHint.Name = "lbRegexHint";
            this.lbRegexHint.Size = new System.Drawing.Size(262, 65);
            this.lbRegexHint.TabIndex = 3;
            this.lbRegexHint.Text = "This is the URL to direct the parser to the 4chan API.";
            this.lbRegexHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lvRegex
            // 
            this.lvRegex.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem12,
            listViewItem13,
            listViewItem14,
            listViewItem15,
            listViewItem16,
            listViewItem17,
            listViewItem18,
            listViewItem19,
            listViewItem20,
            listViewItem21,
            listViewItem22});
            this.lvRegex.Location = new System.Drawing.Point(3, 40);
            this.lvRegex.MultiSelect = false;
            this.lvRegex.Name = "lvRegex";
            this.lvRegex.Size = new System.Drawing.Size(107, 101);
            this.lvRegex.TabIndex = 2;
            this.lvRegex.UseCompatibleStateImageBehavior = false;
            this.lvRegex.View = System.Windows.Forms.View.SmallIcon;
            this.lvRegex.SelectedIndexChanged += new System.EventHandler(this.lvRegex_SelectedIndexChanged);
            // 
            // txtRegex
            // 
            this.txtRegex.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRegex.Location = new System.Drawing.Point(116, 114);
            this.txtRegex.Name = "txtRegex";
            this.txtRegex.Size = new System.Drawing.Size(262, 22);
            this.txtRegex.TabIndex = 1;
            this.txtRegex.TextHint = "No regex pattern selected.";
            this.txtRegex.TextChanged += new System.EventHandler(this.txtRegex_TextChanged);
            // 
            // lbRegexInfo
            // 
            this.lbRegexInfo.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbRegexInfo.Location = new System.Drawing.Point(8, 3);
            this.lbRegexInfo.Name = "lbRegexInfo";
            this.lbRegexInfo.Size = new System.Drawing.Size(370, 34);
            this.lbRegexInfo.TabIndex = 0;
            this.lbRegexInfo.Text = "This tab allows you to change Regex patterns if they change.\r\nYou\'re better off l" +
    "eaving these set empty if you\'re unsure.";
            this.lbRegexInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // 
            // btnProtocol
            // 
            this.btnProtocol.Enabled = false;
            this.btnProtocol.Location = new System.Drawing.Point(93, 173);
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
            this.btnSCan.Location = new System.Drawing.Point(307, 173);
            this.btnSCan.Name = "btnSCan";
            this.btnSCan.Size = new System.Drawing.Size(75, 24);
            this.btnSCan.TabIndex = 23;
            this.btnSCan.Text = "&Cancel";
            this.ttSettings.SetToolTip(this.btnSCan, "Does not save any settings changed.\r\n");
            this.btnSCan.UseVisualStyleBackColor = true;
            this.btnSCan.Click += new System.EventHandler(this.btnSCan_Click);
            // 
            // btnSSave
            // 
            this.btnSSave.Location = new System.Drawing.Point(226, 173);
            this.btnSSave.Name = "btnSSave";
            this.btnSSave.Size = new System.Drawing.Size(75, 24);
            this.btnSSave.TabIndex = 22;
            this.btnSSave.Text = "&Save";
            this.ttSettings.SetToolTip(this.btnSSave, "Saves the settings.");
            this.btnSSave.UseVisualStyleBackColor = true;
            this.btnSSave.Click += new System.EventHandler(this.btnSSave_Click);
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
            this.ClientSize = new System.Drawing.Size(392, 205);
            this.Controls.Add(this.btnSSave);
            this.Controls.Add(this.btnUserScript);
            this.Controls.Add(this.btnProtocol);
            this.Controls.Add(this.btnSCan);
            this.Controls.Add(this.tcMain);
            this.Icon = global::YChanEx.Properties.Resources.YChanEx;
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
            this.tabReset.ResumeLayout(false);
            this.tabReset.PerformLayout();
            this.tabRegex.ResumeLayout(false);
            this.tabRegex.PerformLayout();
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
        private System.Windows.Forms.Button btnSCan;
        private System.Windows.Forms.Button btnSSave;
        private System.Windows.Forms.CheckBox chkPreventDuplicates;
        private System.Windows.Forms.CheckBox chkSaveOriginalFileNames;
        private System.Windows.Forms.CheckBox chkDownloadThumbnails;
        private System.Windows.Forms.CheckBox chkDownloadHTML;
        private HintTextBox txtSavePath;
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
        private HintTextBox txtUserAgent;
        private System.Windows.Forms.Button btnOpenLocalFiles;
        private System.Windows.Forms.TabPage tabRegex;
        private System.Windows.Forms.Label lbRegexInfo;
        private HintTextBox txtRegex;
        private System.Windows.Forms.ListView lvRegex;
        private System.Windows.Forms.CheckBox chkAllowFileNamesGreaterThan255;
        private System.Windows.Forms.CheckBox chkMinimizeInsteadOfExiting;
        private System.Windows.Forms.Label lbRegexHint;
        private System.Windows.Forms.CheckBox chkResetRegexSettings;
        private System.Windows.Forms.CheckBox chkResetAdvancedSettings;
        private System.Windows.Forms.CheckBox chkResetApplicationSettings;
        private System.Windows.Forms.CheckBox chkResetDownloadSettings;
        private System.Windows.Forms.Button btnResetSettings;
        private System.Windows.Forms.CheckBox chkEnableSettingsReset;
        private System.Windows.Forms.Label lbScanDelaySeconds;
        private System.Windows.Forms.CheckBox chkRetrieveThreadName;
    }
}