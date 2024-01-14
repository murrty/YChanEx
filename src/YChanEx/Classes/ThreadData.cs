#nullable enable
namespace YChanEx;
using System.Runtime.Serialization;
using YChanEx.Posts;

[DataContract]
public sealed class ThreadData {
    /// <summary>
    /// The chan type associated with the data.
    /// </summary>
    [IgnoreDataMember]
    public ChanType ChanType { get; set; }
    /// <summary>
    /// The overall status of the thread. (Alive/404/Aborted/Archived).
    /// <seealso cref="YChanEx.ThreadStatus"/>
    /// </summary>
    [DataMember]
    public ThreadState ThreadState { get; set; }

    /// <summary>
    /// The ID of the thread being downloaded.
    /// </summary>
    [DataMember]
    public string Id { get; set; }
    /// <summary>
    /// The board that the thread that's being downloaded is on. Generally an acronym or a single letter.
    /// </summary>
    [DataMember]
    public string Board { get; set; }
    /// <summary>
    /// The URL of the thread that will be downloaded.
    /// </summary>
    [DataMember]
    public string Url { get; set; }
    /// <summary>
    /// The host string that will be used when downloading the API.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public string? UrlHost { get; set; }

    /// <summary>
    /// The list of post IDs that have been parsed.
    /// </summary>
    [DataMember]
    public List<ulong> ParsedPostIds { get; set; }
    /// <summary>
    /// The posts in the thread.
    /// </summary>
    [DataMember]
    public List<GenericPost> ThreadPosts { get; set; }
    /// <summary>
    /// Dictionary of duplicate names.
    /// </summary>
    [DataMember]
    public Dictionary<string, int> DuplicateNames { get; set; }
    /// <summary>
    /// The estimated total size of the thread.
    /// </summary>
    [DataMember]
    public long EstimatedSize { get; set; }

    /// <summary>
    /// Counts the images in the thread when they were added.
    /// <para>Used by all chans.</para>
    /// </summary>
    [DataMember]
    public int ThreadImagesCount { get; set; }
    /// <summary>
    /// Counts how many images were downloaded, for the form.
    /// <para>Used by all chans.</para>
    /// </summary>
    [DataMember]
    public int DownloadedImagesCount { get; set; }
    /// <summary>
    /// The amount of posts counted up to.
    /// <para>Used by 8chan and 8kun.</para>
    /// </summary>
    [DataMember]
    public int ThreadPostsCount { get; set; }

    /// <summary>
    /// The last modified date, used as a header when trying to download json or html information.
    /// <para>Uses "If-Modified-Since" header on HttpWebRequests to prevent overload.</para>
    /// <para>All download logic includes this, but only a few actually make use of it.</para>
    /// </summary>
    [DataMember]
    public DateTimeOffset? LastModified { get; set; }
    /// <summary>
    /// Determines if the thread is archived.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public bool ThreadArchived { get; set; }
    /// <summary>
    /// The board name of the thread retrieved from <seealso cref="BoardTitles"/>.
    /// <para>Chans that support user-made boards requires the name to be parsed from HTML.</para>
    /// </summary>
    [DataMember]
    public string? BoardName { get; set; }
    /// <summary>
    /// The name of the thread, given by OP of the Thread.
    /// <para>Used for easier thread identification.</para>
    /// </summary>
    [DataMember]
    public string? ThreadName { get; set; }
    /// <summary>
    /// The user-set name for the thread. Only appears in the main form listview, and the thread form text.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public string? CustomThreadName { get; set; }
    /// <summary>
    /// The subtitle of the board used by certain chans.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public string? BoardSubtitle { get; set; }

    /// <summary>
    /// The thread name that is displayed on the download form.
    /// </summary>
    [IgnoreDataMember]
    public string DownloadFormThreadNameDisplay {
        get {
            return CustomThreadName ?? ThreadName ?? Id;
        }
    }
    /// <summary>
    /// Json file path, for reloading the thread.
    /// </summary>
    [IgnoreDataMember]
    public string JsonFilePath { get; set; } = Environment.CurrentDirectory;

    public ThreadData(string ThreadId, string ThreadBoard, string Url, ChanType Type) {
        this.Id = ThreadId;
        this.Board = ThreadBoard;
        this.Url = Url;
        this.ParsedPostIds = [];
        this.ThreadPosts = [];
        this.DuplicateNames = [];
        this.ChanType = Type;
    }
}