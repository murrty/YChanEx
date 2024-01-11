// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
using System.Collections.Generic;
using System.Linq;
public static class HtmlExtensionMethods {
    #region Find
    /// <summary>
    /// Recursively finds all nodes for which the given predicate returns true.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    /// <remarks>
    /// Implemented without recursion for better performance on deeply nested collections.
    /// </remarks>
    public static IEnumerable<HtmlNode> Find(this IEnumerable<HtmlNode> nodes, Func<HtmlNode, bool> predicate) {
        var stack = new Stack<IEnumerator<HtmlNode>>();
        var enumerator = nodes.GetEnumerator();

        try {
            while (true) {
                if (enumerator.MoveNext()) {
                    HtmlNode node = enumerator.Current;
                    if (predicate(node)) {
                        yield return node;
                    }

                    if (node is HtmlElementNode elementNode) {
                        stack.Push(enumerator);
                        enumerator = elementNode.Children.GetEnumerator();
                    }
                }
                else if (stack.Count > 0) {
                    enumerator.Dispose();
                    enumerator = stack.Pop();
                }
                else {
                    yield break;
                }
            }
        }
        finally {
            enumerator.Dispose();

            // Dispose enumerators in case of exception
            while (stack.Count > 0) {
                enumerator = stack.Pop();
                enumerator.Dispose();
            }
        }
    }

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified selectors.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static IEnumerable<HtmlElementNode> Find(this IEnumerable<HtmlNode> nodes, string? selector) {
        SelectorCollection selectors = Selector.ParseSelectors(selector);
        return selectors.Find(nodes);
    }

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified compiled selectors.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static IEnumerable<HtmlElementNode> Find(this IEnumerable<HtmlNode> nodes, Selector selector) => selector.Find(nodes);

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified compiled selectors.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Pre-compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static IEnumerable<HtmlElementNode> Find(this IEnumerable<HtmlNode> nodes, DefaultSelectors.DefaultSelector selector) => selector._selector.Find(nodes);

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified compiled selectors.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static IEnumerable<HtmlElementNode> Find(this IEnumerable<HtmlNode> nodes, SelectorCollection selectors) => selectors.Find(nodes);

    /// <summary>
    /// Recursively finds all nodes of the specified type.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <returns>The matching nodes.</returns>
    public static IEnumerable<T> FindOfType<T>(this IEnumerable<HtmlNode> nodes) where T : HtmlNode => Find(nodes, n => n is T).Cast<T>();

    /// <summary>
    /// Recursively finds all nodes of the specified type for which the given predicate returns true.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    public static IEnumerable<T> FindOfType<T>(this IEnumerable<HtmlNode> nodes, Func<T, bool> predicate) where T : HtmlNode => Find(nodes, n => n is T node && predicate(node)).Cast<T>();
    #endregion Find

    #region First
    /// <summary>
    /// Recursively finds the first node for which the given predicate returns true.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    /// <remarks>
    /// Implemented without recursion for better performance on deeply nested collections.
    /// </remarks>
    public static HtmlNode First(this IEnumerable<HtmlNode> nodes, Func<HtmlNode, bool> predicate) => Find(nodes, predicate).First();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified selectors.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode First(this IEnumerable<HtmlNode> nodes, string? selector) => Find(nodes, selector).First();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode First(this IEnumerable<HtmlNode> nodes, Selector selector) => Find(nodes, selector).First();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Pre-compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode First(this IEnumerable<HtmlNode> nodes, DefaultSelectors.DefaultSelector selector) => Find(nodes, selector).First();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selectors.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode First(this IEnumerable<HtmlNode> nodes, SelectorCollection selectors) => Find(nodes, selectors).First();

    /// <summary>
    /// Recursively finds the first node of the specified type.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <returns>The matching nodes.</returns>
    public static T FirstOfType<T>(this IEnumerable<HtmlNode> nodes) where T : HtmlNode => FindOfType<T>(nodes).First();

    /// <summary>
    /// Recursively finds the first node of the specified type for which the given predicate returns true.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    public static T FirstOfType<T>(this IEnumerable<HtmlNode> nodes, Func<T, bool> predicate) where T : HtmlNode => FindOfType<T>(nodes, predicate).First();
    #endregion First

    #region FirstOrDefault
    /// <summary>
    /// Recursively finds the first node for which the given predicate returns true, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    /// <remarks>
    /// Implemented without recursion for better performance on deeply nested collections.
    /// </remarks>
    public static HtmlNode? FirstOrDefault(this IEnumerable<HtmlNode> nodes, Func<HtmlNode, bool> predicate) => Find(nodes, predicate).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified selectors, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode? FirstOrDefault(this IEnumerable<HtmlNode> nodes, string? selector) => Find(nodes, selector).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode? FirstOrDefault(this IEnumerable<HtmlNode> nodes, Selector selector) => Find(nodes, selector).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selector">Pre-compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode? FirstOrDefault(this IEnumerable<HtmlNode> nodes, DefaultSelectors.DefaultSelector selector) => Find(nodes, selector).FirstOrDefault();

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selectors, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public static HtmlElementNode? FirstOrDefault(this IEnumerable<HtmlNode> nodes, SelectorCollection selectors) => Find(nodes, selectors).FirstOrDefault();

    /// <summary>
    /// Recursively finds the first node of the specified type, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <returns>The matching nodes.</returns>
    public static T? FirstOrDefaultOfType<T>(this IEnumerable<HtmlNode> nodes) where T : HtmlNode => FindOfType<T>(nodes).FirstOrDefault();

    /// <summary>
    /// Recursively finds the first node of the specified type for which the given predicate returns true, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="nodes">The nodes to be searched.</param>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    public static T? FirstOrDefaultOfType<T>(this IEnumerable<HtmlNode> nodes, Func<T, bool> predicate) where T : HtmlNode => FindOfType<T>(nodes, predicate).FirstOrDefault();
    #endregion FirstOrDefault

    /// <summary>
    /// Generates an HTML string from the contents of this node collection.
    /// </summary>
    /// <returns>A string with the markup for this node collection.</returns>
    public static string ToHtml(this IEnumerable<HtmlNode> nodes) => string.Concat(nodes.Select(n => n.OuterHtml));
}
