#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using static YChanEx.Parsers.EightKun;
[DataContract]
internal sealed class EightKunPost {
    [DataMember(Name = "no")]
    public ulong no { get; set; }

    [DataMember(Name = "sub")]
    public string? sub { get; set; }

    [DataMember(Name = "com")]
    public string? com { get; set; }

    [DataMember(Name = "name")]
    public string? name { get; set; }

    [DataMember(Name = "trip")]
    public string? trip { get; set; }

    [DataMember(Name = "capcode")]
    public string? capcode { get; set;}

    [DataMember(Name = "time")]
    public long time { get; set; } // unix timestamp

    [DataMember(Name = "omitted_posts")]
    public int omitted_posts { get; set; }

    [DataMember(Name = "omitted_images")]
    public int omitted_images { get; set; }

    [DataMember(Name = "sticky")]
    public int sticky { get; set; }

    [DataMember(Name = "locked")]
    public int locked { get; set; }

    [DataMember(Name = "cyclical")]
    public int cyclical { get; set; }

    [DataMember(Name = "bumplocked")]
    public int bumplocked { get; set; }

    [DataMember(Name = "last_modified")]
    public int last_modified { get; set; }

    [DataMember(Name = "id")]
    public string? id { get; set; }

    [DataMember(Name = "tx_link")]
    public bool tx_link { get; set; }

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

    [DataMember(Name = "resto")]
    public int resto { get; set; }

    [DataMember(Name = "extra_files")]
    public EightKunFile[]? extra_files { get; set; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(filename), nameof(ext), nameof(tim), nameof(md5))]
    public bool HasFiles {
        get {
            return !filename.IsNullEmptyWhitespace() && !ext.IsNullEmptyWhitespace();
        }
    }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(this.extra_files))]
    public bool MultiPost => this.extra_files?.Length > 0;

    [IgnoreDataMember]
    public bool OldPath => this.fpath < 1;

    [IgnoreDataMember]
    public DateTimeOffset CleanedDateTime => GetPostTime(this);

    [IgnoreDataMember]
    public string CleanedMessage => CleanMessage(com);

    [IgnoreDataMember]
    public ulong[]? RespondsTo {
        get {
            if (com.IsNullEmptyWhitespace()) {
                return null;
            }

            var Matches = Parsers.Helpers.ParsersShared.RepliesHtmlRegex.Matches(com);
            if (Matches.Count < 1) {
                return null;
            }

            return Matches
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(x => x.Value[(x.Value.LastIndexOf('#') + 1)..^1])
                .Select(ulong.Parse)
                .Distinct()
                .ToArray();
        }
    }

    [OnDeserialized]
    void Deserialized(StreamingContext ctx) {
        if (this.MultiPost) {
            for (int i = 0; i < this.extra_files.Length; i++) {
                this.extra_files[i].Parent = this;
            }
        }
    }

    public override bool Equals(object? obj) => obj is EightKunPost other && this.Equals(other);
    public bool Equals(EightKunPost? other) {
        if (other is null) {
            return this is null;
        }
        if (this is null) {
            return false;
        }
        return this.no == other.no;
    }

    public override int GetHashCode() => unchecked((int)(this.no & 0x7FFFFFFF)); // Negative-bit ignored.
}
