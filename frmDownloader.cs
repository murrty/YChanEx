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
        frmMain MainFormInstance = Program.GetMainFormInstance();

        public string DownloadPath = null; // all
        public string ThreadURL = null; // all
        public ChanTypes.Types ChanType = ChanTypes.Types.None; // all
        public DateTime LastModified = default(DateTime); // if supported

        private string ThreadBoard = null; // all
        private string ThreadID = null; // all
        private string LastThreadHTML = null; // u18chan

        private List<string> ImageFiles = new List<string>();
        private List<string> ThumbnailFiles = new List<string>();
        private List<string> FileIDs = new List<string>();
        private List<string> FileNames = new List<string>();
        private List<string> FileHashes = new List<string>();
        private List<string> FileExtensions = new List<string>();
        private int ThreadImageCount = 0;
        private bool ThreadHasScanned = false;
        private bool ThreadHas404 = false;
        private bool IsAbortingDownload = false;
        private int CountdownToNextScan = 0;
        private Thread DownloadThread;

        public frmDownloader() {
            InitializeComponent();
            this.Icon = Properties.Resources.YChanEx;
        }
        private void frmDownloader_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }
        private void btnStopDownload_Click(object sender, EventArgs e) {
            StopDownload();
        }
        private void btnClose_Click(object sender, EventArgs e) {
            this.Hide();
        }
        private void tmrScan_Tick(object sender, EventArgs e) {
            if (Program.SettingsOpen) {
                return;
            }
            if (ThreadHas404) {
                lbScanTimer.Text = "404'd";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                this.Icon = Properties.Resources.YChanEx404;
                MainFormInstance.Announce404(ThreadID, ThreadBoard, ThreadURL);
                MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Has404);
                btnStopDownload.Enabled = false;
                tmrScan.Stop();
                return;
            }
            else if (CountdownToNextScan == 50) {
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

        public void StartDownload() {
            ChanType = Chans.GetChanType(ThreadURL);
            switch (ChanType) {
                case (int)ChanTypes.Types.fourChan:
                    if (!ThreadHasScanned) {
                        ThreadBoard = ThreadURL.Split('/')[ThreadURL.Split('/').Length - 3];
                        ThreadID = ThreadURL.Split('/')[ThreadURL.Split('/').Length - 1];
                        this.Text = "4chan thread - " + ThreadBoard + " - " + ThreadID;
                        DownloadPath = Downloads.Default.DownloadPath + "\\4chan\\" + ThreadBoard + "\\" + ThreadID;

                    }
                    SetFourChanThread();
                    DownloadThread.Start();
                    break;
                case ChanTypes.Types.fourTwentyChan:
                    break;
                case ChanTypes.Types.sevenChan:
                    break;
                case ChanTypes.Types.eightChan:
                    break;
                case ChanTypes.Types.fchan:
                    break;
                case ChanTypes.Types.uEighteenChan:
                    if (!ThreadHasScanned) {
                        ThreadBoard = ThreadURL.Split('/')[ThreadURL.Split('/').Length - 3];
                        ThreadID = ThreadURL.Split('/')[ThreadURL.Split('/').Length - 1];
                        this.Text = "u18chan thread - " + ThreadBoard + " - " + ThreadID;
                        DownloadPath = Downloads.Default.DownloadPath + "\\u18chan\\" + ThreadBoard + "\\" + ThreadID;
                        SetUEighteenChanThread();
                    }
                    BeginUEighteenChanDownlad();
                    break;
                default:
                    return;
            }

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
            btnStopDownload.Enabled = false;
            MainFormInstance.AnnounceAbort(ThreadURL);
            MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.HasAborted);
        }
        public void AfterDownload() {
            if (IsAbortingDownload) {
                return;
            }
            else if (ThreadHas404) {
                lbScanTimer.Text = "404'd";
                lbScanTimer.ForeColor = Color.FromKnownColor(KnownColor.Firebrick);
                this.Icon = Properties.Resources.YChanEx404;
                MainFormInstance.Announce404(ThreadID, ThreadBoard, ThreadURL);
                MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Has404);
                btnStopDownload.Enabled = false;
            }
            else {
                this.BeginInvoke(new MethodInvoker(() => {
                    lbScanTimer.Text = "soon (tm)";
                    MainFormInstance.SetItemStatus(ThreadURL, ThreadStatuses.Waiting);
                    CountdownToNextScan = Downloads.Default.ScannerDelay;
                    tmrScan.Start();
                    if (ChanType == ChanTypes.Types.uEighteenChan) {
                        GC.Collect();
                    }
                }));
            }
        }

        #region 4chan
        private void SetFourChanThread() {
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://i.4cdn.org/" + ThreadURL.Split('/')[3] + "/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    ThreadBoard = ThreadURL.Split('/')[ThreadURL.Split('/').Length - 3];
                    ThreadID = ThreadURL.Split('/')[ThreadURL.Split('/').Length - 1];

                    if (ThreadBoard == null || ThreadID == null) {
                        ThreadHas404 = true;
                        AfterDownload();
                        return;
                    }

                    CurrentURL = "https://a.4cdn.org/" + ThreadBoard + "/thread/" + ThreadID + ".json";
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

                    for (int i = ThreadImageCount; i < xmlFileID.Count; i++, ThreadImageCount++) {
                        if (xmlFileID[i] == null) {
                            continue;
                        }
                        FileIDs.Add(xmlFileID[i].InnerText);
                        ImageFiles.Add(FileBaseURL + xmlFileID[i].InnerText + xmlExt[i].InnerText);
                        ThumbnailFiles.Add(FileBaseURL + xmlFileID[i].InnerText + "s.jpg");
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                            string FileName = xmlFileName[i].InnerText;
                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[j], "_");
                            }
                            FileNames.Add(FileName + xmlExt[i].InnerText);
                        }
                        else {
                            FileNames.Add(xmlFileID[i].InnerText + xmlExt[i].InnerText);
                        }
                        FileHashes.Add(xmlHash[i].InnerText);

                        if (YChanEx.Downloads.Default.SaveHTML) {
                            string OldHTMLLinks = null;

                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                OldHTMLLinks = "//i.4cdn.org/" + ThreadBoard + "/" + xmlFileID[i].InnerText + "s.jpg";
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + xmlFileID[i].InnerText + "s.jpg");
                            }
                            OldHTMLLinks = "//i.4cdn.org/" + ThreadBoard + "/" + xmlFileID[i].InnerText;
                            if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                string FileNamePrefix = "";
                                if (YChanEx.Downloads.Default.PreventDuplicates) {
                                    if (i >= 10) {
                                        if (i >= 100) {
                                            FileNamePrefix = "(" + i.ToString() + ") ";
                                        }
                                        else {
                                            FileNamePrefix = "(0" + i.ToString() + ") ";
                                        }
                                    }
                                    else {
                                        FileNamePrefix = "(00" + i.ToString() + ") ";
                                    }
                                }
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, FileNamePrefix + xmlFileName[i].InnerText);
                            }
                            else {
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, xmlFileID[i].InnerText);
                            }
                        }

                        ListViewItem lvi = new ListViewItem();
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.Name = xmlFileID[i].InnerText;
                        lvi.SubItems[0].Text = xmlFileID[i].InnerText;
                        lvi.SubItems[1].Text = xmlExt[i].InnerText.Trim('.');
                        lvi.SubItems[2].Text = xmlFileName[i].InnerText;
                        lvi.SubItems[3].Text = xmlHash[i].InnerText;
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items.Add(lvi);
                        }));
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ThreadImageCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                        lbScanTimer.Text = "Downloading files";
                    }));

                    for (int i = 0; i < ImageFiles.Count; i++) {
                        if (ImageFiles[i] == null) {
                            continue;
                        }
                        string FileName = FileNames[i];
                        CurrentURL = ImageFiles[i];
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames && YChanEx.Downloads.Default.PreventDuplicates) {
                            if (i >= 10) {
                                if (i >= 100) {
                                    FileName = "(" + i.ToString() + ") " + FileName;
                                }
                                else {
                                    FileName = "(0" + i.ToString() + ") " + FileName;
                                }
                            }
                            else {
                                FileName = "(00" + i.ToString() + ") " + FileName;
                            }
                        }

                        Chans.DownloadFile(CurrentURL, DownloadPath, FileName);

                        if (YChanEx.Downloads.Default.SaveThumbnails) {
                            CurrentURL = ThumbnailFiles[i];
                            Chans.DownloadFile(CurrentURL, DownloadPath + "\\thumb", FileIDs[i] + "s.jpg");
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
        private static string GetFourChanBoardTopic(string board) {
            // Japanese Culture
            if (board == "/a/")
                return "Anime & Manga";
            else if (board == "/c/")
                return "Anime/Cute";
            else if (board == "/w/")
                return "Anime/Wallpapers";
            else if (board == "/m/")
                return "Mecha";
            else if (board == "/cgl/")
                return "Cosplay & EGL";
            else if (board == "/cm/")
                return "Cute/Male";
            else if (board == "/f/")
                return "Flash";
            else if (board == "/n/")
                return "Transportation";
            else if (board == "/jp/")
                return "Otaku Culture";

            // Video Games
            else if (board == "/v/")
                return "Video Games";
            else if (board == "/vg/")
                return "Video Game Generals";
            else if (board == "/vp/")
                return "Pokémon";
            else if (board == "/vr/")
                return "Retro Games";

            // Interests
            else if (board == "/co/")
                return "Comics & Cartoons";
            else if (board == "/g/")
                return "Technology";
            else if (board == "/tv/")
                return "Television & Film";
            else if (board == "/k/")
                return "Weapons";
            else if (board == "/o/")
                return "Auto";
            else if (board == "/an/")
                return "Animals & Nature";
            else if (board == "/tg/")
                return "Traditional Games";
            else if (board == "/sp/")
                return "Sports";
            else if (board == "/asp/")
                return "Alternative Sports";
            else if (board == "/sci/")
                return "Science & Math";
            else if (board == "/his/")
                return "History & Humanities";
            else if (board == "/int/")
                return "International";
            else if (board == "/out/")
                return "Outdoors";
            else if (board == "/toy/")
                return "Toys";

            // Creative
            else if (board == "/i/")
                return "Oekaki";
            else if (board == "/po/")
                return "Papercraft & Origami";
            else if (board == "/p/")
                return "Photography";
            else if (board == "/ck/")
                return "Food & Cooking";
            else if (board == "/ic/")
                return "Artwork/Critique";
            else if (board == "/wg/")
                return "Wallpapers/General";
            else if (board == "/lit/")
                return "Literature";
            else if (board == "/mu/")
                return "Music";
            else if (board == "/fa/")
                return "Fashion";
            else if (board == "/3/")
                return "3DCG";
            else if (board == "/gd/")
                return "Graphic Design";
            else if (board == "/diy/")
                return "Do It Yourself";
            else if (board == "/wsg/")
                return "Worksafe GIF";
            else if (board == "/qst/")
                return "Quests";

            // Other
            else if (board == "/biz/")
                return "Business & Finance";
            else if (board == "/trv/")
                return "Travel";
            else if (board == "/fit/")
                return "Fitness";
            else if (board == "/x/")
                return "Paranormal";
            else if (board == "/adv/")
                return "Advice";
            else if (board == "/lgbt/")
                return "Lesbian, Gay, Bisexual, & Transgender";
            else if (board == "/mlp/")
                return "My Little Pony"; // disgusting.
            else if (board == "/news/")
                return "Current News";
            else if (board == "/wsr/")
                return "Worksafe Requests";
            else if (board == "/vip/")
                return "Very Important Posts";

            // Misc
            else if (board == "/b/")
                return "Random";
            else if (board == "/r9k/")
                return "ROBOT9001";
            else if (board == "/pol/")
                return "Politically Incorrect";
            else if (board == "/bant/")
                return "International/Random";
            else if (board == "/soc/")
                return "Cams & Meetups";
            else if (board == "/s4s/")
                return "Sh*t 4chan Says";

            // Adult
            else if (board == "/s/")
                return "Sexy Beautiful Women";
            else if (board == "/hc/")
                return "Hardcore";
            else if (board == "/hm/")
                return "Handsome Men";
            else if (board == "/h/")
                return "Hentai";
            else if (board == "/e/")
                return "Ecchi";
            else if (board == "/u/")
                return "Yuri";
            else if (board == "/d/")
                return "Hentai/Alternative";
            else if (board == "/y/")
                return "Yaoi";
            else if (board == "/t/")
                return "Torrents";
            else if (board == "/hr/")
                return "High Resolution";
            else if (board == "/gif/")
                return "Adult GIF";
            else if (board == "/aco/")
                return "Adult Cartoons";
            else if (board == "/r/")
                return "Adult Requests";

            // Unlisted
            else if (board == "/trash/")
                return "Off-Topic";
            else if (board == "/qa/")
                return "Question & Answer";


            else
                return "";
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

                    Regex ImageMatch;
                    if (!string.IsNullOrEmpty(RegexStrings.Default.u18chanFiles)) {
                        ImageMatch = new Regex(RegexStrings.Default.u18chanFiles);
                    }
                    else {
                        ImageMatch = new Regex("(?<=File: <a href=\").*?(?=\" target=\"_blank\">)");
                    }

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
                    MessageBox.Show(WebEx.ToString());
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            });
        }
        private void BeginUEighteenChanDownlad() {
            DownloadThread.Start();
        }
        public static string GetU18ChanTopic(string board) {
            if (board == "/fur/")
                return "Furries";
            else if (board == "/c/")
                return "Furry Comics";
            else if (board == "/gfur/")
                return "Gay Furries";
            else if (board == "/gc/")
                return "Gay Furry Comics";
            else if (board == "/i/")
                return "Intersex";
            else if (board == "/rs/")
                return "Request & Source";
            else if (board == "/a/")
                return "Animated";
            else if (board == "/cute/")
                return "Cute";

            else if (board == "/pb/")
                return "Post Your Naked Body";
            else if (board == "/p/")
                return "Ponies"; // seriously, fuck this mlp shit
            else if (board == "/f/")
                return "Feral";
            else if (board == "/cub/")
                return "Cub";
            else if (board == "/gore/")
                return "Gore";

            else if (board == "/d/")
                return "Discussion";
            else if (board == "/mu/")
                return "Music";
            else if (board == "/w/")
                return "Wallpapers";
            else if (board == "/v/")
                return "Video Games";
            else if (board == "/lo/")
                return "Lounge";
            else if (board == "/tech/")
                return "Technology";
            else if (board == "/lit/")
                return "Literature";

            else
                return string.Empty;
        }
        #endregion

        private void btnForce404_Click(object sender, EventArgs e) {
            ThreadHas404 = true;
            AfterDownload();
        }

    }
}