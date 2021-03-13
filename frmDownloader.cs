using System;
using System.Collections.Generic;
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

        private ThreadInfo CurrentThread;                           // all, the ThreadInfo for information relating to the thread.
                                                                    // the information that is in this class is:
                                                                    // - Chan ID
                                                                    // - ThreadURL
                                                                    // - ThreadID
                                                                    // - ThreadBoard

        public string DownloadPath = null;                          // all, the local directory for the files to save to.
        public string ThreadURL = null;                             // all, the URL passed from the main form.
        public ChanType Chan = ChanType.None;                       // all, the int-based chan type.
        public string PublicThreadID = null;

        private string LastThreadHTML = null;                       // 7chan fchan u18chan, the last HTML of the thread.
        //  (used as a make-shift if-modified-since header)
        private DateTime LastModified = default(DateTime);           // 4chan 7chan 8chan,
        // uses If-Modified-Since header on requests
        // to prevent repeat requests overloading their servers.
        // all logic includes this, but only some make use of it.

        private List<string> ImageFiles = new List<string>();       // all, list of file links.
        private List<string> ThumbnailFiles = new List<string>();   // all, list of thumbnail file links.
        private List<string> ThumbnailNames = new List<string>();   // 8chan 8kun, list of thumbnail file names.
        private List<string> FileIDs = new List<string>();          // all, list of file ids.
        private List<string> FileNames = new List<string>();        // all, list of file names.
        private List<string> OriginalFileNames = new List<string>();// all, list of original file names.
        private List<string> FileNamesDupes = new List<string>();   // all, contains the stringed names of duplicate files.
        private List<string> FileHashes = new List<string>();       // all, list of file hashes.
        private List<string> FileExtensions = new List<string>();   // all, list of file extensions.
        private List<int> FileNamesDupesCount = new List<int>();    // all, contains the amount of files with the same name.
        private bool ThreadScanned = false;         // all, Prevents thread data (ThreadBoard, ThreadID ...) from being rewrote on rescans.
        //private bool DownloadThread404 = false;     // all, determines if a thread 404'd.
        //private bool DownloadAborted = false;       // all, determines if a thread was aborted.
        private bool RetrievedBoardName = false;    // 8chan 8kun, determines if the board title was retrieved from HTML.
        private int ThreadImagesCount = 0;          // all, counts the images in the thread. restarts parsing at this index.
        private int DownloadedImagesCount = 0;      // all, counts the images that have downloaded.
        private int ExtraFilesImageCount = 0;       // 8kun, !LEGACY! restarts parsing extra files at this index.
        private int ThreadPostsCount = 0;           // 8chan 8kun, restarts the parsing at this index.
        private int CountdownToNextScan = 0;        // all, countdown between rescans.
        private int HideModifiedLabelAt = 0;        // all, hides the modified at 10 seconds less of CountdownToNextScan.
        private string BoardName = null;            // 8chan 8kun, the retrieved board name from HTML.
        private Thread DownloadThread;              // all, the main download thread.
        private Thread TimerIdle;                   // all, the timer idler for when the settings form is open.

        // Mostly-debug
        private bool UseOldLogic = false;               // all, maybe user-set. Uses old parsing logic instead of latest.
        private bool UseConfirmedWorkingLogic = false;  // all, if any of them use non-fully tested logic. debug only.
        private bool MessageBoxPerFile = false;         // all, debug to display a message box of the URL before download
        private bool PauseBetweenFiles = true;         // all, temp pauses between file downloads.
        #endregion

        #region Form Controls
        public frmDownloader() {
            InitializeComponent();
            if (Program.IsDebug) {
                btnForce404.Enabled = true;
                btnForce404.Visible = true;
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
                if (TimerIdle != null && TimerIdle.IsAlive) {
                    TimerIdle.Abort();
                }
                TimerIdle = new Thread(() => {
                    try {
                        Thread.Sleep(1000);
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

            if (CurrentThread.Status == ThreadStatus.Thread404) {
                lbScanTimer.Text = "404'd";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                this.Icon = Properties.Resources.YChanEx404;

                //MainFormInstance.Announce404(ThreadID, ThreadBoard, ThreadURL, Chan);
                CurrentThread.Status = ThreadStatus.Thread404;
                MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.Status);
                MainFormInstance.AnnounceEvent(ThreadEvent.Thread404, CurrentThread);
                btnAbortRetry.Text = "Retry";
                //btnStopDownload.Enabled = false;
                tmrScan.Stop();
                return;
            }
            if (CountdownToNextScan == HideModifiedLabelAt) {
                lbNotModified.Visible = false;
                lbScanTimer.Text = CountdownToNextScan.ToString();
                CountdownToNextScan--;
            }
            else if (CountdownToNextScan == 0) {
                StartDownload();
                CurrentThread.Status = ThreadStatus.ThreadScanning;
                lbScanTimer.Text = "scanning now...";
                MainFormInstance.SetItemStatus(ThreadURL, CurrentThread.Status);
                tmrScan.Stop();
            }
            else {
                lbScanTimer.Text = CountdownToNextScan.ToString();
                CountdownToNextScan--;
            }
        }
        private void lvImages_MouseDoubleClick(object sender, MouseEventArgs e) {
            for (int i = 0; i < lvImages.SelectedItems.Count; i++) {
                if (File.Exists(DownloadPath + "\\" + FileNames[lvImages.SelectedItems[i].Index])) {
                    System.Diagnostics.Process.Start(DownloadPath + "\\" + FileNames[lvImages.SelectedItems[i].Index]);
                }
            }
        }
        private void btnForce404_Click(object sender, EventArgs e) {
            if (Program.IsDebug) {
                CurrentThread.Status = ThreadStatus.Thread404;
                btnForce404.Enabled = false;
                AfterDownload();
            }
        }
        private void btnAbortRetry_Click(object sender, EventArgs e) {
            switch (CurrentThread.Status) {
                case ThreadStatus.Thread404:
                case ThreadStatus.ThreadAborted:
                    CurrentThread.Status = ThreadStatus.ThreadScanning;
                    btnAbortRetry.Text = "Abort";
                    if (Program.IsDebug) {
                        btnForce404.Enabled = true;
                    }
                    RetryScanOnFailure();
                    break;
                default:
                    StopDownload();
                    btnAbortRetry.Text = "Retry";
                    lbNotModified.Visible = false;
                    if (Program.IsDebug) {
                        btnForce404.Enabled = false;
                    }
                    break;
            }
        }
        private void btnOpenFolder_Click(object sender, EventArgs e) {
            if (DownloadPath == null) { return; }

            if (Directory.Exists(DownloadPath)) {
                System.Diagnostics.Process.Start(DownloadPath);
            }
        }
        private void btnClose_Click(object sender, EventArgs e) {
            this.Hide();
        }
        #endregion


        #region cmThreadActions
        private void mOpenThreadDownloadFolder_Click(object sender, EventArgs e) {
            if (System.IO.Directory.Exists(DownloadPath)) {
                System.Diagnostics.Process.Start(DownloadPath);
            }
        }

        private void mOpenThreadInBrowser_Click(object sender, EventArgs e) {
            if (ThreadURL != null) {
                System.Diagnostics.Process.Start(ThreadURL);
            }
        }

        private void mCopyThreadID_Click(object sender, EventArgs e) {
            if (CurrentThread.ThreadID != null) {
                Clipboard.SetText(CurrentThread.ThreadID);
            }
        }

        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (ThreadURL != null) {
                Clipboard.SetText(ThreadURL);
            }
        }
        #endregion

        #region Custom Thread Methods
        public void StartDownload(bool ScanThread = true) {
            CurrentThread = new ThreadInfo();
            CurrentThread.ThreadURL = ThreadURL;
            CurrentThread.Chan = Chan;

            switch (Chan) {
                case ChanType.FourChan:
                    if (!ThreadScanned) {
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("4chan CurrentThread.Thread - {0} - {1}", BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\4chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\4chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    Set4chanThread();
                    break;
                case ChanType.FourTwentyChan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 4];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 2].Split('#')[0];
                        this.Text = string.Format("420chan CurrentThread.Thread - {0} - {1}", BoardTitles.FourTwentyChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\420chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    Set420chanThread();
                    break;
                case ChanType.SevenChan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "");
                        this.Text = string.Format("7chan CurrentThread.Thread - {0} - {1}", BoardTitles.SevenChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\7chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\7chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    Set7chanThread();
                    break;
                case ChanType.EightChan:
                    if (!ThreadScanned) {
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        this.Text = string.Format("8chan CurrentThread.Thread - {0} - {1}", BoardTitles.EightChan(CurrentThread.ThreadBoard, false), CurrentThread.ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\8chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\8chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    Set8chanThread();
                    break;
                case ChanType.EightKun:
                    if (!ThreadScanned) {
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        this.Text = string.Format("8kun CurrentThread.Thread - {0} - {1}", BoardTitles.EightKun(CurrentThread.ThreadBoard, false), CurrentThread.ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\8kun\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\8kun\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    Set8kunThread();
                    break;
                case ChanType.fchan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "");
                        this.Text = string.Format("fchan CurrentThread.Thread - {0} - {1}", BoardTitles.fchan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\fchan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\fchan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    SetFchanThread();
                    break;
                case ChanType.u18chan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = CurrentThread.ThreadURL.Split('/');
                        CurrentThread.ThreadBoard = URLSplit[URLSplit.Length - 3];
                        CurrentThread.ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("u18chan CurrentThread.Thread - {0} - {1}", BoardTitles.u18chan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\u18chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\u18chan\\" + CurrentThread.ThreadBoard + "\\" + CurrentThread.ThreadID;
                    }
                    Setu18ChanThread();
                    break;
                default:
                    MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.UnknownStatus);
                    return;
            }

            if (DownloadPath != null) {
                btnOpenFolder.Enabled = true;
            }

            //PublicThreadID = ThreadID;
            HideModifiedLabelAt = Downloads.Default.ScannerDelay - 10;
            if (ScanThread) {
                DownloadThread.Start();
                MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadScanning);
            }
        }

        public void StopDownload() {
            tmrScan.Stop();
            if (DownloadThread != null && DownloadThread.IsAlive) {
                DownloadThread.Abort();
            }
            lbScanTimer.Text = "Aborted";
            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
            //btnStopDownload.Enabled = false;
            CurrentThread.Status = ThreadStatus.ThreadAborted;
            MainFormInstance.AnnounceEvent(ThreadEvent.ThreadAborted, CurrentThread);
            MainFormInstance.SetItemStatus(ThreadURL, CurrentThread.Status);
        }

        public void AfterDownload() {
            if (CurrentThread.Status == ThreadStatus.ThreadAborted) {
                return;
            }
            else {
                this.BeginInvoke(new MethodInvoker(() => {
                    CurrentThread.Status = ThreadStatus.Waiting;
                    lbScanTimer.Text = "soon (tm)";
                    MainFormInstance.SetItemStatus(CurrentThread.ThreadURL, CurrentThread.Status);
                    CountdownToNextScan = Downloads.Default.ScannerDelay - 1;
                    if (Program.IsDebug) {
                        //CountdownToNextScan = 9;
                    }
                    tmrScan.Start();
                }));
                GC.Collect();
            }
        }

        public void RetryScanOnFailure() {
            MainFormInstance.Un404Thread(ThreadURL);
            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
            StartDownload();
            lbScanTimer.Text = "scanning now...";
            btnAbortRetry.Text = "Abort";
            tmrScan.Stop();
        }

        public void AbortDownloadForClosing() {
            if (DownloadThread != null && DownloadThread.IsAlive) {
                DownloadThread.Abort();
            }
        }

        public void ChangeFormTitle() {
            switch (Chan) {
                case ChanType.FourChan:
                    this.Text = string.Format("4chan thread - {0} - {1}", BoardTitles.FourChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    break;
                case ChanType.FourTwentyChan:
                    this.Text = string.Format("420chan thread - {0} - {1}", BoardTitles.FourTwentyChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    break;
                case ChanType.SevenChan:
                    this.Text = string.Format("7chan thread - {0} - {1}", BoardTitles.SevenChan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    break;
                case ChanType.EightChan:
                    this.Text = string.Format("8chan thread - {0} - {1}", BoardTitles.EightChan(CurrentThread.ThreadBoard, false), CurrentThread.ThreadID);
                    break;
                case ChanType.EightKun:
                    this.Text = string.Format("8kun thread - {0} - {1}", BoardTitles.EightKun(CurrentThread.ThreadBoard, false), CurrentThread.ThreadID);
                    break;
                case ChanType.fchan:
                    this.Text = string.Format("fchan thread - {0} - {1}", BoardTitles.fchan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    break;
                case ChanType.u18chan:
                    this.Text = string.Format("u18chan thread - {0} - {1}", BoardTitles.u18chan(CurrentThread.ThreadBoard), CurrentThread.ThreadID);
                    break;
                default:
                    this.Text = string.Format("unknown thread - {0} - {1}", CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                    return;
            }
        }

        public void StartGone(ThreadStatus Status) {
            this.Icon = Properties.Resources.YChanEx404;
            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
            btnAbortRetry.Text = "Retry";
            switch (Status) {
                case ThreadStatus.Thread404:
                    lbScanTimer.Text = "404'd";
                    CurrentThread.Status = ThreadStatus.Thread404;
                    break;
                case ThreadStatus.ThreadAborted:
                    lbScanTimer.Text = "Aborted";
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                    break;
            }
            StartDownload(false);
        }
        #endregion


        #region 4chan Download Logic Completed.
        private void Set4chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://i.4cdn.org/" + CurrentThread.ThreadBoard + "/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    #region API/HTML Download Logic
                    if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                        CurrentThread.Status = ThreadStatus.Thread404;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = string.Format(ChanApiLinks.FourChan, CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        string RawJSON = ResponseReader.ReadToEnd();
                        byte[] JSONBytes = Encoding.UTF8.GetBytes(RawJSON);
                        using (var ByteMemory = new MemoryStream(JSONBytes)) {
                            var Quotas = new XmlDictionaryReaderQuotas();
                            var JSONReader = JsonReaderWriterFactory.CreateJsonReader(ByteMemory, Quotas);
                            var xml = XDocument.Load(JSONReader);
                            ByteMemory.Flush();
                            ByteMemory.Close();
                            ThreadJSON = xml.ToString();
                        }
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    CurrentURL = this.ThreadURL;
                    if (YChanEx.Downloads.Default.SaveHTML) {
                        ThreadHTML = Chans.GetHTML(CurrentURL);
                    }

                    if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Chans.EmptyXML) {
                        // Thread is dead?
                        return;
                    }
                    #endregion

                    #region API Parsing Logic
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlFileID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/tim");
                    XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");
                    XmlNodeList xmlExt = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");
                    XmlNodeList xmlHash = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/md5");

                    for (int FileIdIndex = ThreadPostsCount; FileIdIndex < xmlFileID.Count; FileIdIndex++, ThreadPostsCount++) {
                        if (xmlFileID[FileIdIndex] == null) {
                            continue;
                        }
                        string FileID = xmlFileID[FileIdIndex].InnerText;
                        string OriginalFileName = xmlFileName[FileIdIndex].InnerText;
                        string FileExtension = xmlExt[FileIdIndex].InnerText;
                        string ImageFile = FileBaseURL + xmlFileID[FileIdIndex].InnerText + xmlExt[FileIdIndex].InnerText;
                        string ThumbnailFile = FileBaseURL + FileID + "s.jpg";
                        string FileHash = xmlHash[FileIdIndex].InnerText;

                        FileIDs.Add(FileID);
                        FileExtensions.Add(FileExtension);
                        ThumbnailFiles.Add(ThumbnailFile);
                        ImageFiles.Add(ImageFile);
                        FileHashes.Add(xmlHash[FileIdIndex].InnerText);

                        string FileNameToReplace = FileID;
                        string FileName = FileID;
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                            FileName = OriginalFileName;
                            string FileNamePrefix = "";
                            string FileNameSuffix = "";

                            if (Downloads.Default.PreventDuplicates) {
                                if (OriginalFileNames.Contains(FileName)) {
                                    if (FileNamesDupes.Contains(FileName)) {
                                        int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                        FileNamesDupesCount[DupeNameIndex] += 1;
                                        FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                    }
                                    else {
                                        FileNamesDupes.Add(FileName);
                                        FileNamesDupesCount.Add(1);
                                        FileNameSuffix = " (dupe 1)";
                                    }
                                }
                            }

                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                            }

                            FileNameToReplace = FileNamePrefix + FileName + FileNameSuffix;
                            FileName = FileNamePrefix + FileName + FileNameSuffix;
                        }

                        OriginalFileNames.Add(OriginalFileName);
                        FileNames.Add(FileName + FileExtension);

                        if (YChanEx.Downloads.Default.SaveHTML) {
                            string OldHTMLLinks = null;
                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                OldHTMLLinks = "//i.4cdn.org/" + CurrentThread.ThreadBoard + "/" + FileID + "s.jpg";
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + FileID + "s.jpg");
                            }

                            OldHTMLLinks = "//i.4cdn.org/" + CurrentThread.ThreadBoard + "/" + FileID;
                            string OldHTMLLinks2 = "//is2.4chan.org/" + CurrentThread.ThreadBoard + "/" + FileID;
                            if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileNameToReplace);
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks2, FileNameToReplace);
                            }
                            else {
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileID);
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks2, FileID);
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

                        ThreadImagesCount++;
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ThreadImagesCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download Logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;

                        if (ImageFiles[ImageFilesIndex] == null) {
                            continue;
                        }
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));

                        string FileName = FileNames[ImageFilesIndex];
                        CurrentURL = ImageFiles[ImageFilesIndex];
                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Chans.DownloadFile(CurrentURL, DownloadPath, FileName)) {
                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[ImageFilesIndex] + "s.jpg");
                            }

                            DownloadedImagesCount++;

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
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

                using (System.Security.Cryptography.MD5 FileMD5 = System.Security.Cryptography.MD5.Create()) {
                    using (var FileStream = File.OpenRead(InputFile)) {
                        var FileHash = FileMD5.ComputeHash(FileStream);
                        System.Threading.Thread.Sleep(50);
                        OutputHash = BitConverter.ToString(FileHash).Replace("-", string.Empty).ToLowerInvariant();
                    }
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

        #region 420chan Download Logic Completed.
        private void Set420chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://boards.420chan.org/" + CurrentThread.ThreadBoard + "/src/";
                string ThumbnailBaseUrl = "https://boards.420chan.org/" + CurrentThread.ThreadBoard + "/thumb/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    #region API/HTML Download Logic
                    if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                        CurrentThread.Status = ThreadStatus.Thread404;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = string.Format(ChanApiLinks.FourTwentyChan, CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        string RawJSON = ResponseReader.ReadToEnd();
                        byte[] JSONBytes = Encoding.ASCII.GetBytes(RawJSON);
                        using (var ByteMemory = new MemoryStream(JSONBytes)) {
                            var Quotas = new XmlDictionaryReaderQuotas();
                            var JSONReader = JsonReaderWriterFactory.CreateJsonReader(ByteMemory, Quotas);
                            var xml = XDocument.Load(JSONReader);
                            ByteMemory.Flush();
                            ByteMemory.Close();
                            ThreadJSON = xml.ToString();
                        }
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    CurrentURL = this.ThreadURL;
                    if (YChanEx.Downloads.Default.SaveHTML) {
                        ThreadHTML = Chans.GetHTML(CurrentURL);
                        ThreadHTML.Replace("href=\"/" + CurrentThread.ThreadBoard + "/src/", "");
                        ThreadHTML.Replace("href=\"/" + CurrentThread.ThreadBoard, "");
                        ThreadHTML.Replace("href=\"/static/", "href=\"https://420chan.org/static/");
                    }

                    if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Chans.EmptyXML) {
                        return;
                    }
                    #endregion

                    #region API Parsing Logic
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlFileID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/no");
                    XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");
                    XmlNodeList xmlExt = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");

                    for (int FileNameIndex = ThreadImagesCount; FileNameIndex < xmlFileName.Count; FileNameIndex++, ThreadImagesCount++) {
                        if (xmlFileName[FileNameIndex] != null) {
                            FileIDs.Add(xmlFileID[FileNameIndex].InnerText);
                            FileExtensions.Add(xmlExt[FileNameIndex].InnerText);
                            ImageFiles.Add(FileBaseURL + xmlFileName[FileNameIndex].InnerText + xmlExt[FileNameIndex].InnerText);
                            ThumbnailFiles.Add(ThumbnailBaseUrl + xmlFileName[FileNameIndex].InnerText + "s.jpg");

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
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ThreadImagesCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download Logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;

                        if (ImageFiles[ImageFilesIndex] != null) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            }));
                            string FileName = FileIDs[ImageFilesIndex] + FileExtensions[ImageFilesIndex];
                            CurrentURL = ImageFiles[ImageFilesIndex];

                            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                            if (Chans.DownloadFile(CurrentURL, DownloadPath, FileName)) {
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                    Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[ImageFilesIndex] + "s.jpg");
                                }

                                this.BeginInvoke(new MethodInvoker(() => {
                                    lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "420chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region 7chan Download Logic Basically Completed, Needs: Original File Names
        private void Set7chanThread() {
            DownloadThread = new Thread(() => {
                string BaseURL = "https://7chan.org/";
                string ThreadHTML = null;
                string CurrentURL = null;
                try {

                    #region HTML Download Logic
                    int TryCount = 0;
retryThread:

                    CurrentURL = ThreadURL;
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        ThreadHTML = ResponseReader.ReadToEnd();
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    if (string.IsNullOrEmpty(ThreadHTML)) {
                        TryCount++;
                        if (TryCount == 5) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                            return;
                        }
                        Thread.Sleep(5000);
                        goto retryThread;
                    }

                    if (ThreadHTML == LastThreadHTML) {
                        AfterDownload();
                        return;
                    }

                    LastThreadHTML = ThreadHTML;
                    #endregion

                    #region HTML Parsing Logic
                    if (!UseOldLogic) {
                        #region New HTML Parsing Logic
                        MatchCollection PostMatches = new Regex(ChanRegex.SevenChanPosts).Matches(ThreadHTML);
                        for (int PostMatchesIndex = ThreadPostsCount; PostMatchesIndex < PostMatches.Count; PostMatchesIndex++, ThreadPostsCount++) {
                            string MatchValue = PostMatches[PostMatchesIndex].Value;
                            int IndexOfFileLink = MatchValue.IndexOf("alt=\"") + 5;
                            int IndexOfID = MatchValue.IndexOf("\"><img src=\"");
                            string PostID = MatchValue.Substring(IndexOfFileLink, MatchValue.Length - IndexOfFileLink - 15);
                            string FileLink = MatchValue.Substring(0, IndexOfID);

                            string FileExtension = "." + FileLink.Split('.')[2];
                            string FullFileName = FileLink.Split('/')[5];
                            string FileName = FullFileName.Substring(0, FullFileName.Length - FileExtension.Length);

                            ImageFiles.Add(FileLink);
                            FileExtensions.Add(FileExtension);

                            //if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                            //    FileName = OriginalFileName;
                            //    string FileNamePrefix = "";
                            //    string FileNameSuffix = "";

                            //    if (Downloads.Default.PreventDuplicates) {
                            //        if (OriginalFileNames.Contains(FileName)) {
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

                            //OriginalFileNames.Add(OriginalFileName);
                            FileNames.Add(FileName + FileExtension);

                            if (Downloads.Default.SaveThumbnails) {
                                ThumbnailFiles.Add(BaseURL + CurrentThread.ThreadBoard + "/thumb/" + FileName + "s" + FileExtension);
                                ThumbnailNames.Add(FileName + "s" + FileExtension);
                            }

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

                        if (Downloads.Default.SaveHTML) {
                            ThreadHTML = ThreadHTML.Replace("https://7chan.org/" + CurrentThread.ThreadBoard + "/src/", "");
                            ThreadHTML = ThreadHTML.Replace("https://7chan.org/" + CurrentThread.ThreadBoard + "/thumb/", "thumb/");
                        }
                        #endregion
                    }
                    else {
                        #region Old HTML Parsing Logic
                        Regex ImageMatch = new Regex("http(?:s)?:\\/\\/(?:www\\.)?7chan.org\\/([a-zA-Z0-9]+)\\/src\\/([0-9]+)\\.(?:jpg|jpeg|gif|png|webm|mp4)?");

                        List<string> FilesBuffer = new List<string>();
                        foreach (Match ImageLink in ImageMatch.Matches(ThreadHTML)) {
                            FilesBuffer.Add(ImageLink.ToString());
                        }

                        if (FilesBuffer.Count > ImageFiles.Count) {
                            for (int FilesBufferIndex = ImageFiles.Count; FilesBufferIndex < FilesBuffer.Count; FilesBufferIndex++) {
                                string FileName = FilesBuffer[FilesBufferIndex].Split('/')[FilesBuffer[FilesBufferIndex].Split('/').Length - 1].Split('.')[0];
                                string Extension = FilesBuffer[FilesBufferIndex].Split('.')[FilesBuffer[FilesBufferIndex].Split('.').Length - 1];
                                if (!FileNames.Contains(FileName)) {
                                    ImageFiles.Add(FilesBuffer[FilesBufferIndex]);
                                    FileNames.Add(FileName);
                                    FileExtensions.Add(Extension);

                                    if (Downloads.Default.SaveThumbnails) {
                                        ThumbnailFiles.Add("https://7chan.org/" + CurrentThread.ThreadBoard + "/thumb/" + FileName + "s.jpg");
                                    }

                                    if (Downloads.Default.SaveHTML) {
                                        //HTMLBuffer = HTMLBuffer.Replace(FilesBuffer[i], FileName + "." + Extension);
                                        ThreadHTML = ThreadHTML.Replace("https://7chan.org/" + CurrentThread.ThreadBoard, "");
                                    }

                                    ListViewItem lvi = new ListViewItem();
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.Name = FilesBuffer[FilesBufferIndex];
                                    lvi.SubItems[0].Text = FileName;
                                    lvi.SubItems[1].Text = Extension;
                                    lvi.SubItems[2].Text = FileName;
                                    lvi.ImageIndex = 0;
                                    this.BeginInvoke(new MethodInvoker(() => {
                                        lvImages.Items.Add(lvi);
                                    }));
                                }
                            }
                        }
                        #endregion
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ImageFiles.Count.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download Logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));
                        CurrentURL = ImageFiles[ImageFilesIndex];

                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Chans.DownloadFile(ImageFiles[ImageFilesIndex], DownloadPath, FileNames[ImageFilesIndex])) {
                            if (Downloads.Default.SaveThumbnails) {
                                Chans.DownloadFile(ThumbnailFiles[ImageFilesIndex], DownloadPath + "\\thumb\\", ThumbnailNames[ImageFilesIndex]);
                            }


                            DownloadedImagesCount++;
                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "7chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region 8chan Download Logic Completed.
        private void Set8chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://8chan.moe";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    #region Download JSON Logic
                    if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                        CurrentThread.Status = ThreadStatus.Thread404;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = string.Format(ChanApiLinks.EightChan, CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        string RawJSON = ResponseReader.ReadToEnd();
                        byte[] JSONBytes = Encoding.ASCII.GetBytes(RawJSON);
                        using (var ByteMemory = new MemoryStream(JSONBytes)) {
                            var Quotas = new XmlDictionaryReaderQuotas();
                            var JSONReader = JsonReaderWriterFactory.CreateJsonReader(ByteMemory, Quotas);
                            var xml = XDocument.Load(JSONReader);
                            ByteMemory.Flush();
                            ByteMemory.Close();
                            ThreadJSON = xml.ToString();
                        }
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    CurrentURL = this.ThreadURL;
                    if (YChanEx.Downloads.Default.SaveHTML) {
                        ThreadHTML = Chans.GetHTML(CurrentURL);
                    }

                    if (General.Default.UseFullBoardNameForTitle && !RetrievedBoardName) {
                        if (ThreadHTML == null) {
                            ThreadHTML = Chans.GetHTML(CurrentURL);
                        }

                        int TitleExtraLength = 5 + CurrentThread.ThreadBoard.Length;
                        BoardName = ThreadHTML.Substring(
                            ThreadHTML.IndexOf("<title>") + (7 + TitleExtraLength),
                            ThreadHTML.IndexOf("</title>") - ThreadHTML.IndexOf("<title>") - (7 + TitleExtraLength)
                        );

                        this.BeginInvoke(new MethodInvoker(() => {
                            this.Text = string.Format("8chan thread - {0} - {1}", BoardName, CurrentThread.ThreadID);
                        }));

                        RetrievedBoardName = true;

                        Thread.Sleep(100);
                    }

                    if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Chans.EmptyXML) {
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

                    for (int PostIndex = ThreadImagesCount; PostIndex < xmlFilePath.Count; PostIndex++) {
                        if (xmlFilePath[PostIndex] == null) {
                            continue;
                        }
                        ThreadImagesCount++;
                        string FileUrl = xmlFilePath[PostIndex].InnerText;
                        string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                        string FileID = CurrentThread.ThreadID + "-" + (PostIndex + 1).ToString();
                        string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[FileUrl.Split('/')[2].Split('.').Length - 1];
                        string OriginalFileName = xmlFileName[PostIndex].InnerText;
                        FileExtensions.Add(FileExtension);
                        FileIDs.Add(FileID);
                        FileHashes.Add(FileHash);
                        ImageFiles.Add(FileBaseURL + FileUrl);
                        ThumbnailFiles.Add(FileBaseURL + xmlFileThumbnail[PostIndex].InnerText);
                        ThumbnailNames.Add(xmlFileThumbnail[PostIndex].InnerText.Substring(8));

                        string FileName = FileUrl.Substring(8, FileUrl.Length - 12);
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                            string FileNamePrefix = "";
                            string FileNameSuffix = "";
                            FileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);
                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                            }
                            if (Downloads.Default.PreventDuplicates) {
                                if (OriginalFileNames.Contains(FileName)) {
                                    if (FileNamesDupes.Contains(FileName)) {
                                        int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                        FileNamesDupesCount[DupeNameIndex] += 1;
                                        FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                    }
                                    else {
                                        FileNamesDupes.Add(FileName);
                                        FileNamesDupesCount.Add(1);
                                        FileNameSuffix = " (dupe 1)";
                                    }
                                }
                            }

                            FileName = FileNamePrefix + FileName + FileNameSuffix;
                        }

                        OriginalFileNames.Add(OriginalFileName);
                        FileNames.Add(FileName);

                        if (YChanEx.Downloads.Default.SaveHTML) {
                            string OldHTMLLinks = null;

                            OldHTMLLinks = "src=\"/.media/t_" + FileHash;
                            ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "src=\"thumb/t_" + FileHash + ".jpg");
                            OldHTMLLinks = "href=\"/.media/" + FileHash;
                            ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "href=\"" + FileName);
                        }

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
                    #endregion

                    #region Subsequent posts file(s)
                    if (!UseOldLogic) {
                        #region New logic
                        XmlNodeList xmlPosts = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                        for (int PostsIndex = ThreadPostsCount; PostsIndex < xmlPosts.Count; PostsIndex++) {
                            XmlNodeList xmlPostID = xmlPosts[PostsIndex].SelectNodes("postId");
                            xmlFilePath = xmlPosts[PostsIndex].SelectNodes("files/item/path");
                            xmlFileThumbnail = xmlPosts[PostsIndex].SelectNodes("files/item/thumb");
                            xmlFileName = xmlPosts[PostsIndex].SelectNodes("files/item/originalName");

                            for (int FilePathIndex = 0; FilePathIndex < xmlFilePath.Count; FilePathIndex++) {
                                if (xmlFilePath[FilePathIndex] == null) {
                                    continue;
                                }
                                ThreadImagesCount++;
                                string FileUrl = xmlFilePath[FilePathIndex].InnerText;
                                string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                                string FileID = xmlPostID[0].InnerText + "-" + (FilePathIndex + 1).ToString();
                                string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[FileUrl.Split('/')[2].Split('.').Length - 1];
                                string OriginalFileName = xmlFileName[FilePathIndex].InnerText;
                                FileExtensions.Add(FileExtension);
                                FileIDs.Add(FileID);
                                FileHashes.Add(FileHash);
                                ImageFiles.Add(FileBaseURL + FileUrl);
                                ThumbnailFiles.Add(FileBaseURL + xmlFileThumbnail[FilePathIndex].InnerText);
                                ThumbnailNames.Add(xmlFileThumbnail[FilePathIndex].InnerText.Substring(8));

                                string FileName = FileUrl.Substring(8, FileUrl.Length - 12);
                                if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";
                                    FileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);
                                    for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                        FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                    }
                                    if (Downloads.Default.PreventDuplicates) {
                                        if (OriginalFileNames.Contains(FileName)) {
                                            if (FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                                FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                            }
                                            else {
                                                FileNamesDupes.Add(FileName);
                                                FileNamesDupesCount.Add(1);
                                                FileNameSuffix = " (dupe 1)";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                }

                                OriginalFileNames.Add(OriginalFileName);
                                FileNames.Add(FileName);

                                if (YChanEx.Downloads.Default.SaveHTML) {
                                    string OldHTMLLinks = null;

                                    OldHTMLLinks = "src=\"/.media/t_" + FileHash;
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "src=\"thumb/t_" + FileHash + ".jpg");
                                    OldHTMLLinks = "href=\"/.media/" + FileHash;
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "href=\"" + FileName);
                                }

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
                        #endregion
                    }
                    else {
                        #region Old logic
                        if (!UseConfirmedWorkingLogic) {
                            #region Subsequent Old Logic
                            xmlFilePath = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/files/item/path");
                            xmlFileThumbnail = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/files/item/thumb");
                            xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/files/item/originalName");

                            for (int PostIndex = 0; PostIndex < xmlFilePath.Count; PostIndex++) {
                                if (xmlFilePath[PostIndex] == null) {
                                    continue;
                                }
                                ThreadImagesCount++;
                                string FileUrl = xmlFilePath[PostIndex].InnerText;
                                string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                                string FileID = (ThreadImagesCount + 1).ToString();
                                string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[1];
                                FileExtensions.Add(FileExtension);
                                FileIDs.Add(FileID);
                                FileHashes.Add(FileHash);
                                ImageFiles.Add(FileBaseURL + FileUrl);
                                ThumbnailFiles.Add(FileBaseURL + xmlFileThumbnail[PostIndex].InnerText);

                                string FileName = string.Empty;
                                if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    FileName = xmlFileName[PostIndex].InnerText.Substring(0, xmlFileName[PostIndex].InnerText.Length - 4);
                                    for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                        FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                    }
                                    if (YChanEx.Downloads.Default.PreventDuplicates) {
                                        if (PostIndex >= 10) {
                                            if (PostIndex >= 100) {
                                                FileNamePrefix = "(" + ThreadImagesCount.ToString() + ") ";
                                            }
                                            else {
                                                FileNamePrefix = "(0" + ThreadImagesCount.ToString() + ") ";
                                            }
                                        }
                                        else {
                                            FileNamePrefix = "(00" + ThreadImagesCount.ToString() + ") ";
                                        }
                                        FileName = FileNamePrefix + FileName;
                                    }
                                }
                                else {
                                    FileName = FileUrl.Substring(8, FileUrl.Length - 12);
                                }
                                FileNames.Add(FileName);

                                if (YChanEx.Downloads.Default.SaveHTML) {
                                    string OldHTMLLinks = null;

                                    OldHTMLLinks = "src=\"/.media/t_";
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "src=\"/thumb/t_");
                                    OldHTMLLinks = "href=\"/.media";
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                                }

                                ListViewItem lvi = new ListViewItem();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = xmlFilePath[PostIndex].InnerText;
                                lvi.SubItems[0].Text = FileID;
                                lvi.SubItems[1].Text = FileExtension.Trim('.');
                                lvi.SubItems[2].Text = xmlFileName[PostIndex].InnerText.Substring(0, xmlFileName[PostIndex].InnerText.Length - 4);
                                lvi.SubItems[3].Text = FileHash;
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items.Add(lvi);
                                }));
                            }
                            #endregion
                        }
                        else {
                            XmlNodeList xmlPost = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                            XmlNodeList xmlPostId = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/postId");
                            for (int PostIndex = 0; PostIndex < xmlPost.Count; PostIndex++) {
                                for (int FileIndex = 0; FileIndex < xmlPost[PostIndex].ChildNodes[9].ChildNodes.Count; FileIndex++) {
                                    string xFileName = xmlPost[PostIndex].ChildNodes[9].ChildNodes[FileIndex].ChildNodes[0].InnerText;
                                    string xFilePath = xmlPost[PostIndex].ChildNodes[9].ChildNodes[FileIndex].ChildNodes[1].InnerText;
                                    string xFileThumbnail = xmlPost[PostIndex].ChildNodes[9].ChildNodes[FileIndex].ChildNodes[2].InnerText;
                                    if (xFilePath == null) {
                                        continue;
                                    }
                                    ThreadImagesCount++;
                                    string FileUrl = xFilePath;
                                    string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                                    string FileID = xmlPostId[PostIndex].InnerText + "-" + (FileIndex + 1).ToString();
                                    string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[1];
                                    FileExtensions.Add(FileExtension);
                                    FileIDs.Add(FileID);
                                    FileHashes.Add(FileHash);
                                    ImageFiles.Add(FileBaseURL + FileUrl);
                                    ThumbnailFiles.Add(FileBaseURL + xFileThumbnail);
                                    ThumbnailNames.Add(xFileThumbnail.Substring(8));

                                    string FileName = string.Empty;
                                    if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                        string FileNamePrefix = "";
                                        FileName = xFileName.Substring(0, xFileName.Length - 4);
                                        for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                            FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                        }
                                        if (YChanEx.Downloads.Default.PreventDuplicates) {
                                            if (FileIndex >= 10) {
                                                if (FileIndex >= 100) {
                                                    FileNamePrefix = "(" + ThreadImagesCount.ToString() + ") ";
                                                }
                                                else {
                                                    FileNamePrefix = "(0" + ThreadImagesCount.ToString() + ") ";
                                                }
                                            }
                                            else {
                                                FileNamePrefix = "(00" + ThreadImagesCount.ToString() + ") ";
                                            }
                                            FileName = FileNamePrefix + FileName;
                                        }
                                    }
                                    else {
                                        FileName = FileUrl.Substring(8, FileUrl.Length - 12);
                                    }
                                    FileNames.Add(FileName);

                                    if (YChanEx.Downloads.Default.SaveHTML) {
                                        string OldHTMLLinks = null;

                                        OldHTMLLinks = "src=\"/.media/t_" + FileHash;
                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "src=\"thumb/t_" + FileHash + ".jpg");
                                        OldHTMLLinks = "href=\"/.media/" + FileHash;
                                        ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "href=\"" + FileName);
                                    }

                                    ListViewItem lvi = new ListViewItem();
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                    lvi.Name = xFilePath;
                                    lvi.SubItems[0].Text = FileID;
                                    lvi.SubItems[1].Text = FileExtension.Trim('.');
                                    lvi.SubItems[2].Text = xFileName.Substring(0, xFileName.Length - 4);
                                    lvi.SubItems[3].Text = FileHash;
                                    this.BeginInvoke(new MethodInvoker(() => {
                                        lvImages.Items.Add(lvi);
                                    }));
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ThreadImagesCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;
                        if (ImageFiles[ImageFilesIndex] != null) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            }));
                            string FileName = FileNames[ImageFilesIndex];
                            CurrentURL = ImageFiles[ImageFilesIndex];

                            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                            if (Chans.DownloadFile(CurrentURL, DownloadPath, FileNames[ImageFilesIndex] + FileExtensions[ImageFilesIndex])) {
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                    Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", ThumbnailNames[ImageFilesIndex] + ".jpg");
                                }

                                this.BeginInvoke(new MethodInvoker(() => {
                                    lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "8chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion

        #region 8kun Download Logic Completed.
        private void Set8kunThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://media.8kun.top/file_store/";
                string ThumbnailFileBaseURL = "https://media.8kun.top/file_store/thumb/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    if (CurrentThread.ThreadBoard == null || CurrentThread.ThreadID == null) {
                        CurrentThread.Status = ThreadStatus.Thread404;
                        AfterDownload();
                        return;
                    }

                    #region Download API
                    CurrentURL = string.Format(ChanApiLinks.EightKun, CurrentThread.ThreadBoard, CurrentThread.ThreadID);
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        string RawJSON = ResponseReader.ReadToEnd();
                        byte[] JSONBytes = Encoding.ASCII.GetBytes(RawJSON);
                        using (var ByteMemory = new MemoryStream(JSONBytes)) {
                            var Quotas = new XmlDictionaryReaderQuotas();
                            var JSONReader = JsonReaderWriterFactory.CreateJsonReader(ByteMemory, Quotas);
                            var xml = XDocument.Load(JSONReader);
                            ByteMemory.Flush();
                            ByteMemory.Close();
                            ThreadJSON = xml.ToString();
                        }
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    CurrentURL = this.ThreadURL;
                    if (YChanEx.Downloads.Default.SaveHTML) {
                        ThreadHTML = Chans.GetHTML(CurrentURL);
                    }

                    if (General.Default.UseFullBoardNameForTitle && !RetrievedBoardName) {
                        if (ThreadHTML == null) {
                            ThreadHTML = Chans.GetHTML(CurrentURL);
                        }

                        int TitleExtraLength = 5 + CurrentThread.ThreadBoard.Length;
                        BoardName = ThreadHTML.Substring(
                            ThreadHTML.IndexOf("<h1>") + (4 + TitleExtraLength),
                            ThreadHTML.IndexOf("</h1>") - ThreadHTML.IndexOf("<h1>") - (4 + TitleExtraLength)
                        );

                        this.BeginInvoke(new MethodInvoker(() => {
                            this.Text = string.Format("8kun thread - {0} - {1}", BoardName, CurrentThread.ThreadID);
                        }));

                        RetrievedBoardName = true;

                        Thread.Sleep(100);
                    }

                    if (string.IsNullOrEmpty(ThreadJSON) || ThreadJSON == Chans.EmptyXML) {
                        // Thread is dead?
                        return;
                    }
                    #endregion

                    #region API Parsing logic
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);

                    if (!UseOldLogic) {
                        #region New 8kun
                        XmlNodeList xmlPosts = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                        for (int ThreadPostIndex = ThreadPostsCount; ThreadPostIndex < xmlPosts.Count; ThreadPostIndex++) {
                            if (xmlPosts[ThreadPostIndex] != null) {
                                XmlNodeList xmlPostID = xmlPosts[ThreadPostIndex].SelectNodes("no");
                                string xPostID = xmlPostID[0].InnerText;

                                #region FirstFile
                                XmlNodeList xmlFileID = xmlPosts[ThreadPostIndex].SelectNodes("tim");
                                XmlNodeList xmlFileName = xmlPosts[ThreadPostIndex].SelectNodes("filename");
                                XmlNodeList xmlExtension = xmlPosts[ThreadPostIndex].SelectNodes("ext");
                                XmlNodeList xmlMd5 = xmlPosts[ThreadPostIndex].SelectNodes("md5");
                                if (xmlFileID.Count > 0) {
                                    for (int FileIdIndex = 0; FileIdIndex < xmlFileID.Count; FileIdIndex++) {
                                        if (xmlFileID[FileIdIndex] == null) {
                                            continue;
                                        }
                                        string xFileID = xmlFileID[FileIdIndex].InnerText;
                                        string xExtension = xmlExtension[FileIdIndex].InnerText;
                                        string xMD5 = xmlMd5[FileIdIndex].InnerText;
                                        string xFileName = xmlFileName[FileIdIndex].InnerText;

                                        FileIDs.Add(xFileID);
                                        ImageFiles.Add(FileBaseURL + xFileID + xExtension);
                                        ThumbnailFiles.Add(ThumbnailFileBaseURL + xFileID + xExtension);
                                        ThumbnailNames.Add(xFileID + xExtension);
                                        FileExtensions.Add(xExtension);
                                        FileHashes.Add(xMD5);

                                        string FileName = xFileID;
                                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                            FileName = xFileName;
                                            string FileNamePrefix = string.Empty;
                                            string FileNameSuffix = string.Empty;

                                            if (Downloads.Default.PreventDuplicates) {
                                                if (OriginalFileNames.Contains(FileName)) {
                                                    if (FileNamesDupes.Contains(FileName)) {
                                                        int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                                        FileNamesDupesCount[DupeNameIndex] += 1;
                                                        FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                                    }
                                                    else {
                                                        FileNamesDupes.Add(FileName);
                                                        FileNamesDupesCount.Add(1);
                                                        FileNameSuffix = " (dupe 1)";
                                                    }
                                                }
                                            }

                                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                                FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                            }

                                            FileName = FileNamePrefix + FileName + FileNameSuffix;
                                        }

                                        OriginalFileNames.Add(xFileName);
                                        FileNames.Add(FileName + xExtension);

                                        if (YChanEx.Downloads.Default.SaveHTML) {
                                            string OldHTMLLinks = null;
                                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                                OldHTMLLinks = ThumbnailFileBaseURL;
                                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                            }
                                            OldHTMLLinks = FileBaseURL + xFileID;
                                            ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
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

                                        ThreadImagesCount++;
                                        ThreadPostsCount++;
                                    }
                                }
                                #endregion

                                #region Extra Files
                                xmlFileID = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/tim");
                                xmlFileName = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/filename");
                                xmlExtension = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/ext");
                                xmlMd5 = xmlPosts[ThreadPostIndex].SelectNodes("extra_files/item/md5");
                                if (xmlFileID.Count > 0) {
                                    for (int FileIdIndex = 0; FileIdIndex < xmlFileID.Count; FileIdIndex++) {
                                        if (xmlFileID[FileIdIndex] == null) {
                                            continue;
                                        }
                                        string xFileID = xmlFileID[FileIdIndex].InnerText;
                                        string xExtension = xmlExtension[FileIdIndex].InnerText;
                                        string xMD5 = xmlMd5[FileIdIndex].InnerText;
                                        string xFileName = xmlFileName[FileIdIndex].InnerText;

                                        FileIDs.Add(xFileID);
                                        ImageFiles.Add(FileBaseURL + xFileID + xExtension);
                                        ThumbnailFiles.Add(ThumbnailFileBaseURL + xFileID + xExtension);
                                        ThumbnailNames.Add(xFileID + xExtension);
                                        FileExtensions.Add(xExtension);
                                        FileHashes.Add(xMD5);

                                        string FileName = xFileID;
                                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                            FileName = xFileName;
                                            string FileNamePrefix = string.Empty;
                                            string FileNameSuffix = string.Empty;

                                            if (Downloads.Default.PreventDuplicates) {
                                                if (OriginalFileNames.Contains(FileName)) {
                                                    if (FileNamesDupes.Contains(FileName)) {
                                                        int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                                        FileNamesDupesCount[DupeNameIndex] += 1;
                                                        FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                                    }
                                                    else {
                                                        FileNamesDupes.Add(FileName);
                                                        FileNamesDupesCount.Add(1);
                                                        FileNameSuffix = " (dupe 1)";
                                                    }
                                                }
                                            }

                                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                                FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                            }

                                            FileName = FileNamePrefix + FileName;
                                        }

                                        OriginalFileNames.Add(xFileName);
                                        FileNames.Add(FileName + xExtension);

                                        if (YChanEx.Downloads.Default.SaveHTML) {
                                            string OldHTMLLinks = null;
                                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                                OldHTMLLinks = ThumbnailFileBaseURL;
                                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                            }
                                            OldHTMLLinks = FileBaseURL + xFileID;
                                            ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
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

                                        ThreadImagesCount++;
                                        ThreadPostsCount++;
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    else {
                        #region Legacy 8kun
                        #region Scan first files in posts
                        // tim == fileid
                        XmlNodeList xmlPostID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/no");
                        XmlNodeList xmlFileID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/tim");
                        XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");
                        XmlNodeList xmlExtension = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");
                        XmlNodeList xmlMd5 = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/md5");

                        // Scan for first images in posts
                        for (int FileIdIndex = 0; FileIdIndex < xmlFileID.Count; FileIdIndex++) {
                            if (xmlFileID[FileIdIndex] == null) {
                                continue;
                            }
                            ThreadImagesCount++;
                            string xPostID = xmlPostID[FileIdIndex].InnerText;
                            string xFileID = xmlFileID[FileIdIndex].InnerText;
                            string xExtension = xmlExtension[FileIdIndex].InnerText;
                            string xMD5 = xmlMd5[FileIdIndex].InnerText;
                            string xFileName = xmlFileName[FileIdIndex].InnerText;

                            FileIDs.Add(xFileID);
                            ImageFiles.Add(FileBaseURL + xFileID + xExtension);
                            ThumbnailFiles.Add(ThumbnailFileBaseURL + xFileID + xExtension);
                            ThumbnailNames.Add(xFileID + xExtension);
                            FileExtensions.Add(xExtension);
                            FileHashes.Add(xMD5);

                            string FileName = xFileID;
                            string FileNamePrefix = string.Empty;
                            if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                FileName = xFileName;
                                for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                    FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                }

                                if (YChanEx.Downloads.Default.PreventDuplicates) {
                                    if (FileIdIndex >= 10) {
                                        if (FileIdIndex >= 100) {
                                            FileNamePrefix = "(" + FileIdIndex.ToString() + ") ";
                                        }
                                        else {
                                            FileNamePrefix = "(0" + FileIdIndex.ToString() + ") ";
                                        }
                                    }
                                    else {
                                        FileNamePrefix = "(00" + FileIdIndex.ToString() + ") ";
                                    }
                                }

                                FileName = FileNamePrefix + FileName;
                            }

                            FileNames.Add(FileName + xExtension);

                            if (YChanEx.Downloads.Default.SaveHTML) {
                                string OldHTMLLinks = null;
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    OldHTMLLinks = ThumbnailFileBaseURL;
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                }
                                OldHTMLLinks = FileBaseURL + xFileID;
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                            }

                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = xFileID;
                            lvi.SubItems[0].Text = xFileID;
                            lvi.SubItems[1].Text = xExtension.Trim('.');
                            lvi.SubItems[2].Text = xFileName;
                            lvi.SubItems[3].Text = xMD5;
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));
                        }
                        #endregion

                        #region Scan extra files
                        xmlFileID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/tim");
                        xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/filename");
                        xmlExtension = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/ext");
                        xmlMd5 = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/md5");
                        for (int FileIdIndex = ExtraFilesImageCount; FileIdIndex < xmlFileID.Count; FileIdIndex++) {
                            if (xmlFileID[FileIdIndex] == null) {
                                continue;
                            }
                            ExtraFilesImageCount++;
                            string xTim = xmlFileID[FileIdIndex].InnerText;
                            string xExtension = xmlExtension[FileIdIndex].InnerText;
                            string xMD5 = xmlMd5[FileIdIndex].InnerText;
                            string xFileName = xmlFileName[FileIdIndex].InnerText;

                            FileIDs.Add(xTim);
                            ImageFiles.Add(FileBaseURL + xTim + xExtension);
                            ThumbnailFiles.Add(ThumbnailFileBaseURL + xTim + xExtension);
                            ThumbnailNames.Add(xTim + xExtension);
                            FileExtensions.Add(xExtension);
                            FileHashes.Add(xMD5);

                            string FileName = xTim;
                            string FileNamePrefix = string.Empty;
                            if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                FileName = xFileName;
                                for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                    FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                                }

                                if (YChanEx.Downloads.Default.PreventDuplicates) {
                                    if (FileIdIndex >= 10) {
                                        if (FileIdIndex >= 100) {
                                            FileNamePrefix = "(" + FileIdIndex.ToString() + ") ";
                                        }
                                        else {
                                            FileNamePrefix = "(0" + FileIdIndex.ToString() + ") ";
                                        }
                                    }
                                    else {
                                        FileNamePrefix = "(00" + FileIdIndex.ToString() + ") ";
                                    }
                                }

                                FileName = FileNamePrefix + FileName;
                            }

                            FileNames.Add(FileName + xExtension);

                            if (YChanEx.Downloads.Default.SaveHTML) {
                                string OldHTMLLinks = null;
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    OldHTMLLinks = ThumbnailFileBaseURL;
                                    ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb/");
                                }
                                OldHTMLLinks = FileBaseURL + xTim;
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileName);
                            }

                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = xTim;
                            lvi.SubItems[0].Text = xTim;
                            lvi.SubItems[1].Text = xExtension.Trim('.');
                            lvi.SubItems[2].Text = xFileName;
                            lvi.SubItems[3].Text = xMD5;
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));
                        }
                        #endregion
                        #endregion
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = (ThreadImagesCount + ExtraFilesImageCount).ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;
                        if (ImageFiles[ImageFilesIndex] != null) {
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                            }));
                            string FileName = FileNames[ImageFilesIndex];
                            CurrentURL = ImageFiles[ImageFilesIndex];

                            if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                            if (Chans.DownloadFile(CurrentURL, DownloadPath, FileName)) {
                                if (YChanEx.Downloads.Default.SaveThumbnails) {
                                    CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                    if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                    Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[ImageFilesIndex] + FileExtensions[ImageFilesIndex]);
                                }

                                this.BeginInvoke(new MethodInvoker(() => {
                                    lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    if (Response.StatusCode == HttpStatusCode.BadGateway) {
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
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

                    #region Download HTML Logic
                    int TryCount = 0;
retryThread:
                    CurrentURL = ThreadURL;
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.CreateHttp(ThreadURL);
                    Request.CookieContainer = new CookieContainer();
                    Request.CookieContainer.Add(new Cookie("disclaimer", "seen") { Domain = "fchan.us" });
                    Request.IfModifiedSince = LastModified;
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.Method = "GET";
                    Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    var Response = (HttpWebResponse)Request.GetResponse();
                    Response.Cookies.Add(new Cookie("disclaimer", "seen"));
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        ThreadHTML = ResponseReader.ReadToEnd();
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    if (string.IsNullOrEmpty(ThreadHTML)) {
                        TryCount++;
                        if (TryCount == 5) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                            return;
                        }
                        Thread.Sleep(5000);
                        goto retryThread;
                    }

                    if (ThreadHTML == LastThreadHTML) {
                        AfterDownload();
                        return;
                    }

                    LastThreadHTML = ThreadHTML;
                    #endregion

                    #region HTML Parsing logic
                    MatchCollection NameMatches = new Regex(ChanRegex.fchanNames).Matches(ThreadHTML);
                    MatchCollection PostIDMatches = new Regex(ChanRegex.DefaultRegex.fchanIDs).Matches(ThreadHTML);
                    for (int PostMatchesIndex = ThreadPostsCount; PostMatchesIndex < NameMatches.Count; PostMatchesIndex++) {
                        string NameMatch = NameMatches[PostMatchesIndex].Value;
                        string FileMatch = NameMatch.Substring(0, NameMatch.IndexOf("\" rel=\""));
                        string IDMatch = PostIDMatches[PostMatchesIndex].Value;
                        int IndexOfFullFileName = NameMatch.IndexOf('>') + 1;

                        string FullFileName = FileMatch.Substring(5);                       // file name saved on fchan
                        string FileExtension = "." + FullFileName.Split('.')[FullFileName.Split('.').Length - 1];   // file extension
                        string FileName = FullFileName.Substring(0, FullFileName.Length - FileExtension.Length);    // file name w/o ext
                        //string OriginalFileName = NameMatch.Substring(IndexOfFullFileName);                   // original file name
                        //OriginalFileName = OriginalFileName.Substring(0, OriginalFileName.Length - FileExtension.Length);
                        string PostID = IDMatch.Substring(0, IDMatch.Length - 7).Substring(12);

                        FileIDs.Add(PostID);
                        //OriginalFileNames.Add(OriginalFileName);
                        FileExtensions.Add(FileExtension);
                        ImageFiles.Add(BaseURL + "/src/" + FullFileName.Trim('/'));

                        // I hate fchan, holy god I hate it so.
                        // Why can't they have regular locations for original file names
                        // killing myself.

                        //if (Downloads.Default.SaveOriginalFilenames) {
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

                        FileNames.Add(FileName + FileExtension);

                        if (Downloads.Default.SaveThumbnails) {
                            // trim the board name length + 14 for the image generated information before the 
                            string ThumbnailName = FullFileName.Substring(0, CurrentThread.ThreadBoard.Length + 14) + "s";
                            ThumbnailName += FullFileName.Substring(CurrentThread.ThreadBoard.Length + 14, FullFileName.Length - (CurrentThread.ThreadBoard.Length + 14));
                            string ThumbnailLink = BaseURL + CurrentThread.ThreadBoard + "/thumb/" + ThumbnailName.Substring(0, ThumbnailName.Length - FileExtension.Length).Trim('/');
                            ThumbnailNames.Add(ThumbnailName);
                            ThumbnailFiles.Add(ThumbnailLink + ".jpg");

                            if (Downloads.Default.SaveHTML) {
                                ThreadHTML = ThreadHTML.Replace("src=\"/" + CurrentThread.ThreadBoard + "/thumb/" + ThumbnailName, "src=\"thumb/" + ThumbnailName);
                            }
                        }

                        if (Downloads.Default.SaveHTML) {
                            ThreadHTML = ThreadHTML.Replace("/src/" + CurrentThread.ThreadBoard + "/" + FullFileName, FileName);
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
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ImageFiles.Count.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));

                        CurrentURL = ImageFiles[ImageFilesIndex];
                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Chans.DownloadFile(ImageFiles[ImageFilesIndex], DownloadPath, FileNames[ImageFilesIndex], true, "disclaimer=seen")) {
                            if (Downloads.Default.SaveThumbnails) {
                                CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Chans.DownloadFile(ThumbnailFiles[ImageFilesIndex], DownloadPath + "\\thumb\\", ThumbnailNames[ImageFilesIndex], true, "disclaimer=seen");
                            }

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
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

                    #region HTML Download Logic
                    int TryCount = 0;
retryThread:

                    CurrentURL = ThreadURL;
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        ThreadHTML = ResponseReader.ReadToEnd();
                    }
                    LastModified = Response.LastModified;
                    Response.Dispose();
                    ResponseStream.Dispose();

                    if (string.IsNullOrEmpty(ThreadHTML)) {
                        TryCount++;
                        if (TryCount == 5) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                            return;
                        }
                        Thread.Sleep(5000);
                        goto retryThread;
                    }

                    if (ThreadHTML == LastThreadHTML) {
                        AfterDownload();
                        return;
                    }

                    LastThreadHTML = ThreadHTML;
                    #endregion

                    #region HTML Parsing logic
                    if (!UseOldLogic) {
                        #region New Parse Logic
                        MatchCollection PostMatches = new Regex(ChanRegex.u18chanPosts).Matches(ThreadHTML);
                        for (int PostMatchesIndex = ThreadPostsCount; PostMatchesIndex < PostMatches.Count; PostMatchesIndex++, ThreadPostsCount++) {
                            if (PostMatches[PostMatchesIndex] != null) {
                                string MatchValue = PostMatches[PostMatchesIndex].Value;
                                int IndexOfTag = MatchValue.IndexOf('<');
                                string PostID = MatchValue.Substring(IndexOfTag + 14).Substring(0, 8).Trim('_');
                                IndexOfTag = MatchValue.IndexOf('>');
                                string FileLink = MatchValue.Substring(0, IndexOfTag - 1);

                                string FileName = FileLink.Split('/')[FileLink.Split('/').Length - 1];
                                string FileExtension = "." + FileName.Split('.')[FileName.Split('.').Length - 1];
                                FileName = FileName.Substring(0, FileName.Length - FileExtension.Length);

                                OriginalFileNames.Add(FileName);
                                FileExtensions.Add(FileExtension);
                                ImageFiles.Add(FileLink);

                                if (Downloads.Default.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";

                                    do {
                                        FileName = FileName.Substring(0, FileName.Length - 8);
                                    } while (FileName.EndsWith("_u18chan"));

                                    for (int IllegalCharacterIndex = 0; IllegalCharacterIndex < Chans.InvalidFileCharacters.Length; IllegalCharacterIndex++) {
                                        FileName = FileName.Replace(Chans.InvalidFileCharacters[IllegalCharacterIndex], "_");
                                    }

                                    if (Downloads.Default.PreventDuplicates) {
                                        if (FileNames.Contains(FileName)) {
                                            if (FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                                FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                            }
                                            else {
                                                FileNamesDupes.Add(FileName);
                                                FileNamesDupesCount.Add(1);
                                                FileNameSuffix = " (dupe 1)";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                }

                                FileNames.Add(FileName + FileExtension);

                                if (Downloads.Default.SaveThumbnails) {
                                    string ThumbnailName = FileName + "s";
                                    string ThumbnailLink = FileLink.Substring(0, FileLink.Length - 12) + "s_u18chan" + FileExtension;
                                    ThumbnailNames.Add(ThumbnailName + FileExtension);
                                    ThumbnailFiles.Add(ThumbnailLink);

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

                                ThreadImagesCount++;
                            }
                        }
                        #endregion
                    }
                    else {
                        #region Old Parse Logic
                        List<string> FilesMatches = new List<string>();
                        Regex ImageMatch = new Regex("(?<=File: <a href=\").*?(?=\" target=\"_blank\">)");
                        foreach (Match ImageLink in ImageMatch.Matches(ThreadHTML)) {
                            FilesMatches.Add(ImageLink.Value);
                        }
                        if (FilesMatches.Count > ImageFiles.Count) {
                            for (int FilesMatchesIndex = ThreadImagesCount; FilesMatchesIndex < FilesMatches.Count; FilesMatchesIndex++, ThreadImagesCount++) {
                                string FileName = FilesMatches[FilesMatchesIndex].Split('/')[FilesMatches[FilesMatchesIndex].Split('/').Length - 1];
                                string Extension = "." + FileName.Split('.')[FileName.Split('.').Length - 1];
                                FileName = FileName.Substring(0, FileName.Length - Extension.Length); // Remove extension from filename

                                //FileIDs.Add(IDs[i]);
                                OriginalFileNames.Add(FileName);
                                FileExtensions.Add(Extension);
                                ImageFiles.Add(FilesMatches[FilesMatchesIndex]);

                                if (Downloads.Default.SaveOriginalFilenames) {
                                    string FileNamePrefix = "";
                                    string FileNameSuffix = "";

                                    do {
                                        FileName = FileName.Substring(0, FileName.Length - 8);
                                    } while (FileName.EndsWith("_u18chan"));

                                    for (int IllegalCharacterIndex = 0; IllegalCharacterIndex < Chans.InvalidFileCharacters.Length; IllegalCharacterIndex++) {
                                        FileName = FileName.Replace(Chans.InvalidFileCharacters[IllegalCharacterIndex], "_");
                                    }

                                    if (Downloads.Default.PreventDuplicates) {
                                        if (FileNames.Contains(FileName)) {
                                            if (FileNamesDupes.Contains(FileName)) {
                                                int DupeNameIndex = FileNamesDupes.IndexOf(FileName);
                                                FileNamesDupesCount[DupeNameIndex] += 1;
                                                FileNameSuffix = " (dupe " + FileNamesDupesCount[DupeNameIndex].ToString() + ")";
                                            }
                                            else {
                                                FileNamesDupes.Add(FileName);
                                                FileNamesDupesCount.Add(1);
                                                FileNameSuffix = " (dupe 1)";
                                            }
                                        }
                                    }

                                    FileName = FileNamePrefix + FileName + FileNameSuffix;
                                }

                                FileNames.Add(FileName);

                                if (Downloads.Default.SaveThumbnails) {
                                    string Ext = FilesMatches[FilesMatchesIndex].Split('.')[FilesMatches[FilesMatchesIndex].Split('.').Length - 1];
                                    string ThumbFileBuffer = FilesMatches[FilesMatchesIndex].Replace("_u18chan." + Ext, "s_u18chan." + Ext);
                                    ThumbnailFiles.Add(ThumbFileBuffer);
                                    if (Downloads.Default.SaveHTML) {
                                        ThreadHTML = ThreadHTML.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=", "src=");
                                        ThreadHTML = ThreadHTML.Replace(ThumbFileBuffer, "thumb/" + ThumbFileBuffer.Split('/')[ThumbFileBuffer.Split('/').Length - 1]);
                                    }
                                }

                                if (Downloads.Default.SaveHTML) {
                                    ThreadHTML = ThreadHTML.Replace(FilesMatches[FilesMatchesIndex], FileName);
                                }

                                ListViewItem lvi = new ListViewItem();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = FilesMatches[FilesMatchesIndex];
                                lvi.SubItems[0].Text = (FilesMatchesIndex + 1).ToString();
                                lvi.SubItems[1].Text = FileExtensions[FilesMatchesIndex];
                                lvi.SubItems[2].Text = FileNames[FilesMatchesIndex];
                                lvi.ImageIndex = 0;
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items.Add(lvi);
                                }));
                            }
                        }
                        #endregion
                    }


                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ThreadImagesCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                        MainFormInstance.SetItemStatus(ThreadURL, ThreadStatus.ThreadDownloading);
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        CurrentThread.Status = ThreadStatus.ThreadDownloading;
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));
                        CurrentURL = ImageFiles[ImageFilesIndex];

                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Chans.DownloadFile(ImageFiles[ImageFilesIndex], DownloadPath, FileNames[ImageFilesIndex])) {
                            DownloadedImagesCount++;

                            if (Downloads.Default.SaveThumbnails) {
                                CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Chans.DownloadFile(ThumbnailFiles[ImageFilesIndex], DownloadPath + "\\thumb", ThumbnailNames[ImageFilesIndex]);
                            }

                            this.BeginInvoke(new MethodInvoker(() => {
                                lbDownloadedFiles.Text = DownloadedImagesCount.ToString();
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    CurrentThread.Status = ThreadStatus.ThreadAborted;
                    return;
                }
                catch (ObjectDisposedException) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lbNotModified.Visible = true;
                        }));
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            CurrentThread.Status = ThreadStatus.Thread404;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                #endregion
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "u18chan thread /" + CurrentThread.ThreadBoard + "/" + CurrentThread.ThreadID;
        }
        #endregion
    }
}