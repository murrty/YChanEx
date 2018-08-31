// 4chan.net
// Supports IsModifiedSince

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
    class fourChan : ImageBoard {
        public static string regThread  = "boards.4chan.org/[a-zA-Z0-9]*?/thread/[0-9]*";
        public static string regBoard   = "boards.4chan.org/[a-zA-Z0-9]*?/$";
        public static string regTitle   = "(?<=<title>).*?(?= - 4chan</title>)";
        public static string regOSS     = @"(?=<script type=""text/javascript"">\(function\(\){var).*?(?=</script>)";

        public fourChan(string url, bool isBoard) : base(url, isBoard) {
            this.Board     = isBoard;
            this.imName    = "4chan";
            if(!isBoard) {
                Match match = Regex.Match(url, @"boards.4chan.org/[a-zA-Z0-9]*?/thread/\d*");
                this.URL = "https://" + match.Groups[0].Value;
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName+ "\\" + getURL().Split('/')[3] + "\\" + getURL().Split('/')[5];
            }
            else {
                this.URL = url;
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3];
            }
            this.checkedAt = DateTime.Now.AddYears(-20);
            this.fileCount = 0;
        }

        public new static bool isThread(string url) { 
            Regex urlMatcher = new Regex(regThread);
            if(urlMatcher.IsMatch(url))
                return true;
            else
                return false;
        }
        public new static bool isBoard(string url) { 
            //Regex urlMatcher = new Regex(regBoard);
            //if(urlMatcher.IsMatch(url))
            //    return true;
            //else
            return false;
        }

        override public void download() {
            List<string> downloadURLs = new List<string>();                                                                     // List of images
            List<string> downloadThumbs = new List<string>();                                                                   // List of thumbnails
            string baseURL = "//i.4cdn.org/" + getURL().Split('/')[3] + "/";                                                    // Base URL used for downloading & html
            string threadSrc;                                                                                                   // String that contains the source of the thread
            string currentURL = string.Empty;                                                                                   // String for deciding which URL is being used
            string jsonURL = "https://a.4cdn.org/" + getURL().Split('/')[3] + "/thread/" + getURL().Split('/')[5] + ".json";    // API url of current thread

            try {
                currentURL = jsonURL;

            // Get JSON with IfModifiedSince status, dispose afterwards
                HttpWebRequest requestJSON = (HttpWebRequest)WebRequest.Create(jsonURL);
                requestJSON.UserAgent = Adv.Default.UserAgent;
                requestJSON.IfModifiedSince = this.checkedAt;
                requestJSON.Method = "GET";
                var reqResponse = (HttpWebResponse)requestJSON.GetResponse();
                var responseStream = reqResponse.GetResponseStream();
                string str = string.Empty;
                using (StreamReader strReader = new StreamReader(responseStream)) {
                    string json = strReader.ReadToEnd();
                    byte[] jBytes = Encoding.ASCII.GetBytes(json);
                    using (var memStream = new MemoryStream(jBytes)) {
                        var quotas = new XmlDictionaryReaderQuotas();
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(memStream, quotas);
                        var xml = XDocument.Load(jsonReader);
                        memStream.Flush();
                        memStream.Close();
                        if (xml.ToString() == Controller.emptyXML)
                            str = null;
                        else
                            str = xml.ToString();
                    }
                }
                reqResponse.Dispose();
                responseStream.Dispose();

            // Check XML and prepare html source
                if (string.IsNullOrEmpty(str))
                    return;
                currentURL = this.getURL();
                threadSrc = Controller.getHTML(this.getURL());

            // Start creating XmlDocument and get required information.
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(str);
                XmlNodeList xmlTim = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/tim");                // File ID
                XmlNodeList xmlFileName = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/filename");      // Original file name
                XmlNodeList xmlExt = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/ext");                // File extension
                XmlNodeList xmlMd5 = xmlDoc.DocumentElement.SelectNodes("/root/posts/item/md5");                // Base64 MD5 of the file.

            // Count the files and do source maintenance
                for (int i = 0; i < xmlExt.Count; i++) {
                    downloadURLs.Add("https:" + baseURL + xmlTim[i].InnerText + xmlExt[i].InnerText);
                    downloadThumbs.Add("https:" + baseURL + xmlTim[i].InnerText + "s.jpg");

                    string oldURL = baseURL + xmlTim[i].InnerText + xmlExt[i].InnerText;                                        // Old URL of the files
                    string oldThumbURL = "//t.4cdn.org/" + getURL().Split('/')[3] + "/" + xmlTim[i].InnerText + "s.jpg";        // Old URL of the thumbnail
                    string newURL = xmlTim[i].InnerText + xmlExt[i].InnerText;                                                  // New URL of the files
                    threadSrc = threadSrc.Replace(oldURL, newURL);
                    threadSrc = threadSrc.Replace(oldThumbURL, "thumb");
                }

            // Create save directory
                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);
                else {
                    if (YCSettings.Default.htmlDownload && this.fileCount == 0) {
                        this.fileCount = Directory.GetFiles(this.SaveTo, "*", SearchOption.TopDirectoryOnly).Length - 1;
                    }
                    else if (this.fileCount == 0) {
                        this.fileCount = Directory.GetFiles(this.SaveTo, "*", SearchOption.TopDirectoryOnly).Length;
                    }
                }

            // Begin download
                string fileName = string.Empty;
                for (int i = 0; i < downloadURLs.Count; i++) {
                    if (YCSettings.Default.originalName) {
                        currentURL = downloadURLs[i];

                    // Replace illegal characters in file names that aren't allowed on Windows machines
                        string[] invalidCharacters = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                        fileName = xmlFileName[i].InnerText;

                        for (int y = 0; y < invalidCharacters.Length; y++)
                            fileName = fileName.Replace(invalidCharacters[y], "_");

                    // Check for duplicates based on the FileCount
                        if (YCSettings.Default.preventDupes) {
                            //if (File.Exists(this.SaveTo + "\\" + fileName + xmlExt[i].InnerText)) {
                            //    //if ((i + 1) > this.fileCount) { // If the count is greater than current file count, then it's unique.
                            //    if (!thisFileExists(this.SaveTo + "\\" + fileName + xmlExt[i].InnerText, xmlMd5[i].InnerText)) {
                            //        if (!thisFileExists(this.SaveTo + "\\" + fileName + " (" + i + ")" + xmlExt[i].InnerText, xmlMd5[i].InnerText)) {
                            //            fileName += " (" + i + ")" + xmlExt[i].InnerText;
                            //            Controller.downloadFile(downloadURLs[i], this.SaveTo, true, fileName);
                            //            this.fileCount++;
                            //        }
                            //    }
                            //    else {
                            //        continue;
                            //    }
                            //}
                            //else {
                            //    fileName += xmlExt[i].InnerText;
                            //    Controller.downloadFile(downloadURLs[i], this.SaveTo, true, fileName);
                            //    this.fileCount++;
                            //}
                            string fileCount = string.Empty;
                            if (i >= 10) {
                                if (i >= 100) {
                                    fileCount += i.ToString();
                                }
                                else {
                                    fileCount += "0" + i.ToString();
                                }
                            }
                            else {
                                fileCount += "00" + i.ToString();
                            }
                            fileName = "(" + fileCount + ") " + fileName + xmlExt[i].InnerText;

                            if (!File.Exists(this.SaveTo + "\\" + fileName)) {
                                Controller.downloadFile(downloadURLs[i], this.SaveTo, true, fileName);
                                this.fileCount++;
                            }
                        }
                        else {
                            if (!File.Exists(this.SaveTo + "\\" + fileName)) {
                                fileName += xmlExt[i].InnerText;
                                Controller.downloadFile(downloadURLs[i], this.SaveTo, true, fileName);
                                this.fileCount++;
                            }
                        }
                    }
                    else {
                        Controller.downloadFile(downloadURLs[i], this.SaveTo);
                    }
                }

            // Download thumbnails
                if (YCSettings.Default.downloadThumbnails) {
                    for (int i = 0; i < downloadThumbs.Count; i++) {
                        currentURL = downloadThumbs[i];
                        Controller.downloadFile(downloadThumbs[i], this.SaveTo + "\\thumb");
                    }
                }

            // Get rid of Off-site scripts (New bullshit scripts introduced by 4channel
                Regex siteScript = new Regex(regOSS);
                foreach (Match foundScript in siteScript.Matches(threadSrc))
                    threadSrc = threadSrc.Replace(foundScript.ToString(), string.Empty);

            // Save HTML
                if (YCSettings.Default.htmlDownload && threadSrc != string.Empty)
                    Controller.saveHTML(false, threadSrc.Replace("class=\"fileThumb\" href=\"thumb/", "class=\"fileThumb\" href=\""), this.SaveTo);
            }
            catch (ThreadAbortException) {
                return;
            }
            catch (WebException webEx) {
                Debug.Print(webEx.ToString());
                var response = (HttpWebResponse)webEx.Response;
                if (webEx.Status != WebExceptionStatus.ProtocolError || response.StatusCode != HttpStatusCode.NotModified) {
                    if (((int)webEx.Status) == 7)
                        this.Gone = true;
                    else
                        ErrorLog.reportWebError(webEx, currentURL);
                }

                GC.Collect();
                return;
            }
            catch (IOException) {
                // It seems to be in use
            }
            catch (Exception ex) {
                ErrorLog.reportError(ex.ToString());
                GC.Collect();
                return;
            }

            GC.Collect();
        }

        public static bool thisFileExists(string file, string hash) {
            try {
                if (!File.Exists(file)) {
                    return false;
                }
                string output;
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                    using (var stream = File.OpenRead(file)) {
                        var fhash = md5.ComputeHash(stream);
                        Thread.Sleep(100);
                        output = BitConverter.ToString(fhash).Replace("-", string.Empty).ToLowerInvariant();
                    }
                }

                byte[] raw = new byte[16];
                for (int i = 0; i < 32; i += 2)
                    raw[i / 2] = Convert.ToByte(output.Substring(i, 2), 16);
                
                output = Convert.ToBase64String(raw);

                if (output == hash)
                    return true;
                else
                    return false;
            }
            catch (Exception ex) {
                ErrorLog.reportError(ex.ToString());
                return false;
            }
        }

        public static string getTopic(string board) {
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
    }
}
