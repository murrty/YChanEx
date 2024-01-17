#nullable enable
namespace YChanEx.Parsers.Helpers;
using System.Text.RegularExpressions;
internal static class ParsersShared {
    private const int MaximumThumbnailSize = 150;
    private const int MaximumOpThumbnailSize = 250;

    // Regex
    internal static readonly Regex RepliesRegex = new("href=\"#p\\d+\"", RegexOptions.IgnoreCase);
    internal static readonly Regex RepliesHtmlRegex = new("href=\"/[a-zA-Z0-9_]+/res/\\d+\\.html#\\d+\"", RegexOptions.IgnoreCase);
    internal static readonly Regex RepliesSimpleRegex = new(@">>\d+", RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the extension of a file.
    /// </summary>
    public static string GetExtension(string file) {
        int index = file.LastIndexOf('.');
        if (index < 0) {
            return file;
        }
        return file[(index + 1)..];
    }
    /// <summary>
    /// Gets the extension of a file and outs the index of the char.
    /// </summary>
    public static string GetExtension(string file, out int index) {
        index = file.LastIndexOf(".");
        if (index < 0) {
            return file;
        }
        return file[(index + 1)..];
    }
    /// <summary>
    /// Gets the name of the file without extension.
    /// </summary>
    public static string GetNameWithoutExtension(string file) {
        int index = file.LastIndexOf('.');
        if (index < 0) {
            return file;
        }
        return file[..index];
    }
    /// <summary>
    /// Gets the name of the file without extension and outs the index of the char.
    /// </summary>
    public static string GetNameWithoutExtension(string file, out int index) {
        index = file.LastIndexOf('.');
        return file[..index];
    }

    /// <summary>
    /// Gets the file name (minus extension) from a url.
    /// </summary>
    public static string GetFileNameFromUrl(string url) {
        int lastIndex = url.LastIndexOf('.');
        int lastPathIndex = url.LastIndexOf('/');

        if (lastIndex < 0) {
            if (lastPathIndex < 0) {
                return url;
            }
            return url[(lastPathIndex + 1)..];
        }

        if (lastPathIndex < 0) {
            return url[..lastIndex];
        }
        return url[(lastPathIndex + 1)..lastIndex];
    }
    /// <summary>
    /// Gets the file name (minus extension) from a url.
    /// </summary>
    public static string GetFileNameFromUrl(string url, int startingIndex) {
        int lastIndex = url.IndexOf('.', startingIndex);
        int lastPathIndex = url.IndexOf('/', startingIndex);

        if (lastIndex < 0) {
            if (lastPathIndex < 0) {
                return url;
            }
            return url[(lastPathIndex + 1)..];
        }

        if (lastPathIndex < 0) {
            return url[..lastIndex];
        }

        return url[(lastPathIndex + 1)..lastIndex];
    }
    /// <summary>
    /// Gets the file name and extension from a url.
    /// </summary>
    public static string GetFileNameAndExtFromUrl(string url) {
        int lastPathIndex = url.LastIndexOf('/');

        if (lastPathIndex < 0) {
            return url;
        }

        return url[(lastPathIndex + 1)..];
    }
    /// <summary>
    /// Gets the file name and extension from a url.
    /// </summary>
    public static string GetFileNameAndExtFromUrl(string url, int startingIndex) {
        int lastPathIndex = url.IndexOf('/', startingIndex);

        if (lastPathIndex < 0) {
            return url;
        }

        return url[(lastPathIndex + 1)..];
    }

    /// <summary>
    /// Calculates an MD5 hash from a byte array.
    /// </summary>
    public static string CalculateMd5(byte[] bytes) {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var calculated = md5.ComputeHash(bytes);
        return BitConverter.ToString(calculated).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// Gets the thumbnail size, biased to height limited to 250 pixels.
    /// </summary>
    /// <param name="Width">The width of the file.</param>
    /// <param name="Height">The height of the file.</param>
    /// <param name="FirstPost">Whether the file is the first post. First posts have a larger thumbnail size.</param>
    /// <returns>A size of the expected thumbnail size.</returns>
    public static System.Drawing.Size GetThumbnailSize(int Width, int Height, bool FirstPost) {
        if (Height > MaximumThumbnailSize) {
            decimal rnd = (FirstPost ? MaximumOpThumbnailSize : MaximumThumbnailSize) / (decimal)Height;
            Width = (int)Math.Round(Width * rnd);
            Height = (int)Math.Round(Height * rnd);
        }
        else if (Width > MaximumThumbnailSize) {
            decimal rnd = (FirstPost ? MaximumOpThumbnailSize : MaximumThumbnailSize) / (decimal)Width;
            Width = (int)Math.Round(Width * rnd);
            Height = (int)Math.Round(Height * rnd);
        }
        return new(Width, Height);
    }
}
