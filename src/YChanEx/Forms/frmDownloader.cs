#nullable enable
namespace YChanEx;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using murrty.classes;
using murrty.controls;
using YChanEx.Parsers;
using YChanEx.Posts;

/// <summary>
/// The form that thread downloads will be relegated to.
/// </summary>
public partial class frmDownloader : Form {
    private delegate Task DownloadStream(Stream HttpStream, Stream Output, CancellationToken token);
    private const int DefaultBuffer = 81920;

    private static HttpClient DownloadClient;
    private static HttpMessageHandler DownloadClientHandler;
    private static CookieContainer CookieContainer;
    private static readonly DownloadStream StreamCallback;
    private static readonly bool Throttle;
    private static readonly int ThrottleSize;
    private static readonly int ThrottleBufferSize;
    internal static ImageList DownloadImages = new();
    const int WaitingImage = 0;
    const int DownloadingImage = 1;
    const int FinishedImage = 2;
    const int ErrorImage = 3;
    const int _404Image = 4;
    const int ReloadedDownloadedImage = 5;
    const int ReloadedMissingImage = 6;
    const int RemovedFromThreadImage = 7;

    /// <summary>
    /// The IMainFormInterface to interface with the main form,
    /// used for updating the main form with this forms' threads' status, or name.
    /// </summary>
    private readonly IMainFom MainFormInstance;

    /// <summary>
    /// The ThreadInfo containing all information about this forms' thread.
    /// </summary>
    public ThreadInfo ThreadInfo { get; init; }

    /// <summary>
    /// The thread for the thread parser/downloader
    /// </summary>
    private Thread? DownloadThread;
    /// <summary>
    /// The thread that's used to delay the rescanner.
    /// </summary>
    private Thread? TimerIdle;
    /// <summary>
    /// The reste vent that is used to block the thread during idle.
    /// </summary>
    private readonly ManualResetEventSlim ResetThread;

    // Mostly-debug
    /// <summary>
    /// Pauses the file downloader 100ms between each file,
    /// used to prevent sending too many requests.
    /// </summary>
    private const bool PauseBetweenFiles = true;
    /// <summary>
    /// The cancellation token that will be used to kill the main loop.
    /// </summary>
    private CancellationTokenSource CancellationToken;

    [MemberNotNull(nameof(DownloadClient), nameof(DownloadClientHandler), nameof(CookieContainer))]
    public static void RecreateDownloadClient() {
        if (CookieContainer == null) {
            CookieContainer = new CookieContainer();
            // Required cookies.
            CookieContainer.Add(new Cookie("disclaimer", "seen", "/", "fchan.us"));
            for (int i = 0; i < 20; i++) {
                CookieContainer.Add(new Cookie("disclaimer" + i, "1", "/", "8chan.moe"));
            }

            if (Cookies.CookieList?.Count > 0) {
                foreach (var Cookie in Cookies.CookieList) {
                    CookieContainer.Add(Cookie);
                }
            }
        }

        // 100 limit
        ServicePointManager.DefaultConnectionLimit = 100;

        // Try to load TLS 1.3 (Win 11+)
        // It will use 1.2 if 1.3 is not available, regardless of if the first try works.
        //Tls12OrHigher = true;
        try {
#if NETCOREAPP
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13
#else
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)12288
#endif
                                                 | SecurityProtocolType.Tls12 // 3072
                                                 | SecurityProtocolType.Tls11 // 768
                                                 | SecurityProtocolType.Tls; // 192
        }
        catch (NotSupportedException) {
            try {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 // 3072
                                                     | SecurityProtocolType.Tls11 // 768
                                                     | SecurityProtocolType.Tls; // 192
            }
            catch (NotSupportedException) {
                //Tls12OrHigher = false;
                try {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 // 768
                                                         | SecurityProtocolType.Tls; // 192
                }
                catch (NotSupportedException) {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls; // 192
                }
            }
        }

        DownloadClientHandler = ProxyHandler.GetProxyHandler(Initialization.Proxy, 60_000, CookieContainer);
        DownloadClient = new(DownloadClientHandler);

        DownloadClient.DefaultRequestHeaders.Accept.Add(new("*/*"));
        //DownloadClient.DefaultRequestHeaders.AcceptEncoding.Add(new("br"));
        DownloadClient.DefaultRequestHeaders.AcceptEncoding.Add(new("gzip"));
        DownloadClient.DefaultRequestHeaders.AcceptEncoding.Add(new("deflate"));
        DownloadClient.DefaultRequestHeaders.AcceptLanguage.Add(new("*"));
        DownloadClient.DefaultRequestHeaders.ConnectionClose = false;
        DownloadClient.DefaultRequestHeaders.UserAgent.ParseAdd(Advanced.UserAgent);
    }

    static frmDownloader() {
        DownloadImages.ColorDepth = ColorDepth.Depth32Bit;
        DownloadImages.Images.Add(Properties.Resources.waiting);            // 0
        DownloadImages.Images.Add(Properties.Resources.downloading);        // 1
        DownloadImages.Images.Add(Properties.Resources.finished);           // 2
        DownloadImages.Images.Add(Properties.Resources.error);              // 3
        DownloadImages.Images.Add(Properties.Resources._404);               // 4
        DownloadImages.Images.Add(Properties.Resources.reloaded_downloaded);// 5
        DownloadImages.Images.Add(Properties.Resources.reloaded_missing);   // 6
        DownloadImages.Images.Add(Properties.Resources.removed_from_thread);// 7
        RecreateDownloadClient();

        Throttle = false;
        ThrottleSize = Initialization.ThrottleSize;
        ThrottleBufferSize = Math.Min(ThrottleSize, DefaultBuffer);
        StreamCallback = Throttle ? ThrottledWriteToStreamAsync : WriteToStreamAsync;
    }
    public frmDownloader(IMainFom MainForm, ThreadInfo ThreadInfo) {
        InitializeComponent();
        MainFormInstance = MainForm;
        this.ThreadInfo = ThreadInfo;

        Debug.Print("Created download form");

        if (Config.ValidPoint(Saved.DownloadFormLocation)) {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = Saved.DownloadFormLocation;
        }
        if (Config.ValidSize(Saved.DownloadFormSize)) {
            this.Size = Saved.DownloadFormSize;
        }

        if (Program.DebugMode) {
            btnForce404.Enabled = true;
            btnForce404.Visible = true;
            btnPauseTimer.Enabled = true;
            btnPauseTimer.Visible = true;
        }
        lvImages.SmallImageList = DownloadImages;
        ResetThread = new();
        CancellationToken = new();
    }

    private void frmDownloader_FormClosing(object sender, FormClosingEventArgs e) {
        Saved.DownloadFormLocation = this.Location;
        Saved.DownloadFormSize = this.Size;
        e.Cancel = true;
        this.Hide();
    }
    private void tmrScan_Tick(object sender, EventArgs e) {
        if (Program.SettingsOpen) {
            TimerIdle = new Thread(() => {
                try {
                    while (Program.SettingsOpen) {
                        Thread.Sleep(1000);
                    }
                    if (this.IsHandleCreated) {
                        this.Invoke(() => tmrScan.Start());
                    }
                }
                catch (ThreadAbortException) { }
            }) {
                Name = "Idling timer for Settings"
            };
            TimerIdle.Start();
            tmrScan.Stop();
            return;
        }

        switch (ThreadInfo.CountdownToNextScan) {
            case 0: {
                tmrScan.Stop();
                btnPauseTimer.Enabled = false;
                ManageThread(ThreadEvent.RestartDownload);
            } break;

            case 15: {
                ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanningSoon;
                MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                btnPauseTimer.Enabled = true;
                lbScanTimer.Text = ThreadInfo.CountdownToNextScan.ToString();
                ThreadInfo.CountdownToNextScan--;
            } break;

            default: {
                if (ThreadInfo.CountdownToNextScan == ThreadInfo.HideModifiedLabelAt) {
                    lbNotModified.Visible = false;
                }
                btnPauseTimer.Enabled = true;
                lbScanTimer.Text = ThreadInfo.CountdownToNextScan.ToString();
                ThreadInfo.CountdownToNextScan--;
            } break;
        }
    }
    private void lvImages_MouseDoubleClick(object sender, MouseEventArgs e) {
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            if (File.Exists(ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile)) {
                Process.Start(ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile);
            }
        }
    }
    private void lvImages_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar == (char)Keys.Return) {
            for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
                if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                    continue;
                }
                if (File.Exists(ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile)) {
                    Process.Start(ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile);
                }
            }
            e.Handled = true;
        }
    }
    private void btnForce404_Click(object sender, EventArgs e) {
        if (Program.DebugMode) {
            tmrScan.Stop();
            if (DownloadThread?.IsAlive == true) {
                DownloadThread.Abort();
            }

            ThreadInfo.CurrentActivity = ThreadStatus.ThreadIs404;
            btnForce404.Enabled = false;
            ManageThread(ThreadEvent.AfterDownload);
        }
    }
    private void btnAbortRetry_Click(object sender, EventArgs e) {
        switch (ThreadInfo.CurrentActivity) {
            case ThreadStatus.ThreadIs404:
            case ThreadStatus.ThreadIsAborted:
            case ThreadStatus.ThreadIsArchived:
                ManageThread(ThreadEvent.RetryDownload);
                break;
            default:
                ManageThread(ThreadEvent.AbortDownload);
                break;
        }
    }
    private void btnOpenFolder_Click(object sender, EventArgs e) {
        if (ThreadInfo.Data.DownloadPath == null) {
            return;
        }

        if (Directory.Exists(ThreadInfo.Data.DownloadPath)) {
            Process.Start(ThreadInfo.Data.DownloadPath);
        }
    }
    private void btnClose_Click(object sender, EventArgs e) {
        this.Hide();
    }
    private void mOpenThreadDownloadFolder_Click(object sender, EventArgs e) {
        if (Directory.Exists(ThreadInfo.Data.DownloadPath)) {
            Process.Start(ThreadInfo.Data.DownloadPath);
        }
    }
    private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
        if (ThreadInfo.Data.Url != null) {
            Process.Start(ThreadInfo.Data.Url);
        }
    }
    private void mCopyThreadID_Click(object sender, EventArgs e) {
        if (ThreadInfo.Data.Id != null) {
            Clipboard.SetText(ThreadInfo.Data.Id);
        }
    }
    private void mCopyThreadURL_Click(object sender, EventArgs e) {
        if (ThreadInfo.Data.Url != null) {
            Clipboard.SetText(ThreadInfo.Data.Url);
        }
    }
    private void mCopyThreadApiUrl_Click(object sender, EventArgs e) {
        if (ThreadInfo.Data.Url != null) {
            switch (ThreadInfo.Chan) {
                case ChanType.FourChan:
                case ChanType.FourTwentyChan:
                case ChanType.EightChan:
                case ChanType.EightKun:
                    Clipboard.SetText(Networking.GetAPILink(ThreadInfo));
                    break;

                case ChanType.SevenChan:
                case ChanType.fchan:
                case ChanType.u18chan:
                    Clipboard.SetText(ThreadInfo.Data.Url);
                    break;
            }
        }
    }
    private void mOpenImages_Click(object sender, EventArgs e) {
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            if (File.Exists(ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile)) {
                Process.Start(ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile);
            }
        }
    }
    private void mRemoveImages_Click(object sender, EventArgs e) {
        RemoveFileFromSystem();
    }
    private void mRemoveImagesFromSystem_Click(object sender, EventArgs e) {
        RemoveFileFromSystem();
    }
    private void mRemoveImagesFromThread_Click(object sender, EventArgs e) {
        RemoveFileFromThread();
    }
    private void mRemoveImagesFromBoth_Click(object sender, EventArgs e) {
        RemoveFileFromSystem();
        RemoveFileFromThread();
    }
    private void mCopyPostIDs_Click(object sender, EventArgs e) {
        StringBuilder ClipboardBuffer = new();
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            ClipboardBuffer.AppendLine(PostFile.Parent.PostId.ToString());
        }

        if (ClipboardBuffer.Length == 0) {
            Clipboard.SetText(string.Empty);
        }
        Clipboard.SetText(ClipboardBuffer.ToString());
    }
    private void mCopyImageIDNames_Click(object sender, EventArgs e) {
        StringBuilder ClipboardBuffer = new();
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            ClipboardBuffer.AppendLine(PostFile.FileId);
        }

        if (ClipboardBuffer.Length == 0) {
            Clipboard.SetText(string.Empty);
        }
        Clipboard.SetText(ClipboardBuffer.ToString());
    }
    private void mCopyOriginalFileNames_Click(object sender, EventArgs e) {
        StringBuilder ClipboardBuffer = new();
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            ClipboardBuffer.AppendLine(PostFile.OriginalFileName);
        }

        if (ClipboardBuffer.Length == 0) {
            Clipboard.SetText(string.Empty);
        }
        Clipboard.SetText(ClipboardBuffer.ToString());
    }
    private void mCopyDupeCheckedOriginalFileNames_Click(object sender, EventArgs e) {
        StringBuilder ClipboardBuffer = new();
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            ClipboardBuffer.AppendLine(PostFile.SavedFileName);
        }

        if (ClipboardBuffer.Length == 0) {
            Clipboard.SetText(string.Empty);
        }
        Clipboard.SetText(ClipboardBuffer.ToString());
    }
    private void mCopyFileHashes_Click(object sender, EventArgs e) {
        StringBuilder ClipboardBuffer = new();
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile || PostFile.FileHash == null) {
                continue;
            }
            ClipboardBuffer.AppendLine(PostFile.FileHash);
        }

        if (ClipboardBuffer.Length == 0) {
            Clipboard.SetText(string.Empty);
        }
        Clipboard.SetText(ClipboardBuffer.ToString());
    }
    private void mShowInExplorer_Click(object sender, EventArgs e) {
        for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
            if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                continue;
            }
            Process.Start("explorer.exe", "/select, \"" + ThreadInfo.Data.DownloadPath + "\\" + PostFile.SavedFile + "\"");
        }
    }
    private void cmPosts_Popup(object sender, EventArgs e) {
        if (lvImages.SelectedItems.Count > 0) {
            mOpenImages.Enabled = true;
            mRemoveImages.Enabled = true;
            //mRemoveImagesFromSystem.Enabled = true;
            //mRemoveImagesFromThread.Enabled = true;
            //mRemoveImagesFromBoth.Enabled = true;
            mCopyPostIDs.Enabled = true;
            mCopyImageIDNames.Enabled = true;
            mCopyOriginalFileNames.Enabled = true;
            mCopyDupeCheckedOriginalFileNames.Enabled = true;
            mCopyFileHashes.Enabled = true;
            mShowInExplorer.Enabled = true;
        }
        else {
            mOpenImages.Enabled = false;
            mRemoveImages.Enabled = false;
            //mRemoveImagesFromSystem.Enabled = false;
            //mRemoveImagesFromThread.Enabled = false;
            //mRemoveImagesFromBoth.Enabled = false;
            mCopyPostIDs.Enabled = false;
            mCopyImageIDNames.Enabled = false;
            mCopyOriginalFileNames.Enabled = false;
            mCopyDupeCheckedOriginalFileNames.Enabled = false;
            mCopyFileHashes.Enabled = false;
            mShowInExplorer.Enabled = false;
        }
    }
    private void btnPauseTimer_Click(object sender, EventArgs e) {
        if (tmrScan.Enabled) {
            tmrScan.Stop();
            btnPauseTimer.Text = "Start Tmr";
        }
        else {
            tmrScan.Start();
            btnPauseTimer.Text = "Pause Tmr";
        }
    }

    public void ManageThread(ThreadEvent Event, bool threadRemovedFromForm = false) {
        switch (Event) {
            case ThreadEvent.ParseForInfo: {
                Debug.Print("ParseThreadForInfo called");
                string[] URLSplit = ThreadInfo.Data.Url.Split('/');

                switch (ThreadInfo.Chan) {
                    #region 4chan
                    case ChanType.FourChan: {
                        ThreadInfo.Data.BoardName = ThreadInfo.Data.Board = URLSplit[^3];
                        ThreadInfo.Data.Id = URLSplit[^1].Split('#')[0];
                        ThreadInfo.Data.BoardName = Chans.GetFullBoardName(ThreadInfo);
                        ThreadInfo.Data.DownloadPath = Path.Combine(Downloads.DownloadPath, "4chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"4chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region 7chan
                    case ChanType.SevenChan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.Data.BoardName = ThreadInfo.Data.Board = URLSplit[^3];
                        ThreadInfo.Data.Id = URLSplit[^1].Split('#')[0].Replace(".html", "");
                        ThreadInfo.Data.DownloadPath = Path.Combine(Downloads.DownloadPath, "7chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"7chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region 8chan
                    case ChanType.EightChan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.Data.BoardName = ThreadInfo.Data.Board = URLSplit[^3];
                        ThreadInfo.Data.Id = URLSplit[^1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        ThreadInfo.Data.DownloadPath = Path.Combine(Downloads.DownloadPath, "8chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"8chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region 8kun
                    case ChanType.EightKun: {
                        if (Chans.StupidFuckingBoard(ChanType.EightKun, ThreadInfo.Data.Url)) {
                            MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadIs404);
                            this.Dispose();
                            return;
                        }
                        ThreadInfo.Data.BoardName = ThreadInfo.Data.Board = URLSplit[^3];
                        ThreadInfo.Data.Id = URLSplit[^1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        ThreadInfo.Data.DownloadPath = Path.Combine(Downloads.DownloadPath, "8kun", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"8kun thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region fchan
                    case ChanType.fchan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.Data.BoardName = ThreadInfo.Data.Board = URLSplit[^3];
                        ThreadInfo.Data.Id = URLSplit[^1].Split('#')[0].Replace(".html", "");
                        ThreadInfo.Data.DownloadPath = Path.Combine(Downloads.DownloadPath, "fchan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"fchan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region u18chan
                    case ChanType.u18chan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.Data.BoardName = ThreadInfo.Data.Board = URLSplit[^3];
                        ThreadInfo.Data.Id = URLSplit[^1].Split('#')[0];
                        ThreadInfo.Data.DownloadPath = Path.Combine(Downloads.DownloadPath, "u18chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"u18chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    default: {
                        this.Text = "Invalid chan";
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadIs404);
                    } return;
                }

                ThreadInfo.UpdateJsonPath();
                if (ThreadInfo.Data.DownloadPath != null) {
                    btnOpenFolder.Enabled = true;
                }
            } break;

            case ThreadEvent.StartDownload: {
                switch (ThreadInfo.Chan) {
                    case ChanType.FourChan: {
                        if (ThreadInfo.Data.DownloadPath != $"{Downloads.DownloadPath}\\4chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}") {
                            ThreadInfo.Data.DownloadPath = $"{Downloads.DownloadPath}\\4chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}";
                        }
                        Register4chanThread();
                    } break;
                    case ChanType.SevenChan: {
                        if (ThreadInfo.Data.DownloadPath != $"{Downloads.DownloadPath}\\7chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}") {
                            ThreadInfo.Data.DownloadPath = $"{Downloads.DownloadPath}\\7chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}";
                        }
                        Register7chanThread();
                    } break;
                    case ChanType.EightChan: {
                        if (ThreadInfo.Data.DownloadPath != $"{Downloads.DownloadPath}\\8chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}") {
                            ThreadInfo.Data.DownloadPath = $"{Downloads.DownloadPath}\\8chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}";
                        }
                        Register8chanThread();
                    } break;
                    case ChanType.EightKun: {
                        if (ThreadInfo.Data.DownloadPath != $"{Downloads.DownloadPath}\\8kun\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}") {
                            ThreadInfo.Data.DownloadPath = $"{Downloads.DownloadPath}\\8kun\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}";
                        }
                        Register8kunThread();
                    } break;
                    case ChanType.fchan: {
                        if (ThreadInfo.Data.DownloadPath != $"{Downloads.DownloadPath}\\fchan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}") {
                            ThreadInfo.Data.DownloadPath = $"{Downloads.DownloadPath}\\fchan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}";
                        }
                        RegisterfchanThread();
                    } break;
                    case ChanType.u18chan: {
                        if (ThreadInfo.Data.DownloadPath != $"{Downloads.DownloadPath}\\u18chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}") {
                            ThreadInfo.Data.DownloadPath = $"{Downloads.DownloadPath}\\u18chan\\{ThreadInfo.Data.Board}\\{ThreadInfo.Data.Id}";
                        }
                        RegisterU18chanThread();
                    } break;
                    default: {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.NoStatusSet);
                    } return;
                }

                if (DownloadThread == null) {
                    Log.Write("Download thread was not created.");
                    return;
                }

                if (ThreadInfo.Data.DownloadPath != null) {
                    btnOpenFolder.Enabled = true;
                }

                ThreadInfo.HideModifiedLabelAt = Downloads.ScannerDelay - 10;
                MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadScanning);
                lbScanTimer.Text = "scanning now...";
                DownloadThread.Start();
            } break;

            case ThreadEvent.AfterDownload: {
                switch (ThreadInfo.CurrentActivity) {
                    case ThreadStatus.ThreadIsAborted: {
                        lbScanTimer.Text = "Aborted";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        this.Icon = Properties.Resources.ProgramIcon_Dead;

                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                    } break;

                    case ThreadStatus.ThreadIs404: {
                        if (Downloads.AutoRemoveDeadThreads) {
                            MainFormInstance.ThreadKilled(ThreadInfo);
                            return;
                        }
                        else {
                            lbScanTimer.Text = "404'd";
                            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                            this.Icon = Properties.Resources.ProgramIcon_Dead;

                            MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                            btnAbortRetry.Text = "Retry";
                        }
                    } break;

                    case ThreadStatus.ThreadFile404: {
                        ThreadInfo.CurrentActivity = ThreadStatus.Waiting;
                        ThreadInfo.FileWas404 = true;
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        ThreadInfo.CountdownToNextScan = Downloads.ScannerDelay - 1;
                        lvImages.Items[ThreadInfo.Data.DownloadedImagesCount].ImageIndex = _404Image;
                        if (ThreadInfo.RetryCountFor404 == 4) {
                            ThreadInfo.RetryCountFor404 = 0;
                            ThreadInfo.FileWas404 = true;
                            ThreadInfo.Data.DownloadedImagesCount++;
                            lbScanTimer.Text = "File 404, skipping";
                        }
                        else {
                            ThreadInfo.RetryCountFor404++;
                            lbScanTimer.Text = "File 404, retrying";
                        }
                        tmrScan.Start();
                    } break;

                    case ThreadStatus.ThreadIsArchived: {
                        if (Downloads.AutoRemoveDeadThreads) {
                            MainFormInstance.ThreadKilled(ThreadInfo);
                            return;
                        }
                        else {
                            lbScanTimer.Text = "Archived";
                            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                            this.Icon = Properties.Resources.ProgramIcon_Dead;
                            MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                            btnAbortRetry.Text = "Rescan";
                            ThreadInfo.ThreadModified = true;
                        }
                    } break;

                    case ThreadStatus.ThreadDownloading:
                    case ThreadStatus.Waiting:
                    case ThreadStatus.ThreadNotModified: {
                        lbNotModified.Visible = ThreadInfo.CurrentActivity == ThreadStatus.ThreadNotModified;
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        ThreadInfo.CountdownToNextScan = (ThreadInfo.Chan == ChanType.u18chan ? (60 * 5) : Downloads.ScannerDelay) - 1;
                        if (Program.DebugMode) {
                            ThreadInfo.CountdownToNextScan = 10;
                        }
                        lbScanTimer.Text = "soon (tm)";
                        ThreadInfo.CurrentActivity = ThreadStatus.Waiting;
                        tmrScan.Start();
                    } break;

                    case ThreadStatus.ThreadImproperlyDownloaded: {
                        lbScanTimer.Text = "Bad download";
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        ThreadInfo.CountdownToNextScan = Downloads.ScannerDelay - 1;
                        if (Program.DebugMode) {
                            ThreadInfo.CountdownToNextScan = 10;
                        }
                        ThreadInfo.CurrentActivity = ThreadStatus.Waiting;
                        tmrScan.Start();
                    } break;

                    case ThreadStatus.FailedToParseThreadHtml: {
                        lbScanTimer.Text = "Failed to parse thread";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                        ThreadInfo.ThreadModified = true;
                    } break;

                    // How to handle?
                    case ThreadStatus.NoThreadPosts: {
                        lbScanTimer.Text = "No thread posts";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                        ThreadInfo.ThreadModified = true;
                    } break;

                    case ThreadStatus.ThreadIsNotAllowed: {
                        lbScanTimer.Text = "Forbidden";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                        ThreadInfo.ThreadModified = true;
                    } break;

                    case ThreadStatus.ThreadInfoNotSet: {
                        lbScanTimer.Text = "No thread info";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                    } break;
                }
            } break;

            case ThreadEvent.RestartDownload: {
                ThreadInfo.HideModifiedLabelAt = Downloads.ScannerDelay - 10;
                MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadScanning);
                lbScanTimer.Text = "scanning now...";
                ResetThread.Set();
            } break;

            case ThreadEvent.AbortDownload: {
                Debug.Print("AbortDownload called");
                tmrScan.Stop();
                ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsAborted;
                CancellationToken.Cancel();
                ResetThread.Set();

                if (threadRemovedFromForm) {
                    this.Dispose();
                    return;
                }

                this.Icon = Properties.Resources.ProgramIcon_Dead;
                lbScanTimer.Text = "Aborted";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);

                btnAbortRetry.Text = "Retry";
                lbNotModified.Visible = false;
                if (Program.DebugMode) {
                    btnForce404.Enabled = false;
                }
            } break;

            case ThreadEvent.RetryDownload: {
                Debug.Print("RetryDownload called");
                this.Icon = Properties.Resources.ProgramIcon;
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

                ThreadInfo.CurrentActivity = ThreadStatus.ThreadRetrying;
                btnAbortRetry.Text = "Abort";
                if (Program.DebugMode) {
                    btnForce404.Enabled = true;
                }

                MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadInfo.CurrentActivity);
                lbScanTimer.Text = "scanning now...";
                btnAbortRetry.Text = "Abort";
                tmrScan.Stop();
                CancellationToken = new();
                ResetThread.Set();
                ManageThread(ThreadEvent.StartDownload);
            } break;

            case ThreadEvent.AbortForClosing: {
                ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsAborted;
                CancellationToken.Cancel();
                ResetThread.Set();
            } break;

            case ThreadEvent.ReloadThread: {
                if (Downloads.AutoRemoveDeadThreads && ThreadInfo.CurrentActivity switch {
                    ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsArchived or _ when ThreadInfo.Data.ThreadArchived => true,
                    _ => false
                }) {
                    MainFormInstance.ThreadKilled(ThreadInfo);
                    return;
                }

                switch (ThreadInfo.Chan) {
                    case ChanType.FourChan: {
                        this.Text = $"4chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.FourTwentyChan: {
                        this.Text = $"420chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.SevenChan: {
                        lvImages.Columns.RemoveAt(3);
                        this.Text = $"7chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.EightChan: {
                        lvImages.Columns.RemoveAt(3);
                        this.Text = $"8chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                        ThreadInfo.ThreadUri = new(ThreadInfo.Data.Url);
                    } break;
                    case ChanType.EightKun: {
                        this.Text = $"8kun thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.fchan: {
                        lvImages.Columns.RemoveAt(3);
                        this.Text = $"fchan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.u18chan: {
                        lvImages.Columns.RemoveAt(3);
                        this.Text = $"u18chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;

                    default: {
                        ManageThread(ThreadEvent.ParseForInfo);
                    } break;
                }

                ThreadInfo.UpdateJsonPath();

                if (ThreadInfo.Data.DownloadPath != null) {
                    btnOpenFolder.Enabled = true;
                }
            } break;
        }
    }
    public void RemoveFileFromSystem() {
        if (lvImages.SelectedItems.Count < 1) {
            return;
        }

        for (int i = 0; i < lvImages.SelectedItems.Count; i++) {
            var Item = lvImages.SelectedItems[i];
            if (Item.Tag is not GenericFile PostFile) {
                continue;
            }

            string FilePath = Path.Combine(ThreadInfo.Data.DownloadPath, PostFile.SavedFile);
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }

            string ThumbnailPath = Path.Combine(ThreadInfo.Data.DownloadPath, PostFile.SavedThumbnailFile);
            if (File.Exists(ThumbnailPath)) {
                File.Delete(ThumbnailPath);
            }
            ThreadInfo.Data.DownloadedImagesCount--;
            UpdateCounts();
            Item.ImageIndex = WaitingImage;
            PostFile.Status = FileDownloadStatus.Undownloaded;
        }
    }
    public void RemoveFileFromThread() {
        if (lvImages.SelectedItems.Count < 1) {
            return;
        }

        for (int i = lvImages.SelectedItems.Count; i >= 0; i--) {
            var Item = lvImages.SelectedItems[i];
            if (Item.Tag is not GenericFile PostFile) {
                continue;
            }

            string fileName = (PostFile.OriginalFileName + "." + PostFile.FileExtension).ToLowerInvariant();

            if (ThreadInfo.Data.DuplicateNames.TryGetValue(fileName, out int count)) {
                if (count > 1) {
                    ThreadInfo.Data.DuplicateNames[fileName]--;
                }
                else {
                    ThreadInfo.Data.DuplicateNames.Remove(fileName);
                }
            }

            PostFile.Parent.PostFiles.Remove(PostFile);
            ThreadInfo.Data.EstimatedSize -= PostFile.FileSize;
            ThreadInfo.Data.ThreadImagesCount--;
            ThreadInfo.Data.ThreadPostsCount--;
            Item.ImageIndex = RemovedFromThreadImage;
            PostFile.Status = FileDownloadStatus.RemovedFromThread;
            ThreadInfo.ThreadModified = true;
            lvImages.Items.Remove(Item);
        }

        UpdateCounts();
    }
    public void UpdateThreadName(bool ApplyToMainForm = false) {
        string ThreadNameBuffer = "unknown thread - {0} - {1}";
        switch (ThreadInfo.Chan) {
            case ChanType.FourChan:
                ThreadNameBuffer = "4chan thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.FourTwentyChan:
                ThreadNameBuffer = "420chan thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.SevenChan:
                ThreadNameBuffer = "7chan thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.EightChan:
                ThreadNameBuffer = "8chan thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                }
                break;
            case ChanType.EightKun:
                ThreadNameBuffer = "8kun thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                }
                break;
            case ChanType.fchan:
                ThreadNameBuffer = "fchan thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.u18chan:
                ThreadNameBuffer = "u18chan thread - {0} - {1}";
                if (ThreadInfo.Data.RetrievedThreadName) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && !ThreadInfo.Data.SetCustomName) {
                        MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            default:
                this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                return;
        }
    }

    private async Task<HttpResponseMessage?> TryGetResponseIfModifiedAsync(HttpRequestMessage request, CancellationToken token) {
        // If-modified-since is a default header.
        // Any other headers must be added per-chan.
        request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;
        var Response = await TryGetResponseAsync(request, token);
        if (Response != null) {
            ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;
        }
        return Response;
    }
    private async Task<HttpResponseMessage?> TryGetResponseAsync(HttpRequestMessage request, CancellationToken token) {
        HttpResponseMessage? Response = null;
        int Retries = 0;
        while (true) {
            try {
                Response = await DownloadClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
                    .ConfigureAwait(false);

                if (!Response.IsSuccessStatusCode) {
    #if !NET6_0_OR_GREATER // Auto-redirect, for 308+ on framework.
                    if (((int)Response.StatusCode is > 304 and < 400) && Response.Headers.Location is not null) {
                        request.RequestUri = Response.Headers.Location;
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        return await TryGetResponseAsync(request, token)
                            .ConfigureAwait(false);
                    }
    #endif // Auto-redirect, for 308+ on framework.

                    if ((int)Response.StatusCode > 499 && (++Retries) < 5) {
                        RequestMessage.ResetRequest(request);
                        Response.Dispose();
                        continue;
                    }

                    ThreadInfo.StatusCode = Response.StatusCode;
                    Response.Dispose();
                    return null;
                }
            }
            catch {
                Response?.Dispose();
                return null;
            }

            break;
        }

        ThreadInfo.StatusCode = Response.StatusCode;
        return Response;
    }
    private async Task<string> GetStringAsync(HttpResponseMessage Response, CancellationToken token) {
        using Stream Content = await Response.Content.ReadAsStreamAsync();
        using MemoryStream Destination = new();

        await StreamCallback(Content, Destination, token);
        await Destination.FlushAsync();

        byte[] Bytes;
        switch (Response.Content.Headers.ContentEncoding.FirstOrDefault()) {
            case "br": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Brotli(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            case "gzip": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.GZip(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            case "deflate": {
                using MemoryStream DecompressorStream = new();
                await WebDecompress.Deflate(Destination, DecompressorStream);
                Destination.Close();
                Bytes = DecompressorStream.ToArray();
                DecompressorStream.Close();
            } break;
            default: {
                Bytes = Destination.ToArray();
                Destination.Close();
            } break;
        }

        return (Response.Content.Headers.ContentType.CharSet ?? "utf-8").ToLowerInvariant() switch {
            "ascii" => Encoding.ASCII.GetString(Bytes),
            "utf-7" => Encoding.UTF7.GetString(Bytes),
            "utf-32" => Encoding.UTF32.GetString(Bytes),
            "utf-16" or "unicode" => Encoding.Unicode.GetString(Bytes),
            "utf-16-be" or "utf-16be" or "unicode-be" or "unicodebe" => Encoding.BigEndianUnicode.GetString(Bytes),
            _ => Encoding.UTF8.GetString(Bytes),
        };
    }
    private async Task DownloadFilesAsync(CancellationToken token) {
        Log.Write("Downloading files");
        if (!ThreadInfo.AddedNewPosts) {
            return;
        }

        ThreadInfo.CurrentActivity = ThreadStatus.ThreadDownloading;
        ThreadInfo.DownloadingFiles = true;

        string ThumbnailDirectory = Path.Combine(ThreadInfo.Data.DownloadPath, "thumb");

        if (!Directory.Exists(ThreadInfo.Data.DownloadPath)) {
            Directory.CreateDirectory(ThreadInfo.Data.DownloadPath);
        }
        if (Downloads.SaveThumbnails && !Directory.Exists(ThumbnailDirectory)) {
            Directory.CreateDirectory(ThumbnailDirectory);
        }

        if (Downloads.SaveHTML) {
            File.WriteAllText(ThreadInfo.Data.DownloadPath + "\\Thread.html", ThreadInfo.ThreadHTML + HtmlControl.GetHTMLFooter(ThreadInfo));
        }

        for (int PostIndex = 0; PostIndex < ThreadInfo.Data.ThreadPosts.Count; PostIndex++) {
            var Post = ThreadInfo.Data.ThreadPosts[PostIndex];

            // Skip posts without files.
            if (!Post.HasFiles) {
                continue;
            }

            for (int FileIndex = 0; FileIndex < Post.PostFiles.Count; FileIndex++) {
                var PostFile = Post.PostFiles[FileIndex];

                if (PostFile.Status != FileDownloadStatus.Downloaded && PostFile.Status != FileDownloadStatus.FileNotFound) {
                    // Set the icon in the list to "Downloading".
                    this.Invoke(() => PostFile.ListViewItem.ImageIndex = DownloadingImage);

                    string FileDownloadPath = Path.Combine(ThreadInfo.Data.DownloadPath, PostFile.SavedFile);
                    string ThumbFileDownloadPath = Path.Combine(ThreadInfo.Data.DownloadPath, PostFile.SavedThumbnailFile);

                    // Check for existing files.
                    if (!File.Exists(FileDownloadPath)) {
                        HttpRequestMessage FileRequest = new(HttpMethod.Get, PostFile.FileUrl!);
                        if (ThreadInfo.Chan == ChanType.EightChan) {
                            FileRequest.Headers.Add("Referer", ThreadInfo.Data.Url);
                        }
                        using var Response = await TryGetResponseAsync(FileRequest, token)
                            .ConfigureAwait(false);

                        if (Response == null) {
                            if (ThreadInfo.StatusCode == HttpStatusCode.NotFound) {
                                PostFile.Status = FileDownloadStatus.FileNotFound;
                                this.Invoke(() => PostFile.ListViewItem.ImageIndex = _404Image);
                            }
                            else {
                                PostFile.Status = FileDownloadStatus.Error;
                                this.Invoke(() => PostFile.ListViewItem.ImageIndex = ErrorImage);
                            }
                        }
                        else {
                            await GetFile(Response, FileDownloadPath, token)
                                .ConfigureAwait(false);

                            ThreadInfo.Data.DownloadedImagesCount++;
                            PostFile.Status = FileDownloadStatus.Downloaded;
                            ThreadInfo.ThreadModified = true;
                            this.Invoke(() => {
                                lbNumberOfFiles.Text = $"number of files:  {ThreadInfo.Data.DownloadedImagesCount} / {ThreadInfo.Data.ThreadImagesCount} (~{HtmlControl.GetSize(ThreadInfo.Data.EstimatedSize)})";
                                PostFile.ListViewItem.ImageIndex = FinishedImage;
                            });
                        }
                    }
                    else {
                        ThreadInfo.Data.DownloadedImagesCount++;
                        PostFile.Status = FileDownloadStatus.Downloaded;
                        ThreadInfo.ThreadModified = true;
                        this.Invoke(() => {
                            lbNumberOfFiles.Text = $"number of files:  {ThreadInfo.Data.DownloadedImagesCount} / {ThreadInfo.Data.ThreadImagesCount} (~{HtmlControl.GetSize(ThreadInfo.Data.EstimatedSize)})";
                            PostFile.ListViewItem.ImageIndex = ReloadedDownloadedImage;
                        });
                    }

                    // Thumbnails are second-rate, not important if they fail.
                    if (Downloads.SaveThumbnails && !File.Exists(ThumbFileDownloadPath)) {
                        HttpRequestMessage FileRequest = new(HttpMethod.Get, PostFile.ThumbnailFileUrl!);
                        if (ThreadInfo.Chan == ChanType.EightChan) {
                            FileRequest.Headers.Add("Referer", ThreadInfo.Data.Url);
                        }
                        using var Response = await TryGetResponseAsync(FileRequest, token)
                            .ConfigureAwait(false);
                        if (Response != null) {
                            await GetFile(Response, ThumbFileDownloadPath, token)
                                .ConfigureAwait(false);
                        }
                    }

                    // Sleep for 100ms.
                    if (PauseBetweenFiles) {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // Save the thread data now.
        if (General.AutoSaveThreads) {
            ThreadInfo.SaveThread();
            ThreadInfo.ThreadModified = false;
        }
    }
    private static async Task<bool> GetFile(HttpResponseMessage Response, string dest, CancellationToken token) {
        try {
            using Stream Content = await Response.Content.ReadAsStreamAsync();
            using FileStream Destination = new(
                path: dest,
                mode: FileMode.Create,
                access: FileAccess.ReadWrite,
                share: FileShare.Read);

            await StreamCallback(Content, Destination, token);
            await Destination.FlushAsync();
            Destination.Close();
            return true;
        }
        catch {
            return false;
        }
    }
    private static async Task WriteToStreamAsync(Stream HttpStream, Stream Output, CancellationToken token) {
        byte[] buffer = new byte[DefaultBuffer];
        int bytesRead;

        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0) {
            await Output.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }
    private static async Task ThrottledWriteToStreamAsync(Stream HttpStream, Stream Output, CancellationToken token) {
        byte[] buffer = new byte[ThrottleBufferSize];
        int bytesRead;

        using ThrottledStream ThrottledWriter = new(Output, ThrottleSize);
        while ((bytesRead = await HttpStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) != 0) {
            await ThrottledWriter.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
        }
    }

    private string GetFileSuffix(string OriginalName, string Extension) {
        // replace any invalid file name characters.
        // some linux nerds can have invalid windows file names as file names
        // so we gotta filter them.
        OriginalName = FileHandler.ReplaceIllegalCharacters(OriginalName ?? string.Empty);
        string ExpectedFullFile = (OriginalName + "." + Extension).ToLowerInvariant();

        // check for duplicates, and set the suffix to "(d1)" if there is duplicates
        // (the space is intentional)
        string FileNameSuffix = string.Empty;

        if (Downloads.PreventDuplicates) {
            if (!ThreadInfo.Data.DuplicateNames.ContainsKey(ExpectedFullFile)) {
                ThreadInfo.Data.DuplicateNames.Add(ExpectedFullFile, 1);
            }
            else {
                FileNameSuffix = $" (d{ThreadInfo.Data.DuplicateNames[ExpectedFullFile]++})";
            }
        }
        return FileNameSuffix;
    }
    private string GetLastModifiedTime() {
        if (!ThreadInfo.Data.LastModified.HasValue) {
            return "not supported";
        }
        return ThreadInfo.Data.LastModified.ToString();
    }
    private void HandleBadFileName(GenericFile File) {
        Log.Write("Could not handle file " + File.FileId);
        this.Invoke(() => File.ListViewItem.ImageIndex = ErrorImage);
    }
    private void HandleBadThumbFileName(GenericFile File) {
        Log.Write("Could not handle thumb file " + File.FileId);
        this.Invoke(() => File.ListViewItem.ImageIndex = ErrorImage);
    }
    private void HandleStatusCode() {
        ThreadInfo.CurrentActivity = ThreadInfo.StatusCode switch {
            HttpStatusCode.NotModified => ThreadStatus.ThreadNotModified,
            HttpStatusCode.Forbidden => ThreadStatus.ThreadIsNotAllowed,
            HttpStatusCode.NotFound when ThreadInfo.DownloadingFiles => ThreadStatus.ThreadFile404,
            HttpStatusCode.NotFound => ThreadStatus.ThreadIs404,
            _ => ThreadStatus.ThreadImproperlyDownloaded,
        };
    }
    private void UpdateCounts() {
        lbPostsParsed.Text = "posts parsed: " + ThreadInfo.Data.ParsedPostIds.Count.ToString();
        lbNumberOfFiles.Text = $"number of files:  {ThreadInfo.Data.DownloadedImagesCount} / {ThreadInfo.Data.ThreadImagesCount} (~{HtmlControl.GetSize(ThreadInfo.Data.EstimatedSize)})";
        lbLastModified.Text = "last modified: " + GetLastModifiedTime();
    }
    private void PrepareDownload() {
        this.Invoke(() => {
            UpdateCounts();
            lbScanTimer.Text = "Downloading files";
            MainFormInstance.SetItemStatus(ThreadInfo.ThreadIndex, ThreadStatus.ThreadDownloading);
        });
    }
    private void AddNewPost(GenericPost CurrentPost) {
        if (CurrentPost.HasFiles) {
            for (int PostIndex = 0; PostIndex < CurrentPost.PostFiles.Count; PostIndex++) {
                var File = CurrentPost.PostFiles[PostIndex];
                string FileName = (Downloads.SaveOriginalFilenames ? File.OriginalFileName : File.FileId)!;
                string Suffix = GetFileSuffix(File.OriginalFileName!, File.FileExtension!);
                string ThumbFileName = File.ThumbnailFileName!;
                File.SavedFileName = FileName + Suffix;
                File.SavedThumbnailFile = ThumbFileName;

                if (!Downloads.AllowFileNamesGreaterThan255) {
                    int FileNameLength = ThreadInfo.Data.DownloadPath.Length +
                        FileName.Length +
                        File.FileExtension!.Length +
                        Suffix.Length +
                        2; // ext period (1) and download path separator (1)

                    if (FileNameLength > 255) {
                        int TrimSize = FileNameLength - 255;
                        if (FileName.Length <= TrimSize) {
                            HandleBadFileName(File);
                            return;
                        }
                        FileName = FileName[..^TrimSize];
                    }

                    if (Downloads.SaveThumbnails) {
                        int ThumbFileNameLength = ThreadInfo.Data.DownloadPath.Length +
                            ThumbFileName.Length +
                            File.ThumbnailFileExtension!.Length +
                            8; // ext period (1), path separators (2), "thumb" (5)

                        if (ThumbFileNameLength > 255) {
                            int TrimSize = ThumbFileNameLength - 255;
                            if (ThumbFileName.Length <= TrimSize) {
                                HandleBadThumbFileName(File);
                                return;
                            }
                            ThumbFileName = ThumbFileName[..^TrimSize];
                        }
                    }
                }

                File.SavedFile = FileName + Suffix + "." + File.FileExtension;
                File.SavedThumbnailFile = Path.Combine("thumb", ThumbFileName + "." + File.ThumbnailFileExtension);

                // add a new listviewitem to the listview for this image.
                ListViewItem lvi = new();
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.Name = File.FileId;
                lvi.SubItems[0].Text = File.FileId;
                lvi.SubItems[1].Text = File.FileExtension;
                lvi.SubItems[2].Text = File.OriginalFileName + "." + File.FileExtension;
                lvi.SubItems[3].Text = File.FileHash;
                lvi.ImageIndex = WaitingImage;
                lvi.Tag = File;
                File.ListViewItem = lvi;
                this.Invoke(() => lvImages.Items.Add(lvi));

                ThreadInfo.Data.ThreadImagesCount++;
                ThreadInfo.Data.ThreadPostsCount++;
                ThreadInfo.Data.EstimatedSize += File.FileSize;
            }
        }

        // add the new post to the data.
        ThreadInfo.ThreadHTML.Append(HtmlControl.GetPostHtmlData(CurrentPost, ThreadInfo));
        ThreadInfo.Data.ParsedPostIds.Add(CurrentPost.PostId);
        ThreadInfo.Data.ThreadPosts.Add(CurrentPost);
        ThreadInfo.AddedNewPosts = ThreadInfo.ThreadModified = true;
    }
    private void AddNewPostNoHash(GenericPost CurrentPost) {
        if (CurrentPost.HasFiles) {
            for (int PostIndex = 0; PostIndex < CurrentPost.PostFiles.Count;  PostIndex++) {
                var File = CurrentPost.PostFiles[PostIndex];
                string FileName = (Downloads.SaveOriginalFilenames ? File.OriginalFileName : File.FileId)!;
                string Suffix = GetFileSuffix(File.OriginalFileName!, File.FileExtension!);
                string ThumbFileName = File.ThumbnailFileName!;
                File.SavedFileName = FileName + Suffix;
                File.SavedThumbnailFile = ThumbFileName;

                if (!Downloads.AllowFileNamesGreaterThan255) {
                    int FileNameLength = ThreadInfo.Data.DownloadPath.Length +
                        FileName.Length +
                        File.FileExtension!.Length +
                        Suffix.Length +
                        2; // ext period (1) and download path separator (1)

                    if (FileNameLength > 255) {
                        int TrimSize = FileNameLength - 255;
                        if (FileName.Length <= TrimSize) {
                            HandleBadFileName(File);
                            return;
                        }
                        FileName = FileName[..^TrimSize];
                    }

                    if (Downloads.SaveThumbnails) {
                        int ThumbFileNameLength = ThreadInfo.Data.DownloadPath.Length +
                            ThumbFileName.Length +
                            File.ThumbnailFileExtension!.Length +
                            8; // ext period (1), path separators (2), "thumb" (5)

                        if (ThumbFileNameLength > 255) {
                            int TrimSize = ThumbFileNameLength - 255;
                            if (ThumbFileName.Length <= TrimSize) {
                                HandleBadThumbFileName(File);
                                return;
                            }
                            ThumbFileName = ThumbFileName[..^TrimSize];
                        }
                    }
                }

                File.SavedFile = FileName + Suffix + "." + File.FileExtension;
                File.SavedThumbnailFile = Path.Combine("thumb", ThumbFileName + "." + File.ThumbnailFileExtension);

                // add a new listviewitem to the listview for this image.
                ListViewItem lvi = new();
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.Name = File.FileId;
                lvi.SubItems[0].Text = File.FileId;
                lvi.SubItems[1].Text = File.FileExtension;
                lvi.SubItems[2].Text = File.OriginalFileName + "." + File.FileExtension;
                lvi.ImageIndex = WaitingImage;
                lvi.Tag = File;
                File.ListViewItem = lvi;
                this.Invoke(() => lvImages.Items.Add(lvi));

                ThreadInfo.Data.ThreadImagesCount++;
                ThreadInfo.Data.ThreadPostsCount++;
                ThreadInfo.Data.EstimatedSize += File.FileSize;
            }
        }

        // add the new post to the data.
        ThreadInfo.ThreadHTML.Append(HtmlControl.GetPostHtmlData(CurrentPost, ThreadInfo));
        ThreadInfo.Data.ParsedPostIds.Add(CurrentPost.PostId);
        ThreadInfo.Data.ThreadPosts.Add(CurrentPost);
        ThreadInfo.AddedNewPosts = ThreadInfo.ThreadModified = true;
    }
    private void LoadExistingPosts() {
        if (ThreadInfo.Data.ThreadPosts.Count > 0) {
            for (int i = 0; i < ThreadInfo.Data.ThreadPosts.Count; i++) {
                GenericPost Post = ThreadInfo.Data.ThreadPosts[i];
                if (!Post.HasFiles) {
                    continue;
                }

                for (int x = 0; x < Post.PostFiles.Count; x++) {
                    GenericFile File = Post.PostFiles[x];
                    ListViewItem lvi = new();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.Name = File.FileId;
                    lvi.SubItems[0].Text = File.FileId;
                    lvi.SubItems[1].Text = File.FileExtension;
                    lvi.SubItems[2].Text = File.OriginalFileName;
                    lvi.SubItems[3].Text = File.FileHash;
                    lvi.ImageIndex = File.Status switch {
                        FileDownloadStatus.Downloaded =>
                            System.IO.File.Exists(Path.Combine(ThreadInfo.Data.DownloadPath, File.SavedFile)) ?
                                ReloadedDownloadedImage : ReloadedMissingImage,
                        FileDownloadStatus.Error => ErrorImage,
                        FileDownloadStatus.FileNotFound => _404Image,
                        _ => WaitingImage
                    };
                    lvi.Tag = File;
                    File.ListViewItem = lvi;
                    this.Invoke(() => lvImages.Items.Add(lvi));
                }
            }

            this.Invoke(() => UpdateCounts());
        }
    }
    private void LoadExistingPostsNoHash() {
        if (ThreadInfo.Data.ThreadPosts.Count > 0) {
            for (int i = 0; i < ThreadInfo.Data.ThreadPosts.Count; i++) {
                GenericPost Post = ThreadInfo.Data.ThreadPosts[i];
                if (!Post.HasFiles) {
                    continue;
                }

                for (int x = 0; x < Post.PostFiles.Count; x++) {
                    GenericFile File = Post.PostFiles[x];
                    ListViewItem lvi = new();
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    lvi.Name = File.FileId;
                    lvi.SubItems[0].Text = File.FileId;
                    lvi.SubItems[1].Text = File.FileExtension;
                    lvi.SubItems[2].Text = File.OriginalFileName;
                    lvi.ImageIndex = File.Status switch {
                        FileDownloadStatus.Downloaded =>
                            System.IO.File.Exists(Path.Combine(ThreadInfo.Data.DownloadPath, File.SavedFile)) ?
                                ReloadedDownloadedImage : ReloadedMissingImage,
                        FileDownloadStatus.Error => ErrorImage,
                        FileDownloadStatus.FileNotFound => _404Image,
                        _ => WaitingImage
                    };
                    lvi.Tag = File;
                    File.ListViewItem = lvi;
                    this.Invoke(() => lvImages.Items.Add(lvi));
                }
            }

            this.Invoke(() => UpdateCounts());
        }
    }

    private void Register4chanThread() {
        this.DownloadThread = new Thread(async () => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // HTML
                if (!ThreadInfo.ThreadReloaded) {
                    ThreadInfo.ThreadHTML = new(HtmlControl.GetHTMLBase(ThreadInfo));
                }
                else {
                    ThreadInfo.ThreadHTML = new(HtmlControl.RebuildHTML(ThreadInfo));
                    LoadExistingPosts();
                }

                // Main loop
                do {
                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;

                    // Request that will be sent to the API.
                    HttpRequestMessage Request = new(HttpMethod.Get,
                        $"https://a.4cdn.org/{ThreadInfo.Data.Board}/thread/{ThreadInfo.Data.Id}.json");
                    // Should more headers be added?

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await TryGetResponseIfModifiedAsync(Request, CancellationToken.Token)
                        .ConfigureAwait(false);
                    //var ResponseTask = TryGetResponseIfModifiedAsync(Request, CancellationToken.Token);
                    //ResponseTask.Wait();
                    //using var Response = ResponseTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode();
                        if (ThreadInfo.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Get the json.
                    string CurrentJson = await GetStringAsync(Response, CancellationToken.Token)
                        .ConfigureAwait(false);
                    //var JsonTask = GetStringAsync(Response, CancellationToken.Token);
                    //JsonTask.Wait();
                    //string CurrentJson = JsonTask.Result;

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = CurrentJson.JsonDeserialize<FourChanThread>();

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.posts.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!ThreadInfo.Data.RetrievedThreadName && !ThreadInfo.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.posts[0].sub,
                            Comment: ThreadData.posts[0].com,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.Data.RetrievedThreadName = true;
                        ThreadInfo.ThreadHTML = ThreadInfo.ThreadHTML.Replace("<title></title>",
                            $"<title> /{ThreadInfo.Data.Board}/ - {NewName} - 4chan</title>");
                        ThreadInfo.Data.HtmlThreadNameSet = true;

                        // Update the name application wide.
                        this.Invoke(() => UpdateThreadName(true));
                    }

                    // Start counting through the posts.
                    for (int PostIndex = 0; PostIndex < ThreadData.posts.Length; PostIndex++) {
                        // Set the temporary post to the looped index post.
                        FourChanPost Post = ThreadData.posts[PostIndex];
                        if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.no)) {
                            GenericPost CurrentPost = new(Post, ThreadInfo) {
                                FirstPost = PostIndex == 0
                            };

                            AddNewPost(CurrentPost);
                        }

                        CancellationToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    await DownloadFilesAsync(CancellationToken.Token)
                        .ConfigureAwait(false);
                    //var DownloadTask = DownloadFilesAsync(CancellationToken.Token);
                    //DownloadTask.Wait();
                    //CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    // check for archive flag in the post.
                    if (ThreadData.posts[0].archived) {
                        ThreadInfo.Data.ThreadArchived = true;
                        ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Write("Exiting thread " + ThreadInfo.CurrentActivity);
        }) {
            Name = $"4chan thread {ThreadInfo.Data.Board}/{ThreadInfo.Data.Id}"
        };
    }
    private void Register7chanThread() {
        this.DownloadThread = new Thread(() => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // HTML
                if (!ThreadInfo.ThreadReloaded) {
                    ThreadInfo.ThreadHTML = new(HtmlControl.GetHTMLBase(ThreadInfo));
                }
                else {
                    ThreadInfo.ThreadHTML = new(HtmlControl.RebuildHTML(ThreadInfo));
                    LoadExistingPostsNoHash();
                }

                // Main loop
                do {
                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;

                    // Request that will be sent to the API.
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.Data.Url);
                    // Should more headers be added?

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    //using var Response = await TryGetResponseIfModifiedAsync(Request, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var ResponseTask = TryGetResponseIfModifiedAsync(Request, CancellationToken.Token);
                    ResponseTask.Wait();
                    using var Response = ResponseTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode();
                        if (ThreadInfo.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Get the json.
                    //string CurrentJson = await GetStringAsync(Response, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var JsonTask = GetStringAsync(Response, CancellationToken.Token);
                    JsonTask.Wait();
                    string CurrentJson = JsonTask.Result;

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    SevenChanPost[] ThreadData;
                    try {
                        ThreadData = SevenChan.Generate(CurrentJson);
                    }
                    catch (Exception ex) {
                        Log.ReportException(ex);
                        ThreadInfo.CurrentActivity = ThreadStatus.FailedToParseThreadHtml;
                        this.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!ThreadInfo.Data.RetrievedThreadName && !ThreadInfo.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].Subject,
                            Comment: ThreadData[0].MessageBody,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.Data.RetrievedThreadName = true;
                        ThreadInfo.ThreadHTML = ThreadInfo.ThreadHTML.Replace("<title></title>",
                            $"<title> /{ThreadInfo.Data.Board}/ - {NewName} - 7chan</title>");
                        ThreadInfo.Data.HtmlThreadNameSet = true;

                        // Update the name application wide.
                        this.Invoke(() => UpdateThreadName(true));
                    }

                    // check for archive flag in the post.
                    //ThreadInfo.Data.ThreadArchived = ThreadData[0].Archived;

                    // Start counting through the posts.
                    for (int PostIndex = 0; PostIndex < ThreadData.Length; PostIndex++) {
                        // Set the temporary post to the looped index post.
                        SevenChanPost Post = ThreadData[PostIndex];
                        if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.PostId)) {
                            GenericPost CurrentPost = new(Post) {
                                FirstPost = PostIndex == 0
                            };

                            AddNewPostNoHash(CurrentPost);
                        }

                        CancellationToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    //await DownloadFilesAsync(CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var DownloadTask = DownloadFilesAsync(CancellationToken.Token);
                    DownloadTask.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    //// check for archive flag in the post.
                    //if (ThreadData.posts[0].archived) {
                    //    ThreadInfo.Data.ThreadArchived = true;
                    //    ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    //    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                    //    break;
                    //}

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Write("Exiting thread " + ThreadInfo.CurrentActivity);
        }) {
            Name = $"7chan thread {ThreadInfo.Data.Board}/{ThreadInfo.Data.Id}"
        };
    }
    private void Register8chanThread() {
        this.DownloadThread = new Thread(() => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // Uri that is used for requests.
                ThreadInfo.ThreadUri = new(ThreadInfo.Data.Url);

                // Retrieve the board data before the loop.
                if (!ThreadInfo.ThreadReloaded) {
                    var BoardTask = EightChan.GetBoardAsync(ThreadInfo.Data.Board, DownloadClient, CancellationToken.Token);
                    BoardTask.Wait();
                    var Board = BoardTask.Result;
                    if (Board != null) {
                        ThreadInfo.Data.BoardName = Board.BoardName;
                        ThreadInfo.Data.BoardSubtitle = Board.BoardDescription;
                    }
                    else {
                        Log.Write("Could not get the board name.");
                    }
                    ThreadInfo.Data.RetrievedBoardName = true;
                    ThreadInfo.ThreadModified = true;
                    ThreadInfo.ThreadHTML = new(HtmlControl.GetHTMLBase(ThreadInfo));
                    CancellationToken.Token.ThrowIfCancellationRequested();
                }
                else {
                    ThreadInfo.ThreadHTML = new(HtmlControl.RebuildHTML(ThreadInfo));
                    LoadExistingPostsNoHash();
                }

                // Main loop
                do {
                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;

                    // Request that will be sent to the API.
                    HttpRequestMessage Request = new(HttpMethod.Get,
                        $"https://8chan.moe/{ThreadInfo.Data.Board}/res/{ThreadInfo.Data.Id}.json");
                    // Should more headers be added?

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    //using var Response = await TryGetResponseIfModifiedAsync(Request, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var ResponseTask = TryGetResponseIfModifiedAsync(Request, CancellationToken.Token);
                    ResponseTask.Wait();
                    using var Response = ResponseTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode();
                        if (ThreadInfo.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Get the json.
                    //string CurrentJson = await GetStringAsync(Response, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var JsonTask = GetStringAsync(Response, CancellationToken.Token);
                    JsonTask.Wait();
                    string CurrentJson = JsonTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = CurrentJson.JsonDeserialize<EightChanThread>();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        break;
                    }

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!ThreadInfo.Data.RetrievedThreadName && !ThreadInfo.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.subject,
                            Comment: ThreadData.message,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.Data.RetrievedThreadName = true;
                        ThreadInfo.ThreadHTML = ThreadInfo.ThreadHTML.Replace("<title></title>",
                            $"<title> /{ThreadInfo.Data.Board}/ - {NewName} - 8chan</title>");
                        ThreadInfo.Data.HtmlThreadNameSet = true;

                        // Update the name application wide.
                        this.Invoke(() => UpdateThreadName(true));
                    }

                    // Parse the first post
                    if (!ThreadInfo.Data.ParsedPostIds.Contains(ThreadData.threadId)) {
                        GenericPost CurrentPost = new(ThreadData, ThreadInfo) {
                            FirstPost = true,
                        };

                        AddNewPostNoHash(CurrentPost);
                    }

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // Start counting through the replies.
                    if (ThreadData.posts?.Length > 0) {
                        for (int PostIndex = 0; PostIndex < ThreadData.posts.Length; PostIndex++) {
                            // Set the temporary post to the looped index post.
                            EightChanPost Post = ThreadData.posts[PostIndex];
                            if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.postId)) {
                                GenericPost CurrentPost = new(Post, ThreadInfo) {
                                    FirstPost = false,
                                };

                                AddNewPostNoHash(CurrentPost);
                            }

                            CancellationToken.Token.ThrowIfCancellationRequested();
                        }
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    //await DownloadFilesAsync(CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var DownloadTask = DownloadFilesAsync(CancellationToken.Token);
                    DownloadTask.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    // check for archive flag in the post.
                    if (ThreadData.archived) {
                        ThreadInfo.Data.ThreadArchived = true;
                        ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Write("Exiting thread " + ThreadInfo.CurrentActivity);
        }) {
            Name = $"8chan thread {ThreadInfo.Data.Board}/{ThreadInfo.Data.Id}"
        };
    }
    // Needs: Help. Like psychological help. Unused currently, 8kun dead.
    private void Register8kunThread() {
        this.DownloadThread = new Thread(() => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // Retrieve the board data before the loop.
                if (!ThreadInfo.ThreadReloaded) {
                    var BoardTask = EightKun.GetBoardAsync(ThreadInfo, DownloadClient, CancellationToken.Token);
                    BoardTask.Wait();
                    var Board = BoardTask.Result;
                    if (Board != null) {
                        ThreadInfo.Data.BoardName = Board.title;
                        ThreadInfo.Data.BoardSubtitle = Board.subtitle;
                    }
                    else {
                        Log.Write("Could not get the board name.");
                    }
                    ThreadInfo.Data.RetrievedBoardName = true;
                    ThreadInfo.ThreadModified = true;
                    ThreadInfo.ThreadHTML = new(HtmlControl.GetHTMLBase(ThreadInfo));
                    CancellationToken.Token.ThrowIfCancellationRequested();
                }
                else {
                    ThreadInfo.ThreadHTML = new(HtmlControl.RebuildHTML(ThreadInfo));
                    LoadExistingPosts();
                }

                // Main loop
                do {
                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;

                    // Request that will be sent to the API.
                    HttpRequestMessage Request = new(HttpMethod.Get,
                        $"https://8kun.top/{ThreadInfo.Data.Board}/res/{ThreadInfo.Data.Id}.json");
                    // Should more headers be added?

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    //using var Response = await TryGetResponseIfModifiedAsync(Request, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var ResponseTask = TryGetResponseIfModifiedAsync(Request, CancellationToken.Token);
                    ResponseTask.Wait();
                    using var Response = ResponseTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode();
                        if (ThreadInfo.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Get the json.
                    //string CurrentJson = await GetStringAsync(Response, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var JsonTask = GetStringAsync(Response, CancellationToken.Token);
                    JsonTask.Wait();
                    string CurrentJson = JsonTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = CurrentJson.JsonDeserialize<EightKunThread>();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        break;
                    }

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!ThreadInfo.Data.RetrievedThreadName && !ThreadInfo.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.posts[0].sub,
                            Comment: ThreadData.posts[0].com,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.Data.RetrievedThreadName = true;
                        ThreadInfo.ThreadHTML = ThreadInfo.ThreadHTML.Replace("<title></title>",
                            $"<title> /{ThreadInfo.Data.Board}/ - {NewName} - 8kun</title>");
                        ThreadInfo.Data.HtmlThreadNameSet = true;

                        // Update the name application wide.
                        this.Invoke(() => UpdateThreadName(true));
                    }

                    // Start counting through the posts.
                    if (ThreadData.posts?.Length > 0) {
                        for (int PostIndex = 0; PostIndex < ThreadData.posts.Length; PostIndex++) {
                            // Set the temporary post to the looped index post.
                            EightKunPost Post = ThreadData.posts[PostIndex];
                            if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.no)) {
                                GenericPost CurrentPost = new(Post) {
                                    FirstPost = false,
                                };

                                AddNewPost(CurrentPost);
                            }

                            CancellationToken.Token.ThrowIfCancellationRequested();
                        }
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    //await DownloadFilesAsync(CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var DownloadTask = DownloadFilesAsync(CancellationToken.Token);
                    DownloadTask.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    //// check for archive flag in the post.
                    //if (ThreadData.archived) {
                    //    ThreadInfo.Data.ThreadArchived = true;
                    //    ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    //    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                    //    break;
                    //}

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Write("Exiting thread " + ThreadInfo.CurrentActivity);
        }) {
            Name = $"8kun thread {ThreadInfo.Data.Board}/{ThreadInfo.Data.Id}"
        };
    }
    private void RegisterfchanThread() {
        this.DownloadThread = new Thread(() => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // HTML
                if (!ThreadInfo.ThreadReloaded) {
                    ThreadInfo.ThreadHTML = new(HtmlControl.GetHTMLBase(ThreadInfo));
                }
                else {
                    ThreadInfo.ThreadHTML = new(HtmlControl.RebuildHTML(ThreadInfo));
                    LoadExistingPostsNoHash();
                }

                // Main loop
                do {
                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;

                    // Request that will be sent to the API.
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.Data.Url);
                    // Should more headers be added?

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    //using var Response = await TryGetResponseIfModifiedAsync(Request, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var ResponseTask = TryGetResponseIfModifiedAsync(Request, CancellationToken.Token);
                    ResponseTask.Wait();
                    using var Response = ResponseTask.Result;

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode();
                        if (ThreadInfo.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Get the json.
                    //string CurrentJson = await GetStringAsync(Response, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var JsonTask = GetStringAsync(Response, CancellationToken.Token);
                    JsonTask.Wait();
                    string CurrentJson = JsonTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    FChanPost[] ThreadData;
                    try {
                        ThreadData = FChan.Generate(CurrentJson);
                    }
                    catch (Exception ex) {
                        Log.ReportException(ex);
                        ThreadInfo.CurrentActivity = ThreadStatus.FailedToParseThreadHtml;
                        this.Invoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!ThreadInfo.Data.RetrievedThreadName && !ThreadInfo.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].Subject,
                            Comment: ThreadData[0].MessageBody,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.Data.RetrievedThreadName = true;
                        ThreadInfo.ThreadHTML = ThreadInfo.ThreadHTML.Replace("<title></title>",
                            $"<title> /{ThreadInfo.Data.Board}/ - {NewName} - fchan</title>");
                        ThreadInfo.Data.HtmlThreadNameSet = true;

                        // Update the name application wide.
                        this.Invoke(() => UpdateThreadName(true));
                    }

                    // Start counting through the posts.
                    for (int PostIndex = 0; PostIndex < ThreadData.Length; PostIndex++) {
                        // Set the temporary post to the looped index post.
                        FChanPost Post = ThreadData[PostIndex];
                        if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.PostId)) {
                            GenericPost CurrentPost = new(Post) {
                                FirstPost = PostIndex == 0
                            };

                            AddNewPostNoHash(CurrentPost);
                        }

                        CancellationToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    //await DownloadFilesAsync(CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var DownloadTask = DownloadFilesAsync(CancellationToken.Token);
                    DownloadTask.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    //// check for archive flag in the post.
                    //if (ThreadData.archived) {
                    //    ThreadInfo.Data.ThreadArchived = true;
                    //    ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    //    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                    //    break;
                    //}

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Write("Exiting thread " + ThreadInfo.CurrentActivity);
        }) {
            Name = $"fchan thread {ThreadInfo.Data.Board}/{ThreadInfo.Data.Id}"
        };
    }
    private void RegisterU18chanThread() {
        this.DownloadThread = new Thread(() => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // HTML
                if (!ThreadInfo.ThreadReloaded) {
                    ThreadInfo.ThreadHTML = new(HtmlControl.GetHTMLBase(ThreadInfo));
                }
                else {
                    ThreadInfo.ThreadHTML = new(HtmlControl.RebuildHTML(ThreadInfo));
                    LoadExistingPostsNoHash();
                }

                // Main loop
                do {
                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;

                    // Request that will be sent to the API.
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.Data.Url);
                    // Should more headers be added?

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    //using var Response = await TryGetResponseIfModifiedAsync(Request, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var ResponseTask = TryGetResponseIfModifiedAsync(Request, CancellationToken.Token);
                    ResponseTask.Wait();
                    using var Response = ResponseTask.Result;
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode();
                        if (ThreadInfo.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Get the json.
                    //string CurrentJson = await GetStringAsync(Response, CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var JsonTask = GetStringAsync(Response, CancellationToken.Token);
                    JsonTask.Wait();
                    string CurrentJson = JsonTask.Result;

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    U18ChanPost[] ThreadData;
                    try {
                        ThreadData = U18Chan.Generate(CurrentJson);
                    }
                    catch (Exception ex) {
                        Log.ReportException(ex);
                        ThreadInfo.CurrentActivity = ThreadStatus.FailedToParseThreadHtml;
                        this.Invoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (!ThreadInfo.Data.RetrievedThreadName && !ThreadInfo.Data.SetCustomName) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].Subject,
                            Comment: ThreadData[0].MessageBody,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.Data.RetrievedThreadName = true;
                        ThreadInfo.ThreadHTML = ThreadInfo.ThreadHTML.Replace("<title></title>",
                            $"<title> /{ThreadInfo.Data.Board}/ - {NewName} - u18chan</title>");
                        ThreadInfo.Data.HtmlThreadNameSet = true;

                        // Update the name application wide.
                        this.Invoke(() => UpdateThreadName(true));
                    }

                    // Start counting through the posts.
                    for (int PostIndex = 0; PostIndex < ThreadData.Length; PostIndex++) {
                        // Set the temporary post to the looped index post.
                        U18ChanPost Post = ThreadData[PostIndex];
                        if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.PostId)) {
                            GenericPost CurrentPost = new(Post) {
                                FirstPost = PostIndex == 0
                            };

                            AddNewPostNoHash(CurrentPost);
                        }

                        CancellationToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    //await DownloadFilesAsync(CancellationToken.Token)
                    //    .ConfigureAwait(false);
                    var DownloadTask = DownloadFilesAsync(CancellationToken.Token);
                    DownloadTask.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    //// check for archive flag in the post.
                    //if (ThreadData.archived) {
                    //    ThreadInfo.Data.ThreadArchived = true;
                    //    ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    //    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                    //    break;
                    //}

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    CancellationToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Write("Exiting thread " + ThreadInfo.CurrentActivity);
        }) {
            Name = $"u18chan thread {ThreadInfo.Data.Board}/{ThreadInfo.Data.Id}"
        };
    }
}
