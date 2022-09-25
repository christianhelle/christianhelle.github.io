using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace RoundedRectangle
{
    static class GraphicsExtensions
    {
        const int PS_SOLID = 0;
        const int PS_DASH = 1;

        [DllImport("coredll.dll")]
        static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

        [DllImport("coredll.dll")]
        static extern int SetBrushOrgEx(IntPtr hdc, int nXOrg, int nYOrg, ref Point lppt);

        [DllImport("coredll.dll")]
        static extern IntPtr CreateSolidBrush(uint color);

        [DllImport("coredll.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobject);

        [DllImport("coredll.dll")]
        static extern bool DeleteObject(IntPtr hgdiobject);

        [DllImport("coredll.dll")]
        static extern bool RoundRect(
            IntPtr hdc,
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidth,
            int nHeight);

        static uint GetColorRef(Color value)
        {
            return 0x00000000 | ((uint)value.B << 16) | ((uint)value.G << 8) | (uint)value.R;
        }

        public static void FillRoundedRectangle(
            this Graphics graphics,
            Pen border,
            Color color,
            Rectangle rectangle,
            Size ellipseSize)
        {
            var lppt = new Point();
            var hdc = graphics.GetHdc();
            var style = border.DashStyle == DashStyle.Solid ? PS_SOLID : PS_DASH;
            var hpen = CreatePen(style, (int)border.Width, GetColorRef(border.Color));
            var hbrush = CreateSolidBrush(GetColorRef(color));

            try
            {
                SetBrushOrgEx(hdc, rectangle.Left, rectangle.Top, ref lppt);
                SelectObject(hdc, hpen);
                SelectObject(hdc, hbrush);

                RoundRect(hdc,
                          rectangle.Left,
                          rectangle.Top,
                          rectangle.Right,
                          rectangle.Bottom,
                          ellipseSize.Width,
                          ellipseSize.Height);
            }
            finally
            {
                SetBrushOrgEx(hdc, lppt.Y, lppt.X, ref lppt);
                DeleteObject(hpen);
                DeleteObject(hbrush);

                graphics.ReleaseHdc(hdc);
            }
        }
    }
}
