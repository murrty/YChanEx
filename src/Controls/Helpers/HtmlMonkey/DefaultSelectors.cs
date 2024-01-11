#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// A collection of default selectors that are pre-compiled.
/// </summary>
public static class DefaultSelectors {
    #region a
    /// <summary>
    /// Selector for any 'a' nodes.
    /// </summary>
    public static DefaultSelector a => _a ??= new DefaultSelector(Selector.ParseSelector("a"));
    private static DefaultSelector? _a;

    /// <summary>
    /// A collection of selectors for 'a' nodes.
    /// </summary>
    public static class A {
        /// <summary>
        /// Selector for any 'a' nodes with an 'href' attribute.
        /// </summary>
        public static DefaultSelector Href => _href ??= new DefaultSelector(Selector.ParseSelector("a[href]"));
        private static DefaultSelector? _href;

        /// <summary>
        /// Selector for any 'a' nodes with an 'href' attribute with any value.
        /// </summary>
        public static DefaultSelector HrefValue => _hrefValue ??= new DefaultSelector(Selector.ParseSelector("a[href=]"));
        private static DefaultSelector? _hrefValue;
    }
    #endregion a

    #region div
    /// <summary>
    /// Selector for any 'div' nodes.
    /// </summary>
    public static DefaultSelector div => _div ??= new DefaultSelector(Selector.ParseSelector("div"));
    private static DefaultSelector? _div;

    /// <summary>
    /// A collection of selectors for 'div' nodes.
    /// </summary>
    public static class Div {
    }
    #endregion div

    #region em
    /// <summary>
    /// Selector for any 'em' nodes.
    /// </summary>
    public static DefaultSelector em => _em ??= new DefaultSelector(Selector.ParseSelector("em"));
    private static DefaultSelector? _em;

    /// <summary>
    /// A collection of selectors for 'em' nodes.
    /// </summary>
    public static class Em {
    }
    #endregion em

    #region h1
    /// <summary>
    /// Selector for any 'h1' nodes.
    /// </summary>
    public static DefaultSelector h1 => _h1 ??= new DefaultSelector(Selector.ParseSelector("h1"));
    private static DefaultSelector? _h1;

    /// <summary>
    /// A collection of selectors for 'h1' nodes.
    /// </summary>
    public static class H1 {
    }
    #endregion h1

    #region img
    /// <summary>
    /// Selector for any 'img' nodes.
    /// </summary>
    public static DefaultSelector img => _img ??= new DefaultSelector(Selector.ParseSelector("img"));
    private static DefaultSelector? _img;

    /// <summary>
    /// A collection of selectors for 'img' nodes.
    /// </summary>
    public static class Img {
        /// <summary>
        /// Selector for any 'img' nodes with a 'src' attribute.
        /// </summary>
        public static DefaultSelector src => _src ??= new DefaultSelector(Selector.ParseSelector("img[src]"));
        private static DefaultSelector? _src;
    }
    #endregion img

    #region input
    /// <summary>
    /// Selector for any 'input' nodes.
    /// </summary>
    public static DefaultSelector input => _input ??= new DefaultSelector(Selector.ParseSelector("input"));
    private static DefaultSelector? _input;

    /// <summary>
    /// A collection of selectors for 'input' nodes.
    /// </summary>
    public static class Input {
    }
    #endregion input

    #region li
    /// <summary>
    /// Selector for any 'li' nodes.
    /// </summary>
    public static DefaultSelector li => _li ??= new DefaultSelector(Selector.ParseSelector("li"));
    private static DefaultSelector? _li;

    /// <summary>
    /// A collection of selectors for 'li' nodes.
    /// </summary>
    public static class Li {
    }
    #endregion li

    #region lu
    /// <summary>
    /// Selector for any 'lu' nodes.
    /// </summary>
    public static DefaultSelector lu => _lu ??= new DefaultSelector(Selector.ParseSelector("lu"));
    private static DefaultSelector? _lu;

    /// <summary>
    /// A collection of selectors for 'lu' nodes.
    /// </summary>
    public static class Lu {
    }
    #endregion lu

    #region script
    /// <summary>
    /// Selector for any 'script' nodes.
    /// </summary>
    public static DefaultSelector script => _script ??= new DefaultSelector(Selector.ParseSelector("script"));
    private static DefaultSelector? _script;

    /// <summary>
    /// A collection of selectors for 'script' nodes.
    /// </summary>
    public static class Script {
    }
    #endregion script

    #region span
    /// <summary>
    /// Selector for any 'span' nodes.
    /// </summary>
    public static DefaultSelector span => _span ??= new DefaultSelector(Selector.ParseSelector("span"));
    private static DefaultSelector? _span;

    /// <summary>
    /// A collection of selectors for 'span' nodes.
    /// </summary>
    public static class Span {
    }
    #endregion span

    #region table
    /// <summary>
    /// Selector for any 'table' nodes.
    /// </summary>
    public static DefaultSelector table => _table ??= new DefaultSelector(Selector.ParseSelector("table"));
    private static DefaultSelector? _table;

    /// <summary>
    /// A collection of selectors for 'table' nodes.
    /// </summary>
    public static class Table {
    }
    #endregion table

    #region td
    /// <summary>
    /// Selector for any 'td' nodes.
    /// </summary>
    public static DefaultSelector td => _td ??= new DefaultSelector(Selector.ParseSelector("td"));
    private static DefaultSelector? _td;

    /// <summary>
    /// A collection of selectors for 'td' nodes.
    /// </summary>
    public static class Td {
    }
    #endregion td

    #region tr
    /// <summary>
    /// Selector for any 'tr' nodes.
    /// </summary>
    public static DefaultSelector tr => _tr ??= new DefaultSelector(Selector.ParseSelector("tr"));
    private static DefaultSelector? _tr;

    /// <summary>
    /// A collection of selectors for 'tr' nodes.
    /// </summary>
    public static class Tr {
    }
    #endregion tr

    #region u
    /// <summary>
    /// Selector for any 'u' nodes.
    /// </summary>
    public static DefaultSelector u => _u ??= new DefaultSelector(Selector.ParseSelector("u"));
    private static DefaultSelector? _u;

    /// <summary>
    /// A collection of selectors for 'u' nodes.
    /// </summary>
    public static class U {
    }
    #endregion u

    #region ul
    /// <summary>
    /// Selector for any 'ul' nodes.
    /// </summary>
    public static DefaultSelector ul => _ul ??= new DefaultSelector(Selector.ParseSelector("ul"));
    private static DefaultSelector? _ul;

    /// <summary>
    /// A collection of selectors for 'ul' nodes.
    /// </summary>
    public static class Ul {
    }
    #endregion ul

    #region video
    /// <summary>
    /// Selector for any 'video' nodes.
    /// </summary>
    public static DefaultSelector video => _video ??= new DefaultSelector(Selector.ParseSelector("video"));
    private static DefaultSelector? _video;

    /// <summary>
    /// A collection of selectors for 'video' nodes.
    /// </summary>
    public static class Video {
        /// <summary>
        /// Selector for any 'video' nodes with a 'src' attribute.
        /// </summary>
        public static DefaultSelector src => _src ??= new DefaultSelector(Selector.ParseSelector("video[src]"));
        private static DefaultSelector? _src;

        /// <summary>
        /// Selector for any 'video' nodes with both 'src' and 'loop' attributes.
        /// </summary>
        public static DefaultSelector srcLoop => _srcLoop ??= new DefaultSelector(Selector.ParseSelector("video[src][loop]"));
        private static DefaultSelector? _srcLoop;
    }
    #endregion video

    /// <summary>
    /// Represents a default selector. These cannot be created externally, as they're only used for defaults.
    /// </summary>
    public sealed class DefaultSelector {
        internal Selector _selector;
        internal DefaultSelector(Selector selector) => _selector = selector;

        /// <summary>
        /// Recursively searches the given root node and returns the nodes that match this
        /// selector.
        /// </summary>
        /// <param name="htDoc">Html document to search.</param>
        /// <returns>The matching nodes.</returns>
        public IEnumerable<HtmlElementNode> Find(HtmlDocument htDoc) => _selector.Find(htDoc);

        /// <summary>
        /// Recursively searches the given root node and returns the nodes that match this
        /// selector.
        /// </summary>
        /// <param name="rootNode">Root node to search.</param>
        /// <returns>The matching nodes.</returns>
        public IEnumerable<HtmlElementNode> Find(HtmlNode rootNode) => _selector.Find(rootNode);

        /// <summary>
        /// Recursively searches the list of nodes and returns the nodes that match this
        /// selector.
        /// </summary>
        /// <param name="nodes">Nodes to search.</param>
        /// <returns>The matching nodes.</returns>
        public IEnumerable<HtmlElementNode> Find(IEnumerable<HtmlNode> nodes) => _selector.Find(nodes);

        /// <summary>
        /// Recursively searches the given root node and returns the first node that match this
        /// selector.
        /// </summary>
        /// <param name="htDoc">Html document to search.</param>
        /// <returns>The matching nodes.</returns>
        public HtmlElementNode FindFirst(HtmlDocument htDoc) => _selector.Find(htDoc).First();

        /// <summary>
        /// Recursively searches the given root node and returns the first node that match this
        /// selector.
        /// </summary>
        /// <param name="rootNode">Root node to search.</param>
        /// <returns>The matching nodes.</returns>
        public HtmlElementNode FindFirst(HtmlNode rootNode) => _selector.Find(rootNode).First();

        /// <summary>
        /// Recursively searches the list of nodes and returns the first node that match this
        /// selector.
        /// </summary>
        /// <param name="nodes">Nodes to search.</param>
        /// <returns>The matching nodes.</returns>
        public HtmlElementNode FindFirst(IEnumerable<HtmlNode> nodes) => _selector.Find(nodes).First();

        /// <summary>
        /// Recursively searches the given root node and returns the first node that match this
        /// selector, or <see langword="null"/> if none found.
        /// </summary>
        /// <param name="htDoc">Html document to search.</param>
        /// <returns>The matching nodes.</returns>
        public HtmlElementNode? FindFirstOrDefault(HtmlDocument htDoc) => _selector.Find(htDoc).FirstOrDefault();

        /// <summary>
        /// Recursively searches the given root node and returns the first node that match this
        /// selector, or <see langword="null"/> if none found.
        /// </summary>
        /// <param name="rootNode">Root node to search.</param>
        /// <returns>The matching nodes.</returns>
        public HtmlElementNode? FindFirstOrDefault(HtmlNode rootNode) => _selector.Find(rootNode).FirstOrDefault();

        /// <summary>
        /// Recursively searches the list of nodes and returns the first node that match this
        /// selector, or <see langword="null"/> if none found.
        /// </summary>
        /// <param name="nodes">Nodes to search.</param>
        /// <returns>The matching nodes.</returns>
        public HtmlElementNode? FindFirstOrDefault(IEnumerable<HtmlNode> nodes) => _selector.Find(nodes).FirstOrDefault();
    }
}
