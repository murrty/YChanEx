#nullable enable
namespace YChanEx;
using System.Runtime.Serialization;
[DataContract]
public sealed class DownloadHistory {
    [DataContract]
    private class DownloadHistoryOld {
        [DataMember(Name = "4chan", IsRequired = false, Order = 0)]
        public List<string> FourChanHistory { get; set; } = [];
        [DataMember(Name = "420chan", IsRequired = false, Order = 1)]
        public List<string> FourTwentyChanHistory { get; set; } = [];
        [DataMember(Name = "7chan", IsRequired = false, Order = 2)]
        public List<string> SevenChanHistory { get; set; } = [];
        [DataMember(Name = "8chan", IsRequired = false, Order = 3)]
        public List<string> EightChanHistory { get; set; } = [];
        [DataMember(Name = "8kun", IsRequired = false, Order = 4)]
        public List<string> EightKunHistory { get; set; } = [];
        [DataMember(Name = "fchan", IsRequired = false, Order = 5)]
        public List<string> FchanHistory { get; set; } = [];
        [DataMember(Name = "u18chan", IsRequired = false, Order = 6)]
        public List<string> u18chanHistory { get; set; } = [];
        [DataMember(Name = "foolfuuka", IsRequired = false, Order = 7)]
        public List<string> FoolFuukaHistory { get; set; } = [];
        [OnDeserialized]
        void Deserialize(StreamingContext ctx) {
            FourChanHistory ??= [];
            FourTwentyChanHistory ??= [];
            SevenChanHistory ??= [];
            EightChanHistory ??= [];
            EightKunHistory ??= [];
            FchanHistory ??= [];
            u18chanHistory ??= [];
            FoolFuukaHistory ??= [];
        }
    }

    [IgnoreDataMember]
    public static DownloadHistory Data = new();
    [IgnoreDataMember]
    private static readonly string HistoryFile = Program.SavedThreadsPath + System.IO.Path.DirectorySeparatorChar + "History.json";
    [IgnoreDataMember]
    private bool HistoryModified;

    [DataMember(Name = "4chan", IsRequired = false, Order = 0)]
    public PreviousThreadCollection FourChanHistory { get; set; } = [];
    [DataMember(Name = "420chan", IsRequired = false, Order = 1)]
    public PreviousThreadCollection FourTwentyChanHistory { get; set; } = [];
    [DataMember(Name = "7chan", IsRequired = false, Order = 2)]
    public PreviousThreadCollection SevenChanHistory { get; set; } = [];
    [DataMember(Name = "8chan", IsRequired = false, Order = 3)]
    public PreviousThreadCollection EightChanHistory { get; set; } = [];
    [DataMember(Name = "8kun", IsRequired = false, Order = 4)]
    public PreviousThreadCollection EightKunHistory { get; set; } = [];
    [DataMember(Name = "fchan", IsRequired = false, Order = 5)]
    public PreviousThreadCollection FchanHistory { get; set; } = [];
    [DataMember(Name = "u18chan", IsRequired = false, Order = 6)]
    public PreviousThreadCollection u18chanHistory { get; set; } = [];
    [DataMember(Name = "foolfuuka", IsRequired = false, Order = 7)]
    public PreviousThreadCollection FoolFuukaHistory { get; set; } = [];

    [IgnoreDataMember]
    public static int Count =>
        Data.FourChanHistory.Count + Data.FourTwentyChanHistory.Count +
        Data.SevenChanHistory.Count + Data.EightChanHistory.Count +
        Data.EightKunHistory.Count + Data.FchanHistory.Count +
        Data.u18chanHistory.Count + Data.FoolFuukaHistory.Count;

    [IgnoreDataMember]
    public static PreviousThread[] History {
        get {
            return [
                .. Data.FourChanHistory,
                .. Data.FourTwentyChanHistory,
                .. Data.SevenChanHistory,
                .. Data.EightChanHistory,
                .. Data.EightKunHistory,
                .. Data.FchanHistory,
                .. Data.u18chanHistory,
                .. Data.FoolFuukaHistory
            ];
        }
    }

    public static void Add(ChanType Chan, string URL, string ThreadName) {
        switch (Chan) {
            case ChanType.FourChan: {
                if (!Data.FourChanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.FourChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.FourTwentyChan: {
                if (!Data.FourTwentyChanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.FourTwentyChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.SevenChan: {
                if (!Data.SevenChanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.SevenChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.EightChan: {
                if (!Data.EightChanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.EightChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.EightKun: {
                if (!Data.EightKunHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.EightKun, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.fchan: {
                if (!Data.FchanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.fchan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.u18chan: {
                if (!Data.u18chanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.u18chan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.FoolFuuka: {
                if (!Data.FoolFuukaHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.FoolFuuka, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
        }
    }

    public static void AddOrUpdate(ChanType Chan, string URL, string ThreadName) {
        switch (Chan) {
            case ChanType.FourChan: {
                if (!Data.FourChanHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.FourChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.FourTwentyChan: {
                if (!Data.FourTwentyChanHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.FourTwentyChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.SevenChan: {
                if (!Data.SevenChanHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.SevenChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.EightChan: {
                if (!Data.EightChanHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.EightChan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.EightKun: {
                if (!Data.EightKunHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.EightKun, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.fchan: {
                if (!Data.FchanHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.fchan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.u18chan: {
                if (!Data.u18chanHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.u18chan, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
            case ChanType.FoolFuuka: {
                if (!Data.FoolFuukaHistory.Update(URL, ThreadName)) {
                    Data.FourChanHistory.Add(new PreviousThread(ChanType.FoolFuuka, URL, ThreadName));
                    Data.HistoryModified = true;
                }
            } break;
        }
    }

    public static bool Contains(ChanType Chan, string URL) {
        return Chan switch {
            ChanType.FourChan => Data.FourChanHistory.Contains(URL),
            ChanType.FourTwentyChan => Data.FourTwentyChanHistory.Contains(URL),
            ChanType.SevenChan => Data.SevenChanHistory.Contains(URL),
            ChanType.EightChan => Data.EightChanHistory.Contains(URL),
            ChanType.EightKun => Data.EightKunHistory.Contains(URL),
            ChanType.fchan => Data.FchanHistory.Contains(URL),
            ChanType.u18chan => Data.u18chanHistory.Contains(URL),
            ChanType.FoolFuuka => Data.FoolFuukaHistory.Contains(URL),
            _ => throw new Exception($"Invalid chan type {Chan}")
        };
    }

    public static void Remove(string URL) {
        if (Data.FourChanHistory.Contains(URL)) {
            Data.FourChanHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.FourTwentyChanHistory.Contains(URL)) {
            Data.FourTwentyChanHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.SevenChanHistory.Contains(URL)) {
            Data.SevenChanHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.EightChanHistory.Contains(URL)) {
            Data.EightChanHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.EightKunHistory.Contains(URL)) {
            Data.EightKunHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.FchanHistory.Contains(URL)) {
            Data.FchanHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.u18chanHistory.Contains(URL)) {
            Data.u18chanHistory.Remove(URL);
            Data.HistoryModified = true;
        }
        else if (Data.FoolFuukaHistory.Contains(URL)) {
            Data.FoolFuukaHistory.Remove(URL);
            Data.HistoryModified = true;
        }
    }

    public static void Save() {
        if (Data.HistoryModified) {
            if (!System.IO.Directory.Exists(Program.SavedThreadsPath))
                System.IO.Directory.CreateDirectory(Program.SavedThreadsPath);

            System.IO.File.WriteAllText(HistoryFile, Data.JsonSerialize());
            Data.HistoryModified = false;
        }
    }

    public static void Load() {
        if (System.IO.File.Exists(HistoryFile)) {
            using var Stream = new System.IO.FileStream(HistoryFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            try {
                Data = Stream.JsonDeserialize<DownloadHistory>() ?? new();
            }
            catch {
                Data = new();
                try {
                    Stream.Seek(0, System.IO.SeekOrigin.Begin);
                    var OldData = Stream.JsonDeserialize<DownloadHistoryOld>();
                    if (OldData != null) {
                        for (int i = 0; i < OldData.FourChanHistory.Count; i++) {
                            string url = OldData.FourChanHistory[i];
                            Data.FourChanHistory.Add(new PreviousThread(ChanType.FourChan, url, url));
                        }
                        for (int i = 0; i < OldData.FourTwentyChanHistory.Count; i++) {
                            string url = OldData.FourTwentyChanHistory[i];
                            Data.FourTwentyChanHistory.Add(new PreviousThread(ChanType.FourTwentyChan, url, url));
                        }
                        for (int i = 0; i < OldData.SevenChanHistory.Count; i++) {
                            string url = OldData.SevenChanHistory[i];
                            Data.SevenChanHistory.Add(new PreviousThread(ChanType.SevenChan, url, url));
                        }
                        for (int i = 0; i < OldData.EightChanHistory.Count; i++) {
                            string url = OldData.EightChanHistory[i];
                            Data.EightChanHistory.Add(new PreviousThread(ChanType.EightChan, url, url));
                        }
                        for (int i = 0; i < OldData.EightKunHistory.Count; i++) {
                            string url = OldData.EightKunHistory[i];
                            Data.EightKunHistory.Add(new PreviousThread(ChanType.EightKun, url, url));
                        }
                        for (int i = 0; i < OldData.FchanHistory.Count; i++) {
                            string url = OldData.FchanHistory[i];
                            Data.FchanHistory.Add(new PreviousThread(ChanType.fchan, url, url));
                        }
                        for (int i = 0; i < OldData.u18chanHistory.Count; i++) {
                            string url = OldData.u18chanHistory[i];
                            Data.u18chanHistory.Add(new PreviousThread(ChanType.u18chan, url, url));
                        }
                        for (int i = 0; i < OldData.FoolFuukaHistory.Count; i++) {
                            string url = OldData.FoolFuukaHistory[i];
                            Data.FoolFuukaHistory.Add(new PreviousThread(ChanType.FoolFuuka, url, url));
                        }
                        Data.HistoryModified = true;
                        Save();
                    }
                }
                catch { }
            }
        }
    }

    public static void Clear() {
        Data.FourChanHistory.Clear();
        Data.FourTwentyChanHistory.Clear();
        Data.SevenChanHistory.Clear();
        Data.EightChanHistory.Clear();
        Data.EightKunHistory.Clear();
        Data.FchanHistory.Clear();
        Data.u18chanHistory.Clear();
        Data.FoolFuukaHistory.Clear();
    }

    [OnDeserialized]
    void Deserialize(StreamingContext ctx) {
        FourChanHistory ??= [];
        FourTwentyChanHistory ??= [];
        SevenChanHistory ??= [];
        EightChanHistory ??= [];
        EightKunHistory ??= [];
        FchanHistory ??= [];
        u18chanHistory ??= [];
        FoolFuukaHistory ??= [];
    }
}