using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace XmlNotepad {
    public enum TipRequestType { Default, Hover };

    public class IntelliTipEventArgs : EventArgs {
        public TipRequestType Type;
        public string ToolTip;
        public Point Location;
        public Control Focus;
    }
    public delegate void IntelliTipEventHandler(object sender, IntelliTipEventArgs args);

    /// <summary>
    /// This class provides a ToolTip at the cursor location based on mouse hover events
    /// on the watched child views.  It is wraps the WinForms ToolTip class and provides
    /// some added benefits, like being able to monitor multiple child views, and being
    /// able to Start() the tip operation based on some other event, (like list box
    /// selection changed) and word wrapping of the tooltip text string.
    /// </summary>
    public class IntelliTip {

        private Control owner;
        private ToolTip tip = new ToolTip();
        const int HoverDelay = 300;
        int tipTime;
        List<Control> watch = new List<Control>();
        bool tipVisible;
        TipRequestType type;
        Timer popupDelay;
        bool resetpending;
        Rectangle lastHover;
        Control showing;

        public event IntelliTipEventHandler ShowToolTip;

        public IntelliTip(Control owner) {
            this.owner = owner;
            this.tip.Popup += new PopupEventHandler(OnTipPopup);
            owner.MouseMove += new MouseEventHandler(OnWatchMouseMove);

            popupDelay = new Timer();
            popupDelay.Tick += new EventHandler(popupDelay_Tick);
            popupDelay.Interval = 500;
            this.tip.AutoPopDelay = 0;
            this.tip.AutomaticDelay = 0;
            this.tip.UseAnimation = false;
            this.tip.UseFading = false;
        }

        public int PopupDelay {
            get { return this.popupDelay.Interval; }
            set { this.popupDelay.Interval = value; }
        }

        public void AddWatch(Control c) {
            c.MouseHover += new EventHandler(OnWatchMouseHover);
            c.MouseMove += new MouseEventHandler(OnWatchMouseMove);
            c.KeyDown += new KeyEventHandler(OnWatchKeyDown);
            watch.Add(c);
        }

        public bool Visible {
            get { return this.tipVisible; }
        }

        public void Hide() {
            if (showing != null) {
                this.tip.Hide(showing);
            }
            showing = null;
            this.tip.RemoveAll();                    
            this.tipVisible = false;
        }

        //=============================== Private methods ===============================
        void popupDelay_Tick(object sender, EventArgs e) {
            popupDelay.Stop();
            this.owner.Invoke(new EventHandler(OnPopupDelay), new object[] { this, EventArgs.Empty });
        }

        void OnPopupDelay(object sender, EventArgs e) {
            this.OnShowToolTip();
        }

        void OnTipPopup(object sender, PopupEventArgs e) {
            this.tipVisible = true;
        }

        void OnWatchKeyDown(object sender, KeyEventArgs e) {
            Hide();  
        }

        void OnWatchMouseHover(object sender, EventArgs e) {
            this.type = TipRequestType.Hover;
            Start();
        }

        void Start() {
            this.popupDelay.Stop();
            this.popupDelay.Start();
        }

        Control GetFocus() {
            foreach (Control c in this.watch) {
                if (c.Focused) return c;
            }
            return this.owner;
        }

        internal void OnShowToolTip() {
            this.type = TipRequestType.Default;
            lastHover = new Rectangle(Cursor.Position, Size.Empty);
            lastHover.Inflate(10, 10);
            resetpending = true;       

            if (ShowToolTip != null && !owner.Capture) {
                Control c = GetFocus();
                Point local = c.PointToClient(Cursor.Position);
                IntelliTipEventArgs args = new IntelliTipEventArgs();
                args.Type = this.type;
                args.Focus = c;
                args.Location = local;
                ShowToolTip(this, args);
                string toolTip = args.ToolTip;
                if (!string.IsNullOrEmpty(toolTip)) {
                    this.tip.ShowAlways = true;
                    this.tip.Active = true;
                    Point p = args.Location;
                    if (p.X == local.X && p.Y == local.Y) {
                        p.Y += 10;
                        p.Y += 10;
                    }
                    this.tipTime = Environment.TickCount;
                    showing = c;
                    this.tip.Show(WordWrap(toolTip), (IWin32Window)c, p);
                    return;
                }
            }
            this.tip.Hide(owner);
        }

        void OnWatchMouseMove(object sender, MouseEventArgs e) {
            bool outside = !lastHover.Contains(Cursor.Position);

            if (this.tipVisible && outside) {
                Hide();
            }
            if (resetpending && outside) {
                resetpending = false;
                this.ResetHoverTracking(((Control)sender).Handle);
            }
        }

        string WordWrap(string tip) {
            Screen screen = Screen.FromControl(owner);
            int width = screen.Bounds.Width / 2;
            StringBuilder sb = new StringBuilder();
            using (Graphics g = owner.CreateGraphics()) {
                Font f = owner.Font;
                int wrap = 0;
                foreach (string word in tip.Split(' ', '\t', '\r', '\n')) {
                    if (string.IsNullOrEmpty(word)) continue;
                    SizeF size = g.MeasureString(word + " ", f);
                    wrap += (int)size.Width;
                    sb.Append(word);
                    if (wrap > width) {
                        sb.Append('\n');
                        wrap = 0;
                    } else {
                        sb.Append(' ');
                    }
                }
            }
            return sb.ToString();
        }


        #region HoverTracking

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TrackMouseEvent(TRACKMOUSEEVENT tme);

        [StructLayout(LayoutKind.Sequential)]
        public class TRACKMOUSEEVENT {
            public int cbSize = Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
            public int dwFlags;
            public IntPtr hwndTrack;
            public int dwHoverTime = HoverDelay; // Never set this to field ZERO, or to HOVER_DEFAULT, ever!
        }

        TRACKMOUSEEVENT trackMouseEvent;
        const int TME_HOVER = 0x00000001;
        const int WM_MOUSEHOVER = 0x02A1;

        internal void ResetHoverTracking(IntPtr handle) {
            if (trackMouseEvent == null) {
                trackMouseEvent = new TRACKMOUSEEVENT();
                trackMouseEvent.dwFlags = TME_HOVER;
                trackMouseEvent.hwndTrack = handle;
            }
            TrackMouseEvent(trackMouseEvent);
        }

        #endregion 
    }
}
