#nullable enable
namespace YChanEx.Parsers;
internal static class FourTwentyChan {
    // https://boards.420chan.org/(board)/thread/(id)/(semantic)
    public static string GetOldHistoryName(string Url) {
        if (Url.StartsWith("ychanex:")) {
            Url = Url[8..];
        }
        if (Url.StartsWith("view-source:")) {
            Url = Url[12..];
        }
        Url = Networking.CleanURL(Url);

        string[] URLSplit = Url.Split('/');
        return $"{URLSplit[4]} - {URLSplit[6]}";
    }
}
