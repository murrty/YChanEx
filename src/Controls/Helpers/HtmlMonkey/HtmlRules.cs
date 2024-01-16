// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;

using System;
using System.Collections.Generic;

/// <summary>
/// Defines element tag attributes.
/// </summary>
[Flags]
internal enum HtmlTagFlag : byte {
    /// <summary>
    /// Specifies no flags.
    /// </summary>
    None = 0x00,
    /// <summary>
    /// Is an HTML DOCTYPE document header tag
    /// </summary>
    HtmlHeader = 0x01,
    /// <summary>
    /// Is an XML document header tag
    /// </summary>
    XmlHeader = 0x02,
    /// <summary>
    /// Element cannot contain child nodes.
    /// </summary>
    NoChildren = 0x04,
    /// <summary>
    /// Element cannot contain element of same type
    /// </summary>
    NoNested = 0x08,
    /// <summary>
    /// Element cannot be self-closing.
    /// </summary>
    NoSelfClosing = 0x10,
    /// <summary>
    /// Element content is saved but not parsed, and may contain anything.
    /// </summary>
    CData = 0x20,
}

/// <summary>
/// Defines constants and rules that are used to parse and interpret
/// HTML and XML.
/// </summary>
internal static class HtmlRules {
    #region Constant values
    public static readonly string HtmlHeaderTag = "!doctype";
    public static readonly string XmlHeaderTag = "?xml";

    public static readonly char TagStart = '<';
    public static readonly char TagEnd = '>';
    public static readonly char ForwardSlash = '/';

    public static readonly char DoubleQuote = '"';
    public static readonly char SingleQuote = '\'';

    public static List<CDataDefinition> CDataDefinitions = [
        new CDataDefinition {
            StartText = "<!--",
            EndText = "-->",
            StartComparison = StringComparison.Ordinal,
            EndComparison = StringComparison.Ordinal
        },
        new CDataDefinition {
            StartText = "<![CDATA[",
            EndText = "]]>",
            StartComparison = StringComparison.OrdinalIgnoreCase,
            EndComparison = StringComparison.Ordinal
        },
    ];

    public static readonly StringComparison TagStringComparison = StringComparison.CurrentCultureIgnoreCase;
    public static readonly StringComparer TagStringComparer = StringComparer.CurrentCultureIgnoreCase;
    #endregion

    #region String and character classification
    /// <summary>
    /// Returns true if <paramref name="c"/> is a single or double quote character.
    /// </summary>
    /// <param name="c">Character to test.</param>
    public static bool IsQuoteChar(char c) => c == DoubleQuote || c == SingleQuote;

    private static readonly HashSet<char> InvalidChars;

    static HtmlRules() {
        // Characters that are not valid within tag and attribute names (excluding whitespace and control characters)
        InvalidChars = [
            '!', '?', '<', '"',
            '\'', '>', '/', '='
        ];

        for (int i = 0xfdd0; i <= 0xfdef; i++) {
            InvalidChars.Add((char)i);
        }

        InvalidChars.Add('\ufffe');
        InvalidChars.Add('\uffff');
    }

    /// <summary>
    /// Returns true if <paramref name="c"/> is a valid tag name character.
    /// </summary>
    /// <param name="c">Character to test.</param>
    public static bool IsTagCharacter(char c) {
        return !InvalidChars.Contains(c) && !char.IsControl(c) && !char.IsWhiteSpace(c);
    }

    /// <summary>
    /// Returns true if <paramref name="c"/> is a valid attribue name character.
    /// </summary>
    /// <param name="c">Character to test.</param>
    public static bool IsAttributeNameCharacter(char c) {
        return !InvalidChars.Contains(c) && !char.IsControl(c) && !char.IsWhiteSpace(c);
    }

    /// <summary>
    /// Returns true if <paramref name="c"/> is a valid unquoted attribue value character.
    /// </summary>
    /// <param name="c">Character to test.</param>
    public static bool IsAttributeValueCharacter(char c) {
        return !InvalidChars.Contains(c) && !char.IsControl(c) && !char.IsWhiteSpace(c);
    }

    public static bool IsNonQuotedAttribute(char c) => c is not ' ' or '>' or '"';
    #endregion

    #region Tag classification
    /// <summary>
    /// Specifies whether or not HTML rules are enforced. When true, <see cref="GetTagFlags(string)"/>
    /// always returns <see cref="HtmlTagFlag.None"/>.
    /// </summary>
    public static bool IgnoreHtmlRules = false;

    /// <summary>
    /// Defines tag attributes for element tags.
    /// </summary>
    private static readonly Dictionary<string, HtmlTagFlag> TagRules = new(StringComparer.CurrentCultureIgnoreCase) {
        ["!doctype"] = HtmlTagFlag.HtmlHeader,
        ["?xml"] = HtmlTagFlag.XmlHeader,
        ["a"] = HtmlTagFlag.NoNested,
        ["area"] = HtmlTagFlag.NoChildren,
        ["base"] = HtmlTagFlag.NoChildren,
        ["basefont"] = HtmlTagFlag.NoChildren,
        ["bgsound"] = HtmlTagFlag.NoChildren,
        ["br"] = HtmlTagFlag.NoChildren,
        ["col"] = HtmlTagFlag.NoChildren,
        ["dd"] = HtmlTagFlag.NoNested,
        ["dt"] = HtmlTagFlag.NoNested,
        ["embed"] = HtmlTagFlag.NoChildren,
        ["frame"] = HtmlTagFlag.NoChildren,
        ["hr"] = HtmlTagFlag.NoChildren,
        ["img"] = HtmlTagFlag.NoChildren,
        ["input"] = HtmlTagFlag.NoChildren,
        ["isindex"] = HtmlTagFlag.NoChildren,
        ["keygen"] = HtmlTagFlag.NoChildren,
        ["li"] = HtmlTagFlag.NoNested,
        ["link"] = HtmlTagFlag.NoChildren,
        ["menuitem"] = HtmlTagFlag.NoChildren,
        ["meta"] = HtmlTagFlag.NoChildren,
        ["noxhtml"] = HtmlTagFlag.CData,
        ["p"] = HtmlTagFlag.NoNested,
        ["param"] = HtmlTagFlag.NoChildren,
        ["script"] = HtmlTagFlag.CData,
        ["select"] = HtmlTagFlag.NoSelfClosing,
        ["source"] = HtmlTagFlag.NoChildren,
        ["spacer"] = HtmlTagFlag.NoChildren,
        ["style"] = HtmlTagFlag.CData,
        ["table"] = HtmlTagFlag.NoNested,
        ["td"] = HtmlTagFlag.NoNested,
        ["th"] = HtmlTagFlag.NoNested,
        ["textarea"] = HtmlTagFlag.NoSelfClosing,
        ["track"] = HtmlTagFlag.NoChildren,
        ["wbr"] = HtmlTagFlag.NoChildren,
    };

    /// <summary>
    /// Returns the attribute flags for the given tag.
    /// </summary>
    public static HtmlTagFlag GetTagFlags(string tag) {
        if (!IgnoreHtmlRules && TagRules.TryGetValue(tag, out HtmlTagFlag flags)) {
            return flags;
        }
        return HtmlTagFlag.None;
    }

    /// <summary>
    /// Defines element tag nesting values. Tags cannot appear within tags with a lower value. Used when parsing to detected
    /// mismatches open/close tags.
    /// </summary>
    private static readonly Dictionary<string, int> NestLevelLookup = new(StringComparer.CurrentCultureIgnoreCase) {
        ["div"] = 150,
        ["td"] = 160,
        ["th"] = 160,
        ["tr"] = 170,
        ["thead"] = 180,
        ["tbody"] = 180,
        ["tfoot"] = 180,
        ["table"] = 190,
        ["head"] = 200,
        ["body"] = 200,
        ["html"] = 220,
    };

    /// <summary>
    /// Returns a value that signifies the relative nest level of the specified tag. Tags with higher values
    /// cannot be contained within tags with lower levels.
    /// </summary>
    /// <param name="tag">The element tag for which to get the nest level.</param>
    public static int GetTagNestLevel(string tag) => NestLevelLookup.TryGetValue(tag, out int priority) ? priority : 100;
    #endregion

    #region Tag nesting rules logic
    /// <summary>
    /// Returns true if it is valid for the given parent tag to contain the given child tag.
    /// </summary>
    public static bool TagMayContain(string parentTag, string childTag, HtmlParseOptions options) {
        return TagMayContain(parentTag, childTag, GetTagFlags(parentTag), options);
    }

    /// <summary>
    /// Returns true if it is considered valid for the given parent tag to contain the
    /// given child tag. Provide the parent flags, if available, to improve performance.
    /// </summary>
    public static bool TagMayContain(string parentTag, string childTag, HtmlTagFlag parentFlags, HtmlParseOptions options) {
        if (parentFlags.HasFlag(HtmlTagFlag.NoChildren)) {
            return false;
        }

        if (parentFlags.HasFlag(HtmlTagFlag.NoNested) && parentTag.Equals(childTag, TagStringComparison)) {
            return false;
        }

        // Attempt to catch mismatched open/close tags
        if (options.HasFlag(HtmlParseOptions.UseNestLevels) && GetTagNestLevel(childTag) > GetTagNestLevel(parentTag)) {
            return false;
        }

        return true;
    }
    #endregion
}
