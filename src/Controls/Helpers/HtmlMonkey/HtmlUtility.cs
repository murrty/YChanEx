// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: https://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: https://zzzprojects.com/
// Copyright © ZZZ Projects Inc. All rights reserved.
#nullable enable
namespace SoftCircuits.HtmlMonkey;
using System.Globalization;
using System.Text;
/// <summary>
/// A utility class to replace special characters by entities and vice-versa.
/// Follows HTML 4.0 specification found at <see href="http://www.w3.org/TR/html4/sgml/entities.html"/>.<para/>
/// Follows Additional specification found at <see href="https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references"/><para/>
/// See also: <see href="https://html.spec.whatwg.org/multipage/named-characters.html#named-character-references"/><para/>
/// This is directly copied from HtmlAgilityPack by ZZZ Projects (Licensed under MIT) and modified to be more simple to understand.
/// </summary>
public static class HtmlUtility {
    #region Static Members
    private static readonly int _maxEntitySize;

    /// <summary>
    /// Whether to use <see cref="System.Net.WebUtility"/> instead of this class for decoding and encoding.
    /// </summary>
    public static bool UseWebUtility { get; set; }

    /// <summary>
    /// A collection of entities indexed by name.
    /// </summary>
    private static Dictionary<int, string> EntityName;

    /// <summary>
    /// A collection of entities indexed by value.
    /// </summary>
    private static Dictionary<string, int> EntityValue;
    #endregion

    #region Constructors
    static HtmlUtility() {
        EntityName = [];
        EntityValue = [];

        #region Entities Definition
        EntityValue.Add("quot", 34); // quotation mark = APL quote, U+0022 ISOnum 
        EntityName.Add(34, "quot");
        EntityValue.Add("amp", 38); // ampersand, U+0026 ISOnum 
        EntityName.Add(38, "amp");
        EntityValue.Add("apos", 39); // apostrophe-quote     U+0027 (39)
        EntityName.Add(39, "apos");
        EntityValue.Add("lt", 60); // less-than sign, U+003C ISOnum 
        EntityName.Add(60, "lt");
        EntityValue.Add("gt", 62); // greater-than sign, U+003E ISOnum 
        EntityName.Add(62, "gt");
        EntityValue.Add("nbsp", 160); // no-break space = non-breaking space, U+00A0 ISOnum 
        EntityName.Add(160, "nbsp");
        EntityValue.Add("iexcl", 161); // inverted exclamation mark, U+00A1 ISOnum 
        EntityName.Add(161, "iexcl");
        EntityValue.Add("cent", 162); // cent sign, U+00A2 ISOnum 
        EntityName.Add(162, "cent");
        EntityValue.Add("pound", 163); // pound sign, U+00A3 ISOnum 
        EntityName.Add(163, "pound");
        EntityValue.Add("curren", 164); // currency sign, U+00A4 ISOnum 
        EntityName.Add(164, "curren");
        EntityValue.Add("yen", 165); // yen sign = yuan sign, U+00A5 ISOnum 
        EntityName.Add(165, "yen");
        EntityValue.Add("brvbar", 166); // broken bar = broken vertical bar, U+00A6 ISOnum 
        EntityName.Add(166, "brvbar");
        EntityValue.Add("sect", 167); // section sign, U+00A7 ISOnum 
        EntityName.Add(167, "sect");
        EntityValue.Add("uml", 168); // diaeresis = spacing diaeresis, U+00A8 ISOdia 
        EntityName.Add(168, "uml");
        EntityValue.Add("copy", 169); // copyright sign, U+00A9 ISOnum 
        EntityName.Add(169, "copy");
        EntityValue.Add("ordf", 170); // feminine ordinal indicator, U+00AA ISOnum 
        EntityName.Add(170, "ordf");
        EntityValue.Add("laquo", 171);
        // left-pointing double angle quotation mark = left pointing guillemet, U+00AB ISOnum 
        EntityName.Add(171, "laquo");
        EntityValue.Add("not", 172); // not sign, U+00AC ISOnum 
        EntityName.Add(172, "not");
        EntityValue.Add("shy", 173); // soft hyphen = discretionary hyphen, U+00AD ISOnum 
        EntityName.Add(173, "shy");
        EntityValue.Add("reg", 174); // registered sign = registered trade mark sign, U+00AE ISOnum 
        EntityName.Add(174, "reg");
        EntityValue.Add("macr", 175); // macron = spacing macron = overline = APL overbar, U+00AF ISOdia 
        EntityName.Add(175, "macr");
        EntityValue.Add("deg", 176); // degree sign, U+00B0 ISOnum 
        EntityName.Add(176, "deg");
        EntityValue.Add("plusmn", 177); // plus-minus sign = plus-or-minus sign, U+00B1 ISOnum 
        EntityName.Add(177, "plusmn");
        EntityValue.Add("sup2", 178); // superscript two = superscript digit two = squared, U+00B2 ISOnum 
        EntityName.Add(178, "sup2");
        EntityValue.Add("sup3", 179); // superscript three = superscript digit three = cubed, U+00B3 ISOnum 
        EntityName.Add(179, "sup3");
        EntityValue.Add("acute", 180); // acute accent = spacing acute, U+00B4 ISOdia 
        EntityName.Add(180, "acute");
        EntityValue.Add("micro", 181); // micro sign, U+00B5 ISOnum 
        EntityName.Add(181, "micro");
        EntityValue.Add("para", 182); // pilcrow sign = paragraph sign, U+00B6 ISOnum 
        EntityName.Add(182, "para");
        EntityValue.Add("middot", 183); // middle dot = Georgian comma = Greek middle dot, U+00B7 ISOnum 
        EntityName.Add(183, "middot");
        EntityValue.Add("cedil", 184); // cedilla = spacing cedilla, U+00B8 ISOdia 
        EntityName.Add(184, "cedil");
        EntityValue.Add("sup1", 185); // superscript one = superscript digit one, U+00B9 ISOnum 
        EntityName.Add(185, "sup1");
        EntityValue.Add("ordm", 186); // masculine ordinal indicator, U+00BA ISOnum 
        EntityName.Add(186, "ordm");
        EntityValue.Add("raquo", 187);
        // right-pointing double angle quotation mark = right pointing guillemet, U+00BB ISOnum 
        EntityName.Add(187, "raquo");
        EntityValue.Add("frac14", 188); // vulgar fraction one quarter = fraction one quarter, U+00BC ISOnum 
        EntityName.Add(188, "frac14");
        EntityValue.Add("frac12", 189); // vulgar fraction one half = fraction one half, U+00BD ISOnum 
        EntityName.Add(189, "frac12");
        EntityValue.Add("frac34", 190); // vulgar fraction three quarters = fraction three quarters, U+00BE ISOnum 
        EntityName.Add(190, "frac34");
        EntityValue.Add("iquest", 191); // inverted question mark = turned question mark, U+00BF ISOnum 
        EntityName.Add(191, "iquest");
        EntityValue.Add("Agrave", 192);
        // latin capital letter A with grave = latin capital letter A grave, U+00C0 ISOlat1 
        EntityName.Add(192, "Agrave");
        EntityValue.Add("Aacute", 193); // latin capital letter A with acute, U+00C1 ISOlat1 
        EntityName.Add(193, "Aacute");
        EntityValue.Add("Acirc", 194); // latin capital letter A with circumflex, U+00C2 ISOlat1 
        EntityName.Add(194, "Acirc");
        EntityValue.Add("Atilde", 195); // latin capital letter A with tilde, U+00C3 ISOlat1 
        EntityName.Add(195, "Atilde");
        EntityValue.Add("Auml", 196); // latin capital letter A with diaeresis, U+00C4 ISOlat1 
        EntityName.Add(196, "Auml");
        EntityValue.Add("Aring", 197);
        // latin capital letter A with ring above = latin capital letter A ring, U+00C5 ISOlat1 
        EntityName.Add(197, "Aring");
        EntityValue.Add("AElig", 198); // latin capital letter AE = latin capital ligature AE, U+00C6 ISOlat1 
        EntityName.Add(198, "AElig");
        EntityValue.Add("Ccedil", 199); // latin capital letter C with cedilla, U+00C7 ISOlat1 
        EntityName.Add(199, "Ccedil");
        EntityValue.Add("Egrave", 200); // latin capital letter E with grave, U+00C8 ISOlat1 
        EntityName.Add(200, "Egrave");
        EntityValue.Add("Eacute", 201); // latin capital letter E with acute, U+00C9 ISOlat1 
        EntityName.Add(201, "Eacute");
        EntityValue.Add("Ecirc", 202); // latin capital letter E with circumflex, U+00CA ISOlat1 
        EntityName.Add(202, "Ecirc");
        EntityValue.Add("Euml", 203); // latin capital letter E with diaeresis, U+00CB ISOlat1 
        EntityName.Add(203, "Euml");
        EntityValue.Add("Igrave", 204); // latin capital letter I with grave, U+00CC ISOlat1 
        EntityName.Add(204, "Igrave");
        EntityValue.Add("Iacute", 205); // latin capital letter I with acute, U+00CD ISOlat1 
        EntityName.Add(205, "Iacute");
        EntityValue.Add("Icirc", 206); // latin capital letter I with circumflex, U+00CE ISOlat1 
        EntityName.Add(206, "Icirc");
        EntityValue.Add("Iuml", 207); // latin capital letter I with diaeresis, U+00CF ISOlat1 
        EntityName.Add(207, "Iuml");
        EntityValue.Add("ETH", 208); // latin capital letter ETH, U+00D0 ISOlat1 
        EntityName.Add(208, "ETH");
        EntityValue.Add("Ntilde", 209); // latin capital letter N with tilde, U+00D1 ISOlat1 
        EntityName.Add(209, "Ntilde");
        EntityValue.Add("Ograve", 210); // latin capital letter O with grave, U+00D2 ISOlat1 
        EntityName.Add(210, "Ograve");
        EntityValue.Add("Oacute", 211); // latin capital letter O with acute, U+00D3 ISOlat1 
        EntityName.Add(211, "Oacute");
        EntityValue.Add("Ocirc", 212); // latin capital letter O with circumflex, U+00D4 ISOlat1 
        EntityName.Add(212, "Ocirc");
        EntityValue.Add("Otilde", 213); // latin capital letter O with tilde, U+00D5 ISOlat1 
        EntityName.Add(213, "Otilde");
        EntityValue.Add("Ouml", 214); // latin capital letter O with diaeresis, U+00D6 ISOlat1 
        EntityName.Add(214, "Ouml");
        EntityValue.Add("times", 215); // multiplication sign, U+00D7 ISOnum 
        EntityName.Add(215, "times");
        EntityValue.Add("Oslash", 216);
        // latin capital letter O with stroke = latin capital letter O slash, U+00D8 ISOlat1 
        EntityName.Add(216, "Oslash");
        EntityValue.Add("Ugrave", 217); // latin capital letter U with grave, U+00D9 ISOlat1 
        EntityName.Add(217, "Ugrave");
        EntityValue.Add("Uacute", 218); // latin capital letter U with acute, U+00DA ISOlat1 
        EntityName.Add(218, "Uacute");
        EntityValue.Add("Ucirc", 219); // latin capital letter U with circumflex, U+00DB ISOlat1 
        EntityName.Add(219, "Ucirc");
        EntityValue.Add("Uuml", 220); // latin capital letter U with diaeresis, U+00DC ISOlat1 
        EntityName.Add(220, "Uuml");
        EntityValue.Add("Yacute", 221); // latin capital letter Y with acute, U+00DD ISOlat1 
        EntityName.Add(221, "Yacute");
        EntityValue.Add("THORN", 222); // latin capital letter THORN, U+00DE ISOlat1 
        EntityName.Add(222, "THORN");
        EntityValue.Add("szlig", 223); // latin small letter sharp s = ess-zed, U+00DF ISOlat1 
        EntityName.Add(223, "szlig");
        EntityValue.Add("agrave", 224);
        // latin small letter a with grave = latin small letter a grave, U+00E0 ISOlat1 
        EntityName.Add(224, "agrave");
        EntityValue.Add("aacute", 225); // latin small letter a with acute, U+00E1 ISOlat1 
        EntityName.Add(225, "aacute");
        EntityValue.Add("acirc", 226); // latin small letter a with circumflex, U+00E2 ISOlat1 
        EntityName.Add(226, "acirc");
        EntityValue.Add("atilde", 227); // latin small letter a with tilde, U+00E3 ISOlat1 
        EntityName.Add(227, "atilde");
        EntityValue.Add("auml", 228); // latin small letter a with diaeresis, U+00E4 ISOlat1 
        EntityName.Add(228, "auml");
        EntityValue.Add("aring", 229);
        // latin small letter a with ring above = latin small letter a ring, U+00E5 ISOlat1 
        EntityName.Add(229, "aring");
        EntityValue.Add("aelig", 230); // latin small letter ae = latin small ligature ae, U+00E6 ISOlat1 
        EntityName.Add(230, "aelig");
        EntityValue.Add("ccedil", 231); // latin small letter c with cedilla, U+00E7 ISOlat1 
        EntityName.Add(231, "ccedil");
        EntityValue.Add("egrave", 232); // latin small letter e with grave, U+00E8 ISOlat1 
        EntityName.Add(232, "egrave");
        EntityValue.Add("eacute", 233); // latin small letter e with acute, U+00E9 ISOlat1 
        EntityName.Add(233, "eacute");
        EntityValue.Add("ecirc", 234); // latin small letter e with circumflex, U+00EA ISOlat1 
        EntityName.Add(234, "ecirc");
        EntityValue.Add("euml", 235); // latin small letter e with diaeresis, U+00EB ISOlat1 
        EntityName.Add(235, "euml");
        EntityValue.Add("igrave", 236); // latin small letter i with grave, U+00EC ISOlat1 
        EntityName.Add(236, "igrave");
        EntityValue.Add("iacute", 237); // latin small letter i with acute, U+00ED ISOlat1 
        EntityName.Add(237, "iacute");
        EntityValue.Add("icirc", 238); // latin small letter i with circumflex, U+00EE ISOlat1 
        EntityName.Add(238, "icirc");
        EntityValue.Add("iuml", 239); // latin small letter i with diaeresis, U+00EF ISOlat1 
        EntityName.Add(239, "iuml");
        EntityValue.Add("eth", 240); // latin small letter eth, U+00F0 ISOlat1 
        EntityName.Add(240, "eth");
        EntityValue.Add("ntilde", 241); // latin small letter n with tilde, U+00F1 ISOlat1 
        EntityName.Add(241, "ntilde");
        EntityValue.Add("ograve", 242); // latin small letter o with grave, U+00F2 ISOlat1 
        EntityName.Add(242, "ograve");
        EntityValue.Add("oacute", 243); // latin small letter o with acute, U+00F3 ISOlat1 
        EntityName.Add(243, "oacute");
        EntityValue.Add("ocirc", 244); // latin small letter o with circumflex, U+00F4 ISOlat1 
        EntityName.Add(244, "ocirc");
        EntityValue.Add("otilde", 245); // latin small letter o with tilde, U+00F5 ISOlat1 
        EntityName.Add(245, "otilde");
        EntityValue.Add("ouml", 246); // latin small letter o with diaeresis, U+00F6 ISOlat1 
        EntityName.Add(246, "ouml");
        EntityValue.Add("divide", 247); // division sign, U+00F7 ISOnum 
        EntityName.Add(247, "divide");
        EntityValue.Add("oslash", 248);
        // latin small letter o with stroke, = latin small letter o slash, U+00F8 ISOlat1 
        EntityName.Add(248, "oslash");
        EntityValue.Add("ugrave", 249); // latin small letter u with grave, U+00F9 ISOlat1 
        EntityName.Add(249, "ugrave");
        EntityValue.Add("uacute", 250); // latin small letter u with acute, U+00FA ISOlat1 
        EntityName.Add(250, "uacute");
        EntityValue.Add("ucirc", 251); // latin small letter u with circumflex, U+00FB ISOlat1 
        EntityName.Add(251, "ucirc");
        EntityValue.Add("uuml", 252); // latin small letter u with diaeresis, U+00FC ISOlat1 
        EntityName.Add(252, "uuml");
        EntityValue.Add("yacute", 253); // latin small letter y with acute, U+00FD ISOlat1 
        EntityName.Add(253, "yacute");
        EntityValue.Add("thorn", 254); // latin small letter thorn, U+00FE ISOlat1 
        EntityName.Add(254, "thorn");
        EntityValue.Add("yuml", 255); // latin small letter y with diaeresis, U+00FF ISOlat1 
        EntityName.Add(255, "yuml");
        EntityValue.Add("fnof", 402); // latin small f with hook = function = florin, U+0192 ISOtech 
        EntityName.Add(402, "fnof");
        EntityValue.Add("Alpha", 913); // greek capital letter alpha, U+0391 
        EntityName.Add(913, "Alpha");
        EntityValue.Add("Beta", 914); // greek capital letter beta, U+0392 
        EntityName.Add(914, "Beta");
        EntityValue.Add("Gamma", 915); // greek capital letter gamma, U+0393 ISOgrk3 
        EntityName.Add(915, "Gamma");
        EntityValue.Add("Delta", 916); // greek capital letter delta, U+0394 ISOgrk3 
        EntityName.Add(916, "Delta");
        EntityValue.Add("Epsilon", 917); // greek capital letter epsilon, U+0395 
        EntityName.Add(917, "Epsilon");
        EntityValue.Add("Zeta", 918); // greek capital letter zeta, U+0396 
        EntityName.Add(918, "Zeta");
        EntityValue.Add("Eta", 919); // greek capital letter eta, U+0397 
        EntityName.Add(919, "Eta");
        EntityValue.Add("Theta", 920); // greek capital letter theta, U+0398 ISOgrk3 
        EntityName.Add(920, "Theta");
        EntityValue.Add("Iota", 921); // greek capital letter iota, U+0399 
        EntityName.Add(921, "Iota");
        EntityValue.Add("Kappa", 922); // greek capital letter kappa, U+039A 
        EntityName.Add(922, "Kappa");
        EntityValue.Add("Lambda", 923); // greek capital letter lambda, U+039B ISOgrk3 
        EntityName.Add(923, "Lambda");
        EntityValue.Add("Mu", 924); // greek capital letter mu, U+039C 
        EntityName.Add(924, "Mu");
        EntityValue.Add("Nu", 925); // greek capital letter nu, U+039D 
        EntityName.Add(925, "Nu");
        EntityValue.Add("Xi", 926); // greek capital letter xi, U+039E ISOgrk3 
        EntityName.Add(926, "Xi");
        EntityValue.Add("Omicron", 927); // greek capital letter omicron, U+039F 
        EntityName.Add(927, "Omicron");
        EntityValue.Add("Pi", 928); // greek capital letter pi, U+03A0 ISOgrk3 
        EntityName.Add(928, "Pi");
        EntityValue.Add("Rho", 929); // greek capital letter rho, U+03A1 
        EntityName.Add(929, "Rho");
        EntityValue.Add("Sigma", 931); // greek capital letter sigma, U+03A3 ISOgrk3 
        EntityName.Add(931, "Sigma");
        EntityValue.Add("Tau", 932); // greek capital letter tau, U+03A4 
        EntityName.Add(932, "Tau");
        EntityValue.Add("Upsilon", 933); // greek capital letter upsilon, U+03A5 ISOgrk3 
        EntityName.Add(933, "Upsilon");
        EntityValue.Add("Phi", 934); // greek capital letter phi, U+03A6 ISOgrk3 
        EntityName.Add(934, "Phi");
        EntityValue.Add("Chi", 935); // greek capital letter chi, U+03A7 
        EntityName.Add(935, "Chi");
        EntityValue.Add("Psi", 936); // greek capital letter psi, U+03A8 ISOgrk3 
        EntityName.Add(936, "Psi");
        EntityValue.Add("Omega", 937); // greek capital letter omega, U+03A9 ISOgrk3 
        EntityName.Add(937, "Omega");
        EntityValue.Add("alpha", 945); // greek small letter alpha, U+03B1 ISOgrk3 
        EntityName.Add(945, "alpha");
        EntityValue.Add("beta", 946); // greek small letter beta, U+03B2 ISOgrk3 
        EntityName.Add(946, "beta");
        EntityValue.Add("gamma", 947); // greek small letter gamma, U+03B3 ISOgrk3 
        EntityName.Add(947, "gamma");
        EntityValue.Add("delta", 948); // greek small letter delta, U+03B4 ISOgrk3 
        EntityName.Add(948, "delta");
        EntityValue.Add("epsilon", 949); // greek small letter epsilon, U+03B5 ISOgrk3 
        EntityName.Add(949, "epsilon");
        EntityValue.Add("zeta", 950); // greek small letter zeta, U+03B6 ISOgrk3 
        EntityName.Add(950, "zeta");
        EntityValue.Add("eta", 951); // greek small letter eta, U+03B7 ISOgrk3 
        EntityName.Add(951, "eta");
        EntityValue.Add("theta", 952); // greek small letter theta, U+03B8 ISOgrk3 
        EntityName.Add(952, "theta");
        EntityValue.Add("iota", 953); // greek small letter iota, U+03B9 ISOgrk3 
        EntityName.Add(953, "iota");
        EntityValue.Add("kappa", 954); // greek small letter kappa, U+03BA ISOgrk3 
        EntityName.Add(954, "kappa");
        EntityValue.Add("lambda", 955); // greek small letter lambda, U+03BB ISOgrk3 
        EntityName.Add(955, "lambda");
        EntityValue.Add("mu", 956); // greek small letter mu, U+03BC ISOgrk3 
        EntityName.Add(956, "mu");
        EntityValue.Add("nu", 957); // greek small letter nu, U+03BD ISOgrk3 
        EntityName.Add(957, "nu");
        EntityValue.Add("xi", 958); // greek small letter xi, U+03BE ISOgrk3 
        EntityName.Add(958, "xi");
        EntityValue.Add("omicron", 959); // greek small letter omicron, U+03BF NEW 
        EntityName.Add(959, "omicron");
        EntityValue.Add("pi", 960); // greek small letter pi, U+03C0 ISOgrk3 
        EntityName.Add(960, "pi");
        EntityValue.Add("rho", 961); // greek small letter rho, U+03C1 ISOgrk3 
        EntityName.Add(961, "rho");
        EntityValue.Add("sigmaf", 962); // greek small letter final sigma, U+03C2 ISOgrk3 
        EntityName.Add(962, "sigmaf");
        EntityValue.Add("sigma", 963); // greek small letter sigma, U+03C3 ISOgrk3 
        EntityName.Add(963, "sigma");
        EntityValue.Add("tau", 964); // greek small letter tau, U+03C4 ISOgrk3 
        EntityName.Add(964, "tau");
        EntityValue.Add("upsilon", 965); // greek small letter upsilon, U+03C5 ISOgrk3 
        EntityName.Add(965, "upsilon");
        EntityValue.Add("phi", 966); // greek small letter phi, U+03C6 ISOgrk3 
        EntityName.Add(966, "phi");
        EntityValue.Add("chi", 967); // greek small letter chi, U+03C7 ISOgrk3 
        EntityName.Add(967, "chi");
        EntityValue.Add("psi", 968); // greek small letter psi, U+03C8 ISOgrk3 
        EntityName.Add(968, "psi");
        EntityValue.Add("omega", 969); // greek small letter omega, U+03C9 ISOgrk3 
        EntityName.Add(969, "omega");
        EntityValue.Add("thetasym", 977); // greek small letter theta symbol, U+03D1 NEW 
        EntityName.Add(977, "thetasym");
        EntityValue.Add("upsih", 978); // greek upsilon with hook symbol, U+03D2 NEW 
        EntityName.Add(978, "upsih");
        EntityValue.Add("piv", 982); // greek pi symbol, U+03D6 ISOgrk3 
        EntityName.Add(982, "piv");
        EntityValue.Add("bull", 8226); // bullet = black small circle, U+2022 ISOpub 
        EntityName.Add(8226, "bull");
        EntityValue.Add("hellip", 8230); // horizontal ellipsis = three dot leader, U+2026 ISOpub 
        EntityName.Add(8230, "hellip");
        EntityValue.Add("prime", 8242); // prime = minutes = feet, U+2032 ISOtech 
        EntityName.Add(8242, "prime");
        EntityValue.Add("Prime", 8243); // double prime = seconds = inches, U+2033 ISOtech 
        EntityName.Add(8243, "Prime");
        EntityValue.Add("oline", 8254); // overline = spacing overscore, U+203E NEW 
        EntityName.Add(8254, "oline");
        EntityValue.Add("frasl", 8260); // fraction slash, U+2044 NEW 
        EntityName.Add(8260, "frasl");
        EntityValue.Add("weierp", 8472); // script capital P = power set = Weierstrass p, U+2118 ISOamso 
        EntityName.Add(8472, "weierp");
        EntityValue.Add("image", 8465); // blackletter capital I = imaginary part, U+2111 ISOamso 
        EntityName.Add(8465, "image");
        EntityValue.Add("real", 8476); // blackletter capital R = real part symbol, U+211C ISOamso 
        EntityName.Add(8476, "real");
        EntityValue.Add("trade", 8482); // trade mark sign, U+2122 ISOnum 
        EntityName.Add(8482, "trade");
        EntityValue.Add("alefsym", 8501); // alef symbol = first transfinite cardinal, U+2135 NEW 
        EntityName.Add(8501, "alefsym");
        EntityValue.Add("larr", 8592); // leftwards arrow, U+2190 ISOnum 
        EntityName.Add(8592, "larr");
        EntityValue.Add("uarr", 8593); // upwards arrow, U+2191 ISOnum
        EntityName.Add(8593, "uarr");
        EntityValue.Add("rarr", 8594); // rightwards arrow, U+2192 ISOnum 
        EntityName.Add(8594, "rarr");
        EntityValue.Add("darr", 8595); // downwards arrow, U+2193 ISOnum 
        EntityName.Add(8595, "darr");
        EntityValue.Add("harr", 8596); // left right arrow, U+2194 ISOamsa 
        EntityName.Add(8596, "harr");
        EntityValue.Add("crarr", 8629); // downwards arrow with corner leftwards = carriage return, U+21B5 NEW 
        EntityName.Add(8629, "crarr");
        EntityValue.Add("lArr", 8656); // leftwards double arrow, U+21D0 ISOtech 
        EntityName.Add(8656, "lArr");
        EntityValue.Add("uArr", 8657); // upwards double arrow, U+21D1 ISOamsa 
        EntityName.Add(8657, "uArr");
        EntityValue.Add("rArr", 8658); // rightwards double arrow, U+21D2 ISOtech 
        EntityName.Add(8658, "rArr");
        EntityValue.Add("dArr", 8659); // downwards double arrow, U+21D3 ISOamsa 
        EntityName.Add(8659, "dArr");
        EntityValue.Add("hArr", 8660); // left right double arrow, U+21D4 ISOamsa 
        EntityName.Add(8660, "hArr");
        EntityValue.Add("forall", 8704); // for all, U+2200 ISOtech 
        EntityName.Add(8704, "forall");
        EntityValue.Add("part", 8706); // partial differential, U+2202 ISOtech 
        EntityName.Add(8706, "part");
        EntityValue.Add("exist", 8707); // there exists, U+2203 ISOtech 
        EntityName.Add(8707, "exist");
        EntityValue.Add("empty", 8709); // empty set = null set = diameter, U+2205 ISOamso 
        EntityName.Add(8709, "empty");
        EntityValue.Add("nabla", 8711); // nabla = backward difference, U+2207 ISOtech 
        EntityName.Add(8711, "nabla");
        EntityValue.Add("isin", 8712); // element of, U+2208 ISOtech 
        EntityName.Add(8712, "isin");
        EntityValue.Add("notin", 8713); // not an element of, U+2209 ISOtech 
        EntityName.Add(8713, "notin");
        EntityValue.Add("ni", 8715); // contains as member, U+220B ISOtech 
        EntityName.Add(8715, "ni");
        EntityValue.Add("prod", 8719); // n-ary product = product sign, U+220F ISOamsb 
        EntityName.Add(8719, "prod");
        EntityValue.Add("sum", 8721); // n-ary sumation, U+2211 ISOamsb 
        EntityName.Add(8721, "sum");
        EntityValue.Add("minus", 8722); // minus sign, U+2212 ISOtech 
        EntityName.Add(8722, "minus");
        EntityValue.Add("lowast", 8727); // asterisk operator, U+2217 ISOtech 
        EntityName.Add(8727, "lowast");
        EntityValue.Add("radic", 8730); // square root = radical sign, U+221A ISOtech 
        EntityName.Add(8730, "radic");
        EntityValue.Add("prop", 8733); // proportional to, U+221D ISOtech 
        EntityName.Add(8733, "prop");
        EntityValue.Add("infin", 8734); // infinity, U+221E ISOtech 
        EntityName.Add(8734, "infin");
        EntityValue.Add("ang", 8736); // angle, U+2220 ISOamso 
        EntityName.Add(8736, "ang");
        EntityValue.Add("and", 8743); // logical and = wedge, U+2227 ISOtech 
        EntityName.Add(8743, "and");
        EntityValue.Add("or", 8744); // logical or = vee, U+2228 ISOtech 
        EntityName.Add(8744, "or");
        EntityValue.Add("cap", 8745); // intersection = cap, U+2229 ISOtech 
        EntityName.Add(8745, "cap");
        EntityValue.Add("cup", 8746); // union = cup, U+222A ISOtech 
        EntityName.Add(8746, "cup");
        EntityValue.Add("int", 8747); // integral, U+222B ISOtech 
        EntityName.Add(8747, "int");
        EntityValue.Add("there4", 8756); // therefore, U+2234 ISOtech 
        EntityName.Add(8756, "there4");
        EntityValue.Add("sim", 8764); // tilde operator = varies with = similar to, U+223C ISOtech 
        EntityName.Add(8764, "sim");
        EntityValue.Add("cong", 8773); // approximately equal to, U+2245 ISOtech 
        EntityName.Add(8773, "cong");
        EntityValue.Add("asymp", 8776); // almost equal to = asymptotic to, U+2248 ISOamsr 
        EntityName.Add(8776, "asymp");
        EntityValue.Add("ne", 8800); // not equal to, U+2260 ISOtech 
        EntityName.Add(8800, "ne");
        EntityValue.Add("equiv", 8801); // identical to, U+2261 ISOtech 
        EntityName.Add(8801, "equiv");
        EntityValue.Add("le", 8804); // less-than or equal to, U+2264 ISOtech 
        EntityName.Add(8804, "le");
        EntityValue.Add("ge", 8805); // greater-than or equal to, U+2265 ISOtech 
        EntityName.Add(8805, "ge");
        EntityValue.Add("sub", 8834); // subset of, U+2282 ISOtech 
        EntityName.Add(8834, "sub");
        EntityValue.Add("sup", 8835); // superset of, U+2283 ISOtech 
        EntityName.Add(8835, "sup");
        EntityValue.Add("nsub", 8836); // not a subset of, U+2284 ISOamsn 
        EntityName.Add(8836, "nsub");
        EntityValue.Add("sube", 8838); // subset of or equal to, U+2286 ISOtech 
        EntityName.Add(8838, "sube");
        EntityValue.Add("supe", 8839); // superset of or equal to, U+2287 ISOtech 
        EntityName.Add(8839, "supe");
        EntityValue.Add("oplus", 8853); // circled plus = direct sum, U+2295 ISOamsb 
        EntityName.Add(8853, "oplus");
        EntityValue.Add("otimes", 8855); // circled times = vector product, U+2297 ISOamsb 
        EntityName.Add(8855, "otimes");
        EntityValue.Add("perp", 8869); // up tack = orthogonal to = perpendicular, U+22A5 ISOtech 
        EntityName.Add(8869, "perp");
        EntityValue.Add("sdot", 8901); // dot operator, U+22C5 ISOamsb 
        EntityName.Add(8901, "sdot");
        EntityValue.Add("lceil", 8968); // left ceiling = apl upstile, U+2308 ISOamsc 
        EntityName.Add(8968, "lceil");
        EntityValue.Add("rceil", 8969); // right ceiling, U+2309 ISOamsc 
        EntityName.Add(8969, "rceil");
        EntityValue.Add("lfloor", 8970); // left floor = apl downstile, U+230A ISOamsc 
        EntityName.Add(8970, "lfloor");
        EntityValue.Add("rfloor", 8971); // right floor, U+230B ISOamsc 
        EntityName.Add(8971, "rfloor");
        EntityValue.Add("lang", 9001); // left-pointing angle bracket = bra, U+2329 ISOtech 
        EntityName.Add(9001, "lang");
        EntityValue.Add("rang", 9002); // right-pointing angle bracket = ket, U+232A ISOtech 
        EntityName.Add(9002, "rang");
        EntityValue.Add("loz", 9674); // lozenge, U+25CA ISOpub 
        EntityName.Add(9674, "loz");
        EntityValue.Add("spades", 9824); // black spade suit, U+2660 ISOpub 
        EntityName.Add(9824, "spades");
        EntityValue.Add("clubs", 9827); // black club suit = shamrock, U+2663 ISOpub 
        EntityName.Add(9827, "clubs");
        EntityValue.Add("hearts", 9829); // black heart suit = valentine, U+2665 ISOpub 
        EntityName.Add(9829, "hearts");
        EntityValue.Add("diams", 9830); // black diamond suit, U+2666 ISOpub 
        EntityName.Add(9830, "diams");
        EntityValue.Add("OElig", 338); // latin capital ligature OE, U+0152 ISOlat2 
        EntityName.Add(338, "OElig");
        EntityValue.Add("oelig", 339); // latin small ligature oe, U+0153 ISOlat2 
        EntityName.Add(339, "oelig");
        EntityValue.Add("Scaron", 352); // latin capital letter S with caron, U+0160 ISOlat2 
        EntityName.Add(352, "Scaron");
        EntityValue.Add("scaron", 353); // latin small letter s with caron, U+0161 ISOlat2 
        EntityName.Add(353, "scaron");
        EntityValue.Add("Yuml", 376); // latin capital letter Y with diaeresis, U+0178 ISOlat2 
        EntityName.Add(376, "Yuml");
        EntityValue.Add("circ", 710); // modifier letter circumflex accent, U+02C6 ISOpub 
        EntityName.Add(710, "circ");
        EntityValue.Add("tilde", 732); // small tilde, U+02DC ISOdia 
        EntityName.Add(732, "tilde");
        EntityValue.Add("ensp", 8194); // en space, U+2002 ISOpub 
        EntityName.Add(8194, "ensp");
        EntityValue.Add("emsp", 8195); // em space, U+2003 ISOpub 
        EntityName.Add(8195, "emsp");
        EntityValue.Add("thinsp", 8201); // thin space, U+2009 ISOpub 
        EntityName.Add(8201, "thinsp");
        EntityValue.Add("zwnj", 8204); // zero width non-joiner, U+200C NEW RFC 2070 
        EntityName.Add(8204, "zwnj");
        EntityValue.Add("zwj", 8205); // zero width joiner, U+200D NEW RFC 2070 
        EntityName.Add(8205, "zwj");
        EntityValue.Add("lrm", 8206); // left-to-right mark, U+200E NEW RFC 2070 
        EntityName.Add(8206, "lrm");
        EntityValue.Add("rlm", 8207); // right-to-left mark, U+200F NEW RFC 2070 
        EntityName.Add(8207, "rlm");
        EntityValue.Add("ndash", 8211); // en dash, U+2013 ISOpub 
        EntityName.Add(8211, "ndash");
        EntityValue.Add("mdash", 8212); // em dash, U+2014 ISOpub 
        EntityName.Add(8212, "mdash");
        EntityValue.Add("lsquo", 8216); // left single quotation mark, U+2018 ISOnum 
        EntityName.Add(8216, "lsquo");
        EntityValue.Add("rsquo", 8217); // right single quotation mark, U+2019 ISOnum 
        EntityName.Add(8217, "rsquo");
        EntityValue.Add("sbquo", 8218); // single low-9 quotation mark, U+201A NEW 
        EntityName.Add(8218, "sbquo");
        EntityValue.Add("ldquo", 8220); // left double quotation mark, U+201C ISOnum 
        EntityName.Add(8220, "ldquo");
        EntityValue.Add("rdquo", 8221); // right double quotation mark, U+201D ISOnum 
        EntityName.Add(8221, "rdquo");
        EntityValue.Add("bdquo", 8222); // double low-9 quotation mark, U+201E NEW 
        EntityName.Add(8222, "bdquo");
        EntityValue.Add("dagger", 8224); // dagger, U+2020 ISOpub 
        EntityName.Add(8224, "dagger");
        EntityValue.Add("Dagger", 8225); // double dagger, U+2021 ISOpub 
        EntityName.Add(8225, "Dagger");
        EntityValue.Add("permil", 8240); // per mille sign, U+2030 ISOtech 
        EntityName.Add(8240, "permil");
        EntityValue.Add("lsaquo", 8249); // single left-pointing angle quotation mark, U+2039 ISO proposed 
        EntityName.Add(8249, "lsaquo");
        EntityValue.Add("rsaquo", 8250); // single right-pointing angle quotation mark, U+203A ISO proposed 
        EntityName.Add(8250, "rsaquo");
        EntityValue.Add("euro", 8364); // euro sign, U+20AC NEW 
        EntityName.Add(8364, "euro");

        _maxEntitySize = 8 + 1; // we add the # char
        #endregion
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Replace known entities by characters.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <returns>The result text.</returns>
    public static string Decode(string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            return string.Empty;
        }

        if (UseWebUtility) {
            return System.Net.WebUtility.HtmlDecode(text);
        }

        StringBuilder sb = new(text.Length);
        ParseState state = ParseState.Text;
        StringBuilder entity = new(10);

        for (int i = 0; i < text.Length; i++) {
            switch (state) {
                case ParseState.Text:
                    switch (text[i]) {
                        case '&':
                            state = ParseState.EntityStart;
                            break;

                        default:
                            sb.Append(text[i]);
                            break;
                    }

                    break;

                case ParseState.EntityStart:
                    switch (text[i]) {
                        case ';':
                            if (entity.Length == 0) {
                                sb.Append("&;");
                            }
                            else {
                                if (entity[0] == '#') {
                                    string e = entity.ToString();
                                    try {
                                        string codeStr = e[1..].Trim();
                                        int fromBase;
                                        if (codeStr.StartsWith("x", StringComparison.OrdinalIgnoreCase)) {
                                            fromBase = 16;
                                            codeStr = codeStr[1..];
                                        }
                                        else {
                                            fromBase = 10;
                                        }

                                        int code = Convert.ToInt32(codeStr, fromBase);
                                        sb.Append(Convert.ToChar(code));
                                    }
                                    catch {
                                        sb.Append("&#").Append(e).Append(';');
                                    }
                                }
                                else {
                                    // named entity?
                                    if (!EntityValue.TryGetValue(entity.ToString(), out int code)) {
                                        // nope
                                        sb.Append('&').Append(entity).Append(';');
                                    }
                                    else {
                                        // we found one
                                        sb.Append(Convert.ToChar(code));
                                    }
                                }

                                entity.Remove(0, entity.Length);
                            }

                            state = ParseState.Text;
                            break;

                        case '&':
                            // new entity start without end, it was not an entity...
                            sb.Append('&').Append(entity);
                            entity.Remove(0, entity.Length);
                            break;

                        default:
                            entity.Append(text[i]);
                            if (entity.Length > _maxEntitySize) {
                                // unknown stuff, just don't touch it
                                state = ParseState.Text;
                                sb.Append('&').Append(entity);
                                entity.Remove(0, entity.Length);
                            }

                            break;
                    }

                    break;
            }
        }

        // finish the work
        if (state == ParseState.EntityStart) {
            sb.Append('&').Append(entity);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Replace characters above 127 by entities.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <returns>The result text.</returns>
    public static string Encode(string text) {
        return Encode(text, true);
    }

    /// <summary>
    /// Replace characters above 127 by entities.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
    /// <returns>The result text.</returns>
    public static string Encode(string text, bool useNames) {
        return Encode(text, useNames, false);
    }

    /// <summary>
    /// Replace characters above 127 by entities.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
    /// <param name="entitizeQuotAmpAndLtGt">If set to true, the [quote], [ampersand], [lower than] and [greather than] characters will be entitized.</param>
    /// <returns>The result text</returns>
    public static string Encode(string text, bool useNames, bool entitizeQuotAmpAndLtGt) {
        //        _entityValue.Add("quot", 34);    // quotation mark = APL quote, U+0022 ISOnum 
        //        _entityName.Add(34, "quot");
        //        _entityValue.Add("amp", 38);    // ampersand, U+0026 ISOnum 
        //        _entityName.Add(38, "amp");
        //        _entityValue.Add("lt", 60);    // less-than sign, U+003C ISOnum 
        //        _entityName.Add(60, "lt");
        //        _entityValue.Add("gt", 62);    // greater-than sign, U+003E ISOnum 
        //        _entityName.Add(62, "gt");
        if (string.IsNullOrWhiteSpace(text)) {
            return string.Empty;
        }

        StringBuilder sb = new(text.Length);

        if (UseWebUtility) {
            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
            while (enumerator.MoveNext()) {
                sb.Append(System.Net.WebUtility.HtmlEncode(enumerator.GetTextElement()));
            }
        }
        else {
            for (int i = 0; i < text.Length; i++) {
                int code = text[i];
                if ((code > 127) ||
                    (entitizeQuotAmpAndLtGt && ((code == 34) || (code == 38) || (code == 60) || (code == 62)))) {
                    string? entity = null;

                    if (useNames) {
                        EntityName.TryGetValue(code, out entity);
                    }

                    if (entity == null) {
                        sb.Append("&#").Append(code).Append(';');
                    }
                    else {
                        sb.Append('&').Append(entity).Append(';');
                    }
                }
                else {
                    sb.Append(text[i]);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Replaces known Html character codes with their proper values from a <see cref="HtmlNode"/>s text node.
    /// </summary>
    /// <param name="node">The source node.</param>
    /// <returns>The decoded text.</returns>
    public static string Decode(HtmlNode node) {
        return Decode(node.Text);
    }
    #endregion

    #region Nested type: ParseState
    private enum ParseState {
        Text,
        EntityStart
    }
    #endregion
}
