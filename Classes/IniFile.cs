// IniFile Base 1.0.0

using System.Drawing;
using System.IO;
using System.Text;

namespace murrty.classes {

    /// <summary>
    /// The class containing the ini file handling.
    /// </summary>
    internal class IniFile {

        /// <summary>
        /// The full path of the Ini File (Generally, in the same folder as the executable)
        /// </summary>
        public string IniPath { get; private set; }

        /// <summary>
        /// The name of the executing file.
        /// </summary>
        private readonly string ExecutableName = "YChanEx";

        /// <summary>
        /// The IniFile Constructor.
        /// </summary>
        /// <param name="NewIniPath">The string path of the ini file. Defaults to executing directory.</param>
        public IniFile(string NewIniPath = null) {
            ChangeIniPath(NewIniPath);
        }

        /// <summary>
        /// Changes the ini path to a new location.
        /// </summary>
        /// <param name="NewIniPath">The full path of the ini.</param>
        public void ChangeIniPath(string NewIniPath = null, bool MoveIni = false) {
            if (NewIniPath != null) {
                if (!NewIniPath.ToLower().EndsWith(".ini")) {
                    NewIniPath += ".ini";
                }
                if (MoveIni && File.Exists(IniPath)) {
                    if (File.Exists(NewIniPath)) {
                        File.Delete(NewIniPath);
                    }
                    File.Move(IniPath, NewIniPath);
                }
            }
            IniPath = new FileInfo(NewIniPath ?? ExecutableName + ".ini").FullName.ToString();
        }

        /// <summary>
        /// Reads a very short string (1 character long) for key verification.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The string of the retrieved key.</returns>
        private string ReadShortString(string Key, string Section = null) {
            StringBuilder Value = new();
            NativeMethods.GetPrivateProfileString(Section ?? ExecutableName, Key, "", Value, 2, IniPath);
            return Value.ToString();
        }

        /// <summary>
        /// Returns a string of from the ini file, used internally.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The string of the retrieved key.</returns>
        private string Read(string Key, string Section = null) {
            StringBuilder Value = new(65535);
            NativeMethods.GetPrivateProfileString(Section ?? ExecutableName, Key, "", Value, (uint)Value.Capacity, IniPath);
            return Value.ToString();
        }

        /// <summary>
        /// Write a simple string to the ini file, used internally.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The string value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        private void WriteString(string Key, string Value, string Section = null) {
            NativeMethods.WritePrivateProfileString(Section ?? ExecutableName, Key, Value, IniPath);
        }

        /// <summary>
        /// Reads a string from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The string of the retrieved key.</returns>
        public string ReadString(string Key, string Section = null) {
            return Read(Key, Section);
        }
        /// <summary>
        /// Reads a boolean from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The bool of the retrieved key. Defaults to false.</returns>
        public bool ReadBool(string Key, string Section = null) {
            return Read(Key, Section).ToLower() switch {
                "true" => true,
                _ => false,
            };
        }
        /// <summary>
        /// Reads an int from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The int of the retrieved key. Defaults to -1.</returns>
        public int ReadInt(string Key, string Section = null) {
            return int.TryParse(Read(Key, Section), out int NewInt) ? NewInt : -1;
        }
        /// <summary>
        /// Reads a decimal from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The decimal of the retrieved key. Defaults to -1.</returns>
        public decimal ReadDecimal(string Key, string Section = null) {
            return decimal.TryParse(Read(Key, Section), out decimal NewDecimal) ? NewDecimal : -1;
        }
        /// <summary>
        /// Reads a Point from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The Point of the retrieved key. Defaults to (-32000, -32000).</returns>
        public Point ReadPoint(string Key, string Section = null) {
            string[] Value = Read(Key, Section).Split(',');
            return Value.Length == 2 && int.TryParse(Value[0], out int OutX) && int.TryParse(Value[1], out int OutY) ? new(OutX, OutY) : new(-32000, -32000);
        }
        /// <summary>
        /// Reads a Size from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The Size of the retrieved key. Defaults to (-32000, -32000).</returns>
        public Size ReadSize(string Key, string Section = null) {
            string[] Value = Read(Key, Section).Split(',');
            return Value.Length == 2 && int.TryParse(Value[0], out int OutW) && int.TryParse(Value[1], out int OutH) ? new(OutW, OutH) : new(-32000, -32000);
        }
        /// <summary>
        /// Reads a Color from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to retrieve.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>The Point of the retrieved key. Defaults to ARGB(255, 128, 128, 128).</returns>
        public Color ReadColor(string Key, string Section = null) {
            string Value = Read(Key, Section).ToLower();
            switch (Value.Length) {
                case 7:
                case 6: // Can contain a pound sign, not enforced.
                    string ColorIntBuffer = "";
                    for (int i = 0; i < Value.Length; i++) {
                        switch (Value[i]) {
                            case '0':
                            case '1': case '2': case '3':
                            case '4': case '5': case '6':
                            case '7': case '8': case '9':
                            case 'a': case 'b': case 'c':
                            case 'd': case 'e': case 'f':
                                ColorIntBuffer += Value[i];
                                break;

                            case '#':
                                continue;

                            default:
                                return Color.FromArgb(255, 128, 128, 128);
                        }
                    }

                    return ColorIntBuffer.Length switch {
                        6 => Color.FromArgb(255,
                                 int.Parse($"{ColorIntBuffer[0]}{ColorIntBuffer[1]}", System.Globalization.NumberStyles.HexNumber),
                                 int.Parse($"{ColorIntBuffer[2]}{ColorIntBuffer[3]}", System.Globalization.NumberStyles.HexNumber),
                                 int.Parse($"{ColorIntBuffer[4]}{ColorIntBuffer[5]}", System.Globalization.NumberStyles.HexNumber)
                             ),

                        _ => Color.FromArgb(255, 128, 128, 128)
                    };

                default:
                    return Color.FromArgb(255, 128, 128, 128);
            }
        }

        /// <summary>
        /// Writes a string to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The string value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, string Value, string Section = null) {
            WriteString(Key, Value, Section);
        }
        /// <summary>
        /// Writes a bool to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The bool value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, bool Value, string Section = null) {
            WriteString(Key, Value ? "True" : "False", Section);
        }
        /// <summary>
        /// Writes an int to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The int value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, int Value, string Section = null) {
            WriteString(Key, Value.ToString(), Section);
        }
        /// <summary>
        /// Writes a decimal to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The string value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, decimal Value, string Section = null) {
            WriteString(Key, Value.ToString(), Section);
        }
        /// <summary>
        /// Writes a Point to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The Point value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, Point Value, string Section = null) {
            WriteString(Key, $"{Value.X},{Value.Y}", Section);
        }
        /// <summary>
        /// Writes a Size to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The Size value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, Size Value, string Section = null) {
            WriteString(Key, $"{Value.Width},{Value.Height}", Section);
        }
        /// <summary>
        /// Writes a Color to the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Value">The Color value to write to the key.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void Write(string Key, Color Value, string Section = null) {
            WriteString(Key, $"#{Value.ToArgb().ToString("X")[2..]}", Section);
        }

        /// <summary>
        /// Deletes a key from the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        public void DeleteKey(string Key, string Section = null) {
            //Write(Key, null, Section ?? ExecutableName);
            NativeMethods.WritePrivateProfileString(Section ?? ExecutableName, Key, null, IniPath);
        }
        /// <summary>
        /// Deletes a sectionfrom the ini file.
        /// </summary>
        /// <param name="Section">The string name of the section to delete. Default to ExecutableName.</param>
        public void DeleteSection(string Section = null) {
            //Write(null, null, Section ?? ExecutableName);
            NativeMethods.WritePrivateProfileString(Section ?? ExecutableName, null, null, IniPath);
        }

        /// <summary>
        /// Checks if a key exists in the ini file.
        /// </summary>
        /// <param name="Key">The string name of the key to write to.</param>
        /// <param name="Section">The string name of the section it's located at. Default to ExecutableName.</param>
        /// <returns>If the key exists in the ini file.</returns>
        public bool KeyExists(string Key, string Section = null) {
            return ReadShortString(Key, Section).Length > 0;
        }

        public bool KeyExists_FullCheck(string Key, string Section = null) {
            using StreamReader Ini = new(IniPath);

            string ReadLine;
            bool InSection = false;

            while ((ReadLine = Ini.ReadLine()) != null) {
                if (InSection) {
                    if (ReadLine.StartsWith("[")) {
                        break;
                    }
                    if (ReadLine.StartsWith(Key + "=")) {
                        return true;
                    }
                }
                else if (ReadLine.StartsWith($"[{Section ?? ExecutableName}]")) {
                    InSection = true;
                }
                else
                    continue;
            }

            return false;
        }

    }

}
