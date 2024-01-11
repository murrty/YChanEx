// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
/// <summary>
/// Defines a selector that describes a node attribute.
/// </summary>
public class AttributeSelector {
    private readonly StringComparison StringComparison;
    private readonly RegexOptions RegexOptions;
    private Regex? valueRegex;
    private string? _value;

    /// <summary>
    /// Gets or sets the name of the attribute to be compared.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value the attribute should be compared to.
    /// </summary>
    public string? Value {
        get => _value;
        set {
            _value = value;
            if (_mode == AttributeSelectorMode.RegEx) {
                valueRegex = !string.IsNullOrWhiteSpace(value) ? new(value, RegexOptions) : null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the values the attribute should be compared to.
    /// </summary>
    public string[]? Values { get; set; }

    /// <summary>
    /// Gets or sets the type of comparison that should be performed
    /// on the attribute value.
    /// </summary>
    public AttributeSelectorMode Mode {
        get => _mode;
        [MemberNotNull(nameof(IsMatch))]
        set {
            this._mode = value;
            switch (value) {
                case AttributeSelectorMode.ContainsAny: {
                    this.IsMatch = ContainsAnyComparer;
                } break;
                case AttributeSelectorMode.Match: {
                    this.IsMatch = MatchComparer;
                } break;
                case AttributeSelectorMode.RegEx: {
                    if (!string.IsNullOrWhiteSpace(Value)) {
                        this.valueRegex = new(Value, RegexOptions);
                    }
                    this.IsMatch = RegExComparer;
                } break;
                case AttributeSelectorMode.ExistsOnly: {
                    this.IsMatch = ExistsOnlyComparer;
                } break;
                case AttributeSelectorMode.ExistsWithValue: {
                    this.IsMatch = ExistsHasAnyValue;
                } break;
                default: {
                    this.IsMatch = ContainsComparer;
                } break;
            }
        }
    }
    private AttributeSelectorMode _mode;

    /// <summary>
    /// Compares the given node against this selector attribute.
    /// </summary>
    /// <returns>True if the node matches, false otherwise.</returns>
    public Func<HtmlElementNode, bool> IsMatch { get; private set; }

    /// <summary>
    /// Constructs a <see cref="AttributeSelector"></see> instance.
    /// </summary>
    /// <param name="ignoreCase">If <c>true</c>, node comparisons are not case-sensitive. If <c>false</c>,
    /// node comparisons are case-sensitive.</param>
    public AttributeSelector(string name, string? value = null, bool ignoreCase = true) {
        this.Name = name;
        this.Value = value;
        this.Mode = AttributeSelectorMode.Match;
        this.StringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        this.RegexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
    }

    private AttributeSelector(AttributeSelector sel) {
        this.StringComparison = sel.StringComparison;
        this.RegexOptions = sel.RegexOptions;
        this.Name = sel.Name;
        this.Value = sel.Value;
        if (sel.Values != null) {
            this.Values = new string[sel.Values.Length];
            for (int i = 0; i < sel.Values.Length; i++) {
                this.Values[i] = sel.Values[i];
            }
        }
        this.Mode = sel.Mode;
    }

    #region Matching routines
    private bool MatchComparer(HtmlElementNode node) {
        if (Value != null) {
            if (node.Attributes.TryGetValue(Name, out HtmlAttribute? attribute) && attribute.Value != null) {
                return string.Equals(attribute.Value, Value, StringComparison);
            }
        }
        return false;
    }

    private bool RegExComparer(HtmlElementNode node) {
        if (valueRegex != null) {
            if (node.Attributes.TryGetValue(Name, out HtmlAttribute? attribute) && attribute.Value != null) {
                return valueRegex.IsMatch(attribute.Value);
                //return Regex.IsMatch(attribute.Value, Value, RegexOptions);
            }
        }
        return false;
    }

    private bool ContainsComparer(HtmlElementNode node) {
        if (Value != null) {
            if (node.Attributes.TryGetValue(Name, out HtmlAttribute? attribute) && attribute.Value != null) {
                return ParseWords(attribute.Value).Any(a => string.Equals(Value, a, StringComparison));
            }
        }
        return false;
    }

    private bool ContainsAnyComparer(HtmlElementNode node) {
        if (Values != null) {
            if (node.Attributes.TryGetValue(Name, out HtmlAttribute? attribute) && attribute.Value != null) {
                var words = ParseWords(attribute.Value).ToArray();
                for (int i = 0; i < Values.Length; i++) {
                    for (int x = 0; x < words.Length; x++) {
                        if (string.Equals(Values[i], words[x], StringComparison)) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool ExistsOnlyComparer(HtmlElementNode node) => node.Attributes.Contains(this.Name, this.StringComparison);

    private bool ExistsHasAnyValue(HtmlElementNode node) => node.Attributes.ContainsWithAnyValue(this.Name, this.StringComparison);

    private static IEnumerable<string> ParseWords(string s) {
        bool inWord = false;
        int wordStart = 0;

        for (int i = 0; i < s.Length; i++) {
            if (char.IsWhiteSpace(s[i])) {
                if (inWord) {
                    inWord = false;
                    yield return s[wordStart..i];
                }
            }
            else if (!inWord) {
                inWord = true;
                wordStart = i;
            }
        }

        // Check for last word
        if (inWord) {
            yield return s[wordStart..];
        }
    }
    #endregion

    public AttributeSelector Clone() {
        return new AttributeSelector(this);
    }

    public override string ToString() => $"{Name ?? "(null)"}={Value ?? "(null)"}";
}
