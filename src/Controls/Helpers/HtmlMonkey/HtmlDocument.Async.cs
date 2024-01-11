// Copyright (c) 2019-2022 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.IO;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// Holds the nodes of a parsed HTML or XML document. Use the <see cref="RootNodes"/>
/// property to access these nodes. Use the <see cref="ToHtml"/> method to convert the
/// nodes back to markup.
/// </summary>
public partial class HtmlDocument {
    /// <summary>
    /// Asynchronously parses an HTML or XML string and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="html">The HTML or XML string to parse.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static async Task<HtmlDocument> FromHtmlAsync(string? html, HtmlParseOptions options = HtmlParseOptions.None) {
        return await Task.Run(() => FromHtml(html, options))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously parses an HTML or XML file and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="path">The HTML or XML file to parse.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static async Task<HtmlDocument> FromFileAsync(string path, HtmlParseOptions options = HtmlParseOptions.None) {
#if NETCOREAPP
        return await FromHtmlAsync(await File.ReadAllTextAsync(path).ConfigureAwait(false), options)
            .ConfigureAwait(false);
#else
        return await FromHtmlAsync(await Task.Run(() => File.ReadAllText(path)).ConfigureAwait(false), options)
            .ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Asynchronously parses an HTML or XML file and returns an <see cref="HtmlDocument"></see> instance that
    /// contains the parsed nodes.
    /// </summary>
    /// <param name="path">The HTML or XML file to parse.</param>
    /// <param name="encoding">The encoding applied to the contents of the file.</param>
    /// <returns>Returns an <see cref="HtmlDocument"></see> instance that contains the parsed
    /// nodes.</returns>
    public static async Task<HtmlDocument> FromFileAsync(string path, Encoding encoding, HtmlParseOptions options = HtmlParseOptions.None) {
#if NETCOREAPP
        return await FromHtmlAsync(await File.ReadAllTextAsync(path, encoding).ConfigureAwait(false), options)
            .ConfigureAwait(false);
#else
        return await FromHtmlAsync(await Task.Run(() => File.ReadAllText(path, encoding)).ConfigureAwait(false), options)
            .ConfigureAwait(false);
#endif
    }
}
