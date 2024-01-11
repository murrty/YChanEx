namespace SoftCircuits.HtmlMonkey;
/// <summary>
/// Options you can set when parsing HTML. You may 'OR' these options together.
/// </summary>
[System.Flags]
public enum HtmlParseOptions : byte {
    /// <summary>
    /// Parses HTML using defaults.
    /// </summary>
    None = 0x0,
    /// <summary>
    /// Remove empty nodes from the resulting HTML document.
    /// This may remove white-space in places you may expect it.
    /// </summary>
    RemoveEmptyTextNodes = 0x1,
    /// <summary>
    /// Trims text nodes removing any whitespace characters at the start and end.
    /// </summary>
    TrimTextNodes = 0x2,
    /// <summary>
    /// Trims attribute values removing any whitespace characters at the start and end.
    /// </summary>
    TrimAttributeValues = 0x4,
    /// <summary>
    /// Whether to ignore HTML rules when parsing.
    /// </summary>
    IgnoreHtmlRules = 0x8,
    /// <summary>
    /// Whether attributes are allowed to be quoteless.
    /// </summary>
    AllowQuotelessAttributes = 0x10,
    /// <summary>
    /// Whether broken HTML nodes will be ignored. This may fix some sites, but break others. It may also affect how HTML is re-generated.
    /// </summary>
    IgnoreBrokenHtml = 0x20,
    /// <summary>
    /// Whether to enable using nested leveling when broken nodes.
    /// It's best to not use this and to ignore mismatched nodes, but you may find success with it.
    /// Ignored if <see cref="IgnoreBrokenHtml"/> is specified.
    /// </summary>
    UseNestLevels = 0x40,
}
