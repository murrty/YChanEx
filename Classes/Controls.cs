using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YChanEx {
    [System.Diagnostics.DebuggerStepThrough]
    public class HintTextBox : TextBox {
        private string TextHintString = string.Empty;
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, string lp);

        public HintTextBox() { }

        public string TextHint {
            get { return this.TextHintString; }
            set {
                this.TextHintString = value;
                this.Hint(value);
            }
        }

        void Hint(string HintString) {
            SendMessage(this.Handle, 0x1501, (IntPtr)1, HintString);
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public class VistaListView : ListView {
        private bool UseVistaStyle = true;

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        public VistaListView() {
            this.View = System.Windows.Forms.View.Details;
        }

        public bool EnableVistaView {
            get { return this.UseVistaStyle; }
            set {
                this.UseVistaStyle = value;
                if (value) {
                    SetWindowTheme(this.Handle, "Eplorer", null);
                }
                else {
                    SetWindowTheme(this.Handle, null, null);
                }
            }
        }
    }
}

class WebClientMethod : WebClient {
    public string Method { get; set; }

    protected override WebRequest GetWebRequest(Uri address) {
        WebRequest Request = base.GetWebRequest(address);
        if (!string.IsNullOrEmpty(Method))
            Request.Method = Method;

        return Request;
    }
}