﻿namespace YChanEx;

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a modified Version.
/// </summary>
//[System.Diagnostics.DebuggerStepThrough]
[StructLayout(LayoutKind.Sequential)]
public struct Version {
    /// <summary>
    /// Contains an empty Version with no version information relevant.
    /// </summary>
    [NonSerialized]
    public static readonly Version Empty = new(0, 0, 0, 0);

    /// <summary>
    /// The major of the version.
    /// </summary>
    public byte Major { get; }
    /// <summary>
    /// The minor of the version.
    /// </summary>
    public byte Minor { get; }
    /// <summary>
    /// The revision of the version.
    /// </summary>
    public byte Revision { get; }
    /// <summary>
    /// The beta number of the version.
    /// </summary>
    public byte Beta { get; }
    /// <summary>
    /// Whether the version is a beta version.
    /// </summary>
    public bool IsBeta => Beta != 0;

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
        Beta = 0;
    }
    /// <summary>
    /// Initializes a new <see cref="Version"/> structure for defining the current application version.
    /// </summary>
    /// <param name="Major">The major version of the release.</param>
    /// <param name="Minor">The minor version of the release.</param>
    /// <param name="Revision">The revision of the release.</param>
    /// <param name="BetaVersion">The beta version of the release.</param>
    public Version(byte Major, byte Minor, byte Revision, byte BetaVersion) {
        this.Major = Major;
        this.Minor = Minor;
        this.Revision = Revision;
        this.Beta = BetaVersion;
    }
    /// <summary>
    /// Initializes a new <see cref="Version"/> structure for defining the current application version.
    /// </summary>
    /// <param name="Data">The version string that is parsed through.
    /// <para>Example strings are: "1.0.1" and "1.0.1-1" with limited support for "1.01" and "1.01-pre1".</para>
    /// </param>
    public Version(string Data) {
        if (TryParse(Data, out Version v)) {
            this.Major = v.Major;
            this.Minor = v.Minor;
            this.Revision = v.Revision;
            this.Beta = v.Beta;
        }
        else throw new ArgumentException($"Data {Data} is invalid.");
    }

    /// <summary>
    /// Converts a string representation of the <see cref="Version"/> to a structure with the data.
    /// </summary>
    /// <param name="Data">The string data to convert.</param>
    /// <returns>A new <see cref="Version"/> structure filled with relevant information.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Version ToVersion(string Data) =>
        TryParse(Data, out Version vers) ? vers : throw new ArgumentException($"Data {Data} is not valid.");

    /// <summary>
    /// Tries to parsre a string value to a <see cref="Version"/> structure representing the data.
    /// </summary>
    /// <param name="Data">The string value to convert.</param>
    /// <param name="vers">The output <see cref="Version"/> structure.</param>
    /// <returns><see langword="true"/> if the parse was successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidCastException"></exception>
    public static bool TryParse(string Data, out Version vers) {
        if (Data.IsNullEmptyWhitespace()) {
            vers = Empty;
            return false;
        }

        if (Regex.IsMatch(Data, @"^[\d]{1,3}((.[\d]{1,3}){1,2})?(-[\d]{1,3})?$", RegexOptions.Compiled)) {
            byte Minor = 0;
            byte Revision = 0;
            byte Beta = 0;

            if (Data.Contains("-")) {
                if (byte.TryParse(Data.Split('-')[1], out Beta)) {
                    Data = Data.Split('-')[0];
                }
                else throw new ArgumentException($"Cannot use {Data} as a version.");
            }

            string[] Splits = Data.Split('.');
            switch (Splits.Length) {
                case 1 when byte.TryParse(Splits[0], out byte Major): {
                    vers = new(Major, Minor, Revision, Beta);
                } return true;

                case 1: {
                } throw new InvalidCastException($"Cannot use {Splits[0]} as the major.");

                case 2 when byte.TryParse(Splits[0], out byte Major) && byte.TryParse(Splits[1], out Minor): {
                    vers = new(Major, Minor, Revision, Beta);
                } return true;

                case 2: {
                } throw new InvalidCastException($"Cannot use {Splits[1]} as the minor.");

                case 3 when byte.TryParse(Splits[0], out byte Major) && byte.TryParse(Splits[1], out Minor) && byte.TryParse(Splits[2], out Revision): {
                    vers = new(Major, Minor, Revision, Beta);
                } return true;

                case 3: {
                } throw new InvalidCastException($"Cannot use {Splits[2]} as the revision.");

                default: {
                } throw new ArgumentException($"Cannot use {Data} as a version.");
            }
        }
        vers = Empty;
        return false;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Major}.{Minor}.{Revision}{(IsBeta ? $"-{Beta}" : "")}";

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
        obj is Version version && Major == version.Major && Minor == version.Minor && Revision == version.Revision && Beta == version.Beta;

    /// <inheritdoc/>
    public override int GetHashCode() {
        int hashCode = -1072827954;
        hashCode = hashCode * -1521134295 + Major.GetHashCode();
        hashCode = hashCode * -1521134295 + Minor.GetHashCode();
        hashCode = hashCode * -1521134295 + Revision.GetHashCode();
        hashCode = hashCode * -1521134295 + Beta.GetHashCode();
        return hashCode;
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
        return versa == versb || versa >= versb;
    }

    /// <summary>
    /// Determines if the two versions compared are equal to each other.
    /// </summary>
    /// <param name="versa"></param>
    /// <param name="versb"></param>
    /// <returns><see langword="true"/> if the versions are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Version versa, Version versb) =>
        versa.Major == versb.Major && versa.Minor == versb.Minor && versa.Revision == versb.Revision && versa.Beta == versb.Beta;

    /// <summary>
    /// Determines if the two versions compared are not equal to each other.
    /// </summary>
    /// <param name="versa"></param>
    /// <param name="versb"></param>
    /// <returns><see langword="true"/> if the versions are different; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Version versa, Version versb) =>
        versa.Major != versb.Major || versa.Minor != versb.Minor || versa.Revision != versb.Revision || versa.Beta != versb.Beta;

}