// fchan.us
// API WHEN
// File formatting: "<board>_<uploaded file number>_<original name>.<ext>"

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
    class fChan  : ImageBoard {
        public static string regThread = "fchan.us/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
        public static string regTitle = "(?<=<title>).*?(?=</title>)";
        public static string reqCookie = "disclaimer=seen"; //fchan requires a cookie.

        public fChan(string url, bool isBoard) : base(url, isBoard) {
            this.Board = isBoard;
            this.imName = "fchan";
            if (!isBoard) {
                Match match = Regex.Match(URL, @"fchan.us/[a-zA-Z0-9]*?/res/[0-9]\d*");
                this.URL = "http://" + match.Groups[0].Value + ".html";
                this.SaveTo = (YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3] + "\\" + getURL().Split('/')[5].Replace(".html",""));
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
            string[] images;                        // image urls array
            string[] thumbnails;                    // thumbnail url array
            string[] original;                      // original file name array
            var lImages = new List<string>();       // List to add image links to
            var lThumbnails = new List<string>();   // List to add thumbnail links to
            var lOriginal = new List<string>();     // List to add the original name to.
            string website;

            try {
                //if (!isModified(this.getURL())) {
                //    return;
                //}
                website = Controller.getHTML(this.getURL(), true, reqCookie);

                string[] lines = website.Split('\n');
                string foundURL;
                string thumbURL;
                string originalName;
                string baseURL = "http://fchan.us";
                Regex href = new Regex("(?<=<a target=\"_blank\" href=\").*?(?=\" rel=\"nofollow\">)");
                Regex fileName = new Regex("");
                foreach (Match imageLinks in href.Matches(website)) {
                    foundURL = imageLinks.ToString();
                    if (!lImages.Contains(baseURL + foundURL))    // Image
                        lImages.Add(baseURL + foundURL);

                    fileName = new Regex("(?<=File: <a target=\"_blank\" href=\"" + foundURL + "\" rel=\"nofollow\">).*?(?=</a>)");
                    foreach (Match origFileName in fileName.Matches(website)) {
                        originalName = origFileName.ToString();
                        if (YCSettings.Default.originalName) {
                            if (!lOriginal.Contains(originalName))
                                lOriginal.Add(originalName);    // Original file name

                            website = website.Replace(foundURL, originalName);
                        }
                        else {
                            website = website.Replace(foundURL, foundURL.Split('/')[2]);
                        }

                        if (YCSettings.Default.downloadThumbnails) {
                            thumbURL = foundURL.Replace("/src/", "/" + this.getURL().Split('/')[3] + "/thumb/").Replace("_" + originalName, "s_" + originalName).Replace(".png", ".jpg").Replace(".gif", ".jpg");
                            if (!lThumbnails.Contains(baseURL + thumbURL))
                                lThumbnails.Add(baseURL + thumbURL);
                            website = website.Replace(thumbURL, "thumb/" + thumbURL.Split('/')[3]);
                        }
                    }
                }

                images = lImages.ToArray();
                original = lOriginal.ToArray();
                lImages.Clear();
                lOriginal.Clear();


                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);

                for (int y = 0; y < images.Length; y++) {
                    string file = images[y].Split('/')[4];
                    string url = images[y];
                    string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                    string newfilename = file;
                    if (YCSettings.Default.originalName) {
                        newfilename = original[y];
                        for (int z = 0; z < badchars.Length - 1; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");

                        Controller.downloadFile(images[y], this.SaveTo, true, newfilename, true, reqCookie);
                        website = website.Replace(url, newfilename);
                    }
                    else {
                        for (int z = 0; z < badchars.Length; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");

                        Controller.downloadFile(images[y], this.SaveTo, true, newfilename, true, reqCookie);
                        website = website.Replace(url, newfilename);
                    }
                }

                thumbnails = lThumbnails.ToArray();
                lThumbnails.Clear();

                if (YCSettings.Default.downloadThumbnails) {
                    if (!Directory.Exists(this.SaveTo + "\\thumb"))
                        Directory.CreateDirectory(this.SaveTo + "\\thumb");

                    for (int y = 0; y < thumbnails.Length; y++) {
                        string file = thumbnails[y].Split('/')[3];
                        string url = thumbnails[y];
                        Controller.downloadFile(thumbnails[y], this.SaveTo + "\\thumb", false, string.Empty, true, reqCookie);
                        website = website.Replace(url, "thumb\\" + file);
                    }
                }

                if (YCSettings.Default.htmlDownload == true && website != "")
                    Controller.saveHTML(false, website.Replace("type=\"text/javascript\" src=\"", "type=\"text/javascript\" src=\"http://fchan.us").Replace("type=\"text/css\" href=\"", "type=\"text/css\" href=\"http://fchan.us"), this.SaveTo);

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
            } catch (Exception ex) { Debug.Print(ex.ToString()); ErrorLog.reportError(ex.ToString()); }
        }

        public static string getTopic(string board) {
            // Normal image boards
            if (board == "/f/") return "female";
            else if (board == "/m/") return "male";
            else if (board == "/h/") return "herm";
            else if (board == "/s/") return "straight";
            else if (board == "/toon/") return "toon";
            else if (board == "/a/") return "alternative";
            else if (board == "/ah/") return "alternative(hard)";
            else if (board == "/c/") return "clean";

            // Specialized image boards
            else if (board == "/artist/") return "artist";
            else if (board == "/crit/") return "critique";
            else if (board == "/b/") return "banners";

            else return "";
        }
    }
}
