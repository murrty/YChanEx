using System.Diagnostics;
using System.Windows.Forms;

namespace YChanEx {
    class UpdateChecker {
        public static bool bypassDebug = false;
        public static GithubData LastChecked { get; private set; }
        public static GithubData LastCheckedLatestRelease { get; private set; }
        public static GithubData LastCheckedAllRelease { get; private set; }

        public static bool CheckForUpdate(bool CheckForPreRelease, bool ReCheckUpdate = false) {
            bool CanRetry = true;
            do {
                try {
                    if (ReCheckUpdate || (CheckForPreRelease ? LastCheckedAllRelease is null : LastCheckedLatestRelease is null))
                        RefreshRelease(CheckForPreRelease);
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                    CanRetry = MessageBox.Show("Retry checking for update?", "YChanEx", MessageBoxButtons.YesNo) != DialogResult.No;
                }
            } while (CanRetry);

            if (LastChecked is not null && LastChecked.IsNewerVersion) {
                using frmUpdateAvailable Update = new();
                Update.BlockSkip = ReCheckUpdate;
                switch (Update.ShowDialog()) {
                    case DialogResult.Yes:
                        try {
                            UpdateApplication();
                        }
                        catch (Exception ex) {
                            murrty.classes.Log.ReportException(ex);
                            return true;
                        }
                        break;
                }
                return true;
            }
            return false;
        }

        private static void RefreshRelease(bool CheckForPreRelease) {

        }

        public static void UpdateApplication() {
            Process.Start($"{Program.GithubPage}/releases/latest");
        }
    }
}
