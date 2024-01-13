#nullable enable
namespace YChanEx.Posts;

using System.Runtime.Serialization;

[DataContract]
public class FoolFuukaBoard {
    [DataMember]
    public string? name { get; set; }

    [DataMember]
    public string? shortname { get; set; }
}
