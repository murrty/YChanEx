using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

#region Chan classes
#endregion


class fourChanTTT  {



}

#region Chan controllers
/// <summary>
/// This class contains methods for translating and managing threads.
/// Most backend is here, except for individual chan apis and parsing.
/// </summary>
class Chans {
    public static readonly string EmptyXML = "<root type=\"array\"></root>";
    public static readonly string[] InvalidFileCharacters = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
    public static string GetJSON(string InputURL, DateTime ModifiedSince = default(DateTime)) {
        try {
            string JSONOutput = null;
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
                wc.Headers.Add("User-Agent: " + YChanEx.Advanced.Default.UserAgent);
                if (RequireCookie) {
                    wc.Headers.Add(HttpRequestHeader.Cookie, RequiredCookie);
                }

                if (FileName.Length > 100) {
                    string OldFileName = FileName;
                    FileName = FileName.Substring(0, 100);
                    File.WriteAllText(Destination + "\\" + FileName + ".txt", OldFileName);
                }

                if (!File.Exists(Destination + "\\" + FileName)) {
                    wc.DownloadFile(FileURL, Destination + "\\" + FileName);
                }
            }

            return true;
        }
        catch (WebException WebEx) {
            //error log
            System.Windows.Forms.MessageBox.Show(WebEx.ToString());
            return false;
        }
        catch (Exception Ex) {
            //error log
            System.Windows.Forms.MessageBox.Show(Ex.ToString());
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
}
class ChanTypes {
    public enum Types : int {
        fourChan = 0,
        fourTwentyChan = 1,
        sevenChan = 2,
        eightChan = 3,
        fchan = 4,
        uEighteenChan = 5
    }

    public static int fourChan { get { return 0; } }
    public static int fourTwentyChan { get { return 1; } }
    public static int sevenChan { get { return 2; } }
    public static int eightChan { get { return 3; } }
    public static int fchan { get { return 4; } }
    public static int uEighteenChan { get { return 5; } }
}
#endregion