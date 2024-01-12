#nullable enable
namespace YChanEx.Posts;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
using static YChanEx.Parsers.U18Chan;
[DataContract]
[DebuggerDisplay("{PostId} @ {PostTime} | File = {File != null}, Message = {MessageBody != null}")]
internal sealed class U18ChanPost : IEquatable<U18ChanPost> {
    private static readonly Selector PostDetailsSelector = Selector.ParseSelector("div[class=\"PostDetails\"]");
    private static readonly Selector FileDetailsSelector = Selector.ParseSelector("div[class=\"FileDetails\"]");
    private static readonly Selector UserDetailsSelector = Selector.ParseSelector("div[class=\"UserDetails\"]");
    private static readonly Selector SubjectSelector = Selector.ParseSelector("span[class=\"Subject\"]");
    private static readonly Selector DisplayNameSelector = Selector.ParseSelector("span[class=\"UserName\"]");
    private static readonly Selector MessageSelector = Selector.ParseSelector("span[name]");
    private static readonly Selector PostTagsSelector = Selector.ParseSelector("span[id=\"TagBox0\"]");

    [DataMember(Name = "post_id")]
    public ulong PostId { get; set; }

    [DataMember(Name = "subject")]
    public string? Subject { get; set; }

    [DataMember(Name = "name")]
    public string? PosterName { get; set; }

    [DataMember(Name = "capcode")]
    public string? PosterCapcode { get; set; }

    [DataMember(Name = "email")]
    public string? PosterEmail { get; set; }

    [DataMember(Name = "body")]
    public string? MessageBody { get; set; }

    [DataMember(Name = "post_time")]
    public DateTimeOffset PostTime { get; set; }

    [DataMember(Name = "updated_at")]
    public DateTimeOffset UpdateTime { get; set; }

    [DataMember(Name = "tags")]
    public U18ChanTag[]? Tags { get; set; }

    [DataMember(Name = "file")]
    public U18ChanFile? File { get; set; }

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(File))]
    public bool HasFile => File != null;

    [IgnoreDataMember]
    [MemberNotNullWhen(true, nameof(Tags))]
    public bool HasTags => Tags?.Length > 0;

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

    public U18ChanPost(HtmlElementNode node, bool firstPost) {
        // First post node
        if (firstPost) {
            node = node.Children.FirstOrDefault(PostDetailsSelector) ??
                throw new ArgumentNullException("Could not find post details.");
        }

        // Post details
        var UserDetails = node.Children.FirstOrDefault(UserDetailsSelector) ??
            throw new ArgumentNullException("Could not find user details.");

        var Subject = UserDetails.Children.FirstOrDefault(SubjectSelector) ??
            throw new ArgumentNullException("Could not find subject.");
        this.Subject = Subject.Text.Trim();

        var Username = UserDetails.Children.FirstOrDefault(DisplayNameSelector) ??
            throw new ArgumentNullException("Could not find display name.");
        this.PosterName = Username.Text.Trim();

        // Capcode
        var PostMetadataNode = Username.NextNode;
        if (PostMetadataNode is HtmlElementNode) {
            this.PosterCapcode = PostMetadataNode.Text;
            PostMetadataNode = PostMetadataNode.NextNode;
        }

        // Date & time posted
        if (PostMetadataNode == null) {
            throw new ArgumentNullException("Could not find post metadata.");
        }
        string PostMetadata = PostMetadataNode.Text.ReplaceWhitespace(" ").Trim()[..^4];
        this.PostTime = ConvertTimestampToDateTime(PostMetadata);

        // Post ID
        var PostId = PostMetadataNode?.NextNode
            ?? throw new ArgumentNullException("Could not find post id.");
        this.PostId = uint.Parse(PostId.Text.Trim());

        // Poster email
        if (Username.Children.First() is HtmlElementNode InnerUsernameNode
        && InnerUsernameNode.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)) {
            // Weird way to extract the email.
            // It's 'protected', but still in the html source.
            var AltLinkNode = (HtmlElementNode?)InnerUsernameNode.ParentNode?.NextNode?.NextNode?.NextNode;

            if (AltLinkNode?.NextNode is HtmlElementNode Other
            && Other.Attributes.ContainsStartsWith("href", "javascript:EditPost(", StringComparison.OrdinalIgnoreCase)) {
                AltLinkNode = Other;
            }

            if (AltLinkNode?.Attributes.Contains("class", "AltLink", StringComparison.InvariantCultureIgnoreCase) == true
            && AltLinkNode.Attributes.TryGetValue("href", out var hrefNode)) {
                string[]? hrefData = hrefNode.Value?.Split(',');
                if (hrefData != null) {
                    this.PosterEmail = hrefData[2].Trim().Trim('\'');
                }
            }
        }

        // File?
        var FileDetails = (firstPost ? node.ParentNode!.Children : node.Children).FirstOrDefault(FileDetailsSelector);
        if (FileDetails != null) {
            // URL of the file
            var UrlNode = FileDetails.Children.FirstOrDefault(DefaultSelectors.A.Href) ??
                throw new ArgumentNullException("Could not find url data.");
            this.File = new(UrlNode, this, firstPost);
        }

        // Comment
        var PostMessage = (FileDetails?.ParentNode ?? node).Children.FirstOrDefault(MessageSelector);
        this.MessageBody = GetMessage(PostMessage);

        // Update time
        var UpdateTime = (HtmlElementNode?)PostMessage?.NextNode;
        if (UpdateTime?.TagName.Equals("br", StringComparison.OrdinalIgnoreCase) == true
        && UpdateTime.NextNode is HtmlElementNode node1) {
            UpdateTime = node1;
        }
        if (UpdateTime?.TagName.Equals("span", StringComparison.InvariantCultureIgnoreCase) == true
        && UpdateTime.Attributes.TryGetValue("style", out var style)
        && style.Value?.Equals("font-size: 75%;") == true
        && UpdateTime.Text.StartsWith("edited at", StringComparison.InvariantCultureIgnoreCase)) {
            this.UpdateTime = ConvertTimestampToDateTime(UpdateTime.Text[10..]);
        }

        // Tags
        var PostTags = node.Children.Find(PostTagsSelector).ToArray();
        if (PostTags.Length > 0) {
            this.Tags = new U18ChanTag[PostTags.Length];

            for (int i = 0; i < PostTags.Length; i++) {
                this.Tags[i] = new(PostTags[i], this);
            }
        }

        //if (firstPost) {
        //    // Post details
        //    var PostDetails = node.Children.FirstOrDefault(PostDetailsSelector) ??
        //        throw new ArgumentNullException("Could not find post details.");
        //    var UserDetails = PostDetails.Children.FirstOrDefault(UserDetailsSelector) ??
        //        throw new ArgumentNullException("Could not find user details.");

        //    var Subject = UserDetails.Children.FirstOrDefault(SubjectSelector) ??
        //        throw new ArgumentNullException("Could not find subject.");
        //    this.Subject = Subject.Text.Trim();

        //    var Username = UserDetails.Children.FirstOrDefault(DisplayNameSelector) ??
        //        throw new ArgumentNullException("Could not find display name.");
        //    this.PosterName = Username.Text.Trim();

        //    // Capcode
        //    HtmlNode? PostMetadataNode;
        //    if (Username.NextNode is HtmlElementNode CapCodeNode) {
        //        this.PosterCapCode = CapCodeNode.Text;
        //        PostMetadataNode = CapCodeNode.NextNode;
        //    }
        //    else {
        //        PostMetadataNode = Username.NextNode;
        //    }

        //    // Date & time posted
        //    if (PostMetadataNode == null) {
        //        throw new ArgumentNullException("Could not find post metadata.");
        //    }
        //    string PostMetadata = PostMetadataNode.Text.ReplaceWhitespace(" ").Trim()[..^4];
        //    this.PostTime = ConvertTimestampToDateTime(PostMetadata);

        //    // Post ID
        //    var PostId = PostMetadataNode?.NextNode
        //        ?? throw new ArgumentNullException("Could not find post id.");
        //    this.PostId = uint.Parse(PostId.Text.Trim());

        //    // Poster email
        //    if (Username.Children.First() is HtmlElementNode InnerUsernameNode
        //    && InnerUsernameNode.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)) {
        //        // Weird way to extract the email.
        //        // It's 'protected', but still in the html source.
        //        var AltLinkNode = (HtmlElementNode?)InnerUsernameNode.ParentNode?.NextNode?.NextNode?.NextNode;
        //        if (AltLinkNode?.Attributes.Contains("class", "AltLink", StringComparison.InvariantCultureIgnoreCase) == true
        //        && AltLinkNode.Attributes.TryGetValue("href", out var hrefNode)) {
        //            string[]? hrefData = hrefNode.Value?.Split(',');
        //            if (hrefData != null) {
        //                this.PosterEmail = hrefData[2].Trim().Trim('\'');
        //            }
        //        }
        //    }

        //    // Details of the file
        //    var FileDetails = node.Children.FirstOrDefault(FileDetailsSelector) ??
        //        throw new ArgumentNullException("Could not find file details.");

        //    // URL of the file
        //    var UrlNode = FileDetails.Children.FirstOrDefault(DefaultSelectors.A.Href) ??
        //        throw new ArgumentNullException("Could not find url data.");
        //    this.File = new(UrlNode, this);

        //    // Comment
        //    var PostMessage = PostDetails.Children.FirstOrDefault(MessageSelector);
        //    this.MessageBody = GetMessage(PostMessage);

        //    // Update time
        //    var UpdateTime = (HtmlElementNode?)PostMessage?.NextNode;
        //    if (UpdateTime?.TagName.Equals("span", StringComparison.InvariantCultureIgnoreCase) == true
        //    && UpdateTime.Attributes.TryGetValue("style", out var style)
        //    && style.Value?.Equals("font-size: 75%;") == true
        //    && UpdateTime.Text.StartsWith("edited at", StringComparison.InvariantCultureIgnoreCase)) {
        //        this.UpdateTime = ConvertTimestampToDateTime(UpdateTime.Text[10..]);
        //    }
        //}
        //else {
        //    // Post details
        //    var UserDetails = node.Children.FirstOrDefault(UserDetailsSelector) ??
        //        throw new ArgumentNullException("Could not find user details.");

        //    var Subject = UserDetails.Children.FirstOrDefault(SubjectSelector) ??
        //        throw new ArgumentNullException("Could not find subject.");
        //    this.Subject = Subject.Text.Trim();

        //    var Username = UserDetails.Children.FirstOrDefault(DisplayNameSelector) ??
        //        throw new ArgumentNullException("Could not find display name.");
        //    this.PosterName = Username.Text.Trim();

        //    // Date & time posted
        //    var PostMetadataNode = Username.NextNode
        //        ?? throw new ArgumentNullException("Could not find post metadata.");
        //    string PostMetadata = PostMetadataNode.Text.ReplaceWhitespace(" ").Trim()[..^4];
        //    this.PostTime = ConvertTimestampToDateTime(PostMetadata);

        //    // Post ID
        //    var PostId = PostMetadataNode?.NextNode
        //        ?? throw new ArgumentNullException("Could not find post id.");
        //    this.PostId = uint.Parse(PostId.Text.Trim());

        //    // Poster email
        //    if (Username.Children.First() is HtmlElementNode InnerUsernameNode
        //    && InnerUsernameNode.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)) {
        //        // Weird way to extract the email.
        //        // It's 'protected', but still in the html source.
        //        var AltLinkNode = (HtmlElementNode?)InnerUsernameNode.ParentNode?.NextNode?.NextNode?.NextNode;
        //        if (AltLinkNode?.Attributes.Contains("class", "AltLink", StringComparison.InvariantCultureIgnoreCase) == true
        //        && AltLinkNode.Attributes.TryGetValue("href", out var hrefNode)) {
        //            string[]? hrefData = hrefNode.Value?.Split(',');
        //            if (hrefData != null) {
        //                this.PosterEmail = hrefData[2].Trim().Trim('\'');
        //            }
        //        }
        //    }

        //    // File?
        //    var FileDetails = node.Children.FirstOrDefault(FileDetailsSelector);
        //    if (FileDetails != null) {
        //        // URL of the file
        //        var UrlNode = FileDetails.Children.FirstOrDefault(DefaultSelectors.A.Href) ??
        //            throw new ArgumentNullException("Could not find url data.");
        //        this.File = new(UrlNode, this);
        //    }

        //    // Comment
        //    var PostMessage = (FileDetails?.ParentNode ?? node).Children.FirstOrDefault(MessageSelector);
        //    this.MessageBody = GetMessage(PostMessage);

        //    // Update time
        //    var UpdateTime = (HtmlElementNode?)PostMessage?.NextNode;
        //    if (UpdateTime?.TagName.Equals("span", StringComparison.InvariantCultureIgnoreCase) == true
        //    && UpdateTime.Attributes.TryGetValue("style", out var style)
        //    && style.Value?.Equals("font-size: 75%;") == true
        //    && UpdateTime.Text.StartsWith("edited at", StringComparison.InvariantCultureIgnoreCase)) {
        //        this.UpdateTime = ConvertTimestampToDateTime(UpdateTime.Text[10..]);
        //    }
        //}
    }

    public override bool Equals(object obj) => obj is U18ChanPost other && this.Equals(other);
    public bool Equals(U18ChanPost other) {
        if (other is null) {
            return this is null;
        }
        if (this is null) {
            return false;
        }
        return this.PostId == other.PostId;
    }

    public override int GetHashCode() => unchecked((int)(ulong.MaxValue & 0x7FFFFFFF)); // Negative-bit ignored.
}
