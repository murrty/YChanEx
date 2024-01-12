#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
internal sealed partial class CopyData {
    public const nint NULL = 0x0;
    public const nint ERROR_INVALID_HANDLE = 0x6;
    public const nint ERROR_INVALID_DATA = 0xD;
    public const nint ERROR_SIGNAL_REFUSED = 0x9C;

    public const nuint ID_ARGS = 0xFF0;
    public const nuint ID_UPDATEDATA = 0xFF1;
    public const nuint ID_APPLICATIONINFO = 0xFF2;
    public const nuint ID_APPLICATIONHANDLES = 0xFF3;

    /// <summary>
    /// An application sends the WM_COPYDATA message to pass data to another application.
    /// <para />
    /// wParam should have the handle of the window passing the data, and lParam must be a pointer to a COPYDATASTRUCT that contains the data.
    /// </summary>
    public const int WM_COPYDATA = 0x004A;
    /// <summary>
    /// A non-standard window message that tells a form to display itself.
    /// </summary>
    public const int WM_SHOWMAINFORM = 0xFF00;
    /// <summary>
    /// A non-standard window message that tells the main application to send the handles.
    /// </summary>
    public const int WM_GETAPPLICATIONHANDLES = 0xFF01;
    /// <summary>
    /// A non-standard window message that tells the main application to generate and send the new update data to the stub application.
    /// </summary>
    public const int WM_UPDATEDATAREQUEST = 0xFF02;
    /// <summary>
    /// A non-standard window message that tells the main application that the updater is ready and that it can exit.
    /// The stub applcation should await for the process to end before performing the update, but it may download the update to prepare to move instead.
    /// </summary>
    public const int WM_UPDATERREADY = 0xFF03;
    /// <summary>
    /// A non-standard window message that tells the main application to send the application name.
    /// </summary>
    public const int WM_GETAPPLICATIONNAME = 0xFF04;
    /// <summary>
    /// A non-standard window message that tells the main application to die, if it's hanging (QueueHandler.ShowForm_NoForm).
    /// </summary>
    public const int WM_REQUESTCLOSE = 0xFF05;
    /// <summary>
    /// A non-standard window message that tells the main application to die right now, no matter what.
    /// </summary>
    public const int WM_DEMANDCLOSE = 0xFF06;

    // Receiver should never clear allocs, only sender should.
    public static nint NintAlloc<TStruct>(TStruct Structure) where TStruct : struct {
        nint PointerAddress = Marshal.AllocHGlobal(Marshal.SizeOf(Structure));
        Marshal.StructureToPtr(Structure, PointerAddress, true);
        return PointerAddress;
    }
    public static nint NintAlloc<TStruct>(TStruct Structure, int size) where TStruct : struct {
        if (size == 0) {
            size = Marshal.SizeOf(Structure);
        }
        nint PointerAddress = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(Structure, PointerAddress, true);
        return PointerAddress;
    }
    public static void NintFree(ref nint PreAlloc) {
        if (PreAlloc != 0) {
            Marshal.FreeHGlobal(PreAlloc);
            PreAlloc = 0;
        }
    }

    public static T GetParam<T>(nint LParam) {
        var DataStruct = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        return Marshal.PtrToStructure<T>(DataStruct.lpData);
    }

    public static int SendArray(nint hwnd, nint wp, nuint id, string[] array) {
        if (array is null || array.Length == 0 || hwnd == 0) {
            return 2;
        }

        nint pData = 0;
        try {
            int size = 0;

            foreach (string s in array) {
                size += s.Length + 1;
            }

            char[] chars = new char[size];
            int offset = 0;

            foreach (string s in array) {
                s.CopyTo(0, chars, offset, s.Length);
                offset += s.Length;
                chars[offset] = '\0';
                offset++;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(chars);
            pData = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, pData, bytes.Length);

            CopyDataStruct cds = new() {
                dwData = id,
                cbData = bytes.Length * sizeof(byte),
                lpData = pData,
            };

            return SendMessage(hwnd, WM_COPYDATA, wp, ref cds);
        }
        finally {
            NintFree(ref pData);
        }
    }
    public static string[] GetArray(nint LParam, out CopyDataStruct cds) {
        cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        byte[] bytes = new byte[cds.cbData - 1];
        Marshal.Copy(cds.lpData, bytes, 0, cds.cbData - 1);
        char[] chars = Encoding.UTF8.GetChars(bytes);
        return new string(chars).Split('\0');
    }
    public static bool TryGetArray(nint LParam, nuint dwData, [NotNullWhen(true)] out string[]? array) {
        var cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        if (cds.dwData != dwData) {
            array = null;
            return false;
        }
        byte[] bytes = new byte[cds.cbData - 1];
        Marshal.Copy(cds.lpData, bytes, 0, bytes.Length);
        char[] chars = Encoding.UTF8.GetChars(bytes);
        array = new string(chars).Split('\0');
        return true;
    }

    public static int SendStruct<T>(nint hwnd, nint wp, nuint id, T str) where T : struct {
        nint pData = 0;
        nint sData = 0;
        try {
            pData = NintAlloc(str);

            CopyDataStruct cds = new() {
                dwData = id,
                cbData = Marshal.SizeOf(str),
                lpData = pData,
            };

            return SendMessage(hwnd, WM_COPYDATA, wp, ref cds);
        }
        finally {
            NintFree(ref pData);
            NintFree(ref sData);
        }
    }
    public static T GetStruct<T>(nint LParam, out CopyDataStruct cds) where T : struct {
        cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        return Marshal.PtrToStructure<T>(cds.lpData);
    }
    public static bool TryGetStruct<T>(nint LParam, nuint dwData, out T structure) where T : struct {
        var cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        if (cds.dwData != dwData) {
            structure = default;
            return false;
        }
        structure = Marshal.PtrToStructure<T>(cds.lpData);
        return true;
    }
    public static bool TryGetStruct<T>(nint LParam, nuint dwData, [NotNullWhen(true)] out T? structure) where T : struct {
        var cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        if (cds.dwData != dwData) {
            structure = default;
            return false;
        }
        structure = Marshal.PtrToStructure<T>(cds.lpData);
        return true;
    }

    public static int SendBytes(nint hwnd, nint wp, nuint id, byte[] bytes) {
        if (bytes is null || bytes.Length == 0 || hwnd == 0) {
            return 2;
        }

        nint pData = 0;
        try {
            pData = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, pData, bytes.Length);

            CopyDataStruct cds = new() {
                dwData = id,
                cbData = bytes.Length * sizeof(byte),
                lpData = pData,
            };

            return SendMessage(hwnd, WM_COPYDATA, wp, ref cds);
        }
        finally {
            NintFree(ref pData);
        }
    }
    public static byte[] GetBytes(nint LParam, out CopyDataStruct cds) {
        cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        byte[] bytes = new byte[cds.cbData - 1];
        Marshal.Copy(cds.lpData, bytes, 0, cds.cbData - 1);
        return bytes;
    }
    public static bool TryGetBytes(nint LParam, nuint dwData, [NotNullWhen(true)] out byte[]? bytes) {
        var cds = Marshal.PtrToStructure<CopyDataStruct>(LParam);
        if (cds.dwData != dwData) {
            bytes = null;
            return false;
        }
        bytes = new byte[cds.cbData - 1];
        Marshal.Copy(cds.lpData, bytes, 0, bytes.Length);
        return true;
    }

    public static T BytesToStruct<T>(byte[] bytes) where T : struct {
        GCHandle strPtr = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try {
            return Marshal.PtrToStructure<T>(strPtr.AddrOfPinnedObject());
        }
        finally {
            strPtr.Free();
        }
    }
    public static byte[] StructToBytes<T>(T structure) where T : struct {
        nint strPtr = 0;
        try {
            int size = Marshal.SizeOf(structure);
            byte[] array = new byte[size];
            strPtr = NintAlloc(structure, size);
            Marshal.Copy(strPtr, array, 0, size);
            return array;
        }
        finally {
            NintFree(ref strPtr);
        }
    }

#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.SysInt)]
    public static partial nint FindWindow(string? strClassName, string? strWindowName);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageA", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static partial int SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageA", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static partial int SendMessage(nint hWnd, int Msg, nint wParam, ref CopyDataStruct lParam);
#else
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.SysInt)]
    public static extern nint FindWindow(string? strClassName, string? strWindowName);

    [DllImport("user32.dll", EntryPoint = "SendMessageA", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int SendMessage(nint hWnd, int Msg, nint wParam, nint lParam);

    [DllImport("user32.dll", EntryPoint = "SendMessageA", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int SendMessage(nint hWnd, int Msg, nint wParam, ref CopyDataStruct lParam);
#endif
}

[StructLayout(LayoutKind.Sequential)]
public struct CopyDataStruct {
    // Must be sent with WM_COPYDATA, otherwise 'access violations' will occur.

    /// <summary>
    /// The type of the data to be passed to the receiving application. The receiving application defines the valid types.
    /// </summary>
    [MarshalAs(UnmanagedType.SysUInt)]
    public nuint dwData; // Data identifier

    /// <summary>
    /// The size, in bytes, of the data pointed to by the lpData member.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)]
    public int cbData; // The length of 'lpData'

    /// <summary>
    /// The data to be passed to the receiving application. This member can be NULL.
    /// </summary>
    [MarshalAs(UnmanagedType.SysInt)]
    public nint lpData; // The pointer to the data
}
