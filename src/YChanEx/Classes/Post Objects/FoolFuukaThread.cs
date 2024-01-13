#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
public sealed class FoolFuukaThread {
    // ... /_/api/chan/thread?board={0}&num={1}
    [DataMember]
    public FoolFuukaPost? op { get; set; }

    [DataMember]
    public Dictionary<ulong, FoolFuukaPost>? posts { get; set; }
}
