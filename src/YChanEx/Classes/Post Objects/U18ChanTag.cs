#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
using SoftCircuits.HtmlMonkey;
[DataContract]
internal sealed class U18ChanTag {
    [IgnoreDataMember]
    public U18ChanPost Parent { get; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "count")]
    public int Count { get; set; }

    public U18ChanTag(HtmlElementNode Node, U18ChanPost Parent) {
        this.Parent = Parent;
        this.Name = Node.Children[0].Text;
        if (Node.Children.Count > 1) {
            this.Count = int.Parse(Node.Children[1].Text[1..^1]);
        }
    }
}
