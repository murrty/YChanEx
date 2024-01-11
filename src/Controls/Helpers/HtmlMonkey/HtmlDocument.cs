// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.IO;
using System.Text;
/// <summary>
/// Holds the nodes of a parsed HTML or XML document. Use the <see cref="RootNodes"/>
/// property to access these nodes. Use the <see cref="ToHtml"/> method to convert the
/// nodes back to markup.
/// </summary>
public partial class HtmlDocument {
    /// <summary>
    /// Gets the source document path. May be empty or <c>null</c> if there was no
    /// source file.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Gets the document root nodes. Provides access to all document nodes.
    /// </summary>
    public HtmlNodeCollection RootNodes { get; }

    /// <summary>
    /// Gets or sets whether the library enforces HTML rules when parsing markup.
    /// This setting is global for all instances of this class.
    /// </summary>
    public static bool IgnoreHtmlRules {
        get => HtmlRules.IgnoreHtmlRules;
        set => HtmlRules.IgnoreHtmlRules = value;
    }

    /// <summary>
    /// Initializes an empty <see cref="HtmlDocument"> instance.
    /// </summary>
    public HtmlDocument() {
        Path = null;
        RootNodes = new HtmlNodeCollection(null);
    }

    /// <summary>
    /// Generates an HTML string from the contents of this <see cref="HtmlDocument"></see>.
    /// </summary>
    /// <returns>A string with the markup for this document.</returns>
    public string ToHtml() => this.RootNodes.ToHtml();

    #region Static methods
    /// <summary>
    /// Parses an HTML or XML string and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="html">The HTML or XML string to parse.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static HtmlDocument FromHtml(string? html, HtmlParseOptions options = HtmlParseOptions.None) {
        HtmlParser Parser = new();
        HtmlDocument doc = Parser.Parse(html, options);
        return doc;
    }

    /// <summary>
    /// Parses an HTML or XML file and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="path">The HTML or XML file to parse.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static HtmlDocument FromFile(string path, HtmlParseOptions options = HtmlParseOptions.None) {
        return FromHtml(File.ReadAllText(path), options);
    }

    /// <summary>
    /// Parses an HTML or XML file and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="path">The HTML or XML file to parse.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static HtmlDocument FromFile(string path, Encoding encoding, HtmlParseOptions options = HtmlParseOptions.None) {
        return FromHtml(File.ReadAllText(path, encoding), options);
    }
    #endregion
}
