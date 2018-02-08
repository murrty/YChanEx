using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace YChanEx {
    class Controller {

        /* Chan int guide (used to determine what board to work with):
         * 0 = 4chan
         * 1 = 420chan
         * 2 = 7chan
         * 3 = 8chan
         * 4 = fchan
         * 5 = u18chan
        */

        static string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";

        public static string loadURLs(bool board) {
            if (board && File.Exists(settingsDir + "\\boards.dat"))
                return File.ReadAllText(settingsDir + "\\boards.dat");
            else if (!board && File.Exists(settingsDir + "\\threads.dat"))
                return File.ReadAllText(settingsDir + "\\threads.dat");
            else
                return "";
        }
        public static void saveURLs(List<ImageBoard> Boards, List<ImageBoard> Threads) {
            string Buffer = "";
            for (int i = 0; i < Boards.Count; i++)
                Buffer = Buffer + Boards[i].getURL() + "\n";
            if (!File.Exists(settingsDir + "\\boards.dat"))
                File.Create(settingsDir + "\\boards.dat").Dispose();
            File.WriteAllText(settingsDir + "\\boards.dat", Buffer);

            Buffer = "";

            for (int i = 0; i < Threads.Count; i++)
                Buffer = Buffer + Threads[i].getURL() + "\n";
            if (!File.Exists(settingsDir + "\\threads.dat"))
                File.Create(settingsDir + "\\threads.dat").Dispose();
            File.WriteAllText(settingsDir + "\\threads.dat", Buffer);
        }

        public static ImageBoard createNewIMB(string url, bool board) {
            if (!board) {
                if (fourChan.isThread(url))
                    return new fourChan(url, board);
                else if (fourtwentyChan.isThread(url))
                    return new fourtwentyChan(url, board);
                else if (sevenChan.isThread(url))
                    return new sevenChan(url, board);
                else if (infiniteChan.isThread(url))
                    return new infiniteChan(url, board);
                else if (fChan.isThread(url))
                    return new fChan(url, board);
                else if (uEighteenChan.isThread(url))
                    if (uEighteenChan.isNotBlacklisted(url))
                        return new uEighteenChan(url, false);
            }
            //} else {
            //        if (Fchan.isBoard(url))
            //            return new Fchan(url, board);
            //        else if (InfiniteChan.isBoard(url))
            //            return new InfiniteChan(url, board);
            //    }
            return null;
        }

        public static bool isSupported(string URL) {
            if (URL.StartsWith("https://4chan.org/")) return true;
            else if (URL.StartsWith("https://www.4chan.org/")) return true;
            else if (URL.StartsWith("https://boards.4chan.org/")) return true;
            else if (URL.StartsWith("https://boards.420chan.org")) return true;
            else if (URL.StartsWith("https://7chan.org")) return true;
            else if (URL.StartsWith("https://www.7chan.org")) return true;
            else if (URL.StartsWith("https://8ch.net/")) return true;
            else if (URL.StartsWith("https://www.8ch.net/")) return true;
            else if (URL.StartsWith("http://fchan.us")) return true;
            else if (URL.StartsWith("http://www.fchan.us")) return true;
            else if (URL.StartsWith("https://u18chan.com/")) return true;
            else if (URL.StartsWith("https://www.u18chan.com/")) return true;
            else return false;
        }

        private static string getFileName(string hrefLink) {
            string[] parts = hrefLink.Split('/');
            string fileName = "";

            if (parts.Length > 0)
                fileName = parts[parts.Length - 1];
            else
                fileName = hrefLink;

            return fileName;
        }

        public static string getHTMLTitle(int chan, string url, bool requireCookie = false, string cookie = "") {
            string threadTitle = "";
            string threadBoard = "";
            string boardTopic = "";
            Regex findTitle = new Regex("(?<=<title>).*?(?=</title>)");

            //if (chan == 0)
            //    boardTopic = fourChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; findTitle = new Regex(fourChan.regTitle);
            //if (chan == 1)
            //    boardTopic = "";
            //if (chan == 2)
            //    boardTopic = sevenChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; // Redundant for 7chan
            //if (chan == 4)
            //    boardTopic = fChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; findTitle = new Regex(fChan.regTitle);
            //if (chan == 5)
            //    boardTopic = uEighteenChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; // Redundant for u18chan.

            using (WebClient wc = new WebClient()) {
                wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);

                if (requireCookie)
                    wc.Headers.Add(HttpRequestHeader.Cookie, cookie);

                string tempSource = wc.DownloadString(url);
                MatchCollection matchLine = findTitle.Matches(tempSource);

                if (matchLine.Count > 0 && matchLine.Count < 1)
                    return matchLine[0].Value;
                else
                    return "null";
            }
        }

        public static string getHTML(string url, bool requireCookie = false, string cookie = "") {
            using (WebClient wc = new WebClient()) {
                wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);

                if (requireCookie)
                    wc.Headers.Add(HttpRequestHeader.Cookie, cookie);

                return wc.DownloadString(url);
            }
        }

        public static string getJSON(string url) {
            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    string json = wc.DownloadString(url);
                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    using (var stream = new MemoryStream(bytes)) {
                        var quotas = new XmlDictionaryReaderQuotas();
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                        var xml = XDocument.Load(jsonReader);
                        stream.Flush();
                        stream.Close();
                        return xml.ToString();
                    }
                }
            }
            catch (WebException WebE) {
                Debug.Print(WebE.ToString());
                ErrorLog.logError(WebE.ToString(), "FileController.downloadJSON");
                return "null";
                throw WebE;
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "FileController.downloadJSON");
                return "null";
                throw ex;
            }
        }

        public static bool downloadFile(string url, string dir, bool orig = false, string name = "", bool requireCookie = false, string cookie = "") {

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);

                if (requireCookie)
                    wc.Headers.Add(HttpRequestHeader.Cookie, cookie);

                if (orig) {
                    dir = dir + "\\" + name;

                    if (!File.Exists(dir))
                        wc.DownloadFile(url, dir);
                }
                else {
                    string FN = getFileName(url);
                    dir = dir + "\\" + FN;

                    if (!File.Exists(dir)) {
                        wc.DownloadFile(url, dir);
                    }
                }
                return true;
            }
            catch (WebException WebE) {
                Debug.Print(WebE.ToString());
                ErrorLog.logError(WebE.ToString(), "FileControllerDownloadFile");
                return false;
                throw WebE;
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "FileControllerDownloadFile");
                return false;
                throw ex;
            }
        }

        public static bool saveHTML(bool fromURL, string dlStr, string dir) {
            // Create a directory (just in case)
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // If the download string, dir, or htmlDownload setting is nothing or set to false then exit
            if (dlStr == "" || dir == "" || YCSettings.Default.htmlDownload == false)
                return false;

            // Check if the dir ends with \Thread.html and add it, if it doesn't.
            if (!dir.EndsWith("\\Thread.html"))
                dir = dir + "\\Thread.html";

            try {
                // Create & write file
                using (FileStream fs = new FileStream(dir, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs)) {
                    if (fromURL) {
                        using (WebClient wc = new WebClient()) {
                            wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                            sw.Write(wc.DownloadString(dlStr));

                            File.WriteAllText(dir, wc.DownloadString(dlStr));
                        }
                    }
                    else {
                        sw.Write(dlStr);
                    }
                }

                return true;
            }
            catch (WebException WebE) {
                Debug.Print(WebE.ToString());
                ErrorLog.logError(WebE.ToString(), "FileControllerDownloadHTML");
                return false;
                throw WebE;
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "FileControllerDownloadHTML");
                return false;
                throw ex;
            }
        }

        public static bool saveHistory(int chan, string historytext, string url) {
            string channame = "unknownchan";
            string dateNow = "";

            if (chan == 0)
                channame = "4chan";
            else if (chan == 1)
                channame = "420chan";
            else if (chan == 2)
                channame = "7chan";
            else if (chan == 3)
                channame = "8chan";
            else if (chan == 4)
                channame = "fchan";
            else if (chan == 5)
                channame = "u18chan";

            if (YCSettings.Default.saveDate)
                dateNow = DateTime.Now.ToString("(yyyy-MM-dd, HH-mm-ss) ");

            if (!File.Exists(settingsDir + @"\" + channame + "history.dat")) {
                File.Create(settingsDir + @"\" + channame + "history.dat").Close();
                File.AppendAllText(settingsDir + @"\" + channame + "history.dat", dateNow + historytext + "\n");
                return true;
            }
            else {
                string readHistory = File.ReadAllText(settingsDir + @"\" + channame + "history.dat");
                if (!readHistory.Contains(url))
                    File.AppendAllText(settingsDir + @"\" + channame + "history.dat", dateNow + historytext + "\n");
                return true;
            }
        }
    }
}