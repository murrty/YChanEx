#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Runtime.Serialization;
[DataContract]
[DebuggerDisplay("{num} | HasFile = {HasFile}")]
public sealed class FoolFuukaPost {
    [DataMember]
    public ulong doc_id { get; set; }

    [DataMember]
    public ulong num { get; set; }

    [DataMember]
    public int subnum { get; set; }

    [DataMember]
    public ulong thread_num { get; set; }

    [DataMember]
    public byte op { get; set; }

    [DataMember]
    public long timestamp { get; set; }

    [DataMember]
    public int timestamp_expired { get; set; }

    [DataMember]
    public string? capcode { get; set; }

    [DataMember]
    public string? email { get; set; }

    [DataMember]
    public string? name { get; set; }

    [DataMember]
    public string? trip { get; set; }

    [DataMember]
    public string? title { get; set; }

    [DataMember]
    public string? comment { get; set; }

    [DataMember]
    public string? poster_hash { get; set; }

    [DataMember]
    public string? poster_country { get; set; }

    [DataMember]
    public byte sticky { get; set; }

    [DataMember]
    public byte locked { get; set; }

    [DataMember]
    public byte deleted { get; set; }

    [DataMember]
    public object? nreplies { get; set; }

    [DataMember]
    public object? nimages { get; set; }

    [DataMember]
    public string? fourchan_date { get; set; }

    [DataMember]
    public string? comment_sanitized { get; set; }

    [DataMember]
    public string? comment_processed { get; set; }

    [DataMember]
    public bool formatted { get; set; }

    [DataMember]
    public string? title_processed { get; set; }

    [DataMember]
    public string? name_processed { get; set; }

    [DataMember]
    public string? email_processed { get; set; }

    [DataMember]
    public string? trip_processed { get; set; }

    [DataMember]
    public string? poster_hash_processed { get; set; }

    [DataMember]
    public bool poster_country_name { get; set; }

    [DataMember]
    public string? poster_country_name_processed { get; set; }

    [DataMember]
    public object[]? extra_data { get; set; }

    [DataMember]
    public object? exif { get; set; }

    [DataMember]
    public FoolFuukaFile? media { get; set; }

    [DataMember]
    public FoolFuukaBoard? board { get; set; }

    [IgnoreDataMember]
    public bool HasFile => media != null;

    [IgnoreDataMember]
    public ulong[]? Quotes {
        get {
            if (comment_sanitized.IsNullEmptyWhitespace()) {
                return null;
            }

            var Matches = Parsers.Helpers.ParsersShared.RepliesSimpleRegex.Matches(comment_sanitized);
            if (Matches.Count < 1) {
                return null;
            }

            return Matches
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(x => {
                    bool success = ulong.TryParse(x.Value[2..], out ulong value);
                    return new { value, success };
                })
                .Where(x => x.success)
                .Select(x => x.value)
                .Distinct()
                .ToArray();
        }
    }

    public override bool Equals(object? obj) => obj is FoolFuukaPost other && this.Equals(other);
    public bool Equals(FoolFuukaPost other) {
        if (other is null) {
            return this is null;
        }
        if (this is null) {
            return false;
        }
        return this.num == other.num;
    }

    public override int GetHashCode() => unchecked((int)(this.num & 0x7FFFFFFF)); // Negative-bit ignored.
}
