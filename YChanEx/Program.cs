using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using murrty.classes;

namespace YChanEx {
    static class Program {

        #region Version Information
        public static readonly Version CurrentVersion = new(3, 0, 0);
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

            if (DebugMode || (Instance = new(true, ProgramGUID.Value)).WaitOne(TimeSpan.Zero, true)) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Log.InitializeLogging();

                DownloadImages.ColorDepth = ColorDepth.Depth32Bit;
                DownloadImages.Images.Add(Properties.Resources.waiting);        // 0
                DownloadImages.Images.Add(Properties.Resources.downloading);    // 1
                DownloadImages.Images.Add(Properties.Resources.finished);       // 2
                DownloadImages.Images.Add(Properties.Resources.error);          // 3
                DownloadImages.Images.Add(Properties.Resources._404);           // 4
                DownloadImages.Images.Add(Properties.Resources.reloaded_downloaded);// 5
                DownloadImages.Images.Add(Properties.Resources.reloaded_missing);   // 6

                // Check the protocol, if it's active.
                SystemRegistry.CheckProtocol();

                // Set the TLS version to 1.2
                try { //try TLS 1.3
                    System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)12288
                                                                    | (System.Net.SecurityProtocolType)3072
                                                                    | (System.Net.SecurityProtocolType)768
                                                                    |  System.Net.SecurityProtocolType.Tls;
                    Log.Write("TLS 1.3 will be used.");
                }
                catch (NotSupportedException) {
                    try { //try TLS 1.2
                        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072
                                                                        | (System.Net.SecurityProtocolType)768
                                                                        |  System.Net.SecurityProtocolType.Tls;
                        Log.Write("TLS 1.2 will be used.");
                    }
                    catch (NotSupportedException) {
                        try { //try TLS 1.1
                            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)768
                                                                            |  System.Net.SecurityProtocolType.Tls;
                            Log.Write("TLS 1.1 will be used, Github updating may be affected.");
                        }
                        catch (NotSupportedException) { //TLS 1.0
                            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;
                            Log.Write("TLS 1.0 will be used, Github updating may be affected.");
                        }
                    }
                }

                // Set this current process to BelowNormal
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;

                // Load the settings.
                Config.Load();
                DownloadHistory.Load();

                if (Arguments.SetProtocol)
                    SystemRegistry.CreateProtocol();

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
                                DataBuffer = CopyData.NintAlloc(Data);
                                DataStruct.cbData = Marshal.SizeOf(Data);
                                DataStruct.dwData = new(1);
                                DataStruct.lpData = DataBuffer;
                                CopyDataBuffer = CopyData.NintAlloc(DataStruct);
                                CopyData.SendMessage(hwnd, CopyData.WM_COPYDATA, IntPtr.Zero, CopyDataBuffer);
                            }
                            finally {
                                CopyData.NintFree(ref CopyDataBuffer);
                                CopyData.NintFree(ref DataBuffer);
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
