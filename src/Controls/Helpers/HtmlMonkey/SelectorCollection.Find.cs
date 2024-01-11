#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Collections.Generic;
using System.Linq;
public partial class SelectorCollection {
    #region Find
    /// <summary>
    /// Recursively searches the HtmlDocument using this list of selectors.
    /// Returns the matching nodes. Ensures no duplicate nodes are returned.
    /// </summary>
    /// <param name="htmlDoc">The html document to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public IEnumerable<HtmlElementNode> Find(HtmlDocument htmlDoc) {
        return this.SelectMany(s => s.Find(htmlDoc.RootNodes))
            .Distinct();
    }

    /// <summary>
    /// Recursively searches the given root node using this list of selectors.
    /// Returns the matching nodes. Ensures no duplicate nodes are returned.
    /// </summary>
    /// <param name="rootNode">Root node of nodes to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public IEnumerable<HtmlElementNode> Find(HtmlNode rootNode) => Find([ rootNode ]);

    /// <summary>
    /// Recursively searches the given list of nodes using this list of selectors.
    /// Returns the matching nodes. Ensures no duplicate nodes are returned.
    /// </summary>
    /// <param name="nodes">The set of nodes to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public IEnumerable<HtmlElementNode> Find(IEnumerable<HtmlNode> nodes) {
        return this.SelectMany(s => s.Find(nodes))
            .Distinct();
    }
    #endregion Find

    #region First
    /// <summary>
    /// Recursively searches the HtmlDocument using this list of selectors.
    /// Returns the first matching node.
    /// </summary>
    /// <param name="htmlDoc">The html document to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public HtmlElementNode First(HtmlDocument htmlDoc) => Find(htmlDoc).First();

    /// <summary>
    /// Recursively searches the given root node using this list of selectors.
    /// Returns the first matching node.
    /// </summary>
    /// <param name="rootNode">Root node of nodes to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public HtmlElementNode First(HtmlNode rootNode) => Find(rootNode).First();

    /// <summary>
    /// Recursively searches the given list of nodes using this list of selectors.
    /// Returns the first matching node.
    /// </summary>
    /// <param name="nodes">The set of nodes to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public HtmlElementNode First(IEnumerable<HtmlNode> nodes) => Find(nodes).First();
    #endregion First

    #region FirstOrDefault
    /// <summary>
    /// Recursively searches the HtmlDocument using this list of selectors.
    /// Returns the first matching node, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="htmlDoc">The html document to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public HtmlElementNode? FirstOrDefault(HtmlDocument htmlDoc) => Find(htmlDoc).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given root node using this list of selectors.
    /// Returns the first matching node, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="rootNode">Root node of nodes to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public HtmlElementNode? FirstOrDefault(HtmlNode rootNode) => Find(rootNode).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given list of nodes using this list of selectors.
    /// Returns the first matching node, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The set of nodes to search.</param>
    /// <returns>A set of nodes that matches this selector collection.</returns>
    public HtmlElementNode? FirstOrDefault(IEnumerable<HtmlNode> nodes) => Find(nodes).FirstOrDefault();
    #endregion FirstOrDefault
}
