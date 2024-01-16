#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
internal static class HtmlParser {
    /// <summary>
    /// Represents an invalid character. This character is returned when attempting to read
    /// a character at an invalid position. The character value is <c>'\0'</c>.
    /// </summary>
    public const char NullChar = '\0';

    /// <summary>
    /// Attempts to parse an element tag at the current location. If the tag is parsed,
    /// the parser position is advanced to the end of the tag name and true is returned.
    /// Otherwise, false is returned and the current parser position does not change.
    /// </summary>
    /// <param name="tag">Parsed tag name.</param>
    internal static bool ParseTag<T>([NotNullWhen(true)] out string? tag, ITextParser<T> Parser) {
        tag = null;
        int pos = 0;

        Debug.Assert(Parser.Peek() == HtmlRules.TagStart);
        char c = Parser.Peek(++pos);
        if (c == '!' || c == '?') {
            c = Parser.Peek(++pos);
        }

        if (HtmlRules.IsTagCharacter(c)) {
            while (HtmlRules.IsTagCharacter(Parser.Peek(++pos))) ;
            // Move past '<'
            Parser.Next();
            // Extract tag name
            int length = pos - 1;
            tag = Parser.Substring(Parser.Index, length);
            Parser.Index += length;
            return true;
        }
        // No tag found at this position
        return false;
    }

    /// <summary>
    /// Parses the attributes of an element tag. When finished, the parser
    /// position is at the next non-space character that follows the attributes.
    /// </summary>
    internal static HtmlAttributeCollection ParseAttributes<T>(bool allowQuotelessAttribs, bool trimAttribValues, ITextParser<T> Parser) {
        HtmlAttributeCollection attributes = [];

        // Parse tag attributes
        Parser.SkipWhiteSpace();
        char ch = Parser.Peek();
        while (HtmlRules.IsAttributeNameCharacter(ch) || HtmlRules.IsQuoteChar(ch)) {
            // Parse attribute name
            HtmlAttribute attribute = new();
            if (HtmlRules.IsQuoteChar(ch)) {
                attribute.Name = $"\"{Parser.ParseQuotedText()}\"";
            }
            else {
                attribute.Name = Parser.ParseWhile(HtmlRules.IsAttributeNameCharacter);
            }
            Debug.Assert(attribute.Name.Length > 0);

            // Parse attribute value
            Parser.SkipWhiteSpace();
            if (Parser.Peek() == '=') {
                Parser.Next(); // Skip '='
                Parser.SkipWhiteSpace();
                if (HtmlRules.IsQuoteChar(Parser.Peek())) {
                    // Quoted attribute value
                    attribute.Value = Parser.ParseQuotedText();
                }
                else if (allowQuotelessAttribs) {
                    // Alternate method of handling non-quoted attributes (they are allowed, somehow.)
                    // Easiest way to read it is to read until a space or exit tag '>'.
                    attribute.Value = Parser.ParseWhile(HtmlRules.IsNonQuotedAttribute);
                    Debug.Assert(attribute.Value.Length > 0);
                }
                else {
                    attribute.Value = Parser.ParseWhile(HtmlRules.IsAttributeValueCharacter);
                    Debug.Assert(attribute.Value.Length > 0);
                }

                if (trimAttribValues) {
                    attribute.Value = attribute.Value.Trim();
                    if (string.IsNullOrWhiteSpace(attribute.Value)) {
                        attribute.Value = null;
                    }
                }
            }
            else {
                // Null attribute value indicates no equals sign
                attribute.Value = null;
            }
            // Add attribute to tag
            attributes.Add(attribute);
            // Continue
            Parser.SkipWhiteSpace();
            ch = Parser.Peek();
        }
        return attributes;
    }

    /// <summary>
    /// Parses an HTML DOCTYPE header tag. Assumes current position is just after tag name.
    /// </summary>
    internal static HtmlHeaderNode ParseHtmlHeader<T>(bool allowQuotelessAttribs, bool trimAttribValues, ITextParser<T> Parser) {
        HtmlHeaderNode node = new(ParseAttributes(allowQuotelessAttribs, trimAttribValues, Parser));
        const string tagEnd = ">";
        Parser.SkipTo(tagEnd);
        Parser.Index += tagEnd.Length;
        return node;
    }

    /// <summary>
    /// Parses an XML header tag. Assumes current position is just after tag name.
    /// </summary>
    internal static XmlHeaderNode ParseXmlHeader<T>(bool allowQuotelessAttribs, bool trimAttribValues, ITextParser<T> Parser) {
        XmlHeaderNode node = new(ParseAttributes(allowQuotelessAttribs, trimAttribValues, Parser));
        const string tagEnd = "?>";
        Parser.SkipTo(tagEnd);
        Parser.Index += tagEnd.Length;
        return node;
    }

    /// <summary>
    /// Moves the parser position to the closing tag for the given tag name.
    /// If the closing tag is not found, the parser position is set to the end
    /// of the text and false is returned.
    /// </summary>
    /// <param name="tag">Tag name for which the closing tag is being searched.</param>
    /// <param name="content">Returns the content before the closing tag.</param>
    /// <returns></returns>
    internal static bool ParseToClosingTag<T>(string tag, [NotNullWhen(true)] out string? content, ITextParser<T> Parser) {
        string endTag = $"</{tag}";
        int start = Parser.Index;

        // Position assumed to just after open tag
        Debug.Assert(Parser.Index > 0 && Parser.Peek(-1) == HtmlRules.TagEnd);
        while (!Parser.EndOfText) {
            Parser.SkipTo(endTag, StringComparison.OrdinalIgnoreCase);
            // Check that we didn't just match the first part of a longer tag
            if (!HtmlRules.IsTagCharacter(Parser.Peek(endTag.Length))) {
                content = Parser.Extract(start, Parser.Index);
                Parser.Index += endTag.Length;
                Parser.SkipTo(HtmlRules.TagEnd);
                Parser.Next();
                return true;
            }
            Parser.Next();
        }
        content = null;
        return false;
    }

    /// <summary>
    /// Parses a CDATA block, which includes any comment, etc. where we do not process its content.
    /// </summary>
    /// <param name="definition">Definition for this type of CDATA.</param>
    /// <returns></returns>
    internal static HtmlCDataNode ParseCDataNode<T>(CDataDefinition definition, ITextParser<T> Parser) {
        Debug.Assert(Parser.MatchesCurrentPosition(definition.StartText, definition.StartComparison));
        Parser.Index += definition.StartText.Length;
        string content = Parser.ParseTo(definition.EndText, definition.EndComparison);
        Parser.Index += definition.EndText.Length;
        return new HtmlCDataNode(definition.StartText, definition.EndText, content);
    }
}
