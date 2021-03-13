using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YChanEx {
    
    #region HintTextBox
    [System.Diagnostics.DebuggerStepThrough]
    public class HintTextBox : TextBox {
        private string TextHintString = string.Empty;

        public HintTextBox() { }

        public string TextHint {
            get { return this.TextHintString; }
            set {
                this.TextHintString = value;
                this.Hint(value);
            }
        }

        void Hint(string HintString) {
            NativeMethods.SendMessage(this.Handle, 0x1501, (IntPtr)1, HintString);
        }
    }
    #endregion

    #region VistaListView
    [System.Diagnostics.DebuggerStepThrough]
    public class VistaListView : ListView {
        private bool UseVistaStyle = true;
        public VistaListView() {
            this.View = System.Windows.Forms.View.Details;
        }

        public bool EnableVistaView {
            get { return this.UseVistaStyle; }
            set {
                this.UseVistaStyle = value;
                if (value) {
                    NativeMethods.SetWindowTheme(this.Handle, "Eplorer", null);
                }
                else {
                    NativeMethods.SetWindowTheme(this.Handle, null, null);
                }
            }
        }
    }
    #endregion

    #region LinkLabelCleanHandCursor
    public class LinkLabelCleanHandCursor : LinkLabel {

        [System.Diagnostics.DebuggerStepThrough]
        protected override void WndProc(ref Message m) {
            if (m.Msg == 0x0020) {
                NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, (IntPtr)32649));
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }
    }
    #endregion

    #region SplitButton
    public delegate void DropDownClicked();
    
    public class SplitButton : Button {
        [System.Diagnostics.DebuggerStepThrough]
        public SplitButton() {
            this.FlatStyle = FlatStyle.System;
            this.DropDown_Clicked += new DropDownClicked(this.LaunchMenu);
            this.setDropDownContextMenu.Collapse += new EventHandler(this.CloseMenuDropdown);
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cParams = base.CreateParams;
                cParams.Style |= 0x0000000C;
                return cParams;
            }
        }

        public int IsBumped = 0;
        public int IsMouseDown = 0;
        public int IsAtDropDown = 0;
        public int DropDownPushed = 0;

        [System.Diagnostics.DebuggerStepThrough]
        protected override void WndProc(ref Message WndMessage) {
            switch (WndMessage.Msg) {
                case (0x00001600 + 0x0006):
                    if (WndMessage.HWnd == this.Handle) {
                        if (WndMessage.WParam.ToString() == "1") {
                            if (DropDownPushed == 0) {
                                this.DropDownPushed = 1;
                                DropDown_Clicked();
                            }
                        }
                        if (this.IsMouseDown == 1) {
                            this.IsAtDropDown = 1;
                        }
                    }
                    break;
                case (0x0F):
                    if (this.DropDownPushed == 1) {
                        this.SetDropDownState(1);
                    }
                    break;
                case (0x201):
                    this.IsMouseDown = 1;
                    break;
                case (0x2A3):
                    if (this.IsAtDropDown == 1) {
                        this.SetDropDownState(0);
                        this.IsAtDropDown = 0;
                        this.IsMouseDown = 0;
                    }
                    break;
                case (0x202):
                    if (this.IsAtDropDown == 1) {
                        this.SetDropDownState(0);
                        this.IsAtDropDown = 0;
                        this.IsMouseDown = 0;
                    }
                    break;
                case (0x08):
                    if (this.IsAtDropDown == 1) {
                        this.SetDropDownState(0);
                        this.IsAtDropDown = 0;
                        this.IsMouseDown = 0;
                    }
                    break;
            }
            base.WndProc(ref WndMessage);
        }
        public void SetDropDownState(int Pushed) {
            if (Pushed == 0) { this.DropDownPushed = 0; }
            NativeMethods.SendMessage(this.Handle, (0x1600 + 0x0006), (IntPtr)Pushed, IntPtr.Zero);
        }
        public event DropDownClicked DropDown_Clicked;
        private void InitializeComponent() {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        public void LaunchMenu() {
            if (setDropDownContextMenu.MenuItems.Count > 0) {
                this.setDropDownContextMenu.Show(this, new System.Drawing.Point(this.Width + 190, this.Height), LeftRightAlignment.Left);
            }
            else if (setDropDownContextMenuStrip.Items.Count > 0) {
                this.setDropDownContextMenuStrip.Show(this, this.Width - 25, this.Height);
            }
        }

        public void CloseMenuDropdown(object sender, EventArgs e) {
            this.SetDropDownState(0);
        }

        private ContextMenu setDropDownContextMenu = new ContextMenu();
        public ContextMenu DropDownContextMenu {
            get { return setDropDownContextMenu; }
            set { setDropDownContextMenu = value; }
        }

        private ContextMenuStrip setDropDownContextMenuStrip = new ContextMenuStrip();
        public ContextMenuStrip DropDownConextMenuStrip {
            get { return setDropDownContextMenuStrip; }
            set { setDropDownContextMenuStrip = value; }
        }
    }
    #endregion

    class WebClientMethod : WebClient {
        public string Method { get; set; }

        protected override WebRequest GetWebRequest(Uri address) {
            WebRequest Request = base.GetWebRequest(address);
            if (!string.IsNullOrEmpty(Method))
                Request.Method = Method;

            return Request;
        }
    }
}
