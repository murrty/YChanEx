#nullable enable
namespace YChanEx;
using System.Runtime.Serialization;
using System.Windows.Forms;
using YChanEx.Parsers;
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

    public static void AddOrUpdate(ChanType Chan, string URL, string ThreadName, IMainFom MainForm) {
        if (!General.SaveThreadHistory) {
            return;
        }

        switch (Chan) {
            case ChanType.FourChan: {
                CheckItem(Data.FourChanHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.FourTwentyChan: {
                CheckItem(Data.FourTwentyChanHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.SevenChan: {
                CheckItem(Data.SevenChanHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.EightChan: {
                CheckItem(Data.EightChanHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.EightKun: {
                CheckItem(Data.EightKunHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.fchan: {
                CheckItem(Data.FchanHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.u18chan: {
                CheckItem(Data.u18chanHistory, Chan, URL, ThreadName, MainForm);
            } break;
            case ChanType.FoolFuuka: {
                CheckItem(Data.FoolFuukaHistory, Chan, URL, ThreadName, MainForm);
            } break;
        }
        Save();
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
            if (!System.IO.Directory.Exists(Program.SavedThreadsPath)) {
                System.IO.Directory.CreateDirectory(Program.SavedThreadsPath);
            }

            System.IO.File.WriteAllText(HistoryFile, Data.JsonSerialize());
            Data.HistoryModified = false;
        }
    }

    private static void Save(System.IO.FileStream fs) {
        fs.Position = 0;
        Data.JsonSerialize(fs);
    }

    public static void Load() {
        if (System.IO.File.Exists(HistoryFile)) {
            using var Stream = new System.IO.FileStream(HistoryFile, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read);
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
                            Data.FourChanHistory.Add(new PreviousThread(url, FourChan.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.FourTwentyChanHistory.Count; i++) {
                            string url = OldData.FourTwentyChanHistory[i];
                            Data.FourTwentyChanHistory.Add(new PreviousThread(url, FourTwentyChan.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.SevenChanHistory.Count; i++) {
                            string url = OldData.SevenChanHistory[i];
                            Data.SevenChanHistory.Add(new PreviousThread(url, SevenChan.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.EightChanHistory.Count; i++) {
                            string url = OldData.EightChanHistory[i];
                            Data.EightChanHistory.Add(new PreviousThread(url, EightChan.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.EightKunHistory.Count; i++) {
                            string url = OldData.EightKunHistory[i];
                            Data.EightKunHistory.Add(new PreviousThread(url, EightKun.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.FchanHistory.Count; i++) {
                            string url = OldData.FchanHistory[i];
                            Data.FchanHistory.Add(new PreviousThread(url, FChan.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.u18chanHistory.Count; i++) {
                            string url = OldData.u18chanHistory[i];
                            Data.u18chanHistory.Add(new PreviousThread(url, U18Chan.GetOldHistoryName(url)));
                        }
                        for (int i = 0; i < OldData.FoolFuukaHistory.Count; i++) {
                            string url = OldData.FoolFuukaHistory[i];
                            Data.FoolFuukaHistory.Add(new PreviousThread(url, FoolFuuka.GetOldHistoryName(url)));
                        }
                        Data.HistoryModified = true;
                        Save(Stream);
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

    private static void CheckItem(PreviousThreadCollection Collection, ChanType Chan, string URL, string ThreadName, IMainFom MainForm) {
        int Index = Collection.IndexOf(URL);
        if (Index == -1) {
            TreeNode NewNode = new(ThreadName) { Name = URL, };
            PreviousThread NewHistory = new(Chan, URL, ThreadName) { Node = NewNode, };
            Collection.Add(NewHistory);
            MainForm.AddToHistory(NewHistory);
            Data.HistoryModified = true;
        }
        else {
            var Item = Collection[Index];
            if (!Item.ShortName.Equals(ThreadName)) {
                Item.ShortName = ThreadName;
                Data.HistoryModified = true;
            }
        }
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