#nullable enable
namespace murrty.classes;
/// <summary>
/// An implementation of DrawThemeTextEx which supports setting color, glow, and other stuff.
/// Too bad I'm only 14 years late. This would have looked so cool on 7.
/// </summary>
internal static class DwmComposition {
    /// <summary>
    /// Gets whether the composition is supported.
    /// </summary>
    public static bool CompositionSupported {
        get {
            bool CompositionEnabled = false;
            _ = DwmNatives.DwmIsCompositionEnabled(ref CompositionEnabled);
            return Environment.OSVersion.Version.Major >= 6 && CompositionEnabled;
        }
    }

    /// <summary>
    /// Extends the dwm frame into the client area.
    /// </summary>
    /// <param name="DwmInfo">The composition info.</param>
    internal static void ExtendFrame(DwmCompositionInfo DwmInfo) {
        DwmNatives.DwmExtendFrameIntoClientArea(DwmInfo.hWnd, ref DwmInfo.Margins);
    }

    /// <summary>
    /// Enables the form movement function. Be sure to check the mouse button, or this will be funky.
    /// </summary>
    /// <param name="DwmInfo">The composition info.</param>
    internal static void MoveForm(DwmCompositionInfo DwmInfo) {
        _ = DwmNatives.ReleaseCapture();
        _ = DwmNatives.SendMessage(DwmInfo.hWnd, 0xA1, 0x2, 0x0);
    }

    /// <summary>
    /// Fills a region with dwm-aware black, used for rendering the desktop window managers' composition onto the form.
    /// </summary>
    /// <param name="DwmInfo">The composition info.</param>
    public static void FillBlackRegion(DwmCompositionInfo DwmInfo) {
        nint Memdc = DwmNatives.CreateCompatibleDC(DwmInfo.destdc);
        if (DwmNatives.SaveDC(Memdc) != 0) {
            nint bitmap = DwmNatives.CreateDIBSection(Memdc, ref DwmInfo.dib, DwmNatives.DIB_RGB_COLORS, 0, 0, 0);
            if (bitmap != 0) {
                nint bitmapOld = DwmNatives.SelectObject(Memdc, bitmap);
                try {
                    _ = DwmNatives.BitBlt(DwmInfo.destdc, DwmInfo.Rect.left, DwmInfo.Rect.top, DwmInfo.Rect.right - DwmInfo.Rect.left, DwmInfo.Rect.bottom - DwmInfo.Rect.top, Memdc, 0, 0, DwmNatives.SRCCOPY);
                }
                finally {
                    //Remember to clean up
                    _ = DwmNatives.SelectObject(Memdc, bitmapOld);
                    _ = DwmNatives.DeleteObject(bitmap);
                    _ = DwmNatives.ReleaseDC(Memdc, -1);
                    _ = DwmNatives.DeleteDC(Memdc);
                }
            }
        }
        //gph.ReleaseHdc();
    }

    /// <summary>
    /// Draws text onto the DWM composition.
    /// </summary>
    /// <param name="DwmInfo">The composition info.</param>
    /// <param name="TxtInfo">The <see cref="DwmCompositionInfo"/> object that contains information used to render the text.</param>
    public static void DrawTextOnGlass(DwmCompositionInfo DwmInfo, DwmCompositionTextInfo TxtInfo) {
        nint Memdc = DwmNatives.CreateCompatibleDC(DwmInfo.destdc); // Set up a memory DC where we'll draw the text.
        if (DwmNatives.SaveDC(Memdc) != 0) {
            nint bitmap = DwmNatives.CreateDIBSection(Memdc, ref TxtInfo.BitmapInfo, DwmNatives.DIB_RGB_COLORS, 0, 0, 0); // Create a 32-bit bmp for use in offscreen drawing when glass is on
            if (bitmap != 0) {
                nint bitmapOld = DwmNatives.SelectObject(Memdc, bitmap);
                nint hFont = TxtInfo.Font.ToHfont();
                nint logfnotOld = DwmNatives.SelectObject(Memdc, hFont);
                try {
                    _ = DwmNatives.DrawThemeTextEx(TxtInfo.renderer.Handle, Memdc, 0, 0, TxtInfo.Text, -1, TxtInfo.uFormat, ref TxtInfo.Rect2, ref TxtInfo.dttOpts);
                    _ = DwmNatives.BitBlt(DwmInfo.destdc, TxtInfo.Rect1.left, TxtInfo.Rect1.top, TxtInfo.Rect1.right - TxtInfo.Rect1.left, TxtInfo.Rect1.bottom - TxtInfo.Rect1.top, Memdc, 0, 0, DwmNatives.SRCCOPY);
                }
                catch {
                    throw;
                }
                finally {
                    _ = DwmNatives.SelectObject(Memdc, bitmapOld);
                    _ = DwmNatives.SelectObject(Memdc, logfnotOld);
                    _ = DwmNatives.DeleteObject(bitmap);
                    _ = DwmNatives.DeleteObject(hFont);
                    _ = DwmNatives.ReleaseDC(Memdc, -1);
                    _ = DwmNatives.DeleteDC(Memdc);
                }
            }
        }
    }
}
