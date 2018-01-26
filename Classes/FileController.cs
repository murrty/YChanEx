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

namespace YChanEx {
    class FileController {

        static string settingsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";

        public static string loadURLs(bool board) {
            if (board && File.Exists(settingsDir + "\\boards.dat"))
                return File.ReadAllText(settingsDir + "\\boards.dat").Replace("http://", "https://");
            else if (!board && File.Exists(settingsDir + "\\threads.dat"))
                return File.ReadAllText(settingsDir + "\\threads.dat").Replace("http://", "https://");
            else
                return "";
        }
        public static void saveURLs(List<ImageBoard> Boards, List<ImageBoard> Threads) {
            string Buffer = "";
            for (int i = 0; i < Boards.Count; i++)
                Buffer = Buffer + Boards[i].getURL().Replace("http://", "https://") + "\n";
            if (!File.Exists(settingsDir + "\\boards.dat"))
                File.Create(settingsDir + "\\boards.dat").Dispose();
            File.WriteAllText(settingsDir + "\\boards.dat", Buffer);

            Buffer = "";

            for (int i = 0; i < Threads.Count; i++)
                Buffer = Buffer + Threads[i].getURL().Replace("http://", "https://") + "\n";
            if (!File.Exists(settingsDir + "\\threads.dat"))
                File.Create(settingsDir + "\\threads.dat").Dispose();
            File.WriteAllText(settingsDir + "\\threads.dat", Buffer);
        }

        public static ImageBoard createNewIMB(string url, bool board) {
            if (!board){
                if (Fchan.isThread(url))
                    return new Fchan(url, board);
                else if (InfiniteChan.isThread(url))
                    return new InfiniteChan(url, board);
                else if (U18Chan.isThread(url)) {
                    if (U18Chan.isNotBlacklisted(url)) // Check for blacklist before allowing download.
                        return new U18Chan(url, false);}
                else if (Schan.isThread(url)) { 
                    return new Schan(url, board);}
            } else {
                    if (Fchan.isBoard(url))
                        return new Fchan(url, board);
                    else if (InfiniteChan.isBoard(url))
                        return new InfiniteChan(url, board);
                }
            return null;
        }

        public static bool isSupported(string URL) {
            if (URL.StartsWith("https://4chan.org/")) return true;
            else if (URL.StartsWith("https://www.4chan.org/")) return true;
            else if (URL.StartsWith("https://boards.4chan.org/")) return true;
            else if (URL.StartsWith("https://8ch.net/")) return true;
            else if (URL.StartsWith("https://www.8ch.net/")) return true;
            else if (URL.StartsWith("https://u18chan.com/")) return true;
            else if (URL.StartsWith("https://www.u18chan.com/")) return true;
            else if (URL.StartsWith("https://7chan.org")) return true;
            else if (URL.StartsWith("https://www.7chan.org")) return true;
            else return false;
        }

        private static string GetFileName(string hrefLink) {
            string[] parts = hrefLink.Split('/');
            string fileName = "";

            if (parts.Length > 0)
                fileName = parts[parts.Length - 1];
            else
                fileName = hrefLink;

            return fileName;
        }
        
        public static bool downloadFile(string url, string dir, bool orig, string name) {

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);

                if (orig) {
                    dir = dir + "\\" + name;

                    if (!File.Exists(dir))
                        wc.DownloadFile(url, dir);
                }
                else {
                    string FN = GetFileName(url);
                    dir = dir + "\\" + FN;

                    if (!File.Exists(dir)) {
                        wc.DownloadFile(url, dir);
                    }
                }
                return true;
            } catch (WebException WebE) {
                Debug.Print(WebE.ToString());
                ErrorLog.logError(WebE.ToString(), "FileControllerDownloadFile");
                return false;
                throw WebE;
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "FileControllerDownloadFile");
                return false;
                throw ex;
            }
        }

        public static bool downloadHTML(bool fromURL, string dlStr, string dir) {
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
            } catch (WebException WebE) {
                Debug.Print(WebE.ToString());
                ErrorLog.logError(WebE.ToString(), "FileControllerDownloadHTML");
                return false;
                throw WebE;
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "FileControllerDownloadHTML");
                return false;
                throw ex;
            }
        }

    }
}
