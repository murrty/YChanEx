// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
/// <summary>
/// Class that represents a single element attribute.
/// </summary>
public class HtmlAttribute {
    /// <summary>
    /// Gets or sets the name of this attribute.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of this attribute. The library sets this value to
    /// an empty string if the attribute has an empty value. It sets it to null
    /// if the attribute name was not followed by an equal sign.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Constructs an <see cref="HtmlAttribute"/> instance.
    /// </summary>
    public HtmlAttribute() {
        Name = string.Empty;
        Value = null;
    }

    /// <summary>
    /// Constructs an <see cref="HtmlAttribute"/> instance.
    /// </summary>
    /// <param name="name">Name of this attribute.</param>
    /// <param name="value">Value of this attribute. The library sets this value
    /// to an empty string if the attribute has an empty value. It sets it to
    /// null if the attribute name was not followed by an equal sign.</param>
    public HtmlAttribute(string name, string? value = null) {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Constructs an <see cref="HtmlAttribute"/> instance.
    /// </summary>
    /// <param name="attribute"><see cref="HtmlAttribute"/> that contains the initial value
    /// for this attribute.</param>
    public HtmlAttribute(HtmlAttribute attribute) {
        Name = attribute.Name;
        Value = attribute.Value;
    }

    /// <summary>
    /// Whether this attribute contains a value.
    /// </summary>
    /// <param name="value">The value to check within the attribute values.</param>
    /// <returns><see langword="true"/> if the attribute contains a value; otherwise, <see langword="false"/>.</returns>
    public bool ContainsValue(string value) {
        if (value?.Length > 0) {
            string[] values = value.Split(' ');
            for (int i = 0; i < values.Length; i++) {
                if (values[i].Equals(value)) {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Whether this attribute contains a value.
    /// </summary>
    /// <param name="value">The value to check within the attribute values.</param>
    /// <param name="comparison">How the strings will be compared.</param>
    /// <returns><see langword="true"/> if the attribute contains a value; otherwise, <see langword="false"/>.</returns>
    public bool ContainsValue(string value, StringComparison comparison) {
        if (value?.Length > 0) {
            string[] values = value.Split(' ');
            for (int i = 0; i < values.Length; i++) {
                if (values[i].Equals(value, comparison)) {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Converts this <see cref="HtmlAttribute"></see> to a string.
    /// </summary>
    public override string ToString() {
        string name = Name ?? "(null)";
        return (Value != null) ? $"{name ?? "(null)"}=\"{Value}\"" : name;
    }
}
