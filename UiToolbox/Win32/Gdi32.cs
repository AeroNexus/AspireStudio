using System;
using System.Runtime.InteropServices;

namespace Aspire.UiToolbox.Win32
{
  public class Gdi32
  {
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern int CombineRgn(IntPtr dest, IntPtr src1, IntPtr src2, int flags);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateRectRgnIndirect(ref RECT rect);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClipBox(IntPtr hDC, ref RECT rectBox);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClipRgn(IntPtr hDC, ref IntPtr hRgn);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateBrushIndirect(ref LOGBRUSH brush);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool PatBlt(IntPtr hDC, int x, int y, int width, int height, uint flags);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern bool DeleteDC(IntPtr hDC);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
  }
}
