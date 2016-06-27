using System;
using System.Drawing;

using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Common
{
  public sealed class ColorHelper
  {
    private ColorHelper()
    {
    }
    public static Color TabBackgroundFromBaseColor(Color backColor)
    {
      Color result;
      if (backColor.R == 212 && backColor.G == 208 && backColor.B == 200)
      {
        result = Color.FromArgb(247, 243, 233);
      }
      else if (backColor.R == 236 && backColor.G == 233 && backColor.B == 216)
      {
        result = Color.FromArgb(255, 251, 233);
      }
      else
      {
        int num = (int)(255 - (255 - backColor.R) / 2);
        int num2 = (int)(255 - (255 - backColor.G) / 2);
        int num3 = (int)(255 - (255 - backColor.B) / 2);
        result = Color.FromArgb(num, num2, num3);
      }
      return result;
    }
    public static Color CalculateColor(Color front, Color back, int alpha)
    {
      Color color = Color.FromArgb(255, front);
      Color color2 = Color.FromArgb(255, back);
      float num = (float)color.R;
      float num2 = (float)color.G;
      float num3 = (float)color.B;
      float num4 = (float)color2.R;
      float num5 = (float)color2.G;
      float num6 = (float)color2.B;
      float num7 = num * (float)alpha / 255f + num4 * ((float)(255 - alpha) / 255f);
      float num8 = num2 * (float)alpha / 255f + num5 * ((float)(255 - alpha) / 255f);
      float num9 = num3 * (float)alpha / 255f + num6 * ((float)(255 - alpha) / 255f);
      byte b = (byte)num7;
      byte b2 = (byte)num8;
      byte b3 = (byte)num9;
      return Color.FromArgb(255, (int)b, (int)b2, (int)b3);
    }
    public static int ColorDepth()
    {
      IntPtr dC = User32.GetDC(IntPtr.Zero);
      int deviceCaps = Gdi32.GetDeviceCaps(dC, 14);
      int deviceCaps2 = Gdi32.GetDeviceCaps(dC, 12);
      User32.ReleaseDC(IntPtr.Zero, dC);
      return deviceCaps * deviceCaps2;
    }
    public static Color MergeColors(Color color1, float percent1, Color color2, float percent2)
    {
      return ColorHelper.MergeColors(color1, percent1, color2, percent2, Color.Empty, 0f);
    }
    public static Color MergeColors(Color color1, float percent1, Color color2, float percent2, Color color3, float percent3)
    {
      int num = (int)((float)color1.R * percent1 + (float)color2.R * percent2 + (float)color3.R * percent3);
      int num2 = (int)((float)color1.G * percent1 + (float)color2.G * percent2 + (float)color3.G * percent3);
      int num3 = (int)((float)color1.B * percent1 + (float)color2.B * percent2 + (float)color3.B * percent3);
      if (num < 0)
      {
        num = 0;
      }
      if (num > 255)
      {
        num = 255;
      }
      if (num2 < 0)
      {
        num2 = 0;
      }
      if (num2 > 255)
      {
        num2 = 255;
      }
      if (num3 < 0)
      {
        num3 = 0;
      }
      if (num3 > 255)
      {
        num3 = 255;
      }
      return Color.FromArgb(num, num2, num3);
    }
  }
}
