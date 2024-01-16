#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Diagnostics.CodeAnalysis;

using System.Diagnostics;
using System.IO;
internal class HtmlStreamParser {
    private readonly StreamParser Parser;

    /// <summary>
    /// Parsing options that affect how the parser handles the outcome of the document.
    /// </summary>
    public HtmlParseOptions Options { get; set; }

    public HtmlStreamParser() {
        Parser = new StreamParser(null);
    }

    /// <summary>
    /// Parses an HTML document string and returns a new <see cref="HtmlDocument"/>.
    /// </summary>
    /// <param name="stream">The stream to parse.</param>
    public HtmlDocument Parse(Stream stream) {
        return Parse(stream, HtmlParseOptions.None);
    }

    /// <summary>
    /// Parses an HTML document string and returns a new <see cref="HtmlDocument"/>.
    /// </summary>
    /// <param name="stream">The stream to parse.</param>
    /// <param name="options">HTML options that affect parsing.</param>
    public HtmlDocument Parse(Stream stream, HtmlParseOptions options) {
        this.Options = options;
        HtmlDocument document = new();
        IEnumerable<HtmlNode> Nodes = ParseChildren(stream, options);
        document.RootNodes.SetNodes(Nodes);
        return document;
    }

    /// <summary>
    /// Parses the given HTML string into a collection of root nodes and their
    /// children.
    /// </summary>
    /// <param name="stream">The stream to parse.</param>
    public IEnumerable<HtmlNode> ParseChildren(Stream stream, HtmlParseOptions options = HtmlParseOptions.None) {
        HtmlElementNode rootNode = new("[Temp]");
        HtmlElementNode parentNode = rootNode;
        Parser.Reset(stream);
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
                    parentNode.Children.Add(HtmlParser.ParseCDataNode(definition, Parser));
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
                if (HtmlParser.ParseTag(out tag, Parser)) {
                    HtmlTagFlag tagFlags = ignoreHtmlRules ? HtmlTagFlag.None : HtmlRules.GetTagFlags(tag);
                    if (tagFlags.HasFlag(HtmlTagFlag.HtmlHeader)) {
                        parentNode.Children.Add(HtmlParser.ParseHtmlHeader(allowQuotelessAttribs, trimAttribValues, Parser));
                    }
                    else if (tagFlags.HasFlag(HtmlTagFlag.XmlHeader)) {
                        parentNode.Children.Add(HtmlParser.ParseXmlHeader(allowQuotelessAttribs, trimAttribValues, Parser));
                    }
                    else {
                        // Parse attributes
                        HtmlAttributeCollection attributes = HtmlParser.ParseAttributes(allowQuotelessAttribs, trimAttribValues, Parser);

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
                        while (!HtmlRules.TagMayContain(parentNode.TagName, tag, options) && !parentNode.IsTopLevelNode) {
                            parentNode = parentNode.ParentNode;
                            Hierarchy.Pop();
                        }
                        parentNode.Children.Add(node);

                        if (tagFlags.HasFlag(HtmlTagFlag.CData)) {
                            // CDATA tags are treated as elements but we store and do not parse the inner content
                            if (!selfClosing) {
                                if (HtmlParser.ParseToClosingTag(tag, out string? content, Parser) && content.Length > 0) {
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
}
