#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
[DataContract]
internal sealed class EightChanBoard {
    [DataMember(Name = "id")]
    public string? BoardId { get; set; }

    [DataMember(Name = "name")]
    public string? BoardName { get; set; }

    [DataMember(Name = "description")]
    public string? BoardDescription { get; set; }

    public static EightChanBoard? ExtractBoard(string html) {
        var htDoc = HtmlDocument.FromHtml(html);
        var HeaderNode = htDoc.FirstOrDefault(EightChanBoardSelectors.BoardHeaderSelector);
        if (HeaderNode == null) {
            return null;
        }

        var NameNode = HeaderNode.FirstOrDefault(EightChanBoardSelectors.BoardNameSelector);
        if (NameNode == null) {
            return null;
        }

        string Name = NameNode.Text[(NameNode.Text.IndexOf("/ - ") + 4)..];
        string Description = string.Empty;

        var DescriptionNode = HeaderNode.FirstOrDefault(EightChanBoardSelectors.BoardDescriptionSelector);
        if (DescriptionNode != null) {
            Description = DescriptionNode.Text;
        }

        return new EightChanBoard() {
            BoardName = Name,
            BoardDescription = Description,
        };
    }
}

internal static class EightChanBoardSelectors {
    internal static readonly Selector BoardHeaderSelector = Selector.ParseSelector("> body header[class=boardHeader]");
    internal static readonly Selector BoardNameSelector = Selector.ParseSelector("div > p[id=labelName]");
    internal static readonly Selector BoardDescriptionSelector = Selector.ParseSelector("p[id=labelDescription]");
}
