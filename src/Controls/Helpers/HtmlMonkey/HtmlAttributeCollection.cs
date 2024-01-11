// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
/// <summary>
/// Class to represent a collection of element attributes. Implemented as an ordered
/// dictionary that can be used for fast look ups or iterating a set sequence.
/// </summary>
public class HtmlAttributeCollection : IEnumerable<HtmlAttribute> {
    // Internal attribute collections
    private readonly List<HtmlAttribute> Attributes;        // Ordered list of attributes
    private readonly Dictionary<string, int> IndexLookup;   // Attribute name to index lookup

    /// <summary>
    /// Constructs an <see cref="HtmlAttributeCollection"/> instance.
    /// </summary>
    public HtmlAttributeCollection() {
        Attributes = [];
        IndexLookup = new(HtmlRules.TagStringComparer);
    }

    /// <summary>
    /// Constructs an <see cref="HtmlAttributeCollection"/> instance.
    /// </summary>
    /// <param name="attributes">Attributes with which to prepopulate this
    /// collection.</param>
    public HtmlAttributeCollection(HtmlAttributeCollection attributes) {
        Attributes = new(attributes);
        IndexLookup = new(attributes.IndexLookup, HtmlRules.TagStringComparer);
    }

    /// <summary>
    /// Adds an <see cref="HtmlAttribute"/> to the collection. If the attribute already exists in the
    /// collection, the value of the existing attribute is updated.
    /// </summary>
    /// <param name="name">The name of the attribute to add.</param>
    /// <param name="value">The value of the attribute to add.</param>
    public void Add(string name, string? value) => Add(new HtmlAttribute(name, value));

    /// <summary>
    /// Adds an <see cref="HtmlAttribute"/> to the collection. If the attribute already exists, the
    /// value of the existing attribute is updated.
    /// </summary>
    /// <param name="attribute">The attribute to add.</param>
    public void Add(HtmlAttribute attribute) {
        if (attribute == null) {
            throw new ArgumentNullException(nameof(attribute));
        }
        if (string.IsNullOrWhiteSpace(attribute.Name)) {
            throw new ArgumentException("An attribute name is required.");
        }

        // Determine if we already have this attribute
        if (IndexLookup.TryGetValue(attribute.Name, out int existingIndex)) {
            Attributes[existingIndex] = attribute;
        }
        else {
            int index = Attributes.Count;
            Attributes.Add(attribute);
            IndexLookup.Add(attribute.Name, index);
        }
    }

    /// <summary>
    /// Adds a collection of <see cref="HtmlAttribute"/>s to this collection. If any of the
    /// attributes already exists, the value of the existing attribute is updated.
    /// </summary>
    /// <param name="attributes">The collection of attributes to be added.</param>
    public void AddRange(IEnumerable<HtmlAttribute> attributes) {
        foreach (HtmlAttribute attribute in attributes) {
            Add(attribute);
        }
    }

    /// <summary>
    /// Removes the attribute with the specified name from the collection.
    /// </summary>
    /// <param name="name">The name of the attribute to remove.</param>
    /// <returns>True if an attribute with the specified name is found and removed.
    /// False if no attribute was found with the given name.</returns>
    public bool Remove(string name) {
        if (IndexLookup.TryGetValue(name, out int index)) {
            RemoveIndexLookup(index);
            Attributes.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes the attribute at the specified position from the collection.
    /// </summary>
    /// <param name="index">The 0-based index position of the item to be removed.</param>
    public void RemoveAt(int index) {
        if (index >= 0 && index < Attributes.Count) {
            RemoveIndexLookup(index);
            Attributes.RemoveAt(index);
        }
    }

    /// <summary>
    /// Removes the item with the given index from the <see cref="IndexLookup"/> dictionary.
    /// </summary>
    private void RemoveIndexLookup(int index) {
        Debug.Assert(index >= 0 && index < IndexLookup.Count);
#if NETCOREAPP
        foreach (KeyValuePair<string, int> pair in IndexLookup) {
            if (pair.Value > index)
                IndexLookup[pair.Key]--;
            else if (pair.Value == index)
                IndexLookup.Remove(pair.Key);
        }
#else
        foreach (var pair in IndexLookup) {
            if (pair.Value == index) {
                IndexLookup.Remove(pair.Key);
                break;
            }
        }
        foreach (var pair in IndexLookup.ToArray()) {
            if (pair.Value > index) {
                IndexLookup[pair.Key]--;
            }
        }
#endif
    }

    /// <summary>
    /// Returns the <see cref="HtmlAttribute"/> with the given name. This property
    /// returns null rather than throwing an exception when the attribute does not
    /// exist.
    /// </summary>
    /// <param name="name">Attribute name.</param>
    /// <returns>Returns the <see cref="HtmlAttribute"/> with the specified name.</returns>
    public HtmlAttribute? this[string name] {
        get {
            // NOTE: Adding a setter here could allow setting an attribute
            // with a different key than the attribute name? It could also
            // allow setting a null attribute.
            return (name != null && IndexLookup.TryGetValue(name, out int index)) ? Attributes[index] : default;
        }
    }

    /// <summary>
    /// Returns the <see cref="HtmlAttribute"/> at the specified index.
    /// </summary>
    /// <param name="index">The 0-based index of the attribute to return.</param>
    /// <returns>Returns the <see cref="HtmlAttribute"/> at the specified index.</returns>
    // NOTE: Adding a setter here could allow setting an attribute
    // with a different key than the attribute name? It could also
    // allow setting a null attribute.
    public HtmlAttribute this[int index] => Attributes[index];

    /// <summary>
    /// Gets the <see cref="HtmlAttribute"/> with the given name. Returns <c>true</c>
    /// if successful, or <c>false</c> if no matching attribute was found.
    /// </summary>
    /// <param name="name">Attribute name.</param>
    /// <param name="value">Returns the attribute with the specified name, if successful.</param>
    /// <returns>True if successful, false if no matching attribute was found.</returns>
    public bool TryGetValue(string name, [NotNullWhen(true)] out HtmlAttribute? value) {
        if (IndexLookup.TryGetValue(name, out int index)) {
            value = Attributes[index];
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Converts this <see cref="HtmlAttributeCollection"></see> to a string.
    /// </summary>
    public override string ToString() => Attributes.Count > 0 ? $" {string.Join(" ", this)}" : string.Empty;

    /// <summary>
    /// Gets the number of <see cref="HtmlAttribute"/>s in this collection.
    /// </summary>
    public int Count => Attributes.Count;

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool Contains(string name) => IndexLookup.ContainsKey(name);

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool Contains(string name, StringComparison comparison) {
        for (int i = 0; i < Attributes.Count; i++) {
            if (Attributes[i].Name.Equals(name, comparison)) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name with a given value.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool Contains(string name, string value) {
        return IndexLookup.TryGetValue(name, out int index) && Attributes[index].Value == value;
    }

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name with a given value.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool Contains(string name, string value, StringComparison comparison) {
        return IndexLookup.TryGetValue(name, out int index) && Attributes[index].Value?.Equals(value, comparison) == true;
    }

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name and contains a non null, empty, or whitespace string.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool ContainsWithAnyValue(string name) {
        return IndexLookup.TryGetValue(name, out int index) && !string.IsNullOrWhiteSpace(Attributes[index].Value);
    }

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name and contains a non null, empty, or whitespace string.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool ContainsWithAnyValue(string name, StringComparison comparison) {
        for (int i = 0; i < Attributes.Count; i++) {
            if (Attributes[i].Name.Equals(name, comparison)) {
                if (!string.IsNullOrWhiteSpace(Attributes[i].Value)) {
                    return true;
                }
                break;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name and starts with <paramref name="value"/>.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool ContainsStartsWith(string name, string value) {
        return IndexLookup.TryGetValue(name, out int index) && Attributes[index].Value?.StartsWith(value) == true;
    }

    /// <summary>
    /// Returns true if this collection contains an attribute with the given name and starts with <paramref name="value"/>.
    /// </summary>
    /// <param name="name">The name of the attribute to compare.</param>
    public bool ContainsStartsWith(string name, string value, StringComparison comparison) {
        return IndexLookup.TryGetValue(name, out int index) && Attributes[index].Value?.StartsWith(value, comparison) == true;
    }

    /// <summary>
    /// Gets an enumerable on the attribute names.
    /// </summary>
    public IEnumerable<string> Names => Attributes.Select(a => a.Name);

    /// <summary>
    /// Gets an enumerable on the attribute values.
    /// </summary>
    public IEnumerable<string?> Values => Attributes.Select(a => a.Value);

    #region IEnumerable
    /// <summary>
    /// Gets an enumerator that iterates through the <see cref="HtmlAttribute"/>
    /// collection.
    /// </summary>
    public IEnumerator<HtmlAttribute> GetEnumerator() {
        return Attributes.GetEnumerator();
    }

    /// <summary>
    /// Gets an enumerator that iterates through the <see cref="HtmlAttribute"/>
    /// collection.
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return Attributes.GetEnumerator();
    }
    #endregion
}
