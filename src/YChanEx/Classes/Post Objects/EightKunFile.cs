#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
internal sealed class EightKunFile {
    [IgnoreDataMember]
    public EightKunPost? Parent { get; set; }

    [DataMember(Name = "tn_h")]
    public int tn_h { get; set; }

    [DataMember(Name = "tn_w")]
    public int tn_w { get; set; }

    [DataMember(Name = "h")]
    public int h { get; set; }

    [DataMember(Name = "w")]
    public int w { get; set; }

    [DataMember(Name = "fsize")]
    public int fsize { get; set; }

    [DataMember(Name = "filename")]
    public string? filename { get; set; }

    [DataMember(Name = "ext")]
    public string? ext { get; set; }

    [DataMember(Name = "tim")]
    public string? tim { get; set; }

    [DataMember(Name = "fpath")]
    public int fpath { get; set; }

    [DataMember(Name = "spoiler")]
    public int spoiler { get; set; }

    [DataMember(Name = "md5")]
    public string? md5 { get; set; }
}
