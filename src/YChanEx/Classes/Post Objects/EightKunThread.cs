#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
internal sealed class EightKunThread {
    [DataMember(Name = "posts")]
    public EightKunPost[] posts { get; set; } = [ ];
}
