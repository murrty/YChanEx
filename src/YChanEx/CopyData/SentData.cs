namespace YChanEx;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SentData {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65_536)] //65,535 + 1 for null terminator
    public string Argument;
}