namespace YChanEx;
using System.Windows.Forms;
public partial class frmSettings : Form {
    public frmSettings() {
        InitializeComponent();
        Program.SettingsOpen = true;
        LoadSettings();
    }

    private void btnSave_Click(object sender, EventArgs e) {
        SaveSettings();
        this.DialogResult = DialogResult.OK;
    }

    private void LoadSettings() {
        txtSavePath.Text = Downloads.DownloadPath;
        numTimer.Value = Downloads.ScannerDelay;
        chkSaveOriginalFileNames.Checked = Downloads.SaveOriginalFilenames;
        chkPreventDuplicates.Checked = Downloads.PreventDuplicates;
        chkAllowFileNamesGreaterThan255.Checked = Downloads.AllowFileNamesGreaterThan255;
        chkDownloadHTML.Checked = Downloads.SaveHTML;
        chkCleanThreadHTML.Checked = Downloads.CleanThreadHTML;
        chkDownloadThumbnails.Checked = Downloads.SaveThumbnails;
        chkAutoRemoveDeadThreads.Checked = Downloads.AutoRemoveDeadThreads;

        chkShowTrayIcon.Checked = General.ShowTrayIcon;
        chkMinimizeToTray.Checked = General.MinimizeToTray;
        chkMinimizeInsteadOfExiting.Checked = General.MinimizeInsteadOfExiting;
        chkShowExitWarning.Checked = General.ShowExitWarning;
        chkEnableUpdates.Checked = General.EnableUpdates;
        chkUseFullBoardNameForTitle.Checked = General.UseFullBoardNameForTitle;
        chkSaveDownloadQueueOnExit.Checked = General.SaveQueueOnExit;
        chkSaveDownloadHistory.Checked = General.SaveThreadHistory;

        txtUserAgent.Text = Advanced.UserAgent;
        chkDisableScannerWhenOpeningSettings.Checked = Advanced.DisableScanWhenOpeningSettings;
        chkSilenceErrors.Checked = Advanced.SilenceErrors;

        if (!SystemRegistry.CheckProtocolKey()) {
            if (!Program.IsAdmin) {
                btnProtocol.Text = " " + btnProtocol.Text;
                NativeMethods.SendMessage(btnProtocol.Handle, NativeMethods.BCM_SETSHIELD, IntPtr.Zero, (IntPtr)2);
            }
            btnProtocol.Visible = true;
        }
        else {
            btnProtocol.Visible = false;
            btnProtocol.Enabled = false;
        }

        chkUseProxy.Checked = Initialization.UseProxy;
        txtProxy.Text = Initialization.Proxy.GetReadableIp();
    }

    private void SaveSettings() {
        if (chkUseProxy.Checked && !txtProxy.Text.IsNullEmptyWhitespace()) {
            if (!ProxyData.TryParse(txtProxy.Text, out var Proxy)) {
                MessageBox.Show("Cannot parse proxy. Enter a valid input string or an empty string to not use a proxy.");
                return;
            }
            Initialization.Proxy = Proxy;
            frmDownloader.RecreateDownloadClient();
        }
        else {
            chkUseProxy.Checked = false;
        }
        Initialization.UseProxy = chkUseProxy.Checked;

        Downloads.DownloadPath = txtSavePath.Text;
        Downloads.ScannerDelay = (int)numTimer.Value;
        Downloads.SaveOriginalFilenames = chkSaveOriginalFileNames.Checked;
        Downloads.PreventDuplicates = chkPreventDuplicates.Checked;
        Downloads.AllowFileNamesGreaterThan255 = chkAllowFileNamesGreaterThan255.Checked;
        Downloads.SaveHTML = chkDownloadHTML.Checked;
        Downloads.CleanThreadHTML = chkCleanThreadHTML.Checked;
        Downloads.SaveThumbnails = chkDownloadThumbnails.Checked;
        Downloads.AutoRemoveDeadThreads = chkAutoRemoveDeadThreads.Checked;

        General.ShowTrayIcon = chkShowTrayIcon.Checked;
        General.MinimizeToTray = chkMinimizeToTray.Checked;
        General.MinimizeInsteadOfExiting = chkMinimizeInsteadOfExiting.Checked;
        General.ShowExitWarning = chkShowExitWarning.Checked;
        General.EnableUpdates = chkEnableUpdates.Checked;
        General.UseFullBoardNameForTitle = chkUseFullBoardNameForTitle.Checked;
        General.SaveQueueOnExit = chkSaveDownloadQueueOnExit.Checked;
        General.SaveThreadHistory = chkSaveDownloadHistory.Checked;

        Advanced.UserAgent = string.IsNullOrWhiteSpace(txtUserAgent.Text) ? Advanced.DefaultUserAgent : txtUserAgent.Text;
        Advanced.DisableScanWhenOpeningSettings = chkDisableScannerWhenOpeningSettings.Checked;
        Advanced.SilenceErrors = chkSilenceErrors.Checked;
    }

    private void btnBrowse_Click(object sender, EventArgs e) {
        using BetterFolderBrowserNS.BetterFolderBrowser fbd = new();
        fbd.Title = "Select a folder to save threads to...";
        fbd.InitialDirectory = Downloads.DownloadPath;
        if (fbd.ShowDialog() == DialogResult.OK) {
            if (chkMoveExistingDownloads.Checked) {
                if (!System.IO.Directory.Exists(fbd.SelectedPath)) {
                    System.IO.Directory.CreateDirectory(fbd.SelectedPath);
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\4chan")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\4chan", fbd.SelectedPath + "\\4chan");
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\420chan")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\420chan", fbd.SelectedPath + "\\420chan");
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\7chan")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\7chan", fbd.SelectedPath + "\\7chan");
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\8chan")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\8chan", fbd.SelectedPath + "\\8chan");
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\8kun")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\8kun", fbd.SelectedPath + "\\8kun");
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\fchan")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\fchan", fbd.SelectedPath + "\\fchan");
                }
                if (System.IO.Directory.Exists(txtSavePath.Text + "\\u18chan")) {
                    System.IO.Directory.Move(txtSavePath.Text + "\\u18chan", fbd.SelectedPath + "\\u18chan");
                }
            }
            txtSavePath.Text = fbd.SelectedPath;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e) {
        this.DialogResult = DialogResult.Cancel;
    }

    private void btnOpenLocalFiles_Click(object sender, EventArgs e) {
        System.Diagnostics.Process.Start(Environment.CurrentDirectory);
    }

    private void chkEnableSettingsReset_CheckedChanged(object sender, EventArgs e) {
        btnResetSettings.Enabled = chkEnableSettingsReset.Checked;
    }

    private void btnResetSettings_Click(object sender, EventArgs e) {
        if (chkResetDownloadSettings.Checked) {
            Downloads.Reset();
        }
        if (chkResetApplicationSettings.Checked) {
            General.Reset();
        }
        if (chkResetAdvancedSettings.Checked) {
            Advanced.Reset();
        }

        LoadSettings();
        chkEnableSettingsReset.Checked = false;
        btnResetSettings.Enabled = false;
    }

    private void btnUserScript_Click(object sender, EventArgs e) {
        System.Diagnostics.Process.Start("https://raw.githubusercontent.com/murrty/YChanEx/master/Resources/YChanEx.user.js");
    }

    private void btnProtocol_Click(object sender, EventArgs e) {
        if (MessageBox.Show($"Setting the protocol will allow webbrowsers to send URLs to this program using the \"ychanex:\" protocol. It's recommended to save this program in a static location. Do you want to point the protocol to \"{Program.FullApplicationPath}\"?", "YChanEx", MessageBoxButtons.YesNo) == DialogResult.Yes) {
            using System.Diagnostics.Process InstallProtocol = new() {
                StartInfo = new() {
                    Arguments = "--protocol",
                    FileName = Program.FullApplicationPath,
                    Verb = "runas",
                    WorkingDirectory = System.IO.Path.GetDirectoryName(Program.FullApplicationPath)
                }
            };
            InstallProtocol.Start();
        }
    }

    private void btnAddCookie_Click(object sender, EventArgs e) {
        using frmAddCookie co = new();
        if (co.ShowDialog() == DialogResult.OK) {
            ListViewItem NewCookie = new(co.Cookie.Name) {
                Tag = co.Cookie
            };
            NewCookie.SubItems.Add(co.Cookie.Value);
            NewCookie.SubItems.Add(co.Cookie.Path);
            NewCookie.SubItems.Add(co.Cookie.Domain);
            lvCookies.Items.Add(NewCookie);
            Cookies.AddCookie(co.Cookie);
        }
    }
}
