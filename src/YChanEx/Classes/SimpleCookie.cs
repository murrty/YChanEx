namespace YChanEx;
using System.Net;
using System.Runtime.Serialization;
[DataContract]
public sealed class SimpleCookie {
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Value { get; set; }

    [DataMember]
    public string Path { get; set; }

    [DataMember]
    public string Domain { get; set; }

    public SimpleCookie(string name, string value) {
        this.Name = name;
        this.Value = value;
    }
    public SimpleCookie(string name, string value, string path) : this(name, value) {
        this.Path = path;
    }
    public SimpleCookie(string name, string value, string path, string domain) : this(name, value, path) {
        this.Domain = domain;
    }

    public static implicit operator Cookie(SimpleCookie cookie) {
        return new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain) {
            Expires = DateTime.MaxValue
        };
    }
    public static implicit operator SimpleCookie(Cookie cookie) {
        return new SimpleCookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
    }
}
