// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Threading.Tasks;
public partial class SelectorCollection {
    /// <summary>
    /// Parses the given selector text and inserts them into the collection asynchronously.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Inserts multiple <see cref="Selector"/>s when the selector contains commas.
    /// </remarks>
    public Task InsertSelectorsAsync(string? selectorText) {
        return Task.Run(() => SelectorParsing.ParseSelectors(selectorText, this));
    }
}
