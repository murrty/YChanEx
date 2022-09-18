// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace SoftCircuits.HtmlMonkey;

using System.IO;
using System.Text;

/// <summary>
/// Holds the nodes of a parsed HTML or XML document. Use the <see cref="RootNodes"/>
/// property to access these nodes. Use the <see cref="ToHtml"/> method to convert the
/// nodes back to markup.
/// </summary>
public class HtmlDocument {
    /// <summary>
    /// Gets the source document path. May be empty or <c>null</c> if there was no
    /// source file.
    /// </summary>
    public string? Path { get; private set; }

    /// <summary>
    /// Gets the document root nodes. Provides access to all document nodes.
    /// </summary>
    public HtmlNodeCollection RootNodes { get; private set; }

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
    /// Recursively searches this document's nodes for ones matching the specified selector.
    /// </summary>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(string? selector) => RootNodes.Find(selector);

    /// <summary>
    /// Recursively searches this document's nodes for ones matching the specified compiled
    /// selectors.
    /// </summary>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(SelectorCollection selectors) => RootNodes.Find(selectors);

    /// <summary>
    /// Recursively finds all HtmlNodes in this document for which the given predicate returns true.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included
    /// in the results.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlNode> Find(Func<HtmlNode, bool> predicate) => RootNodes.Find(predicate);

    /// <summary>
    /// Recursively finds all nodes of the specified type.
    /// </summary>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<T> FindOfType<T>() where T : HtmlNode => RootNodes.FindOfType<T>();

    /// <summary>
    /// Recursively finds all nodes of the specified type, and for which the given predicate returns true.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in
    /// the results.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<T> FindOfType<T>(Func<T, bool> predicate) where T : HtmlNode => RootNodes.FindOfType<T>(predicate);

    /// <summary>
    /// Generates an HTML string from the contents of this <see cref="HtmlDocument"></see>.
    /// </summary>
    /// <returns>A string with the markup for this document.</returns>
    public string ToHtml() => RootNodes.ToHtml();

    #region Static methods

    /// <summary>
    /// Parses an HTML or XML string and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="html">The HTML or XML string to parse.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static HtmlDocument FromHtml(string? html) => new HtmlParser().Parse(html);

    /// <summary>
    /// Parses an HTML or XML file and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="path">The HTML or XML file to parse.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static HtmlDocument FromFile(string path) => FromHtml(File.ReadAllText(path));

    /// <summary>
    /// Parses an HTML or XML file and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="path">The HTML or XML file to parse.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static HtmlDocument FromFile(string path, Encoding encoding) => FromHtml(File.ReadAllText(path, encoding));

    #endregion

}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.