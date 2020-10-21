﻿using System;
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
        frmMain MainFormInstance = Program.GetMainFormInstance();

        public string DownloadPath = null; // all
        public string ThreadURL = null; // all, threadurl passed from the main form
        public int ChanType = -1; // all
        public DateTime LastModified = default(DateTime); // if supported

        private string ThreadBoard = null; // all
        private string ThreadID = null; // all
        private string LastThreadHTML = null; // u18chan

        private List<string> ImageFiles = new List<string>();
        private List<string> ThumbnailFiles = new List<string>();
        private List<string> ThumbnailNames = new List<string>(); // 8chan, 8kun
        private List<string> FileIDs = new List<string>();
        private List<string> FileNames = new List<string>();
        private List<string> FileHashes = new List<string>();
        private List<string> FileExtensions = new List<string>();
        private int ThreadImageCount = 0;
        private int ExtraFilesImageCount = 0; // 8kun
        private bool ThreadHasScanned = false;
        private bool ThreadHas404 = false;
        private bool IsAbortingDownload = false;
        private int CountdownToNextScan = 0;
        private int HideModifiedLabelAt = 0;
        private Thread DownloadThread;

        private bool UseConfirmedWorkingLogic = true; // all, if any of them use non-fully tested logic. debug only
        private bool UseOldLogic = false;

        public frmDownloader() {
            InitializeComponent();
            this.Icon = Properties.Resources.YChanEx;
            if (Program.IsDebug) {
                btnForce404.Enabled = true;
                btnForce404.Visible = true;
            }
        }
        private void frmDownloader_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            this.Hide();
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
        private void cmCancelDownload_Click(object sender, EventArgs e) {
            StopDownload();
            if (Program.IsDebug) {
                btnForce404.Enabled = false;
            }
        }
        private void cmCloseForm_Click(object sender, EventArgs e) {
            this.Hide();
        }
        private void tmrScan_Tick(object sender, EventArgs e) {
            if (Program.SettingsOpen) {
                Thread.Sleep(500);
                return;
            }
            if (ThreadHas404) {
                lbScanTimer.Text = "404'd";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                this.Icon = Properties.Resources.YChanEx404;
                MainFormInstance.Announce404(ThreadID, ThreadBoard, ThreadURL, ChanType);
                MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Has404);
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
                string FileName = lvImages.SelectedItems[i].Text + "." + lvImages.SelectedItems[i].SubItems[1].Text;
                if (File.Exists(DownloadPath + "\\" + FileName)) {
                    System.Diagnostics.Process.Start(DownloadPath + "\\" + FileName);
                }
            }
        }
        private void btnForce404_Click(object sender, EventArgs e) {
            if (Program.IsDebug) {
                ThreadHas404 = true;
                btnForce404.Enabled = false;
                AfterDownload();
            }
        }

        public void StartDownload() {
            ChanType = Chans.GetChanType(ThreadURL);
            switch (ChanType) {
                case (int)ChanTypes.Types.FourChan:
                    if (!ThreadHasScanned) {
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("4chan thread - {0} - {1}", BoardTitles.FourChan(ThreadBoard), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\4chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetFourChanThread();
                    break;
                case (int)ChanTypes.Types.FourTwentyChan:
                    if (!ThreadHasScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 4];
                        ThreadID = URLSplit[URLSplit.Length - 2].Split('#')[0];
                        this.Text = string.Format("420chan thread - {0} - {1}", BoardTitles.FourTwentyChan(ThreadBoard), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\420chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetFourTwentyChanThread();
                    break;
                case (int)ChanTypes.Types.SevenChan:
                    if (!ThreadHasScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "");
                        this.Text = string.Format("7chan thread - {0} - {1}", BoardTitles.SevenChan(ThreadBoard), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\7chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetSevenChanThread();
                    break;
                case (int)ChanTypes.Types.EightChan:
                   if (!ThreadHasScanned) {
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        this.Text = string.Format("8chan thread - {0} - {1}", BoardTitles.EightChan(ThreadBoard, false), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\8chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetEightChanThread();
                    break;
                case (int)ChanTypes.Types.EightKun:
                    if (!ThreadHasScanned) {
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0].Replace(".html", "").Replace(".json", "");
                        this.Text = string.Format("8kun thread - {0} - {1}", BoardTitles.EightKun(ThreadBoard, false), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\8kun\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetEightKunThread();
                    break;
                case (int)ChanTypes.Types.fchan:
                    return;
                    if (!ThreadHasScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("fchan thread - {0} - {1}", BoardTitles.fchan(ThreadBoard), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\fchan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetFchanThread();
                    break;
                case (int)ChanTypes.Types.u18han:
                    if (!ThreadHasScanned) {
                        lvImages.Columns.RemoveAt(3);
                        string[] URLSplit = ThreadURL.Split('/');
                        ThreadBoard = URLSplit[URLSplit.Length - 3];
                        ThreadID = URLSplit[URLSplit.Length - 1].Split('#')[0];
                        this.Text = string.Format("u18chan thread - {0} - {1}", BoardTitles.u18chan(ThreadBoard), ThreadID);
                        DownloadPath = Downloads.Default.DownloadPath + "\\u18chan\\" + ThreadBoard + "\\" + ThreadID;
                    }
                    SetUEighteenChanThread();
                    break;
                default:
                    MainFormInstance.SetItemStatus(ThreadURL, "Invalid");
                    return;
            }

            if (DownloadPath != null) {
                btnOpenFolder.Enabled = true;
            }

            HideModifiedLabelAt = Downloads.Default.ScannerDelay - 9;
            DownloadThread.Start();
            MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Downloading);
        }
        public void StopDownload() {
            IsAbortingDownload = true;
            tmrScan.Stop();
            if (DownloadThread.IsAlive) {
                DownloadThread.Abort();
            }
            lbScanTimer.Text = "aborted download";
            lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
            //btnStopDownload.Enabled = false;
            MainFormInstance.AnnounceAbort(ThreadURL);
            MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.HasAborted);
        }
        public void AfterDownload() {
            if (IsAbortingDownload) {
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
                    if (ChanType == (int)ChanTypes.Types.u18han) {
                        GC.Collect();
                    }
                }));
            }
        }

    #region 4chan
        private void SetFourChanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://i.4cdn.org/" + ThreadBoard + "/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    if (ThreadBoard == null || ThreadID == null) {
                        ThreadHas404 = true;
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

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlFileID = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/tim");
                    XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");
                    XmlNodeList xmlExt = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");
                    XmlNodeList xmlHash = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/md5");

                    for (int FileIdIndex = ThreadImageCount; FileIdIndex < xmlFileID.Count; FileIdIndex++, ThreadImageCount++) {
                        if (xmlFileID[FileIdIndex] == null) {
                            continue;
                        }
                        FileIDs.Add(xmlFileID[FileIdIndex].InnerText);
                        ImageFiles.Add(FileBaseURL + xmlFileID[FileIdIndex].InnerText + xmlExt[FileIdIndex].InnerText);
                        ThumbnailFiles.Add(FileBaseURL + xmlFileID[FileIdIndex].InnerText + "s.jpg");
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                            string FileName = xmlFileName[FileIdIndex].InnerText;
                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                            }
                            FileNames.Add(FileName + xmlExt[FileIdIndex].InnerText);
                        }
                        else {
                            FileNames.Add(xmlFileID[FileIdIndex].InnerText + xmlExt[FileIdIndex].InnerText);
                        }
                        FileHashes.Add(xmlHash[FileIdIndex].InnerText);

                        if (YChanEx.Downloads.Default.SaveHTML) {
                            string OldHTMLLinks = null;

                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                OldHTMLLinks = "//i.4cdn.org/" + ThreadBoard + "/" + xmlFileID[FileIdIndex].InnerText + "s.jpg";
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + xmlFileID[FileIdIndex].InnerText + "s.jpg");
                            }
                            OldHTMLLinks = "//i.4cdn.org/" + ThreadBoard + "/" + xmlFileID[FileIdIndex].InnerText;
                            if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                string FileNamePrefix = "";
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
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileNamePrefix + xmlFileName[FileIdIndex].InnerText);
                            }
                            else {
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, xmlFileID[FileIdIndex].InnerText);
                            }
                        }

                        ListViewItem lvi = new ListViewItem();
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.Name = xmlFileID[FileIdIndex].InnerText;
                        lvi.SubItems[0].Text = xmlFileID[FileIdIndex].InnerText;
                        lvi.SubItems[1].Text = xmlExt[FileIdIndex].InnerText.Trim('.');
                        lvi.SubItems[2].Text = xmlFileName[FileIdIndex].InnerText;
                        lvi.SubItems[3].Text = xmlHash[FileIdIndex].InnerText;
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items.Add(lvi);
                        }));
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ThreadImageCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int ImageFilesIndex = 0; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        if (ImageFiles[ImageFilesIndex] == null) {
                            continue;
                        }
                        string FileName = FileNames[ImageFilesIndex];
                        CurrentURL = ImageFiles[ImageFilesIndex];
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames && YChanEx.Downloads.Default.PreventDuplicates) {
                            if (ImageFilesIndex >= 10) {
                                if (ImageFilesIndex >= 100) {
                                    FileName = "(" + ImageFilesIndex.ToString() + ") " + FileName;
                                }
                                else {
                                    FileName = "(0" + ImageFilesIndex.ToString() + ") " + FileName;
                                }
                            }
                            else {
                                FileName = "(00" + ImageFilesIndex.ToString() + ") " + FileName;
                            }
                        }

                        Chans.DownloadFile(CurrentURL, DownloadPath, FileName);

                        if (YChanEx.Downloads.Default.SaveThumbnails) {
                            CurrentURL = ThumbnailFiles[ImageFilesIndex];
                            Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[ImageFilesIndex] + "s.jpg");
                        }



                    }

                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    ThreadHasScanned = true;
                }
                catch (ThreadAbortException) {
                    IsAbortingDownload = true;
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
                            ThreadHas404 = true;
                        }
                        else {
                            MessageBox.Show(WebEx.ToString());
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "4chan thread /" + ThreadBoard + "/" + ThreadID;
        }
        private static bool GenerateFourChanMD5(string InputFile, string InputFileHash) {
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
                //ErrorLog
                return false;
            }
        }
    #endregion

    #region 420chan
        private void SetFourTwentyChanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://boards.420chan.org/" + ThreadBoard + "/src/";
                string ThumbnailBaseUrl = "https://boards.420chan.org/" + ThreadBoard + "/thumb/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    if (ThreadBoard == null || ThreadID == null) {
                        ThreadHas404 = true;
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

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");
                    XmlNodeList xmlExt = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");

                    for (int FileNameIndex = ThreadImageCount; FileNameIndex < xmlFileName.Count; FileNameIndex++, ThreadImageCount++) {
                        if (xmlFileName[FileNameIndex] != null) {
                            FileIDs.Add(xmlFileName[FileNameIndex].InnerText);
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
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ThreadImageCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int ImageFilesIndex = 0; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        if (ImageFiles[ImageFilesIndex] != null) {
                            string FileName = FileIDs[ImageFilesIndex] + FileExtensions[ImageFilesIndex];
                            CurrentURL = ImageFiles[ImageFilesIndex];

                            Chans.DownloadFile(CurrentURL, DownloadPath, FileName);

                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                CurrentURL = ThumbnailFiles[ImageFilesIndex];
                                Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[ImageFilesIndex] + "s.jpg");
                            }
                        }
                    }

                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }
                    ThreadHasScanned = true;
                }
                catch (ThreadAbortException) {
                    IsAbortingDownload = true;
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
                            ThreadHas404 = true;
                        }
                        else {
                            MessageBox.Show(WebEx.ToString());
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "420chan thread /" + ThreadBoard + "/" + ThreadID;
        }   
    #endregion

    #region 7chan
        private void SetSevenChanThread() {
            DownloadThread = new Thread(() => {
                string ThreadHTML = null;
                string CurrentURL = null;
                try {
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
                            ThreadHas404 = true;
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

                    Regex ImageMatch = new Regex(ChanRegex.SevenChanFiles);

                    List<string> FilesBuffer = new List<string>();
                    string HTMLBuffer = ThreadHTML; // This will be saved instead of ThreadHTML
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
                                this.BeginInvoke(new MethodInvoker(() => {
                                    lvImages.Items.Add(lvi);
                                }));
                            }
                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ImageFiles.Count;
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int ImageFilesIndex = 0; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        string FileName = ImageFiles[ImageFilesIndex].Split('/')[ImageFiles[ImageFilesIndex].Split('/').Length - 1];
                        for (int y = 0; y < Chans.InvalidFileCharacters.Length; y++) {
                            FileName = FileName.Replace(Chans.InvalidFileCharacters[y], "_");
                            HTMLBuffer = HTMLBuffer.Replace(ImageFiles[ImageFilesIndex].Split('/')[ImageFiles[ImageFilesIndex].Split('/').Length - 1], FileName);
                        }
                        Chans.DownloadFile(ImageFiles[ImageFilesIndex], DownloadPath, FileName);
                    }

                    if (Downloads.Default.SaveThumbnails) {
                        for (int i = 0; i < ThumbnailFiles.Count; i++) {
                            string FileName = ThumbnailFiles[i].Split('/')[ThumbnailFiles[i].Split('/').Length - 1];
                            for (int y = 0; y < Chans.InvalidFileCharacters.Length; y++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[y], "_");
                                HTMLBuffer = HTMLBuffer.Replace(ThumbnailFiles[i].Split('/')[ThumbnailFiles[i].Split('/').Length - 1], FileName);
                            }
                            Chans.DownloadFile(ThumbnailFiles[i], DownloadPath + "\\thumb\\", FileName);
                        }
                    }

                    if (Downloads.Default.SaveHTML) {
                        File.WriteAllText(DownloadPath + "\\Thread.html", HTMLBuffer);
                    }
                    ThreadHasScanned = true;
                    AfterDownload();
                }
                catch (ThreadAbortException) {
                    IsAbortingDownload = true;
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
                            ThreadHas404 = true;
                        }
                        else {
                            // ErrorLog
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            });
            DownloadThread.Name = "7chan thread /" + ThreadBoard + "/" + ThreadID;
        }
    #endregion

    #region 8chan
        private void SetEightChanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://8chan.moe";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    if (ThreadBoard == null || ThreadID == null) {
                        ThreadHas404 = true;
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

                    #region First post file(s)
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);
                    XmlNodeList xmlFilePath = xmlDoc.DocumentElement.SelectNodes("/root/files/item/path");
                    XmlNodeList xmlFileThumbnail = xmlDoc.DocumentElement.SelectNodes("/root/files/item/thumb");
                    XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/files/item/originalName");

                    for (int PostIndex = ThreadImageCount; PostIndex < xmlFilePath.Count; PostIndex++) {
                        if (xmlFilePath[PostIndex] == null) {
                            continue;
                        }
                        ThreadImageCount++;
                        string FileUrl = xmlFilePath[PostIndex].InnerText;
                        string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                        string FileID = ThreadID + "-" + (PostIndex + 1).ToString();
                        //string FileID = (ThreadImageCount + 1).ToString();
                        string FileExtension = "." + FileUrl.Split('/')[2].Split('.')[1];
                        FileExtensions.Add(FileExtension);
                        FileIDs.Add(FileID);
                        FileHashes.Add(FileHash);
                        ImageFiles.Add(FileBaseURL + FileUrl);
                        ThumbnailFiles.Add(FileBaseURL + xmlFileThumbnail[PostIndex].InnerText);
                        ThumbnailNames.Add(xmlFileThumbnail[PostIndex].InnerText.Substring(8));

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
                                        FileNamePrefix = "(" + ThreadImageCount.ToString() + ") ";
                                    }
                                    else {
                                        FileNamePrefix = "(0" + ThreadImageCount.ToString() + ") ";
                                    }
                                }
                                else {
                                    FileNamePrefix = "(00" + ThreadImageCount.ToString() + ") ";
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

                    #region Subsequent posts
                    if (UseOldLogic) {
                    #region Subsequent Old Logic
                        xmlFilePath = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/files/item/path");
                        xmlFileThumbnail = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/files/item/thumb");
                        xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/files/item/originalName");

                        for (int PostIndex = 0; PostIndex < xmlFilePath.Count; PostIndex++) {
                            if (xmlFilePath[PostIndex] == null) {
                                continue;
                            }
                            ThreadImageCount++;
                            string FileUrl = xmlFilePath[PostIndex].InnerText;
                            string FileHash = FileUrl.Substring(8, FileUrl.Length - 4 - 8);
                            string FileID = (ThreadImageCount + 1).ToString();
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
                                            FileNamePrefix = "(" + ThreadImageCount.ToString() + ") ";
                                        }
                                        else {
                                            FileNamePrefix = "(0" + ThreadImageCount.ToString() + ") ";
                                        }
                                    }
                                    else {
                                        FileNamePrefix = "(00" + ThreadImageCount.ToString() + ") ";
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
                                ThreadImageCount++;
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
                                                FileNamePrefix = "(" + ThreadImageCount.ToString() + ") ";
                                            }
                                            else {
                                                FileNamePrefix = "(0" + ThreadImageCount.ToString() + ") ";
                                            }
                                        }
                                        else {
                                            FileNamePrefix = "(00" + ThreadImageCount.ToString() + ") ";
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

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ThreadImageCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int ImageFilesIndex = 0; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        if (ImageFiles[ImageFilesIndex] == null) {
                            continue;
                        }
                        string FileName = FileNames[ImageFilesIndex];
                        CurrentURL = ImageFiles[ImageFilesIndex];

                        Chans.DownloadFile(CurrentURL, DownloadPath, FileNames[ImageFilesIndex] + FileExtensions[ImageFilesIndex]);

                        if (YChanEx.Downloads.Default.SaveThumbnails) {
                            CurrentURL = ThumbnailFiles[ImageFilesIndex];
                            Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", ThumbnailNames[ImageFilesIndex] + ".jpg");
                        }
                    }


                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }

                    ThreadHasScanned = true;
                }
                catch (ThreadAbortException) {
                    IsAbortingDownload = true;
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
                            ThreadHas404 = true;
                        }
                        else {
                            MessageBox.Show(WebEx.ToString());
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "8chan thread /" + ThreadBoard + "/" + ThreadID;
        }
    #endregion

    #region 8kun
        private void SetEightKunThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://media.8kun.top/file_store/";
                string ThumbnailFileBaseURL = "https://media.8kun.top/file_store/thumb/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    if (ThreadBoard == null || ThreadID == null) {
                        ThreadHas404 = true;
                        AfterDownload();
                        return;
                    }

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

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(ThreadJSON);

                    if (UseConfirmedWorkingLogic) {
                    #region Legacy 8kun
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
                            ThreadImageCount++;
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

                        // Scan extra files in posts
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
                    }
                    else {
                        // WIP
                        //XmlNodeList xmlPosts = xmlDoc.DocumentElement.SelectNodes("/root/posts/item");
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + (ThreadImageCount + ExtraFilesImageCount).ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int ImageFilesIndex = 0; ImageFilesIndex < ImageFiles.Count; ImageFilesIndex++) {
                        if (ImageFiles[ImageFilesIndex] == null) {
                            continue;
                        }
                        string FileName = FileNames[ImageFilesIndex];
                        CurrentURL = ImageFiles[ImageFilesIndex];

                        MessageBox.Show(CurrentURL);
                        Chans.DownloadFile(CurrentURL, DownloadPath, FileName);

                        if (YChanEx.Downloads.Default.SaveThumbnails) {
                            CurrentURL = ThumbnailFiles[ImageFilesIndex];
                            MessageBox.Show(CurrentURL);
                            Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[ImageFilesIndex] + FileExtensions[ImageFilesIndex]);
                        }
                    }

                    if (YChanEx.Downloads.Default.SaveHTML) {
                        File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                    }

                    ThreadHasScanned = true;
                }
                catch (ThreadAbortException) {
                    IsAbortingDownload = true;
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
                            ThreadHas404 = true;
                        }
                        else {
                            MessageBox.Show(WebEx.ToString());
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
                finally {
                    AfterDownload();
                }
            });
            DownloadThread.Name = "8kun thread /" + ThreadBoard + "/" + ThreadID;
        }
    #endregion

    #region fchan
        private void SetFchanThread() {
            return;
        }
    #endregion

    #region u18chan
        private void SetUEighteenChanThread() {
            DownloadThread = new Thread(() => {
                string ThreadHTML = null;
                string CurrentURL = null;
                try {
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
                            ThreadHas404 = true;
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

                    Regex ImageMatch = new Regex(ChanRegex.u18chanFiles);

                    List<string> IDs = new List<string>();
                    Regex IDMatch = new Regex("(?<=No.<a class=\"AltLink\" href=\"javascript:QuotePost\\().*?(?<=\\);\">)");
                    foreach (Match IDNumber in IDMatch.Matches(ThreadHTML)) {
                        IDs.Add(IDNumber.ToString().Replace(");\">", ""));
                    }

                    List<string> FilesBuffer = new List<string>();
                    string HTMLBuffer = ThreadHTML; // This will be saved instead of ThreadHTML
                    foreach (Match ImageLink in ImageMatch.Matches(ThreadHTML)) {
                        FilesBuffer.Add(ImageLink.ToString());
                    }

                    if (FilesBuffer.Count > ImageFiles.Count) {
                        for (int i = ImageFiles.Count; i < FilesBuffer.Count; i++) {
                            string FileName = FilesBuffer[i].Split('/')[FilesBuffer[i].Split('/').Length - 1];
                            string Extension = FilesBuffer[i].Split('.')[FilesBuffer[i].Split('.').Length - 1];
                            ImageFiles.Add(FilesBuffer[i]);
                            FileIDs.Add(IDs[i]);
                            FileNames.Add(FileName.Replace(FileName, FileName.Substring(0, (FileName.Length - (Extension.Length + 9)))));
                            FileExtensions.Add(Extension);

                            if (Downloads.Default.SaveThumbnails) {
                                string Ext = FilesBuffer[i].Split('.')[FilesBuffer[i].Split('.').Length - 1];
                                string ThumbFileBuffer = FilesBuffer[i].Replace("_u18chan." + Ext, "s_u18chan." + Ext);
                                ThumbnailFiles.Add(ThumbFileBuffer);
                                if (Downloads.Default.SaveHTML) {
                                    HTMLBuffer = HTMLBuffer.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=", "src=");
                                    HTMLBuffer = HTMLBuffer.Replace(ThumbFileBuffer, "thumb/" + ThumbFileBuffer.Split('/')[ThumbFileBuffer.Split('/').Length - 1]);
                                }
                            }

                            if (Downloads.Default.SaveHTML) {
                                HTMLBuffer = HTMLBuffer.Replace(FilesBuffer[i], FileName);
                            }

                            ListViewItem lvi = new ListViewItem();
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                            lvi.Name = FilesBuffer[i];
                            lvi.SubItems[0].Text = IDs[i];
                            lvi.SubItems[1].Text = FileExtensions[i];
                            lvi.SubItems[2].Text = FileNames[i];
                            this.BeginInvoke(new MethodInvoker(() => {
                                lvImages.Items.Add(lvi);
                            }));

                        }
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ImageFiles.Count;
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int i = 0; i < ImageFiles.Count; i++) {
                        string FileName = ImageFiles[i].Split('/')[ImageFiles[i].Split('/').Length - 1];
                        for (int y = 0; y < Chans.InvalidFileCharacters.Length; y++) {
                            FileName = FileName.Replace(Chans.InvalidFileCharacters[y], "_");
                            HTMLBuffer = HTMLBuffer.Replace(ImageFiles[i].Split('/')[ImageFiles[i].Split('/').Length - 1], FileName);
                        }
                        Chans.DownloadFile(ImageFiles[i], DownloadPath, FileName);
                    }

                    if (Downloads.Default.SaveThumbnails) {
                        for (int i = 0; i < ThumbnailFiles.Count; i++) {
                            string FileName = ThumbnailFiles[i].Split('/')[ThumbnailFiles[i].Split('/').Length - 1];
                            for (int y = 0; y < Chans.InvalidFileCharacters.Length; y++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[y], "_");
                                HTMLBuffer = HTMLBuffer.Replace(ThumbnailFiles[i].Split('/')[ThumbnailFiles[i].Split('/').Length - 1], FileName);
                            }
                            Chans.DownloadFile(ThumbnailFiles[i], DownloadPath + "\\thumb\\", FileName);
                        }
                    }

                    if (Downloads.Default.SaveHTML) {
                        File.WriteAllText(DownloadPath + "\\Thread.html", HTMLBuffer);
                    }
                    ThreadHasScanned = true;
                    AfterDownload();
                }
                catch (ThreadAbortException) {
                    IsAbortingDownload = true;
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
                            ThreadHas404 = true;
                        }
                        else {
                            // ErrorLog
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            });
            DownloadThread.Name = "u18chan thread /" + ThreadBoard + "/" + ThreadID;
        }
    #endregion
    }
}