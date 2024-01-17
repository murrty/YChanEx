#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.FChan;
using static YChanEx.Parsers.Helpers.ParsersShared;
[DataContract]
[DebuggerDisplay("Ext = {Extension}")]
internal sealed class FChanFile {
    private const string UrlPrefix = "http://fchan.us";
    private static readonly Selector ThumbnailSelector = Selector.ParseSelector("a > img[class=thumb][width=][height=][src=]");
    private static readonly Selector SourceSelector = Selector.ParseSelector("> small");

    [IgnoreDataMember]
    public FChanPost Parent { get; }

    [DataMember(Name = "file_name")]
    public string? FileName { get; set; }

    [DataMember(Name = "extension")]
    public string? Extension { get; set; }

    [DataMember(Name = "url")]
    public string? Url { get; set; }

    [DataMember(Name = "est_size")]
    public long EstimatedSize { get; set; }

    [DataMember(Name = "width")]
    public int Width { get; set; }

    [DataMember(Name = "height")]
    public int Height { get; set; }

    [DataMember(Name = "thumb_url")]
    public string? ThumbnailUrl { get; set; }

    [DataMember(Name = "thumb_width")]
    public int ThumbnailWidth { get; set; }

    [DataMember(Name = "thumb_height")]
    public int ThumbnailHeight { get; set; }

    [DataMember(Name = "source")]
    public string? Source { get; set; }

    public FChanFile(HtmlElementNode MetadataNode, HtmlElementNode ParentNode, FChanPost Parent) {
        this.Parent = Parent;

        var FileLink = MetadataNode.Children.FirstOrDefault(DefaultSelectors.A.HrefValue) ??
            throw new ArgumentNullException("Could not find link metadata.");
        this.Url = UrlPrefix + FileLink.Attributes["href"]!.Value!;
        this.Extension = GetExtension(FileLink.Text, out int extIndex);
        this.FileName = FileLink.Text[..extIndex];

        var ExtraMetadata = MetadataNode.Children.FirstOrDefault(DefaultSelectors.em) ??
            throw new ArgumentNullException("Could not find extra metadata.");

        string[] Metadata = ExtraMetadata.Text.Split(',');
        this.EstimatedSize = ConvertSizeToBytes(Metadata[0]);
        var Dimensions = ConvertDimensionsToSize(Metadata[1]);
        this.Width = Dimensions.Width;
        this.Height = Dimensions.Height;

        var ThumbnailNode = ParentNode.Children.FirstOrDefault(ThumbnailSelector) ??
            throw new ArgumentNullException("Could not find thumbnail node.");
        this.ThumbnailUrl = UrlPrefix + ThumbnailNode.Attributes["src"]!.Value!;
        this.ThumbnailWidth = int.Parse(ThumbnailNode.Attributes["width"]!.Value!);
        this.ThumbnailHeight = int.Parse(ThumbnailNode.Attributes["height"]!.Value!);

        var SourceNode = ParentNode.Children.FirstOrDefault(SourceSelector);
        if (SourceNode != null) {
            this.Source = SourceNode.Attributes["href"]?.Value ?? SourceNode.Text[8..];
        }
    }
}
