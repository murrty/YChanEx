#nullable enable
namespace YChanEx.Posts;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using static YChanEx.Parsers.EightChan;
[DataContract]
internal sealed class EightChanThread {
    [DataMember(Name = "signedRole")]
    public string? signedRole { get; set; }

    [DataMember(Name = "id")]
    public string? id { get; set; }

    [DataMember(Name = "name")]
    public string? name { get; set; }

    [DataMember(Name = "email")]
    public string? email { get; set; }

    //[DataMember(Name = "boardUri")]
    //public string? boardUri { get; set; }

    [DataMember(Name = "threadId")]
    public ulong threadId { get; set; }

    [DataMember(Name = "subject")]
    public string? subject { get; set; }

    [DataMember(Name = "markdown")]
    public string? markdown { get; set; }

    [DataMember(Name = "message")]
    public string? message { get; set; }

    [DataMember(Name = "creation")]
    public string? creation { get; set; }

    [DataMember(Name = "locked")]
    public bool locked { get; set; }

    [DataMember(Name = "archived")]
    public bool archived { get; set; }

    [DataMember(Name = "pinned")]
    public bool pinned { get; set; }

    //[DataMember(Name = "cyclic")]
    //public bool cyclic { get; set; }

    [DataMember(Name = "autoSage")]
    public bool autoSage { get; set; }

    [DataMember(Name = "files")]
    public EightChanFile[]? files { get; set; }

    [DataMember(Name = "posts")]
    public EightChanPost[]? posts { get; set; }

    [DataMember(Name = "uniquePosters")]
    public int uniquePosters { get; set; }

    //[DataMember(Name = "maxMessageLength")]
    //public int maxMessageLength { get; set; }

    //[DataMember(Name = "usesCustomCss")]
    //public bool usesCustomCss { get; set; }

    //[DataMember(Name = "wssPort")]
    //public int wssPort { get; set; }

    //[DataMember(Name = "wsPort")]
    //public int wsPort { get; set; }

    //[DataMember(Name = "usesCustomJs")]
    //public bool usesCustomJs { get; set; }

    [DataMember(Name = "boardName")]
    public string? boardName { get; set; }

    [DataMember(Name = "boardDescription")]
    public string? boardDescription { get; set; }

    //[DataMember(Name = "boardMarkdown")]
    //public string boardMarkdown { get; set; }

    //[DataMember(Name = "maxFileCount")]
    //public int maxFileCount { get; set; }

    //[DataMember(Name = "maxFileSize")]
    //public string maxFileSize { get; set; }

    [IgnoreDataMember]
    public DateTimeOffset CleanedDateTime => GetPostTime(this);

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(files))]
    public bool HasFiles => files?.Length > 0;

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(files))]
    public bool MultiFilePost => HasFiles && files.Length > 1;

    public string GetCleanMessage(ThreadInfo Thread) => CleanMessage(this, Thread);

    [OnDeserialized]
    void Deserialized(StreamingContext ctx) {
        if (this.files?.Length > 0) {
            for (int i = 0; i < this.files.Length; i++) {
                this.files[i].ParentOp = this;
            }
        }
        if (this.posts?.Length > 0) {
            for (int i = 0; i < this.posts.Length; i++) {
                this.posts[i].Parent = this;
            }
        }
    }
}
