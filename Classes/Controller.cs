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
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace YChanEx {
    class Controller {

        /* Chan int guide (used to determine what board to work with):
         * 0 = 4chan
         * 1 = 420chan
         * 2 = 7chan
         * 3 = 8chan / 8kun
         * 4 = fchan
         * 5 = u18chan
        */

        /// <summary>
        /// The directory where the settings is stored (AppData/Local)
        /// </summary>
        public static readonly string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";
        /// <summary>
        /// Empty XML template. Not used for anything other than detecting pages that are empty.
        /// </summary>
        public static readonly string emptyXML = "<root type=\"array\"></root>";

        /// <summary>
        /// Load the saved urls to resume downloading.
        /// </summary>
        /// <param name="board">Load the boards from the board list.</param>
        /// <returns></returns>
        public static string loadURLs() {//(bool board) {
            //board = false;
            //if (board && File.Exists(settingsDir + "\\boards.dat"))
            //    return File.ReadAllText(settingsDir + "\\boards.dat").TrimEnd('\n');
            if (File.Exists(settingsDir + "\\threads.dat"))
                return File.ReadAllText(settingsDir + "\\threads.dat").TrimEnd('\n');
            else
                return "";
        }
        /// <summary>
        /// Saves the urls being downloaded to be resumed when opened next.
        /// </summary>
        /// <param name="Boards">The list that contains the boards.</param>
        /// <param name="Threads">The list that contains the threads.</param>
        public static void saveURLs(List<ImageBoard> Threads) { //, List<ImageBoard> Threads = null) {
            try {
                string Buffer = string.Empty;
                //for (int i = 0; i < Boards.Count; i++)
                //    Buffer = Buffer + Boards[i].getURL() + "\n";

                //MessageBox.Show(Buffer);
                //if (!File.Exists(settingsDir + "\\boards.dat"))
                //    File.Create(settingsDir + "\\boards.dat").Dispose();
                //File.WriteAllText(settingsDir + "\\boards.dat", Buffer);

                //Buffer = string.Empty;

                for (int i = 0; i < Threads.Count; i++)
                    Buffer = Buffer + Threads[i].getURL() + "\n";

                Buffer = Buffer.TrimEnd('\n');
                File.WriteAllText(settingsDir + "\\threads.dat", Buffer);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }
        public static bool installProtocol() {
            string directory = string.Empty;
            string filename = AppDomain.CurrentDomain.FriendlyName;
            switch (MessageBox.Show("Would you like to specifiy a location to store YChanEx? Select no to use current directory.\n\nThis is required for the plugin to work properly, and is recommended that you select a place that won't be messed with AND have permission to write files to.", "YChanEx", MessageBoxButtons.YesNoCancel)) {
                case DialogResult.Yes:
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select a directory to store YChanEx.exe", SelectedPath = Environment.CurrentDirectory, ShowNewFolderButton = true }) {
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                            directory = fbd.SelectedPath;
                            if (File.Exists(directory + "\\YChanEx.exe"))
                                File.Delete(directory + "\\YChanEx.exe");

                            File.Copy(Environment.CurrentDirectory + "\\" + filename, directory + "\\YChanEx.exe");
                        }
                        else {
                            return false;
                        }
                    }
                    break;
                case DialogResult.No:
                    directory = Environment.CurrentDirectory;
                    break;
                case DialogResult.Cancel:
                    return false;
            }

            Registry.ClassesRoot.CreateSubKey("ychanex");
            RegistryKey setIdentifier = Registry.ClassesRoot.OpenSubKey("ychanex", true);
            setIdentifier.SetValue("URL Protocol", "");
            Registry.ClassesRoot.CreateSubKey("ychanex\\shell");
            Registry.ClassesRoot.CreateSubKey("ychanex\\shell\\open");
            Registry.ClassesRoot.CreateSubKey("ychanex\\shell\\open\\command");
            RegistryKey setProtocol = Registry.ClassesRoot.OpenSubKey("ychanex\\shell\\open\\command", true);
            setProtocol.SetValue("", "\"" + directory + "\\YChanEx.exe\" \"%1\"");
            Registry.ClassesRoot.CreateSubKey("ychanex\\DefaultIcon");
            RegistryKey setIcon = Registry.ClassesRoot.OpenSubKey("ychanex\\DefaultIcon", true);
            setIcon.SetValue("", "\"" + directory + "\\YChanEx.exe\",1");

            if (MessageBox.Show("Protocol information set. Would you like to install the plugin? Requires Greasemonkey/Tampermonkey add-on for your browser.", "YChanEx", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                Process.Start("https://github.com/murrty/YChanEx/raw/master/Plugin/YChanEx.user.js");
            }

            Process.Start(directory + "\\YChanEx.exe");
            Environment.Exit(0);
            return true;
        }

        public const int HWND_YCXBROADCAST = 0xffff;
        public static readonly int WM_ADDYCXDOWNLOAD = RegisterWindowMessage("WM_ADDYCXDOWNLOAD");
        public static readonly int WM_SHOWYCXFORM = RegisterWindowMessage("WM_SHOWYCXFORM");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, bool download = false, string url = null);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
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
            //        if (fourChan.isBoard(url))
            //            return new fourChan(url, board);
            //        else if (infiniteChan.isBoard(url))
            //            return new infiniteChan(url, board);
            //    }
            return null;
        }

        /// <summary>
        /// Checks the URL if it is supported for downloading.
        /// </summary>
        /// <param name="URL">The URL that will be checked.</param>
        /// <returns></returns>
        public static bool isSupported(string URL) {
            URL = URL.Replace("http://", "https://").Replace("https://www.","");

            if (URL.StartsWith("https://4chan.org/")) return true;
            else if (URL.StartsWith("https://4chan.org/")) return true;
            else if (URL.StartsWith("https://boards.4chan.org/")) return true;
            else if (URL.StartsWith("https://boards.420chan.org")) return true;
            else if (URL.StartsWith("https://7chan.org")) return true;
            else if (URL.StartsWith("https://7chan.org")) return true;
            else if (URL.StartsWith("https://8kun.top/")) return true;
            else if (URL.StartsWith("https://8kun.top/")) return true;
            else if (URL.StartsWith("http://fchan.us")) return true;
            else if (URL.StartsWith("http://fchan.us")) return true;
            else if (URL.StartsWith("https://u18chan.com/")) return true;
            else if (URL.StartsWith("https://u18chan.com/")) return true;
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
            MatchCollection matchLine;

            using (WebClient wc = new WebClient()) {
                wc.Headers.Add("user-agent", Adv.Default.UserAgent);

                if (requireCookie)
                    wc.Headers.Add(HttpRequestHeader.Cookie, cookie);

                string tempSource = wc.DownloadString(url);
                matchLine = findTitle.Matches(tempSource);
            }

            if (chan == 0) {
                boardTopic = fourChan.getTopic("/" + url.Split('/')[3] + "/");
                threadBoard = url.Split('/')[3];
                threadTitle = matchLine[0].Value.Split('-')[1].TrimStart(' ').TrimEnd(' ');
            }
            //if (chan == 1)
            //    boardTopic = "";
            //if (chan == 2)
            //    boardTopic = sevenChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; // Redundant for 7chan
            //if (chan == 4)
            //    boardTopic = fChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; findTitle = new Regex(fChan.regTitle);
            //if (chan == 5)
            //    boardTopic = uEighteenChan.getTopic("/" + url.Split('/')[3] + "/"); threadBoard = url.Split('/')[3]; // Redundant for u18chan.


            if (threadTitle.Length > -1)
                return threadTitle;
            else
                return "null";
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
                        if (xml.ToString() == emptyXML)
                            return null;
                        else
                            return xml.ToString();
                    }
                }
            }
            catch (WebException WebE) {
                Debug.Print("========== WEBERROR OCCURED ==========");
                Debug.Print("URL: " + url);
                throw WebE;
            }
            catch (Exception ex) {
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
                    string ext = name.Substring(name.Length - 4);

                    if (!ext.StartsWith(".")) {
                        ext = "." + ext;
                    }

                    if (name.Length > 100) {
                        string oldName = name;
                        name = name.Substring(0, 100) + ext;
                        if (!File.Exists(dir + "\\" + name + ".txt"))
                            File.WriteAllText(dir + "\\" + name + ".txt", "Original name: " + oldName + "\n\n(This file was created from a file name that exceeded 100 characters. You may need to move the file to a new directory to keep the full original name, as even NTFS only supports files up to 256 characters (including the drive and path).");
                    }

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
                Debug.Print("========== WEBERROR OCCURED ==========");
                Debug.Print("URL: " + url);
                var resp = WebE.Response as HttpWebResponse;
                int respID = (int)resp.StatusCode;
                if (resp != null) {
                    if (respID == 404) {
                        Debug.Print("========== 404 ITEM!!! SKIPPING ==========");
                        if (!Adv.Default.disableErrors) 
                            MessageBox.Show("File " + url + " has returned 404. Manual download required.");
                    }
                }
                return false;
            }
            catch (Exception ex) {
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
                Debug.Print("========== WEBERROR OCCURED ==========");
                Debug.Print("URL: " + dlStr);
                throw WebE;
            }
            catch (Exception ex) {
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
                channame = "8kun";
            else if (chan == 4)
                channame = "fchan";
            else if (chan == 5)
                channame = "u18chan";

            if (YCSettings.Default.saveDate)
                dateNow = DateTime.Now.ToString("(yyyy-MM-dd, HH:mm:ss) ");

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