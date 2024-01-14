namespace murrty.controls;
using System;
using System.IO;
using System.Threading;
public class ThrottledStream : Stream {
    public long MaxBytesPerSecond {
        get => maxBytes;
        set {
            if (value < 1)
                throw new ArgumentOutOfRangeException($"Value {value} cannot be lower than 1 byte.");

            maxBytes = value;
        }
    }
    private long maxBytes;

    private long processed;
    private readonly System.Timers.Timer resetTimer;
    private readonly AutoResetEvent wh;
    private readonly Stream parent;

    private ThrottledStream() {
        wh = new(true);
        processed = 0;
        resetTimer = new() {
            Interval = 1000
        };
        resetTimer.Elapsed += (s, e) => {
            processed = 0;
            wh.Set();
        };
        resetTimer.Start();
    }
    public ThrottledStream(Stream ParentStream, long MaxBytesPerSecond) : this() {
        this.MaxBytesPerSecond = MaxBytesPerSecond;
        parent = ParentStream;
    }
    protected void ThrottleStream(long bytes) {
        try {
            processed += bytes;
            if (processed >= maxBytes)
                wh.WaitOne(1000);
        }
        catch { }
    }
    public override bool CanRead => parent.CanRead;
    public override bool CanSeek => parent.CanSeek;
    public override bool CanWrite => parent.CanWrite;
    public override long Length => parent.Length;
    public override long Position {
        get => parent.Position;
        set => parent.Position = value;
    }

    public override void Flush() => parent.Flush();
    public override void Close() {
        resetTimer.Stop();
        resetTimer.Close();
        base.Close();
    }
    protected override void Dispose(bool disposing) {
        resetTimer.Dispose();
        base.Dispose(disposing: disposing);
    }
    public override int Read(byte[] buffer, int offset, int count) {
        int read = parent.Read(buffer: buffer, offset: offset, count: count);
        ThrottleStream(count);
        return read;
    }
    public override long Seek(long offset, SeekOrigin origin) => parent.Seek(offset: offset, origin: origin);
    public override void SetLength(long value) => parent.SetLength(value: value);
    public override void Write(byte[] buffer, int offset, int count) {
        ThrottleStream(count);
        parent.Write(buffer: buffer, offset: offset, count: count);
    }
}
