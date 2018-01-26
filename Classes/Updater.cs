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
        public static string updateURL = "https://github.com/murrty/YChanEx/releases/download/%arg1%";
        public static string updateFile = @"\ycxu.bat";

        public static decimal getCloudVersion() {
            try {
                using (WebClient wc = new WebClient()) {
                    wc.Headers.Add("User-Agent: " + Adv.Default.UserAgent);
                    decimal clVers = decimal.Parse(Regex.Replace(wc.DownloadString(rawURL + "/master/Resources/AppVersion"), @"\s", ""));
                    if (Properties.Settings.Default.currentVersion < clVers) {
                        return clVers;
                    }
                    else { return -1; }
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

        public static void createUpdaterStub() {
            string updaterCode = "@echo off\necho ===========================================\necho //    YCHANEX UPDATER  (BAT VERSION)     \\\\\necho //      CURRENT BATCH VERSION: 1.1       \\\\\necho //  IF THIS WINDOW DOES NOT CLOSE AFTER  \\\\\necho //       YCHANEX IS FULLY UPDATED        \\\\\necho //       THEN IT IS SAFE TO CLOSE.       \\\\\necho ===========================================\nset arg1=%1\nset arg2=%2\ntimeout /t 5 /nobreak\ndel %arg2%\npowershell -Command \"(New-Object Net.WebClient).DownloadFile('" + updateURL + "/YChanEx.exe', '%arg2%')\"\n%arg2%\nexit";
            /*
             * This is the entire code for the updater, it is designed to be light-weight and so is batch-based.
             * Also, this appears in youtube-dl-gui first, but applies greatly to this program.
             
            @echo off
            echo ===========================================
            echo //    YCHANEX UPDATER  (BAT VERSION)     \\
            echo //      CURRENT BATCH VERSION: 1.1       \\
            echo //  IF THIS WINDOW DOES NOT CLOSE AFTER  \\
            echo //       YCHANEX IS FULLY UPDATED        \\
            echo //       THEN IT IS SAFE TO CLOSE.       \\
            echo ===========================================
            set arg1=%1
            set arg2=%2
            timeout /t 5 /nobreak
            del %arg2%
            powershell -Command "(New-Object Net.WebClient).DownloadFile('https://github.com/obscurename/YChanEx/releases/download/%arg1%/YChanEx.exe', '%arg2%')"
            %arg2%
            exit
             
             */
            try {
                if (File.Exists(Application.StartupPath + updateFile))
                    File.Delete(Application.StartupPath + updateFile);

                File.Create(Application.StartupPath + updateFile).Dispose();
                System.IO.StreamWriter writeApp = new System.IO.StreamWriter(Application.StartupPath + updateFile);
                writeApp.WriteLine(updaterCode);
                writeApp.Close();
            } catch (Exception ex) {
                Debug.Print(ex.ToString());
                ErrorLog.logError(ex.ToString(), "UpdaterError");
            }
        }
        public static void runUpdater(decimal updVersion) {
            try { 
                Process Updater = new Process();
                Updater.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + updateFile;
                Updater.StartInfo.Arguments = updVersion.ToString() + " " + System.AppDomain.CurrentDomain.FriendlyName;
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
