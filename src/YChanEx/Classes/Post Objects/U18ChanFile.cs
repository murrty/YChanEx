#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.U18Chan;
using static YChanEx.Parsers.Helpers.ParsersShared;
[DataContract]
[DebuggerDisplay("Ext = {Extension}")]
internal sealed class U18ChanFile {
    private static readonly Selector ThumbnailSelector = Selector.ParseSelector("a[href=] > img[src=][style=][data-original=]");
    private static readonly Selector SpoiledThumbnailSelector = Selector.ParseSelector("a[href=] > img[class=\"ThumbnailImageReply\"]");

    [IgnoreDataMember]
    public U18ChanPost Parent { get; }

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

    [DataMember(Name = "spoiler")]
    public bool Spoiler { get; set; }

    public U18ChanFile(HtmlElementNode UrlNode, U18ChanPost Parent, bool firstPost) {
        this.Parent = Parent;

        // URL of the file
        var hrefAttrib = UrlNode.Attributes["href"]!;
        this.Url = hrefAttrib.Value;
        var FileNameNode = UrlNode.Children.First(DefaultSelectors.u);

        // The rest of the information is optional, maybe don't throw?
        // The file name and extension can be inferred from the URL.

        // File name + extension
        int lastIndex = FileNameNode.Text.LastIndexOf('.');
        this.Extension = GetExtension(FileNameNode.Text);
        string FileName = GetNameWithoutExtension(FileNameNode.Text);
        while (FileName.EndsWith("_u18chan", StringComparison.OrdinalIgnoreCase)) {
            FileName = FileName[..^8];
        }
        this.FileName = FileName;

        // File metadata (size, dimensions)
        var InfoNode = UrlNode.NextNode is HtmlTextNode t ?
            t : throw new ArgumentNullException($"Could not find metadata. (ID {Parent.PostId})");

        string[] Info = InfoNode.Text[(InfoNode.Text.IndexOf('(') + 1)..InfoNode.Text.LastIndexOf(')')]
            .Split([','], StringSplitOptions.RemoveEmptyEntries)[..2];
        this.EstimatedSize = ConvertSizeToBytes(Info[0]);
        var Dimensions = ConvertDimensionsToSize(Info[1].Trim());
        this.Width = Dimensions.Width;
        this.Height = Dimensions.Height;

        var TrueParentNode = UrlNode.ParentNode?.ParentNode ??
            throw new ArgumentNullException($"Could not find parent node. (ID {Parent.PostId})");

        var ThumbnailNode = TrueParentNode.Children.FirstOrDefault(ThumbnailSelector);

        if (ThumbnailNode != null) {
            this.ThumbnailUrl = ThumbnailNode.Attributes["data-original"]!.Value;
            //string ThumbnailLink = FileLink[0..^12] + "s_u18chan" + FileExtension;
            var ThumbnailSize = ConvertThumbnailAttributesToSize(ThumbnailNode.Attributes["style"]!);
            this.ThumbnailWidth = ThumbnailSize.Width;
            this.ThumbnailHeight = ThumbnailSize.Height;
        }
        else {
            ThumbnailNode = TrueParentNode.Children.FirstOrDefault(SpoiledThumbnailSelector) ??
                throw new ArgumentNullException($"Could not find spoiled thumbnail node, the thread may be broken. (ID {Parent.PostId})");
            this.Spoiler = true;

            // The thumbnail exists so we can just brute force it.
            // The size, however, is a mystery.
            this.ThumbnailUrl = this.Url![..(this.Url!.LastIndexOf('.') - 8)] + "s_u18chan." + this.Extension;
            var ThumbSize = GetThumbnailSize(this.Width, this.Height, firstPost);
            this.ThumbnailWidth = ThumbSize.Width;
            this.ThumbnailHeight = ThumbSize.Height;
        }
    }
}
