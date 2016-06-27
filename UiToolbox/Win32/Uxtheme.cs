using System;
using System.Runtime.InteropServices;

namespace Aspire.UiToolbox.Win32
{
  public class Uxtheme
  {
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern bool IsAppThemed();
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern bool IsThemeActive();
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern bool GetCurrentThemeName(char[] themeName, int nameSize, char[] colorName, int colorSize, char[] sizeName, int sizeSize);
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr OpenThemeData(IntPtr hWnd, string classList);
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern int CloseThemeData(IntPtr hTheme);
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetWindowTheme(IntPtr hWnd);
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hDC, int partId, int stateId, ref RECT rect, IntPtr clip);
    [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
    public static extern int GetThemePartSize(IntPtr hTheme, IntPtr hDC, int partId, int stateId, IntPtr rect, THEMESIZE themeSize, ref SIZE size);
  }
}
