using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using YChanEx;

/// <summary>
/// This class contains methods for translating and managing chan threads.
/// Most backend is here, except for individual chan apis and parsing.
/// </summary>
class Chans {
    public static readonly string EmptyXML = "<root type=\"array\"></root>";
    public static readonly string[] InvalidFileCharacters = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
    public static string GetJSON(string InputURL, DateTime ModifiedSince = default(DateTime)) {
        try {
            string JSONOutput = null;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(InputURL);
            Request.UserAgent = YChanEx.Advanced.Default.UserAgent;
            Request.Method = "GET";
            if (ModifiedSince != default(DateTime)) {
                Request.IfModifiedSince = ModifiedSince;
            }
            var Response = (HttpWebResponse)Request.GetResponse();
            var ResponseStream = Response.GetResponseStream();
            using (StreamReader Reader = new StreamReader(ResponseStream)) {
                string JSONString = Reader.ReadToEnd();
                byte[] JSONBytes = Encoding.ASCII.GetBytes(JSONString);
                using (var MemoryStream = new MemoryStream(JSONBytes)) {
                    var Quotas = new XmlDictionaryReaderQuotas();
                    var JSONReader = JsonReaderWriterFactory.CreateJsonReader(MemoryStream, Quotas);
                    var XMLJSON = XDocument.Load(JSONReader);
                    JSONOutput = XMLJSON.ToString();
                    MemoryStream.Flush();
                    MemoryStream.Close();
                }
            }
            ResponseStream.Dispose();
            Response.Dispose();

            GC.Collect();

            if (JSONOutput != null) {
                if (JSONOutput != EmptyXML) {
                    return JSONOutput;
                }
            }

            return null;
        }
        catch (WebException WebEx) {
            //error log
            System.Windows.Forms.MessageBox.Show(WebEx.ToString());
            return null;
        }
        catch (Exception ex) {
            //error log
            System.Windows.Forms.MessageBox.Show(ex.ToString());
            return null;
        }
    }
    public static string GetHTML(string InputURL, bool RequireCookie = false, string RequiredCookie = null) {
        try {
            using (WebClient wc = new WebClient()) {
                wc.Headers.Add("User-Agent: " + YChanEx.Advanced.Default.UserAgent);

                if (RequireCookie && !string.IsNullOrEmpty(RequiredCookie)) {
                    wc.Headers.Add(HttpRequestHeader.Cookie, RequiredCookie);
                }

                return wc.DownloadString(InputURL);
            }
        }
        catch (WebException WebEx) {
            //error log
            System.Windows.Forms.MessageBox.Show(WebEx.ToString());
            return null;
        }
        catch (Exception Ex) {
            //error log
            System.Windows.Forms.MessageBox.Show(Ex.ToString());
            return null;
        }
    }
    public static bool DownloadFile(string FileURL, string Destination, string FileName, bool RequireCookie = false, string RequiredCookie = null) {
        try {
            if (!Directory.Exists(Destination)) {
                Directory.CreateDirectory(Destination);
            }
            using (WebClientMethod wc = new WebClientMethod()) {
                wc.Method = "GET";
                wc.Headers.Add(HttpRequestHeader.UserAgent, YChanEx.Advanced.Default.UserAgent);
                if (RequireCookie) {
                    wc.Headers.Add(HttpRequestHeader.Cookie, RequiredCookie);
                }
                string FullFileName = Destination + "\\" + FileName;

                // TODO: Add option for names greater than 255
                if (FullFileName.Length > 255 && !Downloads.Default.AllowFileNamesGreaterThan255) {
                    string FileExtension = FileName.Split('.')[FileName.Split('.').Length - 1];
                    string OldFileName = FileName;
                    FileName = FullFileName.Substring(0, (255 - FileExtension.Length - 1));
                    File.WriteAllText(Destination + "\\" + FileName + ".txt", OldFileName);

                    FullFileName = Destination + "\\" + FileName;
                }

                if (!File.Exists(FullFileName)) {
                    wc.DownloadFile(FileURL, FullFileName);
                }
            }

            return true;
        }
        catch (WebException WebEx) {
            ErrorLog.ReportWebException(WebEx, FileURL);
            return false;
        }
        catch (Exception Ex) {
            ErrorLog.ReportException(Ex);
            return false;
        }
    }

    public static bool IsModified(string ThreadURL, DateTime LastCheck) {
        try {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(ThreadURL);
            Request.UserAgent = YChanEx.Advanced.Default.UserAgent;
            Request.IfModifiedSince = LastCheck;
            Request.Method = "HEAD";
            var Response = (HttpWebResponse)Request.GetResponse();
            return true;
        }
        catch (WebException WebEx) {
            var Response = (HttpWebResponse)WebEx.Response;
            if (Response.StatusCode != HttpStatusCode.NotModified) {
                //error log
            }
            return false;
        }
    }

    public static bool SupportedChan(string URL) {
        Regex Matcher = new Regex(ChanRegex.FourChanURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        Matcher = new Regex(ChanRegex.FourTwentyChanURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        Matcher = new Regex(ChanRegex.SevenChanURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        Matcher = new Regex(ChanRegex.EightChanURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        Matcher = new Regex(ChanRegex.EightKunURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        Matcher = new Regex(ChanRegex.fchanURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        Matcher = new Regex(ChanRegex.u18chanURL);
        if (Matcher.IsMatch(URL)) {
            return true;
        }

        return false;
    }

    public static int GetChanType(string URL) {
        Regex Matcher = new Regex(ChanRegex.FourChanURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.FourChan;
        }

        Matcher = new Regex(ChanRegex.FourTwentyChanURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.FourTwentyChan;
        }

        Matcher = new Regex(ChanRegex.SevenChanURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.SevenChan;
        }

        Matcher = new Regex(ChanRegex.EightChanURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.EightChan;
        }

        Matcher = new Regex(ChanRegex.EightKunURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.EightKun;
        }

        Matcher = new Regex(ChanRegex.fchanURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.fchan;
        }

        Matcher = new Regex(ChanRegex.u18chanURL);
        if (Matcher.IsMatch(URL)) {
            return (int)ChanTypes.Types.u18chan;
        }

        return (int)ChanTypes.Types.None;
    }

    public static bool SaveThreads(List<string> Threads) {
        if (General.Default.SaveQueueOnExit) {
            try {
                string FileContentBuffer = string.Empty;
                for (int i = 0; i < Threads.Count; i++) {
                    FileContentBuffer += Threads[i] + "\n";
                }
                File.WriteAllText(Program.ApplicationFilesLocation + "\\threads.dat", FileContentBuffer.Trim('\n'));
                return true;
            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return false;
            }
        }
        return false;
    }
    public static string[] LoadThreads() {
        try {
            if (System.IO.File.Exists(Program.ApplicationFilesLocation + "\\threads.dat")) {
                string ReadThreads = System.IO.File.ReadAllText(Program.ApplicationFilesLocation + "\\threads.dat").Trim('\n');
                if (!string.IsNullOrEmpty(ReadThreads)) {
                    return ReadThreads.Split('\n');
                }
            }
            return null;
        }
        catch (Exception ex) {
            ErrorLog.ReportException(ex);
            return null;
        }
    }

    public static string CleanURL(string URL) {
        string NewURL = string.Empty;
        if (URL.StartsWith("http://")) {
            NewURL = "https://" + URL.Substring(7, URL.Length - 7);
        }
        if (NewURL.StartsWith("https://www.")) {
            NewURL = "https://" + NewURL.Substring(12, NewURL.Length - 12);
        }
        return NewURL;
    }

}
/// <summary>
/// Contains string.Format-ready strings for *chan API links
/// </summary>
class ChanApiLinks {
    public static readonly string FourChan = "https://a.4cdn.org/{0}/thread/{1}.json";
    public static readonly string FourTwentyChan = "https://api.420chan.org/{0}/res/{1}.json";
    public static readonly string EightChan = "https://8chan.moe/{0}/res/{1}.json";
    public static readonly string EightKun = "https://8kun.top/{0}/res/{1}.json";

}
/// <summary>
/// The strings for all board titles in chans.
/// </summary>
class BoardTitles {
    public static string FourChan(string Board) {
        if (General.Default.UseFullBoardNameForTitle) {
            switch (Board.ToLower()) {
                #region Japanese Culture
                case "a":
                    return "Anime & Manga";
                case "c":
                    return "Anime/Cute";
                case "w":
                    return "Anime/Wallpapers";
                case "m":
                    return "Mecha";
                case "cgl":
                    return "Cosplay & EGL";
                case "cm":
                    return "Cute/Male";
                case "f":
                    return "Flash";
                case "n":
                    return "Transportation";
                case "jp":
                    return "Otaku Culture";
                #endregion

                #region Video Games
                case "v":
                    return "Video Games";
                case "vg":
                    return "Video Game Generals";
                case "vp":
                    return "Pokémon";
                case "vr":
                    return "Retro Games";
                #endregion

                #region Interests
                case "co":
                    return "Comics & Cartoons";
                case "g":
                    return "Technology";
                case "tv":
                    return "Television & Film";
                case "k":
                    return "Weapons";
                case "o":
                    return "Auto";
                case "an":
                    return "Animals & Nature";
                case "tg":
                    return "Traditional Games";
                case "sp":
                    return "Sports";
                case "asp":
                    return "Alternative Sports";
                case "sci":
                    return "Science & Math";
                case "his":
                    return "History & Humanities";
                case "int":
                    return "International";
                case "out":
                    return "Outdoors";
                case "toy":
                    return "Toys";

                #endregion

                #region Creative
                case "i":
                    return "Oekaki";
                case "po":
                    return "Papercraft & Origami";
                case "p":
                    return "Photography";
                case "ck":
                    return "Food & Cooking";
                case "ic":
                    return "Artwork/Critique";
                case "wg":
                    return "Wallpapers/General";
                case "lit":
                    return "Literature";
                case "mu":
                    return "Music";
                case "fa":
                    return "Fashion";
                case "3":
                    return "3DCG";
                case "gd":
                    return "Graphic Design";
                case "diy":
                    return "Do It Yourself";
                case "wsg":
                    return "Worksafe GIF";
                case "qst":
                    return "Quests";
                #endregion

                #region Other
                case "biz":
                    return "Business & Finance";
                case "trv":
                    return "Travel";
                case "fit":
                    return "Fitness";
                case "x":
                    return "Paranormal";
                case "adv":
                    return "Advice";
                case "lgbt":
                    return "Lesbian, Gay, Bisexual, & Transgender";
                case "mlp":
                    return "My Little Pony"; // disgusting.
                case "news":
                    return "Current News";
                case "wsr":
                    return "Worksafe Requests";
                case "vip":
                    return "Very Important Posts";
                #endregion

                #region Misc
                case "b":
                    return "Random";
                case "r9k":
                    return "ROBOT9001";
                case "pol":
                    return "Politically Incorrect";
                case "bant":
                    return "International/Random";
                case "soc":
                    return "Cams & Meetups";
                case "s4s":
                    return "Shit 4chan Says";
                #endregion

                #region Adult
                case "s":
                    return "Sexy Beautiful Women";
                case "hc":
                    return "Hardcore";
                case "hm":
                    return "Handsome Men";
                case "h":
                    return "Hentai";
                case "e":
                    return "Ecchi";
                case "u":
                    return "Yuri";
                case "d":
                    return "Hentai/Alternative";
                case "y":
                    return "Yaoi";
                case "t":
                    return "Torrents";
                case "hr":
                    return "High Resolution";
                case "gif":
                    return "Adult GIF";
                case "aco":
                    return "Adult Cartoons";
                case "r":
                    return "Adult Requests";
                #endregion

                #region Unlisted
                case "trash":
                    return "Off-Topic";
                case "qa":
                    return "Question & Answer";
                #endregion

                default:
                    return "Unknown board";
            }
        }
        else {
            return Board;
        }
    }
    public static string FourTwentyChan(string Board) {
        if (General.Default.UseFullBoardNameForTitle) {
            switch (Board.ToLower()) {
                #region Drugs
                case "weed":
                    return "Cannabis Discussion";
                case "hooch":
                    return "Alcohol Discussion";
                case "mdma":
                    return "Ecstasy Discussion";
                case "psy":
                    return "Psychedelic Discussion";
                case "stim":
                    return "Stimulant Discussion";
                case "dis":
                    return "Dissociative Discussion";
                case "opi":
                    return "Opiate Discussion";
                case "vape":
                    return "Vaping Discussion";
                case "tobacco":
                    return "Tobacco Discussion";
                case "benz":
                    return "Benzo Discussion";
                case "deli":
                    return "Deliriant Discussion";
                case "other":
                    return "Other Drugs Discussion";
                case "jenk":
                    return "Jenkem Discussion";
                case "detox":
                    return "Detoxing & Rehabilitation";
                #endregion

                #region Lifestye
                case "qq":
                    return "Personal Issues";
                case "dr":
                    return "Dream Discussion";
                case "ana":
                    return "Fitness";
                case "nom":
                    return "Food, Munchies & Cooking";
                case "vroom":
                    return "Travel & Transportation";
                case "st":
                    return "Style & Fashion";
                case "nra":
                    return "Weapons Discussion";
                case "sd":
                    return "Sexuality Discussion";
                case "cd":
                    return "Transgender Discussion";
                #endregion

                #region Academia
                case "art":
                    return "Art & Okekai";
                case "sagan":
                    return "Space... the Final Frontier";
                case "lang":
                    return "World Languages";
                case "stem":
                    return "Science, Technology, Engineering & Mathematics";
                case "his":
                    return "History Discussion";
                case "crops":
                    return "Growing & Botany";
                case "howto":
                    return "Guides & Tutorials";
                case "law":
                    return "Law Discussion";
                case "lit":
                    return "Books & Literature";
                case "med":
                    return "Medicine & Health";
                case "pss":
                    return "Philosophy & Social Sciences";
                case "tech":
                    return "Computers & Tech Support";
                case "prog":
                    return "Programming";
                #endregion

                #region Media
                case "1701":
                    return "Star Trek Discussion";
                case "sport":
                    return "Sports";
                case "mtv":
                    return "Movies & Television";
                case "f":
                    return "Flash";
                case "m":
                    return "Music & Production";
                case "mma":
                    return "Mixed Martial Arts Discussion";
                case "616":
                    return "Comics & Web Comics Discussion";
                case "a":
                    return "Anime & Manga Discussion";
                case "wooo":
                    return "Professional Wrestling Discussion";
                case "n":
                    return "World News";
                case "vg":
                    return "Video Games Discussion";
                case "po":
                    return "Pokémon Discussion";
                case "tg":
                    return "Traditional Games";
                #endregion

                #region Miscellanea
                case "420":
                    return "420chan Discussion & Staff Interaction";
                case "b":
                    return "Random & High Stuff";
                case "spooky":
                    return "Paranormal Discussion";
                case "dino":
                    return "Dinosaur Discussion";
                case "fo":
                    return "Post-apocalyptic";
                case "ani":
                    return "Animal Discussion";
                case "nj":
                    return "Netjester AI Conversation Chamber";
                case "nc":
                    return "Net Characters";
                case "tinfoil":
                    return "Conspiracy Theories";
                case "w":
                    return "Dumb Wallpapers Below";
                #endregion

                #region Adult
                case "h":
                    return "Hentai";
                #endregion

                default:
                    return "Unknown board";
            }
        }
        else {
            return Board;
        }
    }
    public static string SevenChan(string Board) {
        if (General.Default.UseFullBoardNameForTitle) {
            switch (Board.ToLower()) {
                #region 7chan & Related services
                case "7ch":
                    return "Site Discussion";
                case "ch7":
                    return "Channel7 & Radio 7";
                case "irc":
                    return "Internet Relay Circlejerk";
                #endregion

                #region VIP
                case "777":
                    return "gardening";
                case "VIP":
                    return "Very Important Posters";
                case "civ":
                    return "Civics";
                case "vip6":
                    return "IPv6 for VIP";
                #endregion

                #region Premium Content
                case "b":
                    return "Random";
                case "banner":
                    return "Banners";
                case "f":
                    return "Flash";
                case "gfc":
                    return "Grahpics Manipulation";
                case "fail":
                    return "Failure";
                #endregion

                #region SFW
                case "class":
                    return "The Finer Things";
                case "co":
                    return "Comics and Cartoons";
                case "eh":
                    return "Particularly uninteresting conversation";
                case "fit":
                    return "Fitness & Health";
                case "halp":
                    return "Technical Support";
                case "jew":
                    return "Thrifty Living";
                case "lit":
                    return "Literature";
                case "phi":
                    return "Philosophy";
                case "pr":
                    return "Programming";
                case "rnb":
                    return "Rage and Baww";
                case "sci":
                    return "Science, Technology, Engineering, and Mathematics";
                case "tg":
                    return "Tabletop Games";
                case "w":
                    return "Weapons";
                case "zom":
                    return "Zombies";
                #endregion

                #region General
                case "a":
                    return "Anime & Manga";
                case "grim":
                    return "Cold, Grim & Miserable";
                case "hi":
                    return "History and Culture";
                case "me":
                    return "Film, Music & Television";
                case "rx":
                    return "Drugs";
                case "vg":
                    return "Video Games";
                case "wp":
                    return "Wallpapers";
                case "x":
                    return "Paranormal & Conspiracy";
                #endregion

                #region Porn
                case "cake":
                    return "Delicious";
                case "cd":
                    return "Crossdressing";
                case "d":
                    return "Alternative Hentai";
                case "di":
                    return "Sexy Beautiful Traps";
                case "elit":
                    return "Erotic Literature";
                case "fag":
                    return "Men Discussion";
                case "fur":
                    return "Furry";
                case "gif":
                    return "Animated GIFs";
                case "h":
                    return "Hentai";
                case "men":
                    return "Sexy Beautiful Men";
                case "pco":
                    return "Porn Comics";
                case "s":
                    return "Sexy Beautiful Women";
                case "sm":
                    return "Shotacon"; // Why shotacon but no lolicon?
                case "ss":
                    return "Straight Shotacon"; // again, why shotacon but no lolicon?
                case "unf":
                    return "Uniforms";
                case "v":
                    return "The Vineyard";
                #endregion

                default:
                    return "Unknown board";
            }
        }
        else {
            return Board;
        }
    }
    public static string EightChan(string BoardOrDescription, bool IsDescription = false) {
        if (IsDescription) {
            return BoardOrDescription.Replace("<p id=\"labelDescription\">", "").Replace("</p>", "");
        }
        else {
            //if (General.Default.UseFullBoardNameForTitle) {
            //return BoardOrDescription.Replace("<p id=\"labelName\">", "").Replace("</p>", "").Replace("/ - ", "|").Split('|')[1];
            //}
            //else {
            return BoardOrDescription;
            //}
        }
    }
    public static string EightKun(string BoardOrDescription, bool IsDescription = false) {
        if (IsDescription) {
            return BoardOrDescription.Replace("<div class=\"subtitle\">", "").Replace("<p>", "");
        }
        else {
            if (General.Default.UseFullBoardNameForTitle) {
                //return BoardOrDescription.Replace("<h1>", "").Replace("</h1>", "").Replace("/ - ", "|").Split('|')[1];
                return BoardOrDescription;
            }
            else {
                return BoardOrDescription;
            }
        }
    }
    public static string fchan(string Board) {
        if (General.Default.UseFullBoardNameForTitle) {
            switch (Board.ToLower()) {
                #region Normal image boards
                case "f":
                    return "female";
                case "m":
                    return "male";
                case "h":
                    return "herm";
                case "s":
                    return "straight";
                case "toon":
                    return "toon";
                case "a":
                    return "alternative";
                case "ah":
                    return "alternative (hard)";
                case "c":
                    return "clean";
                #endregion

                #region Specialized image boards
                case "artist":
                    return "artist";
                case "crit":
                    return "critique";
                case "b":
                    return "banners";
                #endregion

                default:
                    return "Unknown board";
            }
        }
        else {
            return Board;
        }
    }
    public static string u18chan(string Board) {
        if (General.Default.UseFullBoardNameForTitle) {
            switch (Board.ToLower()) {
                #region Furry Related
                case "fur":
                    return "Furries";
                case "c":
                    return "Furry Comics";
                case "gfur":
                    return "Gay Furries";
                case "gc":
                    return "Gay Furry Comics";
                case "i":
                    return "Intersex";
                case "rs":
                    return "Request & Source";
                case "a":
                    return "Animated";
                case "cute":
                    return "Cute";
                #endregion

                #region The Basement
                case "pb":
                    return "Post Your Naked Body";
                case "p":
                    return "Ponies"; // Why, honestly, WHY?
                case "f":
                    return "Feral";
                case "cub":
                    return "Cub";
                case "gore":
                    return "Gore";
                #endregion

                #region General
                case "d":
                    return "Discussion";
                case "mu":
                    return "Music";
                case "w":
                    return "Wallpapers";
                case "v":
                    return "Video Games";
                case "lo":
                    return "Lounge";
                case "tech":
                    return "Technology";
                case "lit":
                    return "Literature";
                #endregion

                default:
                    return "Unknown board";
            }
        }
        else {
            return Board;
        }
    }
}
/// <summary>
/// The Regex strings for detecting the chans.
/// </summary>
class ChanRegex {
    public static class DefaultRegex {
        public static readonly string FourChanURL =
            "boards.4chan(nel)?.org/[a-zA-Z0-9]*?/thread[0-9]*";
        public static readonly string FourTwentyChanURL =
            "boards.420chan.org/[a-zA-Z0-9]*?/thread/[0-9]*";
        public static readonly string SevenChanURL =
            "7chan.org/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
        public static readonly string SevenChanFiles =
            "http(?:s)?:\\/\\/(?:www\\.)?7chan.org\\/([a-zA-Z0-9]+)\\/src\\/([0-9]+)\\.(?:jpg|jpeg|gif|png|webm|mp4)?";
        public static readonly string EightChanURL =
            "8chan.moe/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
        public static readonly string EightKunURL =
            "8kun.top/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
        public static readonly string fchanURL =
            "fchan.us/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
        public static readonly string fchanFiles =
            "(?<=<a target=\"_blank\" href=\").*?(?=\" rel=\"nofollow\">)";
        public static readonly string fchanNames =
            "(?<=File: <a target=\"_blank\" href=\"{0}\" rel=\"nofollow\">).*?(?=</a>)";
        public static readonly string u18chanURL =
            "u18chan.com/(.*?)[a-zA-Z0-9]*?/topic/[0-9]*";
        public static readonly string u18chanPosts =
            "(?<=a href=\").*?(_image\" style=\"width: )";
    }
    public static string FourChanURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.FourChanURL)) {
                return RegexStrings.Default.FourChanURL;
            }
            else {
                return DefaultRegex.FourChanURL;
            }
        }
    }
    public static string FourTwentyChanURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.FourTwentyChanURL)) {
                return RegexStrings.Default.FourTwentyChanURL;
            }
            else {
                return DefaultRegex.FourTwentyChanURL;
            }
        }
    }
    public static string SevenChanURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.SevenChanURL)) {
                return RegexStrings.Default.SevenChanURL;
            }
            else {
                return DefaultRegex.SevenChanURL;
            }
        }
    }
    public static string SevenChanFiles {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.SevenChanFiles)) {
                return RegexStrings.Default.SevenChanFiles;
            }
            else {
                return DefaultRegex.SevenChanFiles;
            }
        }
    }
    public static string EightChanURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.EightChanURL)) {
                return RegexStrings.Default.EightChanURL;
            }
            else {
                return DefaultRegex.EightChanURL;
            }
        }
    }
    public static string EightKunURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.EightKunURL)) {
                return RegexStrings.Default.EightKunURL;
            }
            else {
                return DefaultRegex.EightKunURL;
            }
        }
    }
    public static string fchanURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.fchanURL)) {
                return RegexStrings.Default.fchanURL;
            }
            else {
                return DefaultRegex.fchanURL;
            }
        }
    }
    public static string fchanFiles {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.fchanFiles)) {
                return RegexStrings.Default.fchanFiles;
            }
            else {
                return DefaultRegex.fchanFiles;
            }
        }
    }
    public static string fchanNames {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.fchanNames)) {
                return RegexStrings.Default.fchanNames;
            }
            else {
                return DefaultRegex.fchanNames;
            }
        }
    }
    public static string u18chanURL {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.u18chanURL)) {
                return RegexStrings.Default.u18chanURL;
            }
            else {
                return DefaultRegex.u18chanURL;
            }
        }
    }
    public static string u18chanPosts {
        get {
            if (!string.IsNullOrEmpty(RegexStrings.Default.u18chanPosts)) {
                return RegexStrings.Default.u18chanPosts;
            }
            else {
                return DefaultRegex.u18chanPosts;
            }
        }
    }
}
/// <summary>
/// Enumerations of the int-value of the supported chans.
/// </summary>
class ChanTypes {
    public enum Types : int {
        None = -1,
        FourChan = 0,
        FourTwentyChan = 1,
        SevenChan = 2,
        EightChan = 3,
        EightKun = 4,
        fchan = 5,
        u18chan = 6
    }
    public static int None { get { return -1; } }
    public static int FourChan { get { return 0; } }
    public static int FourTwentyChan { get { return 1; } }
    public static int SevenChan { get { return 2; } }
    public static int EightChan { get { return 3; } }
    public static int EightKun { get { return 4; } }
    public static int fchan { get { return 5; } }
    public static int u18chan { get { return 6; } }
}
/// <summary>
/// Strings of the thread statuses. If this will be kept is TBD.
/// </summary>
class ThreadStatuses {
    public static string Downloading { get { return "Downloading"; } }
    public static string Waiting { get { return "Waiting"; } }
    public static string Has404 { get { return "404'd"; } }
    public static string HasAborted { get { return "Aborted"; } }
}