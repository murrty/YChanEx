#nullable enable
namespace YChanEx;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using murrty.classes;
using murrty.updater;
internal static class Updater {
    public const string GithubLink = "https://github.com/murrty/ychanex";
    private const string LatestRepo = "https://api.github.com/repos/murrty/ychanex/releases/latest";
    private const string AllReleaseRepo = "https://api.github.com/repos/murrty/ychanex/releases";

    private static CancellationTokenSource? Token;

    internal static Task UpdateTask = Task.CompletedTask;

    public static GithubData? LastCheckedLatestRelease { get; private set; }
    public static GithubData? LastCheckedAllRelease { get; private set; }
    public static GithubData? LastChecked { get; private set; }

    static Updater() {
    }

    public async static Task<bool?> CheckForUpdate(bool ForceCheck) {
        if (!Networking.Tls12OrHigher) {
            Log.Warn("Cannot check for updates, TLS 1.2+ is not in use.");
            return null;
        }

        if (!UpdateTask.IsCompleted) {
            Log.Info("An update check is already in progress.");
            return null;
        }

        if (ForceCheck || (Initialization.CheckForBetaUpdates ? LastCheckedAllRelease is null : LastCheckedLatestRelease is null)) {
            UpdateTask = RefreshRelease();
            await UpdateTask;
        }

        return Initialization.CheckForBetaUpdates ?
            LastCheckedAllRelease?.IsNewerVersion :
            LastCheckedLatestRelease?.IsNewerVersion;
    }

    public static bool IsSkipped() {
        if (LastChecked is null) {
            return false;
        }

        if (LastChecked.IsBetaVersion) {
            return LastChecked.Version == Initialization.SkippedBetaVersion;
        }
        else {
            return LastChecked.Version == Initialization.SkippedVersion;
        }
    }
    public async static void ShowUpdateForm(bool AllowSkip) {
        if (LastChecked is null) {
            return;
        }

        if (Initialization.CheckForBetaUpdates ? LastCheckedAllRelease is null : LastCheckedLatestRelease is null) {
            await RefreshRelease();
            if (!LastChecked.IsNewerVersion) {
                return;
            }
            ShowUpdateForm(AllowSkip);
            return;
        }

        using frmUpdateAvailable UpdateDialog = new(LastChecked) {
            BlockSkip = !AllowSkip
        };
        switch (UpdateDialog.ShowDialog()) {
            case DialogResult.Yes: {
                await BeginUpdate();
            }
            break;

            case DialogResult.Ignore: {
                if (AllowSkip) {
                    Log.Info($"Ignoring update v{LastChecked.Version}");

                    if (Initialization.CheckForBetaUpdates) {
                        if (LastCheckedAllRelease is not null) {
                            Initialization.SkippedBetaVersion = LastCheckedAllRelease.Version;
                        }
                    }
                    else if (LastCheckedLatestRelease is not null) {
                        Initialization.SkippedVersion = LastCheckedLatestRelease.Version;
                    }
                }
            }
            break;
        }
    }
    private static async Task BeginUpdate() {
        await Task.Delay(5);
        Process.Start($"{Program.GithubPage}/releases/latest");
        /*
        string UpdaterPath = Environment.CurrentDirectory + pth.DirectorySeparatorChar + "ychanex-updater.exe";

        if (!File.Exists(UpdaterPath)) {
            File.WriteAllBytes(UpdaterPath, Properties.Resources.UpdaterStub);
        }

        // Simple logic to scan hash and compare it.
        // Not absolutely perfect, but it works well enough.
        using SHA256 CNG = SHA256.Create();

#if NETCOREAPP3_0_OR_GREATER
        await
#endif
        using FileStream UpdateFileStream = File.OpenRead(UpdaterPath);

        while (true) {
            string ReceivedHash = await CNG.GetHashAsync(UpdateFileStream);

            if (!LastKnownUpdaterHash.Equals(ReceivedHash, StringComparison.OrdinalIgnoreCase)) {
                DialogResult AlertResult = Log.MessageBox($$"""
                    The hash calculated compared to the known updater hash does not match.

                    Expected: {{LastKnownUpdaterHash}}
                    Received: {{ReceivedHash}}

                    You can retry the hash check, abort the update, or ignore the mismatch and continue with updating.
                    (note: the check is case insensitive)
                    """, MessageBoxButtons.AbortRetryIgnore);

                switch (AlertResult) {
                    case DialogResult.Retry: {
                        if (UpdateFileStream.CanSeek) {
                            UpdateFileStream.Position = 0;
                        }
                    }
                    continue;
                    case DialogResult.Ignore: break;
                    default: throw new CryptographicException("Update hash did not match.");
                }
            }
            break;
        }
        ProcessStartInfo UpdateProcess = new() {
#if NET5_0_OR_GREATER
            Arguments = $"-pid {Environment.ProcessId} -hwnd {Program.QueueHandler!.Handle}",
#else
            Arguments = $"-pid {Process.GetCurrentProcess().Id} -hwnd {Program.QueueHandler!.Handle}",
#endif
            FileName = UpdaterPath,
            WorkingDirectory = Environment.CurrentDirectory,
        };
        Process.Start(UpdateProcess);
        */
    }
    public static void CancelUpdate() {
        Token?.Cancel();
    }

    private async static Task RefreshRelease() {
        Token = new();
        HttpRequestMessage Request = new(HttpMethod.Get, Initialization.CheckForBetaUpdates ? AllReleaseRepo : LatestRepo);
        using var Response = await Networking.GetResponseAsync(Request, Token.Token);
        if (Response == null) {
            LastChecked = null;
            LastCheckedLatestRelease = null;
            LastCheckedAllRelease = null;
            return;
        }

        string Json = await Networking.GetStringAsync(Response, Token.Token);

        if (Json.IsNullEmptyWhitespace()) {
            LastChecked = null;
            LastCheckedLatestRelease = null;
            LastCheckedAllRelease = null;
            return;
        }

        GithubData? CurrentCheck;

        if (Initialization.CheckForBetaUpdates) {
            GithubData[]? Releases = Json.JsonDeserialize<GithubData[]>();
            if (Releases is null || Releases.Length == 0) {
                LastChecked = null;
                LastCheckedLatestRelease = null;
                LastCheckedAllRelease = null;
                return;
            }
            CurrentCheck = LastCheckedAllRelease = GithubData.GetNewestRelease(Releases);
        }
        else {
            CurrentCheck = Json.JsonDeserialize<GithubData>();
            if (CurrentCheck is null) {
                LastChecked = null;
                LastCheckedLatestRelease = null;
                LastCheckedAllRelease = null;
                return;
            }
            LastCheckedLatestRelease = CurrentCheck;
        }

        LastChecked = CurrentCheck;
    }
}
