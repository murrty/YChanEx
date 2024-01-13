#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.SevenChan;
[DataContract]
[DebuggerDisplay("{PostId} @ {PostTime} | Files = {Files != null}, Message = {MessageBody != null}")]
internal sealed class SevenChanPost : IEquatable<SevenChanPost> {
    // Post data
    private static readonly Selector HeaderSelector = Selector.ParseSelector("div[class=\"post_header\"]");
    private static readonly Selector SubjectSelector = Selector.ParseSelector("span[class=\"subject\"]");
    private static readonly Selector DisplayNameSelector = Selector.ParseSelector("span[class=\"postername\"]");
    private static readonly Selector TripcodeSelector = Selector.ParseSelector("span[class=\"postertrip\"]");
    private static readonly Selector CapcodeSelector = Selector.ParseSelector("span[class=\"capcode\"]");
    private static readonly Selector RefLinkSelector = Selector.ParseSelector("span[class=\"reflink\"]");
    // Single-file
    private static readonly Selector ThumbnailSelector = Selector.ParseSelector("div[class=\"post_thumb\"]");
    // Multi-file
    private static readonly Selector MultiFileSelector = Selector.ParseSelector("div > span[class:=\"multithumb(first)?\"]");
    private static readonly Selector MessageBodySelector = Selector.ParseSelector("p[class=message]");

    [DataMember(Name = "post_id")]
    public ulong PostId { get; set; }

    [DataMember(Name = "subject")]
    public string? Subject { get; set; }

    [DataMember(Name = "name")]
    public string? PosterName { get; set; }

    [DataMember(Name = "tripcode")]
    public string? PosterTripcode { get; set; }

    [DataMember(Name = "capcode")]
    public string? PosterCapcode { get; set; }

    [DataMember(Name = "mailto")]
    public string? PosterEmail { get; set; }

    [DataMember(Name = "poster_id")]
    public string? PosterId { get; set; }

    [DataMember(Name = "body")]
    public string? MessageBody { get; set; }

    [DataMember(Name = "files")] // Can only contain 4 files in total
    public SevenChanFile[]? Files { get; set; }

    [DataMember(Name = "post_time")]
    public DateTimeOffset PostTime { get; set; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(Files))]
    public bool HasFiles => Files?.Length > 0;

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(Files))]
    public bool MultiFilePost => HasFiles && Files.Length > 1;

    [IgnoreDataMember]
    public ulong[]? Quotes {
        get {
            if (MessageBody.IsNullEmptyWhitespace()) {
                return null;
            }

            var Matches = Parsers.Helpers.ParsersShared.RepliesRegex.Matches(MessageBody);
            if (Matches.Count < 1) {
                return null;
            }

            return Matches
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(x => x.Value[8..^1])
                .Select(ulong.Parse)
                .Distinct()
                .ToArray();
        }
    }

    public SevenChanPost(HtmlElementNode node) {
        // Parse post id.
        this.PostId = node.Attributes.TryGetValue("id", out var PostIdNode) ? uint.Parse(PostIdNode.Value) :
            throw new ArgumentNullException("Could not find post id.");

        // Header
        var HeaderNode = node.Children.FirstOrDefault(HeaderSelector) ??
            throw new ArgumentNullException("Could not find header node.");

        // Subject
        var SubjectNode = HeaderNode.Children.FirstOrDefault(SubjectSelector);
        if (SubjectNode != null) {
            this.Subject = SubjectNode.Text;
        }

        // Name & email
        var PosterNameNode = HeaderNode.Children.FirstOrDefault(DisplayNameSelector) ??
            throw new ArgumentNullException("Could not find poster name.");
        var PosterEmailNode = PosterNameNode.Children.FirstOrDefault(DefaultSelectors.A.Href);
        if (PosterEmailNode != null) {
            this.PosterEmail = PosterEmailNode.Attributes["href"]!.Value.RemoveFromStart("mailto:", StringComparison.InvariantCultureIgnoreCase);
            this.PosterName = PosterEmailNode.Text;
        }
        else {
            this.PosterName = PosterNameNode.Text;
        }

        // Tripcode
        var PosterTripNode = HeaderNode.Children.FirstOrDefault(TripcodeSelector);
        if (PosterTripNode != null) {
            this.PosterTripcode = PosterTripNode.Text;
        }

        // Capcode
        var CapcodeNode = HeaderNode.Children.FirstOrDefault(CapcodeSelector);
        if (CapcodeNode != null) {
            this.PosterCapcode = CapcodeNode.Text;
        }

        // Post ID & Time
        var RefLinkNode = HeaderNode.Children.FirstOrDefault(RefLinkSelector) ??
            throw new ArgumentNullException("Could not find reflink.");
        this.PostTime = ConvertTimestampToDateTime(RefLinkNode.PrevNode!.Text);
        this.PosterId = RefLinkNode.Text.StartsWith("no.", StringComparison.OrdinalIgnoreCase) ? RefLinkNode.Text[4..] : RefLinkNode.NextNode!.Text[4..];

        // Find the thumbnail(s)
        var ThumbnailNode = node.Children.FirstOrDefault(ThumbnailSelector);
        if (ThumbnailNode != null) {
            Files = [ new SevenChanFile(ThumbnailNode, node, false, this) ];
        }
        else {
            var MultiPostNode = node.Children.Find(MultiFileSelector)
                .ToArray();

            if (MultiPostNode.Length > 0) {
                Files = new SevenChanFile[MultiPostNode.Length];
                for (int i = 0; i < MultiPostNode.Length; i++) {
                    Files[i] = new SevenChanFile(MultiPostNode[i], node, true, this);
                }
            }
        }

        // Message
        var MessageNode = node.Children.FirstOrDefault(MessageBodySelector);
        this.MessageBody = GetMessage(MessageNode);
    }

    public override bool Equals(object? obj) => obj is SevenChanPost other && this.Equals(other);
    public bool Equals(SevenChanPost other) {
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
