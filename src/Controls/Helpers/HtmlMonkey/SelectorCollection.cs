// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Collections.Generic;
using System.Diagnostics;
/// <summary>
/// Holds a list of selectors that can all be applied to a node search.
/// </summary>
public partial class SelectorCollection : List<Selector> {
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorCollection"/> class.
    /// </summary>
    public SelectorCollection() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorCollection"/> class.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Inserts multiple <see cref="Selector"/>s when the selector contains commas.
    /// </remarks>
    public SelectorCollection(string? selectorText) : this() {
        SelectorParsing.ParseSelectors(selectorText, this);
    }

    /// <summary>
    /// Parses the given selector text and inserts them into the collection.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Inserts multiple <see cref="Selector"/>s when the selector contains commas.
    /// </remarks>
    public void InsertSelectors(string? selectorText) {
        SelectorParsing.ParseSelectors(selectorText, this);
    }

    #region Internal methods
    /// <summary>
    /// Appends a new child selector to the last selector in the collection.
    /// If the collection is empty, a child selector is added to a new parent
    /// selector.
    /// </summary>
    /// <returns>Returns the new child selector.</returns>
    internal Selector AddChildSelector() {
        Selector? selector = GetLastSelector();
        Debug.Assert(selector is not null);
        Debug.Assert(selector!.ChildSelector == null);
        selector.ChildSelector = new Selector();
        return selector.ChildSelector;
    }

    /// <summary>
    /// Returns the last selector or, if the last selector has child selectors, the last
    /// child selector of the last selector. If the collection is empty, a new selector is
    /// added and returned.
    /// <returns>The last child selector of the last selector in the collection.</returns>
    internal Selector GetLastSelector() {
        Selector selector;

        if (Count > 0) {
            // Get last selector
            selector = this[Count - 1];
            // Get last child selector (return selector if no children)
            while (selector.ChildSelector != null) {
                selector = selector.ChildSelector;
            }
            return selector;
        }

        selector = new Selector();
        Add(selector);
        return selector;
    }

    /// <summary>
    /// Removes all selectors and child selectors that do not contain selection data.
    /// </summary>
    internal void RemoveEmptySelectors() {
        for (int i = Count - 1; i >= 0; i--) {
            Selector? selector = RemoveEmptyChildSelectors(this[i]);
            if (selector == null) {
                RemoveAt(i);
            }
            else if (this[i] != selector) {
                this[i] = selector;
            }
        }
    }

    /// <summary>
    /// Removes any empty child selectors of the given selector. If the parent
    /// selector is empty, that is also removed and the new parent is returned.
    /// </summary>
    /// <param name="selector">Selector from which to remove child selectors.</param>
    /// <returns>The new parent selector, which may be the same as
    /// <paramref name="selector"/>.</returns>
    private static Selector? RemoveEmptyChildSelectors(Selector? selector) {
        Selector? parent = selector;
        Selector? prev = null;

        while (selector != null) {
            if (selector.IsEmpty) {
                if (prev == null) {
                    parent = selector.ChildSelector;
                }
                else {
                    prev.ChildSelector = selector.ChildSelector;
                }
            }
            else {
                prev = selector;
            }
            selector = selector.ChildSelector;
        }
        return parent;
    }
    #endregion
}
