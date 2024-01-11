// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Collections.Generic;
using System.Diagnostics;
/// <summary>
/// Describes a selector used for finding elements.
/// </summary>
public partial class Selector {
    /// <summary>
    /// Gets or sets the tag name. Set to <c>null</c> or empty string to
    /// match all tags.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets this selector's attribute selectors.
    /// </summary>
    public List<AttributeSelector> Attributes { get; }

    /// <summary>
    /// Gets or sets this selector's child selector.
    /// </summary>
    public Selector? ChildSelector { get; set; }

    /// <summary>
    /// Gets the selector's parent.
    /// </summary>
    public Selector? ParentSelector { get; }

    /// <summary>
    /// Gets or sets whether this selector applies only to immediate children
    /// (one level down from parent).
    /// </summary>
    public bool ImmediateChildOnly { get; set; }

    /// <summary>
    /// Constructs a new <see cref="Selector"></see> instance.
    /// </summary>
    public Selector() {
        Tag = null;
        Attributes = [];
        ChildSelector = null;
    }
    internal Selector(Selector parent) : this() {
        this.ParentSelector = parent;
    }

    /// <summary>
    /// Returns true if selector has no data. Child selectors are not included in this
    /// evaluation.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Tag) && Attributes.Count < 1;

    /// <summary>
    /// Returns true if this selector matches the specified <see cref="HtmlElementNode"/>.
    /// </summary>
    public bool IsMatch(HtmlElementNode node) {
        // Compare tag
        if (!string.IsNullOrWhiteSpace(Tag) && !string.Equals(Tag, node.TagName, HtmlRules.TagStringComparison)) {
            return false;
        }

        // Compare attributes
        //bool matchFound = false;
        foreach (AttributeSelector selector in Attributes) {
            if (!selector.IsMatch(node)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Resursive portion of Find().
    /// </summary>
    private static void FindRecursive(IEnumerable<HtmlNode> nodes, Selector selector,
        bool matchTopLevelNodes, bool recurse, List<HtmlElementNode> results) {
        Debug.Assert(matchTopLevelNodes || recurse);

        foreach (var node in nodes) {
            if (node is HtmlElementNode elementNode) {
                if (matchTopLevelNodes && selector.IsMatch(elementNode)) {
                    results.Add(elementNode);
                }

                if (recurse) {
                    FindRecursive(elementNode.Children, selector, true, !selector.ImmediateChildOnly, results);
                }
            }
        }
    }

    public override string ToString() => Tag ?? "(null)";

    #region Parsing
    /// <summary>
    /// Parses the given selector text and returns the corresponding data structure.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Returns the first <see cref="Selector"/> when the selector contains commas.
    /// </remarks>
    /// <returns>The parsed selector data structure.</returns>
    public static Selector ParseSelector(string? selectorText) {
        return SelectorParsing.ParseSelector(selectorText);
    }

    /// <summary>
    /// Parses the given selector text and returns the corresponding data structure.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <param name="immediateChild">Whether the selector starts immediately as a child to the parent node.</param>
    /// <remarks>
    /// Returns the first <see cref="Selector"/> when the selector contains commas.
    /// </remarks>
    /// <returns>The parsed selector data structure.</returns>
    public static Selector ParseSelector(string? selectorText, bool immediateChild) {
        var Selector = SelectorParsing.ParseSelector(selectorText);
        Selector.ImmediateChildOnly = immediateChild;
        return Selector;
    }

    /// <summary>
    /// Parses the given selector text and returns the corresponding data structures.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Returns multiple <see cref="Selector"/>s when the selector contains commas.
    /// </remarks>
    /// <returns>The parsed selector data structures.</returns>
    public static SelectorCollection ParseSelectors(string? selectorText) {
        SelectorCollection selectors = [];
        SelectorParsing.ParseSelectors(selectorText, selectors);
        return selectors;
    }
    #endregion
}
