#nullable enable
namespace YChanEx;

// TODO: Set HTML into the post objects, instead of the HTML string builder.
// This is so posts can be updated when needed, and not re-build the entire HTML stringbuilder for a single thread.
// Additionally, this should make the post that gets removed from the thread non-accessible in the HTML.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using murrty.classes;
using YChanEx.Parsers;
using YChanEx.Posts;

/// <summary>
/// The form that thread downloads will be relegated to.
/// </summary>
public partial class frmDownloader : Form {
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
    public ThreadInfo ThreadInfo;

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
    /// <summary>
    /// The cancellation token that will be used to kill the main loop.
    /// </summary>
    private CancellationTokenSource ThreadToken;

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
    }
    public frmDownloader(IMainFom MainForm, ThreadInfo ThreadInfo) {
        InitializeComponent();
        this.MainFormInstance = MainForm;
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
        ThreadToken = new();
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
                MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
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
            if (File.Exists(ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile)) {
                Process.Start(ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile);
            }
        }
    }
    private void lvImages_KeyPress(object sender, KeyPressEventArgs e) {
        if (e.KeyChar == (char)Keys.Return) {
            for (int Post = 0; Post < lvImages.SelectedItems.Count; Post++) {
                if (lvImages.SelectedItems[Post].Tag is not GenericFile PostFile) {
                    continue;
                }
                if (File.Exists(ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile)) {
                    Process.Start(ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile);
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
            case ThreadStatus.NoThreadPosts:
            case ThreadStatus.FailedToParseThreadHtml:
            case ThreadStatus.ThreadUnknownError:
            case ThreadStatus.ThreadIsNotAllowed:
                ManageThread(ThreadEvent.RetryDownload);
                break;

            case ThreadStatus.ThreadUpdateName:
            case ThreadStatus.ThreadInfoNotSet:
                break;

            default:
                ManageThread(ThreadEvent.AbortDownload);
                break;
        }
    }
    private void btnOpenFolder_Click(object sender, EventArgs e) {
        if (ThreadInfo.DownloadPath == null) {
            return;
        }

        if (Directory.Exists(ThreadInfo.DownloadPath)) {
            Process.Start(ThreadInfo.DownloadPath);
        }
    }
    private void btnClose_Click(object sender, EventArgs e) {
        this.Hide();
    }
    private void mOpenThreadDownloadFolder_Click(object sender, EventArgs e) {
        if (Directory.Exists(ThreadInfo.DownloadPath)) {
            Process.Start(ThreadInfo.DownloadPath);
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
                case ChanType.FoolFuuka:
                    Clipboard.SetText(ThreadInfo.ApiLink);
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
            if (File.Exists(ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile)) {
                Process.Start(ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile);
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
            Process.Start("explorer.exe", "/select, \"" + ThreadInfo.DownloadPath + "\\" + PostFile.SavedFile + "\"");
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

                switch (ThreadInfo.Chan) {
                    #region 4chan
                    case ChanType.FourChan: {
                        ThreadInfo.Data.BoardName = Chans.GetFullBoardName(ThreadInfo);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "4chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"4chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region 7chan
                    case ChanType.SevenChan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "7chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"7chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region 8chan
                    case ChanType.EightChan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "8chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"8chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region 8kun
                    case ChanType.EightKun: {
                        if (Chans.StupidFuckingBoard(ChanType.EightKun, ThreadInfo.Data.Url)) {
                            MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadIs404);
                            this.Dispose();
                            return;
                        }
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "8kun", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"8kun thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region fchan
                    case ChanType.fchan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "fchan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"fchan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    #region u18chan
                    case ChanType.u18chan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "u18chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"u18chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                        lbLastModified.Text = "not supported";
                        lbLastModified.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                    } break;
                    #endregion

                    #region FoolFuuka
                    case ChanType.FoolFuuka when !ThreadInfo.Data.UrlHost.IsNullEmptyWhitespace(): {
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, ThreadInfo.Data.UrlHost, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"{ThreadInfo.Data.UrlHost} thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    #endregion

                    default: {
                        this.Text = "Invalid chan";
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadIs404);
                    } return;
                }

                ThreadInfo.UpdateJsonPath();
                if (ThreadInfo.DownloadPath != null) {
                    btnOpenFolder.Enabled = true;
                }
            } break;

            case ThreadEvent.StartDownload: {
                switch (ThreadInfo.Chan) {
                    case ChanType.FourChan: {
                        Register4chanThread();
                    } break;
                    case ChanType.SevenChan: {
                        Register7chanThread();
                    } break;
                    case ChanType.EightChan: {
                        Register8chanThread();
                    } break;
                    case ChanType.EightKun: {
                        Register8kunThread();
                    } break;
                    case ChanType.fchan: {
                        RegisterfchanThread();
                    } break;
                    case ChanType.u18chan: {
                        RegisterU18chanThread();
                    } break;
                    case ChanType.FoolFuuka: {
                        RegisterFoolFuukaThread();
                    } break;
                    default: {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.NoStatusSet);
                    } return;
                }

                if (DownloadThread == null) {
                    Log.Warn("Download thread was not created.");
                    return;
                }

                if (ThreadInfo.DownloadPath != null) {
                    btnOpenFolder.Enabled = true;
                }

                DownloadThread.Name = ThreadInfo.ThreadLogDisplay;
                ThreadInfo.HideModifiedLabelAt = Downloads.ScannerDelay - 10;
                MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadScanning);
                lbScanTimer.Text = "scanning now...";
                DownloadThread.Start();
            } break;

            case ThreadEvent.AfterDownload: {
                switch (ThreadInfo.CurrentActivity) {
                    case ThreadStatus.ThreadIsAborted: {
                        lbScanTimer.Text = "Aborted";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        this.Icon = Properties.Resources.ProgramIcon_Dead;
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
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

                            MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                            btnAbortRetry.Text = "Retry";
                        }
                    } break;

                    case ThreadStatus.ThreadFile404: {
                        ThreadInfo.CurrentActivity = ThreadStatus.Waiting;
                        ThreadInfo.FileWas404 = true;
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
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
                            MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                            btnAbortRetry.Text = "Rescan";
                            ThreadInfo.ThreadModified = true;
                        }
                    } break;

                    case ThreadStatus.ThreadDownloading:
                    case ThreadStatus.Waiting:
                    case ThreadStatus.ThreadNotModified: {
                        lbNotModified.Visible = ThreadInfo.CurrentActivity == ThreadStatus.ThreadNotModified;
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                        ThreadInfo.CountdownToNextScan = (ThreadInfo.Chan == ChanType.u18chan ? (60 * 30) : Downloads.ScannerDelay) - 1;
                        if (Program.DebugMode && ThreadInfo.Chan != ChanType.u18chan) {
                            ThreadInfo.CountdownToNextScan = 10;
                            //ThreadInfo.CountdownToNextScan = 99999;
                        }
                        lbScanTimer.Text = "soon (tm)";
                        ThreadInfo.CurrentActivity = ThreadStatus.Waiting;
                        tmrScan.Start();
                    } break;

                    case ThreadStatus.ThreadImproperlyDownloaded: {
                        lbScanTimer.Text = "Bad download";
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                        ThreadInfo.CountdownToNextScan = Downloads.ScannerDelay - 1;
                        if (Program.DebugMode && ThreadInfo.Chan != ChanType.u18chan) {
                            ThreadInfo.CountdownToNextScan = 10;
                            //ThreadInfo.CountdownToNextScan = 99999;
                        }
                        ThreadInfo.CurrentActivity = ThreadStatus.Waiting;
                        tmrScan.Start();
                    } break;

                    case ThreadStatus.FailedToParseThreadHtml: {
                        lbScanTimer.Text = "Failed to parse thread";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                        ThreadInfo.ThreadModified = true;
                    } break;

                    // How to handle?
                    case ThreadStatus.NoThreadPosts: {
                        lbScanTimer.Text = "No thread posts";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                        ThreadInfo.ThreadModified = true;
                    } break;

                    case ThreadStatus.ThreadIsNotAllowed: {
                        lbScanTimer.Text = "Forbidden";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                        btnAbortRetry.Text = "Retry";
                        ThreadInfo.ThreadModified = true;
                    } break;

                    case ThreadStatus.ThreadInfoNotSet: {
                        lbScanTimer.Text = "No thread info";
                        lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                    } break;
                }
            } break;

            case ThreadEvent.RestartDownload: {
                ThreadInfo.HideModifiedLabelAt = Downloads.ScannerDelay - 10;
                MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadScanning);
                lbScanTimer.Text = "scanning now...";
                ResetThread.Set();
            } break;

            case ThreadEvent.AbortDownload: {
                Debug.Print("AbortDownload called");
                tmrScan.Stop();
                ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsAborted;
                ThreadToken.Cancel();
                ResetThread.Set();

                if (threadRemovedFromForm) {
                    this.Dispose();
                    return;
                }

                this.Icon = Properties.Resources.ProgramIcon_Dead;
                lbScanTimer.Text = "Aborted";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);

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

                MainFormInstance.SetItemStatus(ThreadInfo, ThreadInfo.CurrentActivity);
                lbScanTimer.Text = "scanning now...";
                btnAbortRetry.Text = "Abort";
                tmrScan.Stop();
                ThreadToken = new();
                ResetThread.Set();
                ManageThread(ThreadEvent.StartDownload);
            } break;

            case ThreadEvent.AbortForClosing: {
                ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsAborted;
                ThreadToken.Cancel();
                ResetThread.Set();
            } break;

            case ThreadEvent.ReloadThread: {
                if (Downloads.AutoRemoveDeadThreads && ThreadInfo.Data.ThreadState switch {
                    ThreadState.ThreadIs404 => true,
                    ThreadState.ThreadIsArchived or _ when ThreadInfo.Data.ThreadArchived => true,
                    _ => false
                }) {
                    MainFormInstance.ThreadKilled(ThreadInfo);
                    return;
                }

                switch (ThreadInfo.Chan) {
                    case ChanType.FourChan: {
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "4chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"4chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.FourTwentyChan: {
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "420chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"420chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.SevenChan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "7chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"7chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.EightChan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "8chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"8chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                        ThreadInfo.ThreadUri = new(ThreadInfo.Data.Url);
                    } break;
                    case ChanType.EightKun: {
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "8kun", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"8kun thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.fchan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "fchan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"fchan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;
                    case ChanType.u18chan: {
                        lvImages.Columns.RemoveAt(3);
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, "u18chan", ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"u18chan thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                        lbLastModified.Text = "not supported";
                        lbLastModified.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                    } break;
                    case ChanType.FoolFuuka: {
                        ThreadInfo.DownloadPath = Path.Combine(Downloads.DownloadPath, ThreadInfo.Data.UrlHost, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                        this.Text = $"{ThreadInfo.Data.UrlHost} thread - {Chans.GetFullBoardName(ThreadInfo)} - {ThreadInfo.Data.DownloadFormThreadNameDisplay}";
                    } break;

                    default: {
                        ManageThread(ThreadEvent.ParseForInfo);
                    } break;
                }

                ThreadInfo.UpdateJsonPath();
                if (ThreadInfo.DownloadPath != null) {
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

            string FilePath = Path.Combine(ThreadInfo.DownloadPath, PostFile.SavedFile);
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }

            string ThumbnailPath = Path.Combine(ThreadInfo.DownloadPath, PostFile.SavedThumbnailFile);
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
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.FourTwentyChan:
                ThreadNameBuffer = "420chan thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.SevenChan:
                ThreadNameBuffer = "7chan thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.EightChan:
                ThreadNameBuffer = "8chan thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                }
                break;
            case ChanType.EightKun:
                ThreadNameBuffer = "8kun thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                }
                break;
            case ChanType.fchan:
                ThreadNameBuffer = "fchan thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.u18chan:
                ThreadNameBuffer = "u18chan thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, Chans.GetFullBoardName(ThreadInfo), ThreadInfo.Data.Id);
                }
                break;
            case ChanType.FoolFuuka:
                ThreadNameBuffer = ThreadInfo.Data.UrlHost + " thread - {0} - {1}";
                if (ThreadInfo.Data.ThreadName != null) {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.ThreadName);
                    if (ApplyToMainForm && ThreadInfo.Data.CustomThreadName == null) {
                        MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadUpdateName);
                    }
                }
                else {
                    this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                }
                break;
            default:
                this.Text = string.Format(ThreadNameBuffer, ThreadInfo.Data.Board, ThreadInfo.Data.Id);
                return;
        }
    }
    private string GetFileSuffix(GenericFile CurrentFile) {
        // replace any invalid file name characters.
        // some linux nerds can have invalid windows file names as file names
        // so we gotta filter them.
        string ExpectedFullFile = FileHandler.ReplaceIllegalCharacters(CurrentFile.OriginalFileName ?? string.Empty +
            "." + CurrentFile.FileExtension).ToLowerInvariant();

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
    private void HandleBadFileName(GenericFile File) {
        Log.Warn("Could not handle file " + File.FileId);
        this.Invoke(() => File.ListViewItem.ImageIndex = ErrorImage);
    }
    private void HandleBadThumbFileName(GenericFile File) {
        Log.Warn("Could not handle thumb file " + File.FileId);
        this.Invoke(() => File.ListViewItem.ImageIndex = ErrorImage);
    }
    private void HandleStatusCode(HttpStatusCode StatusCode) {
        ThreadInfo.CurrentActivity = StatusCode switch {
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
        lbLastModified.Text = "last modified: " + (ThreadInfo.Data.LastModified.HasValue ? ThreadInfo.Data.LastModified.ToString() : "not supported");
    }
    private void PrepareDownload() {
        this.Invoke(() => {
            UpdateCounts();
            lbScanTimer.Text = "Downloading files";
            MainFormInstance.SetItemStatus(ThreadInfo, ThreadStatus.ThreadDownloading);
        });
    }
    private void AddNewPost(GenericPost CurrentPost) {
        if (CurrentPost.HasFiles) {
            for (int FileIndex = 0; FileIndex < CurrentPost.PostFiles.Count; FileIndex++) {
                var CurrentFile = CurrentPost.PostFiles[FileIndex];
                string FileName = (Downloads.SaveOriginalFilenames ? CurrentFile.OriginalFileName : CurrentFile.FileId)!;
                string Suffix = GetFileSuffix(CurrentFile);
                string ThumbFileName = CurrentFile.ThumbnailFileName!;
                CurrentFile.SavedFileName = FileName + Suffix;
                CurrentFile.SavedThumbnailFile = ThumbFileName;

                if (!Downloads.AllowFileNamesGreaterThan255) {
                    int FileNameLength = ThreadInfo.DownloadPath.Length +
                        FileName.Length +
                        CurrentFile.FileExtension!.Length +
                        Suffix.Length +
                        2; // ext period (1) and download path separator (1)

                    if (FileNameLength > 255) {
                        int TrimSize = FileNameLength - 255;
                        if (FileName.Length <= TrimSize) {
                            HandleBadFileName(CurrentFile);
                            return;
                        }
                        FileName = FileName[..^TrimSize];
                    }

                    if (Downloads.SaveThumbnails) {
                        int ThumbFileNameLength = ThreadInfo.DownloadPath.Length +
                            ThumbFileName.Length +
                            CurrentFile.ThumbnailFileExtension!.Length +
                            8; // ext period (1), path separators (2), "thumb" (5)

                        if (ThumbFileNameLength > 255) {
                            int TrimSize = ThumbFileNameLength - 255;
                            if (ThumbFileName.Length <= TrimSize) {
                                HandleBadThumbFileName(CurrentFile);
                                return;
                            }
                            ThumbFileName = ThumbFileName[..^TrimSize];
                        }
                    }
                }

                CurrentFile.SavedFile = FileName + Suffix + "." + CurrentFile.FileExtension;
                CurrentFile.SavedThumbnailFile = Path.Combine("thumb", ThumbFileName + "." + CurrentFile.ThumbnailFileExtension);

                // add a new listviewitem to the listview for this image.
                ListViewItem lvi = new();
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.Name = CurrentFile.FileId;
                lvi.SubItems[0].Text = CurrentFile.FileId;
                lvi.SubItems[1].Text = CurrentFile.FileExtension;
                lvi.SubItems[2].Text = CurrentFile.OriginalFileName + "." + CurrentFile.FileExtension;
                lvi.SubItems[3].Text = CurrentFile.FileHash;
                lvi.ImageIndex = WaitingImage;
                lvi.Tag = CurrentFile;
                CurrentFile.ListViewItem = lvi;
                this.Invoke(() => lvImages.Items.Add(lvi));

                ThreadInfo.Data.ThreadImagesCount++;
                ThreadInfo.Data.ThreadPostsCount++;
                ThreadInfo.Data.EstimatedSize += CurrentFile.FileSize;
            }
        }

        // add the new post to the data.
        ThreadInfo.Data.ParsedPostIds.Add(CurrentPost.PostId);
        ThreadInfo.Data.ThreadPosts.Add(CurrentPost);
        ThreadInfo.AddedNewPosts = ThreadInfo.ThreadModified = true;
    }
    private void AddNewPostNoHash(GenericPost CurrentPost) {
        if (CurrentPost.HasFiles) {
            for (int FileIndex = 0; FileIndex < CurrentPost.PostFiles.Count;  FileIndex++) {
                var CurrentFile = CurrentPost.PostFiles[FileIndex];
                string FileName = (Downloads.SaveOriginalFilenames ? CurrentFile.OriginalFileName : CurrentFile.FileId)!;
                string Suffix = GetFileSuffix(CurrentFile);
                string ThumbFileName = CurrentFile.ThumbnailFileName!;
                CurrentFile.SavedFileName = FileName + Suffix;
                CurrentFile.SavedThumbnailFile = ThumbFileName;

                if (!Downloads.AllowFileNamesGreaterThan255) {
                    int FileNameLength = ThreadInfo.DownloadPath.Length +
                        FileName.Length +
                        CurrentFile.FileExtension!.Length +
                        Suffix.Length +
                        2; // ext period (1) and download path separator (1)

                    if (FileNameLength > 255) {
                        int TrimSize = FileNameLength - 255;
                        if (FileName.Length <= TrimSize) {
                            HandleBadFileName(CurrentFile);
                            return;
                        }
                        FileName = FileName[..^TrimSize];
                    }

                    if (Downloads.SaveThumbnails) {
                        int ThumbFileNameLength = ThreadInfo.DownloadPath.Length +
                            ThumbFileName.Length +
                            CurrentFile.ThumbnailFileExtension!.Length +
                            8; // ext period (1), path separators (2), "thumb" (5)

                        if (ThumbFileNameLength > 255) {
                            int TrimSize = ThumbFileNameLength - 255;
                            if (ThumbFileName.Length <= TrimSize) {
                                HandleBadThumbFileName(CurrentFile);
                                return;
                            }
                            ThumbFileName = ThumbFileName[..^TrimSize];
                        }
                    }
                }

                CurrentFile.SavedFile = FileName + Suffix + "." + CurrentFile.FileExtension;
                CurrentFile.SavedThumbnailFile = Path.Combine("thumb", ThumbFileName + "." + CurrentFile.ThumbnailFileExtension);

                // add a new listviewitem to the listview for this image.
                ListViewItem lvi = new();
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                lvi.Name = CurrentFile.FileId;
                lvi.SubItems[0].Text = CurrentFile.FileId;
                lvi.SubItems[1].Text = CurrentFile.FileExtension;
                lvi.SubItems[2].Text = CurrentFile.OriginalFileName + "." + CurrentFile.FileExtension;
                lvi.ImageIndex = WaitingImage;
                lvi.Tag = CurrentFile;
                CurrentFile.ListViewItem = lvi;
                this.Invoke(() => lvImages.Items.Add(lvi));

                ThreadInfo.Data.ThreadImagesCount++;
                ThreadInfo.Data.ThreadPostsCount++;
                ThreadInfo.Data.EstimatedSize += CurrentFile.FileSize;
            }
        }

        // add the new post to the data.
        ThreadInfo.Data.ParsedPostIds.Add(CurrentPost.PostId);
        ThreadInfo.Data.ThreadPosts.Add(CurrentPost);
        ThreadInfo.AddedNewPosts = ThreadInfo.ThreadModified = true;
    }
    private void LoadExistingPosts() {
        if (ThreadInfo.Data.ThreadPosts.Count > 0) {
            for (int i = 0; i < ThreadInfo.Data.ThreadPosts.Count; i++) {
                GenericPost CurrentPost = ThreadInfo.Data.ThreadPosts[i];
                if (CurrentPost.HasFiles) {
                    for (int x = 0; x < CurrentPost.PostFiles.Count; x++) {
                        GenericFile CurrentFile = CurrentPost.PostFiles[x];
                        ListViewItem lvi = new();
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.Name = CurrentFile.FileId;
                        lvi.SubItems[0].Text = CurrentFile.FileId;
                        lvi.SubItems[1].Text = CurrentFile.FileExtension;
                        lvi.SubItems[2].Text = CurrentFile.OriginalFileName + "." + CurrentFile.FileExtension;
                        lvi.SubItems[3].Text = CurrentFile.FileHash;
                        lvi.ImageIndex = CurrentFile.Status switch {
                            FileDownloadStatus.Downloaded =>
                                File.Exists(Path.Combine(ThreadInfo.DownloadPath, CurrentFile.SavedFile)) ?
                                    ReloadedDownloadedImage : ReloadedMissingImage,
                            FileDownloadStatus.Error => ErrorImage,
                            FileDownloadStatus.FileNotFound => _404Image,
                            _ => WaitingImage
                        };
                        lvi.Tag = CurrentFile;
                        CurrentFile.ListViewItem = lvi;
                        this.Invoke(() => lvImages.Items.Add(lvi));
                    }
                }
            }

            this.Invoke(() => UpdateCounts());
        }
    }
    private void LoadExistingPostsNoHash() {
        if (ThreadInfo.Data.ThreadPosts.Count > 0) {
            for (int i = 0; i < ThreadInfo.Data.ThreadPosts.Count; i++) {
                GenericPost CurrentPost = ThreadInfo.Data.ThreadPosts[i];
                if (CurrentPost.HasFiles) {
                    for (int x = 0; x < CurrentPost.PostFiles.Count; x++) {
                        GenericFile CurrentFile = CurrentPost.PostFiles[x];
                        ListViewItem lvi = new();
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.Name = CurrentFile.FileId;
                        lvi.SubItems[0].Text = CurrentFile.FileId;
                        lvi.SubItems[1].Text = CurrentFile.FileExtension;
                        lvi.SubItems[2].Text = CurrentFile.OriginalFileName + "." + CurrentFile.FileExtension;
                        lvi.ImageIndex = CurrentFile.Status switch {
                            FileDownloadStatus.Downloaded =>
                                File.Exists(Path.Combine(ThreadInfo.DownloadPath, CurrentFile.SavedFile)) ?
                                    ReloadedDownloadedImage : ReloadedMissingImage,
                            FileDownloadStatus.Error => ErrorImage,
                            FileDownloadStatus.FileNotFound => _404Image,
                            _ => WaitingImage
                        };
                        lvi.Tag = CurrentFile;
                        CurrentFile.ListViewItem = lvi;
                        this.Invoke(() => lvImages.Items.Add(lvi));
                    }
                }
            }

            this.Invoke(() => UpdateCounts());
        }
    }
    private void ThreadReloaded() {
        if (Downloads.SaveHTML && !ThreadInfo.HtmlExists) {
            ThreadInfo.SaveHtml();
        }
    }

    private async Task DownloadFilesAsync(VolatileHttpClient DownloadClient, CancellationToken token) {
        Log.Info("Downloading files");

        // Save the thread data now.
        if (General.AutoSaveThreads) {
            ThreadInfo.SaveThread();
            ThreadInfo.ThreadModified = false;
        }

        if (!ThreadInfo.AddedNewPosts) {
            return;
        }

        ThreadInfo.CurrentActivity = ThreadStatus.ThreadDownloading;
        ThreadInfo.DownloadingFiles = true;

        string ThumbnailDirectory = Path.Combine(ThreadInfo.DownloadPath, "thumb");

        if (!Directory.Exists(ThreadInfo.DownloadPath)) {
            Directory.CreateDirectory(ThreadInfo.DownloadPath);
        }
        if (Downloads.SaveThumbnails && !Directory.Exists(ThumbnailDirectory)) {
            Directory.CreateDirectory(ThumbnailDirectory);
        }

        if (Downloads.SaveHTML) {
            ThreadInfo.CheckQuotes();
            ThreadInfo.SaveHtml();
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
                    token.ThrowIfCancellationRequested();

                    // Set the icon in the list to "Downloading".
                    this.Invoke(() => PostFile.ListViewItem.ImageIndex = DownloadingImage);

                    string FileDownloadPath = Path.Combine(ThreadInfo.DownloadPath, PostFile.SavedFile);
                    string ThumbFileDownloadPath = Path.Combine(ThreadInfo.DownloadPath, PostFile.SavedThumbnailFile);

                    // Check for existing files.
                    if (!File.Exists(FileDownloadPath)) {
                        HttpRequestMessage FileRequest = new(HttpMethod.Get, PostFile.FileUrl!);
                        if (ThreadInfo.Chan == ChanType.EightChan) {
                            FileRequest.Headers.Add("Referer", ThreadInfo.Data.Url);
                        }
                        using var Response = await DownloadClient.GetResponseAsync(FileRequest, token);
                        token.ThrowIfCancellationRequested();

                        if (Response == null) {
                            PostFile.Status = FileDownloadStatus.Error;
                            this.Invoke(() => PostFile.ListViewItem.ImageIndex = ErrorImage);
                        }
                        else if (!Response.IsSuccessStatusCode) {
                            if (Response.StatusCode == HttpStatusCode.NotFound) {
                                PostFile.Status = FileDownloadStatus.FileNotFound;
                                this.Invoke(() => PostFile.ListViewItem.ImageIndex = _404Image);
                            }
                            else {
                                PostFile.Status = FileDownloadStatus.Error;
                                this.Invoke(() => PostFile.ListViewItem.ImageIndex = ErrorImage);
                            }
                        }
                        else {
                            await DownloadClient.DownloadFileAsync(Response, FileDownloadPath, token);
                            token.ThrowIfCancellationRequested();

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
                        using var Response = await DownloadClient.GetResponseAsync(FileRequest, token);
                        if (Response?.IsSuccessStatusCode == true) {
                            await DownloadClient.DownloadFileAsync(Response, ThumbFileDownloadPath, token);
                        }
                    }

                    // Sleep for 100ms.
                    Thread.Sleep(100);
                }
            }
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
                ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, FourChan.GetHtmlTitle(ThreadInfo.Data));
                ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                if (ThreadInfo.ThreadReloaded) {
                    LoadExistingPosts();
                    ThreadReloaded();
                }
                ThreadToken.Token.ThrowIfCancellationRequested();

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    using var JsonStream = await DownloadClient.GetStringStreamAsync(Response);

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = JsonStream.JsonDeserialize<FourChanThread>();
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.posts.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.posts[0].sub,
                            Comment: ThreadData.posts[0].com,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{FourChan.GetHtmlTitle(ThreadInfo.Data.Board, NewName)}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
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

                        ThreadToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

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
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
    private void Register7chanThread() {
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
                ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, SevenChan.GetHtmlTitle(ThreadInfo.Data));
                ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                if (ThreadInfo.ThreadReloaded) {
                    LoadExistingPostsNoHash();
                    ThreadReloaded();
                }
                ThreadToken.Token.ThrowIfCancellationRequested();

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    string ThreadHtml = await DownloadClient.GetStringAsync(Response, ThreadToken.Token);

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = SevenChan.TryGenerate(ThreadHtml);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].Subject,
                            Comment: ThreadData[0].MessageBody,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{SevenChan.GetHtmlTitle(ThreadInfo.Data.Board, NewName)}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
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

                        ThreadToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

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
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
    private void Register8chanThread() {
        this.DownloadThread = new Thread(async () => {
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
                    Log.Info($"Retrieving 8chan board info for {ThreadInfo.Data.Board}");
                    var Board = await EightChan.GetBoardAsync(ThreadInfo.Data.Board, Networking.LatestClient, ThreadToken.Token);
                    if (Board != null) {
                        ThreadInfo.Data.BoardName = Board.BoardName;
                        ThreadInfo.Data.BoardSubtitle = Board.BoardDescription;
                    }
                    else {
                        Log.Warn("Could not get the board name.");
                    }
                    ThreadInfo.ThreadModified = true;
                    ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo);
                    ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                }
                else {
                    ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, EightChan.GetHtmlTitle(ThreadInfo.Data));
                    ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                    LoadExistingPostsNoHash();
                    ThreadReloaded();
                }
                ThreadToken.Token.ThrowIfCancellationRequested();

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    var JsonStream = await DownloadClient.GetStringStreamAsync(Response);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = JsonStream.JsonDeserialize<EightChanThread>();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.subject,
                            Comment: ThreadData.message,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{EightChan.GetHtmlTitle(ThreadInfo.Data.Board, NewName)}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
                    }

                    // Parse the first post
                    if (!ThreadInfo.Data.ParsedPostIds.Contains(ThreadData.threadId)) {
                        GenericPost CurrentPost = new(ThreadData, ThreadInfo) {
                            FirstPost = true,
                        };

                        AddNewPostNoHash(CurrentPost);
                    }

                    ThreadToken.Token.ThrowIfCancellationRequested();

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

                            ThreadToken.Token.ThrowIfCancellationRequested();
                        }
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

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
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
    // Needs: Help. Like psychological help. Unused currently, 8kun dead.
    private void Register8kunThread() {
        this.DownloadThread = new Thread(async () => {
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
                    Log.Info($"Retrieving 8kun board info for {ThreadInfo.Data.Board}");
                    var Board = await EightKun.GetBoardAsync(ThreadInfo, Networking.LatestClient, ThreadToken.Token);
                    if (Board != null) {
                        ThreadInfo.Data.BoardName = Board.title;
                        ThreadInfo.Data.BoardSubtitle = Board.subtitle;
                    }
                    else {
                        Log.Warn("Could not get the board name.");
                    }
                    ThreadInfo.ThreadModified = true;
                    ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo);
                    ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                }
                else {
                    ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, EightKun.GetHtmlTitle(ThreadInfo.Data));
                    ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                    LoadExistingPosts();
                    ThreadReloaded();
                }
                ThreadToken.Token.ThrowIfCancellationRequested();

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    var JsonStream = await DownloadClient.GetStringStreamAsync(Response);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = JsonStream.JsonDeserialize<EightKunThread>();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData.posts[0].sub,
                            Comment: ThreadData.posts[0].com,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{EightKun.GetHtmlTitle(ThreadInfo.Data.Board, NewName)}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
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

                            ThreadToken.Token.ThrowIfCancellationRequested();
                        }
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

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
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
    private void RegisterfchanThread() {
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
                ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, FChan.GetHtmlTitle(ThreadInfo.Data));
                ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                if (ThreadInfo.ThreadReloaded) {
                    LoadExistingPostsNoHash();
                    ThreadReloaded();
                }
                ThreadToken.Token.ThrowIfCancellationRequested();

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    string ThreadHtml = await DownloadClient.GetStringAsync(Response, ThreadToken.Token);

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = FChan.TryGenerate(ThreadHtml);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].Subject,
                            Comment: ThreadData[0].MessageBody,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{FChan.GetHtmlTitle(ThreadInfo.Data.Board, NewName)}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
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

                        ThreadToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

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
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
    private void RegisterU18chanThread() {
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
                ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, U18Chan.GetHtmlTitle(ThreadInfo.Data));
                ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                if (ThreadInfo.ThreadReloaded) {
                    LoadExistingPostsNoHash();
                    ThreadReloaded();
                }
                ThreadToken.Token.ThrowIfCancellationRequested();

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    string ThreadHtml = await DownloadClient.GetStringAsync(Response, ThreadToken.Token);

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = U18Chan.TryGenerate(ThreadHtml);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].Subject,
                            Comment: ThreadData[0].MessageBody,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{U18Chan.GetHtmlTitle(ThreadInfo.Data.Board, NewName)}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
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

                        ThreadToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

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
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
    private void RegisterFoolFuukaThread() {
        this.DownloadThread = new Thread(async () => {
            try {
                // Check the thread board and id for null value
                // Can't really parse the API without them.
                if (string.IsNullOrWhiteSpace(ThreadInfo.Data.Board) || string.IsNullOrWhiteSpace(ThreadInfo.Data.Id) || string.IsNullOrWhiteSpace(ThreadInfo.Data.UrlHost)) {
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                    ManageThread(ThreadEvent.AfterDownload);
                    return;
                }

                // Main loop
                do {
                    Log.Info($"Scanning {ThreadInfo.ThreadLogDisplay}");

                    // Set the activity to scanning.
                    ThreadInfo.CurrentActivity = ThreadStatus.ThreadScanning;
                    VolatileHttpClient DownloadClient = Networking.LatestClient;
                    HttpRequestMessage Request = new(HttpMethod.Get, ThreadInfo.ApiLink);
                    Request.Headers.IfModifiedSince = ThreadInfo.Data.LastModified;

                    // Try to get the response.
                    this.Invoke(() => lbScanTimer.Text = "Downloading thread data...");
                    using var Response = await DownloadClient.GetResponseAsync(Request, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the response is null, it's a bad result; break the thread.
                    if (Response == null) {
                        HandleStatusCode(HttpStatusCode.NoContent);
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check the status code, if it's bad it cannot be used.
                    if (!Response.IsSuccessStatusCode) {
                        HandleStatusCode(Response.StatusCode);
                        if (Response.StatusCode == HttpStatusCode.NotModified) {
                            ThreadInfo.CurrentActivity = ThreadStatus.ThreadNotModified;
                            Log.Info($"{ThreadInfo.ThreadLogDisplay} not modified, waiting for next loop.");
                            this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                            ResetThread.Reset();
                            ResetThread.Wait();
                            continue;
                        }
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Save the last modified time.
                    ThreadInfo.ThreadModified = ThreadInfo.Data.LastModified != Response.Content.Headers.LastModified;
                    ThreadInfo.Data.LastModified = Response.Content.Headers.LastModified;

                    // Get the json.
                    var JsonStream = await DownloadClient.GetStringStreamAsync(Response);

                    // Serialize the json data into a class object.
                    this.Invoke(() => lbScanTimer.Text = "Parsing thread...");
                    var ThreadData = FoolFuuka.Deserialize(JsonStream);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the posts length is 0, there are no posts. No 404, must be improperly downloaded.
                    if (ThreadData is null || ThreadData.Length < 1) {
                        ThreadInfo.CurrentActivity = ThreadStatus.NoThreadPosts;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }

                    // Check for the board name
                    if (ThreadInfo.Data.BoardName == null) {
                        ThreadInfo.Data.BoardName = ThreadData[0].board!.name;

                        // HTML
                        ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo);
                        ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                        if (ThreadInfo.ThreadReloaded) {
                            LoadExistingPosts();
                            ThreadReloaded();
                        }
                    }
                    else if (ThreadInfo.ThreadReloaded) {
                        ThreadInfo.ThreadTopHtml = HtmlControl.GetHTMLBase(ThreadInfo, FoolFuuka.GetHtmlTitle(ThreadInfo.Data));
                        ThreadInfo.ThreadBottomHtml = HtmlControl.GetHTMLFooter(ThreadInfo);
                    }
                    else {
                        ThreadInfo.CurrentActivity = ThreadStatus.ThreadUnknownError;
                        this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                        break;
                    }
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // Checks if the thread name has been retrieved, and retrieves it if not.
                    // It was supposed to be an option, but honestly, it's not a problematic inclusion.
                    if (ThreadInfo.Data.ThreadName == null) {
                        // NewName is the name that will be used to ID the thread.
                        // If the comment doesn't exist, it'll just use the ID & URL.
                        // If the length is 0, override the set info with the ID & URL.
                        string NewName = FileHandler.GetShortThreadName(
                            Subtitle: ThreadData[0].title,
                            Comment: ThreadData[0].comment_sanitized,
                            FallbackName: ThreadInfo.Data.Id);

                        // Update the data with the new name.
                        ThreadInfo.Data.ThreadName = NewName;
                        ThreadInfo.ThreadTopHtml = ThreadInfo.ThreadTopHtml.ReplaceFirst("<title></title>",
                            $"<title>{FoolFuuka.GetHtmlTitle(ThreadInfo.Data.Board, NewName, Networking.GetHostNameOnly(ThreadInfo.Data.Url))}</title>");

                        // Async invoke on the UI form now, in case wonkiness occurs.
                        this.BeginInvoke(() => {
                            // Add/update history
                            DownloadHistory.AddOrUpdate(ThreadInfo.Chan, ThreadInfo.Data.Url, ThreadInfo.Data.ThreadName, MainFormInstance);

                            // Update the name application wide, if the custom name wasn't set.
                            if (ThreadInfo.Data.CustomThreadName == null) {
                                UpdateThreadName(true);
                            }
                        });
                    }

                    // Start counting through the posts.
                    for (int PostIndex = 0; PostIndex < ThreadData.Length; PostIndex++) {
                        // Set the temporary post to the looped index post.
                        FoolFuukaPost Post = ThreadData[PostIndex];
                        if (!ThreadInfo.Data.ParsedPostIds.Contains(Post.num)) {
                            GenericPost CurrentPost = new(Post) {
                                FirstPost = PostIndex == 0
                            };

                            AddNewPost(CurrentPost);
                        }

                        ThreadToken.Token.ThrowIfCancellationRequested();
                    }

                    // update the form totals and status.
                    PrepareDownload();

                    // Download files.
                    Log.Info($"Downloading {ThreadInfo.ThreadLogDisplay} files.");
                    await DownloadFilesAsync(DownloadClient, ThreadToken.Token);
                    ThreadToken.Token.ThrowIfCancellationRequested();

                    // If the thread is aborted, just break the loop -- its already managed.
                    if (ThreadInfo.CurrentActivity == ThreadStatus.ThreadIsAborted) {
                        break;
                    }

                    //// check for archive flag in the post.
                    //if (ThreadData[0].archived) {
                    //    ThreadInfo.Data.ThreadArchived = true;
                    //    ThreadInfo.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    //    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));
                    //    break;
                    //}

                    // Set the activity.
                    ThreadInfo.CurrentActivity = ThreadStatus.Waiting;

                    // Invoke the post-download management.
                    Log.Info($"{ThreadInfo.ThreadLogDisplay} has finished scan, waiting for next loop.");
                    this?.BeginInvoke(() => ManageThread(ThreadEvent.AfterDownload));

                    // Synchronously wait, since this thread is separate.
                    ResetThread.Reset();
                    ResetThread.Wait();
                    ThreadToken.Token.ThrowIfCancellationRequested();
                } while (ThreadInfo.CurrentActivity is not (ThreadStatus.ThreadIs404 or ThreadStatus.ThreadIsAborted or ThreadStatus.ThreadIsArchived));
            }
            catch (ThreadAbortException) { }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            Log.Info($"Exiting thread {ThreadInfo.ThreadLogDisplay}");
        });
    }
}
