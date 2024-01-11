#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.SevenChan;
using static YChanEx.Parsers.Helpers.ParsersShared;
[DataContract]
[DebuggerDisplay("Ext = {Extension}")]
internal sealed class SevenChanFile {
    // Single-file
    private static readonly Selector FileMetadataSelector = Selector.ParseSelector("p[class=\"file_size\"]");
    // Multi-file
    private static readonly Selector MultiFileImgSelector = Selector.ParseSelector("img[src][title]");

    [IgnoreDataMember]
    public SevenChanPost Parent { get; }

    [DataMember(Name = "file_name")]
    public string? FileName { get; set; }

    [DataMember(Name = "file_id")]
    public string? FileId { get; set; }

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

    public SevenChanFile(HtmlElementNode ThumbnailNode, HtmlElementNode ParentNode, bool MultiPost, SevenChanPost Parent) {
        this.Parent = Parent;

        if (MultiPost) {
            ExtractMultiPost(ThumbnailNode);
            return;
        }
        ExtractSingle(ThumbnailNode, ParentNode);
    }

    private void ExtractSingle(HtmlElementNode ThumbnailNode, HtmlElementNode ParentNode) {
        var ThumbnailImgNode = ThumbnailNode.Children.FirstOrDefault(DefaultSelectors.Img.src) ??
            throw new ArgumentNullException("Could not find thumbnail img node.");

        if (!ThumbnailImgNode.Attributes.TryGetValue("width", out var widthAttrib)) {
            throw new ArgumentNullException("Could not find thumbnail width.");
        }
        if (!ThumbnailImgNode.Attributes.TryGetValue("height", out var heightAttrib)) {
            throw new ArgumentNullException("Could not find thumbnail height.");
        }
        if (ThumbnailImgNode.ParentNode == null || !ThumbnailImgNode.ParentNode.Attributes.TryGetValue("href", out var hrefAttrib) || hrefAttrib.Value.IsNullEmptyWhitespace()) {
            throw new ArgumentNullException("Could not find img href node.");
        }

        this.Url = hrefAttrib.Value;
        this.FileId = hrefAttrib.Value[(hrefAttrib.Value.LastIndexOf('/') + 1)..hrefAttrib.Value.LastIndexOf('.')];
        this.Extension = GetExtension(hrefAttrib.Value);

        this.ThumbnailUrl = ThumbnailImgNode.Attributes["src"]!.Value;
        this.ThumbnailWidth = int.Parse(widthAttrib.Value);
        this.ThumbnailHeight = int.Parse(heightAttrib.Value);

        var FileMetadataNode = ParentNode.Children.FirstOrDefault(FileMetadataSelector) ??
            throw new ArgumentNullException("Could not find file metadata node.");
        var FileMetadata = FileMetadataNode.Children[2].Text[(FileMetadataNode.Children[2].Text.IndexOf('(') + 1)..FileMetadataNode.Children[2].Text.LastIndexOf(')')].Split(',');

        this.EstimatedSize = ConvertSizeToBytes(FileMetadata[0].Trim());
        var Dimensions = ConvertDimensionsToSize(FileMetadata[1].Trim());
        this.Width = Dimensions.Width;
        this.Height = Dimensions.Height;
        this.FileName = GetNameWithoutExtension(string.Join(",", FileMetadata[2..]).Trim());
    }
    private void ExtractMultiPost(HtmlElementNode ThumbnailNode) {
        var ImgNode = ThumbnailNode.ParentNode!.Children.FirstOrDefault(MultiFileImgSelector) ??
            throw new ArgumentNullException("Could not find img src.");

        var hrefAttrib = (ThumbnailNode.Children.FirstOrDefault(DefaultSelectors.A.HrefValue)?.Attributes["href"])
            ?? throw new ArgumentNullException("Could not find the link from the thumbnail node.");

        this.Url = hrefAttrib.Value!;
        this.FileId = hrefAttrib.Value![(hrefAttrib.Value!.LastIndexOf('/') + 1)..hrefAttrib.Value.LastIndexOf('.')];
        this.Extension = this.Url[(this.Url.LastIndexOf('.') + 1)..];

        this.ThumbnailUrl = ImgNode.Attributes["src"]!.Value;
        this.ThumbnailWidth = int.Parse(ImgNode.Attributes["width"]!.Value);
        this.ThumbnailHeight = int.Parse(ImgNode.Attributes["height"]!.Value);

        string MetadataString = ImgNode.Attributes["title"]!.Value!;
        var FileMetadata = MetadataString[(MetadataString.IndexOf('(') + 1)..MetadataString.LastIndexOf(')')].Split(',');

        this.EstimatedSize = ConvertSizeToBytes(FileMetadata[0].Trim());
        var Dimensions = ConvertDimensionsToSize(FileMetadata[1].Trim());
        this.Width = Dimensions.Width;
        this.Height = Dimensions.Height;
        this.FileName = string.Join(",", FileMetadata[2..]).Trim();
    }
}
