#nullable enable
namespace YChanEx;
using System.Runtime.Serialization;
using System.Windows.Forms;
[DataContract]
public sealed class PreviousThread {
    [IgnoreDataMember]
    public ChanType Type { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }

    [DataMember(Name = "name")]
    public string ShortName { get; set; }

    [IgnoreDataMember]
    public TreeNode? Node { get; set; }

    [IgnoreDataMember]
    public bool HasValue => !Url.IsNullEmptyWhitespace();

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
        this.ShortName ??= this.Url ?? string.Empty;
    }
}

public sealed class PreviousThreadCollection : List<PreviousThread> {
    public bool Contains(string Url) {
        for (int i = 0; i < this.Count; i++) {
            var Item = this[i];
            if (Item.Url.Equals(Url, StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
    public int IndexOf(string Url) {
        for (int i = 0; i < this.Count; i++) {
            var Item = this[i];
            if (Item.Url.Equals(Url)) {
                return i;
            }
        }
        return -1;
    }
    public void Remove(string Url) {
        for (int i = 0; i < this.Count; i++) {
            var Item = this[i];
            if (Item.Url.Equals(Url, StringComparison.InvariantCultureIgnoreCase)) {
                this.RemoveAt(i--);
            }
        }
    }
}
