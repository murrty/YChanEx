namespace YChanEx;

using Microsoft.Win32;
using murrty.classes;

public static class SystemRegistry {
    public static string ProtocolValue { get; } = $"\"{Program.FullApplicationPath}\" \"%1\"";
    public static bool ProtocolExists { get; private set; }

    public static bool CheckProtocol() {
        RegistryKey YChanExProtocol = Registry.ClassesRoot.OpenSubKey("ychanex\\shell\\open\\command", false);
        if (Registry.ClassesRoot.GetValue("tags", "URL Protocol") != null && YChanExProtocol != null && (string)YChanExProtocol.GetValue("") == ProtocolValue) {
            ProtocolExists = true;
            return true;
        }
        else {
            ProtocolExists = false;
            return false;
        }
    }

    public static bool CreateProtocol() {
        if (!Program.IsAdmin) {
            return false;
        }
        if (ProtocolExists) {
            return true;
        }

        try {
            RegistryKey setIdentifier = Registry.ClassesRoot.CreateSubKey("ychanex", true);
            setIdentifier.SetValue("URL Protocol", "");

            setIdentifier = setIdentifier.CreateSubKey("shell\\open\\command", true);
            setIdentifier.SetValue("", ProtocolValue);

            setIdentifier = Registry.ClassesRoot.CreateSubKey("ychanex\\DefaultIcon", true);
            setIdentifier.SetValue("", $"\"{Program.FullApplicationPath}\",0");

            return true;
        }
        catch (System.Exception ex) {
            Log.ReportException(ex);
            return false;
        }
    }
}
