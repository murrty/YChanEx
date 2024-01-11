namespace YChanEx;
internal struct DateTimeCounter {
    public DateTime Start;
    public DateTime End;
    public TimeSpan Duration;
    public readonly TimeSpan Elapsed => DateTime.Now - Start;
    public DateTimeCounter() {
        this.Start = DateTime.Now;
    }
    public TimeSpan Stop() {
        this.End = DateTime.Now;
        this.Duration = this.End - this.Start;
        return this.Duration;
    }
}
