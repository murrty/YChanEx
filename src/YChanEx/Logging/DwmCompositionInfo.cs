namespace murrty.classes;

using System.Drawing;
using System.Runtime.InteropServices;

/// <summary>
/// A class that contains information about how the dwm will composite on the form
/// </summary>
internal sealed class DwmCompositionInfo {
    /// <summary>
    /// The handle that will be written to.
    /// </summary>
    public IntPtr hWnd { get; }

    /// <summary>
    /// The rectangle where the dwm composition will be drawn at.
    /// </summary>
    public Rectangle DwmRectangle;

    /// <summary>
    /// Contains relevant information about the positioning of the frame.
    /// </summary>
    public DwmNatives.MARGINS Margins;

    /// <summary>
    /// Contains the text that will be rendered.
    /// </summary>
    public DwmCompositionTextInfo Text;

    /// <summary>
    /// Device context handle.
    /// </summary>
    public IntPtr destdc;

    /// <summary>
    /// Rect struct to render composition at.
    /// </summary>
    public DwmNatives.RECT Rect;

    /// <summary>
    /// DIB bitmap for the dwm composition.
    /// </summary>
    public DwmNatives.BITMAPINFO dib;

    public DwmCompositionInfo(IntPtr hWnd, DwmNatives.MARGINS Margins, Rectangle DwmRectangle) {
        this.hWnd = hWnd;
        this.Margins = Margins;
        this.DwmRectangle = DwmRectangle;

        GenerateValues();
    }

    public DwmCompositionInfo(IntPtr hWnd, DwmNatives.MARGINS Margins, Rectangle DwmRectangle, DwmCompositionTextInfo NewInfo) {
        this.hWnd = hWnd;
        this.Margins = Margins;
        this.DwmRectangle = DwmRectangle;
        this.Text = NewInfo;

        GenerateValues();
    }

    /// <summary>
    /// Generates used-values for rendering. Created to save lines when generating dwm info with or without text.
    /// </summary>
    private void GenerateValues() {
        this.destdc = DwmNatives.GetDC(hWnd);

        this.Rect = new() {
            top = DwmRectangle.Top,
            bottom = DwmRectangle.Bottom,
            left = DwmRectangle.Left,
            right = DwmRectangle.Right
        };

        this.dib = new();
        this.dib.bmiHeader.biHeight = -(Rect.bottom - Rect.top);
        this.dib.bmiHeader.biWidth = Rect.right - Rect.left;
        this.dib.bmiHeader.biPlanes = 1;
        this.dib.bmiHeader.biSize = Marshal.SizeOf(typeof(DwmNatives.BITMAPINFOHEADER));
        this.dib.bmiHeader.biBitCount = 32;
        this.dib.bmiHeader.biCompression = DwmNatives.BI_RGB;
    }
}
