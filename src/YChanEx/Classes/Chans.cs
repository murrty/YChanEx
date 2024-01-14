namespace YChanEx;

using System.Text.RegularExpressions;

/// <summary>
/// This class contains methods for translating and managing chan threads.
/// Most backend is here, except for individual chan apis and parsing.
/// </summary>
internal static class Chans {
    private static readonly Regex FourChanRegex = new(@"^(http(s)?:\/\/)?(boards\.)?4chan(nel)?\.org\/[a-zA-Z0-9]*?\/thread\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex SevenChanRegex = new(@"^(http(s)?:\/\/)?(www\.)?7chan\.org\/[a-zA-Z0-9]*?\/res\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex EightChanRegex = new(@"^(http(s)?:\/\/)?(www\.)?8chan\.(moe|se|cc)\/[a-zA-Z0-9]*?\/res\/\d+\.(html|json)", RegexOptions.IgnoreCase);
    //private static readonly Regex EightKunRegex = new(@"^(http(s)?:\/\/)?(www\.)?8kun\.top\/(?!(qresearch)|(qnotables)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening))[a-zA-Z0-9]*?\/res\/\d+\.(html|json)", RegexOptions.IgnoreCase);
    private static readonly Regex FChanRegex = new(@"^(http(s)?:\/\/)?(www\.)?fchan\.us\/[a-zA-Z0-9]*?\/res\/\d+\.(html)", RegexOptions.IgnoreCase);
    private static readonly Regex U18ChanRegex = new(@"^(http(s)?:\/\/)?(www\.)?u18chan\.com\/(board\/u18chan\/)?[a-zA-Z0-9]*?\/topic\/\d+", RegexOptions.IgnoreCase);
    private static readonly Regex FoolFuukaRegex = new(@"^(http(s)?:\/\/)?(arch\.b4k\.co)|((www\.)?(archived\.moe|desuarchive\.org))\/[a-zA-Z0-9_]\/thread\/\d+", RegexOptions.IgnoreCase);

    /// <summary>
    /// Whether the input <paramref name="URL"/> is supported by the program, with <paramref name="Type"/> as the output <see cref="ChanType"/> if true.
    /// </summary>
    /// <param name="URL">The URL to the thread that is requested to be parsed.</param>
    /// <param name="Type">The out-ChanType chan of the url.</param>
    /// <returns></returns>
    public static bool SupportedChan(string URL, out ChanType Type) {
        if (FourChanRegex.IsMatch(URL)) {
            Type = ChanType.FourChan;
            return true;
        }
        if (SevenChanRegex.IsMatch(URL)) {
            Type = ChanType.SevenChan;
            return true;
        }
        if (EightChanRegex.IsMatch(URL)) {
            Type = ChanType.EightChan;
            return true;
        }

        /* 8kun is dead, this check disables it from being used. May re-enable in the future.
        if (EightKunRegex.IsMatch(URL)) {
            if (StupidFuckingBoard(ChanType.EightKun, URL)) {
                //Log.Write("This program doesn't support archiving boards with content that is considered highly fucking stupid.");
                System.Windows.Forms.MessageBox.Show("This program doesn't support archiving boards with content that is considered highly fucking stupid.", "YChanEx");
                Type = ChanType.Unsupported;
                return false;
            }
            Type = ChanType.EightKun;
            return true;
        }
        */

        if (FChanRegex.IsMatch(URL)) {
            Type = ChanType.fchan;
            return true;
        }
        if (U18ChanRegex.IsMatch(URL)) {
            Type = ChanType.u18chan;
            return true;
        }

        if (FoolFuukaRegex.IsMatch(URL)) {
            Type = ChanType.FoolFuuka;
            return true;
        }

        Type = ChanType.Unsupported;
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
            ChanType.EightKun => System.Text.RegularExpressions.Regex.IsMatch(URL,
                "(qresearch)|(qnotables)|(pnd)|(midnightriders)|(qrb)|(philogeometric)|(qsocial)|(qrnews)|(thestorm)|(patriotsfight)|(projectdcomms)|(greatawakening)"),
            _ => false
        };
    }
}
