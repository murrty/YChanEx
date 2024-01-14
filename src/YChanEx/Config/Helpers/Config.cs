#nullable enable
namespace YChanEx;
using System.Drawing;
public static class Config {
    internal static readonly Point InvalidPoint = new(-32_000, -32_000);

    /// <summary>
    /// Checks if a point is a valid one to use.
    /// </summary>
    /// <param name="input">The <seealso cref="Point"/> value to validate.</param>
    /// <returns>If the input is a valid point.</returns>
    public static bool ValidPoint(Point input) {
        return input.X != -32000 && input.Y != -32000;
    }

    /// <summary>
    /// Checks if a size is a valid one to use.
    /// </summary>
    /// <param name="input">The <seealso cref="Size"/> value to validate.</param>
    /// <returns>If the input is a valid size.</returns>
    public static bool ValidSize(Size input) {
        return input.Width > 0 && input.Height > 0;
    }
}