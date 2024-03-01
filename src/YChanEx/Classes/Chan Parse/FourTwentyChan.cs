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
        return $"/{URLSplit[4]}/ - {URLSplit[6]}";
    }
    public static string GetFullBoardName(string board, bool @override) {
        if (General.UseFullBoardNameForTitle || @override) {
            return board.ToLowerInvariant() switch {
                #region Drugs
                "weed" => "Cannabis Discussion",
                "hooch" => "Alcohol Discussion",
                "mdma" => "Ecstasy Discussion",
                "psy" => "Psychedelic Discussion",
                "stim" => "Stimulant Discussion",
                "dis" => "Dissociative Discussion",
                "opi" => "Opiate Discussion",
                "vape" => "Vaping Discussion",
                "tobacco" => "Tobacco Discussion",
                "benz" => "Benzo Discussion",
                "deli" => "Deliriant Discussion",
                "other" => "Other Drugs Discussion",
                "jenk" => "Jenkem Discussion",
                "detox" => "Detoxing & Rehabilitation",
                #endregion

                #region Lifestye
                "qq" => "Personal Issues",
                "dr" => "Dream Discussion",
                "ana" => "Fitness",
                "nom" => "Food, Munchies & Cooking",
                "vroom" => "Travel & Transportation",
                "st" => "Style & Fashion",
                "nra" => "Weapons Discussion",
                "sd" => "Sexuality Discussion",
                "cd" => "Transgender Discussion",
                #endregion

                #region Academia
                "art" => "Art & Okekai",
                "sagan" => "Space... the Final Frontier",
                "lang" => "World Languages",
                "stem" => "Science, Technology, Engineering & Mathematics",
                "his" => "History Discussion",
                "crops" => "Growing & Botany",
                "howto" => "Guides & Tutorials",
                "law" => "Law Discussion",
                "lit" => "Books & Literature",
                "med" => "Medicine & Health",
                "pss" => "Philosophy & Social Sciences",
                "tech" => "Computers & Tech Support",
                "prog" => "Programming",
                #endregion

                #region Media
                "1701" => "Star Trek Discussion",
                "sport" => "Sports",
                "mtv" => "Movies & Television",
                "f" => "Flash",
                "m" => "Music & Production",
                "mma" => "Mixed Martial Arts Discussion",
                "616" => "Comics & Web Comics Discussion",
                "a" => "Anime & Manga Discussion",
                "wooo" => "Professional Wrestling Discussion",
                "n" => "World News",
                "vg" => "Video Games Discussion",
                "po" => "Pokémon Discussion",
                "tg" => "Traditional Games",
                #endregion

                #region Miscellanea
                "420" => "420chan Discussion & Staff Interaction",
                "b" => "Random & High Stuff",
                "spooky" => "Paranormal Discussion",
                "dino" => "Dinosaur Discussion",
                "fo" => "Post-apocalyptic",
                "ani" => "Animal Discussion",
                "nj" => "Netjester AI Conversation Chamber",
                "nc" => "Net Characters",
                "tinfoil" => "Conspiracy Theories",
                "w" => "Dumb Wallpapers Below",
                #endregion

                #region Adult
                "h" => "Hentai",
                #endregion

                _ => $"{board} (Unknown board)"
            };
        }
        return board;
    }
}
