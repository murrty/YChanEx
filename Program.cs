using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    static class Program {

        /// <summary>
        /// The Program GUID used for Mutex one-instance enforcement.
        /// </summary>
        private static readonly GuidAttribute ProgramGUID =
            (GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
        /// <summary>
        /// The Mutex for one-instance enforcement.
        /// </summary>
        private static Mutex mtx;
        /// <summary>
        /// The <see cref="frmMain"/> form.
        /// </summary>
        private static frmMain MainForm; // Keep it private, or no?

        /// <summary>
        /// Whether to load saved threads while debugging.
        /// </summary>
        public static bool LoadThreadsInDebug = true;
        /// <summary>
        /// If debug mode is active.
        /// </summary>
        public static volatile bool IsDebug = false;
        /// <summary>
        /// If the application is admin.
        /// </summary>
        public static bool IsAdmin = false;
        /// <summary>
        /// The local application data folder path for YChanEx, to store threads when not using the INI file.
        /// </summary>
        public static readonly string ApplicationFilesLocation =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\YChanEx";
        /// <summary>
        /// The full path of the application.
        /// </summary>
        public static readonly string FullApplicationPath =
            System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        /// <summary>
        /// If the settings form is currently open. Used to pause scanning.
        /// </summary>
        public static bool SettingsOpen { get; set; }

        [STAThread]
        static void Main() {
            Console.WriteLine("Welcome to the amazing world of: Loading application.");

#if DEBUG
            IsDebug = true;
#endif

            // Check debug and enable form stuff
            IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create new mutex thread for single-instance
            mtx = new Mutex(true, ProgramGUID.Value);

            // Check if there's already a program running with this mutex.
            if (mtx.WaitOne(TimeSpan.Zero, true) || IsDebug) {

                // Check the protocol, if it's active.
                SystemRegistry.CheckProtocol();

                // If the '--protocol' arg is present, we create the protocol.
                foreach (string arg in Environment.GetCommandLineArgs()) {
                    if (arg == "--protocol") {
                        SystemRegistry.CreateProtocol();
                        break;
                    }
                }

                // Set the TLS version to 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Set this current process to BelowNormal
                using (System.Diagnostics.Process ThisProgram = System.Diagnostics.Process.GetCurrentProcess()) {
                    ThisProgram.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                }

                // Load the settings.
                Config.Settings.Load();

                // Check for "FirstTime" flag in the default settings of the application
                if (!IsDebug && Properties.Settings.Default.FirstTime) {
                    switch (MessageBox.Show("Would you like to specify a download path now? If not, it'll default to the current direcotry.", "YChanEx", MessageBoxButtons.YesNo)) {
                        case DialogResult.Yes: {
                            using BetterFolderBrowserNS.BetterFolderBrowser fbd = new();
                            fbd.Title = "Select a folder to download to";
                            fbd.RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                            if (fbd.ShowDialog() == DialogResult.OK) {
                                Config.Settings.Downloads.DownloadPath = fbd.SelectedPath;
                            }
                            else {
                                MessageBox.Show("Downloads will be saved at \"" + Environment.CurrentDirectory + "\". You can change this at any time in the settings.", "YChanEx");
                                Config.Settings.Downloads.DownloadPath = Environment.CurrentDirectory;
                            }
                        } break;
                        case DialogResult.No: {
                            MessageBox.Show("Downloads will be saved at \"" + Environment.CurrentDirectory + "\". You can change this at any time in the settings.", "YChanEx");
                            Config.Settings.Downloads.DownloadPath = Environment.CurrentDirectory;
                        } break;
                    }

                    Config.Settings.Initialization.FirstTime = false;
                    Config.Settings.Initialization.Save();
                    Config.Settings.Downloads.Save();
                }

                // Create and run the main form
                MainForm = new frmMain();
                Application.Run(MainForm);

                // Release mutex after the form closes.
                mtx.ReleaseMutex();
            }
            else {
                // Check arguments
                List<string> SentArguments = new();
                bool SetProtocol = false;
                foreach (string argument in Environment.GetCommandLineArgs()) {
                    if (argument.StartsWith("ychanex:")) {
                        SentArguments.Add(argument);
                    }
                    else if (argument == "--protocol" && !SetProtocol) {
                        SystemRegistry.CreateProtocol();
                        SetProtocol = true;
                    }
                }

                int hwnd = Win32.FindWindow(null, "YChanEx");
                if (hwnd != 0) {
                    Win32.CopyDataStruct DataStruct = new();
                    try {
                        if (SentArguments.Count > 0) {
                            string Threads = string.Join("|", SentArguments);
                            DataStruct.cbData = (Threads.Length + 1) * 2;
                            DataStruct.lpData = Win32.LocalAlloc(0x40, DataStruct.cbData);
                            Marshal.Copy(Threads.ToCharArray(), 0, DataStruct.lpData, Threads.Length);
                            DataStruct.dwData = (IntPtr)1;
                            Win32.SendMessage((IntPtr)hwnd, Win32.WM_COPYDATA, IntPtr.Zero, ref DataStruct);
                        }
                        else {
                            Win32.SendMessage((IntPtr)hwnd, Win32.WM_SHOWFORM, IntPtr.Zero, ref DataStruct);
                        }
                        Console.WriteLine("Hey, whats up? I shoved some info to the other program.");
                        Thread.Sleep(1500);
                    }
                    finally {
                        DataStruct.Dispose();
                    }
                }
            }

            Console.WriteLine("It is now safe to turn off your application.");
        }

    }
}
