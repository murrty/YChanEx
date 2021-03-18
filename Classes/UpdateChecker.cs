using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace YChanEx {
    class UpdateChecker {
        public static bool bypassDebug = false;
        public static GitData GitData = GitData.GetInstance();

        public static void CheckForUpdate(bool ForceCheck = false) {
            if (Program.IsDebug) {
            //    Debug.Print("-version " + GitData.UpdateVersion + " -name " + System.AppDomain.CurrentDomain.FriendlyName);
                return;
            }

            if (!General.Default.EnableUpdates && !ForceCheck) { return; }


            if (GitData.UpdateAvailable) {
                using (frmUpdateAvailable Update = new frmUpdateAvailable()) {
                    Update.BlockSkip = ForceCheck;
                    switch (Update.ShowDialog()) {
                        case DialogResult.Yes:
                            try {
                                UpdateApplication();
                            }
                            catch (Exception ex) {
                                ErrorLog.ReportException(ex);
                                return;
                            }
                            break;
                    }
                }
            }
            else {
                Thread checkUpdates = new Thread(() => {
                    if (GitData.UpdateVersion == "-1" || ForceCheck) {
                        decimal GitVersion = GetGitVersion(0);
                        if (IsUpdateAvailable(GitVersion)) {
                            GitData.UpdateAvailable = true;
                            if (GitVersion != Properties.Settings.Default.SkippedVersion || ForceCheck) {
                                using (frmUpdateAvailable Update = new frmUpdateAvailable()) {
                                    Update.BlockSkip = ForceCheck;
                                    switch (Update.ShowDialog()) {
                                        case DialogResult.Yes:
                                            try {
                                                UpdateApplication();
                                            }
                                            catch (Exception ex) {
                                                ErrorLog.ReportException(ex);
                                                return;
                                            }
                                            break;
                                        case DialogResult.Ignore:
                                            Properties.Settings.Default.SkippedVersion = GitVersion;
                                            Properties.Settings.Default.Save();
                                            break;
                                    }
                                }
                            }
                        }
                        else if (ForceCheck) {
                            MessageBox.Show("No updates available.");
                        }
                    }
                });
                checkUpdates.Name = "Check for application update";
                checkUpdates.Start();
            }
        }

        public static void UpdateApplication() {
            //var UpdaterBytes = Properties.Resources.ychanex_updater;
            //File.WriteAllBytes(Environment.CurrentDirectory + "\\ychanex-updater.exe", UpdaterBytes);

            //Process Updater = new Process();
            //Updater.StartInfo.FileName = Environment.CurrentDirectory + "\\ychanex-updater.exe";
            //string ArgumentsBuffer = "";
            //ArgumentsBuffer += "-v " + GitData.UpdateVersion + " -n " + System.AppDomain.CurrentDomain.FriendlyName;
            //Updater.StartInfo.Arguments = ArgumentsBuffer;
            //Updater.Start();
            //Environment.Exit(0);
            Process.Start("https://github.com/murrty/ychanex/releases/latest");
        }
        public static string GetGitVersionString(int GitID) {
            try {
                string xml = Networking.GetJsonToXml(string.Format(GitData.GitLinks.GithubLatestJson, GitData.GitLinks.Users[GitID], GitData.GitLinks.ApplciationNames[GitID]));

                if (xml == null)
                    return null;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList xmlTag = doc.DocumentElement.SelectNodes("/root/tag_name");

                if (GitID == 0) {
                    XmlNodeList xmlName = doc.DocumentElement.SelectNodes("/root/name");
                    XmlNodeList xmlBody = doc.DocumentElement.SelectNodes("/root/body");


                    GitData.UpdateVersion = xmlTag[0].InnerText;
                    GitData.UpdateName = xmlName[0].InnerText;
                    GitData.UpdateBody = xmlBody[0].InnerText;
                    return GitData.UpdateVersion;
                }
                else {
                    return null;
                }

            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return null;
            }
        }
        public static decimal GetGitVersion(int GitID) {
            try {
                string xml = Networking.GetJsonToXml(string.Format(GitData.GitLinks.GithubLatestJson, GitData.GitLinks.Users[GitID], GitData.GitLinks.ApplciationNames[GitID]));

                if (xml == null)
                    return -1;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList xmlTag = doc.DocumentElement.SelectNodes("/root/tag_name");

                if (GitID == 0) {
                    XmlNodeList xmlName = doc.DocumentElement.SelectNodes("/root/name");
                    XmlNodeList xmlBody = doc.DocumentElement.SelectNodes("/root/body");


                    GitData.UpdateVersion = xmlTag[0].InnerText;
                    GitData.UpdateName = xmlName[0].InnerText;
                    GitData.UpdateBody = xmlBody[0].InnerText;
                    return GitData.GitLinks.GetGitVersionDecimal(GitData.UpdateVersion);
                }
                else {
                    return -1;
                }
            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return -1;
            }
        }

        public static bool IsUpdateAvailable(decimal cloudVersion) {
            try {
                if (Properties.Settings.Default.AppVersion < cloudVersion) { return true; }
                else { return false; }
            }
            catch (Exception ex) {
                ErrorLog.ReportException(ex);
                return false;
            }
        }
    }

    public class GitData {

        public class GitLinks {
            public static readonly string GithubRawUrl = "https://raw.githubusercontent.com/{0}/{1}";
            public static readonly string GithubRepoUrl = "https://github.com/{0}/{1}";
            public static readonly string GithubIssuesUrl = "https://github.com/{0}/{1}/issues";
            public static readonly string GithubLatestJson = "http://api.github.com/repos/{0}/{1}/releases/latest";
            public static readonly string ApplicationDownloadUrl = "https://github.com/{0}/{1}/releases/download/{2}/{1}.exe";

            public static readonly string[] Users = { "murrty" };
            public static readonly string[] ApplciationNames = { "ychanex" };

            public static decimal GetGitVersionDecimal(string InputVersion) {
                return decimal.Parse(InputVersion.Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator), NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }

        private static GitData GitDataInstance = new GitData();
        private static volatile string UpdateVersionString = "-1";
        private static volatile string UpdateNameString = "UpdateNameString";
        private static volatile string UpdateBodyString = "UpdateBodyString";
        private static volatile bool UpdateAvailableBool = false;

        public static GitData GetInstance() {
            return GitDataInstance;
        }

        public string GithubIssuesPage {
            get { return string.Format(GitLinks.GithubIssuesUrl, GitLinks.Users[0], GitLinks.ApplciationNames[0]); }
        }
        public string UpdateVersion {
            get { return UpdateVersionString; }
            set { UpdateVersionString = value; }
        }
        public string UpdateName {
            get { return UpdateNameString; }
            set { UpdateNameString = value; }
        }
        public string UpdateBody {
            get { return UpdateBodyString; }
            set { UpdateBodyString = value; }
        }
        public bool UpdateAvailable {
            get { return UpdateAvailableBool; }
            set { UpdateAvailableBool = value; }
        }
    }
}
