#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
public sealed class FoolFuukaFile {
    [DataMember]
    public long media_id { get; set; }

    [DataMember]
    public byte spoiler { get; set; }

    [DataMember]
    public string? preview_orig { get; set; }

    [DataMember]
    public string? media { get; set; }

    [DataMember]
    public string? preview_op { get; set; }

    [DataMember]
    public string? preview_reply { get; set; }

    [DataMember]
    public int preview_w { get; set; }

    [DataMember]
    public int preview_h { get; set; }

    [DataMember]
    public string? media_filename { get; set; }

    [DataMember]
    public int media_w { get; set; }

    [DataMember]
    public int media_h { get; set; }

    [DataMember]
    public long media_size { get; set; }

    [DataMember]
    public string? media_hash { get; set; }

    [DataMember]
    public string? media_orig { get; set; }

    [DataMember]
    public string? exif { get; set; }

    [DataMember]
    public int total { get; set; }

    [DataMember]
    public byte banned { get; set; }

    [DataMember]
    public string? media_status { get; set; }

    [DataMember]
    public string? safe_media_hash { get; set; }

    [DataMember]
    public string? remote_media_link { get; set; }

    [DataMember]
    public string? media_link { get; set; }

    [DataMember]
    public string? thumb_link { get; set; }

    [DataMember]
    public string? media_filename_processed { get; set; }
}
