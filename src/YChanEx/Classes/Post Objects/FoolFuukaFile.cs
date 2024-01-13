#nullable enable
namespace YChanEx.Posts;

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.FoolFuuka;
using static YChanEx.Parsers.Helpers.ParsersShared;
[DataContract]
internal sealed class FoolFuukaFile {
    private static readonly Selector ThumbnailSelector = Selector.ParseSelector("a[class=\"thread_image_link\"] img[src=]");
    private static readonly Selector BadThumbnailSelector = Selector.ParseSelector("a[class=\"thread_image_link\"] div[class=\"spoiler_box\"]");
    private static readonly Selector FileInfoSelector = Selector.ParseSelector("> div[class=\"post_file\"]");
    private static readonly Selector FileUrlSelector = Selector.ParseSelector("a[class=\"post_file_filename\"]");
    private static readonly Selector FileMetadataSelector = Selector.ParseSelector("span[class=\"post_file_metadata\"]");

    [IgnoreDataMember]
    public FoolFuukaPost Parent { get; }

    [DataMember(Name = "file_id")]
    public string? FileId { get; set; }

    [DataMember(Name = "file_name")]
    public string? FileName { get; set; }

    [DataMember(Name = "extension")]
    public string? Extension { get; set; }

    [DataMember(Name = "url")]
    public string? Url { get; set; }

    [DataMember(Name = "est_size")]
    public long EstimatedSize { get; set; }

    [DataMember(Name = "file_hash")]
    public string? FileHash { get; set; }

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

    [DataMember(Name = "spoiler")]
    public bool Spoiler { get; set; }

    public FoolFuukaFile(HtmlElementNode node, FoolFuukaPost Parent) {
        this.Parent = Parent;

        var ThumbnailNode = node.FirstOrDefault(ThumbnailSelector);

        // Thumbnail, hash, & spoiled
        if (ThumbnailNode == null) {
            _ = node.FirstOrDefault(BadThumbnailSelector) ??
                throw new ArgumentNullException("Could not find thumbnail node");
            this.Spoiler = true;
        }
        else {
            this.ThumbnailUrl = ThumbnailNode.Attributes["src"]!.Value!;
            this.ThumbnailWidth = int.Parse(ThumbnailNode.Attributes["width"]!.Value!);
            this.ThumbnailHeight = int.Parse(ThumbnailNode.Attributes["height"]!.Value!);
            this.FileHash = ThumbnailNode.Attributes["data-md5"]!.Value;
            this.Spoiler = ThumbnailNode.Attributes.ContainsWithValue("class", "is_spoiler_image");
        }

        // Metadata
        var FileInfoNode = node.FirstOrDefault(FileInfoSelector) ?? node.ParentNode!.FirstOrDefault(FileInfoSelector) ??
            throw new ArgumentNullException("Could not find file info node.");
        var PostMetadataNode = FileInfoNode.FirstOrDefault(FileMetadataSelector)?.Children[0] ?? FileInfoNode.Children[0];
        string[] InfoString = PostMetadataNode.Text.Trim(',', ' ').Split(',');
        this.EstimatedSize = ConvertSizeToBytes(InfoString[0].Trim());
        var FileSize = ConvertDimensionsToSize(InfoString[1].Trim());
        this.Width = FileSize.Width;
        this.Height = FileSize.Height;

        // Url, id, ext
        var UrlNode = FileInfoNode.Children.FirstOrDefault(FileUrlSelector) ??
            throw new ArgumentNullException("Could not find url node.");
        string url = UrlNode.Attributes["href"]!.Value!;
        this.Url = url;
        this.FileId = GetFileNameFromUrl(url);
        this.Extension = GetExtension(url);

        // Original file name
        if (UrlNode.Attributes.TryGetValue("title", out var NameAttrib)) {
            this.FileName = GetFileNameFromUrl(NameAttrib.Value!);
        }
        else {
            this.FileName = GetFileNameFromUrl(UrlNode.Text);
        }
    }
}
