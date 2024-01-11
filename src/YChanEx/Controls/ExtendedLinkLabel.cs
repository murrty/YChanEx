namespace murrty.controls;
using System.Windows.Forms;
internal class ExtendedLinkLabel : LinkLabel {
    /// <summary>
    /// Constructor
    /// </summary>
    public ExtendedLinkLabel() {
        this.LinkColor = System.Drawing.Color.FromArgb(0x00, 0x66, 0xCC);
        this.VisitedLinkColor = System.Drawing.Color.FromArgb(0x80, 0x00, 0x80);
        this.ActiveLinkColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00);
    }

    [System.Diagnostics.DebuggerStepThrough]
    protected override void WndProc(ref Message m) {
        switch (m.Msg) {
            case NativeMethods.WM_SETCURSOR: {
                NativeMethods.SetCursor(NativeMethods.HandCursor);
                m.Result = IntPtr.Zero;
            } break;

            default: {
                base.WndProc(ref m);
            } break;
        }
    }
}