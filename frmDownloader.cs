using System;
using System.Collections.Generic;
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
        public string ThreadURL = null;
        public string DownloadPath = null;
        public int ChanType = -1;
        public DateTime LastModified = default(DateTime);

        private List<string> ImageFiles = new List<string>();
        private List<string> ThumbnailFiles = new List<string>();
        private List<string> FileNames = new List<string>();
        private List<string> FileHashes = new List<string>();
        private int ImageCount = 0;
        private bool Is404 = false;
        private bool IsAborting = false;
        private int CountDown = 0;
        private string ThreadID = null;

        private Thread DownloadThread = new Thread(() => { });

        public frmDownloader() {
            InitializeComponent();
            this.Icon = Properties.Resources.YChanEx;
        }
        private void frmDownloader_FormClosing(object sender, FormClosingEventArgs e) {
            if (!Is404 && !IsAborting) {
                e.Cancel = true;
                this.Hide();
            }
            else {
                this.Dispose();
            }
        }
        private void btnStopDownload_Click(object sender, EventArgs e) {
            
        }
        private void btnClose_Click(object sender, EventArgs e) {
            if (Is404) {
                this.Dispose();
            }
            else {
                this.Hide();
            }
        }
        private void tmrScan_Tick(object sender, EventArgs e) {
            if (Is404) {
                lbScanTimer.Text = "404'd";
                this.Icon = Properties.Resources.YChanEx404;
                frmMain m = Program.GetMainFormInstance();
                m.Announce404(ThreadURL);
                tmrScan.Stop();
            }
            if (CountDown == 0) {
                StartDownload();
                lbScanTimer.Text = "scanning now...";
                tmrScan.Stop();
            }
            else {
                lbScanTimer.Text = CountDown.ToString();
                CountDown--;
            }
            //ListViewItem lvi = new ListViewItem();
            //lvi.Text = "tesT";
            //lvImages.Items.Add(lvi);
        }

        public void StartDownload() {
            switch (ChanType) {
                case (int)ChanTypes.Types.fourChan:
                    string RegThread = "boards.4chan.org/[a-zA-Z0-9]*?/thread[0-9]*";
                    Match match = Regex.Match(ThreadURL, @"https://boards.4chan.org/[a-zA-Z0-9]*?/thread/\d*");
                    BeginFourChanDownload();
                    break;
            }
        }
        public void StopDownload() {
            if (DownloadThread.IsAlive) {
                IsAborting = true;
                DownloadThread.Abort();
                tmrScan.Stop();
            }
        }
        public void AfterDownload() {
            this.BeginInvoke(new MethodInvoker(() => {
                CountDown = Downloads.Default.ScannerDelay;
                tmrScan.Start();
            }));
        }

        #region 4chan
        private void BeginFourChanDownload() {
            this.Text = "4chan thread - " + ThreadURL.Split('/')[3] + " - " + ThreadURL.Split('/')[5];
            ThreadID = ThreadURL.Split('/')[5];
            DownloadPath += "\\" + ThreadID;
            DownloadThread = new Thread(() => {
                string FileBaseURL = "https://i.4cdn.org/" + ThreadURL.Split('/')[3] + "/";
                string ThreadJSON = null;
                string ThreadHTML = null;
                string CurrentURL = null;

                try {
                    CurrentURL = "https://a.4cdn.org/" + ThreadURL.Split('/')[3] + "/thread/" + ThreadURL.Split('/')[5] + ".json";
                    HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(CurrentURL);
                    Request.UserAgent = Advanced.Default.UserAgent;
                    Request.IfModifiedSince = LastModified;
                    Request.Method = "GET";
                    var Response = (HttpWebResponse)Request.GetResponse();
                    var ResponseStream = Response.GetResponseStream();
                    using (StreamReader ResponseReader = new StreamReader(ResponseStream)) {
                        string json = ResponseReader.ReadToEnd();
                        byte[] JSONBytes = Encoding.ASCII.GetBytes(json);
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

                    for (int i = ImageCount; i < xmlFileID.Count; i++, ImageCount++) {
                        if (xmlFileID[i] == null) {
                            continue;
                        }
                        ImageFiles.Add(FileBaseURL + xmlFileID[i].InnerText + xmlExt[i].InnerText);
                        ThumbnailFiles.Add(FileBaseURL + xmlFileID[i] + "s.jpg");
                        if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                            string FileName = xmlFileName[i].InnerText;
                            for (int j = 0; j < Chans.InvalidFileCharacters.Length; j++) {
                                FileName = FileName.Replace(Chans.InvalidFileCharacters[i], "_");
                            }
                            FileNames.Add(FileName + xmlExt[i].InnerText);
                        }
                        else {
                            FileNames.Add(xmlFileID[i].InnerText + xmlExt[i].InnerText);
                        }
                        FileHashes.Add(xmlHash[i].InnerText);

                        if (YChanEx.Downloads.Default.SaveHTML) {
                            string OldHTMLLinks = string.Empty;
                            if (YChanEx.Downloads.Default.SaveThumbnails) {
                                OldHTMLLinks = "//i.4cdn.org/" + ThreadURL.Split('/')[3] + "/" + xmlFileID[i].InnerText + "s.jpg";
                                ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, "thumb\\" + xmlFileID[i].InnerText + "s.jpg");
                            }

                            if (YChanEx.Downloads.Default.SaveOriginalFilenames) {
                                OldHTMLLinks = "//i.4cdn.org/" + this.ThreadURL.Split('/')[3] + "/" + xmlFileName[i].InnerText;
                            }
                            else {
                                OldHTMLLinks = "//i.4cdn.org/" + this.ThreadURL.Split('/')[3] + "/" + xmlFileID[i].InnerText;
                            }
                            OldHTMLLinks += xmlExt[i].InnerText;
                            ThreadHTML = ThreadHTML.Replace(OldHTMLLinks, xmlFileID[i].InnerText);
                        }

                        ListViewItem lvi = new ListViewItem();
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                        lvi.Name = xmlFileID[i].InnerText;
                        lvi.SubItems[0].Text = xmlFileID[i].InnerText;
                        lvi.SubItems[1].Text = xmlExt[i].InnerText;
                        lvi.SubItems[2].Text = xmlFileName[i].InnerText;
                        lvi.SubItems[3].Text = xmlHash[i].InnerText;
                        this.BeginInvoke(new MethodInvoker(() => {
                            lvImages.Items.Add(lvi);
                        }));
                    }

                    this.BeginInvoke(new MethodInvoker(() => {
                        lbTotal.Text = "number of files: " + ImageCount.ToString();
                        lbLastModified.Text = "last modified: " + LastModified.ToString();
                    }));

                    for (int i = 0; i < ImageFiles.Count; i++) {
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
                            Chans.DownloadFile(CurrentURL, DownloadPath, "");
                        }

                        if (YChanEx.Downloads.Default.SaveHTML) {
                            File.WriteAllText(DownloadPath + "\\Thread.html", ThreadHTML);
                        }
                    }

                    //AfterDownload();
                }
                catch (ThreadAbortException ThreadEx) {
                    return;
                }
                catch (WebException WebEx) {
                    var Response = (HttpWebResponse)WebEx.Response;
                    if (Response.StatusCode == HttpStatusCode.NotModified) {
                        //not modified
                    }
                    else {
                        if (((int)WebEx.Status) == 7) {
                            Is404 = true;
                        }
                        else {
                            //error log
                        }
                    }
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.Print(ex.ToString());
                    //error log
                }
            });
            DownloadThread.Start();
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




    }
}
