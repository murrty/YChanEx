#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
public class StreamParser : ITextParser<Stream> {
    private Stream _stream;

    public int Index {
        get {
            return (int)_stream.Position;
        }
        set {
            if (value < 0) {
                _stream.Position = 0;
            }
            else if (value > _stream.Length) {
                _stream.Position = _stream.Length - 1;
            }
            else {
                _stream.Position = value;
            }
        }
    }
    public bool EndOfText => _stream.Position >= _stream.Length;
    public Stream BackingValue => _stream;

    public StreamParser(Stream? stream) {
        Reset(stream);
    }

    [MemberNotNull(nameof(_stream))]
    public void Reset(Stream? stream) {
        this._stream = Stream.Null;
        if (stream?.CanSeek == true && stream.CanRead) {
            this._stream = stream;
            Index = 0;
        }
    }

    public char Peek() {
        var peek = _stream.ReadByte();
        _stream.Position--;
        if (peek == -1) {
            return HtmlParser.NullChar;
        }
        return (char)peek;
    }

    public char Peek(int count) {
        int currentPos = Index;

        _stream.Seek(count, SeekOrigin.Current);
        var peek = _stream.ReadByte();
        _stream.Seek(currentPos, SeekOrigin.Begin);
        if (peek == -1) {
            return HtmlParser.NullChar;
        }
        return (char)peek;
    }

    public char Get() {
        var peek = _stream.ReadByte();
        if (peek == -1) {
            return HtmlParser.NullChar;
        }
        return (char)peek;
    }

    public void Next() {
        if (!EndOfText) {
            _stream.Position++;
        }
    }

    public void SkipWhile(Func<char, bool> predicate) {
        while (!EndOfText && predicate((char)_stream.ReadByte())) ;
        Index--;
    }

    public void SkipWhiteSpace() {
        SkipWhile(char.IsWhiteSpace);
    }

    public bool SkipTo(params char[] chars) {
        while (!EndOfText) {
            if (chars.Contains(Get())) {
                Index--;
                return true;
            }
        }
        return false;
    }

    public bool SkipTo(string s, bool includeToken = false) {
        int sLength = s.Length;
        byte[] buffer = new byte[sLength];

        while (!EndOfText) {
            int pos = Index;
            _stream.Read(buffer, 0, sLength);
            Index = pos;

            string sr = Encoding.UTF8.GetString(buffer);
            if (s.Equals(sr)) {
                if (!includeToken) {
                    Index = pos;
                }
                return true;
            }

            Index = pos + sLength;
        }

        return false;
    }

    public bool SkipTo(string s, StringComparison comparison, bool includeToken = false) {
        int sLength = s.Length;
        byte[] buffer = new byte[sLength];

        while (!EndOfText) {
            int pos = Index;
            _stream.Read(buffer, 0, sLength);
            Index = pos;

            string sr = Encoding.UTF8.GetString(buffer);
            if (s.Equals(sr, comparison)) {
                if (!includeToken) {
                    Index = pos;
                }
                return true;
            }

            Index = pos + 1;
        }

        return false;
    }

    public string ParseCharacter() {
        char c = Get();
        return c.ToString();
    }

    public string ParseWhile(Func<char, bool> predicate) {
        int start = Index;
        SkipWhile(predicate);
        return Extract(start, Index);
    }

    public string ParseQuotedText() {
        StringBuilder sb = new();
        char quote = Get();

        while (!EndOfText) {
            sb.Append(ParseTo(quote));
            Next();
            if (Peek() == quote) {
                sb.Append(quote);
                Next();
            }
            else {
                break;
            }
        }

        return sb.ToString();
    }

    public string ParseTo(params char[] chars) {
        int start = Index;
        SkipTo(chars);
        if (EndOfText) {
            return string.Empty;
        }
        return Extract(start, Index);
    }

    public string ParseTo(string s, StringComparison comparison, bool includeToken = false) {
        int start = Index;
        SkipTo(s, comparison, includeToken);
        return Extract(start, Index);
    }

    public bool MatchesCurrentPosition(string? s, StringComparison comparison) {
        if (string.IsNullOrWhiteSpace(s) || EndOfText) {
            return false;
        }
        int index = Index;
        byte[] buffer = new byte[s!.Length];
        _stream.Read(buffer, 0, s.Length);
        _stream.Position = index;
        string str = Encoding.UTF8.GetString(buffer);
        return string.Compare(str, 0, s, 0, s.Length, comparison) == 0;
    }

    public string Extract(int start, int end) {
        int pos = Index;

        Index = start;
        byte[] buffer = new byte[end - start];
        _stream.Read(buffer, 0, buffer.Length);
        Index = pos;

        return Encoding.UTF8.GetString(buffer);
    }

    public string Substring(int start, int length) {
        int pos = Index;

        Index = start;
        byte[] buffer = new byte[length];
        _stream.Read(buffer, 0, length);
        Index = pos;

        return Encoding.UTF8.GetString(buffer);
    }
}
