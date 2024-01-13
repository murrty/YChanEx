#nullable enable
namespace YChanEx.Parsers;
using SoftCircuits.HtmlMonkey;
using YChanEx.Posts;
internal static class FoolFuuka {
    public static FoolFuukaPost[]? Deserialize(string json) {
        var Deserialize = json.JsonDeserialize<Dictionary<ulong, FoolFuukaThread>>()
            .FirstOrDefault().Value;

        if (Deserialize?.op == null) {
            return null;
        }

        return [Deserialize.op, .. Deserialize.posts?.Select(x => x.Value)];
    }
    public static async Task<FoolFuukaPost[]?> DeserializeAsync(string json) {
        return await Task.Run<FoolFuukaPost[]?>(() => {
            var Deserialize = json.JsonDeserialize<Dictionary<ulong, FoolFuukaThread>>()
                .FirstOrDefault().Value;

            if (Deserialize?.op == null) {
                return null;
            }

            return [ Deserialize.op, .. Deserialize.posts?.Select(x => x.Value) ];
        });
    }

    public static DateTimeOffset GetPostTime(long timestamp) {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp);
    }
    internal static string? GetMessage(string? message) {
        if (message.IsNullEmptyWhitespace()) {
            return null;
        }
        // TODO: translate message html
        return message;
    }
    internal static string? GetMessage(HtmlElementNode? node) {
        if (node?.Children.Count > 0) {
            if (node.Children[^1] is HtmlElementNode cnode && cnode.TagName.Equals("br", StringComparison.OrdinalIgnoreCase)) {
                node.Children.RemoveAt(node.Children.Count - 1);
            }
            for (int i = 0; i < node.Children.Count; i++) {
                if (node.Children[i] is HtmlElementNode element) {
                    CleanMessageNode(element);
                }
            }
            if (!node.InnerHtml.IsNullEmptyWhitespace()) {
                return node.InnerHtml;
            }
        }
        return null;
    }
    private static void CleanMessageNode(HtmlElementNode node) {
        // Clean quotes, replies, and spoilers
        if (node.TagName.Equals("span", StringComparison.OrdinalIgnoreCase)) {
            // Replies and quotes are both in the same greentext span.
            // This needs to be fixed.
            if (node.Attributes.Contains("class", "greentext", StringComparison.InvariantCultureIgnoreCase)) {
                if (node.Children.Count > 0) {
                    if (node.Children[0] is HtmlElementNode c) {
                        node = c;
                        if (node.Attributes.TryGetValue("data-post", out var postId)) {
                            node.Attributes.Clear();
                            node.Attributes.Add("class", "quotelink");
                            node.Attributes.Add("href", "#p" + postId.Value);
                        }
                    }
                    else {
                        node.Attributes.Remove("class");
                        node.Attributes.Add("class", "quote");
                    }
                }
            }

            // Clean spoilers
            else if (node.Attributes.ContainsWithValue("class", "spoiler", StringComparison.InvariantCultureIgnoreCase)) {
                node.TagName = "s";
                node.Attributes.Clear();
            }
        }

        // Remove links (safety)
        else if (node.TagName.Equals("a", StringComparison.OrdinalIgnoreCase)
        && node.Attributes.ContainsWithAnyValue("value", StringComparison.InvariantCultureIgnoreCase)
        && node.Attributes.ContainsWithValue("rel", "nofollow", StringComparison.InvariantCultureIgnoreCase)) {

        }

        if (node.Children.Count > 0) {
            for (int i = 0; i < node.Children.Count; i++) {
                if (node.Children[i] is HtmlElementNode element) {
                    CleanMessageNode(element);
                }
            }
        }
    }
}
