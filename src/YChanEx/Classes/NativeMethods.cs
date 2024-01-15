#nullable enable
using System.Drawing;
using System.Runtime.InteropServices;
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
        public static extern int WritePrivateProfileString(string? lpAppName, string lpKeyName, string? lpDefault, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string? lpAppName, string lpKeyName, string? lpDefault, char[] lpReturnedString, int nSize, string lpFileName);
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
        public static extern int SetWindowTheme(nint hwnd, string pszSubAppName, string? pszSubIdList);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        internal static extern nint GetWindowDC(nint hWnd);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(nint hWnd, nint hDC);

        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(nint hWnd, ref RECT lpRect);

        [DllImport("gdi32.dll", EntryPoint = "ExcludeClipRect")]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern int ExcludeClipRect(nint hdc, int nLeftrect, int nTopRect, int nRightRect, int nBottomRect);
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct NCCALCSIZE_PARAMS {
        public RECT rgrc0, rgrc1, rgrc2;
        [MarshalAs(UnmanagedType.SysInt)]
        public nint lppos;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT {
        public static readonly RECT Empty = new(0, 0, 0, 0);

        [MarshalAs(UnmanagedType.I4)]
        public int left;
        [MarshalAs(UnmanagedType.I4)]
        public int top;
        [MarshalAs(UnmanagedType.I4)]
        public int right;
        [MarshalAs(UnmanagedType.I4)]
        public int bottom;

        public RECT(int left, int top, int right, int bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static implicit operator Rectangle(RECT rect) => new(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }
}