#nullable enable
namespace YChanEx;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using murrty.classes;
static class Program {
    /// <summary>
    /// The current version of the software.
    /// </summary>
    public static Version CurrentVersion { get; } = new(3, 0, 0, 2);

    /// <summary>
    /// The user-agent used by ychanex.
    /// </summary>
    public static string UserAgent { get; }

    /// <summary>
    /// Whether to load saved threads while debugging.
    /// </summary>
    public static readonly bool LoadThreadsInDebug = true;

    /// <summary>
    /// The absolute name of the application, with extension.
    /// </summary>
    internal static string ApplicationName { get; }
    /// <summary>
    /// The absolute path of the application.
    /// </summary>
    internal static string FullApplicationPath { get; }
    /// <summary>
    /// The path of the application.
    /// </summary>
    internal static string ApplicationDirectory { get; }

    /// <summary>
    /// The string to the Github page.
    /// </summary>
    public const string GithubPage = "https://github.com/murrty/YChanEx";

    /// <summary>
    /// Gets whether the program is running in debug mode.
    /// </summary>
    public static bool DebugMode { get; }

    /// <summary>
    /// Gets whether the program is running as an Administrator.
    /// </summary>
    public static bool IsAdmin { get; }

    /// <summary>
    /// Gets a string-representation of the current common language runtime version.
    /// </summary>
    public static string CLR { get; }

    /// <summary>
    /// Gets or sets the exit code of the application.
    /// </summary>
    public static int ExitCode { get; set; }

    /// <summary>
    /// The mutex of the program instance.
    /// </summary>
    private static readonly Mutex Instance;

    /// <summary>
    /// The GUID of the program.
    /// </summary>
    private static Guid ProgramGUID { get; }

    /// <summary>
    /// The saved threads path.
    /// </summary>
    internal static string SavedThreadsPath { get; set; }

    /// <summary>
    /// If the settings form is currently open. Used to pause scanning.
    /// </summary>
    public static bool SettingsOpen { get; set; }

    static Program() {
        // An array of possible application paths.
        string?[] Paths = [
            // The executing assembly location, most likely to work on all CLR versions.
            Assembly.GetExecutingAssembly().Location,

            // Uses the App-Domain values for cross-platform support, if it becomes possible.
            AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar
                + AppDomain.CurrentDomain.FriendlyName,

            // Gets the main file name from the executing process.
            Process.GetCurrentProcess().MainModule?.FileName,

            // null value, it will prevent the application from launching since the value has to be known.
            null];

        // Get the correct path. It may be null, but it's okay since it won't launch in that case.
        // The reason its non-nullable is because the application should KNOW it's not null to run.
        FullApplicationPath = Paths.FirstNonNullEmptyWhiteSpace(true);
        ApplicationDirectory = (File.Exists(FullApplicationPath) ? Path.GetDirectoryName(FullApplicationPath) : null)!;

        // Set the set application name, which can be different based on users preference.
        ApplicationName = AppDomain.CurrentDomain.FriendlyName;

        // Try to get the GUID, if it exists.
        // The default GUID should keep it running, but it may be problematic.
        // Maybe work on handling it in a more elegant way?
        string? GuidString = Assembly.GetExecutingAssembly().GetCustomAttribute<GuidAttribute>()?.Value;
        ProgramGUID = GuidString.IsNullEmptyWhitespace() ? default : Guid.Parse(GuidString);

        // Set the common language runtime string
        CLR = RuntimeInformation.FrameworkDescription.UnlessNullEmptyWhiteSpace("unknown framework")
#if NET7_0_OR_GREATER
                + "-" + RuntimeInformation.RuntimeIdentifier.UnlessNullEmptyWhiteSpace("unknown runtime")
#endif
                + (Environment.Is64BitProcess ? " (64-bit)" : " (32-bit)");

        // Check admin.
        IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        // Debug stuff.
#if DEBUG
        DebugMode = true;
        if (SystemRegistry.TryGetKey("DebugDir", out string? NewDir)) {
            if (!Directory.Exists(NewDir)) {
                Directory.CreateDirectory(NewDir);
            }
            ApplicationDirectory = NewDir;
            Environment.CurrentDirectory = NewDir;
        }
#endif

        // Set the saved threads path.
        SavedThreadsPath = Path.Combine(Environment.CurrentDirectory, "SavedThreads");
        UserAgent = $"Mozilla/5.0 Gecko/20100101 ychanex/{CurrentVersion} ({CLR})";
        Instance = new Mutex(true, ProgramGUID.ToString());
    }

    [STAThread]
    static int Main(string[] argv) {
        Console.WriteLine("Welcome to the amazing world of: Loading application.");
        if (!DebugMode && !Instance.WaitOne(TimeSpan.Zero, true)) {
            ExitCode = 1152; // Cannot start more than one instance of the specified program.

            if (Arguments.SetProtocol(argv)) {
                SystemRegistry.SetProtocolKey();
                return ExitCode;
            }

            nint hWnd = CopyData.FindWindow(null, "YChanEx");
            if (hWnd != 0) {
                if (argv.Length > 0) {
                    CopyData.SendArray(hWnd, 0, CopyData.ID_ARGS, argv);
                }
                else {
                    CopyData.SendMessage(hWnd, CopyData.WM_SHOWMAINFORM, 0, 0);
                }
            }

            return ExitCode;
        }

        // Check debug and enable form stuff

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Log.InitializeLogging();

        if (!string.Equals(Environment.CurrentDirectory, ApplicationDirectory, StringComparison.InvariantCultureIgnoreCase)) {
            Log.Warn($"Environment.CurrentDirectory is set to '{Environment.CurrentDirectory}', when it should be '{ApplicationDirectory}'.");
            Environment.CurrentDirectory = ApplicationDirectory;
            Log.Warn("Set the Environment.CurrentDirectory to the correct one.");
            SavedThreadsPath = Path.Combine(Environment.CurrentDirectory, "SavedThreads");
        }

        Arguments.Argv = argv;

        // Check the protocol, if it's active.
        SystemRegistry.CheckProtocolKey();

        // Set this current process to BelowNormal
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        // Load the settings.
        DownloadHistory.Load();

        // Check for "FirstTime" flag in the default settings of the application
        if (Initialization.FirstTime) {
            switch (MessageBox.Show("Would you like to specify a download path now? If not, it'll default to the current directory.", "YChanEx", MessageBoxButtons.YesNo)) {
                case DialogResult.Yes: {
                    using BetterFolderBrowserNS.BetterFolderBrowser fbd = new();
                    fbd.Title = "Select a folder to download to";
                    fbd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                    if (fbd.ShowDialog() == DialogResult.OK) {
                        Downloads.DownloadPath = fbd.SelectedPath;
                    }
                    else {
                        MessageBox.Show("Downloads will be saved at \"" + Environment.CurrentDirectory + "\". You can change this at any time in the settings.", "YChanEx");
                        Downloads.DownloadPath = Environment.CurrentDirectory;
                    }
                } break;
                case DialogResult.No: {
                    MessageBox.Show("Downloads will be saved at \"" + Environment.CurrentDirectory + "\". You can change this at any time in the settings.", "YChanEx");
                    Downloads.DownloadPath = Environment.CurrentDirectory;
                } break;
            }

            Initialization.FirstTime = false;
        }

        // Create and run the main form
        Application.Run(new frmMain());

        // Save config.
        DownloadHistory.Save();

        // Release mutex after the form closes.
        Instance.ReleaseMutex();
        Console.WriteLine("It is now safe to turn off your application.");
        return ExitCode;
    }
}
