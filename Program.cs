using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YChanEx {
    static class Program {
        static frmMain MainForm;                            // Main form instance
        private static volatile bool IsSettingsOpen = false;// Detects if the settings form is open

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new frmMain();
            Application.Run(MainForm);
        }
    }
}
