#nullable enable
namespace murrty.structs;
using System.Text.RegularExpressions;
/// <summary>
/// Represents a modified Version.
/// </summary>
[System.Diagnostics.DebuggerStepThrough]
public readonly partial struct Version : IEquatable<Version>, IEqualityComparer<Version>, IComparable<Version>, IComparer<Version> {
    /// <summary>
    /// Contains an empty Version with no version information relevant.
    /// </summary>
    [NonSerialized]
    public static readonly Version Empty = new(0, 0, 0, 0);

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){1,3}(-(25[0-5]|(2[0-4]|1\d|[1-9]|)\d))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex VersionRegexGenerated();
    private static readonly Regex VersionRegex = VersionRegexGenerated();
#else
    private static readonly Regex VersionRegex = new(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){1,3}(-(25[0-5]|(2[0-4]|1\d|[1-9]|)\d))?$");
#endif

    /// <summary>
    /// The major verison number.
    /// </summary>
    public byte Major { get; init; }
    /// <summary>
    /// The minor version number.
    /// </summary>
    public byte Minor { get; init; }
    /// <summary>
    /// The revision version number.
    /// </summary>
    public byte Revision { get; init; }
    /// <summary>
    /// The beta version number.
    /// </summary>
    public byte Beta { get; init; }
    /// <summary>
    /// Whether the version is a beta version.
    /// </summary>
    public readonly bool IsBeta => this.Beta > 0;

    /// <summary>
    /// Initializes a new <see cref="Version"/> structure for defining the current application version.
    /// </summary>
    /// <param name="Major">The major version of the release.</param>
    /// <param name="Minor">The minor version of the release.</param>
    /// <param name="Revision">The revision of the release.</param>
    public Version(byte Major, byte Minor, byte Revision) {
        this.Major = Major;
        this.Minor = Minor;
        this.Revision = Revision;
        this.Beta = 0;
    }
    /// <summary>
    /// Initializes a new <see cref="Version"/> structure for defining the current application version.
    /// </summary>
    /// <param name="Major">The major version of the release.</param>
    /// <param name="Minor">The minor version of the release.</param>
    /// <param name="Revision">The revision of the release.</param>
    /// <param name="Beta">The beta version of the release.</param>
    public Version(byte Major, byte Minor, byte Revision, byte Beta) {
        this.Major = Major;
        this.Minor = Minor;
        this.Revision = Revision;
        this.Beta = Beta;
    }
    /// <summary>
    /// Initializes a new <see cref="Version"/> structure for defining the current application version.
    /// </summary>
    /// <param name="Data">The version string that is parsed through.
    /// <para>Example strings are: "1.0.1" and "1.0.1-1" with limited support for "1.01" and "1.01-pre1".</para>
    /// </param>
    public Version(string Data) {
        Version vers = Parse(Data);
        this.Major = vers.Major;
        this.Minor = vers.Minor;
        this.Revision = vers.Revision;
        this.Beta = vers.Beta;
    }

    /// <summary>
    /// Converts a string representation of the <see cref="Version"/> to a structure with the data.
    /// </summary>
    /// <param name="Data">The string data to convert.</param>
    /// <returns>A new <see cref="Version"/> structure filled with relevant information.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Version Parse(string Data) {
        if (string.IsNullOrWhiteSpace(Data)) {
            throw new ArgumentException("The version string is null, empty, or whitespace.");
        }

        if (!VersionRegex.IsMatch(Data)) {
            throw ThrowBadVersionString(Data);
        }

        byte Minor = 0;
        byte Revision = 0;
        byte Beta = 0;

        if (Data.Contains('-')) {
            if (!byte.TryParse(Data.Split('-')[1], out Beta)) {
                throw ThrowBadVersionString(Data);
            }
            Data = Data.Split('-')[0];
        }

        string[] Splits = Data.Split('.');
        return Splits.Length switch {
            1 when byte.TryParse(Splits[0], out byte Major) => new Version(Major, Minor, Revision, Beta),
            2 when byte.TryParse(Splits[0], out byte Major) && byte.TryParse(Splits[1], out Minor) => new Version(Major, Minor, Revision, Beta),
            3 when byte.TryParse(Splits[0], out byte Major) && byte.TryParse(Splits[1], out Minor) && byte.TryParse(Splits[2], out Revision) => new Version(Major, Minor, Revision, Beta),
            _ => throw ThrowBadVersionString(Data),
        };
    }
    /// <summary>
    /// Tries to parsre a string value to a <see cref="Version"/> structure representing the data.
    /// </summary>
    /// <param name="Data">The string value to convert.</param>
    /// <param name="vers">The output <see cref="Version"/> structure.</param>
    /// <returns><see langword="true"/> if the parse was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(string Data, out Version vers) {
        try {
            vers = Parse(Data);
            return true;
        }
        catch {
            vers = Empty;
            return false;
        }
    }

    /// <summary>
    /// Retrieves the string representation of the struct instance.
    /// </summary>
    /// <returns>A formatted string representing a <see cref="Version"/> instance, such as "1.0.0-1" or "1.1.0".</returns>
    public override string ToString() {
        return $"{Major}.{Minor}.{Revision}{(IsBeta ? $"-{Beta}" : "")}";
    }

    /// <summary>
    ///     Returns the version to a byte array.
    /// </summary>
    /// <returns>A byte array of the current instance.</returns>
    public byte[] ToArray() {
        return [Major, Minor, Revision, Beta];
    }

    public override readonly bool Equals(object? obj) {
        return obj is Version v && Equals(this, v);
    }
    public override readonly int GetHashCode() {
        int num = 0;
        num |= (Major & 0xF) << 28;
        num |= (Minor & 0xFF) << 20;
        num |= (Revision & 0xFF) << 12;
        return num | (Beta & 0xFFF);
        //int hashCode = -1072827954;
        //hashCode = hashCode * -1521134295 + Major.GetHashCode();
        //hashCode = hashCode * -1521134295 + Minor.GetHashCode();
        //hashCode = hashCode * -1521134295 + Revision.GetHashCode();
        //hashCode = hashCode * -1521134295 + Beta.GetHashCode();
        //return hashCode;
    }

    public bool Equals(Version other) {
        return Equals(this, other);
    }
    public bool Equals(Version x, Version y) {
        return x.Major == y.Major && x.Minor == y.Minor && x.Revision == y.Revision && x.Beta == y.Beta;
    }
    public int GetHashCode(Version obj) {
        return obj.GetHashCode();
    }

    public int CompareTo(Version other) {
        return Compare(this, other);
    }
    public int Compare(Version x, Version y) {
        if (x.Major > y.Major) {
            return 1;
        }
        else if (x.Major < y.Major) {
            return -1;
        }

        if (x.Minor > y.Minor) {
            return 1;
        }
        else if (x.Major < y.Minor) {
            return -1;
        }

        if (x.Revision > y.Revision) {
            return 1;
        }
        else if (x.Revision < y.Revision) {
            return -1;
        }

        if (x.Beta > y.Beta) {
            return 1;
        }
        else if (x.Beta < y.Beta) {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// Determines if the left version is older-than the right version.
    /// </summary>
    /// <param name="versa">The first version to check</param>
    /// <param name="versb">The second version to check</param>
    /// <returns><see langword="true"/> if the left version is older; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Version versa, Version versb) {
        if (versa.IsBeta) {
            if (versb.IsBeta) {
                if (versa.Major < versb.Major) {
                    return true;
                }
                else if (versa.Major == versb.Major) {
                    if (versa.Minor < versb.Minor) {
                        return true;
                    }
                    else if (versa.Minor == versb.Minor) {
                        if (versa.Revision < versb.Revision) {
                            return true;
                        }
                        else if (versa.Revision == versb.Revision) {
                            return versa.Beta < versb.Beta;
                        }
                    }
                }
            }
            else {
                if (versa.Major < versb.Major) {
                    return true;
                }
                else if (versa.Major == versb.Major) {
                    if (versa.Minor < versb.Minor) {
                        return true;
                    }
                    else if (versa.Minor == versb.Minor) {
                        return versa.Revision <= versb.Revision;
                    }
                }
            }
        }
        else {
            if (versa.Major < versb.Major) {
                return true;
            }
            else if (versa.Major == versb.Major) {
                if (versa.Minor < versb.Minor) {
                    return true;
                }
                else if (versa.Minor == versb.Minor) {
                    return versa.Revision < versb.Revision;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Determines if the left version is newer-than the right version.
    /// </summary>
    /// <param name="versa">The first version to check</param>
    /// <param name="versb">The second version to check</param>
    /// <returns><see langword="true"/> if the left version is newer; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Version versa, Version versb) {
        if (versb.IsBeta) {
            if (versa.IsBeta) {
                if (versa.Major > versb.Major) {
                    return true;
                }
                else if (versa.Major == versb.Major) {
                    if (versa.Minor > versb.Minor) {
                        return true;
                    }
                    else if (versa.Minor == versb.Minor) {
                        if (versa.Revision > versb.Revision) {
                            return true;
                        }
                        else if (versa.Revision == versb.Revision) {
                            return versa.Beta > versb.Beta;
                        }
                    }
                }
            }
            else {
                if (versa.Major > versb.Major) {
                    return true;
                }
                else if (versa.Major == versb.Major) {
                    if (versa.Minor > versb.Minor) {
                        return true;
                    }
                    else if (versa.Minor == versb.Minor) {
                        if (versa.Revision >= versb.Revision) {
                            return true;
                        }
                    }
                }
            }
        }
        else {
            if (versa.Major > versb.Major) {
                return true;
            }
            else if (versa.Major == versb.Major) {
                if (versa.Minor > versb.Minor) {
                    return true;
                }
                else if (versa.Minor == versb.Minor) {
                    return versa.Revision > versb.Revision;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Determines if the left version is older-than or equal-to the right version.
    /// </summary>
    /// <param name="versa">The first version to check</param>
    /// <param name="versb">The second version to check</param>
    /// <returns><see langword="true"/> if the left version is older-than or equal-to; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Version versa, Version versb) {
        return versa == versb || versa < versb;
    }
    /// <summary>
    /// Determines if the left version is newer-than or equal-to the right version.
    /// </summary>
    /// <param name="versa">The first version to check</param>
    /// <param name="versb">The second version to check</param>
    /// <returns><see langword="true"/> if the left version is newer-than or equal-to; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Version versa, Version versb) {
        return versa == versb || versa > versb;
    }
    /// <summary>
    /// Determines if the two versions compared are equal to each other.
    /// </summary>
    /// <param name="versa"></param>
    /// <param name="versb"></param>
    /// <returns><see langword="true"/> if the versions are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Version versa, Version versb) {
        return versa.Major == versb.Major && versa.Minor == versb.Minor && versa.Revision == versb.Revision && versa.Beta == versb.Beta;
    }
    /// <summary>
    /// Determines if the two versions compared are not equal to each other.
    /// </summary>
    /// <param name="versa"></param>
    /// <param name="versb"></param>
    /// <returns><see langword="true"/> if the versions are different; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Version versa, Version versb) {
        return versa.Major != versb.Major || versa.Minor != versb.Minor || versa.Revision != versb.Revision || versa.Beta != versb.Beta;
    }

    private static ArgumentException ThrowBadVersionString(string Data) {
        return new ArgumentException($"Cannot use '{Data}' as a version.");
    }
}
