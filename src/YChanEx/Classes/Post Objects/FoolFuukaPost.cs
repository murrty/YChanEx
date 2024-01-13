#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.FoolFuuka;
[DataContract]
[DebuggerDisplay("{PostId} | HasFile = {HasFile}")]
internal sealed class FoolFuukaPost {
    private static readonly Selector HeaderSelector = Selector.ParseSelector("header > div[class=\"post_data\"]");
    private static readonly Selector PostIdSelector = Selector.ParseSelector("a[data-post:=\"\\d+\"][data-function=\"quote\"]");
    private static readonly Selector PosterDataSelector = Selector.ParseSelector("span[class=\"post_poster_data\"]");
    private static readonly Selector SubjectSelector = Selector.ParseSelector("h2[class=\"post_title\"]");
    private static readonly Selector PosterNameSelector = Selector.ParseSelector("> span[class=\"post_author\"]");
    private static readonly Selector PosterTripcodeSelector = Selector.ParseSelector("> span[class=\"post_tripcode\"]");
    private static readonly Selector PosterCapcodeSelector = Selector.ParseSelector("> span[class=\"post_level\"]");
    private static readonly Selector PosterIdSelector = Selector.ParseSelector("> span[class=\"poster_hash\"]");
    private static readonly Selector PostTimeSelector = Selector.ParseSelector("span[class=\"time_wrap\"] > time[datetime=]");
    private static readonly Selector MessageSelector = Selector.ParseSelector("> div[class=\"text\"]");

    // Image
    private static readonly Selector ImageBoxSelector = Selector.ParseSelector("> div[class=\"thread_image_box\"]");

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

    [DataMember(Name = "post_time")]
    public DateTimeOffset PostTime { get; set; }

    [DataMember(Name = "file")]
    public FoolFuukaFile? File { get; set; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(File))]
    public bool HasFile => File != null;

    [IgnoreDataMember]
    public ulong[]? RespondsTo {
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

    public FoolFuukaPost(HtmlElementNode node) {
        // The poster information of the post, id, name, subject, time, etc.
        var HeaderNode = node.Find(HeaderSelector) ??
            throw new ArgumentNullException("Could not find header node.");

        // Post ID
        var PostIdNode = HeaderNode.FirstOrDefault(PostIdSelector) ??
            throw new ArgumentNullException("Could not find post id node.");
        this.PostId = ulong.Parse(PostIdNode.Attributes["data-post"]!.Value);

        // Name, Tripcode, Capcode, ID?
        var PosterDataNode = HeaderNode.FirstOrDefault(PosterDataSelector) ??
            throw new ArgumentNullException("Could not find poster data node.");
        var PosterNameNode = PosterDataNode.FirstOrDefault(PosterNameSelector);
        if (PosterNameNode?.Text.IsNullEmptyWhitespace() == false) {
            var PosterEmailNode = PosterNameNode.Children.FirstOrDefault(DefaultSelectors.A.Href);
            if (PosterEmailNode != null) {
                this.PosterEmail = PosterEmailNode.Attributes["href"]!.Value.RemoveFromStart("mailto:", StringComparison.InvariantCultureIgnoreCase);
                this.PosterName = PosterEmailNode.Text;
            }
            else {
                this.PosterName = PosterNameNode.Text;
            }
        }
        else {
            this.PosterName = "Anonymous";
        }
        var PosterTripcodeNode = PosterDataNode.FirstOrDefault(PosterTripcodeSelector);
        if (PosterTripcodeNode?.Text.IsNullEmptyWhitespace() == false) {
            this.PosterTripcode = PosterTripcodeNode.Text;
        }
        var PosterCapcodeNode = PosterDataNode.FirstOrDefault(PosterCapcodeSelector);
        if (PosterCapcodeNode?.Text.IsNullEmptyWhitespace() == false) {
            this.PosterCapcode = PosterCapcodeNode.Text;
        }
        var PosterIdNode = PosterDataNode.FirstOrDefault(PosterIdSelector);
        if (PosterIdNode?.Text.IsNullEmptyWhitespace() == false) {
            this.PosterId = PosterIdNode.Text[3..];
        }

        // Subject
        var SubjectNode = HeaderNode.FirstOrDefault(SubjectSelector);
        if (SubjectNode != null) {
            this.Subject = SubjectNode.Text;
        }

        // Post time
        var PostTimeNode = HeaderNode.FirstOrDefault(PostTimeSelector) ??
            throw new ArgumentNullException("Could not find post time selector.");
        this.PostTime = ConvertTimestampToDateTime(PostTimeNode.Attributes["datetime"]!.Value!);

        // Message
        var MessageNode = node.FirstOrDefault(MessageSelector);
        if (MessageNode != null) {
            this.MessageBody = GetMessage(MessageNode);
        }

        // Image
        var ImageNode = node.FirstOrDefault(ImageBoxSelector);
        if (ImageNode != null) {
            File = new(ImageNode, this);
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
        return this.PostId == other.PostId;
    }

    public override int GetHashCode() => unchecked((int)(this.PostId & 0x7FFFFFFF)); // Negative-bit ignored.
}
