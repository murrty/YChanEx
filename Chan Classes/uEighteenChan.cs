﻿// u18chan.com
// API WHEN

/*
 * This is because they do not have an api, much like 4ch & 4+4ch
 * This is the best way to download from u18chan, through source parsing.
 * 
 * I decided against board downloading for u18chan. It's just easier if entire boards don't get downloaded.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YChanEx {
    class uEighteenChan : ImageBoard {
        // Numbers in case u18chan adds a new board that has a number
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

        }

        public new static bool isThread(string url) {
            if (url.StartsWith("http://")) { url = url.Replace("http://", "https://"); }
            if (url.StartsWith("https://u18chan.com/") || url.StartsWith("https://www.u18chan.com/") || url.StartsWith("https://u18chan.com/board/u18chan/") || url.StartsWith("https://www.u18chan.com/board/u18chan/"))
            { return true; } else { return false; }
        }
        public new static bool isBoard(string url) { return false; } // Always return false for board downloading.

        public override void download() {
            string[] images;                    // Array that contains direct URLs to the image files
            string[] thumbnails;                // Array that contains direct URLs to the thumbnails
            var lwebsite = new List<string>();  // List that contains all the lines that start with "File: <a href=\"" for parsing
            var limages = new List<string>();   // List that contains all the lines that have image URLs after parsing them
            string website;                // The string that contains the source for HTML saving.

            try {
                // Create the directory to save files in.
                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);

                // Download the HTML source
                website = Controller.getHTML(this.getURL());

                // Sift through the HTML source for lines that start with "File: <a href=\""
                var lines = website.Split('\n');
                foreach (string line in lines) {
                    if (line.Replace("	", "").StartsWith("File: <a href=\"")) {
                        lwebsite.Add(line);
                    }
                }

                // Get strings that are within <a href> html tags
                Regex findFiles = new Regex("(?<=<a href=\").*?(?=\" target=\"_blank\">)");
                foreach (Match imageLinks in findFiles.Matches(string.Join("\n", lwebsite))) {
                    limages.Add(imageLinks.ToString());
                }

                // Convert the images to an array & clear the useless lists (save RAM)
                images = limages.ToArray();
                limages.Clear();
                lwebsite.Clear();

                // Downloads images from the lists
                for (int y = 0; y < images.Length; y++) {
                    string file = images[y].Split('/')[6];
                    string url = images[y];
                    string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                    string newfilename = file;
                    limages.Add(images[y].Replace("_u18chan.","s_u18chan.")); // Renames the _u18chan to s_u18chan for thumbnail URLs
                    if (YCSettings.Default.originalName) {
                        // Replace any invalid file names.
                        newfilename = newfilename.Replace("_u18chan", "");
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

            } catch (WebException webEx) {
                Debug.Print(webEx.ToString());
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.logError(webEx.ToString(), "U18chan.download");

                return;
            } catch (Exception ex) { ErrorLog.logError(ex.ToString(), "U18chan.download"); }

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
