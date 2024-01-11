#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
internal sealed class FourChanPost {
    [DataMember(Name = "no")]
    public ulong no { get; set; }

    //[DataMember(Name = "now")]
    //public string? now { get; set; }

    [DataMember(Name = "name")]
    public string? name { get; set; }

    [DataMember(Name = "com")]
    public string? com { get; set; }

    [DataMember(Name = "filename")]
    public string? filename { get; set; }

    [DataMember(Name = "sub")]
    public string? sub { get; set; }

    [DataMember(Name = "ext")]
    public string? ext { get; set; }

    [DataMember(Name = "w")]
    public int w { get; set; }

    [DataMember(Name = "h")]
    public int h { get; set; }

    [DataMember(Name = "tn_w")]
    public int tn_w { get; set; }

    [DataMember(Name = "tn_h")]
    public int tn_h { get; set; }

    [DataMember(Name = "tim")]
    public long tim { get; set; }

    [DataMember(Name = "time")]
    public long time { get; set; }

    [DataMember(Name = "md5")]
    public string? md5 { get; set; }

    [DataMember(Name = "fsize")]
    public long fsize { get; set; }

    [DataMember(Name = "spoiler")]
    public int spoiler { get; set; }

    //[DataMember(Name = "resto")]
    //public int? resto { get; set; }

    [DataMember(Name = "id")]
    public string? id { get; set; }

    [DataMember(Name = "capcode")]
    public string? capcode { get; set; }

    [DataMember(Name = "trip")]
    public string? trip { get; set; }

    //[DataMember(Name = "bumplimit")]
    //public int bumplimit { get; set; }

    //[DataMember(Name = "imagelimit")]
    //public int imagelimit { get; set; }

    //[DataMember(Name = "semantic_url")]
    //public string? semantic_url { get; set; }

    //[DataMember(Name = "custom_spoiler")]
    //public int custom_spoiler { get; set; }

    //[DataMember(Name = "replies")]
    //public int replies { get; set; }

    //[DataMember(Name = "images")]
    //public int images { get; set; }

    //[DataMember(Name = "unique_ips")]
    //public int unique_ips { get; set; }

    //[DataMember(Name = "sticky")]
    //public int sticky { get; set; }

    //[DataMember(Name = "closed")]
    //public int closed { get; set; }

    [DataMember(Name = "archived")]
    public bool archived { get; set; }

    [DataMember(Name = "filedeleted")]
    public int filedeleted { get; set; }

    [IgnoreDataMember]
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(filename), nameof(ext), nameof(md5))]
    public bool HasFile {
        get {
            return !string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(ext) && !string.IsNullOrWhiteSpace(md5);
        }
    }
}
