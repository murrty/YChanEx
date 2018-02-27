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
            List<string> urls = new List<string>();
            List<string> thumbs = new List<string>();
            string baseURL = "//i.4cdn.org/" + getURL().Split('/')[3] + "/";    // Base URL used for downloading the files
            string website;                                                     // String that contains the source of the thread
            string curl = "Not defined";
            string JURL = "https://a.4cdn.org/" + getURL().Split('/')[3] + "/thread/" + getURL().Split('/')[5] + ".json";

            try {
                curl = JURL;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(JURL);
                request.UserAgent = Adv.Default.UserAgent;
                request.IfModifiedSince = this.checkedAt;
                request.Method = "GET";
                var resp = (HttpWebResponse)request.GetResponse();
                this.checkedAt = resp.LastModified;
                var respstream = resp.GetResponseStream();
                string str = string.Empty;
                using (StreamReader reader = new StreamReader(respstream)) {
                    string json = reader.ReadToEnd();
                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    using (var stream = new MemoryStream(bytes)) {
                        var quotas = new XmlDictionaryReaderQuotas();
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                        var xml = XDocument.Load(jsonReader);
                        stream.Flush();
                        stream.Close();
                        if (xml.ToString() == Controller.emptyXML)
                            str = null;
                        else
                            str =  xml.ToString();
                    }
                }
                resp.Dispose();
                respstream.Dispose();

                if (string.IsNullOrEmpty(str))
                    return;
                curl = this.getURL();
                website = Controller.getHTML(this.getURL());

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                XmlNodeList xmlTim = doc.DocumentElement.SelectNodes("/root/posts/item/tim");
                XmlNodeList xmlFilename = doc.DocumentElement.SelectNodes("/root/posts/item/filename");
                XmlNodeList xmlExt = doc.DocumentElement.SelectNodes("/root/posts/item/ext");
                XmlNodeList xmlMd5 = doc.DocumentElement.SelectNodes("/root/posts/item/md5");

                for (int i = 0; i < xmlExt.Count; i++) {
                    string old = baseURL + xmlTim[i].InnerText + xmlExt[i].InnerText;
                    string rep = xmlTim[i].InnerText + xmlExt[i].InnerText;
                    website = website.Replace(old, rep);

                    urls.Add("https:" + baseURL + xmlTim[i].InnerText + xmlExt[i].InnerText);
                    thumbs.Add("https:" + baseURL + xmlTim[i].InnerText + "s.jpg");

                    old = "//t.4cdn.org/" + getURL().Split('/')[3] + "/" + xmlTim[i].InnerText + "s.jpg";
                    website = website.Replace("//i.4cdn.org/" + getURL().Split('/')[3], "thumb");
                }

                website = website.Replace("=\"//", "=\"http://");

                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);

                string newfilename = string.Empty;

                for (int y = 0; y < urls.Count; y++) {
                    if (YCSettings.Default.originalName) {
                        curl = urls[y];
                        string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                        newfilename = xmlFilename[y].InnerText;

                        for (int z = 0; z < badchars.Length - 1; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");

                        if (YCSettings.Default.preventDupes) {
                            if (File.Exists(this.SaveTo + "\\" + newfilename + xmlExt[y].InnerText)) {
                                if (!thisFileExists(this.SaveTo + "\\" + newfilename + xmlExt[y].InnerText, xmlMd5[y].InnerText)) {
                                    if (!thisFileExists(this.SaveTo + "\\" + newfilename + " (" + y + ")" + xmlExt[y].InnerText, xmlMd5[y].InnerText)) {
                                        newfilename += " (" + y.ToString() + ")" + xmlExt[y].InnerText;
                                        Controller.downloadFile(urls[y], this.SaveTo, true, newfilename);
                                    }
                                }
                            }
                            else {
                                newfilename += xmlExt[y].InnerText;
                                Controller.downloadFile(urls[y], this.SaveTo, true, newfilename);
                            }
                        }
                        else {
                            newfilename += xmlExt[y].InnerText;
                            Controller.downloadFile(urls[y], this.SaveTo, true, newfilename);
                        }

                        website = website.Replace(xmlTim[y].InnerText + xmlExt[y].InnerText, newfilename);
                    }
                    else {
                        Controller.downloadFile(urls[y], this.SaveTo);
                    }
                }


                if (YCSettings.Default.downloadThumbnails) {
                    for (int i = 0; i < thumbs.Count; i++) {
                        curl = thumbs[i];
                        Controller.downloadFile(thumbs[i], this.SaveTo + "\\thumb");
                    }
                }

                Regex siteScript = new Regex(regOSS);
                foreach (Match script in siteScript.Matches(website))
                    website = website.Replace(script.ToString(), string.Empty);

                if (YCSettings.Default.htmlDownload == true && website != "") {
                    Controller.saveHTML(false, website.Replace("class=\"fileThumb\" href=\"thumb/", "class=\"fileThumb\" href=\""), this.SaveTo);
                }
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
                        ErrorLog.reportWebError(webEx, curl);
                }

                GC.Collect();
                return;
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
