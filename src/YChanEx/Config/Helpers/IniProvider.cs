#nullable enable
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using murrty.controls;
/// <summary>
/// The class containing the ini file handling.
/// </summary>
[System.Diagnostics.DebuggerStepThrough]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Scope = "member")]
[SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Scope = "member")]
internal static class IniProvider {
    /// <summary>
    /// Const string for an "empty" value.
    /// </summary>
    private const string EmptyString = "{empty_key_value}";
    private static readonly int EmptyStringLengthNull = EmptyString.Length + 1;

    /// <summary>
    /// The name of the executing file.
    /// </summary>
    private const string DefaultSection = "YChanEx";

    /// <summary>
    /// The full path of the Ini File (Generally, in the same folder as the executable)
    /// </summary>
    internal static readonly string IniPath = $"{Program.ApplicationDirectory}\\YChanEx.ini";

    private static bool InternalKeyExists(string Key, [NotNullWhen(true)] out string? Value, string? Section = null, object? Ignored = null) {
        return InternalKeyExists(Key, 4097, out Value, Section, Ignored);
    }
    private static string InternalWriteString(string Key, string Value, string? Section = null, object? Ignored = null) {
        _ = NativeMethods.WritePrivateProfileString(Section ?? DefaultSection, Key, Value, IniPath);
        return Value;
    }
    private static bool InternalKeyExists(string Key, int Length, [NotNullWhen(true)] out string? Value, string? Section = null, object? Ignored = null) {
        if (Key.IsNullEmptyWhitespace()) {
            Value = null;
            return false;
        }

        if (Length < EmptyStringLengthNull) {
            Length = EmptyStringLengthNull;
        }

        char[] ReadValue = new char[Length];
        int ReadChars = NativeMethods.GetPrivateProfileString(Section ?? DefaultSection, Key, EmptyString, ReadValue, Length, IniPath);
        Value = new string(ReadValue, 0, ReadChars);

        if (Value.IsNullEmptyWhitespace()) {
            Value = null;
            return false;
        }

        if (string.Equals(EmptyString, Value, StringComparison.OrdinalIgnoreCase)) {
            Value = null;
            return false;
        }

        return true;
    }

    internal static string ReadString(string Key, string? Section) {
        return InternalKeyExists(Key, out string? Data, Section) ? Data : string.Empty;
    }
    internal static void Delete(string Key, string? Section = null) {
        InternalWriteString(Key, null!, Section);
    }

    internal static void DeleteKey(string Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
    }
    internal static void DeleteKey(bool Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
    }
    internal static void DeleteKey(int Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
    }
    internal static void DeleteKey(Point Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
    }
    internal static void DeleteKey(Size Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
    }
    internal static void DeleteKey(Version Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
    }
    internal static Guid DeleteKey(Guid Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, null!, Section, Value);
        return Value;
    }

    internal static string Read(string Value, string Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, out string? Data, Section, Value))
            return Data;
        return Default;
    }
    internal static bool Read(bool Value, bool Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 6, out string? Data, Section, Value)) {
            return Data.ToLower() switch {
                "true" or "on" or "1" => true,
                _ => false
            };
        }
        return Default;
    }
    internal static int Read(int Value, int Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 11, out string? Data, Section, Value) && int.TryParse(Data, out int NewVal))
            return NewVal;
        return Default;
    }
    internal static Point Read(Point Value, Point Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 23, out string? Data, Section, Value)) {
            string[] DataSplit = Data.RemoveWhitespace().Split(',');
            if (DataSplit.Length >= 2 && int.TryParse(DataSplit[0], out int X) && int.TryParse(DataSplit[1], out int Y))
                return new(X, Y);
        }
        return Default;
    }
    internal static Size Read(Size Value, Size Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 23, out string? Data, Section, Value)) {
            string[] DataSplit = Data.RemoveWhitespace().Split(',');
            if (DataSplit.Length >= 2 && int.TryParse(DataSplit[0], out int W) && int.TryParse(DataSplit[1], out int H))
                return new(W, H);
        }
        return Default;
    }
    internal static Version Read(Version Value, Version Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 16, out string? Data, Section, Value) && Version.TryParse(Data, out Version NewVers))
            return NewVers;
        return Default;
    }
    internal static Guid Read(Guid Value, Guid Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 37, out string? Data, Section, Value) && Guid.TryParse(Data, out Guid NewGuid))
            return NewGuid;
        return Default;
    }
    internal static Proxy Read(Proxy Value, Proxy Default, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        if (InternalKeyExists(Key, 32, out string? Data, Section, Value) && Proxy.TryParse(Data, out var NewIPv4))
            return NewIPv4;
        return Default;
    }

    internal static string Write(string Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, Value, Section);
        return Value;
    }
    internal static bool Write(bool Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, Value ? "True" : "False", Section);
        return Value;
    }
    internal static int Write(int Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, Value.ToString(), Section);
        return Value;
    }
    internal static Point Write(Point Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, $"{Value.X},{Value.Y}", Section);
        return Value;
    }
    internal static Size Write(Size Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, $"{Value.Width},{Value.Height}", Section);
        return Value;
    }
    internal static Version Write(Version Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, Value.ToString(), Section);
        return Value;
    }
    internal static Guid Write(Guid Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, Value.ToString(), Section);
        return Value;
    }
    internal static Proxy Write(Proxy Value, string? Section = null, [CallerArgumentExpression(nameof(Value))] string Key = null!) {
        InternalWriteString(Key, Value.GetReadableIP(), Section);
        return Value;
    }
}