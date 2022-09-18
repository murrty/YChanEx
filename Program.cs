using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace YChanEx {
    static class Program {

        #region Version Information
        public static readonly Version CurrentVersion = new(2, 1, 0);
        /// <summary>
        /// The string to the Github page.
        /// </summary>
        public const string GithubPage = "https://github.com/murrty/YChanEx";
        #endregion

        #region Runtime Fields
        /// <summary>
        /// Gets whether the program is running in debug mode.
        /// </summary>
        public static bool DebugMode { get; private set; } = false;

        /// <summary>
        /// Gets whether the program is running as an Administrator.
        /// </summary>
        public static bool IsAdmin { get; private set; } = false;

        /// <summary>
        /// Gets or sets the exit code of the application.
        /// </summary>
        public static int ExitCode { get; set; } = 0;

        /// <summary>
        /// The mutex of the program instance.
        /// </summary>
        private static Mutex Instance;
        /// <summary>
        /// The GUID of the program.
        /// </summary>
        private static readonly GuidAttribute ProgramGUID =
            (GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
        #endregion

        /// <summary>
        /// Whether to load saved threads while debugging.
        /// </summary>
        public static bool LoadThreadsInDebug = true;
        /// <summary>
        /// The full path of the application.
        /// </summary>
        public static readonly string FullApplicationPath =
            System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public static readonly string ApplicationDirectory =
            System.IO.Path.GetDirectoryName(FullApplicationPath);

        internal static ImageList DownloadImages { get; } = new();

        /// <summary>
        /// If the settings form is currently open. Used to pause scanning.
        /// </summary>
        public static bool SettingsOpen { get; set; }

        [STAThread]
        static int Main(string[] args) {
            Console.WriteLine("Welcome to the amazing world of: Loading application.");

#if DEBUG
            DebugMode = true;
#endif

            // Check debug and enable form stuff
            IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            if (Environment.CurrentDirectory != ApplicationDirectory)
                Environment.CurrentDirectory = ApplicationDirectory;

            Arguments.ParseArguments(args);

            DownloadImages.ColorDepth = ColorDepth.Depth32Bit;
            DownloadImages.Images.Add(Properties.Resources.waiting);    // 0
            DownloadImages.Images.Add(Properties.Resources.download);   // 1
            DownloadImages.Images.Add(Properties.Resources.finished);   // 2
            DownloadImages.Images.Add(Properties.Resources.error);      // 3
            DownloadImages.Images.Add(Properties.Resources._404);       // 4
            DownloadImages.Images.Add(Properties.Resources.finished_reloaded); // 5

            if (DebugMode || (Instance = new(true, ProgramGUID.Value)).WaitOne(TimeSpan.Zero, true)) {
                // Check the protocol, if it's active.
                SystemRegistry.CheckProtocol();

                // Set the TLS version to 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Set this current process to BelowNormal
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

                // Load the settings.
                Config.Load();
                DownloadHistory.Load();

                if (Config.Settings == null) {
                    Config.Settings = new();
                }

                if (Arguments.SetProtocol) {
                    SystemRegistry.CreateProtocol();
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Check for "FirstTime" flag in the default settings of the application
                if (Config.Settings.Initialization.FirstTime) {
                    switch (MessageBox.Show("Would you like to specify a download path now? If not, it'll default to the current directory.", "YChanEx", MessageBoxButtons.YesNo)) {
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
                }

                // Create and run the main form
                Application.Run(new frmMain());

                // Save config.
                Config.Save();
                DownloadHistory.Save();

                // Release mutex after the form closes.
                if (!DebugMode) {
                    Instance.ReleaseMutex();
                }
            }
            else {
                if (Arguments.SetProtocol) {
                    SystemRegistry.CreateProtocol();
                }
                else {
                    IntPtr hwnd = CopyData.FindWindow(null, "YChanEx");
                    if (hwnd != IntPtr.Zero) {
                        if (Arguments.URLs.Count > 0) {
                            CopyData.SentData Data = new() { Argument = string.Join("|", Arguments.URLs) };
                            CopyData.CopyDataStruct DataStruct = new();
                            IntPtr CopyDataBuffer = IntPtr.Zero;
                            IntPtr DataBuffer = IntPtr.Zero;
                            try {
                                DataBuffer = CopyData.IntPtrAlloc(Data);
                                DataStruct.cbData = Marshal.SizeOf(Data);
                                DataStruct.dwData = new(1);
                                DataStruct.lpData = DataBuffer;
                                CopyDataBuffer = CopyData.IntPtrAlloc(DataStruct);
                                CopyData.SendMessage(hwnd, CopyData.WM_COPYDATA, IntPtr.Zero, CopyDataBuffer);
                            }
                            finally {
                                CopyData.IntPtrFree(ref CopyDataBuffer);
                                CopyData.IntPtrFree(ref DataBuffer);
                            }
                        }
                        else {
                            CopyData.SendMessage(hwnd, CopyData.WM_SHOWFORM, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                }
                ExitCode = 1152; // Cannot start more than one instance of the specified program.
            }

            Console.WriteLine("It is now safe to turn off your application.");
            return ExitCode;
        }

    }
}
