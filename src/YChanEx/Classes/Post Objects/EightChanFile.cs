#nullable enable
namespace YChanEx.Posts;
using System.Drawing;
using System.Runtime.Serialization;
using static YChanEx.Parsers.Helpers.ParsersShared;
[DataContract]
internal sealed class EightChanFile {
    [IgnoreDataMember]
    public EightChanThread? ParentOp { get; set; }

    [IgnoreDataMember]
    public EightChanPost? ParentReply { get; set; }

    [DataMember(Name = "originalName")]
    public string? originalName { get; set; }

    [DataMember(Name = "path")]
    public string? path { get; set; }

    [DataMember(Name = "thumb")]
    public string? thumb { get; set; }

    [DataMember(Name = "mime")]
    public string? mime { get; set; }

    [DataMember(Name = "size")]
    public long size { get; set; }

    [DataMember(Name = "width")]
    public int width { get; set; }

    [DataMember(Name = "height")]
    public int height { get; set; }

    [IgnoreDataMember]
    public string? id {
        get {
            if (this.path.IsNullEmptyWhitespace()) {
                throw new Exception("Could not get id from 8chan path");
            }
            return GetFileNameFromUrl(this.path!, 4); // /.media/...
        }
    }

    [IgnoreDataMember]
    public string? ProperThumbPath {
        get {
            if (propThumb != null) {
                return propThumb;
            }
            string original = id!;
            return propThumb = !thumb!.EndsWith(original, StringComparison.OrdinalIgnoreCase) ?
                ("/.media/t_" + original) : thumb;
        }
    }
    [IgnoreDataMember]
    private string? propThumb;

    [IgnoreDataMember]
    public Size ThumbnailSize {
        get {
            return GetThumbnailSize(width, height, ParentOp != null);
        }
    }
}
