namespace YChanEx;

using System.Text.RegularExpressions;

/// <summary>
/// This class contains methods for translating and managing chan threads.
/// Most backend is here, except for individual chan apis and parsing.
/// </summary>
internal class Chans {

    /// <summary>
    /// Class containing default regular expression patterns that was last known to work.
    /// </summary>
    internal class DefaultRegex {
        public const string FourChanURL =
            "(boards.)?4chan(nel)?.org\\/[a-zA-Z0-9]*?\\/thread[0-9]*";

        public const string FourTwentyChanURL =
            "boards.420chan.org\\/[a-zA-Z0-9]*?\\/thread/[0-9]*";

        public const string SevenChanURL =
            "7chan.org\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

        public const string SevenChanPosts =
            "(?<=<a target=\"_blank\" href=\").*?( class=\"thumb\")";

        public const string SevenChanHtmlMonkeyPosts =
            "div[class:=\"post\"][id:=\"^([0-9]+)\"]";

        public const string EightChanURL =
            "8chan[.moe|.se|.cc]+\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

        public const string EightKunURL =
            "8kun.top\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

        public const string fchanURL =
            "fchan.us\\/[a-zA-Z0-9]*?\\/res\\/[0-9]*.[^0-9]*";

        public const string fchanFiles =
            "(?<=File: <a target=\"_blank\" href=\").*?(?=</a>)";

        public const string fchanIDs =
            "(?=<img id=\"img).*?(\" src=\")";

        public const string u18chanURL =
            "u18chan.com\\/(.*?)[a-zA-Z0-9]*?\\/topic\\/[0-9]*";

        public const string u18chanPosts =
            "(?<=a href=\").*?(_image\" style=\"width: )";
    }

    /// <summary>
    /// Whether the input <paramref name="URL"/> is supported by the program, with <paramref name="Type"/> as the output <see cref="ChanType"/> if true.
    /// </summary>
    /// <param name="URL">The URL to the thread that is requested to be parsed.</param>
    /// <param name="Type">The out-ChanType chan of the url.</param>
    /// <returns></returns>
    public static bool SupportedChan(string URL, out ChanType Type) {
        if (Regex.IsMatch(URL, DefaultRegex.FourChanURL)) {
            Type = ChanType.FourChan;
            return true;
        }
        if (Regex.IsMatch(URL, DefaultRegex.FourTwentyChanURL)) {
            Type = ChanType.FourTwentyChan;
            return true;
        }
        if (Regex.IsMatch(URL, DefaultRegex.SevenChanURL)) {
            Type = ChanType.SevenChan;
            return true;
        }
        if (Regex.IsMatch(URL, DefaultRegex.EightChanURL)) {
            Type = ChanType.EightChan;
            return true;
        }
        if (Regex.IsMatch(URL, DefaultRegex.EightKunURL)) {
            if (StupidFuckingBoard(ChanType.EightKun, URL)) {
                try {
                    throw new Exception("This program doesn't support archiving boards with content that is considered highly fucking stupid.");
                }
                catch (Exception ex) {
                    murrty.classes.Log.ReportException(ex);
                    Type = ChanType.Unsupported;
                    return false;
                }
            }
            Type = ChanType.EightKun;
            return true;
        }
        if (Regex.IsMatch(URL, DefaultRegex.fchanURL)) {
            Type = ChanType.fchan;
            return true;
        }
        if (Regex.IsMatch(URL, DefaultRegex.u18chanURL)) {
            Type = ChanType.u18chan;
            return true;
        }

        Type = ChanType.Unsupported;
        return false;
    }

    /// <summary>
    /// Gets the full chan title from the board id.
    /// </summary>
    /// <param name="Chan">The <see cref="ChanType"/> to parse the name from.</param>
    /// <param name="Board">The board (or extra info) to parse from.</param>
    /// <param name="OverrideOrDescription">If it should override the settings check for HTML, or if it's obtaining the info from the description</param>
    /// <returns>The string value of the title. If none is parse, it'll return the input board.</returns>
    public static string GetFullBoardName(ThreadInfo Thread, bool OverrideOrDescription = false) {
        switch (Thread.Chan) {

            case ChanType.FourChan: {
                if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.ThreadBoard.ToLower() switch {

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

                        _ => $"{Thread.Data.ThreadBoard} (Unknown board)"

                    };
                }
                else return Thread.Data.ThreadBoard;
            }

            case ChanType.FourTwentyChan: {
                if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.ThreadBoard.ToLower() switch {

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

                        _ => $"{Thread.Data.ThreadBoard} (Unknown board)"

                    };
                }
                else return Thread.Data.ThreadBoard;
            }

            case ChanType.SevenChan: {
                if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.ThreadBoard.ToLower() switch {

                        #region 7chan & Related services
                        "7ch" => "Site Discussion",
                        "ch7" => "Channel7 & Radio 7",
                        "irc" => "Internet Relay Circlejerk",
                        #endregion

                        #region VIP
                        "777" => "gardening",
                        "VIP" => "Very Important Posters",
                        "civ" => "Civics",
                        "vip6" => "IPv6 for VIP",
                        #endregion

                        #region Premium Content
                        "b" => "Random",
                        "banner" => "Banners",
                        "f" => "Flash",
                        "gfc" => "Grahpics Manipulation",
                        "fail" => "Failure",
                        #endregion

                        #region SFW
                        "class" => "The Finer Things",
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
                        "v" => "The Vineyard",
                        #endregion

                        _ => $"{Thread.Data.ThreadBoard} (Unknown board)"

                    };
                }
                else return Thread.Data.ThreadBoard;
            }

            case ChanType.EightChan:
            case ChanType.EightKun: {
                return Thread.Data.BoardName;
            }

            case ChanType.fchan: {
                if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.ThreadBoard.ToLower() switch {

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

                        _ => $"{Thread.Data.ThreadBoard} (Unknown board)"
                    };
                }
                else return Thread.Data.ThreadBoard;
            }

            case ChanType.u18chan: {
                if (Config.Settings.General.UseFullBoardNameForTitle || OverrideOrDescription) {
                    return Thread.Data.ThreadBoard.ToLower() switch {

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

                        _ => $"{Thread.Data.ThreadBoard} (Unknown board)"

                    };
                }
                else return Thread.Data.ThreadBoard;
            }

            default: return Thread.Data.ThreadBoard;

        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public static bool StupidFuckingBoard(ChanType Chan, string URL) {
        return Chan switch { // fuck you
            ChanType.EightKun => Regex.IsMatch(URL, "(qresearch)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening)"),
            _ => false
        };
    }

    public static string FindRegex(string Data, string Prefix, string Suffix) {
        Match M = new Regex($"(?<={Prefix}).*?(?={Suffix})").Match(Data);
        return M.Value;
    }
}