using System.Windows.Forms;

namespace dx_book {
    public delegate bool MyWndProc(ref Message m);
    /// <summary>
    /// Subclass of the standard WindowsForms Form class, which allows us to inject a custom window procedure
    /// This allows us to go a little lower-level with some of the windows messages that are somewhat clunky 
    /// to handle using the normal WinForms callback events
    /// </summary>
    public class D3DForm : Form {
        public MyWndProc MyWndProc;
        protected override void WndProc(ref Message m) {
            if (MyWndProc != null) {
                if (MyWndProc(ref m)) return;
            }
            base.WndProc(ref m);
        }
    }
}