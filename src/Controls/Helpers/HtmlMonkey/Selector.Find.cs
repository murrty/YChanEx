#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Collections.Generic;
using System.Linq;
public partial class Selector {
    #region Find
    /// <summary>
    /// Recursively searches the given root node and returns the nodes that matches this
    /// selector.
    /// </summary>
    /// <param name="htDoc">Html document to search.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(HtmlDocument htDoc) {
        return Find(htDoc.RootNodes);
    }

    /// <summary>
    /// Recursively searches the given root node and returns the nodes that matches this
    /// selector.
    /// </summary>
    /// <param name="rootNode">Root node to search.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(HtmlNode rootNode) => Find([ rootNode ]);

    /// <summary>
    /// Recursively searches the list of nodes and returns the nodes that matches this
    /// selector.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(IEnumerable<HtmlNode> nodes) {
        List<HtmlElementNode>? results = null;
        bool matchTopLevelNodes = true;

        // Search from this selector on down through its child selectors
        for (Selector? selector = this; selector != null; selector = selector.ChildSelector) {
            results = [];
            FindRecursive(nodes, selector, matchTopLevelNodes, !this.ImmediateChildOnly, results);
            // In next iteration, apply nodes that matched this iteration
            nodes = results;
            matchTopLevelNodes = false;
        }
        return results?.Distinct() ?? Enumerable.Empty<HtmlElementNode>();
    }
    #endregion Find

    #region First
    /// <summary>
    /// Recursively searches the given root node and returns the first node that matches this
    /// selector.
    /// </summary>
    /// <param name="htDoc">Html document to search.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(HtmlDocument htDoc) => Find(htDoc).First();

    /// <summary>
    /// Recursively searches the given root node and returns the first node that matches this
    /// selector.
    /// </summary>
    /// <param name="rootNode">Root node to search.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(HtmlNode rootNode) => Find(rootNode).First();

    /// <summary>
    /// Recursively searches the list of nodes and returns the first node that matches this
    /// selector.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(IEnumerable<HtmlNode> nodes) => Find(nodes).First();
    #endregion First

    #region FirstOrDefault
    /// <summary>
    /// Recursively searches the given root node and returns the first node that matches this
    /// selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="htDoc">Html document to search.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(HtmlDocument htDoc) => Find(htDoc).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given root node and returns the first node that matches this
    /// selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="rootNode">Root node to search.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(HtmlNode rootNode) => Find(rootNode).FirstOrDefault();

    /// <summary>
    /// Recursively searches the list of nodes and returns the first node that matches this
    /// selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(IEnumerable<HtmlNode> nodes) => Find(nodes).FirstOrDefault();
    #endregion FirstOrDefault
}
