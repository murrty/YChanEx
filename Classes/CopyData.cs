using System.Runtime.InteropServices;

namespace YChanEx {
    internal sealed class CopyData {

        //  0x40 is an unused message.
        public const int WM_SHOWFORM = 0x0040;
        public const int WM_COPYDATA = 0x004A;

        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SentData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65_534)]
            public string Argument;
        }

        public static IntPtr IntPtrAlloc<T>(T param) {
            IntPtr retval = Marshal.AllocHGlobal(Marshal.SizeOf(param));
            Marshal.StructureToPtr(param, retval, false);
            return retval;
        }
        public static void IntPtrFree(ref IntPtr PreAlloc) {
            if (PreAlloc != IntPtr.Zero) {
                Marshal.FreeHGlobal(PreAlloc);
                PreAlloc = IntPtr.Zero;
            }
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    }
}
