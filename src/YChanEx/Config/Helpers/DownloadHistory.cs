namespace YChanEx;

using System.Runtime.Serialization;

[DataContract]
public sealed class DownloadHistory {
    [IgnoreDataMember]
    public static DownloadHistory Data = new();
    [IgnoreDataMember]
    private static readonly string HistoryFile = Program.SavedThreadsPath + System.IO.Path.DirectorySeparatorChar + "History.json";
    [IgnoreDataMember]
    private bool HistoryModified;

    [DataMember(Name = "4chan", IsRequired = false, Order = 0)]
    public List<string> FourChanHistory = [];
    [DataMember(Name = "420chan", IsRequired = false, Order = 1)]
    public List<string> FourTwentyChanHistory = [];
    [DataMember(Name = "7chan", IsRequired = false, Order = 2)]
    public List<string> SevenChanHistory = [];
    [DataMember(Name = "8chan", IsRequired = false, Order = 3)]
    public List<string> EightChanHistory = [];
    [DataMember(Name = "8kun", IsRequired = false, Order = 4)]
    public List<string> EightKunHistory = [];
    [DataMember(Name = "fchan", IsRequired = false, Order = 5)]
    public List<string> FchanHistory = [];
    [DataMember(Name = "u18chan", IsRequired = false, Order = 6)]
    public List<string> u18chanHistory = [];

    [IgnoreDataMember]
    public static int Count =>
        Data.FourChanHistory.Count + Data.FourTwentyChanHistory.Count +
        Data.SevenChanHistory.Count + Data.EightChanHistory.Count +
        Data.EightKunHistory.Count + Data.FchanHistory.Count + Data.u18chanHistory.Count;

    [IgnoreDataMember]
    public static string[] History {
        get {
            return [
                .. Data.FourChanHistory,
                .. Data.FourTwentyChanHistory,
                .. Data.SevenChanHistory,
                .. Data.EightChanHistory,
                .. Data.EightKunHistory,
                .. Data.FchanHistory,
                .. Data.u18chanHistory,
            ];
        }
    }

    public static void Add(ChanType Chan, string URL) {
        switch (Chan) {
            case ChanType.FourChan: {
                if (!Data.FourChanHistory.Contains(URL)) {
                    Data.FourChanHistory.Add(URL);
                    Data.HistoryModified = true;
                }
            } break;

            case ChanType.FourTwentyChan: {
                if (!Data.FourTwentyChanHistory.Contains(URL)) {
                    Data.FourTwentyChanHistory.Add(URL);
                    Data.HistoryModified = true;
                }
            } break;

            case ChanType.SevenChan: {
                if (!Data.SevenChanHistory.Contains(URL)) {
                    Data.SevenChanHistory.Add(URL);
                    Data.HistoryModified = true;
                }
            } break;

            case ChanType.EightChan: {
                if (!Data.EightChanHistory.Contains(URL)) {
                    Data.EightChanHistory.Add(URL);
                    Data.HistoryModified = true;
                }
            } break;

            case ChanType.EightKun: {
                if (!Data.EightKunHistory.Contains(URL)) {
                    Data.EightKunHistory.Add(URL);
                    Data.HistoryModified = true;
                }
            } break;

            case ChanType.fchan: {
                if (!Data.FchanHistory.Contains(URL)) {
                    Data.FchanHistory.Add(URL);
                    Data.HistoryModified = true;
                }
            } break;

            case ChanType.u18chan: {
                if (!Data.u18chanHistory.Contains(URL)) {
                    Data.u18chanHistory.Add(URL);
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
            Data = System.IO.File.ReadAllText(HistoryFile).JsonDeserialize<DownloadHistory>();
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
    }
}