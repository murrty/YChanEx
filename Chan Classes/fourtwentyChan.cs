// 420chan.org

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
using System.Threading;

namespace YChanEx {
    class fourtwentyChan : ImageBoard {
        public static string regThread = "boards.420chan.org/[a-zA-Z0-9]*?/res/[0-9]*";
        public static string regBoard = "boards.420chan.org/[a-zA-Z0-9]*?/$";
        public static string regTitle = "(?<=<title>).*?(?= - 420chan </title>)";

        public fourtwentyChan(string url, bool isBoard) : base(url, isBoard) {
            this.Board = isBoard;
            this.imName = "420chan";
            if (!isBoard) {
                Match match = Regex.Match(url, @"boards.420chan.org/[a-zA-Z0-9]*?/res/\d*");
                this.URL = "https://" + match.Groups[0].Value + ".php";
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3] + "\\" + getURL().Split('/')[5].Replace(".php","");
            }
            else {
                this.URL = url;
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3];
            }
            this.checkedAt = DateTime.Now.AddYears(-20);
        }

        public new static bool isThread(string url) {
            Regex urlMatcher = new Regex(regThread);
            if (urlMatcher.IsMatch(url))
                return true;
            else
                return false;
        }
        public new static bool isBoard(string url) { return false; }

        public bool isModified(string url) {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = Adv.Default.UserAgent;
                request.IfModifiedSince = this.checkedAt;
                request.Method = "HEAD";
                var resp = (HttpWebResponse)request.GetResponse();

                this.checkedAt = resp.LastModified;

                return true;
            }
            catch (WebException webEx) {
                var response = (HttpWebResponse)webEx.Response;
                if (webEx.Status != WebExceptionStatus.ProtocolError || response.StatusCode != HttpStatusCode.NotModified) {
                    Debug.Print("========== WEBERROR OCCURED ==========");
                    Debug.Print("URL: " + url);
                    Debug.Print(webEx.ToString());
                    throw (WebException)webEx;
                }

                return false;
            }
        }

        public override void download() {
            string[] URLs;
            string[] thumbs;
            string strThumbs = "";
            string baseURL = "https://boards.420chan.org/" + getURL().Split('/')[3] + "/src/";
            string thumbURL = "https://boards.420chan.org/" + getURL().Split('/')[3] + "/thumb/";
            string JURL = "https://api.420chan.org/" + getURL().Split('/')[3] + "/res/" + getURL().Split('/')[5].Replace(".php", ".json");
            string website;

            try {
                //if (!isModified(this.getURL())) {
                //    return;
                //}
                string str = Controller.getJSON(JURL);
                website = Controller.getHTML(this.getURL());

                if (string.IsNullOrWhiteSpace(str)) {
                    MessageBox.Show("Thread " + getURL().Split('/')[5] + " may be invald, or your requests may be timing out.");
                    return;
                }
                else if (str == "Not modified") {
                    return;
                }

                this.checkedAt = DateTime.Now;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                XmlNodeList xmlFilename = doc.DocumentElement.SelectNodes("/root/posts/item/filename");
                XmlNodeList xmlExt = doc.DocumentElement.SelectNodes("/root/posts/item/ext");

                website = website.Replace("href=\"/" + this.getURL().Split('/')[3] + "/src/", "");
                website = website.Replace("href=\"/" + this.getURL().Split('/')[3], "");
                website = website.Replace("href=\"/static/", "href=\"https://420chan.org/static/");

                for (int i = 0; i < xmlExt.Count; i++) {
                    if (xmlExt[i].InnerText == ".gif")  website.Replace("href=\"/thumb/" + xmlFilename[i].InnerText + xmlExt[i].InnerText, xmlFilename[i].InnerText + xmlExt[i].InnerText);
                    else strThumbs = strThumbs + thumbURL + xmlFilename[i].InnerText + "s.jpg\n";
                }

                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);

                var list = new List<String>();
                for (int j = 0; j < xmlFilename.Count; j++) {
                    list.Add(baseURL + xmlFilename[j].InnerText + xmlExt[j].InnerText);
                }

                URLs = list.ToArray();
                list.Clear();

                for (int y = 0; y < URLs.Length; y++) {
                    // 420chan doesnt support original file names.
                    Controller.downloadFile(URLs[y], this.SaveTo);
                    website = website.Replace(URLs[y], "");
                }

                if (YCSettings.Default.downloadThumbnails) {
                    thumbs = strThumbs.Split('\n');

                    for (int i = 0; i < thumbs.Length - 1; i++)
                        Controller.downloadFile(thumbs[i], this.SaveTo + "\\thumb");
                }

                if (YCSettings.Default.htmlDownload)
                    Controller.saveHTML(false, website, this.SaveTo);

            }
            catch (ThreadAbortException) {
                return;
            }
            catch (WebException webEx) {
                Debug.Print(webEx.ToString());
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.reportWebError(webEx);

                GC.Collect();
                return;
            }
            catch (Exception ex) {
                ErrorLog.reportError(ex.ToString());
            }
        }

        public static string getTopic(string board) {
            // Drugs
            if (board == "/weed/")
                return "Cannabis Discussion";
            else if (board == "/drank/")
                return "Alcohol Discussion";
            else if (board == "/mdma/")
                return "Ecstasy Discussion";
            else if (board == "/psy/")
                return "Psychedelic Discussion";
            else if (board == "/stim/")
                return "Stimulant Discussion";
            else if (board == "/dis/")
                return "Dissociative Discussion";
            else if (board == "/opi/")
                return "Opiate Discussion";
            else if (board == "/vape/")
                return "Vaping Discussion";
            else if (board == "/tobacco/")
                return "Tobacco Discussion";
            else if (board == "/benz/")
                return "Benzo Discussion";
            else if (board == "/deli/")
                return "Deliriant Discussion";
            else if (board == "/other/")
                return "Other Drugs";
            else if (board == "/jenk/")
                return "Jenkem Discussion";
            else if (board == "/detox/")
                return "Detox";

            // Lifestyle



            return "";
        }
    }
}
