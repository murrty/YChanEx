using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
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
        NoStatusSet = -1,

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
        /// The thread is not allowed to view the content.
        /// </summary>
        ThreadIsNotAllowed = 5,
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
        Unsupported = -1,
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

        public static string GetJsonToXml(string InputURL, DateTime ModifiedSince = default) {
            try {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(InputURL);
                Request.UserAgent = Config.Settings.Advanced.UserAgent;
                Request.Method = "GET";
                if (ModifiedSince != default) {
                    Request.IfModifiedSince = ModifiedSince;
                }
                using HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                using Stream ResponseStream = Response.GetResponseStream();
                using StreamReader Reader = new(ResponseStream);
                string JSONString = Reader.ReadToEnd();

                ApiHandler.ConvertJsonToXml(JSONString, out string JSONOutput);
                GC.Collect();
                return JSONOutput;
            }
            catch (WebException) {
                throw;
            }
            catch (Exception) {
                throw;
            }
        }

        public static bool DownloadFile(Uri FileURL, string Destination, string FileName, string RequiredCookie = null) {
            try {
                if (!Directory.Exists(Destination)) {
                    Directory.CreateDirectory(Destination);
                }
                using murrty.classcontrols.ExtendedWebClient wc = new();
                wc.Method = murrty.classcontrols.HttpMethod.GET;
                wc.UserAgent = Config.Settings.Advanced.UserAgent;
                if (RequiredCookie != null) {
                    wc.AddCookie(RequiredCookie.Split('=')[0], RequiredCookie[(RequiredCookie.IndexOf('=') + 1)..], FileURL.Host);
                }

                string FullFileName = Destination + "\\" + FileName;

                if (FullFileName.Length > 255 && !Config.Settings.Downloads.AllowFileNamesGreaterThan255) {
                    string FileExtension = FileName.Split('.')[^1];
                    string OldFileName = FileName;
                    FileName = FullFileName[..(255 - FileExtension.Length - 1)];
                    File.WriteAllText(Destination + "\\" + FileName + ".txt", OldFileName);

                    FullFileName = Destination + "\\" + FileName;
                }

                if (!File.Exists(FullFileName)) {
                    wc.DownloadFile(FileURL, FullFileName);
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
            return Type switch {
                ChanType.FourChan => "https://a.4cdn.org/{0}/thread/{1}.json",
                ChanType.FourTwentyChan => "https://api.420chan.org/{0}/res/{1}.json",
                ChanType.EightChan => "https://8chan.moe/{0}/res/{1}.json",
                ChanType.EightKun => "https://8kun.top/{0}/res/{1}.json",
                _ => null
            };
        }

        public static string CleanURL(string URL) {
            string NewURL = string.Empty;
            if (URL.StartsWith("http://")) {
                NewURL = "https://" + URL[7..];
            }
            if (NewURL.StartsWith("https://www.")) {
                NewURL = "https://" + NewURL[12..];
            }
            return NewURL;
        }

    }

    /// <summary>
    /// Contains usability methods to handle API things.
    /// </summary>
    class ApiHandler {

        public const string EmptyXML = "<root type=\"array\"></root>";

        public static bool ConvertJsonToXml(string Json, out string Xml) {
            try {
                Xml = null;
                using MemoryStream MemoryStream = new(Encoding.ASCII.GetBytes(Json));
                Xml = XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(MemoryStream, new())).ToString();
                MemoryStream.Flush();
                MemoryStream.Dispose();
                return Xml != EmptyXML && !string.IsNullOrWhiteSpace(Xml);
            }
            catch { throw; }
        }

        public static string CleanURL(string URL) {
            if (URL.StartsWith("http://")) {
                URL = URL[7..];
            }
            if (URL.StartsWith("www.")) {
                URL = URL[4..];
            }
            if (URL.StartsWith("fchan.us")) {
                URL = "http://" + URL;
            }
            else {
                if (!URL.StartsWith("https://")) {
                    URL = "https://" + URL;
                }
            }
            return URL;
        }
    }

    /// <summary>
    /// Contains usability methods governing local files.
    /// </summary>
    class FileHandler {

        /// <summary>
        /// The Dictionary of illegal file name characters.
        /// </summary>
        private static readonly Dictionary<string, string> IllegalCharacters = new() {
            { "\\", "_" },
            { "/",  "_" },
            { ":",  "_" },
            { "*",  "_" },
            { "?",  "_" },
            { "\"", "_" },
            { "<",  "_" },
            { ">",  "_" },
            { "|",  "_" }
        };

        /// <summary>
        /// Replaces the illegal file name characters in a string.
        /// </summary>
        /// <param name="Input">The string to replace bad characters.</param>
        /// <returns>The string with the illegal characters filtered out.</returns>
        public static string ReplaceIllegalCharacters(string Input) {
            return IllegalCharacters.Aggregate(Input, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
        }

        /// <summary>
        /// Replaces the illegal file name characters in a string.
        /// </summary>
        /// <param name="Input">The string to replace bad characters.</param>
        /// <param name="ReplacementCharacter">The <see cref="string"/> replacement character to replace it with.</param>
        /// <returns>The string with the illegal characters filtered out.</returns>
        public static string ReplaceIllegalCharacters(string Input, string ReplacementCharacter) {
            return IllegalCharacters.Aggregate(Input, (current, replacement) => current.Replace(replacement.Key, ReplacementCharacter));
        }

        /// <summary>
        /// Return the name of the file, and the extension of the file.
        /// </summary>
        /// <param name="Input">The <see cref="string"/> input to parse.</param>
        /// <param name="FileName">The <see cref="string"/> output name of the file.</param>
        /// <param name="FileExt">The <see cref="string"/> output extension of the file.</param>
        /// <returns>If <paramref name="Input"/> was parsed properly. True regardless, because there's no reason not to.</returns>
        public static bool StripFileNameAndExtension(string Input, out string FileName, out string FileExt) {
            try {
                while (Input.Contains("\\")) Input = Input[(Input.IndexOf("\\") + 1)..];
                FileName = Input[..Input.IndexOf(".")];
                if (Input.Contains(".")) FileExt = Input.Split('.')[^1];
                else FileExt = string.Empty;
                return true;
            }
            catch { throw; }
        }

        /// <summary>
        /// Generates a short 64-char thread name for the title and main form for easier identification.
        /// </summary>
        /// <param name="Subtitle">The subtitle of the post, IE the main thing about it. Optional.</param>
        /// <param name="Comment">The main post text, containing the message in the post.</param>
        /// <param name="FallbackName">The fallback name that will be used if the new name isn't a usable name.</param>
        /// <returns>If either the subtitle or comment contain text, returns them either grouped or solo; otherwise, the thread ID.</returns>
        public static string GetShortThreadName(string Subtitle, string Comment, string FallbackName) {
            string NewName = string.Empty;

            if (string.IsNullOrEmpty(FallbackName)) {
                FallbackName = "No fallback name";
            }

            if (Subtitle is not null) {
                NewName = Subtitle = Subtitle.Trim();
            }

            if (Comment is not null) {
                NewName += (Subtitle is not null && Subtitle.Length > 0 && Comment.Length > 0 ? " - " : "") + Comment.Trim();
            }

            if (NewName.Length > 0) {
                NewName = NewName
                    .Replace("<br><br>", " ") // New lines
                    .Replace("<br>", " ")
                    .Replace("<wbr>", "") // Weird inserts between URLs
                    .Replace("<span class=\"quote\">", "") // >implying text xd
                    .Replace("</span>", "") // close of >implying text xd
                    //.Replace("&gt;", ">")  // These are fixed by WebUtility.HtmlDecode.
                    //.Replace("&lt;", "<")  // But I'm keeping them commented.
                    //.Replace("&amp;", "&") // Just in case.
                    .Replace("</a>", "") // the end of any quote-link urls.
                    .Trim(); // Cleans up any trailing spaces, new-line and the windows \n, too.

                NewName = Regex.Replace(NewName, "<a href=\\\"(.*?)\\\" class=\\\"quotelink\\\">", "");
                NewName = WebUtility.HtmlDecode(NewName);

                if (NewName.Length > 64) {
                    NewName = NewName[..64];
                }

                NewName = NewName.Trim();
            }

            return NewName.Length > 0 ? NewName : FallbackName;
        }

    }

    /// <summary>
    /// Contains usability methods governing the application events.
    /// </summary>
    class ProgramSettings {

        public static List<int> GetColumnSizes(string ColumnSizesString) {
            string[] KnownSizes =
                ColumnSizesString.Contains('|') ? ColumnSizesString.Split('|') : (ColumnSizesString.Contains(',') ? ColumnSizesString.Split(',') : new string[] { });
            List<int> Sizes = new();
            if (KnownSizes.Length == 4) {
                for (int i = 0; i < 4; i++) {
                    if (int.TryParse(KnownSizes[i], out int Size)) {
                        Sizes.Add(Size);
                    }
                }
            }
            return Sizes;
        }

        public static string GetColumnSizes(int Column, int Column2, int Column3, int Column4) =>
            Column + "," + Column2 + "," + Column3 + "," + Column4;

        public static bool SaveThreads(List<ThreadInfo> ThreadInfo){
            try {
                XmlDocument doc = new();
                XmlDeclaration xmlDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement xmlRoot = doc.DocumentElement;
                doc.InsertBefore(xmlDec, xmlRoot);
                XmlElement xmlBody = doc.CreateElement(string.Empty, "body", string.Empty);
                doc.AppendChild(xmlBody);

                if (ThreadInfo.Count > 0) {
                    for (int i = 0; i < ThreadInfo.Count; i++) {
                        XmlElement xmlThreadParent = doc.CreateElement(string.Empty, "thread", string.Empty);
                        xmlBody.AppendChild(xmlThreadParent);

                        XmlElement xmlThreadURL = doc.CreateElement(string.Empty, "url", string.Empty);
                        XmlText xmlTextThreadURL = doc.CreateTextNode(ThreadInfo[i].ThreadURL);
                        xmlThreadURL.AppendChild(xmlTextThreadURL);
                        xmlThreadParent.AppendChild(xmlThreadURL);


                        int Status = (int)ThreadInfo[i].OverallStatus;
                        XmlElement xmlOverallStatus = doc.CreateElement(string.Empty, "overallstatus", string.Empty);
                        XmlText xmlTextOverallStatus = doc.CreateTextNode(Status.ToString());
                        xmlOverallStatus.AppendChild(xmlTextOverallStatus);
                        xmlThreadParent.AppendChild(xmlOverallStatus);


                        XmlElement xmlGotThreadName = doc.CreateElement(string.Empty, "gotthreadname", string.Empty);
                        XmlText xmlTextGotThreadName;
                        XmlElement xmlThreadName = doc.CreateElement(string.Empty, "threadname", string.Empty);
                        if (ThreadInfo[i].RetrievedThreadName) {
                            xmlTextGotThreadName = doc.CreateTextNode("true");
                            XmlText xmlTextThreadName = doc.CreateTextNode(ThreadInfo[i].ThreadName);
                            xmlThreadName.AppendChild(xmlTextThreadName);
                        }
                        else {
                            xmlTextGotThreadName = doc.CreateTextNode("false");
                        }
                        xmlGotThreadName.AppendChild(xmlTextGotThreadName);
                        xmlThreadParent.AppendChild(xmlGotThreadName);
                        xmlThreadParent.AppendChild(xmlThreadName);


                        XmlElement xmlSetCustomName = doc.CreateElement(string.Empty, "setcustomname", string.Empty);
                        XmlElement xmlCustomName = doc.CreateElement(string.Empty, "customname", string.Empty);
                        if (ThreadInfo[i].SetCustomName) {
                            XmlText xmlTextUseCustomName = doc.CreateTextNode("true");
                            xmlSetCustomName.AppendChild(xmlTextUseCustomName);
                            XmlText xmlTextCustomName = doc.CreateTextNode(ThreadInfo[i].CustomName);
                            xmlCustomName.AppendChild(xmlTextCustomName);
                        }
                        else {
                            XmlText xmlTextUseCustomName = doc.CreateTextNode("false");
                            xmlSetCustomName.AppendChild(xmlTextUseCustomName);
                        }
                        xmlThreadParent.AppendChild(xmlSetCustomName);
                        xmlThreadParent.AppendChild(xmlCustomName);

                        // Save all information about the thread in it's own file?

                        //if (ThreadInfo[i].FileIDs.Count > 0) {
                        //    XmlElement xmlFilesRoot = doc.CreateElement(string.Empty, "files", string.Empty);
                        //    xmlThreadParent.AppendChild(xmlFilesRoot);
                        //    for (int j = 0; j < ThreadInfo[i].FileIDs.Count; j++) {
                        //        XmlElement xmlPost = doc.CreateElement(string.Empty, "file", string.Empty);

                        //        XmlElement xmlID = doc.CreateElement(string.Empty, "id", string.Empty);
                        //        XmlText xmlTextID = doc.CreateTextNode(ThreadInfo[i].FileIDs[j]);
                        //        xmlID.AppendChild(xmlTextID);
                        //        xmlPost.AppendChild(xmlID);

                        //        XmlElement xmlExtension = doc.CreateElement(string.Empty, "extension", string.Empty);
                        //        XmlText xmlTextExtension = doc.CreateTextNode(ThreadInfo[i].FileExtensions[j]);
                        //        xmlExtension.AppendChild(xmlTextExtension);
                        //        xmlPost.AppendChild(xmlExtension);

                        //        XmlElement xmlOriginalName = doc.CreateElement(string.Empty, "originalname", string.Empty);
                        //        XmlText xmlTextOriginalName = doc.CreateTextNode(ThreadInfo[i].FileOriginalNames[j]);
                        //        xmlOriginalName.AppendChild(xmlTextOriginalName);
                        //        xmlPost.AppendChild(xmlOriginalName);

                        //        XmlElement xmlHash = doc.CreateElement(string.Empty, "hash", string.Empty);
                        //        XmlText xmlTextHash = doc.CreateTextNode(ThreadInfo[i].FileHashes[j]);
                        //        xmlHash.AppendChild(xmlTextHash);
                        //        xmlPost.AppendChild(xmlHash);

                        //        xmlFilesRoot.AppendChild(xmlPost);
                        //    }
                        //}
                    }
                }

                if (!Directory.Exists(Path.GetDirectoryName(Config.Settings.SavedThreadsPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(Config.Settings.SavedThreadsPath));

                doc.Save(Config.Settings.SavedThreadsPath);
                return true;
            }
            catch { throw; }
        }

        public static List<SavedThreadInfo> LoadThreads() {
            if (File.Exists(Config.Settings.SavedThreadsPath)) {
                List<SavedThreadInfo> Threads = new();
                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(File.ReadAllText(Config.Settings.SavedThreadsPath));
                XmlNodeList xmlThreads = xmlDoc.DocumentElement.SelectNodes("/body/thread");

                for (int i = 0; i < xmlThreads.Count; i++) {
                    SavedThreadInfo CurrentThread = new();
                    XmlNodeList xmlURLs = xmlThreads[i].SelectNodes("url");
                    XmlNodeList xmlOverallStatus = xmlThreads[i].SelectNodes("overallstatus");
                    XmlNodeList xmlGotThreadName = xmlThreads[i].SelectNodes("gotthreadname");
                    XmlNodeList xmlThreadName = xmlThreads[i].SelectNodes("threadname");
                    XmlNodeList xmlSetCustomName = xmlThreads[i].SelectNodes("setcustomname");
                    XmlNodeList xmlCustomName = xmlThreads[i].SelectNodes("customname");

                    CurrentThread.ThreadURL = xmlURLs[0].InnerText;
                    CurrentThread.Status = (ThreadStatus)int.Parse(xmlOverallStatus[0].InnerText);
                    switch (xmlGotThreadName[0].InnerText) {
                        case "true":
                            CurrentThread.RetrievedThreadName = true;
                            if (xmlThreadName.Count > 0) {
                                CurrentThread.ThreadName = WebUtility.HtmlDecode(xmlThreadName[0].InnerText);
                            }
                            else {
                                CurrentThread.ThreadName = string.Empty;
                            }
                            break;
                    }

                    switch (xmlSetCustomName[0].InnerText) {
                        case "true":
                            CurrentThread.SetCustomName = true;
                            if (xmlCustomName.Count > 0) {
                                CurrentThread.CustomName = xmlCustomName[0].InnerText;
                            }
                            else {
                                CurrentThread.CustomName = string.Empty;
                            }
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

        /// <summary>
        /// Class containing default regular expression patterns that was last known to work.
        /// </summary>
        internal class DefaultRegex {
            public const string FourChanURL =
                "(boards.)?4chan(nel)?.org\\/[a-zA-Z0-9]*?\\/thread[0-9]*";

            public const string FourTwentyChanURL =
                "boards.420chan.org\\/[a-zA-Z0-9]*?\\/thread/[0-9]*";

            public const string SevenChanURL =
                "7chan.org\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

            public const string SevenChanPosts =
                "(?<=<a target=\"_blank\" href=\").*?( class=\"thumb\")";

            public const string SevenChanHtmlMonkeyPosts =
                "div[class:=\"post\"][id:=\"^([0-9]+)\"]";

            public const string EightChanURL =
                "8chan[.moe|.se|.cc]+\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

            public const string EightKunURL =
                "8kun.top\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

            public const string fchanURL =
                "fchan.us\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

            public const string fchanFiles =
                "(?<=File: <a target=\"_blank\" href=\").*?(?=</a>)";

            public const string fchanIDs =
                "(?=<img id=\"img).*?(\" src=\")";

            public const string u18chanURL =
                "u18chan.com\\/(.*?)[a-zA-Z0-9]*?\\/topic\\/[0-9]*";

            public const string u18chanPosts =
                "(?<=a href=\").*?(_image\" style=\"width: )";
        }

        /// <summary>
        /// Whether the input <paramref name="URL"/> is supported by the program, with <paramref name="Type"/> as the output <see cref="ChanType"/> if true.
        /// </summary>
        /// <param name="URL">The URL to the thread that is requested to be parsed.</param>
        /// <param name="Type">The out-ChanType chan of the url.</param>
        /// <returns></returns>
        public static bool SupportedChan(string URL, out ChanType Type) {
            Regex Matcher = new(string.IsNullOrWhiteSpace(Config.Settings.Regex.FourChanURL) ? DefaultRegex.FourChanURL : Config.Settings.Regex.FourChanURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.FourChan;
                return true;
            }

            Matcher = new Regex(string.IsNullOrWhiteSpace(Config.Settings.Regex.FourTwentyChanURL) ? DefaultRegex.FourTwentyChanURL : Config.Settings.Regex.FourTwentyChanURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.FourTwentyChan;
                return true;
            }

            Matcher = new Regex(string.IsNullOrWhiteSpace(Config.Settings.Regex.SevenChanURL) ? DefaultRegex.SevenChanURL : Config.Settings.Regex.SevenChanURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.SevenChan;
                return true;
            }

            Matcher = new Regex(string.IsNullOrWhiteSpace(Config.Settings.Regex.EightChanURL) ? DefaultRegex.EightChanURL : Config.Settings.Regex.EightChanURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.EightChan;
                return true;
            }

            Matcher = new Regex(string.IsNullOrWhiteSpace(Config.Settings.Regex.EightKunURL) ? DefaultRegex.EightKunURL : Config.Settings.Regex.EightKunURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.EightKun;
                return true;
            }

            Matcher = new Regex(string.IsNullOrWhiteSpace(Config.Settings.Regex.fchanURL) ? DefaultRegex.fchanURL : Config.Settings.Regex.fchanURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.fchan;
                return true;
            }

            Matcher = new Regex(string.IsNullOrWhiteSpace(Config.Settings.Regex.u18chanURL) ? DefaultRegex.u18chanURL : Config.Settings.Regex.u18chanURL);
            if (Matcher.IsMatch(URL)) {
                Type = ChanType.u18chan;
                return true;
            }

            Type = ChanType.Unsupported;
            return false;
        }

        /// <summary>
        /// Gets the board subtitle. Usually a disclaimer.
        /// </summary>
        /// <param name="Chan"></param>
        /// <param name="Board"></param>
        /// <returns></returns>
        public static string GetBoardSubtitle(ChanType Chan, string Board) {
            return Chan switch {

                ChanType.FourChan => Board.ToLower() switch {
                    "trash" or
                    "b" => "The stories and information posted here are artistic works of fiction and falsehood.<br>Only a fool would take anything posted here as fact.",
                    
                    _ => "The content archived here is not owned or endorsed by 4chan or YChanEx.",
                },

                ChanType.FourTwentyChan => "The content archived here is not owned or endorsed by 420chan or YChanEx.",

                ChanType.SevenChan => "The content archived here is not owned or endorsed by 7chan or YChanEx.",

                ChanType.EightChan => "The content archived here is not owned or endorsed by 8chan or YChanEx.",

                ChanType.EightKun => "The content archived here is not owned or endorsed by 8kun or YChanEx.",

                ChanType.fchan => "The content archived here is not owned or endorsed by fchan or YChanEx.",

                ChanType.u18chan => "The content archived here is not owned or endorsed by u18chan or YChanEx.",

                _ => string.Empty,
            };
        }

        /// <summary>
        /// Gets the full chan title from the board id.
        /// </summary>
        /// <param name="Chan">The <see cref="ChanType"/> to parse the name from.</param>
        /// <param name="Board">The board (or extra info) to parse from.</param>
        /// <param name="OverrideOrDescription">If it should override the settings check for HTML, or if it's obtaining the info from the description</param>
        /// <returns>The string value of the title. If none is parse, it'll return the input board.</returns>
        public static string GetFullBoardName(ChanType Chan, string Board, bool OverrideOrDescription = false) {
            switch (Chan) {

                case ChanType.FourChan: {
                    if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                        return Board.ToLower() switch {

                            #region Japanese Culture
                            "a"   => "Anime & Manga",
                            "c"   => "Anime/Cute",
                            "w"   => "Anime/Wallpapers",
                            "m"   => "Mecha",
                            "cgl" => "Cosplay & EGL",
                            "cm"  => "Cute/Male",
                            "f"   => "Flash",
                            "n"   => "Transportation",
                            "jp"  => "Otaku Culture",
                            #endregion

                            #region Video Games
                            "v"    => "Video Games",
                            "vrpg" => "Video Games/RPG",
                            "vmg"  => "Video Games/Mobile",
                            "vst"  => "Video Games/Strategy",
                            "vm"   => "Video Games/Multiplayer",
                            "vg"   => "Video Game Generals",
                            "vp"   => "Pokémon",
                            "vr"   => "Retro Games",
                            #endregion

                            #region Interests
                            "co"  => "Comics & Cartoons",
                            "g"   => "Technology",
                            "tv"  => "Television & Film",
                            "k"   => "Weapons",
                            "o"   => "Auto",
                            "an"  => "Animals & Nature",
                            "tg"  => "Traditional Games",
                            "sp"  => "Sports",
                            "xs"  => "Extreme Sports",
                            "pw"  => "Professional Wrestling",
                            "asp" => "Alternative Sports",
                            "sci" => "Science & Math",
                            "his" => "History & Humanities",
                            "int" => "International",
                            "out" => "Outdoors",
                            "toy" => "Toys",

                            #endregion

                            #region Creative
                            "i"   => "Oekaki",
                            "po"  => "Papercraft & Origami",
                            "p"   => "Photography",
                            "ck"  => "Food & Cooking",
                            "ic"  => "Artwork/Critique",
                            "wg"  => "Wallpapers/General",
                            "lit" => "Literature",
                            "mu"  => "Music",
                            "fa"  => "Fashion",
                            "3"   => "3DCG",
                            "gd"  => "Graphic Design",
                            "diy" => "Do It Yourself",
                            "wsg" => "Worksafe GIF",
                            "qst" => "Quests",
                            #endregion

                            #region Other
                            "biz"  => "Business & Finance",
                            "trv"  => "Travel",
                            "fit"  => "Fitness",
                            "x"    => "Paranormal",
                            "adv"  => "Advice",
                            "lgbt" => "Lesbian, Gay, Bisexual, & Transgender",
                            "mlp"  => "My Little Pony", // disgusting.
                            "news" => "Current News",
                            "wsr"  => "Worksafe Requests",
                            "vip"  => "Very Important Posts",
                            #endregion

                            #region Misc
                            "b"    => "Random",
                            "r9k"  => "ROBOT9001",
                            "pol"  => "Politically Incorrect",
                            "bant" => "International/Random",
                            "soc"  => "Cams & Meetups",
                            "s4s"  => "Shit 4chan Says",
                            #endregion

                            #region Adult
                            "s"   => "Sexy Beautiful Women",
                            "hc"  => "Hardcore",
                            "hm"  => "Handsome Men",
                            "h"   => "Hentai",
                            "e"   => "Ecchi",
                            "u"   => "Yuri",
                            "d"   => "Hentai/Alternative",
                            "y"   => "Yaoi",
                            "t"   => "Torrents",
                            "hr"  => "High Resolution",
                            "gif" => "Adult GIF",
                            "aco" => "Adult Cartoons",
                            "r"   => "Adult Requests",
                            #endregion

                            #region Unlisted
                            "trash" => "Off-Topic",
                            "qa"    => "Question & Answer",
                            #endregion

                            _ => $"{Board} (Unknown board)"

                        };
                    }
                    else return Board;
                }

                case ChanType.FourTwentyChan: {
                    if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                        return Board.ToLower() switch {

                            #region Drugs
                            "weed" => "Cannabis Discussion",
                            "hooch" => "Alcohol Discussion",
                            "mdma" => "Ecstasy Discussion",
                            "psy" => "Psychedelic Discussion",
                            "stim" => "Stimulant Discussion",
                            "dis" => "Dissociative Discussion",
                            "opi" => "Opiate Discussion",
                            "vape" => "Vaping Discussion",
                            "tobacco" => "Tobacco Discussion",
                            "benz" => "Benzo Discussion",
                            "deli" => "Deliriant Discussion",
                            "other" => "Other Drugs Discussion",
                            "jenk" => "Jenkem Discussion",
                            "detox" => "Detoxing & Rehabilitation",
                            #endregion

                            #region Lifestye
                            "qq" => "Personal Issues",
                            "dr" => "Dream Discussion",
                            "ana" => "Fitness",
                            "nom" => "Food, Munchies & Cooking",
                            "vroom" => "Travel & Transportation",
                            "st" => "Style & Fashion",
                            "nra" => "Weapons Discussion",
                            "sd" => "Sexuality Discussion",
                            "cd" => "Transgender Discussion",
                            #endregion

                            #region Academia
                            "art" => "Art & Okekai",
                            "sagan" => "Space... the Final Frontier",
                            "lang" => "World Languages",
                            "stem" => "Science, Technology, Engineering & Mathematics",
                            "his" => "History Discussion",
                            "crops" => "Growing & Botany",
                            "howto" => "Guides & Tutorials",
                            "law" => "Law Discussion",
                            "lit" => "Books & Literature",
                            "med" => "Medicine & Health",
                            "pss" => "Philosophy & Social Sciences",
                            "tech" => "Computers & Tech Support",
                            "prog" => "Programming",
                            #endregion

                            #region Media
                            "1701" => "Star Trek Discussion",
                            "sport" => "Sports",
                            "mtv" => "Movies & Television",
                            "f" => "Flash",
                            "m" => "Music & Production",
                            "mma" => "Mixed Martial Arts Discussion",
                            "616" => "Comics & Web Comics Discussion",
                            "a" => "Anime & Manga Discussion",
                            "wooo" => "Professional Wrestling Discussion",
                            "n" => "World News",
                            "vg" => "Video Games Discussion",
                            "po" => "Pokémon Discussion",
                            "tg" => "Traditional Games",
                            #endregion

                            #region Miscellanea
                            "420" => "420chan Discussion & Staff Interaction",
                            "b" => "Random & High Stuff",
                            "spooky" => "Paranormal Discussion",
                            "dino" => "Dinosaur Discussion",
                            "fo" => "Post-apocalyptic",
                            "ani" => "Animal Discussion",
                            "nj" => "Netjester AI Conversation Chamber",
                            "nc" => "Net Characters",
                            "tinfoil" => "Conspiracy Theories",
                            "w" => "Dumb Wallpapers Below",
                            #endregion

                            #region Adult
                            "h" => "Hentai",
                            #endregion

                            _ => $"{Board} (Unknown board)"

                        };
                    }
                    else return Board;
                }

                case ChanType.SevenChan: {
                    if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                        return Board.ToLower() switch {

                            #region 7chan & Related services
                            "7ch" => "Site Discussion",
                            "ch7" => "Channel7 & Radio 7",
                            "irc" => "Internet Relay Circlejerk",
                            #endregion

                            #region VIP
                            "777" => "gardening",
                            "VIP" => "Very Important Posters",
                            "civ" => "Civics",
                            "vip6" => "IPv6 for VIP",
                            #endregion

                            #region Premium Content
                            "b" => "Random",
                            "banner" => "Banners",
                            "f" => "Flash",
                            "gfc" => "Grahpics Manipulation",
                            "fail" => "Failure",
                            #endregion

                            #region SFW
                            "class" => "The Finer Things",
                            "co" => "Comics and Cartoons",
                            "eh" => "Particularly uninteresting conversation",
                            "fit" => "Fitness & Health",
                            "halp" => "Technical Support",
                            "jew" => "Thrifty Living",
                            "lit" => "Literature",
                            "phi" => "Philosophy",
                            "pr" => "Programming",
                            "rnb" => "Rage and Baww",
                            "sci" => "Science, Technology, Engineering, and Mathematics",
                            "tg" => "Tabletop Games",
                            "w" => "Weapons",
                            "zom" => "Zombies",
                            #endregion

                            #region General
                            "a" => "Anime & Manga",
                            "grim" => "Cold, Grim & Miserable",
                            "hi" => "History and Culture",
                            "me" => "Film, Music & Television",
                            "rx" => "Drugs",
                            "vg" => "Video Games",
                            "wp" => "Wallpapers",
                            "x" => "Paranormal & Conspiracy",
                            #endregion

                            #region Porn
                            "cake" => "Delicious",
                            "cd" => "Crossdressing",
                            "d" => "Alternative Hentai",
                            "di" => "Sexy Beautiful Traps",
                            "elit" => "Erotic Literature",
                            "fag" => "Men Discussion",
                            "fur" => "Furry",
                            "gif" => "Animated GIFs",
                            "h" => "Hentai",
                            "men" => "Sexy Beautiful Men",
                            "pco" => "Porn Comics",
                            "s" => "Sexy Beautiful Women",
                            "sm" => "Shotacon",
                            "ss" => "Straight Shotacon",
                            "unf" => "Uniforms",
                            "v" => "The Vineyard",
                            #endregion

                            _ => $"{Board} (Unknown board)"

                        };
                    }
                    else return Board;
                }

                case ChanType.EightChan: {
                    if (OverrideOrDescription) {
                        return Board.Replace("<p id=\"labelDescription\">", "").Replace("</p>", "");
                    }
                    else {
                        //if (General.Default.UseFullBoardNameForTitle) {
                        //return BoardOrDescription.Replace("<p id=\"labelName\">", "").Replace("</p>", "").Replace("/ - ", "|").Split('|')[1];
                        //}
                        //else {
                        return Board;
                        //}
                    }
                }

                case ChanType.EightKun: {
                    if (OverrideOrDescription) {
                        return Board.Replace("<div class=\"subtitle\">", "").Replace("<p>", "");
                    }
                    else {
                        if (Config.Settings.General.UseFullBoardNameForTitle) {
                            //return BoardOrDescription.Replace("<h1>", "").Replace("</h1>", "").Replace("/ - ", "|").Split('|')[1];
                            return Board;
                        }
                        else {
                            return Board;
                        }
                    }
                }

                case ChanType.fchan: {
                    if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                        return Board.ToLower() switch {

                            #region Normal image boards
                            "f" => "female",
                            "m" => "male",
                            "h" => "herm",
                            "s" => "straight",
                            "toon" => "toon",
                            "a" => "alternative",
                            "ah" => "alternative (hard)",
                            "c" => "clean",
                            #endregion

                            #region Specialized image boards
                            "artist" => "artist",
                            "crit" => "critique",
                            "b" => "banners",
                            #endregion

                            _ => $"{Board} (Unknown board)"
                        };
                    }
                    else return Board;
                }

                case ChanType.u18chan: {
                    if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                        return Board.ToLower() switch {

                            #region Furry Related
                            "fur" => "Furries",
                            "c" => "Furry Comics",
                            "gfur" => "Gay Furries",
                            "gc" => "Gay Furry Comics",
                            "i" => "Intersex",
                            "rs" => "Request & Source",
                            "a" => "Animated",
                            "cute" => "Cute",
                            #endregion

                            #region The Basement
                            "pb" => "Post Your Naked Body",
                            "p" => "Ponies", // Why, honestly, WHY?
                            "f" => "Feral",
                            "cub" => "Cub",
                            "gore" => "Gore",
                            #endregion

                            #region General
                            "d" => "Discussion",
                            "mu" => "Music",
                            "w" => "Wallpapers",
                            "v" => "Video Games",
                            "lo" => "Lounge",
                            "tech" => "Technology",
                            "lit" => "Literature",
                            #endregion

                            _ => $"{Board} (Unknown board)"

                        };
                    }
                    else return Board;
                }

                default: return Board;

            }
        }
    }

    /// <summary>
    /// The Regex strings for detecting the chans.
    /// </summary>
    class ChanRegex {
        public static string SevenChanPosts =>
            string.IsNullOrWhiteSpace(Config.Settings.Regex.SevenChanPosts) ? Chans.DefaultRegex.SevenChanPosts : Config.Settings.Regex.SevenChanPosts;
        public static string fchanNames =>
            string.IsNullOrWhiteSpace(Config.Settings.Regex.fchanIDs) ? Chans.DefaultRegex.fchanFiles : Config.Settings.Regex.fchanIDs;
        public static string fchanIDs =>
            string.IsNullOrWhiteSpace(Config.Settings.Regex.fchanIDs) ? Chans.DefaultRegex.fchanIDs : Config.Settings.Regex.fchanIDs;
        public static string u18chanPosts =>
            string.IsNullOrWhiteSpace(Config.Settings.Regex.u18chanPosts) ? Chans.DefaultRegex.u18chanPosts : Config.Settings.Regex.u18chanPosts;
    }
}