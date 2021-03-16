using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace YChanEx {

    #region Enumerations
    /// <summary>
    /// Enumeration of the various thread statuses available
    /// </summary>
    public enum ThreadStatus : int {
        /// <summary>
        /// The thread has an unknown status.
        /// </summary>
        UnknownStatus = -1,

        /// <summary>
        /// The thread is waiting for the delay to rescan.
        /// </summary>
        Waiting = 0,
        /// <summary>
        /// The thread us currently scanning for files.
        /// </summary>
        ThreadScanning = 1,
        /// <summary>
        /// The thread is currently downloading files.
        /// </summary>
        ThreadDownloading = 2,
        /// <summary>
        /// The thread was not modified since last scan.
        /// </summary>
        ThreadNotModified = 3,
        /// <summary>
        /// The file from the thread has 404'd.
        /// </summary>
        ThreadFile404 = 4,
        /// <summary>
        /// The thread is reloading into memory.
        /// </summary>
        ThreadReloaded = 6,


        /// <summary>
        /// The thread was alive when it was saved.
        /// </summary>
        ThreadIsAlive = 100,
        /// <summary>
        /// The thread 404'd
        /// </summary>
        ThreadIs404 = 101,
        /// <summary>
        /// The thread was aborted.
        /// </summary>
        ThreadIsAborted = 102,
        /// <summary>
        /// The thread was archived.
        /// </summary>
        ThreadIsArchived = 103,
        /// <summary>
        /// The thread is not allowed to view the content.
        /// </summary>
        ThreadIsNotAllowed = 104,

        /// <summary>
        /// The thread is retrying the download.
        /// </summary>
        ThreadRetrying = 666,
        /// <summary>
        /// The thread wasn't downloaded properly.
        /// </summary>
        ThreadImproperlyDownloaded = 777,
        /// <summary>
        /// The thread information wasn't given when the thread download started.
        /// </summary>
        ThreadInfoNotSet = 888,
        /// <summary>
        /// The thread encountered an unknown error.
        /// </summary>
        ThreadUnknownError = 999,
        /// <summary>
        /// Thread is requesting to update the name
        /// </summary>
        ThreadUpdateName = 1111,
    }

    /// <summary>
    /// Enumeration of the thread events
    /// </summary>
    public enum ThreadEvent : int {
        /// <summary>
        /// The thread should parse the given information
        /// </summary>
        ParseForInfo = 0,
        /// <summary>
        /// The thread should start to download
        /// </summary>
        StartDownload = 1,
        /// <summary>
        /// The thread should update itself and wait until next download
        /// </summary>
        AfterDownload = 2,
        /// <summary>
        /// The thread should abort because the user requested it
        /// </summary>
        AbortDownload = 3,
        /// <summary>
        /// The thread should retry because the user requested it
        /// </summary>
        RetryDownload = 4,
        /// <summary>
        /// The thread was 404'd or aborted when it was added.
        /// </summary>
        ThreadWasGone = 5,

        /// <summary>
        /// The thread should abort because the application is closing.
        /// </summary>
        AbortForClosing = 999
    }

    /// <summary>
    /// Enumeration of the chan types available
    /// </summary>
    public enum ChanType : int {
        /// <summary>
        /// No chan was selected to download.
        /// </summary>
        None = -1,
        /// <summary>
        /// 4chan(nel) was selected to download.
        /// </summary>
        FourChan = 0,
        /// <summary>
        /// 420chan was selected to download.
        /// </summary>
        FourTwentyChan = 1,
        /// <summary>
        /// 7chan was selected to download.
        /// </summary>
        SevenChan = 2,
        /// <summary>
        /// 8chan was selected to download.
        /// </summary>
        EightChan = 3,
        /// <summary>
        /// 8kun was selected to download.
        /// </summary>
        EightKun = 4,
        /// <summary>
        /// fchan was selected to download.
        /// </summary>
        fchan = 5,
        /// <summary>
        /// u18chan was selected to download.
        /// </summary>
        u18chan = 6
    }
    #endregion

    class Networking {
        public static readonly string[] InvalidFileCharacters = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
        public static readonly string EmptyXML = "<root type=\"array\"></root>";

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string GetJsonToXml(string InputURL, DateTime ModifiedSince = default(DateTime)) {
            try {
                string JSONOutput = null;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(InputURL);
                Request.UserAgent = YChanEx.Advanced.Default.UserAgent;
                Request.Method = "GET";

                if (ModifiedSince != default(DateTime)) {
                    Request.IfModifiedSince = ModifiedSince;
                }

                using (var Response = (HttpWebResponse)Request.GetResponse())
                using (var ResponseStream = Response.GetResponseStream())
                using (var Reader = new StreamReader(ResponseStream)) {
                    string JSONString = Reader.ReadToEnd();
                    byte[] JSONBytes = Encoding.ASCII.GetBytes(JSONString);
                    using (var MemoryStream = new MemoryStream(JSONBytes)) {
                        var Quotas = new XmlDictionaryReaderQuotas();
                        var JSONReader = JsonReaderWriterFactory.CreateJsonReader(MemoryStream, Quotas);
                        var XMLJSON = XDocument.Load(JSONReader);
                        JSONOutput = XMLJSON.ToString();
                    }
                }

                GC.Collect();

                if (JSONOutput != null && JSONOutput != EmptyXML) {
                    return JSONOutput;
                }

                return null;
            }
            catch (WebException) {
                throw;
            }
            catch (Exception) {
                throw;
            }
        }

        public static bool DownloadFile(string FileURL, string Destination, string FileName) {
            try {
                if (!Directory.Exists(Destination)) {
                    Directory.CreateDirectory(Destination);
                }
                using (WebClientMethod wc = new WebClientMethod()) {
                    wc.Method = "GET";
                    wc.Headers.Add(HttpRequestHeader.UserAgent, YChanEx.Advanced.Default.UserAgent);

                    string FullFileName = Destination + "\\" + FileName;

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
            catch (WebException) {
                throw;
            }
            catch (Exception) {
                throw;
            }
        }
        public static bool DownloadFile(string FileURL, string Destination, string FileName, string RequiredCookie) {
            try {
                if (!Directory.Exists(Destination)) {
                    Directory.CreateDirectory(Destination);
                }
                using (WebClientMethod wc = new WebClientMethod()) {
                    wc.Method = "GET";
                    wc.Headers.Add(HttpRequestHeader.UserAgent, YChanEx.Advanced.Default.UserAgent);
                    wc.Headers.Add(HttpRequestHeader.Cookie, RequiredCookie);
                    string FullFileName = Destination + "\\" + FileName;

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
            catch (WebException) {
                throw;
            }
            catch (Exception) {
                throw;
            }
        }

        public static string GetAPILink(ChanType Type) {
            switch (Type) {
                case ChanType.FourChan:
                    return "https://a.4cdn.org/{0}/thread/{1}.json";
                case ChanType.FourTwentyChan:
                    return "https://api.420chan.org/{0}/res/{1}.json";
                case ChanType.EightChan:
                    return "https://8chan.moe/{0}/res/{1}.json";
                case ChanType.EightKun:
                    return "https://8kun.top/{0}/res/{1}.json";
            }
            return null;
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

    class ProgramSettings {
        public static List<int> GetColumnSizes(string[] ColumnSizesString) {
            List<int> Sizes = new List<int>();
            for (int i = 0; i < ColumnSizesString.Length; i++) {
                Sizes.Add(int.Parse(ColumnSizesString[i]));
            }
            return Sizes;
        }

        public static string GetColumnSizes(int Column, int Column2, int Column3, int Column4) {
            return Column + "|" + Column2 + "|" + Column3 + "|" + Column4;
        }

        public static bool SaveThreads(List<string> ThreadURLs, List<ThreadStatus> ThreadStatus) {
            if (General.Default.SaveQueueOnExit) {
                try {
                    string FileContentBuffer = string.Empty;
                    for (int i = 0; i < ThreadURLs.Count; i++) {
                        FileContentBuffer += ThreadURLs[i].Replace("=", "%61").Replace("|", "%124") + " = " + (int)ThreadStatus[i] + "\n";
                    }
                    FileContentBuffer = FileContentBuffer.Trim('\n');

                    File.WriteAllText(Program.ApplicationFilesLocation + "\\threads.dat", FileContentBuffer);
                    return true;
                }
                catch (Exception ex) {
                    ErrorLog.ReportException(ex);
                    return false;
                }
            }
            return false;
        }
        public static bool SaveThreads(List<string> ThreadURLs, List<ThreadStatus> ThreadStatus, List<string> ThreadNames) {
            try {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement xmlRoot = doc.DocumentElement;
                doc.InsertBefore(xmlDec, xmlRoot);
                XmlElement xmlBody = doc.CreateElement(string.Empty, "body", string.Empty);
                doc.AppendChild(xmlBody);

                if (ThreadURLs.Count != 0 && ThreadStatus.Count != 0 && ThreadNames.Count != 0) {
                    for (int i = 0; i < ThreadURLs.Count; i++) {
                        int Status = (int)ThreadStatus[i];
                        XmlElement xmlThreadParent = doc.CreateElement(string.Empty, "thread", string.Empty);
                        xmlBody.AppendChild(xmlThreadParent);

                        XmlElement xmlThreadURL = doc.CreateElement(string.Empty, "url", string.Empty);
                        XmlText xmlTextThreadURL = doc.CreateTextNode(ThreadURLs[i]);
                        xmlThreadURL.AppendChild(xmlTextThreadURL);
                        xmlThreadParent.AppendChild(xmlThreadURL);

                        XmlElement xmlThreadStatus = doc.CreateElement(string.Empty, "status", string.Empty);
                        XmlText xmlTextStatus = doc.CreateTextNode(Status.ToString());
                        xmlThreadStatus.AppendChild(xmlTextStatus);
                        xmlThreadParent.AppendChild(xmlThreadStatus);

                        XmlElement xmlThreadName = doc.CreateElement(string.Empty, "name", string.Empty);
                        XmlElement xmlCustomName = doc.CreateElement(string.Empty, "customname", string.Empty);
                        if (ThreadNames[i] != ThreadURLs[i]) {
                            XmlText xmlTextThreadName = doc.CreateTextNode(ThreadNames[i]);
                            xmlThreadName.AppendChild(xmlTextThreadName);
                            xmlThreadParent.AppendChild(xmlThreadName);

                            XmlText xmlTextCustomName = doc.CreateTextNode("true");
                            xmlCustomName.AppendChild(xmlTextCustomName);
                            xmlThreadParent.AppendChild(xmlCustomName);
                        }
                        else {
                            XmlText xmlTextThreadName = doc.CreateTextNode("");
                            xmlThreadName.AppendChild(xmlTextThreadName);
                            xmlThreadParent.AppendChild(xmlThreadName);

                            XmlText xmlTextCustomName = doc.CreateTextNode("false");
                            xmlCustomName.AppendChild(xmlTextCustomName);
                            xmlThreadParent.AppendChild(xmlCustomName);
                        }
                    }
                }

                doc.Save(Program.ApplicationFilesLocation + "\\threads.xml");
                return true;
            }
            catch (Exception) {
                throw;
            }
        }
        public static string LoadThreadsAsString() {
            try {
                if (System.IO.File.Exists(Program.ApplicationFilesLocation + "\\threads.dat")) {
                    return System.IO.File.ReadAllText(Program.ApplicationFilesLocation + "\\threads.dat").Replace("\r", "").Trim('\n');
                }
                return string.Empty;
            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return string.Empty;
            }
        }
        public static List<SavedThreadInfo> LoadThreads() {
            if (File.Exists(Program.ApplicationFilesLocation + "\\threads.xml")) {
                List<SavedThreadInfo> Threads = new List<SavedThreadInfo>();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(File.ReadAllText(Program.ApplicationFilesLocation + "\\threads.xml"));
                XmlNodeList xmlThreads = xmlDoc.DocumentElement.SelectNodes("/body/thread");

                for (int i = 0; i < xmlThreads.Count; i++) {
                    SavedThreadInfo CurrentThread = new SavedThreadInfo();
                    XmlNodeList xmlURLs = xmlThreads[i].SelectNodes("url");
                    XmlNodeList xmlStatus = xmlThreads[i].SelectNodes("status");
                    XmlNodeList xmlName = xmlThreads[i].SelectNodes("name");
                    XmlNodeList xmlCustomName = xmlThreads[i].SelectNodes("customname");

                    CurrentThread.ThreadURL = xmlURLs[0].InnerText;
                    CurrentThread.Status = (ThreadStatus)int.Parse(xmlStatus[0].InnerText);
                    if (string.IsNullOrEmpty(xmlName[0].InnerText)) {
                        CurrentThread.ThreadName = null;
                    }
                    else {
                        CurrentThread.ThreadName = xmlName[0].InnerText;
                    }
                    switch (xmlCustomName[0].InnerText) {
                        case "true":
                            CurrentThread.CustomName = true;
                            break;
                        default:
                            CurrentThread.CustomName = false;
                            break;
                    }

                    Threads.Add(CurrentThread);
                }

                return Threads;
            }
            else return null;
        }
    }

    /// <summary>
    /// This class contains methods for translating and managing chan threads.
    /// Most backend is here, except for individual chan apis and parsing.
    /// </summary>
    class Chans {
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
        public static ChanType GetChanType(string URL) {
            Regex Matcher = new Regex(ChanRegex.FourChanURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.FourChan;
            }

            Matcher = new Regex(ChanRegex.FourTwentyChanURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.FourTwentyChan;
            }

            Matcher = new Regex(ChanRegex.SevenChanURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.SevenChan;
            }

            Matcher = new Regex(ChanRegex.EightChanURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.EightChan;
            }

            Matcher = new Regex(ChanRegex.EightKunURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.EightKun;
            }

            Matcher = new Regex(ChanRegex.fchanURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.fchan;
            }

            Matcher = new Regex(ChanRegex.u18chanURL);
            if (Matcher.IsMatch(URL)) {
                return ChanType.u18chan;
            }

            return ChanType.None;
        }
    }

    /// <summary>
    /// The strings for all board titles in chans.
    /// </summary>
    class BoardTitles {
        public static string FourChan(string Board, bool OverrideRequirement = false) {
            if (General.Default.UseFullBoardNameForTitle || OverrideRequirement) {
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
            public static readonly string SevenChanPosts =
                "(?<=<a target=\"_blank\" href=\").*?( class=\"thumb\")";
            public static readonly string EightChanURL =
                "8chan.moe/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
            public static readonly string EightKunURL =
                "8kun.top/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
            public static readonly string fchanURL =
                "fchan.us/[a-zA-Z0-9]*?/res/[0-9]*.[^0-9]*";
            public static readonly string fchanFiles =
                "(?<=File: <a target=\"_blank\" href=\").*?(?=</a>)";
            public static readonly string fchanIDs =
                "(?=<img id=\"img).*?(\" src=\")";
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
        public static string SevenChanPosts {
            get {
                if (!string.IsNullOrEmpty(RegexStrings.Default.SevenChanPosts)) {
                    return RegexStrings.Default.SevenChanPosts;
                }
                else {
                    return DefaultRegex.SevenChanPosts;
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
        public static string fchanNames {
            get {
                if (!string.IsNullOrEmpty(RegexStrings.Default.fchanIDs)) {
                    return RegexStrings.Default.fchanIDs;
                }
                else {
                    return DefaultRegex.fchanFiles;
                }
            }
        }
        public static string fchanIDs {
            get {
                if (!string.IsNullOrEmpty(RegexStrings.Default.fchanIDs)) {
                    return RegexStrings.Default.fchanIDs;
                }
                else {
                    return DefaultRegex.fchanIDs;
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

}