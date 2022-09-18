namespace YChanEx;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct CopyDataStruct {
    public IntPtr dwData;
    public int cbData;
    public IntPtr lpData;
}