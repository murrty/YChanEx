using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Diagnostics;


namespace YChanEx
{
    class InfiniteChan : ImageBoard {
        public static string regThread = "8ch.net/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";  // Regex to check whether is Thread or not
        public static string regBoard = "8ch.net/[a-zA-Z0-9]*?/";                    // Regex to check whether is Board or not

        public InfiniteChan(string url, bool isBoard) : base(url, isBoard) {
            this.Board = isBoard;
            this.imName = "8ch";
            if (!isBoard) {
                Match match = Regex.Match(url, @"8ch.net/[a-zA-Z0-9]*?/res/[0-9]*");
                this.URL = "https://" + match.Groups[0].Value + ".html";      // simplify thread url
                this.SaveTo = (YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3] + "\\" + getURL().Split('/')[5]).Replace(".html", "");
            }
            else {
                this.URL = url;
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3];
            }

        }

        public new static bool isThread(string url) {
            Regex urlMatcher = new Regex(regThread);
            if (urlMatcher.IsMatch(url))
                return true;
            else
                return false;
        }
        public new static bool isBoard(string url) {
            Regex urlMatcher = new Regex(regBoard);
            if (urlMatcher.IsMatch(url))
                return true;
            else
                return false;
        }

        override protected string getLinks() {
            string exed = "";
            string JSONUrl = ("http://8ch.net/" + getURL().Split('/')[3] + "/res/" + getURL().Split('/')[5] + ".json").Replace(".html", "");
            string str = "";
            XmlNodeList xmlTim;
            XmlNodeList xmlFilename;
            XmlNodeList xmlExt;
            try {
                string Content = new WebClient().DownloadString(JSONUrl);

                byte[] bytes = Encoding.ASCII.GetBytes(Content);
                using (var stream = new MemoryStream(bytes)) {
                    var quotas = new XmlDictionaryReaderQuotas();
                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                    var xml = XDocument.Load(jsonReader);
                    str = xml.ToString();
                    stream.Flush();
                    stream.Close();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                xmlTim = doc.DocumentElement.SelectNodes("/root/posts/item/tim");
                xmlFilename = doc.DocumentElement.SelectNodes("/root/posts/item/filename");
                xmlExt = doc.DocumentElement.SelectNodes("/root/posts/item/ext");

                for (int i = 0; i < xmlExt.Count; i++) {
                    exed = exed + "https://8ch.net/file_store/" + xmlTim[i].InnerText + xmlExt[i].InnerText + "\n";
                }

                xmlTim = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/tim");
                xmlFilename = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/filename");
                xmlExt = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/ext");
                for (int i = 0; i < xmlExt.Count; i++) {
                    exed = exed + "https://8ch.net/file_store/" + xmlTim[i].InnerText + xmlExt[i].InnerText + "\n";
                }


            }
            catch (WebException webEx) {
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.logError(webEx.ToString(), "8chanDownloadError");
                throw webEx;
            }

            return exed;
        }
        override public string getThreads() {
            string URL = "http://8ch.net/" + getURL().Split('/')[3] + "/catalog.json";
            string Res = "";
            string str = "";
            XmlNodeList tNo;
            try {
                string json = new WebClient().DownloadString(URL);
                byte[] bytes = Encoding.ASCII.GetBytes(json);
                using (var stream = new MemoryStream(bytes)) {
                    var quotas = new XmlDictionaryReaderQuotas();
                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                    var xml = XDocument.Load(jsonReader);
                    str = xml.ToString();
                    stream.Flush();
                    stream.Close();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                tNo = doc.DocumentElement.SelectNodes("/root/item/threads/item/no");
                for (int i = 0; i < tNo.Count; i++) {
                    Res = Res + "http://8ch.net/" + getURL().Split('/')[3] + "/res/" + tNo[i].InnerText + ".html\n";
                }
            }
            catch (WebException webEx) { ErrorLog.logError(webEx.ToString(), "8chanGetThreadError"); }
            catch (Exception ex) { ErrorLog.logError(ex.ToString(), "8chanGetThreadError"); }
            return Res;
        }

        override public void download() {
            if (!Directory.Exists(this.SaveTo))
                        Directory.CreateDirectory(this.SaveTo);

            string[] thumbs;
            string[] thumbsExt;
            string strThumbs = "";
            string strThumbsExt = "";
            string website = "";
            string str;
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                website = wc.DownloadString(this.getURL());
                string JURL = this.getURL().Replace(".html", ".json");
                string Content = wc.DownloadString(JURL);
                byte[] bytes = Encoding.ASCII.GetBytes(Content);
                using (var stream = new MemoryStream(bytes)) {
                    var quotas = new XmlDictionaryReaderQuotas();
                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                    var xml = XDocument.Load(jsonReader);
                    str = xml.ToString();
                    stream.Flush();
                    stream.Close();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);

                // get single images
                XmlNodeList xmlTim = doc.DocumentElement.SelectNodes("/root/posts/item/tim");
                XmlNodeList xmlFilename = doc.DocumentElement.SelectNodes("/root/posts/item/filename");
                XmlNodeList xmlExt = doc.DocumentElement.SelectNodes("/root/posts/item/ext");
                XmlNodeList xmlMd5 = doc.DocumentElement.SelectNodes("/root/posts/item/md5");
                for (int i = 0; i < xmlExt.Count; i++) {
                    string tim = xmlTim[i].InnerText;
                    string filename = xmlFilename[i].InnerText;
                    string ext = xmlExt[i].InnerText;
                    string md5 = xmlMd5[i].InnerText;

                    strThumbs = strThumbs + "https://8ch.net/file_store/thumb/" + tim + ext + "\n";
                    website = website.Replace("https://8ch.net/file_store/thumb/" + tim + ext, "thumb/" + tim + ext);
                    website = website.Replace("=\"/file_store/thumb/" + tim + ext, "=\"thumb/" + tim + ext);
                    website = website.Replace("https://8ch.net/file_store/thumb/" + tim + ext, "thumb/" + tim + ext);
                    website = website.Replace("https://media.8ch.net/file_store/thumb/" + tim + ext, "thumb/" + tim + ext);

                    if (YCSettings.Default.originalName) {
                        website = website.Replace("=\"/file_store/" + tim + ext, "=\"" + filename + ext);
                        website = website.Replace("https://media.8ch.net/file_store/" + tim + ext, filename + ext);
                        website = website.Replace("https://8ch.net/file_store/" + tim + ext, filename + ext);
                    }
                    else {
                        website = website.Replace("=\"/file_store/" + tim + ext, "=\"" + tim + ext);
                        website = website.Replace("https://media.8ch.net/file_store/" + tim + ext, tim + ext);
                        website = website.Replace("https://8ch.net/file_store/" + tim + ext, tim + ext);
                    }

                    // Attempt download
                    string dlURL = "https://8ch.net/file_store/" + tim + ext;
                    if (YCSettings.Default.originalName) {
                        string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                        string newfilename = filename + " (" + md5 + ")" + ext;
                        for (int z = 0; z < badchars.Length - 1; z++) {
                            newfilename = newfilename.Replace(badchars[z], "-");
                        }
                        FileController.downloadFile(dlURL, this.SaveTo, true, newfilename);
                    }
                    else {
                        FileController.downloadFile(dlURL, this.SaveTo, false, string.Empty);
                    }

                    if (YCSettings.Default.downloadThumbnails) {
                        thumbs = strThumbs.Split('\n');

                        FileController.downloadFile(thumbs[i], this.SaveTo + "\\thumb", false, string.Empty);
                    }
                }

                // get images of posts with multiple images
                XmlNodeList xmlTimExt = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/tim");
                XmlNodeList xmlFilenameExt = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/filename");
                XmlNodeList xmlExtExt = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/ext");
                XmlNodeList xmlMd5Ext = doc.DocumentElement.SelectNodes("/root/posts/item/extra_files/item/md5");
                for (int i = 0; i < xmlExtExt.Count; i++) {
                    string tim = xmlTimExt[i].InnerText;
                    string filename = xmlFilenameExt[i].InnerText;
                    string ext = xmlExtExt[i].InnerText;
                    string md5 = xmlMd5Ext[i].InnerText;

                    strThumbsExt = strThumbsExt + "https://8ch.net/file_store/thumb/" + tim + ext + "\n";
                    website = website.Replace("https://8ch.net/file_store/thumb/" + tim + ext, "thumb/" + tim + ext);
                    website = website.Replace("=\"/file_store/thumb/" + tim + ext, "=\"thumb/" + tim + ext);
                    website = website.Replace("https://8ch.net/file_store/thumb/" + tim + ext, "thumb/" + tim + ext);
                    website = website.Replace("https://media.8ch.net/file_store/thumb/" + tim + ext, "thumb/" + tim + ext);

                    if (YCSettings.Default.originalName) {
                        website = website.Replace("=\"/file_store/" + tim + ext, "=\"" + filename + ext);
                        website = website.Replace("https://media.8ch.net/file_store/" + tim + ext, filename + ext);
                        website = website.Replace("https://8ch.net/file_store/" + tim + ext, filename + ext);
                    }
                    else {
                        website = website.Replace("=\"/file_store/" + tim + ext, "=\"" + tim + ext);
                        website = website.Replace("https://media.8ch.net/file_store/" + tim + ext, tim + ext);
                        website = website.Replace("https://8ch.net/file_store/" + tim + ext, tim + ext);
                    }

                    // Attempt download
                    string dlURL = "https://8ch.net/file_store/" + tim + ext;
                    if (YCSettings.Default.originalName) {
                        string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                        string newfilename = filename + " (" + md5 + ")" + ext;
                        for (int z = 0; z < badchars.Length - 1; z++) {
                            newfilename = newfilename.Replace(badchars[z], "-");
                        }
                        FileController.downloadFile(dlURL, this.SaveTo, true, newfilename);
                    }
                    else {
                        FileController.downloadFile(dlURL, this.SaveTo, false, string.Empty);
                    }

                    if (YCSettings.Default.downloadThumbnails) {
                        thumbsExt = strThumbsExt.Split('\n');
                        FileController.downloadFile(thumbsExt[i], this.SaveTo + "\\thumb", false, string.Empty);
                    }
                }

                website = website.Replace("=\"/", "=\"https://8ch.net/");

                if (YCSettings.Default.htmlDownload)
                    FileController.downloadHTML(false, website, this.SaveTo);
            } catch (WebException webEx) {
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.logError(webEx.ToString(), "8chanDownloadError");

                Debug.Print(webEx.ToString());
                GC.Collect();
                return;
            } catch (Exception ex) { ErrorLog.logError(ex.ToString(), "8chanDownloadError"); }

            GC.Collect();
        }
    }
}
