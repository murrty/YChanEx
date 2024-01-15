namespace murrty.controls;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
internal sealed class ExplorerTreeView : TreeView {
    private RECT BorderRect;

    protected override void OnHandleCreated(EventArgs e) {
        base.OnHandleCreated(e);
        _ = NativeMethods.SetWindowTheme(this.Handle, "Explorer", null);
    }
    protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            this.SelectedNode = e.Node;
        }
        base.OnNodeMouseClick(e);
    }
    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        _ = NativeMethods.SendMessage(this.Handle, 0x0085, 0, 0);
    }
    protected override void OnLostFocus(EventArgs e) {
        base.OnLostFocus(e);
        _ = NativeMethods.SendMessage(this.Handle, 0x0085, 0, 0);
    }
    protected override void WndProc(ref Message m) {
        switch (m.Msg) {
            // WM_NCPAINT
            case 0x0085: {
                base.WndProc(ref m);

                if (System.Windows.Forms.VisualStyles.VisualStyleInformation.IsEnabledByUser) {
                    RECT r = RECT.Empty;
                    _ = NativeMethods.GetWindowRect(Handle, ref r);
                    r.right -= r.left + 1;
                    r.bottom -= r.top + 1;
                    r.top = 1;
                    r.left = 1;

                    r.left += BorderRect.left;
                    r.top += BorderRect.top;
                    r.right -= BorderRect.right;
                    r.bottom -= BorderRect.bottom;

                    nint hDc = NativeMethods.GetWindowDC(Handle);
                    _ = NativeMethods.ExcludeClipRect(hDc, r.left, r.top, r.right, r.bottom);

                    using Graphics g = Graphics.FromHdc(hDc);
                    Color DrawColor = Enabled ?
                        (Focused ? SystemColors.ControlDark : SystemColors.ControlLight) :
                        SystemColors.ControlDarkDark;
                    g.Clear(DrawColor);

                    _ = NativeMethods.ReleaseDC(Handle, hDc);
                }

                m.Result = IntPtr.Zero;
            } break;

            // WM_NCCALCSIZE
            case 0x0083: {
                base.WndProc(ref m);
                if (!System.Windows.Forms.VisualStyles.VisualStyleInformation.IsEnabledByUser) {
                    return;
                }

                NCCALCSIZE_PARAMS param = new();
                RECT winRect = RECT.Empty;

                if (m.WParam != IntPtr.Zero) {
                    param = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);
                    winRect = param.rgrc0;
                }

                RECT clientRect = winRect;
                // 2px border
                //clientRect.left += 2;
                //clientRect.top += 2;
                //clientRect.right -= 2;
                //clientRect.bottom -= 2;

                // 1px border
                //clientRect.left++;
                //clientRect.top++;
                //clientRect.right--;
                //clientRect.bottom--;

                BorderRect = new(clientRect.left - winRect.left,
                    clientRect.top - winRect.top,
                    winRect.right - clientRect.right,
                    winRect.bottom - clientRect.bottom);

                if (m.WParam == IntPtr.Zero) {
                    Marshal.StructureToPtr(clientRect, m.LParam, false);
                }
                else {
                    param.rgrc0 = clientRect;
                    Marshal.StructureToPtr(param, m.LParam, false);
                }

                const int WVR_HREDRAW = 0x100;
                const int WVR_VREDRAW = 0x200;
                const int WVR_REDRAW = WVR_HREDRAW | WVR_VREDRAW;

                m.Result = (IntPtr)WVR_REDRAW;
            } break;

            // WM_THEMECHANGED
            case 0x31A: {
                base.WndProc(ref m);
                UpdateStyles();
            } break;

            default: {
                base.WndProc(ref m);
            } break;
        }
    }
}
