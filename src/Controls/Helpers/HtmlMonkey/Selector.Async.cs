#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Threading.Tasks;
public partial class Selector {
    /// <summary>
    /// Parses the given selector text asynchronously and returns the corresponding data structure.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Returns the first <see cref="Selector"/> when the selector contains commas.
    /// </remarks>
    /// <returns>The parsed selector data structure.</returns>
    public static Task<Selector> ParseSelectorAsync(string? selectorText) {
        return Task.Run(() => SelectorParsing.ParseSelector(selectorText));
    }

    /// <summary>
    /// Parses the given selector text asynchronously and returns the corresponding data structures.
    /// </summary>
    /// <param name="selectorText">The selector text to be parsed.</param>
    /// <remarks>
    /// Returns multiple <see cref="Selector"/>s when the selector contains commas.
    /// </remarks>
    /// <returns>The task representing the work parsing the selector.</returns>
    public static Task<SelectorCollection> ParseSelectorsAsync(string? selectorText) {
        return Task.Run(() => ParseSelectors(selectorText));
    }
}
