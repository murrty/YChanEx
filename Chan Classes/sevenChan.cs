// 7chan.org
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
    class sevenChan : ImageBoard {
        public static string regThread = "7chan.org/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";

        public sevenChan(string url, bool isBoard) : base(url, isBoard) {
            this.Board = isBoard;
            this.imName = "7chan";
            if (!isBoard) {
                Match match = Regex.Match(URL, @"7chan.org/[a-zA-Z0-9]*?/res/[0-9]\d*");
                this.URL = "https://" + match.Groups[0].Value + ".html";
                this.SaveTo = (YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3] + "\\" + getURL().Split('/')[5].Replace(".html",""));
            }
            else {
                this.URL = url;
                this.SaveTo = YCSettings.Default.downloadPath + "\\" + this.imName + "\\" + getURL().Split('/')[3];
            }
        }

        public new static bool isThread(string url) {
            Regex urlMatcher = new Regex(regThread);
            if (urlMatcher.IsMatch(url))
                return true;
            else
                return false;
        }
        public new static bool isBoard(string url) { return false; } // Always return false for board downloading.

        public override void download() {
            string[] images;
            string[] thumbnails; // .Split('.")[2] = Extension
            string[] original;
            var lwebsite = new List<string>();
            var lImages = new List<string>();
            var lThumbnails = new List<string>();
            var lOriginal = new List<string>();
            string website;

            try {
                if (!Directory.Exists(this.SaveTo))
                    Directory.CreateDirectory(this.SaveTo);

                website = Controller.getHTML(this.getURL());

                string[] lines = website.Split('\n');
                string extension;
                Regex href = new Regex("(?<=<a href=\").*?(?=\" id=\"expandimg_)");
                foreach (Match imageLinks in href.Matches(website)) {
                    lImages.Add(imageLinks.ToString());
                    if (YCSettings.Default.downloadThumbnails) {
                        extension = imageLinks.ToString().Split('.')[2];
                        lThumbnails.Add(imageLinks.ToString().Replace("." + extension, "s." + extension).Replace("/src/", "/thumb/"));
                    }
                    if (YCSettings.Default.originalName)
                        lOriginal.Add(lines[Array.FindIndex(lines, x => x.Contains(imageLinks.ToString())) + 8].Replace(", ", ""));
                }

                images = lImages.ToArray();
                original = lOriginal.ToArray();
                lImages.Clear();
                lOriginal.Clear();
                lwebsite.Clear();

                for (int y = 0; y < images.Length; y++) {
                    string file = images[y].Split('/')[5];
                    string url = images[y];
                    string[] badchars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
                    string newfilename = file;
                    if (YCSettings.Default.originalName) {
                        newfilename = original[y];
                        for (int z = 0; z < badchars.Length - 1; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");

                        Controller.downloadFile(images[y], this.SaveTo, true, newfilename);
                        website = website.Replace(url, newfilename);
                    }
                    else {
                        for (int z = 0; z < badchars.Length; z++)
                            newfilename = newfilename.Replace(badchars[z], "-");

                        Controller.downloadFile(images[y], this.SaveTo, true, newfilename);
                        website = website.Replace(url, newfilename);
                    }
                }

                thumbnails = lThumbnails.ToArray();
                lThumbnails.Clear();

                if (YCSettings.Default.downloadThumbnails) {
                    if (!Directory.Exists(this.SaveTo + "\\thumb"))
                        Directory.CreateDirectory(this.SaveTo + "\\thumb");

                    for (int y = 0; y < thumbnails.Length; y++) {
                        string file = thumbnails[y].Split('/')[5];
                        string url = thumbnails[y];
                        Controller.downloadFile(thumbnails[y], this.SaveTo + "\\thumb");
                        website = website.Replace(url, "thumb\\" + file);
                    }
                }

                if (YCSettings.Default.htmlDownload == true && website != "")
                    Controller.saveHTML(false, website.Replace("\"//7chan.org", "\"https://7chan.org"), this.SaveTo);
            
            }
            catch (WebException webEx) {
                Debug.Print(webEx.ToString());
                if (((int)webEx.Status) == 7)
                    this.Gone = true;
                else
                    ErrorLog.logError(webEx.ToString(), "Schan.download");

                return;
            } catch (Exception ex) { Debug.Print(ex.ToString()); ErrorLog.logError(ex.ToString(), "Schan.download"); }
        }

        public static string getTopic (string board) {
            // 7chan & Related services
            if (board == "7ch") return "Site Discussion";
            else if (board == "ch7") return "Channel7 & Radio 7";
            else if (board == "irc") return "Internet Relay Circlejerk";
            // VIP
            else if (board == "777") return "gardening";
            else if (board == "VIP") return "Very Important Posters";
            else if (board == "civ") return "Civics";
            else if (board == "vip6") return "IPv6 for VIP";
            // Premium Content
            else if (board == "b") return "Random";
            else if (board == "banner") return "Banners";
            else if (board == "f") return "Flash";
            else if (board == "gfc") return "Grahpics Manipulation";
            else if (board == "fail") return "Failure";
            // SFW
            else if (board == "class") return "The Finer Things";
            else if (board == "co") return "Comics and Cartoons";
            else if (board == "eh") return "Particularly uninteresting conversation";
            else if (board == "fit") return "Fitness & Health";
            else if (board == "halp") return "Technical Support";
            else if (board == "jew") return "Thrifty Living";
            else if (board == "lit") return "Literature";
            else if (board == "phi") return "Philosophy";
            else if (board == "pr") return "Programming";
            else if (board == "rnb") return "Rage and Baww";
            else if (board == "sci") return "Science, Technology, Engineering, and Mathematics";
            else if (board == "tg") return "Tabletop Games";
            else if (board == "w") return "Weapons";
            else if (board == "zom") return "Zombies";
            // General
            else if (board == "a") return "Anime & Manga";
            else if (board == "grim") return "Cold, Grim & Miserable";
            else if (board == "hi") return "History and Culture";
            else if (board == "me") return "Film, Music & Television";
            else if (board == "rx") return "Drugs";
            else if (board == "vg") return "Video Games";
            else if (board == "wp") return "Wallpapers";
            else if (board == "x") return "Paranormal & Conspiracy";
            // Porn
            else if (board == "cake") return "Delicious";
            else if (board == "cd") return "Crossdressing";
            else if (board == "d") return "Alternative Hentai";
            else if (board == "di") return "Sexy Beautiful Traps";
            else if (board == "elit") return "Erotic Literature";
            else if (board == "fag") return "Men Discussion";
            else if (board == "fur") return "Furry";
            else if (board == "gif") return "Animated GIFs";
            else if (board == "h") return "Hentai";
            else if (board == "men") return "Sexy Beautiful Men";
            else if (board == "pco") return "Porn Comics";
            else if (board == "s") return "Sexy Beautiful Women";
            else if (board == "sm") return "Shotacon"; // Why shotacon but no lolicon?
            else if (board == "ss") return "Straight Shotacon"; // again, why shotacon but no lolicon?
            else if (board == "unf") return "Uniforms";
            else if (board == "v") return "The Vineyard";
            else return "";
        }
    }
}
