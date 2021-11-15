using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    static class Program {

        // TODO: Add a class for settings to save them locally, or portably in an ini

        private static readonly GuidAttribute ProgramGUID = (GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
        private static Mutex mtx;
        private static frmMain MainForm;                    // Main form instance
        private static volatile bool IsSettingsOpen = false;// Detects if the settings form is open
        public static volatile bool IsDebug = false;        // Enables debug methods and logic
        public static readonly string ApplicationFilesLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";


        public static bool SettingsOpen {
            get { return IsSettingsOpen; }
            set { IsSettingsOpen = value; }
        }

        [STAThread]
        static void Main() {
            // Create new mutex thread for single-instance
            mtx = new Mutex(true, ProgramGUID.Value);

            // Check debug and enable form stuff
            EnableDebug();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if there's already a program running with this mutex.
            if (mtx.WaitOne(TimeSpan.Zero, true)) {
                // Set the TLS version to 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Set this current process to BelowNormal
                using (System.Diagnostics.Process ThisProgram = System.Diagnostics.Process.GetCurrentProcess()) {
                    ThisProgram.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                }

                // Check for "FirstTime" flag in the default settings of the application
                if (Properties.Settings.Default.FirstTime) {
                    switch (MessageBox.Show("Would you like to specify a download path now? If not, it'll default to the current direcotry.", "YChanEx", MessageBoxButtons.YesNo)) {
                        case DialogResult.Yes:
                            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                                fbd.Description = "Select a folder to download to";
                                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
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

                // Create and run the main form
                MainForm = new frmMain();
                Application.Run(MainForm);

                // Release mutex after the form closes.
                mtx.ReleaseMutex();
            }
            else {
                // Get the currently running programs

                // Check arguments
                List<string> SentArguments = new List<string>();
                foreach (string argument in Environment.GetCommandLineArgs()) {
                    if (argument.StartsWith("ychanex:")) {
                        SentArguments.Add(argument.Substring(8));
                    }
                }

                IntPtr hwnd = (IntPtr)Win32.FindWindow(null, "YChanEx");
                Win32.CopyDataStruct CDS = new Win32.CopyDataStruct();

                try {
                    if (SentArguments.Count > 0) {
                        string argument = string.Join("|", SentArguments);
                        CDS.cbData = (argument.Length + 1) * 2;
                        CDS.lpData = Win32.LocalAlloc(0x40, CDS.cbData);
                        Marshal.Copy(argument.ToCharArray(), 0, CDS.lpData, argument.Length);
                        CDS.dwData = (IntPtr)1;
                        Win32.SendMessage(hwnd, Win32.WM_COPYDATA, IntPtr.Zero, ref CDS);
                    }
                    else {
                        Win32.SendMessage(hwnd, Win32.WM_SHOWFORM, IntPtr.Zero, ref CDS);
                    }
                }
                finally {
                    CDS.Dispose();
                }
            }

            Console.WriteLine("It is now safe to turn off your application.");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        static void EnableDebug() {
            IsDebug = true;
        }

    }
}
