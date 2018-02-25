﻿using System;
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

        public static string getJSON(string url) {
            try {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    string json = wc.DownloadString(url);
                    byte[] bytes = Encoding.ASCII.GetBytes(json);
                    using (var stream = new MemoryStream(bytes)) {
                        var quotas = new XmlDictionaryReaderQuotas();
                        var jsonReader = JsonReaderWriterFactory.CreateJsonReader(stream, quotas);
                        var xml = XDocument.Load(jsonReader);
                        stream.Flush();
                        stream.Close();
                        return xml.ToString();
                    }
                }
            }
            catch (WebException WebE) {
                Debug.Print(WebE.ToString());
                MessageBox.Show(WebE.ToString());
                return null;
                throw WebE;
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                MessageBox.Show(ex.ToString());
                return null;
                throw ex;
            }
        }

        public static decimal getCloudVersion() {
            try {
                string xml = getJSON(githubJSON);
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

        public static void createUpdaterStub(decimal cloudVersion) {
            /*
             * This is the entire code for the updater, it is designed to be light-weight and so is batch-based.
             
                "@echo off"
                "echo Updating YChanEx..."
                "set upVersion=" + updVersion
                "set programName=" + System.AppDomain.CurrentDomain.FriendlyName;
                "timeout /t 5 /nobreak"
                "del %programName%"
                "powershell -Command "(New-Object Net.WebClient).DownloadFile(upateURL + '/%upVersion%/YChanEx.exe', '%programName%')""
                "%programName%"
                 "exit"
             
             */

            try {
                if (File.Exists(Application.StartupPath + updateFile))
                    File.Delete(Application.StartupPath + updateFile);

                File.Create(Application.StartupPath + updateFile).Dispose();
                System.IO.StreamWriter writeApp = new System.IO.StreamWriter(Application.StartupPath + updateFile);
                writeApp.WriteLine("@echo off");
                writeApp.WriteLine("echo Updating YChanEx...");
                writeApp.WriteLine("set upVersion=" + cloudVersion.ToString());
                writeApp.WriteLine("set programName=" + System.AppDomain.CurrentDomain.FriendlyName);
                writeApp.WriteLine("timeout /t 5 /nobreak");
                writeApp.WriteLine("del %programName%");
                writeApp.WriteLine("powershell -Command \"(New-Object Net.WebClient).DownloadFile('" + downloadURL + "', '%programName%')\"");
                writeApp.WriteLine("%programName%");
                writeApp.WriteLine("eixt");
                writeApp.Close();
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.reportError(ex.ToString());
            }
        }
        public static void runUpdater() {
            try {
                Process Updater = new Process();
                Updater.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + updateFile;
                Updater.StartInfo.UseShellExecute = false;
                Updater.StartInfo.CreateNoWindow = false;
                Properties.Settings.Default.runningUpdate = true;
                Updater.Start();
                Environment.Exit(0);
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.reportError(ex.ToString());
                return;
            }
        }
    }
}