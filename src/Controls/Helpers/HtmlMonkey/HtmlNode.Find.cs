#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
using System.Collections.Generic;
public partial class HtmlElementNode {
    #region Find
    /// <summary>
    /// Recursively finds all nodes for which the given predicate returns true.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    /// <remarks>
    /// Implemented without recursion for better performance on deeply nested collections.
    /// </remarks>
    public IEnumerable<HtmlNode> Find(Func<HtmlNode, bool> predicate) => this.Children.Find(predicate);

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified selectors.
    /// </summary>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(string? selector) => this.Children.Find(selector);

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified compiled selectors.
    /// </summary>
    /// <param name="selector">Compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(Selector selector) => this.Children.Find(selector);

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified compiled selectors.
    /// </summary>
    /// <param name="selector">Pre-compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(DefaultSelectors.DefaultSelector selector) => this.Children.Find(selector);

    /// <summary>
    /// Recursively searches the given nodes for ones matching the specified compiled selectors.
    /// </summary>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<HtmlElementNode> Find(SelectorCollection selectors) => this.Children.Find(selectors);

    /// <summary>
    /// Recursively finds all nodes of the specified type.
    /// </summary>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<T> FindOfType<T>() where T : HtmlNode => this.Children.FindOfType<T>();

    /// <summary>
    /// Recursively finds all nodes of the specified type for which the given predicate returns true.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    public IEnumerable<T> FindOfType<T>(Func<T, bool> predicate) where T : HtmlNode => this.Children.FindOfType<T>(predicate);
    #endregion Find

    #region First
    /// <summary>
    /// Recursively finds the first node for which the given predicate returns true.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    /// <remarks>
    /// Implemented without recursion for better performance on deeply nested collections.
    /// </remarks>
    public HtmlNode First(Func<HtmlNode, bool> predicate) => this.Children.First(predicate);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified selectors.
    /// </summary>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(string? selector) => this.Children.First(selector);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector.
    /// </summary>
    /// <param name="selector">Compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(Selector selector) => this.Children.First(selector);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector.
    /// </summary>
    /// <param name="selector">Pre-compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(DefaultSelectors.DefaultSelector selector) => this.Children.First(selector);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selectors.
    /// </summary>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode First(SelectorCollection selectors) => this.Children.First(selectors);

    /// <summary>
    /// Recursively finds the first node of the specified type.
    /// </summary>
    /// <returns>The matching nodes.</returns>
    public T FirstOfType<T>() where T : HtmlNode => this.Children.FirstOfType<T>();

    /// <summary>
    /// Recursively finds the first node of the specified type for which the given predicate returns true.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    public T FirstOfType<T>(Func<T, bool> predicate) where T : HtmlNode => this.Children.FirstOfType<T>(predicate);
    #endregion First

    #region FirstOrDefault
    /// <summary>
    /// Recursively finds the first node for which the given predicate returns true, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    /// <remarks>
    /// Implemented without recursion for better performance on deeply nested collections.
    /// </remarks>
    public HtmlNode? FirstOrDefault(Func<HtmlNode, bool> predicate) => this.Children.FirstOrDefault(predicate);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified selectors, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="selector">Selector that describes the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(string? selector) => this.Children.FirstOrDefault(selector);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="selector">Compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(Selector selector) => this.Children.FirstOrDefault(selector);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selector, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="selector">Pre-compiled selector that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(DefaultSelectors.DefaultSelector selector) => this.Children.FirstOrDefault(selector);

    /// <summary>
    /// Recursively searches the given nodes for the first node matching the specified compiled selectors, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="selectors">Compiled selectors that describe the nodes to find.</param>
    /// <returns>The matching nodes.</returns>
    public HtmlElementNode? FirstOrDefault(SelectorCollection selectors) => this.Children.FirstOrDefault(selectors);

    /// <summary>
    /// Recursively finds the first node of the specified type, or <see langword="null"/> if none found.
    /// </summary>
    /// <returns>The matching nodes.</returns>
    public T? FirstOrDefaultOfType<T>() where T : HtmlNode => this.Children.FirstOrDefaultOfType<T>();

    /// <summary>
    /// Recursively finds the first node of the specified type for which the given predicate returns true, or <see langword="null"/> if none found.
    /// </summary>
    /// <param name="predicate">A function that determines if the item should be included in the results.</param>
    /// <returns>The matching nodes.</returns>
    public T? FirstOrDefaultOfType<T>(Func<T, bool> predicate) where T : HtmlNode => this.Children.FirstOrDefaultOfType<T>(predicate);
    #endregion FirstOrDefault
}
