#nullable enable
namespace YChanEx.Posts;
using System.Runtime.Serialization;
[DataContract]
internal sealed class EightKunBoard {
    [DataMember(Name = "uri")]
    public string? uri { get; set; }

    [DataMember(Name = "title")]
    public string? title { get; set; }

    [DataMember(Name = "subtitle")]
    public string? subtitle { get; set; }

    //[DataMember(Name = "indexed")]
    //public int indexed { get; set; }

    [DataMember(Name = "sfw")]
    public int sfw { get; set; }

    //[DataMember(Name = "posts_total")]
    //public int posts_total { get; set; }

    //[DataMember(Name = "time")]
    //public string? time { get; set; }

    //[DataMember(Name = "weight")]
    //public double weight { get; set; }

    //[DataMember(Name = "locale")]
    //public string? locale { get; set; }

    //[DataMember(Name = "tags")]
    //public string[]? tags { get; set; }

    //[DataMember(Name = "max")]
    //public uint max { get; set; }

    //[DataMember(Name = "active")]
    //public int active { get; set; }

    //[DataMember(Name = "pph")]
    //public int pph { get; set; }

    //[DataMember(Name = "ppd")]
    //public int ppd { get; set; }

    //[DataMember(Name = "pph_average")]
    //public double pph_average { get; set; }
}
