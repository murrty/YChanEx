using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace YChanEx {
    public partial class frmDownloader : Form {

        #region Variables
        frmMain MainFormInstance = Program.GetMainFormInstance();   // all, the instance of the main for for modifying it
        // when anything major changes in the download form.

        public ThreadInfo CurrentThread;                            // all, the ThreadInfo relating to the current thread.

        public ThreadStatus LastStatus;

        private Thread DownloadThread;              // all, the main download thread.
        private Thread TimerIdle;                   // all, the timer idler for when the settings form is open.

        // Mostly-debug
        private bool MessageBoxPerFile = false;         // all, debug to display a message box of the URL before download
        private bool PauseBetweenFiles = true;         // all, temp pauses between file downloads.
        #endregion

        #region Form Controls
        public frmDownloader() {
            InitializeComponent();
            System.Diagnostics.Debug.Print("Created download form");
            if (Program.IsDebug) {
                btnForce404.Enabled = true;
                btnForce404.Visible = true;
                btnPauseTimer.Enabled = true;
                btnPauseTimer.Visible = true;
            }
            ilStatus.Images.Add(Properties.Resources.waiting);
            ilStatus.Images.Add(Properties.Resources.downloading);
            ilStatus.Images.Add(Properties.Resources.finished);
            ilStatus.Images.Add(Properties.Resources.errored);
            lvImages.SmallImageList = ilStatus;
        }
        private void frmDownloader_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }
        private void tmrScan_Tick(object sender, EventArgs e) {
            if (Program.SettingsOpen) {
                TimerIdle = new Thread(() => {
                    try {
                        Thread.Sleep(5000);
                        this.BeginInvoke(new MethodInvoker(() => {
                            tmrScan.Start();
                        }));
                    }
                    catch (ThreadAbortException) {
                        return;
                    }
                });
                TimerIdle.Name = "Idling timer for Settings";
                TimerIdle.Start();
                tmrScan.Stop();
                return;
            }

            if (CurrentThread.CountdownToNextScan == 0) {
                tmrScan.Stop();
                btnPauseTimer.Enabled = false;
                ManageThread(ThreadEvent.StartDownload);
            }
            else {
                if (CurrentThread.CountdownToNextScan == CurrentThread.HideModifiedLabelAt) {
                    lbNotModified.Visible = false;
                }
                btnPauseTimer.Enabled = true;
                lbScanTimer.Text = CurrentThread.CountdownToNextScan.ToString();
                CurrentThread.CountdownToNextScan--;
            }
        }
        private void lvImages_MouseDoubleClick(object sender, MouseEventArgs e) {
            for (int i = 0; i < lvImages.SelectedItems.Count; i++) {
                if (File.Exists(CurrentThread.DownloadPath + "\\" + CurrentThread.FileNames[lvImages.SelectedItems[i].Index])) {
                    System.Diagnostics.Process.Start(CurrentThread.DownloadPath + "\\" + CurrentThread.FileNames[lvImages.SelectedItems[i].Index]);
                }
            }
        }
        private void btnForce404_Click(object sender, EventArgs e) {
            if (Program.IsDebug) {
                tmrScan.Stop();
                if (DownloadThread != null && DownloadThread.IsAlive) {
                    DownloadThread.Abort();
                }

                CurrentThread.CurrentActivity = ThreadStatus.ThreadIs404;
                btnForce404.Enabled = false;
                ManageThread(ThreadEvent.AfterDownload);
            }
        }
        private void btnAbortRetry_Click(object sender, EventArgs e) {
            switch (CurrentThread.CurrentActivity) {
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
            if (CurrentThread.DownloadPath == null) { return; }

            if (Directory.Exists(CurrentThread.DownloadPath)) {
                System.Diagnostics.Process.Start(CurrentThread.DownloadPath);
            }
        }
        private void btnClose_Click(object sender, EventArgs e) {
            this.Hide();
        }

        #region cmThreadActions
        private void mOpenThreadDownloadFolder_Click(object sender, EventArgs e) {
            if (System.IO.Directory.Exists(CurrentThread.DownloadPath)) {
                System.Diagnostics.Process.Start(CurrentThread.DownloadPath);
            }
        }

        private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
            if (CurrentThread.ThreadURL != null) {
                System.Diagnostics.Process.Start(CurrentThread.ThreadURL);
            }
        }

        private void mCopyThreadID_Click(object sender, EventArgs e) {
            if (CurrentThread.ThreadID != null) {
                Clipboard.SetText(CurrentThread.ThreadID);
            }
        }

        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (CurrentThread.ThreadURL != null) {
                Clipboard.SetText(CurrentThread.ThreadURL);
            }
        }
        #endregion

        #endregion

        #region Custom Thread Methods
        public void ManageThread(ThreadEvent Event) {
            switch (Event) {
                #region ParseForInfo
                case ThreadEvent.ParseForInfo:
                    Debug.Print("ParseThreadForInfo called");
                    string[] URLSplit = CurrentThread.ThreadURL.Split('/');

                    switch (CurrentThread.Chan) {
                        case ChanType.FourChan:
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                            CurrentThread.BoardName = BoardTitles.FourChan(CurrentThread.ThreadBoard);
                            this.Text = string.Format("4chan thread - {0} - {1}", CurrentThread.BoardName, CurrentThread.ThreadID);
                            CurrentThread.ThreadHTML = ChanHTML.GetHTMLBase(CurrentThread.Chan, CurrentThread);

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\4chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                        case ChanType.FourTwentyChan:
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 4];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 2].Split('#')[0];
                            this.Text = string.Format("420chan thread - {0} - {1}", BoardTitles.FourTwentyChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                        case ChanType.SevenChan:
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "");
                            this.Text = string.Format("7chan thread - {0} - {1}", BoardTitles.SevenChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\7chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                        case ChanType.EightChan:
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                            this.Text = string.Format("8chan thread - {0} - {1}", BoardTitles.EightChan(CurrentThread.ThreadBoard, false), CurrentThread.ThreadID);

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\8chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                        case ChanType.EightKun:
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                            this.Text = string.Format("8kun thread - {0} - {1}", BoardTitles.EightKun(CurrentThread.ThreadBoard, false), CurrentThread.ThreadID);

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\8kun\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                        case ChanType.fchan:
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "");
                            this.Text = string.Format("fchan thread - {0} - {1}", BoardTitles.fchan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                            CurrentThread.ThreadCookieContainer = new CookieContainer();
                            CurrentThread.ThreadCookieContainer.Add(new Cookie("disclaimer", "seen") { Domain = "fchan.us" });

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\fchan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                        case ChanType.u18chan:
                            lvImages.Columns.RemoveAt(3);
                            CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                            CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                            this.Text = string.Format("u18chan thread - {0} - {1}", BoardTitles.u18chan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);

                            CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\u18chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            break;
                    }

                    if (CurrentThread.DownloadPath != null) {
                        btnOpenFolder.Enabled = true;
                    }
                    break;
                #endregion

                #region StartDownload
                case ThreadEvent.StartDownload:
                    switch (CurrentThread.Chan) {
                        case ChanType.FourChan:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\4chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\4chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            Set4chanThread();
                            break;
                        case ChanType.FourTwentyChan:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\420chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            Set420chanThread();
                            break;
                        case ChanType.SevenChan:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\7chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\7chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            Set7chanThread();
                            break;
                        case ChanType.EightChan:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\8chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\8chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            Set8chanThread();
                            break;
                        case ChanType.EightKun:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\8kun\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\8kun\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            Set8kunThread();
                            break;
                        case ChanType.fchan:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\fchan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\fchan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            SetFchanThread();
                            break;
                        case ChanType.u18chan:
                            if (CurrentThread.DownloadPath != Downloads.Default.DownloadPath + "\\u18chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                                CurrentThread.DownloadPath = Downloads.Default.DownloadPath + "\\u18chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                            }
                            Setu18ChanThread();
                            break;

                        default:
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.NoStatusSet);
                            return;
                    }

                    if (CurrentThread.DownloadPath != null) {
                        btnOpenFolder.Enabled = true;
                    }

                    CurrentThread.HideModifiedLabelAt = Downloads.Default.ScannerDelay - 10;
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;
                    MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadScanning);
                    lbScanTimer.Text = "scanning now...";
                    DownloadThread.Start();
                    break;
                #endregion

                #region AfterDownload
                case ThreadEvent.AfterDownload:
                    switch (CurrentThread.CurrentActivity) {
                        case ThreadStatus.ThreadIsAborted:
                            lbScanTimer.Text = "Aborted";
                            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                            this.Icon = Properties.Resources.YChanEx404;

                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);
                            btnAbortRetry.Text = "Retry";
                            break;

                        case ThreadStatus.ThreadIs404:
                            lbScanTimer.Text = "404'd";
                            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                            this.Icon = Properties.Resources.YChanEx404;

                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);
                            btnAbortRetry.Text = "Retry";
                            break;

                        case ThreadStatus.ThreadFile404:
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            CurrentThread.FileWas404 = true;
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);
                            CurrentThread.CountdownToNextScan = Downloads.Default.ScannerDelay - 1;
                            if (Program.IsDebug) {
                                CurrentThread.CountdownToNextScan = 9;
                            }
                            lvImages.Items[CurrentThread.DownloadedImagesCount].ImageIndex = 3;
                            if (CurrentThread.RetryCountFor404 == 4) {
                                CurrentThread.RetryCountFor404 = 0;
                                CurrentThread.FileWas404 = true;
                                CurrentThread.DownloadedImagesCount++;
                                lbScanTimer.Text = "File 404, skipping";
                            }
                            else {
                                CurrentThread.RetryCountFor404++;
                                lbScanTimer.Text = "File 404, retrying";
                            }
                            tmrScan.Start();
                            break;

                        case ThreadStatus.ThreadIsArchived:
                            lbScanTimer.Text = "Archived";
                            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                            this.Icon = Properties.Resources.YChanEx404;

                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);
                            btnAbortRetry.Text = "Rescan";
                            break;

                        case ThreadStatus.ThreadDownloading:
                        case ThreadStatus.Waiting:
                        case ThreadStatus.ThreadNotModified:
                            switch (CurrentThread.CurrentActivity) {
                                case ThreadStatus.ThreadNotModified:
                                    lbNotModified.Visible = true;
                                    break;
                            }
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);
                            CurrentThread.CountdownToNextScan = Downloads.Default.ScannerDelay - 1;
                            if (Program.IsDebug) {
                                CurrentThread.CountdownToNextScan = 9;
                            }
                            lbScanTimer.Text = "soon (tm)";
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            tmrScan.Start();
                            break;

                        case ThreadStatus.ThreadIsNotAllowed:
                            break;

                        case ThreadStatus.ThreadInfoNotSet:
                            break;
                    }
                    break;
                #endregion

                #region AbortDownload
                case ThreadEvent.AbortDownload:
                    Debug.Print("AbortDownload called");
                    tmrScan.Stop();
                    if (DownloadThread != null && DownloadThread.IsAlive) {
                        DownloadThread.Abort();
                    }
                    this.Icon = Properties.Resources.YChanEx404;
                    lbScanTimer.Text = "Aborted";
                    lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);

                    btnAbortRetry.Text = "Retry";
                    lbNotModified.Visible = false;
                    if (Program.IsDebug) {
                        btnForce404.Enabled = false;
                    }
                    break;
                #endregion

                #region RetryDownload
                case ThreadEvent.RetryDownload:
                    Debug.Print("RetryDownload called");
                    this.Icon = Properties.Resources.YChanEx;
                    lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.ControlText);

                    CurrentThread.CurrentActivity = ThreadStatus.ThreadRetrying;
                    btnAbortRetry.Text = "Abort";
                    if (Program.IsDebug) {
                        btnForce404.Enabled = true;
                    }

                    MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.CurrentActivity);
                    lbScanTimer.Text = "scanning now...";
                    btnAbortRetry.Text = "Abort";
                    tmrScan.Stop();
                    ManageThread(ThreadEvent.StartDownload);
                    break;
                #endregion

                #region ThreadWasGone
                case ThreadEvent.ThreadWasGone:
                    this.Icon = Properties.Resources.YChanEx404;
                    lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                    btnAbortRetry.Text = "Retry";
                    switch (LastStatus) {
                        case ThreadStatus.ThreadIs404:
                            lbScanTimer.Text = "404'd";
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadIs404;
                            break;
                        case ThreadStatus.ThreadIsAborted:
                            lbScanTimer.Text = "Aborted";
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                            break;
                        case ThreadStatus.ThreadIsArchived:
                            lbScanTimer.Text = "Archived";
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadIsArchived;
                            break;
                    }
                    ManageThread(ThreadEvent.ParseForInfo);
                    break;
                #endregion

                #region AbortForClosing
                case ThreadEvent.AbortForClosing:
                    if (DownloadThread != null && DownloadThread.IsAlive) {
                        DownloadThread.Abort();
                    }
                    break;
                #endregion
            }
        }

        public void UpdateThreadName(bool ApplyToMainForm = false) {
            string ThreadNameBuffer = "unknown thread - {0} - {1}";
            switch (CurrentThread.Chan) {
                case ChanType.FourChan:
                    ThreadNameBuffer = "4chan thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                case ChanType.FourTwentyChan:
                    ThreadNameBuffer = "420chan thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                case ChanType.SevenChan:
                    ThreadNameBuffer = "7chan thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                case ChanType.EightChan:
                    ThreadNameBuffer = "8chan thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                case ChanType.EightKun:
                    ThreadNameBuffer = "8kun thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                case ChanType.fchan:
                    ThreadNameBuffer = "fchan thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                case ChanType.u18chan:
                    ThreadNameBuffer = "u18chan thread - {0} - {1}";
                    if (Downloads.Default.UseThreadName && CurrentThread.RetrievedThreadName) {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadName);
                        if (ApplyToMainForm && !CurrentThread.SetCustomName) {
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadUpdateName);
                        }
                    }
                    else {
                        this.Text = string.Format(ThreadNameBuffer, BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    break;
                default:
                    this.Text = string.Format(ThreadNameBuffer, CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                    return;
            }
        }
        #endregion

        // TODO: Don't skip API parsing if a file is 404'd.
        // There could be new posts! it'd be a shame to skip them.
        #region Shared Chan Logic
        /// <summary>
        /// Retrieve the HTML of a given Thread URL for parsing or aesthetics.
        /// </summary>
        /// <param name="URL">The URL of the page to download the HTML source.</param>
        /// <param name="UseWebClient">Use WebClient to recieve HTML instead of HttpWebRequest.</param>
        /// <returns>The HTML of a given Thread</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private string GetThreadHTML(string URL, bool SkipModifiedSince = false, bool UseWebClient = false) {
            try {
                if (UseWebClient) {
                    using (WebClientExtended wc = new WebClientExtended()) {
                        if (!SkipModifiedSince) {
                            wc.IfModifiedSince = CurrentThread.LastModified;
                        }
                        wc.Method = "GET";
                        wc.Headers.Add("user-agent", Advanced.Default.UserAgent);
                        return wc.DownloadString(URL);
                    }
                }
                else {
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.CreateHttp(CurrentThread.ThreadURL);
                    if (!SkipModifiedSince) {
                        Request.IfModifiedSince = CurrentThread.LastModified;
                    }
                    Request.CookieContainer = CurrentThread.ThreadCookieContainer;
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.Method = "GET";
                    Request = (HttpWebRequest)WebRequest.Create(URL);
                    using (var Response = (HttpWebResponse)Request.GetResponse())
                    using (var ResponseStream = Response.GetResponseStream())
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        CurrentThread.LastModified = Response.LastModified;
                        return ResponseReader.ReadToEnd();
                    }
                }
            }
            catch (WebException) {
                throw;
            }
            catch (Exception) {
                throw;
            }
        }
        /// <summary>
        /// Retrieve the JSON info of a given Thread URL for parsing.
        /// </summary>
        /// <param name="URL">The URL of the JSON file to download.</param>
        /// <returns>The JSON of a given Thread</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private string GetThreadJSON(string URL) {
            try {
                string RetrievedJson = null;
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
                Request.IfModifiedSince = CurrentThread.LastModified;
                Request.CookieContainer = CurrentThread.ThreadCookieContainer;
                Request.UserAgent = Advanced.Default.UserAgent;
                Request.Method = "GET";
                using (var Response = (HttpWebResponse)Request.GetResponse())
                using (var ResponseStream = Response.GetResponseStream())
                using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                    string RawJSON = ResponseReader.ReadToEnd();
                    byte[] JSONBytes = Encoding.ASCII.GetBytes(RawJSON);
                    using (var ByteMemory = new MemoryStream(JSONBytes)) {
                        var Quotas = new XmlDictionaryReaderQuotas();
                        var JSONReader = JsonReaderWriterFactory.CreateJsonReader(ByteMemory, Quotas);
                        var xml = XDocument.Load(JSONReader);
                        CurrentThread.LastModified = Response.LastModified;
                        RetrievedJson = xml.ToString();
                    }
                }
                return RetrievedJson;
            }
            catch (WebException) {
                throw;
            }
            catch (Exception) {
                throw;
            }
        }
        private bool GetThreadJSON(string URL, out string JsonString) {
            string RetrievedJson = null;
            JsonString = null;

            try {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
                Request.IfModifiedSince = CurrentThread.LastModified;
                Request.CookieContainer = CurrentThread.ThreadCookieContainer;
                Request.UserAgent = Advanced.Default.UserAgent;
                Request.Method = "GET";
                using (var Response = (HttpWebResponse)Request.GetResponse())
                using (var ResponseStream = Response.GetResponseStream())
                using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                    string RawJSON = ResponseReader.ReadToEnd();
                    byte[] JSONBytes = Encoding.ASCII.GetBytes(RawJSON);
                    using (var ByteMemory = new MemoryStream(JSONBytes)) {
                        var Quotas = new XmlDictionaryReaderQuotas();
                        var JSONReader = JsonReaderWriterFactory.CreateJsonReader(ByteMemory, Quotas);
                        var xml = XDocument.Load(JSONReader);
                        CurrentThread.LastModified = Response.LastModified;
                        RetrievedJson = xml.ToString();
                    }
                }

                JsonString = RetrievedJson;
                return true;
            }
            catch (WebException WebEx) {
                HandleWebException(WebEx, URL);
                return false;
            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return false;
            }
        }
        private string GetThreadName(string HTML) {
            if (!string.IsNullOrEmpty(CurrentThread.ThreadName)) {
                return CurrentThread.ThreadName;
            }

            // First, trim the HTML from the start of the <title> tag to, up to the </title> closing tag
            string TitleWork = HTML.Substring(HTML.IndexOf("<title>") + 7,
                                              HTML.IndexOf("</title>") - (HTML.IndexOf("<title>") + 7));

            switch (CurrentThread.Chan) {
                case ChanType.FourChan:
                    // trim the end of the text in the tag
                    TitleWork = TitleWork.Replace(" - " + BoardTitles.FourChan(CurrentThread.ThreadBoard, true) + " - 4chan", "");
                    TitleWork = TitleWork.Replace("/" + CurrentThread.ThreadBoard + "/ - ", "");
                    // decode any html chars
                    TitleWork = WebUtility.HtmlDecode(TitleWork);

                    CurrentThread.ThreadName = TitleWork;
                    CurrentThread.RetrievedThreadName = true;

                    return TitleWork;
                case ChanType.FourTwentyChan:

                    break;
                case ChanType.SevenChan:

                    break;
                case ChanType.EightChan:

                    break;
                case ChanType.EightKun:

                    break;
                case ChanType.fchan:

                    break;
                case ChanType.u18chan:

                    break;
                default:
                    return string.Empty;
            }
            return string.Empty;
        }

        #region Exception Handling
        private void HandleWebException(WebException WebEx, string CurrentURL) {
            switch (((HttpWebResponse)WebEx.Response).StatusCode) {
                case HttpStatusCode.NotModified:
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadNotModified;
                    break;
                case HttpStatusCode.NotFound:
                    if (CurrentThread.DownloadingFiles) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadFile404;
                    }
                    else {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadIs404;
                    }
                    break;
                case HttpStatusCode.Forbidden:
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsNotAllowed;
                    break;
                default:
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                    ErrorLog.ReportWebException(WebEx, CurrentURL);
                    break;
            }
        }
        #endregion

        #endregion

        #region 4chan Download Logic Completed. (Rescans from the Beginning)
        private void Set4chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://i.4cdn.org/" + CurrentThread.ThreadBoard + "/";
                string ThreadJSON = null;
                //string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    if (!CurrentThread.FileWas404) {
                        #region API/HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                        CurrentURL = string.Format(Networking.GetAPILink(CurrentThread.Chan), CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                        ThreadJSON = GetThreadJSON(CurrentURL);

                        if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Networking.EmptyXML) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                            return;
                        }

                        CurrentURL = CurrentThread.ThreadURL;
                        //if (YChanEx.Downloads.Default.SaveHTML || Downloads.Default.UseThreadName && !CurrentThread.RetrievedThreadName) {
                        //    ThreadHTML = GetThreadHTML(CurrentURL, true, true);
                        //    if (Downloads.Default.UseThreadName && !CurrentThread.RetrievedThreadName) {
                        //        CurrentThread.ThreadName = GetThreadName(ThreadHTML);

                        //        this.BeginInvoke(new MethodInvoker(() => {
                        //            UpdateThreadName(true);
                        //        }));
                        //    }
                        //}
                        #endregion

                        #region API Parsing Logic
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(ThreadJSON);
                        XmlNodeList xmlPost = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                        XmlNodeList xmlPostNumber = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/no");
                        XmlNodeList xmlArchived = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/archived");

                        if (Downloads.Default.UseThreadName && !CurrentThread.RetrievedThreadName) {
                            string xmlOpComment = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/com")[0].InnerText.Replace("<br>"," ").Replace("<wbr>"," ");
                            if (xmlOpComment.Length > 64) {
                                xmlOpComment = xmlOpComment.Substring(0, 64);
                            }
                            CurrentThread.ThreadName = xmlOpComment;
                            CurrentThread.RetrievedThreadName = true;
                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace("<title></title>", "<title> /" + CurrentThread.ThreadBoard + "/ - " + xmlOpComment + " - 4chan</title>");
                            CurrentThread.HtmlTheadNameSet = true;
                            this.BeginInvoke((MethodInvoker)delegate() {
                                UpdateThreadName(true);
                            });
                        }
                        else if (!CurrentThread.HtmlTheadNameSet) {
                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace("<title></title>", "<title> /" + CurrentThread.ThreadBoard + "/ - " + CurrentThread.ThreadID + " - 4chan</title>");
                            CurrentThread.HtmlTheadNameSet = true;
                        }

                        for (int PostIndex = 0; PostIndex < xmlPost.Count; PostIndex++) {
                            if (!CurrentThread.ParsedPosts.Contains(xmlPostNumber[PostIndex].InnerText)) {
                                PostInfo CurrentPost = new PostInfo();
                                XmlNodeList xmlFileID = xmlPost[PostIndex].SelectNodes("tim"); // File on the server
                                XmlNodeList xmlFileName = xmlPost[PostIndex].SelectNodes("filename"); // original file name
                                XmlNodeList xmlExt = xmlPost[PostIndex].SelectNodes("ext"); // file extension
                                XmlNodeList xmlHash = xmlPost[PostIndex].SelectNodes("md5"); // md5 short hash
                                if (xmlFileID.Count > 0) {
                                    string FileID = xmlFileID[0].InnerText;
                                    string OriginalFileName = xmlFileName[0].InnerText;
                                    string FileExtension = xmlExt[0].InnerText;
                                    string ImageFile = FileBaseURL + xmlFileID[0].InnerText + xmlExt[0].InnerText;
                                    string ThumbnailFile = FileBaseURL + FileID + "s.jpg";
                                    string FileHash = xmlHash[0].InnerText;

                                    CurrentThread.FileIDs.Add(FileID);
                                    CurrentThread.FileExtensions.Add(FileExtension);
                                    CurrentThread.ThumbnailFiles.Add(ThumbnailFile);
                                    CurrentThread.ImageFiles.Add(ImageFile);
                                    CurrentThread.FileHashes.Add(xmlHash[0].InnerText);

                                    string FileNameToReplace = FileID;
                                    string FileName = FileID;
                                    if (Downloads.Default.SaveOriginalFilenames) {
                                        FileName = OriginalFileName;
                                        string FileNamePrefix = "";
                                        string FileNameSuffix = "";

                                        if (Downloads.Default.PreventDuplicates) {
                                            if (CurrentThread.FileOriginalNames.Contains(FileName)) {
                                                if (CurrentThread.FileNamesDupes.Contains(FileName)) {
                                                    int DupeNameIndex = CurrentThread.FileNamesDupes.IndexOf(FileName);
                                                    CurrentThread.FileNamesDupesCount[DupeNameIndex] += 1;
                                                    FileNamePrefix = "(" + CurrentThread.FileNamesDupesCount[DupeNameIndex].ToString() + ") ";
                                                }
                                                else {
                                                    CurrentThread.FileNamesDupes.Add(FileName);
                                                    CurrentThread.FileNamesDupesCount.Add(1);
                                                    FileNamePrefix = "(1) ";
                                                }
                                            }
                                        }

                                        for (int j = 0; j < Networking.InvalidFileCharacters.Length; j++) {
                                            FileName = FileName.Replace(Networking.InvalidFileCharacters[j], "_");
                                        }

                                        FileNameToReplace = FileNamePrefix + FileName + FileNameSuffix;
                                        FileName = FileNamePrefix + FileName + FileNameSuffix;
                                    }

                                    CurrentThread.FileOriginalNames.Add(OriginalFileName);
                                    CurrentThread.FileNames.Add(FileName + FileExtension);

                                    if (Downloads.Default.SaveHTML) {
                                        CurrentPost.PostContainsFile = true;
                                        CurrentPost.PostOutputFileName = FileName + FileExtension;
                                        XmlNodeList xmlPostWidth = xmlPost[PostIndex].SelectNodes("w");
                                        XmlNodeList xmlPostHeight = xmlPost[PostIndex].SelectNodes("h");
                                        XmlNodeList xmlThumbnailWidth = xmlPost[PostIndex].SelectNodes("tn_w");
                                        XmlNodeList xmlThumbnailHeight = xmlPost[PostIndex].SelectNodes("tn_h");
                                        XmlNodeList xmlFileSize = xmlPost[PostIndex].SelectNodes("fsize");
                                        CurrentPost.PostOriginalName = OriginalFileName;
                                        CurrentPost.PostFileExtension = FileExtension;
                                        CurrentPost.PostWidth = xmlPostWidth[0].InnerText;
                                        CurrentPost.PostHeight = xmlPostHeight[0].InnerText;
                                        CurrentPost.PostThumbnailWidth = xmlThumbnailWidth[0].InnerText;
                                        CurrentPost.PostThumbnailHeight = xmlThumbnailHeight[0].InnerText;
                                        CurrentPost.PostFileID = FileID;
                                        CurrentPost.PostFileSize = xmlPostHeight[0].InnerText;

                                        string OldHTMLLinks = null;
                                        if (YChanEx.Downloads.Default.SaveThumbnails) {
                                            OldHTMLLinks = "//i.4cdn.org/" + CurrentThread.ThreadBoard + "/" + FileID + "s.jpg";
                                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + FileID + "s.jpg");
                                        }

                                        OldHTMLLinks = "//i.4cdn.org/" + CurrentThread.ThreadBoard + "/" + FileID;
                                        string OldHTMLLinks2 = "//is2.4chan.org/" + CurrentThread.ThreadBoard + "/" + FileID;
                                        if (Downloads.Default.SaveOriginalFilenames) {
                                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks, FileNameToReplace);
                                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks2, FileNameToReplace);
                                        }
                                        else {
                                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks, FileID);
                                            CurrentThread.ThreadHTML = CurrentThread.ThreadHTML.Replace(OldHTMLLinks2, FileID);
                                        }
                                    }

                                    ListViewItem lvi = new ListViewItem();
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.Name = FileID;
                                    lvi.SubItems[0].Text = FileID;
                                    lvi.SubItems[1].Text = FileExtension.Trim('.');
                                    lvi.SubItems[2].Text = OriginalFileName;
                                    lvi.SubItems[3].Text = FileHash;
                                    lvi.ImageIndex = 0;
                                    this.BeginInvoke(new MethodInvoker(() => {
                                        lvImages.Items.Add(lvi);
                                    }));

                                    CurrentThread.ThreadImagesCount++;
                                    CurrentThread.ThreadPostsCount++;
                                }

                                if (Downloads.Default.SaveHTML) {
                                    //XmlNodeList xmlFileID = xmlPost[PostIndex].SelectNodes("tim"); // File on the server
                                    //XmlNodeList xmlFileName = xmlPost[PostIndex].SelectNodes("filename"); // original file name
                                    //XmlNodeList xmlExt = xmlPost[PostIndex].SelectNodes("ext"); // file extension
                                    //XmlNodeList xmlHash = xmlPost[PostIndex].SelectNodes("md5"); // md5 short hash
                                    XmlNodeList xmlPostNow = xmlPost[PostIndex].SelectNodes("now");
                                    XmlNodeList xmlPostName = xmlPost[PostIndex].SelectNodes("name");
                                    XmlNodeList xmlPostSubject = xmlPost[PostIndex].SelectNodes("sub");
                                    XmlNodeList xmlPostComment = xmlPost[PostIndex].SelectNodes("com");

                                    CurrentPost.PostID = xmlPostNumber[PostIndex].InnerText;
                                    CurrentPost.PostDate = xmlPostNow[0].InnerText;
                                    CurrentPost.PosterName = xmlPostName[0].InnerText;
                                    if (xmlPostSubject.Count > 0) {
                                        CurrentPost.PostSubject = xmlPostSubject[0].InnerText;
                                    }
                                    else {
                                        CurrentPost.PostSubject = string.Empty;
                                    }
                                    if (xmlPostComment.Count > 0) {
                                        CurrentPost.PostComment = xmlPostComment[0].InnerText;
                                    }
                                    else {
                                        CurrentPost.PostComment = string.Empty;
                                    }

                                    CurrentThread.ThreadHTML += ChanHTML.GetPostForHTML(CurrentPost, PostIndex == 0);
                                }

                                CurrentThread.ParsedPosts.Add(xmlPostNumber[PostIndex].InnerText);
                            }
                        }

                        CurrentThread.ThreadArchived = xmlArchived.Count > 0;

                        this.BeginInvoke(new MethodInvoker(() => {
                            lbTotalFiles.Text = CurrentThread.ThreadImagesCount.ToString();
                            lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                            lbScanTimer.Text = "Downloading files";
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                        }));
                        #endregion
                    }

                    #region Download Logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;
                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {
                        if (CurrentThread.ImageFiles[ImageFilesIndex] == null) {
                            continue;
                        }
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));

                        string FileName = CurrentThread.FileNames[ImageFilesIndex];
                        CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];
                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath, FileName)) {
                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                CurrentURL = CurrentThread.ThumbnailFiles[ImageFilesIndex];
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath + "\\thumb", CurrentThread.FileIDs[ImageFilesIndex] + "s.jpg");
                            }

                            CurrentThread.DownloadedImagesCount++;

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                            }));
                        }
                        else {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                            }));
                        }


                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }

                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", CurrentThread.ThreadHTML + ChanHTML.GetHTMLFooter());
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.FileWas404 = false;
                    CurrentThread.RetryCountFor404 = 0;
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    if (CurrentThread.ThreadArchived) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadIsArchived;
                    }
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "4chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }

        private static bool Generate4chanMD5(string InputFile, string InputFileHash) {
            // Attempts to convert existing file to 4chan's hash type
            try {
                if (!File.Exists(InputFile)) {
                    return false;
                }

                string OutputHash = null;

                using (System.Security.Cryptography.MD5 FileMD5 = System.Security.Cryptography.MD5.Create())
                using (var FileStream = File.OpenRead(InputFile)) {
                    var FileHash = FileMD5.ComputeHash(FileStream);
                    System.Threading.Thread.Sleep(50);
                    OutputHash = BitConverter.ToString(FileHash).Replace("-", string.Empty).ToLowerInvariant();
                }

                byte[] RawByte = new byte[16];
                for (int i = 0; i < 32; i += 2) {
                    RawByte[i / 2] = Convert.ToByte(OutputHash.Substring(i, 2), 16);
                }

                OutputHash = Convert.ToBase64String(RawByte);

                if (OutputHash == InputFileHash) { return true; }
                else { return false; }
            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return false;
            }
        }
        #endregion

        #region 420chan Download Logic Completed. (Rescans from the Beginning)
        private void Set420chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://boards.420chan.org/" + CurrentThread.ThreadBoard + "/src/";
                string ThumbnailBaseUrl = "https://boards.420chan.org/" + CurrentThread.ThreadBoard + "/thumb/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    if (!CurrentThread.FileWas404) {

                        #region API/HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                        CurrentURL = string.Format(Networking.GetAPILink(CurrentThread.Chan), CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                        ThreadJSON = GetThreadJSON(CurrentURL);

                        if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Networking.EmptyXML) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                            return;
                        }

                        CurrentURL = this.CurrentThread.ThreadURL;
                        if (YChanEx.Downloads.Default.SaveHTML) {
                            ThreadHTML = GetThreadHTML(CurrentURL, true);
                            ThreadHTML.Replace("href=\"/" + CurrentThread.ThreadBoard + "/src/", "");
                            ThreadHTML.Replace("href=\"/" + CurrentThread.ThreadBoard, "");
                            ThreadHTML.Replace("href=\"/static/", "href=\"https://420chan.org/static/");
                        }
                        #endregion

                        #region API Parsing Logic
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlFileID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/no");
                    XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");
                    XmlNodeList xmlExt = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");

                    for (int FileNameIndex = 0; FileNameIndex < xmlFileName.Count; FileNameIndex++) {
                        if (xmlFileName[FileNameIndex] != null && !CurrentThread.FileIDs.Contains(xmlFileID[FileNameIndex].InnerText)) {
                            CurrentThread.FileIDs.Add(xmlFileID[FileNameIndex].InnerText);
                            CurrentThread.FileExtensions.Add(xmlExt[FileNameIndex].InnerText);
                            CurrentThread.ImageFiles.Add(FileBaseURL + xmlFileName[FileNameIndex].InnerText + xmlExt[FileNameIndex].InnerText);
                            CurrentThread.ThumbnailFiles.Add(ThumbnailBaseUrl + xmlFileName[FileNameIndex].InnerText + "s.jpg");

                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = xmlFileName[FileNameIndex].InnerText;
                            lvi.Text = xmlFileName[FileNameIndex].InnerText;
                            lvi.SubItems[1].Text = xmlExt[FileNameIndex].InnerText.Trim('.');
                            lvi.SubItems[2].Text = xmlFileName[FileNameIndex].InnerText;
                            lvi.ImageIndex = 0;
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));

                            CurrentThread.ThreadImagesCount++;
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = CurrentThread.ThreadImagesCount.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion
                    }

                    #region Download Logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {

                        if (CurrentThread.ImageFiles[ImageFilesIndex] != null) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            }));
                            string FileName = CurrentThread.FileIDs[ImageFilesIndex] + CurrentThread.FileExtensions[ImageFilesIndex];
                            CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];

                            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                            if (Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath, FileName)) {
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    CurrentURL = CurrentThread.ThumbnailFiles[ImageFilesIndex];
                                    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                    Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath + "\\thumb", CurrentThread.FileIDs[ImageFilesIndex] + "s.jpg");
                                }

                                CurrentThread.DownloadedImagesCount++;

                                this.BeginInvoke(new MethodInvoker(() => {
                                    lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                                }));
                            }
                            else {
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                                }));
                            }

                        }

                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }

                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.FileWas404 = false;
                    CurrentThread.RetryCountFor404 = 0;
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "420chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region 7chan Download Logic Basically Completed, Needs: Original File Names (Rescans from the Beginning)
        private void Set7chanThread() {
            DownloadThread = new Thread(() => {
                string BaseURL = "https://7chan.org/";
                string ThreadHTML = null;
                string CurrentURL = null;
                try {

                    if (!CurrentThread.FileWas404) {
                        #region HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                        for (int TryCount = 0; TryCount < 5; TryCount++) {
                            CurrentURL = CurrentThread.ThreadURL;
                            ThreadHTML = GetThreadHTML(CurrentURL);

                            if (string.IsNullOrEmpty(ThreadHTML)) {
                                if (TryCount == 5) {
                                    CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                                    return;
                                }
                                Thread.Sleep(5000);
                            }
                            else {
                                break;
                            }
                        }

                        if (ThreadHTML == CurrentThread.LastThreadHTML) {
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.LastThreadHTML = ThreadHTML;
                        #endregion

                        #region HTML Parsing Logic
                        MatchCollection PostMatches = new Regex(ChanRegex.SevenChanPosts).Matches(ThreadHTML);
                        for (int PostMatchesIndex = 0; PostMatchesIndex < PostMatches.Count; PostMatchesIndex++) {
                            string MatchValue = PostMatches[PostMatchesIndex].Value;
                            int IndexOfFileLink = MatchValue.IndexOf("alt=\"") + 5;
                            int IndexOfID = MatchValue.IndexOf("\"><img src=\"");
                            string PostID = MatchValue.Substring(IndexOfFileLink, MatchValue.Length - IndexOfFileLink - 15);
                            if (!CurrentThread.FileIDs.Contains(PostID)) {
                                string FileLink = MatchValue.Substring(0, IndexOfID);
                                string FileExtension = "." + FileLink.Split('.')[2];
                                string FullFileName = FileLink.Split('/')[5];
                                string FileName = FullFileName.Substring(0, FullFileName.Length - FileExtension.Length);

                                CurrentThread.ImageFiles.Add(FileLink);
                                CurrentThread.FileExtensions.Add(FileExtension);
                                CurrentThread.FileIDs.Add(PostID);

                                //if (Downloads.Default.SaveOriginalFilenames) {
                                //    FileName = OriginalFileName;
                                //    string FileNamePrefix = "";
                                //    string FileNameSuffix = "";

                                //    if (Downloads.Default.PreventDuplicates) {
                                //        if (CurrentThread.PostOriginalNames.Contains(FileName)) {
                                //            if (FileNamesDupes.Contains(FileName)) {
                                //                int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                //                FileNamesDupesCount[DupeNameIndex] += 1;
                                //                FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                //            }
                                //            else {
                                //                FileNamesDupes.Add(FileName);
                                //                FileNamesDupesCount.Add(1);
                                //                FileNameSuffix = " (dupe 1)";
                                //            }
                                //        }
                                //    }

                                //    for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                //        FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                //    }

                                //    FileNameToReplace = FileNamePrefix + FileName + FileNameSuffix;
                                //    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                //}

                                //CurrentThread.PostOriginalNames.Add(OriginalFileName);
                                CurrentThread.FileNames.Add(FileName + FileExtension);

                                if (Downloads.Default.SaveThumbnails) {
                                    CurrentThread.ThumbnailFiles.Add(BaseURL + CurrentThread.ThreadBoard + "/thumb/" + FileName + "s" + FileExtension);
                                    CurrentThread.ThumbnailNames.Add(FileName + "s" + FileExtension);
                                }

                                CurrentThread.ThreadPostsCount++;

                                ListViewItem lvi = new ListViewItem();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = FileLink;
                                lvi.SubItems[0].Text = FileName;
                                lvi.SubItems[1].Text = FileExtension;
                                lvi.SubItems[2].Text = FileName;
                                lvi.ImageIndex = 0;
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items.Add(lvi);
                                }));
                            }
                        }

                        if (Downloads.Default.SaveHTML) {
                            ThreadHTML = ThreadHTML.Replace("https://7chan.org/" + CurrentThread.ThreadBoard + "/src/", "");
                            ThreadHTML = ThreadHTML.Replace("https://7chan.org/" + CurrentThread.ThreadBoard + "/thumb/", "thumb/");
                        }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = CurrentThread.ImageFiles.Count.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion
                    }

                    #region Download Logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));
                        CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];

                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Networking.DownloadFile(CurrentThread.ImageFiles[ImageFilesIndex], CurrentThread.DownloadPath, CurrentThread.FileNames[ImageFilesIndex])) {
                            if (Downloads.Default.SaveThumbnails) {
                                Networking.DownloadFile(CurrentThread.ThumbnailFiles[ImageFilesIndex], CurrentThread.DownloadPath + "\\thumb\\", CurrentThread.ThumbnailNames[ImageFilesIndex]);
                            }

                            CurrentThread.DownloadedImagesCount++;

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                            }));
                        }
                        else {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                            }));
                        }

                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }

                    if (Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.FileWas404 = false;
                    CurrentThread.RetryCountFor404 = 0;
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "7chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region 8chan Download Logic Completed. (Rescans from Beginning)
        private void Set8chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://8chan.moe";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    if (!CurrentThread.FileWas404) {
                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                        #region API/HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentURL = string.Format(Networking.GetAPILink(CurrentThread.Chan), CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                        ThreadJSON = GetThreadJSON(CurrentURL);

                        if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Networking.EmptyXML) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                            return;
                        }

                        CurrentURL = this.CurrentThread.ThreadURL;
                        if (YChanEx.Downloads.Default.SaveHTML) {
                            ThreadHTML = GetThreadHTML(CurrentURL);
                        }

                        if (General.Default.UseFullBoardNameForTitle && !CurrentThread.RetrievedBoardName) {
                            if (ThreadHTML == null) {
                                ThreadHTML = GetThreadHTML(CurrentURL, true);
                            }

                            int TitleExtraLength = 5 + CurrentThread.ThreadBoard.Length;
                            CurrentThread.BoardName = ThreadHTML.Substring(
                                ThreadHTML.IndexOf("<title>") + (7 + TitleExtraLength),
                                ThreadHTML.IndexOf("</title>") - ThreadHTML.IndexOf("<title>") - (7 + TitleExtraLength)
                            );

                            this.BeginInvoke(new MethodInvoker(() => {
                                this.Text = string.Format("8chan thread - {0} - {1}", CurrentThread.BoardName, CurrentThread.ThreadID);
                            }));

                            CurrentThread.RetrievedBoardName = true;

                            Thread.Sleep(100);
                        }

                        if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Networking.EmptyXML) {
                            // Thread is dead?
                            return;
                        }
                        #endregion

                        #region API Parsing logic
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(ThreadJSON);

                        #region First post file(s)
                        XmlNodeList xmlFilePath = xmlDoc.DocumentElement.SelectNodes("/root/files/item/path");
                        XmlNodeList xmlFileThumbnail = xmlDoc.DocumentElement.SelectNodes("/root/files/item/thumb");
                        XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/files/item/originalName");

                        for (int PostIndex = 0; PostIndex < xmlFilePath.Count; PostIndex++) {
                            if (xmlFilePath[PostIndex] == null) {
                                continue;
                            }
                            string FileID = CurrentThread.ThreadID;
                            if (PostIndex > 0) {
                                FileID += "-" + (PostIndex + 1).ToString();
                            }
                            if (!CurrentThread.FileIDs.Contains(FileID)) {
                                string FileUrl = xmlFilePath[PostIndex].InnerText;
                                string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                                string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[FileUrl.Split('/')[2].Split('.').Length - 1];
                                string OriginalFileName = xmlFileName[PostIndex].InnerText;
                                CurrentThread.FileExtensions.Add(FileExtension);
                                CurrentThread.FileIDs.Add(FileID);
                                CurrentThread.FileHashes.Add(FileHash);
                                CurrentThread.ImageFiles.Add(FileBaseURL + FileUrl);
                                CurrentThread.ThumbnailFiles.Add(FileBaseURL + xmlFileThumbnail[PostIndex].InnerText);
                                CurrentThread.ThumbnailNames.Add(xmlFileThumbnail[PostIndex].InnerText.Substring(8));

                                string FileName = FileUrl.Substring(8, FileUrl.Length - 12);
                                if (Downloads.Default.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";
                                    FileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);
                                    for (int j = 0; j < Networking.InvalidFileCharacters.Length; j++) {
                                        FileName = FileName.Replace(Networking.InvalidFileCharacters[j], "_");
                                    }
                                    if (Downloads.Default.PreventDuplicates) {
                                        if (CurrentThread.FileOriginalNames.Contains(FileName)) {
                                            if (CurrentThread.FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = CurrentThread.FileNamesDupes.IndexOf(FileName);
                                                CurrentThread.FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNameSuffix = " (dupe " + CurrentThread.FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                            }
                                            else {
                                                CurrentThread.FileNamesDupes.Add(FileName);
                                                CurrentThread.FileNamesDupesCount.Add(1);
                                                FileNameSuffix = " (dupe 1)";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                }

                                CurrentThread.FileOriginalNames.Add(OriginalFileName);
                                CurrentThread.FileNames.Add(FileName);

                                if (YChanEx.Downloads.Default.SaveHTML) {
                                    string OldHTMLLinks = null;

                                    OldHTMLLinks = "src=\"/.media/t_" + FileHash;
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "src=\"thumb/t_" + FileHash + ".jpg");
                                    OldHTMLLinks = "href=\"/.media/" + FileHash;
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "href=\"" + FileName);
                                }

                                CurrentThread.ThreadImagesCount++;

                                ListViewItem lvi = new ListViewItem();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = xmlFilePath[PostIndex].InnerText;
                                lvi.SubItems[0].Text = FileID;
                                lvi.SubItems[1].Text = FileExtension.Trim('.');
                                lvi.SubItems[2].Text = xmlFileName[PostIndex].InnerText.Substring(0, xmlFileName[PostIndex].InnerText.Length - 4);
                                lvi.SubItems[3].Text = FileHash;
                                lvi.ImageIndex = 0;
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items.Add(lvi);
                                }));
                            }
                        }
                        #endregion

                        #region Subsequent posts file(s)
                        XmlNodeList xmlPosts = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                        for (int PostsIndex = 0; PostsIndex < xmlPosts.Count; PostsIndex++) {
                            XmlNodeList xmlPostID = xmlPosts[PostsIndex].SelectNodes("postId");
                            xmlFilePath = xmlPosts[PostsIndex].SelectNodes("files/item/path");
                            xmlFileThumbnail = xmlPosts[PostsIndex].SelectNodes("files/item/thumb");
                            xmlFileName = xmlPosts[PostsIndex].SelectNodes("files/item/originalName");

                            for (int FilePathIndex = 0; FilePathIndex < xmlFilePath.Count; FilePathIndex++) {
                                if (xmlFilePath[FilePathIndex] == null) {
                                    continue;
                                }
                                string FileID = xmlPostID[0].InnerText;
                                if (FilePathIndex > 0) {
                                    FileID += "-" + (FilePathIndex + 1).ToString();
                                }
                                if (!CurrentThread.FileIDs.Contains(FileID)) {
                                    string FileUrl = xmlFilePath[FilePathIndex].InnerText;
                                    string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                                    string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[FileUrl.Split('/')[2].Split('.').Length - 1];
                                    string OriginalFileName = xmlFileName[FilePathIndex].InnerText;
                                    CurrentThread.FileExtensions.Add(FileExtension);
                                    CurrentThread.FileIDs.Add(FileID);
                                    CurrentThread.FileHashes.Add(FileHash);
                                    CurrentThread.ImageFiles.Add(FileBaseURL + FileUrl);
                                    CurrentThread.ThumbnailFiles.Add(FileBaseURL + xmlFileThumbnail[FilePathIndex].InnerText);
                                    CurrentThread.ThumbnailNames.Add(xmlFileThumbnail[FilePathIndex].InnerText.Substring(8));

                                    string FileName = FileUrl.Substring(8, FileUrl.Length - 12);
                                    if (Downloads.Default.SaveOriginalFilenames) {
                                        string FileNamePrefix = "";
                                        string FileNameSuffix = "";
                                        FileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);
                                        for (int j = 0; j < Networking.InvalidFileCharacters.Length; j++) {
                                            FileName = FileName.Replace(Networking.InvalidFileCharacters[j], "_");
                                        }
                                        if (Downloads.Default.PreventDuplicates) {
                                            if (CurrentThread.FileOriginalNames.Contains(FileName)) {
                                                if (CurrentThread.FileNamesDupes.Contains(FileName)) {
                                                    int DupeNameIndex = CurrentThread.FileNamesDupes.IndexOf(FileName);
                                                    CurrentThread.FileNamesDupesCount[DupeNameIndex] += 1;
                                                    FileNameSuffix = " (dupe " + CurrentThread.FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                                }
                                                else {
                                                    CurrentThread.FileNamesDupes.Add(FileName);
                                                    CurrentThread.FileNamesDupesCount.Add(1);
                                                    FileNameSuffix = " (dupe 1)";
                                                }
                                            }
                                        }

                                        FileName = FileNamePrefix + FileName + FileNameSuffix;
                                    }

                                    CurrentThread.FileOriginalNames.Add(OriginalFileName);
                                    CurrentThread.FileNames.Add(FileName);

                                    if (YChanEx.Downloads.Default.SaveHTML) {
                                        string OldHTMLLinks = null;

                                        OldHTMLLinks = "src=\"/.media/t_" + FileHash;
                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "src=\"thumb/t_" + FileHash + ".jpg");
                                        OldHTMLLinks = "href=\"/.media/" + FileHash;
                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "href=\"" + FileName);
                                    }

                                    CurrentThread.ThreadPostsCount++;
                                    CurrentThread.ThreadImagesCount++;

                                    ListViewItem lvi = new ListViewItem();
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.Name = xmlFilePath[FilePathIndex].InnerText;
                                    lvi.SubItems[0].Text = FileID;
                                    lvi.SubItems[1].Text = FileExtension.Trim('.');
                                    lvi.SubItems[2].Text = xmlFileName[FilePathIndex].InnerText.Substring(0, xmlFileName[FilePathIndex].InnerText.Length - 4);
                                    lvi.SubItems[3].Text = FileHash;
                                    lvi.ImageIndex = 0;
                                    this.BeginInvoke(new MethodInvoker(() => {
                                        lvImages.Items.Add(lvi);
                                    }));
                                }
                            }
                        }
                        #endregion

                        this.BeginInvoke(new MethodInvoker(() => {
                            lbTotalFiles.Text = CurrentThread.ThreadImagesCount.ToString();
                            lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                            lbScanTimer.Text = "Downloading files";
                            MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                        }));
                        #endregion
                    }

                    #region Download logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {
                        if (CurrentThread.ImageFiles[ImageFilesIndex] != null) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            }));
                            string FileName = CurrentThread.FileNames[ImageFilesIndex];
                            CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];

                            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                            if (Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath, CurrentThread.FileNames[ImageFilesIndex] + CurrentThread.FileExtensions[ImageFilesIndex])) {
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    CurrentURL = CurrentThread.ThumbnailFiles[ImageFilesIndex];
                                    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                    Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath + "\\thumb", CurrentThread.ThumbnailNames[ImageFilesIndex] + ".jpg");
                                }

                                CurrentThread.DownloadedImagesCount++;

                                this.BeginInvoke(new MethodInvoker(() => {
                                    lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                                }));

                            }
                            else {
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                                }));
                            }
                        }

                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }


                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.FileWas404 = false;
                    CurrentThread.RetryCountFor404 = 0;
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    Debug.Print(CurrentURL);
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "8chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region 8kun Download Logic Completed. (Rescans from Beginning)
        private void Set8kunThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL_fpath = "https://media.8kun.top/file_store/";
                string ThumbnailFileBaseURL_fpath = "https://media.8kun.top/file_store/thumb/";
                string FileBaseURL = "https://media.8kun.top/{0}/src/{1}";
                string ThumbnailFileBaseURL = "https://media.8kun.top/{0}/thumb/{1}.jpg";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    if (!CurrentThread.FileWas404) {
                        #region API/HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                        CurrentURL = string.Format(Networking.GetAPILink(CurrentThread.Chan), CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                        ThreadJSON = GetThreadJSON(CurrentURL);

                        if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Networking.EmptyXML) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                            return;
                        }

                        CurrentURL = this.CurrentThread.ThreadURL;

                        if (Downloads.Default.SaveHTML || General.Default.UseFullBoardNameForTitle && !CurrentThread.RetrievedBoardName) {
                            ThreadHTML = GetThreadHTML(CurrentURL, true);

                            int TitleExtraLength = 5 + CurrentThread.ThreadBoard.Length;
                            CurrentThread.BoardName = ThreadHTML.Substring(
                                ThreadHTML.IndexOf("<h1>") + (4 + TitleExtraLength),
                                ThreadHTML.IndexOf("</h1>") - ThreadHTML.IndexOf("<h1>") - (4 + TitleExtraLength)
                            );

                            this.BeginInvoke(new MethodInvoker(() => {
                                this.Text = string.Format("8kun thread - {0} - {1}", CurrentThread.BoardName, CurrentThread.ThreadID);
                            }));

                            CurrentThread.RetrievedBoardName = true;

                            Thread.Sleep(100);
                        }
                        #endregion

                        #region API Parsing logic
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);

                    XmlNodeList xmlPosts = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                    for (int ThreadPostIndex = 0; ThreadPostIndex < xmlPosts.Count - 1; ThreadPostIndex++) {
                        if (xmlPosts[ThreadPostIndex] != null) {
                            XmlNodeList xmlPostID = xmlPosts[ThreadPostIndex].SelectNodes("no");
                            XmlNodeList xmlFpath = xmlPosts[ThreadPostIndex].SelectNodes("fpath");
                            string xPostID = xmlPostID[0].InnerText;

                            XmlNodeList xmlFileID = xmlPosts[ThreadPostIndex].SelectNodes("tim");
                            XmlNodeList xmlFileName = xmlPosts[ThreadPostIndex].SelectNodes("filename");
                            XmlNodeList xmlExtension = xmlPosts[ThreadPostIndex].SelectNodes("ext");
                            XmlNodeList xmlMd5 = xmlPosts[ThreadPostIndex].SelectNodes("md5");
                            if (xmlFileID.Count > 0) {
                                string xFpath = xmlFpath[0].InnerText;

                                #region FirstFile
                                for (int FileIdIndex = 0; FileIdIndex < xmlFileID.Count; FileIdIndex++) {
                                    if (xmlFileID[FileIdIndex] == null) {
                                        continue;
                                    }
                                    string xFileID = xmlFileID[FileIdIndex].InnerText;
                                    if (!CurrentThread.FileIDs.Contains(xFileID)) {
                                        string xExtension = xmlExtension[FileIdIndex].InnerText;
                                        string xMD5 = xmlMd5[FileIdIndex].InnerText;
                                        string xFileName = xmlFileName[FileIdIndex].InnerText;
                                        CurrentThread.FileIDs.Add(xFileID);
                                        switch (xFpath) {
                                            case "0":
                                                CurrentThread.ImageFiles.Add(string.Format(FileBaseURL, CurrentThread.ThreadBoard, xFileID + xExtension));
                                                CurrentThread.ThumbnailFiles.Add(string.Format(ThumbnailFileBaseURL, CurrentThread.ThreadBoard, xFileID));
                                                break;
                                            default:
                                                CurrentThread.ImageFiles.Add(FileBaseURL_fpath + xFileID + xExtension);
                                                CurrentThread.ThumbnailFiles.Add(ThumbnailFileBaseURL_fpath + xFileID + xExtension);
                                                break;
                                        }
                                        CurrentThread.ThumbnailNames.Add(xFileID + xExtension);
                                        CurrentThread.FileExtensions.Add(xExtension);
                                        CurrentThread.FileHashes.Add(xMD5);

                                        string FileName = xFileID;
                                        if (Downloads.Default.SaveOriginalFilenames) {
                                            FileName = xFileName;
                                            string FileNamePrefix = string.Empty;
                                            string FileNameSuffix = string.Empty;

                                            if (Downloads.Default.PreventDuplicates) {
                                                if (CurrentThread.FileOriginalNames.Contains(FileName)) {
                                                    if (CurrentThread.FileNamesDupes.Contains(FileName)) {
                                                        int DupeNameIndex = CurrentThread.FileNamesDupes.IndexOf(FileName);
                                                        CurrentThread.FileNamesDupesCount[DupeNameIndex] += 1;
                                                        FileNameSuffix = " (dupe " + CurrentThread.FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                                    }
                                                    else {
                                                        CurrentThread.FileNamesDupes.Add(FileName);
                                                        CurrentThread.FileNamesDupesCount.Add(1);
                                                        FileNameSuffix = " (dupe 1)";
                                                    }
                                                }
                                            }

                                            for (int j = 0; j < Networking.InvalidFileCharacters.Length; j++) {
                                                FileName = FileName.Replace(Networking.InvalidFileCharacters[j], "_");
                                            }

                                            FileName = FileNamePrefix + FileName + FileNameSuffix;
                                        }

                                        CurrentThread.FileOriginalNames.Add(xFileName);
                                        CurrentThread.FileNames.Add(FileName + xExtension);

                                        if (YChanEx.Downloads.Default.SaveHTML) {
                                            string OldHTMLLinks = null;
                                            switch (xFpath) {
                                                case "0":
                                                    if (YChanEx.Downloads.Default.SaveThumbnails) {
                                                        OldHTMLLinks = ThumbnailFileBaseURL;
                                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                                    }
                                                    OldHTMLLinks = FileBaseURL + xFileID;
                                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                                                    break;
                                                default:
                                                    if (YChanEx.Downloads.Default.SaveThumbnails) {
                                                        OldHTMLLinks = ThumbnailFileBaseURL_fpath;
                                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                                    }
                                                    OldHTMLLinks = FileBaseURL_fpath + xFileID;
                                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                                                    break;
                                            }
                                        }

                                        ListViewItem lvi = new ListViewItem();
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.Name = xFileID;
                                        lvi.SubItems[0].Text = xPostID + "-" + (FileIdIndex + 1); //xFileID;
                                        lvi.SubItems[1].Text = xExtension.Trim('.');
                                        lvi.SubItems[2].Text = xFileName;
                                        lvi.SubItems[3].Text = xMD5;
                                        lvi.ImageIndex = 0;
                                        this.BeginInvoke(new MethodInvoker(() => {
                                            lvImages.Items.Add(lvi);
                                        }));

                                        CurrentThread.ThreadImagesCount++;
                                        CurrentThread.ThreadPostsCount++;
                                    }
                                }
                                #endregion

                                #region Extra Files
                                xmlFileID = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/tim");
                                xmlFileName = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/filename");
                                xmlExtension = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/ext");
                                xmlMd5 = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/md5");
                                for (int FileIdIndex = 0; FileIdIndex < xmlFileID.Count; FileIdIndex++) {
                                    if (xmlFileID[FileIdIndex] == null) {
                                        continue;
                                    }
                                    string xFileID = xmlFileID[FileIdIndex].InnerText;
                                    if (!CurrentThread.FileIDs.Contains(xFileID)) {
                                        string xExtension = xmlExtension[FileIdIndex].InnerText;
                                        string xMD5 = xmlMd5[FileIdIndex].InnerText;
                                        string xFileName = xmlFileName[FileIdIndex].InnerText;

                                        CurrentThread.FileIDs.Add(xFileID);
                                        switch (xFpath) {
                                            case "0":
                                                CurrentThread.ImageFiles.Add(string.Format(FileBaseURL, CurrentThread.ThreadBoard, xFileID + xExtension));
                                                CurrentThread.ThumbnailFiles.Add(string.Format(ThumbnailFileBaseURL, CurrentThread.ThreadBoard, xFileID));
                                                break;
                                            default:
                                                CurrentThread.ImageFiles.Add(FileBaseURL_fpath + xFileID + xExtension);
                                                CurrentThread.ThumbnailFiles.Add(ThumbnailFileBaseURL_fpath + xFileID + xExtension);
                                                break;
                                        }
                                        CurrentThread.ThumbnailNames.Add(xFileID + xExtension);
                                        CurrentThread.FileExtensions.Add(xExtension);
                                        CurrentThread.FileHashes.Add(xMD5);

                                        string FileName = xFileID;
                                        if (Downloads.Default.SaveOriginalFilenames) {
                                            FileName = xFileName;
                                            string FileNamePrefix = string.Empty;
                                            string FileNameSuffix = string.Empty;

                                            if (Downloads.Default.PreventDuplicates) {
                                                if (CurrentThread.FileOriginalNames.Contains(FileName)) {
                                                    if (CurrentThread.FileNamesDupes.Contains(FileName)) {
                                                        int DupeNameIndex = CurrentThread.FileNamesDupes.IndexOf(FileName);
                                                        CurrentThread.FileNamesDupesCount[DupeNameIndex] += 1;
                                                        FileNameSuffix = " (dupe " + CurrentThread.FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                                    }
                                                    else {
                                                        CurrentThread.FileNamesDupes.Add(FileName);
                                                        CurrentThread.FileNamesDupesCount.Add(1);
                                                        FileNameSuffix = " (dupe 1)";
                                                    }
                                                }
                                            }

                                            for (int j = 0; j < Networking.InvalidFileCharacters.Length; j++) {
                                                FileName = FileName.Replace(Networking.InvalidFileCharacters[j], "_");
                                            }

                                            FileName = FileNamePrefix + FileName;
                                        }

                                        CurrentThread.FileOriginalNames.Add(xFileName);
                                        CurrentThread.FileNames.Add(FileName + xExtension);

                                        if (YChanEx.Downloads.Default.SaveHTML) {
                                            string OldHTMLLinks = null;
                                            switch (xFpath) {
                                                case "0":
                                                    if (YChanEx.Downloads.Default.SaveThumbnails) {
                                                        OldHTMLLinks = ThumbnailFileBaseURL;
                                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                                    }
                                                    OldHTMLLinks = FileBaseURL + xFileID;
                                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                                                    break;
                                                default:
                                                    if (YChanEx.Downloads.Default.SaveThumbnails) {
                                                        OldHTMLLinks = ThumbnailFileBaseURL_fpath;
                                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                                    }
                                                    OldHTMLLinks = FileBaseURL_fpath + xFileID;
                                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                                                    break;
                                            }
                                        }

                                        ListViewItem lvi = new ListViewItem();
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                        lvi.Name = xFileID;
                                        lvi.SubItems[0].Text = xPostID + "-" + (FileIdIndex + 2); //xFileID;
                                        lvi.SubItems[1].Text = xExtension.Trim('.');
                                        lvi.SubItems[2].Text = xFileName;
                                        lvi.SubItems[3].Text = xMD5;
                                        lvi.ImageIndex = 0;
                                        this.BeginInvoke(new MethodInvoker(() => {
                                            lvImages.Items.Add(lvi);
                                        }));

                                        CurrentThread.ThreadImagesCount++;
                                    }
                                }
                            }
                            #endregion

                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = (CurrentThread.ThreadImagesCount + CurrentThread.ExtraFilesImageCount).ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion
                    }

                    #region Download logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {
                        if (CurrentThread.ImageFiles[ImageFilesIndex] != null) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            }));
                            string FileName = CurrentThread.FileNames[ImageFilesIndex];
                            CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];

                            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                            if (Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath, FileName)) {
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    CurrentURL = CurrentThread.ThumbnailFiles[ImageFilesIndex];
                                    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                    Networking.DownloadFile(CurrentURL, CurrentThread.DownloadPath + "\\thumb", CurrentThread.FileIDs[ImageFilesIndex] + CurrentThread.FileExtensions[ImageFilesIndex]);
                                }

                                CurrentThread.DownloadedImagesCount++;

                                this.BeginInvoke(new MethodInvoker(() => {
                                    lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                                }));
                            }
                            else {
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                                }));
                            }
                        }
                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }

                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.FileWas404 = false;
                    CurrentThread.RetryCountFor404 = 0;
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "8kun thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region fchan Download Logic Works, very poorly.
        /* here's some information.
         * fchan is parsed using html, but even then it's inconsistent.
         * some file names have underscores after the file ID, or a period.
         * do people use fchan?
         * the main problem is, the regex will find the right lines, but I can only
         * guess the substrings to make it work... but it just sometimes doesn't work.
         * Unless fchan can implement an API, I'm not going to be updating this.
         * It will sometimes work, it sometimes won't.
         * That's all. I'm tired of fchan's HTML guessing game.
         */
        private void SetFchanThread() {
            DownloadThread = new Thread(() => {
                string BaseURL = "http://fchan.us/";
                string ThreadHTML = null;
                string CurrentURL = null;
                try {

                    if (!CurrentThread.FileWas404) {
                        #region HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;
                    
                        for (int TryCount = 0; TryCount < 5; TryCount++) {
                            CurrentURL = CurrentThread.ThreadURL;
                            ThreadHTML = GetThreadHTML(CurrentURL);

                            if (string.IsNullOrEmpty(ThreadHTML)) {
                                if (TryCount == 5) {
                                    CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                                    return;
                                }
                                Thread.Sleep(5000);
                            }
                            else {
                                break;
                            }
                        }

                        if (ThreadHTML == CurrentThread.LastThreadHTML) {
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.LastThreadHTML = ThreadHTML;
                        #endregion

                        #region HTML Parsing logic
                    MatchCollection NameMatches = new Regex(ChanRegex.fchanNames).Matches(ThreadHTML);
                    MatchCollection PostIDMatches = new Regex(ChanRegex.DefaultRegex.fchanIDs).Matches(ThreadHTML);
                    for (int PostMatchesIndex = 0; PostMatchesIndex < NameMatches.Count; PostMatchesIndex++) {
                        string IDMatch = PostIDMatches[PostMatchesIndex].Value;
                        string PostID = IDMatch.Substring(0, IDMatch.Length - 7).Substring(12);
                        if (!CurrentThread.FileIDs.Contains(PostID)) {
                            string NameMatch = NameMatches[PostMatchesIndex].Value;
                            string FileMatch = NameMatch.Substring(0, NameMatch.IndexOf("\" rel=\""));
                            int IndexOfFullFileName = NameMatch.IndexOf('>') + 1;

                            string FullFileName = FileMatch.Substring(5);                       // file name saved on fchan
                            string FileExtension = "." + FullFileName.Split('.')[FullFileName.Split('.').Length - 1];   // file extension
                            string FileName = FullFileName.Substring(0, FullFileName.Length - FileExtension.Length);    // file name w/o ext
                            //string OriginalFileName = NameMatch.Substring(IndexOfFullFileName);                   // original file name
                            //OriginalFileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);

                            CurrentThread.FileIDs.Add(PostID);
                            //CurrentThread.PostOriginalNames.Add(OriginalFileName);
                            CurrentThread.FileExtensions.Add(FileExtension);
                            CurrentThread.ImageFiles.Add(BaseURL + "/src/" + FullFileName.Trim('/'));

                            // I hate fchan, holy god I hate it so.
                            // Why can't they have regular locations for original file names
                            // killing myself.

                            //if (Downloads.Default.SaveCurrentThread.PostOriginalNames) {
                            //    FileName = OriginalFileName;
                            //    string FileNamePrefix = "";
                            //    string FileNameSuffix = "";

                            //    for (int IllegalCharacterIndex = 0; IllegalCharacterIndex < Chans.InvalidFileCharacters.Length; IllegalCharacterIndex++) {
                            //        FileName = FileName.Replace(Chans.InvalidFileCharacters[IllegalCharacterIndex], "_");
                            //    }

                            //    if (Downloads.Default.PreventDuplicates) {
                            //        if (FileNames.Contains(FileName)) {
                            //            if (FileNamesDupes.Contains(FileName)) {
                            //                int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                            //                FileNamesDupesCount[DupeNameIndex] += 1;
                            //                FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                            //            }
                            //            else {
                            //                FileNamesDupes.Add(FileName);
                            //                FileNamesDupesCount.Add(1);
                            //                FileNameSuffix = " (dupe 1)";
                            //            }
                            //        }
                            //    }

                            //    FileName = FileNamePrefix + FileName + FileNameSuffix;
                            //}

                            CurrentThread.FileNames.Add(FileName + FileExtension);

                            if (Downloads.Default.SaveThumbnails) {
                                // trim the board name length + 14 for the image generated information before the 
                                string ThumbnailName = FullFileName.Substring(0, CurrentThread.ThreadBoard.Length + 14) + "s";
                                ThumbnailName += FullFileName.Substring(CurrentThread.ThreadBoard.Length + 14, FullFileName.Length - (CurrentThread.ThreadBoard.Length + 14));
                                string ThumbnailLink = BaseURL + CurrentThread.ThreadBoard + "/thumb/" + ThumbnailName.Substring(0, ThumbnailName.Length - FileExtension.Length).Trim('/');
                                CurrentThread.ThumbnailNames.Add(ThumbnailName);
                                CurrentThread.ThumbnailFiles.Add(ThumbnailLink + ".jpg");

                                if (Downloads.Default.SaveHTML) {
                                    ThreadHTML = ThreadHTML.Replace("src=\"/" + CurrentThread.ThreadBoard + "/thumb/" + ThumbnailName, "src=\"thumb/" + ThumbnailName);
                                }
                            }

                            if (Downloads.Default.SaveHTML) {
                                ThreadHTML = ThreadHTML.Replace("/src/" + CurrentThread.ThreadBoard + "/" + FullFileName, FileName);
                            }

                            CurrentThread.ThreadPostsCount++;

                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = PostID;
                            lvi.SubItems[0].Text = PostID;
                            lvi.SubItems[1].Text = FileExtension;
                            lvi.SubItems[2].Text = FileName;
                            lvi.ImageIndex = 0;
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = CurrentThread.ImageFiles.Count.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion
                    }

                    #region Download logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));

                        CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];
                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Networking.DownloadFile(CurrentThread.ImageFiles[ImageFilesIndex], CurrentThread.DownloadPath, CurrentThread.FileNames[ImageFilesIndex], "disclaimer=seen")) {
                            if (Downloads.Default.SaveThumbnails) {
                                CurrentURL = CurrentThread.ThumbnailFiles[ImageFilesIndex];
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Networking.DownloadFile(CurrentThread.ThumbnailFiles[ImageFilesIndex], CurrentThread.DownloadPath + "\\thumb\\", CurrentThread.ThumbnailNames[ImageFilesIndex], "disclaimer=seen");
                            }

                            CurrentThread.DownloadedImagesCount++;

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                            }));
                        }
                        else {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                            }));
                        }


                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }

                    if (Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "fchan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region u18chan Download Logic Basically completed Needs: Fixed HTML replacement.
        private void Setu18ChanThread() {
            DownloadThread = new Thread(() => {
                string ThreadHTML = null;
                string CurrentURL = null;
                try {

                    if (!CurrentThread.FileWas404) {
                        #region HTML Download Logic
                        if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                            CurrentThread.CurrentActivity = ThreadStatus.ThreadInfoNotSet;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.CurrentActivity = ThreadStatus.ThreadScanning;

                        for (int TryCount = 0; TryCount < 5; TryCount++) {
                            CurrentURL = CurrentThread.ThreadURL;
                            ThreadHTML = GetThreadHTML(CurrentURL);

                            if (string.IsNullOrEmpty(ThreadHTML)) {
                                if (TryCount == 5) {
                                    CurrentThread.CurrentActivity = ThreadStatus.ThreadImproperlyDownloaded;
                                    return;
                                }
                                Thread.Sleep(5000);
                            }
                            else {
                                break;
                            }
                        }

                        if (ThreadHTML == CurrentThread.LastThreadHTML) {
                            CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                            ManageThread(ThreadEvent.AfterDownload);
                            return;
                        }

                        CurrentThread.LastThreadHTML = ThreadHTML;
                        #endregion

                        #region HTML Parsing logic
                    MatchCollection PostMatches = new Regex(ChanRegex.u18chanPosts).Matches(ThreadHTML);
                    for (int PostMatchesIndex = 0; PostMatchesIndex < PostMatches.Count; PostMatchesIndex++) {
                        if (PostMatches[PostMatchesIndex] != null) {
                            string MatchValue = PostMatches[PostMatchesIndex].Value;
                            int IndexOfTag = MatchValue.IndexOf('<');
                            string PostID = MatchValue.Substring(IndexOfTag + 14).Substring(0, 8).Trim('_');
                            if (!CurrentThread.FileIDs.Contains(PostID)) {
                                IndexOfTag = MatchValue.IndexOf('>');
                                string FileLink = MatchValue.Substring(0, IndexOfTag - 1);

                                string FileName = FileLink.Split('/')[FileLink.Split('/').Length - 1];
                                string FileExtension = "." + FileName.Split('.')[FileName.Split('.').Length - 1];
                                FileName = FileName.Substring(0, FileName.Length - FileExtension.Length);

                                CurrentThread.FileOriginalNames.Add(FileName);
                                CurrentThread.FileExtensions.Add(FileExtension);
                                CurrentThread.ImageFiles.Add(FileLink);

                                if (Downloads.Default.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";

                                    do {
                                        FileName = FileName.Substring(0, FileName.Length - 8);
                                    } while (FileName.EndsWith("_u18chan"));

                                    for (int IllegalCharacterIndex = 0; IllegalCharacterIndex < Networking.InvalidFileCharacters.Length; IllegalCharacterIndex++) {
                                        FileName = FileName.Replace(Networking.InvalidFileCharacters[IllegalCharacterIndex], "_");
                                    }

                                    if (Downloads.Default.PreventDuplicates) {
                                        if (CurrentThread.FileNames.Contains(FileName)) {
                                            if (CurrentThread.FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = CurrentThread.FileNamesDupes.IndexOf(FileName);
                                                CurrentThread.FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNameSuffix = " (dupe " + CurrentThread.FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                            }
                                            else {
                                                CurrentThread.FileNamesDupes.Add(FileName);
                                                CurrentThread.FileNamesDupesCount.Add(1);
                                                FileNameSuffix = " (dupe 1)";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                }

                                CurrentThread.FileNames.Add(FileName + FileExtension);

                                if (Downloads.Default.SaveThumbnails) {
                                    string ThumbnailName = FileName + "s";
                                    string ThumbnailLink = FileLink.Substring(0, FileLink.Length - 12) + "s_u18chan" + FileExtension;
                                    CurrentThread.ThumbnailNames.Add(ThumbnailName + FileExtension);
                                    CurrentThread.ThumbnailFiles.Add(ThumbnailLink);

                                    if (Downloads.Default.SaveHTML) {
                                        ThreadHTML = ThreadHTML.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=", "src=\"");
                                        ThreadHTML = ThreadHTML.Replace(ThumbnailLink, "thumb/" + ThumbnailLink.Split('/')[ThumbnailLink.Split('/').Length - 1]);
                                    }
                                }

                                if (Downloads.Default.SaveHTML) {
                                    ThreadHTML = ThreadHTML.Replace(FileLink, FileName + FileExtension);
                                }

                                ListViewItem lvi = new ListViewItem();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = PostID;
                                lvi.SubItems[0].Text = PostID;
                                lvi.SubItems[1].Text = FileExtension;
                                lvi.SubItems[2].Text = FileName;
                                lvi.ImageIndex = 0;
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items.Add(lvi);
                                }));

                                CurrentThread.ThreadImagesCount++;
                                CurrentThread.ThreadPostsCount++;
                            }
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = CurrentThread.ThreadImagesCount.ToString();
                        lbLastModified.Text = "last modified: " + CurrentThread.LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion
                    }

                    #region Download logic
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadDownloading;
                    CurrentThread.DownloadingFiles = true;

                    for (int ImageFilesIndex = CurrentThread.DownloadedImagesCount; ImageFilesIndex < CurrentThread.ImageFiles.Count; ImageFilesIndex++) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));
                        CurrentURL = CurrentThread.ImageFiles[ImageFilesIndex];

                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Networking.DownloadFile(CurrentThread.ImageFiles[ImageFilesIndex], CurrentThread.DownloadPath, CurrentThread.FileNames[ImageFilesIndex])) {
                            CurrentThread.DownloadedImagesCount++;

                            if (Downloads.Default.SaveThumbnails) {
                                CurrentURL = CurrentThread.ThumbnailFiles[ImageFilesIndex];
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Networking.DownloadFile(CurrentThread.ThumbnailFiles[ImageFilesIndex], CurrentThread.DownloadPath + "\\thumb", CurrentThread.ThumbnailNames[ImageFilesIndex]);
                            }

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = CurrentThread.DownloadedImagesCount.ToString();
                                lvImages.Items[ImageFilesIndex].ImageIndex = 2;
                            }));
                        }
                        else {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 3;
                            }));
                        }

                        if (PauseBetweenFiles) { Thread.Sleep(100); }
                    }

                    if (Downloads.Default.SaveHTML) {
                        File.WriteAllText(CurrentThread.DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    CurrentThread.DownloadingFiles = false;
                    #endregion

                    CurrentThread.FileWas404 = false;
                    CurrentThread.RetryCountFor404 = 0;
                    CurrentThread.CurrentActivity = ThreadStatus.Waiting;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.CurrentActivity = ThreadStatus.ThreadIsAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    HandleWebException(WebEx, CurrentURL);
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    this.BeginInvoke((MethodInvoker)delegate() {
                        ManageThread(ThreadEvent.AfterDownload);
                    });
                }
            });
            DownloadThread.Name = "u18chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

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

    }
}