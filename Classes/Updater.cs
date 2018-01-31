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
    class Updater {

        public static string rawURL = "https://raw.githubusercontent.com/murrty/YChanEx";
        public static string githubURL = "https://github.com/murrty/YChanEx";
        public static string downloadURL = "https://github.com/murrty/YChanEx/releases/download/%upVersion%/YChanEx.exe";
        public static string updateFile = @"\ycxu.bat";

        public static decimal getCloudVersion() {
            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    decimal clVers = decimal.Parse(Regex.Replace(wc.DownloadString(rawURL + "/master/Resources/AppVersion"), @"\s", ""));
                    return clVers;
                }
            } catch (WebException wEx) {
                Debug.Print(wEx.ToString());
                ErrorLog.logError(wEx.ToString(), "UpdateCheckError");
                return -1;
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdateCheckError");
                return -1;
            }
        }
        public static bool isUpdateAvailable(decimal cloudVersion) {
            try {
                if (Properties.Settings.Default.currentVersion < cloudVersion)
                {
                    return true;
                } else { return false; }
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdateCheckError");
                return false;
            }
        }
        public static bool isUpdateCritical() {
            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    bool isCritical = Regex.Replace(wc.DownloadString(rawURL + "/master/Resources/AppCrit"), @"\s", "").Equals("True")? true : false;
                    if (isCritical)
                        return true;
                    else
                        return false;
                }
            } catch (WebException wEx) {
                Debug.Print(wEx.ToString());
                ErrorLog.logError(wEx.ToString(), "UpdateCheckError");
                return false;
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdateCheckError");
                return false;
            }
        }
        public static string getCriticalInformation()
        {
            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    string getCritInfo = wc.DownloadString(rawURL + "/master/Resources/CriticalInformation");
                    return getCritInfo;
                }
            } catch (WebException wEx) {
                Debug.Print(wEx.ToString());
                ErrorLog.logError(wEx.ToString(), "UpdateCheckError");
                return "Unable to get information from " + rawURL + "/master/Resources/CriticalInformation";
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdateCheckError");
                return "Unable to get information from " + rawURL + "/master/Resources/CriticalInformation";
            }
        }

        public static void createUpdaterStub(decimal updVersion) {
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
                writeApp.WriteLine("set upVersion=" + updVersion);
                writeApp.WriteLine("set programName=" + System.AppDomain.CurrentDomain.FriendlyName);
                writeApp.WriteLine("timeout /t 5 /nobreak");
                writeApp.WriteLine("del %programName%");
                writeApp.WriteLine("powershell -Command \"(New-Object Net.WebClient).DownloadFile('" + downloadURL + "', '%programName%')\"");
                writeApp.WriteLine("%programName%");
                writeApp.WriteLine("eixt");
                writeApp.Close();
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdaterError");
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
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdaterError");
                return;
            }
        }
  
    }
}
