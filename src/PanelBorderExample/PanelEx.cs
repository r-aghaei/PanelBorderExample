using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static PanelBorderExample.Win32Helpers;
namespace PanelBorderExample
{
    public class PanelEx : Panel
    {
        public PanelEx()
        {
            BorderStyle = BorderStyle.FixedSingle;
        }
        private Color borderColor = Color.Blue;
        [DefaultValue(typeof(Color), "Blue")]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    Redraw();
                }
            }
        }
        private int borderWidth = 16;
        [DefaultValue(16)]
        public int BorderWidth
        {
            get { return borderWidth; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("The value should be greater than 0");
                if (borderWidth != value)
                {
                    borderWidth = value;
                    RecalculateClientSize();
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (BorderStyle != BorderStyle.FixedSingle)
            {
                base.WndProc(ref m);
                return;
            }
            if (m.Msg == WM_NCPAINT)
            {
                base.WndProc(ref m);
                WmNCPaint(ref m);
            }
            else if (m.Msg == WM_NCCALCSIZE)
            {
                base.WndProc(ref m);
                WmNCCalcSize(ref m);
            }
            else if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                WmNCHitTest(ref m);
            }
            else
                base.WndProc(ref m);

        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Redraw();
        }
        private void Redraw()
        {
            RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
               RDW_FRAME | RDW_INVALIDATE | RDW_UPDATENOW);
        }
        private void RecalculateClientSize()
        {
            SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOSIZE | SWP_NOMOVE | SWP_FRAMECHANGED | SWP_NOZOORDER);
        }
        private void WmNCCalcSize(ref Message m)
        {
            if (BorderStyle != BorderStyle.FixedSingle)
                return;

            if (m.WParam != IntPtr.Zero)
            {
                var nccsp = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(NCCALCSIZE_PARAMS));
                nccsp.rgrc[0].top += borderWidth - 1;
                nccsp.rgrc[0].bottom -= borderWidth - 1;
                nccsp.rgrc[0].left += borderWidth - 1;
                nccsp.rgrc[0].right -= borderWidth - 1;
                Marshal.StructureToPtr(nccsp, m.LParam, true);
                InvalidateRect(this.Handle, nccsp.rgrc[0], true);
                m.Result = IntPtr.Zero;
            }
            else
            {
                var clnRect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                clnRect.top += borderWidth - 1;
                clnRect.bottom -= borderWidth - 1;
                clnRect.left += borderWidth - 1;
                clnRect.right -= borderWidth - 1;
                Marshal.StructureToPtr(clnRect, m.LParam, true);
                m.Result = IntPtr.Zero;
            }
        }
        private void WmNCPaint(ref Message m)
        {
            var dc = GetWindowDC(Handle);
            using (var g = Graphics.FromHdc(dc))
            {
                using (var p = new Pen(BorderColor, borderWidth) { Alignment = PenAlignment.Inset })
                {
                    if (VScroll && HScroll)
                    {
                        Rectangle bottomCornerRectangle = new Rectangle(
                            Width - SystemInformation.VerticalScrollBarWidth - borderWidth,
                            Height - SystemInformation.HorizontalScrollBarHeight - borderWidth,
                            SystemInformation.VerticalScrollBarWidth,
                            SystemInformation.HorizontalScrollBarHeight);
                        if (RightToLeft == RightToLeft.Yes)
                        {
                            bottomCornerRectangle.X = Width - bottomCornerRectangle.Right;
                        }
                        g.FillRectangle(SystemBrushes.Control, bottomCornerRectangle);
                    }
                    var adjustment = borderWidth == 1 ? 1 : 0;
                    g.DrawRectangle(p, 0, 0, Width - adjustment, Height - adjustment);
                }
            }
            ReleaseDC(Handle, dc);
            m.Result = IntPtr.Zero;
        }
        private void WmNCHitTest(ref Message m)
        {
            var pt = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
            var rect = Parent.RectangleToScreen(Bounds);
            if (((pt.X >= rect.Left && pt.X <= rect.Left + borderWidth) ||
                (pt.X >= rect.Right - borderWidth && pt.X <= rect.Right)) ||
                ((pt.Y >= rect.Top && pt.Y <= rect.Top + borderWidth) ||
                (pt.Y >= rect.Bottom - borderWidth && pt.Y <= rect.Bottom)))
                m.Result = (IntPtr)HTBORDER;
        }
    }
}
