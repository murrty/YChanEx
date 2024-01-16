namespace YChanEx;
using System.Runtime.Serialization;
[DataContract]
public sealed class PreviousThread {
    [IgnoreDataMember]
    public ChanType Type { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "name")]
    public string ShortName { get; set; }

    public PreviousThread(string Url, string ShortName) {
        this.Url = Url;
        this.ShortName = ShortName;
    }

    public PreviousThread(ChanType Type, string Url, string ShortName) : this(Url, ShortName) {
        this.Type = Type;
    }

    [OnDeserialized]
    void D(StreamingContext ctx) {
        this.Url ??= string.Empty;
        this.ShortName ??= string.Empty;
    }
}

public sealed class PreviousThreadCollection : List<PreviousThread> {
    public bool Contains(string Url) {
        for (int i = 0; i < this.Count; i++) {
            if (this[i].Url.Equals(Url, StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
    public void Remove(string Url) {
        for (int i = 0; i < this.Count; i++) {
            if (this[i].Url.Equals(Url, StringComparison.InvariantCultureIgnoreCase)) {
                this.RemoveAt(i--);
            }
        }
    }
    public bool Update(string Url, string ShortName) {
        for (int i = 0; i < this.Count; i++) {
            if (this[i].Url.Equals(Url, StringComparison.InvariantCultureIgnoreCase)) {
                this[i].ShortName = ShortName;
                return true;
            }
        }
        return false;
    }
}
