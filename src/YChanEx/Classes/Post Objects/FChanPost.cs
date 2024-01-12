#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.FChan;
[DataContract]
[DebuggerDisplay("{PostId} @ {PostTime} | File = {HasFile}, Message = {MessageBody != null}")]
internal sealed class FChanPost {
    // Regex
    private static readonly Regex RepliesRegex = new("href=\"/[a-zA-Z0-9]+/res/\\d+\\.html#\\d+\"", RegexOptions.IgnoreCase);

    // Continue researching FCHAN.
    private static readonly Selector SubjectSelector = Selector.ParseSelector("span[class=filetitle]");
    private static readonly Selector ThreadSelector = Selector.ParseSelector("> div[id:=\"thread\\d+\"]");
    internal static readonly Selector ReplyPostSelector = Selector.ParseSelector("td[class?=\"reply replyhide\"]");
    internal static readonly Selector RegexReplyPostSelector = Selector.ParseSelector("td[id:=\"reply\\d+\"]");

    private static readonly Selector PostIdSelector = Selector.ParseSelector("span[class=reflink]");
    private static readonly Selector PosterNameSelector = Selector.ParseSelector("span[class?=\"postername cpname\"]");
    private static readonly Selector PosterTripcodeSelector = Selector.ParseSelector("span[class?=\"postertrip\"]");
    private static readonly Selector PosterSubjectSelector = Selector.ParseSelector("span[class=replytitle]");
    private static readonly Selector FileMetadataSelector = Selector.ParseSelector("> span[class=filesize]");
    private static readonly Selector MessageSelector = Selector.ParseSelector("blockquote[class=\"com_hide\"]");

    [DataMember(Name = "post_id")]
    public ulong PostId { get; set; }

    [DataMember(Name = "subject")]
    public string? Subject { get; set; }

    [DataMember(Name = "name")]
    public string? PosterName { get; set; }

    [DataMember(Name = "tripcode")]
    public string? PosterTripcode { get; set; }

    [DataMember(Name = "mailto")]
    public string? PosterEmail { get; set; }

    [DataMember(Name = "body")]
    public string? MessageBody { get; set; }

    [DataMember(Name = "file")]
    public FChanFile? File { get; set; }

    [DataMember(Name = "post_time")]
    public DateTimeOffset PostTime { get; set; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(File))]
    public bool HasFile => File != null;

    [IgnoreDataMember]
    public ulong[]? RespondsTo {
        get {
            if (MessageBody.IsNullEmptyWhitespace()) {
                return null;
            }

            var Matches = RepliesRegex.Matches(MessageBody);
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

    public FChanPost(HtmlElementNode PostNode, HtmlElementNode ThreadNode, bool ChildrenNested, bool FirstPost) {
        HtmlElementNode? InnerPostNode;

        // Whether the replies are nested in the "thread#" node.
        if (FirstPost) {
            InnerPostNode = ChildrenNested ? ThreadNode.Children.FirstOrDefault(ThreadSelector) :
                PostNode.Children.FirstOrDefault(DefaultSelectors.td);

            HtmlElementNode? SubjectNode = ThreadNode.Children.FirstOrDefault(SubjectSelector) ??
                    throw new ArgumentNullException("Could not find subject node.");

            this.Subject = SubjectNode.Text;
        }
        else {
            InnerPostNode = PostNode.Children.FirstOrDefault(ReplyPostSelector);
        }

        // Checking the inner post node, if it's null it may still be fixable (Old threads may have this issue).
        if (InnerPostNode == null) {
            if (FirstPost) {
                if (ThreadNode.Children.FirstOrDefault(FileMetadataSelector) != null) {
                    InnerPostNode = ThreadNode;
                }
            }
            else {
                InnerPostNode = PostNode.Children.FirstOrDefault(RegexReplyPostSelector);
            }

            if (InnerPostNode == null) {
                throw new ArgumentNullException("Could not find inner post node.");
            }
        }

        var PostIdNode = InnerPostNode.Children.FirstOrDefault(PostIdSelector) ??
            throw new ArgumentNullException("Could not find post id node.");
        if (!PostIdNode.Text.StartsWith("No.")) {
            throw new ArgumentException("Invalid Post ID node.");
        }
        this.PostId = ulong.Parse(PostIdNode.Text[3..]);

        var PosterInfoNode = (HtmlElementNode?)PostIdNode.PrevNode ??
            throw new ArgumentNullException("Could not find post time node.");

        var PosterNameNode = PosterInfoNode.Children.FirstOrDefault(PosterNameSelector);
        if (PosterNameNode?.Text.IsNullEmptyWhitespace() == false) {
            var PosterLinkNode = PosterNameNode.Children.FirstOrDefault(DefaultSelectors.A.HrefValue);
            if (PosterLinkNode != null) {
                this.PosterName = PosterLinkNode.Text;
                var Href = PosterLinkNode.Attributes["href"]!;
                if (Href.Value!.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) {
                    this.PosterEmail = Href.Value[7..];
                }
            }
            else {
                this.PosterName = PosterNameNode.Text;
            }
        }
        else {
            this.PosterName = "Anonymous";
        }
        this.PosterName = PosterNameNode != null && !string.IsNullOrWhiteSpace(PosterNameNode.Text) ? PosterNameNode.Text : "Anonymous";

        var PosterTripcodeNode = PosterInfoNode.Children.FirstOrDefault(PosterTripcodeSelector);
        if (PosterTripcodeNode != null) {
            this.PosterTripcode = PosterTripcodeNode.Text;
        }

        if (!FirstPost) {
            var PosterSubjectNode = PosterInfoNode.Children.FirstOrDefault(PosterSubjectSelector);
            if (PosterSubjectNode != null) {
                this.Subject = PosterSubjectNode.Text;
            }
        }

        this.PostTime = ConvertTimestampToDateTime(PosterInfoNode.Children[^1].Text);

        var MetadataNode = InnerPostNode.Children.FirstOrDefault(FileMetadataSelector);
        if (MetadataNode != null) {
            this.File = new(MetadataNode, InnerPostNode, this);
        }

        var MessageNode = InnerPostNode.Children.FirstOrDefault(MessageSelector);
        if (MessageNode?.Children.Count > 0) {
            this.MessageBody = GetMessage(MessageNode);
        }
    }
}
