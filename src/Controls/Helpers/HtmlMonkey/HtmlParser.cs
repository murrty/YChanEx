// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Class to parse HTML or XML.
/// </summary>
internal class HtmlParser {
    private readonly TextParser Parser;

    /// <summary>
    /// Enables fixing broken nodes based on a nest-level instead of ignoring them.
    /// It may be best to keep this as <see langword="false"/>, but you may gain success with in enabled.
    /// </summary>
    public HtmlParseOptions Options { get; set; }

    public HtmlParser() {
        Parser = new TextParser(null);
    }

    /// <summary>
    /// Parses an HTML document string and returns a new <see cref="HtmlDocument"/>.
    /// </summary>
    /// <param name="html">The HTML text to parse.</param>
    public HtmlDocument Parse(string? html) {
        return Parse(html, HtmlParseOptions.None);
    }

    /// <summary>
    /// Parses an HTML document string and returns a new <see cref="HtmlDocument"/>.
    /// </summary>
    /// <param name="html">The HTML text to parse.</param>
    /// <param name="options">HTML options that affect parsing.</param>
    public HtmlDocument Parse(string? html, HtmlParseOptions options) {
        this.Options = options;
        HtmlDocument document = new();
        IEnumerable<HtmlNode> Nodes = ParseChildren(html, options);
        document.RootNodes.SetNodes(Nodes);
        return document;
    }

    /// <summary>
    /// Parses the given HTML string into a collection of root nodes and their
    /// children.
    /// </summary>
    /// <param name="html">The HTML text to parse.</param>
    public IEnumerable<HtmlNode> ParseChildren(string? html, HtmlParseOptions options = HtmlParseOptions.None) {
        HtmlElementNode rootNode = new("[Temp]");
        HtmlElementNode parentNode = rootNode;
        Parser.Reset(html);
        bool selfClosing;
        string? tag;

        if (options == HtmlParseOptions.None && this.Options != HtmlParseOptions.None) {
            options = this.Options;
        }

        bool removeEmptyTextNodes = options.HasFlag(HtmlParseOptions.RemoveEmptyTextNodes);
        bool trimTextNodes = options.HasFlag(HtmlParseOptions.TrimTextNodes);
        bool trimAttribValues = options.HasFlag(HtmlParseOptions.TrimAttributeValues);
        bool ignoreHtmlRules = options.HasFlag(HtmlParseOptions.IgnoreHtmlRules);
        bool allowQuotelessAttribs = options.HasFlag(HtmlParseOptions.AllowQuotelessAttributes);
        bool ignoreBrokenHtml = options.HasFlag(HtmlParseOptions.IgnoreBrokenHtml);
        bool useNestedLeveling = options.HasFlag(HtmlParseOptions.UseNestLevels);

        // Possible: Add a 'stack' collection to prevent non-closed nodes from breaking HTML.
        // Additionally, the 'stack' collection should prevent stray end tags from ruining things, too.
        TagStack Hierarchy = [];

        // Loop until end of input
        while (!Parser.EndOfText) {
            if (Parser.Peek() == HtmlRules.TagStart) {
                // CDATA segments (blocks we store but don't parse--includes comments)
                CDataDefinition? definition = HtmlRules.CDataDefinitions.Find(dd => Parser.MatchesCurrentPosition(dd.StartText, dd.StartComparison));
                if (definition != null) {
                    parentNode.Children.Add(ParseCDataNode(definition));
                    continue;
                }

                // Closing tag
                if (Parser.Peek(1) == HtmlRules.ForwardSlash) {
                    Parser.Index += 2;
                    tag = Parser.ParseWhile(HtmlRules.IsTagCharacter);
                    if (tag.Length > 0) {
                        if (parentNode.TagName.Equals(tag, HtmlRules.TagStringComparison)) {
                            // Should never have matched parent if the top-level node
                            if (!parentNode.IsTopLevelNode) {
                                parentNode = parentNode.ParentNode;
                                Hierarchy.Pop();
                            }
                        }
                        else if (!ignoreBrokenHtml) {
                            string? hierarchyTag = Hierarchy.Count > 0 ? Hierarchy.Peek() : null;

                            // Uses the HtmlMonkey-style mismatch handling, using level priority.
                            if (useNestedLeveling) {
                                // Handle mismatched closing tag

                                int tagPriority = HtmlRules.GetTagNestLevel(tag);

                                while (!parentNode.IsTopLevelNode && tagPriority > HtmlRules.GetTagNestLevel(parentNode.TagName))
                                    parentNode = parentNode.ParentNode;

                                if (parentNode.TagName.Equals(tag, HtmlRules.TagStringComparison)) {
                                    if (!parentNode.IsTopLevelNode) {
                                        parentNode = parentNode.ParentNode;
                                    }
                                }
                            }

                            // If the parent node tag does not equal the hierarchy tag, it should find the parent.
                            // If not -- it's a stray.
                            else if (!parentNode.TagName.Equals(hierarchyTag, HtmlRules.TagStringComparison)) {
                                // If the hierarchy has the tag being parsed, any non-finished objects need to be closed off.
                                // This basically pops the hierarchy until the proper tag is found.
                                // If the hierarchy does not contain the node, it's probably a stray end tag.
                                if (Hierarchy.Contains(tag)) {
                                    while (!parentNode.IsTopLevelNode && !parentNode.TagName.Equals(tag, StringComparison.OrdinalIgnoreCase)) {
                                        parentNode = parentNode.ParentNode;
                                        Hierarchy.Pop();
                                    }

                                    // We have the parent node, so we can use the parent of that node.
                                    if (!parentNode.IsTopLevelNode) {
                                        parentNode = parentNode.ParentNode;
                                    }
                                }
                            }
                        }
                    }
                    Parser.SkipTo(HtmlRules.TagEnd);
                    Parser.Next();
                    continue;
                }

                // Open tag
                if (ParseTag(out tag)) {
                    HtmlTagFlag tagFlags = ignoreHtmlRules ? HtmlTagFlag.None : HtmlRules.GetTagFlags(tag);
                    if (tagFlags.HasFlag(HtmlTagFlag.HtmlHeader)) {
                        parentNode.Children.Add(ParseHtmlHeader(allowQuotelessAttribs, trimAttribValues));
                    }
                    else if (tagFlags.HasFlag(HtmlTagFlag.XmlHeader)) {
                        parentNode.Children.Add(ParseXmlHeader(allowQuotelessAttribs, trimAttribValues));
                    }
                    else {
                        // Parse attributes
                        HtmlAttributeCollection attributes = ParseAttributes(allowQuotelessAttribs, trimAttribValues);

                        // Parse rest of tag
                        if (Parser.Peek() == HtmlRules.ForwardSlash) {
                            Parser.Next();
                            Parser.SkipWhiteSpace();
                            selfClosing = true;
                        }
                        else {
                            selfClosing = false;
                        }
                        Parser.SkipTo(HtmlRules.TagEnd);
                        Parser.Next();

                        // Add node
                        HtmlElementNode node = new(tag, attributes);
                        while (!HtmlRules.TagMayContain(parentNode.TagName, tag, this) && !parentNode.IsTopLevelNode) {
                            parentNode = parentNode.ParentNode;
                            Hierarchy.Pop();
                        }
                        parentNode.Children.Add(node);

                        if (tagFlags.HasFlag(HtmlTagFlag.CData)) {
                            // CDATA tags are treated as elements but we store and do not parse the inner content
                            if (!selfClosing) {
                                if (ParseToClosingTag(tag, out string? content) && content.Length > 0) {
                                    node.Children.Add(new HtmlCDataNode(string.Empty, string.Empty, content));
                                }
                            }
                        }
                        else {
                            if (selfClosing && tagFlags.HasFlag(HtmlTagFlag.NoSelfClosing)) {
                                selfClosing = false;
                            }
                            if (!selfClosing && !tagFlags.HasFlag(HtmlTagFlag.NoChildren)) {
                                parentNode = node;  // Node becomes new parent
                                Hierarchy.Push(tag);
                            }
                        }
                    }
                    continue;
                }
            }

            // Text node: must be at least 1 character (includes '<' that was not part of a tag)
            string text = Parser.ParseCharacter();
            text += Parser.ParseTo(HtmlRules.TagStart);
            if (trimTextNodes) {
                text = text.Trim();
            }

            if (removeEmptyTextNodes && string.IsNullOrWhiteSpace(text)) {
                continue;
            }

            HtmlTextNode newNode = new(text);
            parentNode.Children.Add(newNode);
        }

        // Remove references to temporary parent node
        parentNode.Children.ForEach(n => n.ParentNode = null);

        // Return collection of top-level nodes from nodes just parsed
        return parentNode.Children;
    }

    /// <summary>
    /// Attempts to parse an element tag at the current location. If the tag is parsed,
    /// the parser position is advanced to the end of the tag name and true is returned.
    /// Otherwise, false is returned and the current parser position does not change.
    /// </summary>
    /// <param name="tag">Parsed tag name.</param>
    private bool ParseTag([NotNullWhen(true)] out string? tag) {
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
            tag = Parser.Text.Substring(Parser.Index, length);
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
    private HtmlAttributeCollection ParseAttributes(bool allowQuotelessAttribs, bool trimAttribValues) {
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
    private HtmlHeaderNode ParseHtmlHeader(bool allowQuotelessAttribs, bool trimAttribValues) {
        HtmlHeaderNode node = new(ParseAttributes(allowQuotelessAttribs, trimAttribValues));
        const string tagEnd = ">";
        Parser.SkipTo(tagEnd);
        Parser.Index += tagEnd.Length;
        return node;
    }

    /// <summary>
    /// Parses an XML header tag. Assumes current position is just after tag name.
    /// </summary>
    private XmlHeaderNode ParseXmlHeader(bool allowQuotelessAttribs, bool trimAttribValues) {
        XmlHeaderNode node = new(ParseAttributes(allowQuotelessAttribs, trimAttribValues));
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
    private bool ParseToClosingTag(string tag, [NotNullWhen(true)] out string? content) {
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
    private HtmlCDataNode ParseCDataNode(CDataDefinition definition) {
        Debug.Assert(Parser.MatchesCurrentPosition(definition.StartText, definition.StartComparison));
        Parser.Index += definition.StartText.Length;
        string content = Parser.ParseTo(definition.EndText, definition.EndComparison);
        Parser.Index += definition.EndText.Length;
        return new HtmlCDataNode(definition.StartText, definition.EndText, content);
    }
}
