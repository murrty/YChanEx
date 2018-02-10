// u18chan.com
// API WHEN
// File foramtting: "<original name>_u18chan.<ext>"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    class uEighteenChan : ImageBoard {
        public static string regThread = "u18chan.com/[a-zA-Z0-9]*?/topic/[0-9]*";

        public uEighteenChan(string url, bool isBoard) : base(url, isBoard) {
            this.Board = isBoard;
            this.imName = "u18chan";
            if (!isBoard) {
                Match match = Regex.Match(url, @"u18chan.com/[a-zA-Z0-9]*?/topic/[0-9]\d*");
                this.URL = "https://" + match.Groups[0].Value;
                this.SaveTo = (YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3] + "\\" + getURL().Split('/')[5]);
            }
            else {
                this.URL = url;
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3];
            }
            this.checkedAt = DateTime.Now.AddYears(-20);
        }

        public new static bool isThread(string url) {
            if (url.StartsWith("http://")) { url = url.Replace("http://", "https://"); }
            if (url.StartsWith("https://u18chan.com/") || url.StartsWith("https://www.u18chan.com/") || url.StartsWith("https://u18chan.com/board/u18chan/") || url.StartsWith("https://www.u18chan.com/board/u18chan/"))
            { return true; } else { return false; }
        }
        public new static bool isBoard(string url) { return false; } // Always return false for board downloading.

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
            string[] images;                    // Array that contains direct URLs to the image files
            string[] thumbnails;                // Array that contains direct URLs to the thumbnails
            //var lwebsite = new List<string>();  // List that contains all the lines that start with "File: <a href=\"" for parsing
            var limages = new List<string>();   // List that contains all the lines that have image URLs after parsing them
            string website;                // The string that contains the source for HTML saving.

            try {
                if (!isModified(this.getURL())) {
                    return;
                }
                website = Controller.getHTML(this.getURL());

                // Download the HTML source
                website = Controller.getHTML(this.getURL());
                
                // Look for the file links using Regex
                Regex href = new Regex("(?<=File: <a href=\").*?(?=\" target=\"_blank\">)");
                foreach (Match imageLinks in href.Matches(website))
                    limages.Add(imageLinks.ToString());

                // Convert the images to an array & clear the useless lists (save RAM)
                images = limages.ToArray();
                limages.Clear();

                // Create the directory to save files in.
                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);

                // Downloads images from the lists
                for (int y = 0; y < images.Length; y++) {
                    string file = images[y].Split('/')[6];
                    string url = images[y];
                    string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                    string newfilename = file;
                    limages.Add(images[y].Replace("_u18chan.","s_u18chan.")); // Renames the _u18chan to s_u18chan for thumbnail URLs
                    if (YCSettings.Default.originalName) {
                        // Replace any invalid file names.
                        newfilename = newfilename.Replace("_u18chan", ""); // This removes the _u18chan name in any files, as they use the original name.
                        for (int z = 0; z < badchars.Length - 1; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");
                        
                        Controller.downloadFile(images[y], this.SaveTo, true, newfilename);
                        website = website.Replace(url, newfilename);
                    }
                    else {
                        // u-18chan saves files as the original file name, just appends _u18chan to the end of the file names
                        for (int z = 0; z < badchars.Length; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");

                        Controller.downloadFile(images[y], this.SaveTo, true, newfilename);
                        website = website.Replace(url, this.SaveTo + "\\" + file);
                    }
                }

                // Convert thumbnails to an array & clear the list
                thumbnails = limages.ToArray();
                limages.Clear();

                // Downloads thumbnails
                if (YCSettings.Default.downloadThumbnails) {
                    if (!Directory.Exists(this.SaveTo + "\\thumb"))
                        Directory.CreateDirectory(this.SaveTo + "\\thumb");

                    for (int y = 0; y < thumbnails.Length; y++) {
                        string file = thumbnails[y].Split('/')[6];
                        string url = thumbnails[y];
                        Controller.downloadFile(thumbnails[y], this.SaveTo + "\\thumb");
                        website = website.Replace("src=\"//u18chan.com/uploads/user/lazyLoadPlaceholder_u18chan.gif\" data-original=\"" + url, "src=\"thumb\\" + file + "\" data-original=\"" + url);
                    }
                }

                // Download HTML
                if (YCSettings.Default.htmlDownload == true && website != "")
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

                return;
            }
            catch (Exception ex) {
                ErrorLog.reportError(ex.ToString()); 
            }

            GC.Collect();
        }

        public static string getTopic(string board) {
            // Furry Related (why)
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

            // The Basement (WHY)
            else if (board == "/pb/")
                return "Post Your Naked Body";
            else if (board == "/p/")
                return "Ponies"; // Why, honestly, WHY?
            else if (board == "/f/")
                return "Feral";
            else if (board == "/cub/")
                return "Cub";
            else if (board == "/gore/")
                return "Gore";

            // General
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

        public static bool isNotBlacklisted(string url) {
            string[] indiceURLs = new string[] { "ifur", "ic", "igfur", "igc", "ii", "ia", "ip", "if", "icub", "igore", "chat", "r", "guide", "vlkyra" };
            for (int y = 0; y < indiceURLs.Length - 1; y++)
                if (url.Split('/')[3] == indiceURLs[y]) { return false; }

            return true;
        }
    }
}
