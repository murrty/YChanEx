using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YChanEx {
    static class Program {
        static Mutex mtx = new Mutex(true, "{BLESSED-BE-LIKULAU-YChanEx}");

        [STAThread]
        static void Main() {
            if ((new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator)) {
                for (int i = 1; i < Environment.GetCommandLineArgs().Length; i++) {
                    string arg = Environment.GetCommandLineArgs()[i];
                    if (arg.StartsWith("installProtocol")) {
                        Controller.installProtocol();
                    }
                }
            }
            if (mtx.WaitOne(TimeSpan.Zero, true)) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
                mtx.ReleaseMutex();
            }
            else {
                bool isDownload = false;
                for (int i = 1; i < Environment.GetCommandLineArgs().Length; i++) {
                    string arg = Environment.GetCommandLineArgs()[i].Replace("ychanex:", "");
                    if (Controller.isSupported(arg)) {
                        File.WriteAllText(Controller.settingsDir + "\\Arg.nfo", arg);
                        Controller.PostMessage((IntPtr)Controller.HWND_BROADCAST, Controller.WM_ADDDOWNLOAD, IntPtr.Zero, IntPtr.Zero);
                        isDownload = true;
                        break;
                    }
                }
                if (!isDownload) {
                    Controller.PostMessage((IntPtr)Controller.HWND_BROADCAST, Controller.WM_SHOWFORM, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }
    }
}