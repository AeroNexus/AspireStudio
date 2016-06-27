using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Aspire.UiToolbox.Win32
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct BLENDFUNCTION
  {
    public byte BlendOp;
    public byte BlendFlags;
    public byte SourceConstantAlpha;
    public byte AlphaFormat;
  }

  public struct LOGBRUSH
  {
    public uint lbStyle;
    public uint lbColor;
    public uint lbHatch;
  }

  public struct MSG
  {
    public IntPtr hwnd;
    public int message;
    public IntPtr wParam;
    public IntPtr lParam;
    public int time;
    public int pt_x;
    public int pt_y;
  }

  public struct PAINTSTRUCT
  {
    public IntPtr hdc;
    public int fErase;
    public Rectangle rcPaint;
    public int fRestore;
    public int fIncUpdate;
    public int Reserved1;
    public int Reserved2;
    public int Reserved3;
    public int Reserved4;
    public int Reserved5;
    public int Reserved6;
    public int Reserved7;
    public int Reserved8;
  }

  public struct POINT
  {
    public int x;
    public int y;
  }

  public struct RECT
  {
    public int left;
    public int top;
    public int right;
    public int bottom;
  }

  public struct SIZE
  {
    public int cx;
    public int cy;
  }

  public struct TRACKMOUSEEVENTS
  {
    public uint cbSize;
    public uint dwFlags;
    public IntPtr hWnd;
    public uint dwHoverTime;
  }

}
