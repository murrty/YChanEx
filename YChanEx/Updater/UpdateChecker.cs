using System.Diagnostics;
using System.Windows.Forms;

namespace YChanEx {
    class UpdateChecker {
        private const string AllReleases = "https://api.github.com/repos/murrty/YChanEx/releases";
        private const string LatestRelease = "https://api.github.com/repos/murrty/YChanEx/releases/latest";
        public static bool bypassDebug = false;
        public static GithubData LastChecked { get; private set; }
        public static GithubData LastCheckedLatestRelease { get; private set; }
        public static GithubData LastCheckedAllRelease { get; private set; }

        public static bool? CheckForUpdate(bool CheckForPreRelease, bool ReCheckUpdate = false) {
            bool CanRetry = true;
            do {
                try {
                    if (ReCheckUpdate || (CheckForPreRelease ? LastCheckedAllRelease is null : LastCheckedLatestRelease is null))
                        RefreshRelease(CheckForPreRelease);

                    CanRetry = false;
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                    if (MessageBox.Show("Retry checking for update?", "YChanEx", MessageBoxButtons.YesNo) != DialogResult.No) {
                        return null;
                    }
                }
            } while (CanRetry);

            if (LastChecked is not null && LastChecked.IsNewerVersion) {
                using frmUpdateAvailable Update = new(LastChecked, ReCheckUpdate);
                switch (Update.ShowDialog()) {
                    case DialogResult.Yes: {
                        try {
                            UpdateApplication();
                        }
                        catch (Exception ex) {
                            murrty.classes.Log.ReportException(ex);
                            return true;
                        }
                    } break;

                    case DialogResult.Ignore: {
                        Config.Settings.Initialization.SkippedVersion = LastChecked.Version;
                        Config.Settings.Initialization.Save();
                    } break;
                }
                return true;
            }
            return false;
        }

        private static void RefreshRelease(bool CheckForPreRelease) { 
            try {
                string Json = Networking.DownloadString(CheckForPreRelease ? AllReleases : LatestRelease);
                if (string.IsNullOrWhiteSpace(Json)) throw new InvalidOperationException("JSON downloaded was empty");

                GithubData CurrentCheck = null;

                if (CheckForPreRelease) {
                    var Releases = Json.JsonDeserialize<GithubData[]>();
                    if (Releases.Length == 0) throw new NullReferenceException("The found releases were empty.");
                    CurrentCheck = Releases[0];
                    LastCheckedAllRelease = CurrentCheck;

                    if (CurrentCheck is null)
                        return;
                }
                else {
                    CurrentCheck = Json.JsonDeserialize<GithubData>();
                    LastCheckedLatestRelease = CurrentCheck;
                }

                if (string.IsNullOrWhiteSpace(CurrentCheck.VersionTag)) throw new InvalidOperationException("tag_name was empty");

                CurrentCheck.ParseData(Program.CurrentVersion);
                LastChecked = CurrentCheck;
            }
            catch { throw; }
        }

        public static void UpdateApplication() {
            Process.Start($"{Program.GithubPage}/releases/latest");
        }
    }
}
