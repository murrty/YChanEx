namespace YChanEx {
    partial class Settings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.btnSSave = new System.Windows.Forms.Button();
            this.btnSCan = new System.Windows.Forms.Button();
            this.lbSavePath = new System.Windows.Forms.Label();
            this.lbTimer = new System.Windows.Forms.Label();
            this.edtPath = new System.Windows.Forms.TextBox();
            this.chkHTML = new System.Windows.Forms.CheckBox();
            this.chkSave = new System.Windows.Forms.CheckBox();
            this.chkMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.chkWarn = new System.Windows.Forms.CheckBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.edtTimer = new System.Windows.Forms.NumericUpDown();
            this.chkUpdates = new System.Windows.Forms.CheckBox();
            this.chkHistory = new System.Windows.Forms.CheckBox();
            this.lb404 = new System.Windows.Forms.Label();
            this.cb404 = new System.Windows.Forms.ComboBox();
            this.chkMove = new System.Windows.Forms.CheckBox();
            this.chkThumbnails = new System.Windows.Forms.CheckBox();
            this.ttInfo = new System.Windows.Forms.ToolTip(this.components);
            this.chkShowTray = new System.Windows.Forms.CheckBox();
            this.chkUpdateInfo = new System.Windows.Forms.CheckBox();
            this.chkOriginalNames = new System.Windows.Forms.CheckBox();
            this.chkSaveDate = new System.Windows.Forms.CheckBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.chkDisableScan = new System.Windows.Forms.CheckBox();
            this.txtUserAgent = new System.Windows.Forms.TextBox();
            this.chkLogErrors = new System.Windows.Forms.CheckBox();
            this.chkDisableErrors = new System.Windows.Forms.CheckBox();
            this.chkPreventDupes = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabDownloads = new System.Windows.Forms.TabPage();
            this.tabApplication = new System.Windows.Forms.TabPage();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.lbAdv = new System.Windows.Forms.Label();
            this.lbUserAgent = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.rbAdvanced = new System.Windows.Forms.RadioButton();
            this.rbRegular = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.edtTimer)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabDownloads.SuspendLayout();
            this.tabApplication.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSSave
            // 
            this.btnSSave.Location = new System.Drawing.Point(234, 182);
            this.btnSSave.Name = "btnSSave";
            this.btnSSave.Size = new System.Drawing.Size(75, 23);
            this.btnSSave.TabIndex = 14;
            this.btnSSave.Text = "Save";
            this.btnSSave.UseVisualStyleBackColor = true;
            this.btnSSave.Click += new System.EventHandler(this.btnSSave_Click);
            // 
            // btnSCan
            // 
            this.btnSCan.Location = new System.Drawing.Point(315, 182);
            this.btnSCan.Name = "btnSCan";
            this.btnSCan.Size = new System.Drawing.Size(75, 23);
            this.btnSCan.TabIndex = 15;
            this.btnSCan.Text = "Cancel";
            this.btnSCan.UseVisualStyleBackColor = true;
            this.btnSCan.Click += new System.EventHandler(this.btnSCan_Click);
            // 
            // lbSavePath
            // 
            this.lbSavePath.AutoSize = true;
            this.lbSavePath.Location = new System.Drawing.Point(6, 14);
            this.lbSavePath.Name = "lbSavePath";
            this.lbSavePath.Size = new System.Drawing.Size(57, 13);
            this.lbSavePath.TabIndex = 16;
            this.lbSavePath.Text = "Save &Path\r\n";
            // 
            // lbTimer
            // 
            this.lbTimer.AutoSize = true;
            this.lbTimer.Location = new System.Drawing.Point(6, 38);
            this.lbTimer.Name = "lbTimer";
            this.lbTimer.Size = new System.Drawing.Size(59, 13);
            this.lbTimer.TabIndex = 17;
            this.lbTimer.Text = "Timer (sec)";
            // 
            // edtPath
            // 
            this.edtPath.Location = new System.Drawing.Point(71, 11);
            this.edtPath.Name = "edtPath";
            this.edtPath.ReadOnly = true;
            this.edtPath.Size = new System.Drawing.Size(194, 20);
            this.edtPath.TabIndex = 0;
            this.edtPath.Text = "C:\\";
            this.ttInfo.SetToolTip(this.edtPath, "The location where threads will be downloaded.\r\n(<Directory>/4chan/<Board>/<Threa" +
        "d>)");
            // 
            // chkHTML
            // 
            this.chkHTML.AutoSize = true;
            this.chkHTML.Location = new System.Drawing.Point(9, 63);
            this.chkHTML.Name = "chkHTML";
            this.chkHTML.Size = new System.Drawing.Size(106, 17);
            this.chkHTML.TabIndex = 6;
            this.chkHTML.Text = "Download HTML";
            this.ttInfo.SetToolTip(this.chkHTML, "Downloads thread HTML when downloading a thread to save conversations or... other" +
        " creepy things.");
            this.chkHTML.UseVisualStyleBackColor = true;
            this.chkHTML.CheckedChanged += new System.EventHandler(this.chkHTML_CheckedChanged);
            // 
            // chkSave
            // 
            this.chkSave.AutoSize = true;
            this.chkSave.Location = new System.Drawing.Point(9, 84);
            this.chkSave.Name = "chkSave";
            this.chkSave.Size = new System.Drawing.Size(166, 17);
            this.chkSave.TabIndex = 5;
            this.chkSave.Text = "Save download queue on exit";
            this.ttInfo.SetToolTip(this.chkSave, "Save URLs in the queue when closing to restart downloading when opening the progr" +
        "am again.");
            this.chkSave.UseVisualStyleBackColor = true;
            // 
            // chkMinimizeToTray
            // 
            this.chkMinimizeToTray.AutoSize = true;
            this.chkMinimizeToTray.Location = new System.Drawing.Point(141, 55);
            this.chkMinimizeToTray.Name = "chkMinimizeToTray";
            this.chkMinimizeToTray.Size = new System.Drawing.Size(101, 17);
            this.chkMinimizeToTray.TabIndex = 7;
            this.chkMinimizeToTray.Text = "Minimize to Tray";
            this.ttInfo.SetToolTip(this.chkMinimizeToTray, "Minimizing the program will hide it to the tray.");
            this.chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // chkWarn
            // 
            this.chkWarn.AutoSize = true;
            this.chkWarn.Location = new System.Drawing.Point(248, 55);
            this.chkWarn.Name = "chkWarn";
            this.chkWarn.Size = new System.Drawing.Size(115, 17);
            this.chkWarn.TabIndex = 8;
            this.chkWarn.Text = "Show Exit Warning";
            this.ttInfo.SetToolTip(this.chkWarn, "Shows an exit warning before closing if there are any threads being downloaded.");
            this.chkWarn.UseVisualStyleBackColor = true;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(271, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(24, 20);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // edtTimer
            // 
            this.edtTimer.Location = new System.Drawing.Point(71, 36);
            this.edtTimer.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.edtTimer.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.edtTimer.Name = "edtTimer";
            this.edtTimer.Size = new System.Drawing.Size(56, 20);
            this.edtTimer.TabIndex = 3;
            this.edtTimer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ttInfo.SetToolTip(this.edtTimer, "Time (in seconds) to rescan threads/boards to download.");
            this.edtTimer.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // chkUpdates
            // 
            this.chkUpdates.AutoSize = true;
            this.chkUpdates.Location = new System.Drawing.Point(18, 76);
            this.chkUpdates.Name = "chkUpdates";
            this.chkUpdates.Size = new System.Drawing.Size(101, 17);
            this.chkUpdates.TabIndex = 10;
            this.chkUpdates.Text = "Enable Updates";
            this.ttInfo.SetToolTip(this.chkUpdates, "Enables the program to check for any updates.");
            this.chkUpdates.UseVisualStyleBackColor = true;
            // 
            // chkHistory
            // 
            this.chkHistory.AutoSize = true;
            this.chkHistory.Location = new System.Drawing.Point(257, 63);
            this.chkHistory.Name = "chkHistory";
            this.chkHistory.Size = new System.Drawing.Size(116, 17);
            this.chkHistory.TabIndex = 11;
            this.chkHistory.Text = "Save thread history";
            this.ttInfo.SetToolTip(this.chkHistory, "Saves your thread history you\'ve downloaded.");
            this.chkHistory.UseVisualStyleBackColor = true;
            this.chkHistory.CheckedChanged += new System.EventHandler(this.chkHistory_CheckedChanged);
            // 
            // lb404
            // 
            this.lb404.AutoSize = true;
            this.lb404.Location = new System.Drawing.Point(6, 105);
            this.lb404.Name = "lb404";
            this.lb404.Size = new System.Drawing.Size(180, 13);
            this.lb404.TabIndex = 18;
            this.lb404.Text = "Action when clicking 404 notification";
            // 
            // cb404
            // 
            this.cb404.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb404.FormattingEnabled = true;
            this.cb404.Items.AddRange(new object[] {
            "Do nothing",
            "Copy archive link",
            "Open archive link",
            "Copy original link",
            "Open original link",
            "Copy thread ID",
            "Copy download folder path",
            "Open download folder"});
            this.cb404.Location = new System.Drawing.Point(9, 122);
            this.cb404.Name = "cb404";
            this.cb404.Size = new System.Drawing.Size(166, 21);
            this.cb404.TabIndex = 13;
            this.ttInfo.SetToolTip(this.cb404, "When a thread 404s, a balloon tip will show up. Select an option below to perform" +
        " an action when clicking on the balloon tip.");
            // 
            // chkMove
            // 
            this.chkMove.AutoSize = true;
            this.chkMove.Checked = true;
            this.chkMove.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMove.Location = new System.Drawing.Point(297, 6);
            this.chkMove.Name = "chkMove";
            this.chkMove.Size = new System.Drawing.Size(93, 30);
            this.chkMove.TabIndex = 2;
            this.chkMove.Text = "Move existing \r\ndownloads";
            this.ttInfo.SetToolTip(this.chkMove, "Move the existing downloaded threads to the new directory (if checked)");
            this.chkMove.UseVisualStyleBackColor = true;
            // 
            // chkThumbnails
            // 
            this.chkThumbnails.AutoSize = true;
            this.chkThumbnails.Location = new System.Drawing.Point(121, 63);
            this.chkThumbnails.Name = "chkThumbnails";
            this.chkThumbnails.Size = new System.Drawing.Size(130, 17);
            this.chkThumbnails.TabIndex = 9;
            this.chkThumbnails.Text = "Download Thumbnails";
            this.ttInfo.SetToolTip(this.chkThumbnails, resources.GetString("chkThumbnails.ToolTip"));
            this.chkThumbnails.UseVisualStyleBackColor = true;
            // 
            // ttInfo
            // 
            this.ttInfo.AutomaticDelay = 250;
            this.ttInfo.AutoPopDelay = 30000;
            this.ttInfo.InitialDelay = 250;
            this.ttInfo.ReshowDelay = 50;
            this.ttInfo.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.ttInfo.UseAnimation = false;
            this.ttInfo.UseFading = false;
            // 
            // chkShowTray
            // 
            this.chkShowTray.AutoSize = true;
            this.chkShowTray.Location = new System.Drawing.Point(34, 55);
            this.chkShowTray.Name = "chkShowTray";
            this.chkShowTray.Size = new System.Drawing.Size(95, 17);
            this.chkShowTray.TabIndex = 4;
            this.chkShowTray.Text = "Show tray icon";
            this.ttInfo.SetToolTip(this.chkShowTray, "Show the program in the system tray.");
            this.chkShowTray.UseVisualStyleBackColor = true;
            this.chkShowTray.CheckedChanged += new System.EventHandler(this.chkShowTray_CheckedChanged);
            // 
            // chkUpdateInfo
            // 
            this.chkUpdateInfo.AutoSize = true;
            this.chkUpdateInfo.Enabled = false;
            this.chkUpdateInfo.Location = new System.Drawing.Point(125, 76);
            this.chkUpdateInfo.Name = "chkUpdateInfo";
            this.chkUpdateInfo.Size = new System.Drawing.Size(142, 17);
            this.chkUpdateInfo.TabIndex = 11;
            this.chkUpdateInfo.Text = "Show update information";
            this.ttInfo.SetToolTip(this.chkUpdateInfo, "When an update is available (and updating is enabled), checking this option will " +
        "pop up a window with information about the update.\r\n(Currently limited to critic" +
        "al updates only)");
            this.chkUpdateInfo.UseVisualStyleBackColor = true;
            this.chkUpdateInfo.Visible = false;
            // 
            // chkOriginalNames
            // 
            this.chkOriginalNames.AutoSize = true;
            this.chkOriginalNames.Location = new System.Drawing.Point(257, 105);
            this.chkOriginalNames.Name = "chkOriginalNames";
            this.chkOriginalNames.Size = new System.Drawing.Size(136, 17);
            this.chkOriginalNames.TabIndex = 19;
            this.chkOriginalNames.Text = "Save original file names";
            this.ttInfo.SetToolTip(this.chkOriginalNames, "Saves the files with the original file names. (4chan only, for now)\r\n\r\nDownloaded" +
        " HTML will be broken unless you replace the URLs in the HTMLs.");
            this.chkOriginalNames.UseVisualStyleBackColor = true;
            // 
            // chkSaveDate
            // 
            this.chkSaveDate.AutoSize = true;
            this.chkSaveDate.Checked = true;
            this.chkSaveDate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSaveDate.Enabled = false;
            this.chkSaveDate.Location = new System.Drawing.Point(257, 84);
            this.chkSaveDate.Name = "chkSaveDate";
            this.chkSaveDate.Size = new System.Drawing.Size(129, 17);
            this.chkSaveDate.TabIndex = 10;
            this.chkSaveDate.Text = "Save date with history";
            this.ttInfo.SetToolTip(this.chkSaveDate, "Save the date next to the history entry of the downloaded thread.");
            this.chkSaveDate.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(150, 97);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(96, 23);
            this.btnReset.TabIndex = 12;
            this.btnReset.Text = "Reset to Default";
            this.ttInfo.SetToolTip(this.btnReset, "I\'m a goofus.");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // chkDisableScan
            // 
            this.chkDisableScan.AutoSize = true;
            this.chkDisableScan.Checked = true;
            this.chkDisableScan.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDisableScan.Location = new System.Drawing.Point(46, 83);
            this.chkDisableScan.Name = "chkDisableScan";
            this.chkDisableScan.Size = new System.Drawing.Size(210, 17);
            this.chkDisableScan.TabIndex = 9;
            this.chkDisableScan.Text = "Disable scanner when opening settings";
            this.ttInfo.SetToolTip(this.chkDisableScan, "Disables the sacnner when settings is opened.");
            this.chkDisableScan.UseVisualStyleBackColor = true;
            // 
            // txtUserAgent
            // 
            this.txtUserAgent.Location = new System.Drawing.Point(82, 51);
            this.txtUserAgent.Name = "txtUserAgent";
            this.txtUserAgent.Size = new System.Drawing.Size(297, 20);
            this.txtUserAgent.TabIndex = 8;
            this.txtUserAgent.Text = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:55.0) Gecko/20100101 Firefox/55.0";
            this.ttInfo.SetToolTip(this.txtUserAgent, "The User-Agent used when downloading.");
            // 
            // chkLogErrors
            // 
            this.chkLogErrors.AutoSize = true;
            this.chkLogErrors.Checked = true;
            this.chkLogErrors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogErrors.Location = new System.Drawing.Point(273, 76);
            this.chkLogErrors.Name = "chkLogErrors";
            this.chkLogErrors.Size = new System.Drawing.Size(105, 17);
            this.chkLogErrors.TabIndex = 12;
            this.chkLogErrors.Text = "Log errors to files";
            this.ttInfo.SetToolTip(this.chkLogErrors, "When an error occures, log files will be created pertaining to the activity it wa" +
        "s attempting to do.\r\nGood for reporting errors.");
            this.chkLogErrors.UseVisualStyleBackColor = true;
            // 
            // chkDisableErrors
            // 
            this.chkDisableErrors.AutoSize = true;
            this.chkDisableErrors.Location = new System.Drawing.Point(262, 83);
            this.chkDisableErrors.Name = "chkDisableErrors";
            this.chkDisableErrors.Size = new System.Drawing.Size(89, 17);
            this.chkDisableErrors.TabIndex = 13;
            this.chkDisableErrors.Text = "Silence errors";
            this.ttInfo.SetToolTip(this.chkDisableErrors, "This will silence all errors and logging.");
            this.chkDisableErrors.UseVisualStyleBackColor = true;
            this.chkDisableErrors.CheckedChanged += new System.EventHandler(this.chkDisableErrors_CheckedChanged);
            // 
            // chkPreventDupes
            // 
            this.chkPreventDupes.AutoSize = true;
            this.chkPreventDupes.Location = new System.Drawing.Point(257, 126);
            this.chkPreventDupes.Name = "chkPreventDupes";
            this.chkPreventDupes.Size = new System.Drawing.Size(113, 17);
            this.chkPreventDupes.TabIndex = 20;
            this.chkPreventDupes.Text = "Prevent duplicates";
            this.ttInfo.SetToolTip(this.chkPreventDupes, "Prevent duplicate file names by appending the MD5 of the file to the end of the f" +
        "ile name.\r\nThis is a niche problem, so it may make original file names end with " +
        "text that isn\'t required.");
            this.chkPreventDupes.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabDownloads);
            this.tabControl1.Controls.Add(this.tabApplication);
            this.tabControl1.Controls.Add(this.tabAdvanced);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(404, 177);
            this.tabControl1.TabIndex = 19;
            // 
            // tabDownloads
            // 
            this.tabDownloads.Controls.Add(this.chkPreventDupes);
            this.tabDownloads.Controls.Add(this.chkOriginalNames);
            this.tabDownloads.Controls.Add(this.lbSavePath);
            this.tabDownloads.Controls.Add(this.chkSaveDate);
            this.tabDownloads.Controls.Add(this.edtPath);
            this.tabDownloads.Controls.Add(this.btnBrowse);
            this.tabDownloads.Controls.Add(this.chkThumbnails);
            this.tabDownloads.Controls.Add(this.edtTimer);
            this.tabDownloads.Controls.Add(this.chkHistory);
            this.tabDownloads.Controls.Add(this.cb404);
            this.tabDownloads.Controls.Add(this.chkMove);
            this.tabDownloads.Controls.Add(this.chkSave);
            this.tabDownloads.Controls.Add(this.lb404);
            this.tabDownloads.Controls.Add(this.lbTimer);
            this.tabDownloads.Controls.Add(this.chkHTML);
            this.tabDownloads.Location = new System.Drawing.Point(4, 22);
            this.tabDownloads.Name = "tabDownloads";
            this.tabDownloads.Padding = new System.Windows.Forms.Padding(3);
            this.tabDownloads.Size = new System.Drawing.Size(396, 151);
            this.tabDownloads.TabIndex = 0;
            this.tabDownloads.Text = "Downloads";
            this.tabDownloads.UseVisualStyleBackColor = true;
            // 
            // tabApplication
            // 
            this.tabApplication.Controls.Add(this.chkLogErrors);
            this.tabApplication.Controls.Add(this.chkUpdateInfo);
            this.tabApplication.Controls.Add(this.chkShowTray);
            this.tabApplication.Controls.Add(this.chkMinimizeToTray);
            this.tabApplication.Controls.Add(this.chkUpdates);
            this.tabApplication.Controls.Add(this.chkWarn);
            this.tabApplication.Location = new System.Drawing.Point(4, 22);
            this.tabApplication.Name = "tabApplication";
            this.tabApplication.Padding = new System.Windows.Forms.Padding(3);
            this.tabApplication.Size = new System.Drawing.Size(396, 151);
            this.tabApplication.TabIndex = 1;
            this.tabApplication.Text = "Application";
            this.tabApplication.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.chkDisableErrors);
            this.tabAdvanced.Controls.Add(this.lbAdv);
            this.tabAdvanced.Controls.Add(this.chkDisableScan);
            this.tabAdvanced.Controls.Add(this.txtUserAgent);
            this.tabAdvanced.Controls.Add(this.lbUserAgent);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvanced.Size = new System.Drawing.Size(396, 151);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // lbAdv
            // 
            this.lbAdv.AutoSize = true;
            this.lbAdv.Location = new System.Drawing.Point(77, 5);
            this.lbAdv.Name = "lbAdv";
            this.lbAdv.Size = new System.Drawing.Size(243, 13);
            this.lbAdv.TabIndex = 11;
            this.lbAdv.Text = "By advanced, I really mean advanced. Be careful.";
            // 
            // lbUserAgent
            // 
            this.lbUserAgent.AutoSize = true;
            this.lbUserAgent.Location = new System.Drawing.Point(17, 54);
            this.lbUserAgent.Name = "lbUserAgent";
            this.lbUserAgent.Size = new System.Drawing.Size(63, 13);
            this.lbUserAgent.TabIndex = 7;
            this.lbUserAgent.Text = "User-Agent:";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rbAll);
            this.tabPage1.Controls.Add(this.rbAdvanced);
            this.tabPage1.Controls.Add(this.rbRegular);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.btnReset);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(396, 151);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "Reset";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rbAll
            // 
            this.rbAll.AutoSize = true;
            this.rbAll.Location = new System.Drawing.Point(273, 70);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(74, 17);
            this.rbAll.TabIndex = 16;
            this.rbAll.TabStop = true;
            this.rbAll.Text = "All settings";
            this.rbAll.UseVisualStyleBackColor = true;
            // 
            // rbAdvanced
            // 
            this.rbAdvanced.AutoSize = true;
            this.rbAdvanced.Checked = true;
            this.rbAdvanced.Location = new System.Drawing.Point(155, 70);
            this.rbAdvanced.Name = "rbAdvanced";
            this.rbAdvanced.Size = new System.Drawing.Size(112, 17);
            this.rbAdvanced.TabIndex = 15;
            this.rbAdvanced.TabStop = true;
            this.rbAdvanced.Text = "Advanced settings";
            this.rbAdvanced.UseVisualStyleBackColor = true;
            // 
            // rbRegular
            // 
            this.rbRegular.AutoSize = true;
            this.rbRegular.Location = new System.Drawing.Point(49, 69);
            this.rbRegular.Name = "rbRegular";
            this.rbRegular.Size = new System.Drawing.Size(100, 17);
            this.rbRegular.TabIndex = 14;
            this.rbRegular.TabStop = true;
            this.rbRegular.Text = "Regular settings";
            this.rbRegular.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(85, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(226, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "This will reset the settings you choose to reset!";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(402, 212);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnSCan);
            this.Controls.Add(this.btnSSave);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(410, 242);
            this.MinimumSize = new System.Drawing.Size(410, 242);
            this.Name = "Settings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YChanEx Settings";
            this.Shown += new System.EventHandler(this.Settings_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.edtTimer)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabDownloads.ResumeLayout(false);
            this.tabDownloads.PerformLayout();
            this.tabApplication.ResumeLayout(false);
            this.tabApplication.PerformLayout();
            this.tabAdvanced.ResumeLayout(false);
            this.tabAdvanced.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSSave;
        private System.Windows.Forms.Button btnSCan;
        private System.Windows.Forms.Label lbSavePath;
        private System.Windows.Forms.Label lbTimer;
        private System.Windows.Forms.TextBox edtPath;
        private System.Windows.Forms.CheckBox chkHTML;
        private System.Windows.Forms.CheckBox chkSave;
        private System.Windows.Forms.CheckBox chkMinimizeToTray;
        private System.Windows.Forms.CheckBox chkWarn;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.NumericUpDown edtTimer;
        private System.Windows.Forms.CheckBox chkUpdates;
        private System.Windows.Forms.CheckBox chkHistory;
        private System.Windows.Forms.Label lb404;
        private System.Windows.Forms.ComboBox cb404;
        private System.Windows.Forms.CheckBox chkMove;
        private System.Windows.Forms.CheckBox chkThumbnails;
        private System.Windows.Forms.ToolTip ttInfo;
        private System.Windows.Forms.CheckBox chkShowTray;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabDownloads;
        private System.Windows.Forms.TabPage tabApplication;
        private System.Windows.Forms.CheckBox chkUpdateInfo;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.CheckBox chkSaveDate;
        private System.Windows.Forms.CheckBox chkDisableScan;
        private System.Windows.Forms.TextBox txtUserAgent;
        private System.Windows.Forms.Label lbUserAgent;
        private System.Windows.Forms.Label lbAdv;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.CheckBox chkOriginalNames;
        private System.Windows.Forms.CheckBox chkLogErrors;
        private System.Windows.Forms.CheckBox chkDisableErrors;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.RadioButton rbAdvanced;
        private System.Windows.Forms.RadioButton rbRegular;
        private System.Windows.Forms.CheckBox chkPreventDupes;
    }
}