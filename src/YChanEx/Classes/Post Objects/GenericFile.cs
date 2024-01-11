#nullable enable
namespace YChanEx.Posts;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;

[DataContract]
public sealed class GenericFile {
    [DataMember(Name = "file_id")]
    public string? FileId { get; set; }

    [DataMember(Name = "file_url")]
    public string? FileUrl { get; set; }

    [DataMember(Name = "generated_file_name")]
    public string? GeneratedFileName { get; set; }

    [DataMember(Name = "original_file_name")]
    public string? OriginalFileName { get; set; }

    [DataMember(Name = "file_extension")]
    public string? FileExtension { get; set; }

    [DataMember(Name = "file_hash")]
    public string? FileHash { get; set; }

    [DataMember(Name = "file_dimensions")]
    public Size FileDimensions { get; set; }

    [DataMember(Name = "file_size")]
    public long FileSize { get; set; }

    [DataMember(Name = "thumbnail_file_url")]
    public string? ThumbnailFileUrl { get; set; }

    [DataMember(Name = "thumbnail_name")]
    public string? ThumbnailFileName { get; set; }

    [DataMember(Name = "thumbnail_extension")]
    public string? ThumbnailFileExtension { get; set; }

    [DataMember(Name = "thumbnail_dimensions")]
    public Size ThumbnailFileDimensions { get; set; }

    [DataMember(Name = "thumbnail_spoiled")]
    public bool ThumbnailFileSpoiled { get; set; }

    [DataMember(Name = "status")]
    public FileDownloadStatus Status { get; set; }

    [DataMember(Name = "saved_file")]
    public string? SavedFile { get; set; }

    [DataMember(Name = "saved_file_name")]
    public string? SavedFileName { get; set; }

    [DataMember(Name = "saved_thumbnail_file")]
    public string? SavedThumbnailFile { get; set; }

    [DataMember(Name = "saved_thumbnail_file_name")]
    public string? SavedThumbnailFileName { get; set; }

    [IgnoreDataMember]
    public ListViewItem ListViewItem { get; set; } = null!;

    [IgnoreDataMember]
    public GenericPost Parent { get; set; }

    public GenericFile(GenericPost Parent) {
        this.Parent = Parent;
    }

    public override bool Equals(object obj) => obj is GenericFile other && this.FileId == other.FileId;
    public override int GetHashCode() => this.FileId?.GetHashCode() ?? 0;
}
