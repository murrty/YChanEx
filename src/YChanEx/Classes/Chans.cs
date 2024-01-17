#nullable enable
#define ENABLE_8KUN
namespace YChanEx;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
/// <summary>
/// This class contains methods for verification and minor usability.
/// </summary>
internal static class Chans {
    private static readonly Regex FourChanRegex = new(@"^https:\/\/(boards\.)?4chan(nel)?\.org\/[a-zA-Z0-9]+\/thread\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex SevenChanRegex = new(@"^https:\/\/7chan\.org\/[a-zA-Z0-9]+\/res\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex EightChanRegex = new(@"^https:\/\/8chan\.(moe|se|cc)\/[a-zA-Z0-9]+\/res\/\d+\.(html|json)", RegexOptions.IgnoreCase);
#if ENABLE_8KUN && !RELEASE
    private static readonly Regex EightKunRegex = new(@"^https:\/\/8kun\.top\/(?!(qresearch)|(qnotables)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening))[a-zA-Z0-9]+\/res\/\d+\.(html|json)", RegexOptions.IgnoreCase);
#endif
    private static readonly Regex FChanRegex = new(@"^https:\/\/fchan\.us\/[a-zA-Z0-9]+\/res\/\d+\.(html)", RegexOptions.IgnoreCase);
    private static readonly Regex U18ChanRegex = new(@"^https:\/\/u18chan\.com\/(board\/u18chan\/)?[a-zA-Z0-9]+\/topic\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex FoolFuukaRegex = new(@"^https:\/\/((arch\.b4k\.co)|((www\.)?(archived\.moe)|(desuarchive\.org)|(thebarchive\.com)))\/[a-zA-Z0-9_]+\/thread\/\d+", RegexOptions.IgnoreCase);

    /// <summary>
    /// Tries to verify a chan URL.
    /// </summary>
    /// <param name="Url">The Url that will be scanned.</param>
    /// <param name="ThreadData">The thread data associated with the chan, if it's valid and can be used.</param>
    /// <returns><see langword="true"/> if the chan url is valid and can be use; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetThreadData([NotNullWhen(true)] string? Url, [NotNullWhen(true)] out ThreadData? ThreadData) {
        if (Url.IsNullEmptyWhitespace()) {
            ThreadData = null;
            return false;
        }

        if (Url.StartsWith("ychanex:")) {
            Url = Url[8..];
        }
        if (Url.StartsWith("view-source:")) {
            Url = Url[12..];
        }
        Url = Networking.CleanURL(Url);

        if (FourChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.FourChan);
            return true;
        }

        if (SevenChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.SevenChan);
            return true;
        }

        if (EightChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1].SubstringBeforeLastChar('.'),
                ThreadBoard: URLSplit[^3],
                Url: Url.Replace(".json", ".html", StringComparison.OrdinalIgnoreCase),
                Type: ChanType.EightChan);
            return true;
        }

        //8kun is dead, this check disables it from being used. May re-enable in the future.
#if ENABLE_8KUN && !RELEASE
        if (EightKunRegex.IsMatch(Url)) {
            if (StupidFuckingBoard(ChanType.EightKun, Url)) {
                //Log.Write("This program doesn't support archiving boards with content that is considered highly fucking stupid.");
                System.Windows.Forms.MessageBox.Show("This program doesn't support archiving boards with content that is considered highly fucking stupid.", "YChanEx");
                ThreadData = null;
                return false;
            }

            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1].SubstringBeforeLastChar('.'),
                ThreadBoard: URLSplit[^3],
                Url: Url.Replace(".json", ".html", StringComparison.OrdinalIgnoreCase),
                Type: ChanType.EightKun);
            return true;
        }
#endif

        if (FChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: "http" + Url[5..],
                Type: ChanType.fchan);
            return true;
        }
        if (U18ChanRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.u18chan);
            return true;
        }

        if (FoolFuukaRegex.IsMatch(Url)) {
            string[] URLSplit = Url.Split('/');
            ThreadData = new ThreadData(
                ThreadId: URLSplit[^1],
                ThreadBoard: URLSplit[^3],
                Url: Url,
                Type: ChanType.FoolFuuka);
            return true;
        }

        ThreadData = null;
        return false;
    }
    /// <summary>
    /// Whether the input <paramref name="Data"/> is supported by the program.
    /// </summary>
    /// <param name="Data">The data to reverify.</param>
    /// <returns></returns>
    public static bool ReverifyThreadData(ThreadData Data) {
        if (FourChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.FourChan;
            return true;
        }
        if (SevenChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.SevenChan;
            return true;
        }
        if (EightChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.EightChan;
            return true;
        }

        /* 8kun is dead, this check disables it from being used. May re-enable in the future.
        if (EightKunRegex.IsMatch(Data.Url)) {
            if (StupidFuckingBoard(ChanType.EightKun, Data.Url)) {
                //Log.Write("This program doesn't support archiving boards with content that is considered highly fucking stupid.");
                System.Windows.Forms.MessageBox.Show("This program doesn't support archiving boards with content that is considered highly fucking stupid.", "YChanEx");
                return false;
            }
            Data.ChanType = ChanType.EightKun;
            return true;
        }
        */

        if (FChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.fchan;
            return true;
        }
        if (U18ChanRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.u18chan;
            return true;
        }

        if (FoolFuukaRegex.IsMatch(Data.Url)) {
            Data.ChanType = ChanType.FoolFuuka;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the full chan title from the board id.
    /// </summary>
    /// <param name="Thread">Thread info to derive the board name from.</param>
    /// <param name="OverrideOrDescription">If it should override the settings check for HTML, or if it's obtaining the info from the description</param>
    /// <returns>The string value of the title. If none is parse, it'll return the input board.</returns>
    public static string GetFullBoardName(ThreadInfo Thread, bool OverrideOrDescription = false) {
        switch (Thread.Chan) {
            case ChanType.FourChan: {
                if (General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.Board.ToLower() switch {
                        #region Japanese Culture
                        "a"   => "Anime & Manga",
                        "c"   => "Anime/Cute",
                        "w"   => "Anime/Wallpapers",
                        "m"   => "Mecha",
                        "cgl" => "Cosplay & EGL",
                        "cm"  => "Cute/Male",
                        "f"   => "Flash",
                        "n"   => "Transportation",
                        "jp"  => "Otaku Culture",
                        #endregion

                        #region Video Games
                        "v"    => "Video Games",
                        "vrpg" => "Video Games/RPG",
                        "vmg"  => "Video Games/Mobile",
                        "vst"  => "Video Games/Strategy",
                        "vm"   => "Video Games/Multiplayer",
                        "vg"   => "Video Game Generals",
                        "vp"   => "Pokémon",
                        "vr"   => "Retro Games",
                        #endregion

                        #region Interests
                        "co"  => "Comics & Cartoons",
                        "g"   => "Technology",
                        "tv"  => "Television & Film",
                        "k"   => "Weapons",
                        "o"   => "Auto",
                        "an"  => "Animals & Nature",
                        "tg"  => "Traditional Games",
                        "sp"  => "Sports",
                        "xs"  => "Extreme Sports",
                        "pw"  => "Professional Wrestling",
                        "asp" => "Alternative Sports",
                        "sci" => "Science & Math",
                        "his" => "History & Humanities",
                        "int" => "International",
                        "out" => "Outdoors",
                        "toy" => "Toys",

                        #endregion

                        #region Creative
                        "i"   => "Oekaki",
                        "po"  => "Papercraft & Origami",
                        "p"   => "Photography",
                        "ck"  => "Food & Cooking",
                        "ic"  => "Artwork/Critique",
                        "wg"  => "Wallpapers/General",
                        "lit" => "Literature",
                        "mu"  => "Music",
                        "fa"  => "Fashion",
                        "3"   => "3DCG",
                        "gd"  => "Graphic Design",
                        "diy" => "Do It Yourself",
                        "wsg" => "Worksafe GIF",
                        "qst" => "Quests",
                        #endregion

                        #region Other
                        "biz"  => "Business & Finance",
                        "trv"  => "Travel",
                        "fit"  => "Fitness",
                        "x"    => "Paranormal",
                        "adv"  => "Advice",
                        "lgbt" => "Lesbian, Gay, Bisexual, & Transgender",
                        "mlp"  => "My Little Pony", // disgusting.
                        "news" => "Current News",
                        "wsr"  => "Worksafe Requests",
                        "vip"  => "Very Important Posts",
                        #endregion

                        #region Misc
                        "b"    => "Random",
                        "r9k"  => "ROBOT9001",
                        "pol"  => "Politically Incorrect",
                        "bant" => "International/Random",
                        "soc"  => "Cams & Meetups",
                        "s4s"  => "Shit 4chan Says",
                        #endregion

                        #region Adult
                        "s"   => "Sexy Beautiful Women",
                        "hc"  => "Hardcore",
                        "hm"  => "Handsome Men",
                        "h"   => "Hentai",
                        "e"   => "Ecchi",
                        "u"   => "Yuri",
                        "d"   => "Hentai/Alternative",
                        "y"   => "Yaoi",
                        "t"   => "Torrents",
                        "hr"  => "High Resolution",
                        "gif" => "Adult GIF",
                        "aco" => "Adult Cartoons",
                        "r"   => "Adult Requests",
                        #endregion

                        #region Unlisted
                        "trash" => "Off-Topic",
                        "qa"    => "Question & Answer",
                        #endregion

                        _ => $"{Thread.Data.Board} (Unknown board)"
                    };
                }
                return Thread.Data.Board;
            }

            case ChanType.FourTwentyChan: {
                if (General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.Board.ToLower() switch {
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

                        _ => $"{Thread.Data.Board} (Unknown board)"
                    };
                }
                return Thread.Data.Board;
            }

            case ChanType.SevenChan: {
                if (General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.Board.ToLower() switch {
                        #region 7chan & Related services
                        "7ch" => "Site Discussion",
                        "ch7" => "Channel7 & Radio7",
                        "irc" => "Internet Relay Circlejerk",
                        #endregion

                        #region VIP
                        "VIP" => "Very Important Posters",
                        "civ" => "Civics",
                        //"vip6" => "IPv6 for VIP",
                        #endregion

                        #region Premium Content
                        "777" => "weed",
                        "b" => "Random",
                        "banner" => "Banners",
                        "f" => "Flash",
                        "gfx" => "Grahpics Manipulation",
                        "fail" => "Failure",
                        #endregion

                        #region SFW
                        //"class" => "The Finer Things",
                        "co" => "Comics and Cartoons",
                        "eh" => "Particularly uninteresting conversation",
                        "fit" => "Fitness & Health",
                        "halp" => "Technical Support",
                        "jew" => "Thrifty Living",
                        "lit" => "Literature",
                        "phi" => "Philosophy",
                        "pr" => "Programming",
                        "rnb" => "Rage and Baww",
                        "sci" => "Science, Technology, Engineering, and Mathematics",
                        "tg" => "Tabletop Games",
                        "w" => "Weapons",
                        "zom" => "Zombies",
                        #endregion

                        #region General
                        "a" => "Anime & Manga",
                        "ai" => "AI-Generated Artwork & Text",
                        "grim" => "Cold, Grim & Miserable",
                        "hi" => "History and Culture",
                        "me" => "Film, Music & Television",
                        "rx" => "Drugs",
                        "vg" => "Video Games",
                        "wp" => "Wallpapers",
                        "x" => "Paranormal & Conspiracy",
                        #endregion

                        #region Porn
                        "cake" => "Delicious",
                        "cd" => "Crossdressing",
                        "d" => "Alternative Hentai",
                        "di" => "Sexy Beautiful Traps",
                        "elit" => "Erotic Literature",
                        "fag" => "Men Discussion",
                        "fur" => "Furry",
                        "gif" => "Animated GIFs",
                        "h" => "Hentai",
                        "men" => "Sexy Beautiful Men",
                        "pco" => "Porn Comics",
                        "s" => "Sexy Beautiful Women",
                        "sm" => "Shotacon",
                        "ss" => "Straight Shotacon",
                        "unf" => "Uniforms",
                        //"v" => "The Vineyard",
                        #endregion

                        _ => $"{Thread.Data.Board} (Unknown board)"
                    };
                }
                return Thread.Data.Board;
            }

            case ChanType.EightChan:
            case ChanType.EightKun: {
                return Thread.Data.BoardName ?? Thread.Data.Board;
            }

            case ChanType.fchan: {
                if (General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.Board.ToLower() switch {
                        #region Normal image boards
                        "f" => "female",
                        "m" => "male",
                        "h" => "herm",
                        "s" => "straight",
                        "toon" => "toon",
                        "a" => "alternative",
                        "ah" => "alternative (hard)",
                        "c" => "clean",
                        #endregion

                        #region Specialized image boards
                        "artist" => "artist",
                        "crit" => "critique",
                        "b" => "banners",
                        #endregion

                        _ => $"{Thread.Data.Board} (Unknown board)"
                    };
                }
                return Thread.Data.Board;
            }

            case ChanType.u18chan: {
                if (General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.Board.ToLower() switch {
                        #region Furry Related
                        "fur" => "Furries",
                        "c" => "Furry Comics",
                        "gfur" => "Gay Furries",
                        "gc" => "Gay Furry Comics",
                        "i" => "Intersex",
                        "rs" => "Request & Source",
                        "a" => "Animated",
                        "cute" => "Cute",
                        #endregion

                        #region The Basement
                        "pb" => "Post Your Naked Body",
                        "p" => "Ponies", // Why, honestly, WHY?
                        "f" => "Feral",
                        "cub" => "Cub",
                        "gore" => "Gore",
                        #endregion

                        #region General
                        "d" => "Discussion",
                        "mu" => "Music",
                        "w" => "Wallpapers",
                        "v" => "Video Games",
                        "lo" => "Lounge",
                        "tech" => "Technology",
                        "lit" => "Literature",
                        #endregion

                        _ => $"{Thread.Data.Board} (Unknown board)"
                    };
                }
                return Thread.Data.Board;
            }
        }

        // All else fails fallback.
        return Thread.Data.Board;
    }

    [System.Diagnostics.DebuggerStepThrough]
    public static bool StupidFuckingBoard(ChanType Chan, string URL) {
        return Chan switch {
            ChanType.EightKun => Regex.IsMatch(URL,
                "(qresearch)|(qnotables)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening)"),
            _ => false
        };
    }

    internal static string? SearchMatch(string input, string prefix, string suffix) {
        Match M = new Regex($"(?<={prefix}).*?(?={suffix})").Match(input);
        if (!M.Success) {
            return null;
        }
        return M.Value;
    }
    internal static string? SearchMatch(string input, string prefix, string suffix, bool escape) {
        Match M = escape ?
            new Regex($"(?<={Regex.Escape(prefix)}).*?(?={Regex.Escape(suffix)})").Match(input) :
            new Regex($"(?<={prefix}).*?(?={suffix})").Match(input);
        if (!M.Success) {
            return null;
        }
        return M.Value;
    }
    internal static string[] SearchMatches(string input, string prefix, string suffix) {
        MatchCollection M = new Regex($"(?<={prefix}).*?(?={suffix})").Matches(input);
        if (M.Count < 1) {
            return [];
        }
        return M.Cast<Match>()
            .Select(x => x.Value)
            .ToArray();
    }
    internal static string[] SearchMatches(string input, string prefix, string suffix, bool escape) {
        MatchCollection M = escape ?
            new Regex($"(?<={Regex.Escape(prefix)}).*?(?={Regex.Escape(suffix)})").Matches(input) :
            new Regex($"(?<={prefix}).*?(?={suffix})").Matches(input);
        if (M.Count < 1) {
            return [];
        }
        return M.Cast<Match>()
            .Select(x => x.Value)
            .ToArray();
    }
}
