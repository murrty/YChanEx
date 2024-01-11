// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
/// <summary>
///     Specifies the type of comparison to use on an attribute selector.
/// </summary>
public enum AttributeSelectorMode : byte {
    /// <summary>
    ///     Matches if any word in the attribute value matches a string.
    ///     When using CSS selectors, this is the mode used.
    /// <para/>
    ///     <c>#</c> - Searches a nodes ID.<para/>
    ///     <c>.</c> - Searches a nodes class<para/>
    ///     <c>:</c> - Searches a type.
    /// </summary>
    Contains = 0x0,
    /// <summary>
    ///     Matches if any value(s) in the attribute matches any of the values in the selector.
    /// <para/>
    ///     Uses the selector operator '<c>?=</c>', <c>div[class?="align-right"]</c>
    /// </summary>
    ContainsAny = 0x1,
    /// <summary>
    ///     Matches by comparing the attribute value to a string.
    ///     This is absolute, so the selector must be equal-to the full value of the attribute.
    /// <para/>
    ///     Uses the selector operator '<c>==</c>', <c>div[class=="align-right hidden"]</c>
    /// </summary>
    Match = 0x2,
    /// <summary>
    ///     Matches by comparing the attribute value to a regular expression.
    /// <para/>
    ///     Uses the selector operator '<c>:=</c>', <c>div[class:="reply-<see langword="\d+"/>"]</c>.
    /// </summary>
    RegEx = 0x3,
    /// <summary>
    ///     Matches if the attribute exists, regardless of the attribute value.
    /// <para/>
    ///     The selector operator must only contain the name of the attribute, <c>div[class]</c>
    /// </summary>
    ExistsOnly = 0x4,
    /// <summary>
    ///     Matches if the attribute exists and has a value that is not null, empty, or whitespace.
    /// <para/>
    ///     The selector must only include the operator '<c>=</c>' with no value, <c>div[class=]</c>
    /// </summary>
    ExistsWithValue = 0x5,
}
