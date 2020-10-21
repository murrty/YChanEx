using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
