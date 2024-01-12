﻿#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static YChanEx.Parsers.EightChan;
[DataContract]
internal sealed class EightChanPost {
    // Regex
    private static readonly Regex RepliesRegex = new("href=\"/[a-zA-Z0-9_]+/res/\\d+\\.html#\\d+\"", RegexOptions.IgnoreCase);

    [IgnoreDataMember]
    public EightChanThread? Parent { get; set; }

    [DataMember(Name = "name")]
    public string? name { get; set; }

    [DataMember(Name = "signedRole")]
    public string? signedRole { get; set; }

    [DataMember(Name = "email")]
    public string? email { get; set; }

    [DataMember(Name = "id")]
    public string? id { get; set; }

    [DataMember(Name = "subject")]
    public string? subject { get; set; }

    [DataMember(Name = "markdown")]
    public string? markdown { get; set; }

    [DataMember(Name = "message")]
    public string? message { get; set; }

    [DataMember(Name = "postId")]
    public ulong postId { get; set; }

    [DataMember(Name = "creation")]
    public string? creation { get; set; } // UTC 0:00

    [DataMember(Name = "files")]
    public EightChanFile[]? files { get; set; }

    [IgnoreDataMember]
    public DateTimeOffset CleanedDateTime => GetPostTime(this);

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(files))]
    public bool HasFiles => files?.Length > 0;

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(files))]
    public bool MultiFilePost => HasFiles && files.Length > 1;

    [IgnoreDataMember]
    public ulong[]? RespondsTo {
        get {
            if (markdown.IsNullEmptyWhitespace()) {
                return null;
            }

            var Matches = RepliesRegex.Matches(markdown);
            if (Matches.Count < 1) {
                return null;
            }

            return Matches
                .Cast<Match>()
                .Select(x => x.Value[(x.Value.LastIndexOf('#') + 1)..^1])
                .Select(ulong.Parse)
                .ToArray();
        }
    }

    public string GetCleanMessage(ThreadInfo Thread) => CleanMessage(this, Thread);

    [OnDeserialized]
    void Deserialized(StreamingContext ctx) {
        if (this.files?.Length > 0) {
            for (int i = 0; i < this.files.Length; i++) {
                this.files[i].ParentReply = this;
            }
        }
    }
}
