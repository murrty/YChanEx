namespace YChanEx;

using System.IO;

/// <summary>
/// Contains usability methods governing the application events.
/// </summary>
internal static class ProgramSettings {
    public static bool SaveThreads(this List<ThreadInfo> Data){
        List<string> Files = [];
        if (Directory.Exists($"{Program.SavedThreadsPath}")) {
            DirectoryInfo ExistingFiles = new(Program.SavedThreadsPath);
            Files = ExistingFiles.GetFiles("*.thread.json", SearchOption.TopDirectoryOnly)
                .Select(x => x.FullName)
                .ToList();
        }

        if (Data.Count > 0) {
            if (!Directory.Exists(Program.SavedThreadsPath)) {
                Directory.CreateDirectory(Program.SavedThreadsPath);
            }
            for (int i = 0; i < Data.Count; i++) {
                Files.Remove(Data[i].SavedThreadJson);
                if (Data[i].ThreadModified) {
                    File.WriteAllText($"{Data[i].SavedThreadJson}", Data[i].Data.JsonSerialize());
                }
            }

            if (Files.Count > 0) {
                for (int i = 0; i < Files.Count; i++) {
                    if (File.Exists(Files[i])) {
                        File.Delete(Files[i]);
                    }
                }
            }
        }
        return true;
    }
    public static void SaveThread(this ThreadInfo Thread) {
        if (Thread.ThreadModified) {
            ForceSaveThread(Thread);
        }
    }
    public static void ForceSaveThread(this ThreadInfo Thread) {
        if (!Directory.Exists(Program.SavedThreadsPath)) {
            Directory.CreateDirectory(Program.SavedThreadsPath);
        }
        File.WriteAllText($"{Thread.SavedThreadJson}", Thread.Data.JsonSerialize());
        Thread.ThreadModified = false;
    }

    public static List<ThreadData> LoadThreads() {
        List<ThreadData> list = [];

        List<FileInfo> SavedFiles = [];
        DirectoryInfo ScanningDirectory;
        if (Directory.Exists($"{Program.SavedThreadsPath}")) {
            ScanningDirectory = new($"{Program.SavedThreadsPath}");
            SavedFiles =
            [ .. SavedFiles,
                .. ScanningDirectory .GetFiles("*.thread.json", SearchOption.TopDirectoryOnly), ];
        }

        if (SavedFiles.Count > 0) {
            for (int i = 0; i < SavedFiles.Count; i++) {
                ThreadData Thread = File.ReadAllText(SavedFiles[i].FullName).JsonDeserialize<ThreadData>();
                if (Thread == null) {
                    continue;
                }
                Thread.FilePath = SavedFiles[i].FullName;
                list.Add(Thread);
            }
            //list.Sort((x, y) => x.QueueIndex.CompareTo(y.QueueIndex));
        }

        return list;
    }
}