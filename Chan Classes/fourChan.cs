// 4chan.net

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

namespace YChanEx {
    class fourChan : ImageBoard {
        public static string regThread  = "boards.4chan.org/[a-zA-Z0-9]*?/thread/[0-9]*";
        public static string regBoard   = "boards.4chan.org/[a-zA-Z0-9]*?/$";
        public static string regTitle = "(?<=<title>).*?(?= - 4chan</title>)";

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

        override protected string getLinks() {
            string exed = "";
            string JSONUrl = "http://a.4cdn.org/" + getURL().Split('/')[3] +"/thread/" + getURL().Split('/')[5] +".json";
            string baseURL = "http://i.4cdn.org/" + getURL().Split('/')[3] + "/";
            string str = "";
            XmlNodeList xmlTim;
            XmlNodeList xmlExt;
            XmlNodeList xmlFilename;
            try {
                string Content;
                using(WebClient wc = new WebClient()){
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    Content = wc.DownloadString(JSONUrl);
                }

                byte[] bytes = Encoding.ASCII.GetBytes(Content);
                using(var stream = new MemoryStream(bytes)) {
                    var quotas = new XmlDictionaryReaderQuotas();
                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                    var xml = XDocument.Load(jsonReader);
                    str = xml.ToString();
                    stream.Flush();
                    stream.Close();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                if (getURL().Split('/')[3] == "f")
                    xmlTim = doc.DocumentElement.SelectNodes("/root/posts/item/filename");
                else
                    xmlTim = doc.DocumentElement.SelectNodes("/root/posts/item/tim");

                xmlFilename = doc.DocumentElement.SelectNodes("/root/posts/item/filename");

                xmlExt     = doc.DocumentElement.SelectNodes("/root/posts/item/ext");
                for(int i = 0; i < xmlExt.Count; i++) {
                    exed = exed + baseURL + xmlTim[i].InnerText + xmlExt[i].InnerText + "\n";
                    // MessageBox.Show(exed);
                }

                return exed;
            }
            catch(WebException webEx) {
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.logError(webEx.ToString(), "Fchan.getLinks");
                throw webEx; }
            catch (Exception ex) { ErrorLog.logError(ex.ToString(), "Fchan.getLinks"); throw ex; }
        }
        override public string getThreads() {
            string URL = "http://a.4cdn.org/" + getURL().Split('/')[3] + "/catalog.json";
            string Res = "";
            string str = "";
            XmlNodeList tNa;
            XmlNodeList tNo;
            try {
                string json = new WebClient().DownloadString(URL);
                byte[] bytes = Encoding.ASCII.GetBytes(json);
                using(var stream = new MemoryStream(bytes)) {
                    var quotas = new XmlDictionaryReaderQuotas();
                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                    var xml = XDocument.Load(jsonReader);
                    str = xml.ToString();
                    stream.Flush();
                    stream.Close();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                tNo     = doc.DocumentElement.SelectNodes("/root/item/threads/item/no");
                tNa     = doc.DocumentElement.SelectNodes("/root/item/threads/item/semantic_url");
                for(int i = 0; i < tNo.Count; i++) {
                    Res = Res + "http://boards.4chan.org/" + getURL().Split('/')[3] + "/thread/" + tNo[i].InnerText + "/" + tNa[i].InnerText + "\n";
                }
            }
            catch(WebException webEx) {
                Debug.Print(webEx.ToString());
                ErrorLog.logError(webEx.ToString(), "Fchan.getThreads"); }
            catch (Exception ex) { ErrorLog.logError(ex.ToString(), "Fchan.getThreads"); }

            return Res;
        }

        override public void download() {
            string[] URLs;                                                      // Array of the image URLs
            string[] thumbs;                                                    // Array of the thumbnail URLs
            string strThumbs = "";                                              // ?
            string baseURL = "//i.4cdn.org/" + getURL().Split('/')[3] + "/";    // Base URL used for downloading the files
            string website;                                                     // String that contains the source of the thread

            try {
                website = Controller.getHTML(this.getURL());

                string JURL = "http://a.4cdn.org/" + getURL().Split('/')[3] + "/thread/" + getURL().Split('/')[5] + ".json";
                string str = Controller.getJSON(JURL);
                if (str == "null") {
                    return;
                }

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

                    old = "//t.4cdn.org/" + getURL().Split('/')[3] + "/" + xmlTim[i].InnerText + "s.jpg";
                    strThumbs = strThumbs + "https:" + old + "\n";
                    website = website.Replace("//i.4cdn.org/" + getURL().Split('/')[3], "thumb");
                }

                website = website.Replace("=\"//", "=\"http://");

                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);
                URLs = Regex.Split(getLinks(), "\n");

                for (int y = 0; y < URLs.Length - 1; y++)
                    if (YCSettings.Default.originalName) {
                        string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                        string newfilename = xmlFilename[y].InnerText;

                        if (YCSettings.Default.preventDupes)
                            newfilename = newfilename + " (" + xmlMd5[y].InnerText + ")" + xmlExt[y].InnerText;
                        else
                            newfilename = newfilename + xmlExt[y].InnerText;

                        //if (File.Exists(this.SaveTo + newfilename + xmlExt[y].InnerText)) { 
                            // TODO: Implement MD5 checking
                        //}
                        for (int z = 0; z < badchars.Length - 1; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");
                        
                        Controller.downloadFile(URLs[y], this.SaveTo, true, newfilename);
                        website = website.Replace(URLs[y].Split('/')[4], newfilename + xmlExt[y].InnerText);
                    }
                    else {
                        Controller.downloadFile(URLs[y], this.SaveTo);
                    }

                if (YCSettings.Default.downloadThumbnails) {
                    thumbs = strThumbs.Split('\n');

                    for (int i = 0; i < thumbs.Length - 1; i++)
                        Controller.downloadFile(thumbs[i], this.SaveTo + "\\thumb");
                }

                if (YCSettings.Default.htmlDownload == true && website != "")
                    Controller.saveHTML(false, website.Replace("class=\"fileThumb\" href=\"thumb/", "class=\"fileThumb\" href=\""), this.SaveTo);
            }
            catch (WebException webEx) {
                Debug.Print(webEx.ToString());
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.logError(webEx.ToString(), "Fchan.download");

                GC.Collect();
                return;
            }
            catch (Exception ex) { ErrorLog.logError(ex.ToString(), "Fchan.download"); }

            GC.Collect();
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
