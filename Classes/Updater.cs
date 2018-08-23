using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace YChanEx {
    class Updater {

        public static string rawURL = "https://raw.githubusercontent.com/murrty/YChanEx";
        public static string githubURL = "https://github.com/murrty/YChanEx";
        public static string githubJSON = "https://api.github.com/repos/murrty/ychanex/releases/latest";
        public static string downloadURL = "https://github.com/murrty/YChanEx/releases/download/%upVersion%/YChanEx.exe";
        public static string updateFile = @"\ycxu.bat";

        public static decimal getCloudVersion() {
            try {
                string xml = string.Empty;
                HttpWebRequest requestJSON = (HttpWebRequest)WebRequest.Create(githubJSON);
                requestJSON.UserAgent = Adv.Default.UserAgent;
                requestJSON.Method = "GET";
                var reqResponse = (HttpWebResponse)requestJSON.GetResponse();
                var responseStream = reqResponse.GetResponseStream();
                string str = string.Empty;
                using (StreamReader strReader = new StreamReader(responseStream)) {
                    string json = strReader.ReadToEnd();
                    byte[] jBytes = Encoding.ASCII.GetBytes(json);
                    using (var memStream = new MemoryStream(jBytes)) {
                        var quotas = new XmlDictionaryReaderQuotas();
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(memStream, quotas);
                        var rxml = XDocument.Load(jsonReader);
                        memStream.Flush();
                        memStream.Close();
                        if (rxml.ToString() == Controller.emptyXML)
                            xml = null;
                        else
                            xml = rxml.ToString();
                    }
                }
                reqResponse.Dispose();
                responseStream.Dispose();
                if (xml == null)
                    return -1;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList xmlTag = doc.DocumentElement.SelectNodes("/root/tag_name");

                return decimal.Parse(xmlTag[0].InnerText.Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator), NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.reportError(ex.ToString());
                return -1;
            }
        }
        public static bool isUpdateAvailable(decimal cloudVersion) {
            try {
                if (Properties.Settings.Default.currentVersion < cloudVersion) 
                    return true;
                else 
                    return false;
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.reportError(ex.ToString());
                return false;
            }
        }

        public static bool downloadNewVersion(decimal cloudVersion) {
            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    wc.DownloadFile("https://github.com/murrty/YChanEx/releases/download/" + (cloudVersion) + "/YChanEx.exe", Environment.CurrentDirectory + "\\ycx.exe");
                    return true;
                }
            }
            catch (WebException webe){
                return false;
            }
            catch (Exception ex){
                return false;
            }
        }
        public static void runMerge() {
            if (File.Exists(Application.StartupPath + updateFile))
                File.Delete(Application.StartupPath + updateFile);

            File.Create(Application.StartupPath + updateFile).Dispose();
            System.IO.StreamWriter writeApp = new System.IO.StreamWriter(Application.StartupPath + updateFile);
            writeApp.WriteLine("@echo off");
            writeApp.WriteLine("set programName=" + System.AppDomain.CurrentDomain.FriendlyName);
            writeApp.WriteLine("del %programName%");
            writeApp.WriteLine("REN " + Environment.CurrentDirectory + "\\ycx.exe %programName%");
            writeApp.WriteLine("%programName%");
            writeApp.WriteLine("eixt");
            writeApp.Close();
        }
    }
}