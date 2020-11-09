using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YChanEx {
    static class Program {
        static frmMain MainForm;                            // Main form instance
        private static volatile bool IsSettingsOpen = false;// Detects if the settings form is open
        public static volatile bool IsDebug = false;        // Enables debug methods and logic
        public static readonly string ApplicationFilesLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        public static readonly Cursor SystemHandCursor = new Cursor(LoadCursor(IntPtr.Zero, 32649));

        public static frmMain GetMainFormInstance() {
            return MainForm;
        }
        public static bool SettingsOpen {
            get { return IsSettingsOpen; }
            set { IsSettingsOpen = value; }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            EnableDebug();

            using (System.Diagnostics.Process ThisProgram = System.Diagnostics.Process.GetCurrentProcess()) {
                ThisProgram.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            }

            if (Properties.Settings.Default.FirstTime) {
                switch (MessageBox.Show("Would you like to specify a download path now? If not, it'll default to the current direcotry.", "YChanEx", MessageBoxButtons.YesNo)) {
                    case DialogResult.Yes:
                        using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                            fbd.Description = "Select a folder to download to";
                            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                            if (fbd.ShowDialog() == DialogResult.OK) {
                                Downloads.Default.DownloadPath = fbd.SelectedPath;
                            }
                            else {
                                MessageBox.Show("Downloads will be saved at \"" + Environment.CurrentDirectory + "\". You can change this at any time in the settings.", "YChanEx");
                                Downloads.Default.DownloadPath = Environment.CurrentDirectory;
                            }
                        }
                        break;
                    case DialogResult.No:
                        MessageBox.Show("Downloads will be saved at \"" + Environment.CurrentDirectory + "\". You can change this at any time in the settings.", "YChanEx");
                                Downloads.Default.DownloadPath = Environment.CurrentDirectory;
                        break;
                }

                Properties.Settings.Default.FirstTime = false;
                Properties.Settings.Default.Save();
                Downloads.Default.Save();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new frmMain();
            Application.Run(MainForm);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        static void EnableDebug() {
            IsDebug = true;
        }
    }
}
