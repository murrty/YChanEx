#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;
/// <summary>
/// Main class that handles reading and writing to the registry.
/// Used for setting/updating/checking the protocol.
/// </summary>
//[System.Diagnostics.DebuggerStepThrough]
internal static class SystemRegistry {
    private const string UserKeyName = "SOFTWARE\\ychanex";

    /// <summary>
    ///     The registry key that's used for the users' registry hive.
    /// </summary>
    private static readonly RegistryKey UserRegistryKey;

    static SystemRegistry() {
        UserRegistryKey = Registry.CurrentUser.OpenSubKey(UserKeyName, true)
            ?? Registry.CurrentUser.CreateSubKey(UserKeyName, true);
    }

    /// <summary>
    ///     Checks if the protocol is installed and pointing to the current program path.
    /// </summary>
    /// <returns></returns>
    public static bool CheckProtocolKey() {
        using RegistryKey? YchanRegistryKey = Registry.ClassesRoot.OpenSubKey("ychanex\\shell\\open\\command", false);

        if (YchanRegistryKey is null) {
            return false;
        }

        return Registry.ClassesRoot.GetValue("ychanex", "URL Protocol") is not null
            && YchanRegistryKey.GetValue("") is string val
            && val.Equals($"\"{Program.FullApplicationPath}\" \"%1\"", StringComparison.InvariantCultureIgnoreCase);
    }
    /// <summary>
    ///     Sets the protocol in the registry to the current program path.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"/>
    public static int SetProtocolKey() {
        if (!Program.IsAdmin) {
            throw new UnauthorizedAccessException("The program does not have administrative permission to write to the registry.");
        }

        RegistryKey? YchanRegistryKey = Registry.ClassesRoot.CreateSubKey("ychanex");
        YchanRegistryKey.SetValue("URL Protocol", "");
        YchanRegistryKey.Dispose();

        YchanRegistryKey = Registry.ClassesRoot.CreateSubKey("ychanex\\shell\\open\\command");
        YchanRegistryKey.SetValue("", $"\"{Program.FullApplicationPath}\" \"%1\"");
        YchanRegistryKey.Dispose();

        YchanRegistryKey = Registry.ClassesRoot.CreateSubKey("ychanex\\DefaultIcon");
        YchanRegistryKey.SetValue("", $"\"{Program.FullApplicationPath}\",0");
        YchanRegistryKey.Dispose();

        return 0;
    }

    /// <summary>
    ///     Tries to retrieve a key from the registry.
    /// </summary>
    /// <param name="KeyName">
    ///     The key name that should be retrieved.
    /// </param>
    /// <param name="Value">
    ///     The output value of the key if it's found, otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if they key eixsts and was retireved; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetKey(string KeyName, [NotNullWhen(true)] out string? Value) {
        if (UserRegistryKey.GetValue(KeyName) is string val) {
            Value = val;
            return true;
        }
        Value = null;
        return false;
    }
    /// <summary>
    ///     Sets a key value in the registry.
    /// </summary>
    /// <param name="KeyName">
    ///     The name of the key to write.
    /// </param>
    /// <param name="Value">
    ///     The value to set. If <see langword="null"/>, it will delete the key.
    /// </param>
    public static void Write(string KeyName, string? Value) {
        if (Value is null) {
            Delete(KeyName);
        }
        else {
            UserRegistryKey.SetValue(KeyName, Value);
        }
    }
    /// <summary>
    /// Deletes a key from the registry.
    /// </summary>
    /// <param name="KeyName"></param>
    public static void Delete(string KeyName) {
        UserRegistryKey.DeleteValue(KeyName);
    }

    public static bool TryGetKey(string KeyName, string SubKeyPath, [NotNullWhen(true)] out string? Value) {
        using RegistryKey? SubKey = UserRegistryKey.OpenSubKey(SubKeyPath, false);
        if (SubKey is not null) {
            object? KeyValue = SubKey.GetValue(KeyName);
            if (KeyValue is string val) {
                Value = val;
                return true;
            }
        }
        Value = null;
        return false;
    }
    /// <summary>
    ///     Sets a key value in the registry.
    /// </summary>
    /// <param name="KeyName">
    ///     The name of the key to write.
    /// </param>
    /// <param name="Value">
    ///     The value to set. If <see langword="null"/>, it will delete the key.
    /// </param>
    public static void Write(string KeyName, string SubKeyPath, string? Value) {
        if (Value is null) {
            Delete(KeyName, SubKeyPath);
        }
        else {
            using RegistryKey SubKey = UserRegistryKey.CreateSubKey(SubKeyPath, true);
            if (SubKey is not null) {
                SubKey.SetValue(KeyName, Value);
            }
        }
    }
    /// <summary>
    /// Deletes a key from the registry.
    /// </summary>
    /// <param name="KeyName"></param>
    public static void Delete(string KeyName, string SubKeyPath) {
        using RegistryKey? SubKey = UserRegistryKey.OpenSubKey(SubKeyPath, true);
        if (SubKey is not null) {
            SubKey.DeleteValue(KeyName);
        }
    }
}
