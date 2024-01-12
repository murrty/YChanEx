using System.Runtime.InteropServices;
using System.Text;

namespace YChanEx {
    public static class NativeMethods {
        #region HintTextBox & UAC Shield Button
        public const int BCM_FIRST = 0x1600;
        public const int BCM_SETSHIELD = (BCM_FIRST + 0x000c);
        [DllImport("user32.dll")]
        public static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);
        #endregion

        #region Hand Cursor
        public static readonly nint IDC_HAND = (nint)32649;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern nint LoadCursor(nint hInstance, nint lpCursorName);
        #endregion

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, char[] lpReturnedString, int nSize, string lpFileName);
    }
}

namespace murrty {
    public static class NativeMethods {
        public const int WM_SETCURSOR = 0x0020;
        public const int EM_SETMARGINS = 0xd3;
        public const int EC_RIGHTMARGIN = 2;
        public const int EC_LEFTMARGIN = 1;

        public static readonly nint IDC_HAND = (nint)32649;
        public static readonly nint HandCursor = LoadCursor(0, IDC_HAND);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern nint LoadCursor(nint hInstance, nint lpCursorName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SetCursor(nint hCursor);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(nint hwnd, string pszSubAppName, string pszSubIdList);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);
    }
}