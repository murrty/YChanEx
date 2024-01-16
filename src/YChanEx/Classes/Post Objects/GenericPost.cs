#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
using YChanEx.Parsers;
using static YChanEx.Parsers.Helpers.ParsersShared;
[DataContract]
public sealed class GenericPost {
    [DataMember(Name = "post_id")]
    public ulong PostId { get; set; }

    [DataMember(Name = "post_time")]
    public DateTimeOffset PostDate { get; set; }

    [DataMember(Name = "poster_name")]
    public string PosterName { get; set; }

    [DataMember(Name = "poster_tripcode", EmitDefaultValue = false)]
    public string? PosterTripcode { get; set; }

    [DataMember(Name = "poster_capcode", EmitDefaultValue = false)]
    public string? PosterCapcode { get; set; }

    [DataMember(Name = "poster_id", EmitDefaultValue = false)]
    public string? PosterId { get; set; }

    [DataMember(Name = "post_subject", EmitDefaultValue = false)]
    public string? PostSubject { get; set; }

    [DataMember(Name = "post_message", EmitDefaultValue = false)]
    public string? PostMessage { get; set; }

    [DataMember(Name = "files")]
    public List<GenericFile> PostFiles { get; set; }

    [DataMember(Name = "tags")]
    public string[] Tags { get; set; }

    [DataMember(Name = "first", EmitDefaultValue = false)]
    public bool FirstPost { get; set; }

    [DataMember(Name = "quotes", EmitDefaultValue = false)]
    public ulong[]? Quotes { get; set; }

    [DataMember(Name = "quoted_by", EmitDefaultValue = false)]
    public ulong[]? QuotedBy { get; set; }

    [IgnoreDataMember]
    public bool HasFiles => PostFiles.Count > 0;

    [IgnoreDataMember]
    public bool MultiFilePost => PostFiles.Count > 1;

    [IgnoreDataMember]
    public long GetTotalSize {
        get {
            if (MultiFilePost) {
                return PostFiles
                    .Select(x => x.FileSize)
                    .Sum();
            }
            else if (HasFiles) {
                return PostFiles[0].FileSize;
            }
            return 0;
        }
    }

    [IgnoreDataMember]
    public string PostHtml { get; set; }

    private GenericPost() {
        this.PosterName = "Anonymous";
        this.PostFiles = [];
        this.Tags = [];
        this.PostHtml = string.Empty;
    }

    internal GenericPost(FourChanPost Post, ThreadInfo Thread) : this() {
        this.PostId = Post.no;
        this.PostDate = FourChan.GetPostTime(Post.time);
        if (Post.name != null) this.PosterName = Post.name;
        this.PosterTripcode = Post.trip;
        this.PosterCapcode = Post.capcode;
        this.PosterId = Post.id;
        this.PostSubject = Post.sub;
        this.PostMessage = Post.com;
        this.Quotes = Post.Quotes;

        if (Post.HasFile) {
            // query "?b=1" is a bypass.
            // see https://github.com/4chan/4chan-API/issues/99
            GenericFile NewFile = new(this) {
                FileId = Post.tim.ToString(),
                FileUrl = $"https://i.4cdn.org/{Thread.Data.Board}/{Post.tim}{Post.ext}?b=1",
                GeneratedFileName = Post.tim.ToString(),
                OriginalFileName = Post.filename,
                FileExtension = Post.ext.Trim('.'),
                FileHash = Post.md5,
                FileDimensions = new(Post.w, Post.h),
                FileSize = Post.fsize,
                ThumbnailFileUrl = $"https://i.4cdn.org/{Thread.Data.Board}/{Post.tim}s{Post.ext}?b=1",
                ThumbnailFileName = Post.tim.ToString() + "s",
                ThumbnailFileExtension = "jpg",
                ThumbnailFileDimensions = new(Post.tn_w, Post.tn_h),
                ThumbnailFileSpoiled = Post.spoiler > 0,
            };

            this.PostFiles.Add(NewFile);
        }
    }

    // TODO: Find 7chan spoiler?
    internal GenericPost(SevenChanPost Post) : this() {
        this.PostId = Post.PostId;
        this.PostDate = Post.PostTime;
        if (Post.PosterName != null) this.PosterName = Post.PosterName;
        this.PosterTripcode = Post.PosterTripcode;
        this.PosterCapcode = Post.PosterCapcode;
        this.PosterId = Post.PosterId;
        this.PostSubject = Post.Subject;
        this.PostMessage = Post.MessageBody;
        this.Quotes = Post.Quotes;

        if (Post.HasFiles) {
            for (int i = 0; i < Post.Files.Length; i++) {
                var File = Post.Files[i];

                GenericFile NewFile = new(this) {
                    FileId = File.FileId,
                    FileUrl = File.Url,
                    GeneratedFileName = File.FileId,
                    OriginalFileName = File.FileName,
                    FileExtension = File.Extension,
                    FileHash = null,
                    FileDimensions = new(File.Width, File.Height),
                    FileSize = File.EstimatedSize,
                    ThumbnailFileUrl = File.ThumbnailUrl,
                    ThumbnailFileName = File.FileId + "s",
                    ThumbnailFileExtension = "jpg",
                    ThumbnailFileDimensions = new(File.ThumbnailWidth, File.ThumbnailHeight),
                    ThumbnailFileSpoiled = false,
                };

                this.PostFiles.Add(NewFile);
            }
        }
    }

    // TODO: Find 8chan capcode (non signed role) & tripcode & spoiler?
    internal GenericPost(EightChanThread Post, ThreadInfo Thread) : this() {
        this.PostId = Post.threadId;
        this.PostDate = Post.CleanedDateTime;
        if (Post.name != null) this.PosterName = Post.name;
        //this.PosterTripcode = Post.tripcode;
        this.PosterCapcode = Post.signedRole;
        this.PosterId = Post.id;
        this.PostSubject = Post.subject;
        this.PostMessage = Post.GetCleanMessage(Thread);
        this.Quotes = Post.Quotes;

        if (Post.HasFiles) {
            for (int i = 0; i < Post.files.Length; i++) {
                var File = Post.files[i];
                string id = File.id!;

                GenericFile NewFile = new(this) {
                    FileId = id,
                    FileUrl = $"https://8chan.moe{File.path}",
                    GeneratedFileName = id,
                    OriginalFileName = File.originalName![..File.originalName!.LastIndexOf('.')],
                    FileExtension = File.mime!.Split('/')[1],
                    FileHash = null,
                    FileDimensions = new(File.width, File.height),
                    FileSize = File.size,
                    ThumbnailFileUrl = File.ProperThumbPath,
                    ThumbnailFileName = "t_" + id,
                    ThumbnailFileExtension = "png",
                    ThumbnailFileDimensions = File.ThumbnailSize,
                    ThumbnailFileSpoiled = false,
                };

                this.PostFiles.Add(NewFile);
            }
        }
    }
    internal GenericPost(EightChanPost Post, ThreadInfo Thread) : this() {
        this.PostId = Post.postId;
        this.PostDate = Post.CleanedDateTime;
        if (Post.name != null) this.PosterName = Post.name;
        //this.PosterTripcode = Post.tripcode;
        this.PosterCapcode = Post.signedRole;
        this.PosterId = Post.id;
        this.PostSubject = Post.subject;
        this.PostMessage = Post.GetCleanMessage(Thread);
        this.Quotes = Post.Quotes;

        if (Post.HasFiles) {
            for (int i = 0; i < Post.files.Length; i++) {
                var File = Post.files[i];
                string id = File.id!;

                GenericFile NewFile = new(this) {
                    FileId = id,
                    FileUrl = $"https://8chan.moe{File.path}",
                    GeneratedFileName = id,
                    OriginalFileName = File.originalName![..File.originalName!.LastIndexOf('.')],
                    FileExtension = File.mime!.Split('/')[1],
                    FileHash = null,
                    FileDimensions = new(File.width, File.height),
                    FileSize = File.size,
                    ThumbnailFileUrl = File.ProperThumbPath,
                    ThumbnailFileName = "t_" + id,
                    ThumbnailFileExtension = "png",
                    ThumbnailFileDimensions = File.ThumbnailSize,
                    ThumbnailFileSpoiled = false,
                };

                this.PostFiles.Add(NewFile);
            }
        }
    }

    internal GenericPost(EightKunPost Post) : this() {
        this.PostId = Post.no;
        this.PostDate = Post.CleanedDateTime;
        if (Post.name != null) this.PosterName = Post.name;
        this.PosterTripcode = Post.trip;
        this.PosterCapcode = Post.capcode;
        this.PosterId = Post.id;
        this.PostSubject = Post.sub;
        this.PostMessage = Post.CleanedMessage;
        this.Quotes = Post.Quotes;

        if (Post.HasFiles) {
            GenericFile NewFile = new(this) {
                FileId = Post.tim,
                FileUrl = $"https://media.128ducks.com/file_store/{Post.tim}{Post.ext}",
                GeneratedFileName = Post.tim,
                OriginalFileName = Post.filename,
                FileExtension = Post.ext[..Post.ext.LastIndexOf('.')],
                FileHash = Post.md5,
                FileDimensions = new(Post.w, Post.h),
                FileSize = Post.fsize,
                ThumbnailFileUrl = $"https://media.128ducks.com/file_store/thumb/{Post.tim}.jpg",
                ThumbnailFileName = Post.tim,
                ThumbnailFileExtension = "json",
                ThumbnailFileDimensions = new(Post.tn_w, Post.tn_h),
                ThumbnailFileSpoiled = Post.spoiler > 0,
            };

            this.PostFiles.Add(NewFile);

            if (!Post.MultiPost) {
                return;
            }

            for (int i = 0; i < Post.extra_files.Length; i++) {
                var File = Post.extra_files[i];

                NewFile = new(this) {
                    FileId = Post.tim,
                    FileUrl = $"https://media.128ducks.com/file_store/{File.tim}{File.ext}",
                    GeneratedFileName = File.tim,
                    OriginalFileName = File.filename,
                    FileExtension = File.ext![..File.ext!.LastIndexOf('.')],
                    FileHash = File.md5,
                    FileDimensions = new(File.w, File.h),
                    FileSize = File.fsize,
                    ThumbnailFileUrl = $"https://media.128ducks.com/file_store/thumb/{File.tim}.jpg",
                    ThumbnailFileName = File.tim,
                    ThumbnailFileExtension = "json",
                    ThumbnailFileDimensions = new(File.tn_w, File.tn_h),
                    ThumbnailFileSpoiled = File.spoiler > 0,
                };

                this.PostFiles.Add(NewFile);
            }
        }
    }

    // TODO: Find fchan capcode & poster id & spoiler?
    internal GenericPost(FChanPost Post) : this() {
        this.PostId = Post.PostId;
        this.PostDate = Post.PostTime;
        if (Post.PosterName != null) this.PosterName = Post.PosterName;
        this.PosterTripcode = Post.PosterTripcode;
        //this.PosterCapcode = Post.PosterCapcode;
        //this.PosterId = Post.PosterId;
        this.PostSubject = Post.Subject;
        this.PostMessage = Post.MessageBody;
        this.Quotes = Post.Quotes;

        if (Post.HasFile) {
            var File = Post.File;
            GenericFile Newfile = new(this) {
                FileId = Post.PostId.ToString(),
                FileUrl = File.Url,
                GeneratedFileName = GetFileNameFromUrl(File.Url!),
                OriginalFileName = File.FileName,
                FileExtension = File.Extension,
                FileHash = null,
                FileDimensions = new(File.Width, File.Height),
                FileSize = File.EstimatedSize,
                ThumbnailFileUrl = File.ThumbnailUrl,
                ThumbnailFileName = GetFileNameFromUrl(File.ThumbnailUrl!),
                ThumbnailFileExtension = "jpg",
                ThumbnailFileDimensions = new(File.ThumbnailWidth, File.ThumbnailHeight),
                ThumbnailFileSpoiled = false,
            };

            this.PostFiles.Add(Newfile);
        }
    }

    internal GenericPost(U18ChanPost Post) : this() {
        this.PostId = Post.PostId;
        this.PostDate = Post.PostTime;
        if (Post.PosterName != null) this.PosterName = Post.PosterName;
        //this.PosterTripcode = Post.PosterTripcode;
        this.PosterCapcode = Post.PosterCapcode;
        //this.PosterId = Post.PosterId;
        this.PostSubject = Post.Subject;
        this.PostMessage = Post.MessageBody;
        this.Quotes = Post.Quotes;

        if (Post.HasFile) {
            var File = Post.File;
            GenericFile Newfile = new(this) {
                FileId = Post.PostId.ToString(),
                FileUrl = File.Url,
                GeneratedFileName = GetFileNameFromUrl(File.Url!),
                OriginalFileName = File.FileName,
                FileExtension = File.Extension,
                FileHash = null,
                FileDimensions = new(File.Width, File.Height),
                FileSize = File.EstimatedSize,
                ThumbnailFileUrl = File.ThumbnailUrl,
                ThumbnailFileName = GetFileNameFromUrl(File.ThumbnailUrl!),
                ThumbnailFileExtension = "jpg",
                ThumbnailFileDimensions = new(File.ThumbnailWidth, File.ThumbnailHeight),
                ThumbnailFileSpoiled = Post.File.Spoiler,
            };

            this.PostFiles.Add(Newfile);
        }

        if (Post.HasTags) {
            this.Tags = Post.Tags
                .Select(x => x.Name)
                .Where(x => !x.IsNullEmptyWhitespace())
                .ToArray();
        }
    }

    internal GenericPost(FoolFuukaPost Post) : this() {
        this.PostId = Post.num;
        this.PostDate = FoolFuuka.GetPostTime(Post.timestamp);
        if (Post.name != null) this.PosterName = Post.name;
        this.PosterTripcode = Post.trip;
        this.PosterCapcode = Post.capcode switch {
            "m" or "M" => "## Mod",
            "a" or "A" => "## Admin",
            "n" or "N" => null,
            _ => $"## {Post.capcode ?? "?"}"
        };
        this.PosterId = Post.poster_hash_processed;
        this.PostSubject = Post.title;
        this.PostMessage = FoolFuuka.GetMessage(Post.comment_processed);
        this.Quotes = Post.Quotes;

        if (Post.HasFile) {
            var File = Post.media;
            GenericFile NewFile = new(this) {
                FileId = GetExtension(File.media!),
                FileUrl = File.media_link ?? File.remote_media_link,
                GeneratedFileName = GetFileNameFromUrl(File.media!),
                OriginalFileName = GetFileNameFromUrl(File.media_filename!),
                FileExtension = GetExtension(File.media!),
                FileHash = File.media_hash,
                FileDimensions = new(File.media_w, File.media_h),
                FileSize = File.media_size,
                ThumbnailFileUrl = File.thumb_link,
                ThumbnailFileName = GetFileNameFromUrl(File.thumb_link!),
                ThumbnailFileExtension = GetExtension(File.thumb_link!),
                ThumbnailFileDimensions = new(File.preview_w, File.preview_h),
                ThumbnailFileSpoiled = File.spoiler == 1,
            };

            this.PostFiles.Add(NewFile);
        }
    }

    [OnDeserialized]
    void Deserialized(StreamingContext ctx) {
        if (this.HasFiles) {
            for (int i = 0; i < this.PostFiles.Count; i++) {
                this.PostFiles[i].Parent = this;
            }
        }
    }

    public override bool Equals(object? obj) => obj is GenericPost other && this.Equals(other);
    public bool Equals(GenericPost? other) {
        if (other is null) {
            return this is null;
        }
        if (this is null) {
            return false;
        }
        return this.PostId == other.PostId;
    }

    public override int GetHashCode() => unchecked((int)(this.PostId & 0x7FFFFFFF)); // Negative-bit ignored.
}
