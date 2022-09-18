using System.Windows.Forms;

namespace murrty.controls {
    [System.Diagnostics.DebuggerStepThrough]
    public class VistaListView : ListView {

        private bool UseVistaStyle = true;

        public VistaListView() {
            this.View = View.Details;
        }

        public bool VistaView {
            get { return this.UseVistaStyle; }
            set {
                this.UseVistaStyle = value;
                if (value) {
                    NativeMethods.SetWindowTheme(this.Handle, "Explorer", null);
                }
                else {
                    NativeMethods.SetWindowTheme(this.Handle, null, null);
                }
            }
        }
    }
}
