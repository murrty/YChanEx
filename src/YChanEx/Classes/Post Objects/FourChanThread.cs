#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
internal sealed class FourChanThread {
    [DataMember(Name = "posts")]
    public FourChanPost[] posts { get; set; } = [ ];
}
