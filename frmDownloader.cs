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
        // Theory: If file name exists in FileNames, add a prefix based on FileNames.Count + 1
        // Only works if it's counted using the ThreadImageCount instead of 0.
        // 4chan done.

        #region Variables
        frmMain MainFormInstance = Program.GetMainFormInstance();   // all, the instance of the main for for modifying it
        // when anything major changes in the download form.

        public string DownloadPath = null;                          // all, the local directory for the files to save to.
        public string ThreadURL = null;                             // all, the URL passed from the main form.
        public int ChanType = (int)ChanTypes.Types.None;            // all, the int-based chan type.
        public DateTime LastModified = default(DateTime);           // 4chan 7chan 8chan,
        // uses If-Modified-Since header on requests
        // to prevent repeat requests overloading their servers.
        // all logic includes this, but only some make use of it.

        private string ThreadBoard = null;                          // all, the chan board.
        private string ThreadID = null;                             // all, the thread ID.
        private string LastThreadHTML = null;                       // 7chan fchan u18chan, the last HTML of the thread.
        // (used as a make-shift if-modified-since header)

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
        private bool ThreadScanned = false;     // all, Prevents thread data (ThreadBoard, ThreadID ...) from being rewrote on rescans.
        private bool Thread404 = false;         // all, determines if a thread 404'd.
        private bool DownloadAborted = false;   // all, determines if a thread was aborted.
        private int ThreadImagesCount = 0;       // all, counts the images in the thread. restarts parsing at this index.
        private int DownloadedImagesCount = 0;   // all, counts the images that have downloaded.
        private int ExtraFilesImageCount = 0;   // 8kun, !LEGACY! restarts parsing extra files at this index.
        private int ThreadPostsCount = 0;        // 8chan 8kun, restarts the parsing at this index.
        private int CountdownToNextScan = 0;    // all, countdown between rescans.
        private int HideModifiedLabelAt = 0;    // all, hides the modified at 10 seconds less of CountdownToNextScan.
        private Thread DownloadThread;          // all, the main download thread.
        private Thread TimerIdle;               // all, the timer idler for when the settings form is open.

        // Mostly-debug
        private bool UseOldLogic = false;               // all, maybe user-set. Uses old parsing logic instead of latest.
        private bool UseConfirmedWorkingLogic = false;  // all, if any of them use non-fully tested logic. debug only.
        private bool MessageBoxPerFile = false;         // all, debug to display a message box of the URL before download
        private bool PauseBetweenFiles = true;         // all, temp pauses between file downloads.
        #endregion

        public frmDownloader() {
            InitializeComponent();
            this.Icon = Properties.Resources.YChanEx;
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

            if (Thread404) {
                lbScanTimer.Text = "404'd";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                this.Icon = Properties.Resources.YChanEx404;
                MainFormInstance.Announce404(ThreadID, ThreadBoard, ThreadURL, ChanType);
                MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Has404);
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
                lbScanTimer.Text = "scanning now...";
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
                Thread404 = true;
                btnForce404.Enabled = false;
                AfterDownload();
            }
        }
        private void btnAbortRetry_Click(object sender, EventArgs e) {
            if (!Thread404 && !DownloadAborted) {
                StopDownload();
                btnAbortRetry.Text = "Retry";
                lbNotModified.Visible = false;
                if (Program.IsDebug) {
                    btnForce404.Enabled = false;
                }
            }
            else {
                btnAbortRetry.Text = "Abort";
                if (Program.IsDebug) {
                    btnForce404.Enabled = true;
                }
                RetryScanOnFailure();
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
            if (ThreadID != null) {
                Clipboard.SetText(ThreadID);
            }
        }

        private void mCopyThreadURL_Click(object sender, EventArgs e) {
            if (ThreadURL != null) {
                Clipboard.SetText(ThreadURL);
            }
        }
        #endregion
        #region Custom Thread Actions Methods
        public void StartDownload() {
            GC.Collect();
            ChanType = Chans.GetChanType(ThreadURL);
            switch (ChanType) {
                case (int)ChanTypes.Types.FourChan:
                    if (!ThreadScanned) {
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("4chan thread - {0} - {1}", BoardTitles.FourChan(ThreadBoard), ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\4chan\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\4chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    Set4chanThread();
                    break;
                case (int)ChanTypes.Types.FourTwentyChan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 4];
                        ThreadID = URLSplit[URLSplit.Length - 2].Split('#')[0];
                        this.Text = string.Format("420chan thread - {0} - {1}", BoardTitles.FourTwentyChan(ThreadBoard), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\420chan\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    Set420chanThread();
                    break;
                case (int)ChanTypes.Types.SevenChan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "");
                        this.Text = string.Format("7chan thread - {0} - {1}", BoardTitles.SevenChan(ThreadBoard), ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\7chan\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\7chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    Set7chanThread();
                    break;
                case (int)ChanTypes.Types.EightChan:
                    if (!ThreadScanned) {
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        this.Text = string.Format("8chan thread - {0} - {1}", BoardTitles.EightChan(ThreadBoard, false), ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\8chan\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\8chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    Set8chanThread();
                    break;
                case (int)ChanTypes.Types.EightKun:
                    if (!ThreadScanned) {
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        this.Text = string.Format("8kun thread - {0} - {1}", BoardTitles.EightKun(ThreadBoard, false), ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\8kun\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\8kun\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    Set8kunThread();
                    break;
                case (int)ChanTypes.Types.fchan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("fchan thread - {0} - {1}", BoardTitles.fchan(ThreadBoard), ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\fchan\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\fchan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetFchanThread();
                    break;
                case (int)ChanTypes.Types.u18chan:
                    if (!ThreadScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("u18chan thread - {0} - {1}", BoardTitles.u18chan(ThreadBoard), ThreadID);
                    }
                    if (DownloadPath != Downloads.Default.DownloadPath + "\\u18chan\\" + ThreadBoard + "\\" + ThreadID) {
                        DownloadPath = Downloads.Default.DownloadPath + "\\u18chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    Setu18ChanThread();
                    break;
                default:
                    MainFormInstance.SetItemStatus(ThreadURL, "Invalid");
                    return;
            }

            if (DownloadPath != null) {
                btnOpenFolder.Enabled = true;
            }

            HideModifiedLabelAt = Downloads.Default.ScannerDelay - 10;
            DownloadThread.Start();
            MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Downloading);
        }
        public void StopDownload() {
            DownloadAborted = true;
            tmrScan.Stop();
            if (DownloadThread != null && DownloadThread.IsAlive) {
                DownloadThread.Abort();
            }
            lbScanTimer.Text = "aborted download";
            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
            //btnStopDownload.Enabled = false;
            MainFormInstance.AnnounceAbort(ThreadURL);
            MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.HasAborted);
        }
        public void AfterDownload() {
            if (DownloadAborted) {
                return;
            }
            else {
                this.BeginInvoke(new MethodInvoker(() => {
                    lbScanTimer.Text = "soon (tm)";
                    MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Waiting);
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
            Thread404 = false;
            DownloadAborted = false;
            MainFormInstance.Un404Thread(ThreadURL);
            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
            StartDownload();
            lbScanTimer.Text = "scanning now...";
            tmrScan.Stop();
        }
        public void AbortDownloadForClosing() {
            if (DownloadThread != null && DownloadThread.IsAlive) {
                DownloadThread.Abort();
            }
        }
        #endregion


        #region 4chan Download Logic Completed.
        private void Set4chanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://i.4cdn.org/" + ThreadBoard + "/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    #region API/HTML Download Logic
                    if (ThreadBoard == null || ThreadID == null) {
                        Thread404 = true;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = string.Format(ChanApiLinks.FourChan, ThreadBoard, ThreadID);
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
                                OldHTMLLinks = "//i.4cdn.org/" + ThreadBoard + "/" + FileID + "s.jpg";
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + FileID + "s.jpg");
                            }

                            OldHTMLLinks = "//i.4cdn.org/" + ThreadBoard + "/" + FileID;
                            string OldHTMLLinks2 = "//is2.4chan.org/" + ThreadBoard + "/" + FileID;
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
                    }));
                    #endregion

                    #region Download Logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
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
                    DownloadAborted = true;
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
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "4chan thread /" + ThreadBoard + "/" + ThreadID;
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
                string FileBaseURL = "https://boards.420chan.org/" + ThreadBoard + "/src/";
                string ThumbnailBaseUrl = "https://boards.420chan.org/" + ThreadBoard + "/thumb/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {

                    #region API/HTML Download Logic
                    if (ThreadBoard == null || ThreadID == null) {
                        Thread404 = true;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = string.Format(ChanApiLinks.FourTwentyChan, ThreadBoard, ThreadID);
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
                        ThreadHTML.Replace("href=\"/" + ThreadBoard + "/src/", "");
                        ThreadHTML.Replace("href=\"/" + ThreadBoard, "");
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
                    }));
                    #endregion

                    #region Download Logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
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
                    DownloadAborted = true;
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
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "420chan thread /" + ThreadBoard + "/" + ThreadID;
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
                            Thread404 = true;
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
                    string HTMLBuffer = ThreadHTML; // This will be saved instead of ThreadHTML
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
                            string FileName = FileLink.Split('/')[5];
                            FileName = FileName.Substring(0, FileName.Length - FileExtension.Length);

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
                                ThumbnailFiles.Add(BaseURL + ThreadBoard + "/thumb/" + FileName + "s" + FileExtension);
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
                            HTMLBuffer = HTMLBuffer.Replace("https://7chan.org/" + ThreadBoard + "/src/", "");
                            HTMLBuffer = HTMLBuffer.Replace("https://7chan.org/" + ThreadBoard + "/thumb/", "thumb/");
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
                                        ThumbnailFiles.Add("https://7chan.org/" + ThreadBoard + "/thumb/" + FileName + "s.jpg");
                                    }

                                    if (Downloads.Default.SaveHTML) {
                                        //HTMLBuffer = HTMLBuffer.Replace(FilesBuffer[i], FileName + "." + Extension);
                                        HTMLBuffer = HTMLBuffer.Replace("https://7chan.org/" + ThreadBoard, "");
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
                    }));
                    #endregion

                    #region Download Logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", HTMLBuffer);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch Logic
                catch (ThreadAbortException) {
                    DownloadAborted = true;
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
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "7chan thread /" + ThreadBoard + "/" + ThreadID;
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
                    if (ThreadBoard == null || ThreadID == null) {
                        Thread404 = true;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = string.Format(ChanApiLinks.EightChan, ThreadBoard, ThreadID);
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
                        string FileID = ThreadID + "-" + (PostIndex + 1).ToString();
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
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
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
                    DownloadAborted = true;
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
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "8chan thread /" + ThreadBoard + "/" + ThreadID;
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
                    if (ThreadBoard == null || ThreadID == null) {
                        Thread404 = true;
                        AfterDownload();
                        return;
                    }

                    #region Download API
                    CurrentURL = string.Format(ChanApiLinks.EightKun, ThreadBoard, ThreadID);
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
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
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
                    DownloadAborted = true;
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
                        AfterDownload();
                    }
                    if (Response.StatusCode == HttpStatusCode.BadGateway) {
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "8kun thread /" + ThreadBoard + "/" + ThreadID;
        }
        #endregion

        #region fchan Download Logic TODO: Actually implement properly.
        private void SetFchanThread() {
            DownloadThread = new Thread(() => {
                string ThreadHTML = null;
                string CurrentURL = null;
                try {

                    #region Download HTML Logic
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
                            Thread404 = true;
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
                    Regex ImageMatch = new Regex(ChanRegex.fchanFiles);
                    Regex NameMatch = new Regex(ChanRegex.fchanNames);

                    foreach (Match FoundMatch in ImageMatch.Matches(ThreadHTML)) {

                    }

                    List<string> FilesBuffer = new List<string>();
                    string HTMLBuffer = ThreadHTML; // This will be saved instead of ThreadHTML
                    foreach (Match ImageLink in ImageMatch.Matches(ThreadHTML)) {
                        FilesBuffer.Add(ImageLink.ToString());
                    }

                    if (FilesBuffer.Count > ImageFiles.Count) {
                        for (int i = ImageFiles.Count; i < FilesBuffer.Count; i++) {


                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = FilesBuffer[i];
                            //lvi.SubItems[0].Text = IDs[i];
                            lvi.SubItems[1].Text = FileExtensions[i];
                            lvi.SubItems[2].Text = FileNames[i];
                            lvi.ImageIndex = 0;
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));

                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotalFiles.Text = ImageFiles.Count.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++, DownloadedImagesCount++) {
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items[ImageFilesIndex].ImageIndex = 1;
                        }));

                        string FileName = ImageFiles[ImageFilesIndex].Split('/')[ImageFiles[ImageFilesIndex].Split('/').Length - 1];
                        CurrentURL = ImageFiles[ImageFilesIndex];
                        for (int y = 0; y < Chans.InvalidFileCharacters.Length; y++) {
                            FileName = FileName.Replace(Chans.InvalidFileCharacters[y], "_");
                            HTMLBuffer = HTMLBuffer.Replace(ImageFiles[ImageFilesIndex].Split('/')[ImageFiles[ImageFilesIndex].Split('/').Length - 1], FileName);
                        }

                        if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                        if (Chans.DownloadFile(ImageFiles[ImageFilesIndex], DownloadPath, FileName)) {
                            if (Downloads.Default.SaveThumbnails) {
                                CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                FileName = ThumbnailFiles[ImageFilesIndex].Split('/')[ThumbnailFiles[ImageFilesIndex].Split('/').Length - 1];
                                for (int y = 0; y < Chans.InvalidFileCharacters.Length; y++) {
                                    FileName = FileName.Replace(Chans.InvalidFileCharacters[y], "_");
                                    HTMLBuffer = HTMLBuffer.Replace(ThumbnailFiles[ImageFilesIndex].Split('/')[ThumbnailFiles[ImageFilesIndex].Split('/').Length - 1], FileName);
                                }
                                if (MessageBoxPerFile) { MessageBox.Show(CurrentURL); }
                                Chans.DownloadFile(ThumbnailFiles[ImageFilesIndex], DownloadPath + "\\thumb\\", FileName);
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", HTMLBuffer);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    DownloadAborted = true;
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
                        AfterDownload();
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "fchan thread /" + ThreadBoard + "/" + ThreadID;
        }
        #endregion

        #region u18chan Download Logic (Basically completed) TODO: HTML replacement.
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
                            Thread404 = true;
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
                    string HTMLBuffer = ThreadHTML; // This will be saved instead of ThreadHTML
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
                                string Extension = "." + FileName.Split('.')[FileName.Split('.').Length - 1];
                                FileName = FileName.Substring(0, FileName.Length - Extension.Length);

                                OriginalFileNames.Add(FileName);
                                FileExtensions.Add(Extension);
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

                                FileNames.Add(FileName + Extension);

                                if (Downloads.Default.SaveThumbnails) {
                                    string ThumbnailName = FileName + "s";
                                    string ThumbnailLink = FileLink.Substring(0, FileLink.Length - 12) + "s_u18chan" + Extension;
                                    ThumbnailNames.Add(ThumbnailName + Extension);
                                    ThumbnailFiles.Add(ThumbnailLink);

                                    if (Downloads.Default.SaveHTML) {
                                        HTMLBuffer = HTMLBuffer.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=", "src=\"");
                                        HTMLBuffer = HTMLBuffer.Replace(ThumbnailLink, "thumb/" + ThumbnailLink.Split('/')[ThumbnailLink.Split('/').Length - 1]);
                                    }
                                }

                                if (Downloads.Default.SaveHTML) {
                                    HTMLBuffer = HTMLBuffer.Replace(FileLink, FileName + Extension);
                                }

                                ListViewItem lvi = new ListViewItem();
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                                lvi.Name = PostID;
                                lvi.SubItems[0].Text = PostID;
                                lvi.SubItems[1].Text = Extension;
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
                                        HTMLBuffer = HTMLBuffer.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=", "src=");
                                        HTMLBuffer = HTMLBuffer.Replace(ThumbFileBuffer, "thumb/" + ThumbFileBuffer.Split('/')[ThumbFileBuffer.Split('/').Length - 1]);
                                    }
                                }

                                if (Downloads.Default.SaveHTML) {
                                    HTMLBuffer = HTMLBuffer.Replace(FilesMatches[FilesMatchesIndex], FileName);
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
                    }));
                    #endregion

                    #region Download logic
                    for (int ImageFilesIndex = DownloadedImagesCount; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
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
                        File.WriteAllText(DownloadPath + "\\Thread.html", HTMLBuffer);
                    }
                    #endregion

                    ThreadScanned = true;
                }
                #region Catch logic
                catch (ThreadAbortException) {
                    DownloadAborted = true;
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
                            Thread404 = true;
                        }
                        else {
                            ErrorLog.ReportWebException(WebEx, CurrentURL);
                        }
                    }
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                }
                finally {
                    AfterDownload();
                }
                #endregion
            });
            DownloadThread.Name = "u18chan thread /" + ThreadBoard + "/" + ThreadID;
        }
        #endregion
    }
}