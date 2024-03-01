#nullable enable
namespace YChanEx.Parsers;
internal static class FourChan {
    public static string GetOldHistoryName(string Url) {
        if (Url.StartsWith("ychanex:")) {
            Url = Url[8..];
        }
        if (Url.StartsWith("view-source:")) {
            Url = Url[12..];
        }
        Url = Networking.CleanURL(Url);

        string[] URLSplit = Url.Split('/');
        return $"/{URLSplit[^3]}/ - {URLSplit[^1]}";
    }

    public static string? GetHtmlTitle(ThreadData data) {
        if (data.ThreadName == null) {
            return null;
        }
        return GetHtmlTitle(data.Board, data.ThreadName);
    }
    public static string GetHtmlTitle(string board, string name) {
        return $"/{board}/ - {name} - 4chan";
    }
    public static string GetFullBoardName(string board, bool @override = false) {
        if (General.UseFullBoardNameForTitle || @override) {
            return board.ToLowerInvariant() switch {
                #region Japanese Culture
                "a" => "Anime & Manga",
                "c" => "Anime/Cute",
                "w" => "Anime/Wallpapers",
                "m" => "Mecha",
                "cgl" => "Cosplay & EGL",
                "cm" => "Cute/Male",
                "f" => "Flash",
                "n" => "Transportation",
                "jp" => "Otaku Culture",
                #endregion

                #region Video Games
                "v" => "Video Games",
                "vrpg" => "Video Games/RPG",
                "vmg" => "Video Games/Mobile",
                "vst" => "Video Games/Strategy",
                "vm" => "Video Games/Multiplayer",
                "vg" => "Video Game Generals",
                "vp" => "Pokémon",
                "vr" => "Retro Games",
                #endregion

                #region Interests
                "co" => "Comics & Cartoons",
                "g" => "Technology",
                "tv" => "Television & Film",
                "k" => "Weapons",
                "o" => "Auto",
                "an" => "Animals & Nature",
                "tg" => "Traditional Games",
                "sp" => "Sports",
                "xs" => "Extreme Sports",
                "pw" => "Professional Wrestling",
                "asp" => "Alternative Sports",
                "sci" => "Science & Math",
                "his" => "History & Humanities",
                "int" => "International",
                "out" => "Outdoors",
                "toy" => "Toys",

                #endregion

                #region Creative
                "i" => "Oekaki",
                "po" => "Papercraft & Origami",
                "p" => "Photography",
                "ck" => "Food & Cooking",
                "ic" => "Artwork/Critique",
                "wg" => "Wallpapers/General",
                "lit" => "Literature",
                "mu" => "Music",
                "fa" => "Fashion",
                "3" => "3DCG",
                "gd" => "Graphic Design",
                "diy" => "Do It Yourself",
                "wsg" => "Worksafe GIF",
                "qst" => "Quests",
                #endregion

                #region Other
                "biz" => "Business & Finance",
                "trv" => "Travel",
                "fit" => "Fitness",
                "x" => "Paranormal",
                "adv" => "Advice",
                "lgbt" => "Lesbian, Gay, Bisexual, & Transgender",
                "mlp" => "My Little Pony", // disgusting.
                "news" => "Current News",
                "wsr" => "Worksafe Requests",
                "vip" => "Very Important Posts",
                #endregion

                #region Misc
                "b" => "Random",
                "r9k" => "ROBOT9001",
                "pol" => "Politically Incorrect",
                "bant" => "International/Random",
                "soc" => "Cams & Meetups",
                "s4s" => "Shit 4chan Says",
                #endregion

                #region Adult
                "s" => "Sexy Beautiful Women",
                "hc" => "Hardcore",
                "hm" => "Handsome Men",
                "h" => "Hentai",
                "e" => "Ecchi",
                "u" => "Yuri",
                "d" => "Hentai/Alternative",
                "y" => "Yaoi",
                "t" => "Torrents",
                "hr" => "High Resolution",
                "gif" => "Adult GIF",
                "aco" => "Adult Cartoons",
                "r" => "Adult Requests",
                #endregion

                #region Unlisted
                "trash" => "Off-Topic",
                "qa" => "Question & Answer",
                #endregion

                _ => $"{board} (Unknown board)"
            };
        }
        return board;
    }

    public static DateTimeOffset GetPostTime(long timestamp) {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp);
    }
    public static string CalculatePackedBase64Hash(string md5) {
        // 24 character, packed base64 MD5 hash of file
        // Requires a query-string to bypass cloudflare caching which changes the hash.

        byte[] raw_bytes = new byte[16];
        for (int i = 0; i < 32; i += 2) {
            raw_bytes[i / 2] = Convert.ToByte(md5[i..(i + 2)], 16);
        }
        return Convert.ToBase64String(raw_bytes);
    }
}
