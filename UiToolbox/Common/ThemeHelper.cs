using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Common
{
  public class ThemeHelper : IDisposable
  {
    private IntPtr _hTheme;
    private Control _control;
    private string _classList;
    public event EventHandler ThemeOpened;

    public event EventHandler ThemeClosed;

    public bool IsControlThemed
    {
      get
      {
        return _hTheme != IntPtr.Zero;
      }
    }
    public static bool IsAppThemed
    {
      get
      {
        bool result;
        try
        {
          result = Uxtheme.IsAppThemed();
        }
        catch
        {
          result = false;
        }
        return result;
      }
    }
    public static bool IsThemeActive
    {
      get
      {
        bool result;
        try
        {
          result = Uxtheme.IsThemeActive();
        }
        catch
        {
          result = false;
        }
        return result;
      }
    }
    public ThemeHelper(Control control, string classList)
    {
      _hTheme = IntPtr.Zero;
      _control = control;
      _classList = classList;
      _control.HandleCreated += OnControlCreated;
      _control.HandleDestroyed += OnControlDestroyed;
    }
    public void Dispose()
    {
      this.CloseTheme();
      _control.HandleCreated -= OnControlCreated;
      _control.HandleDestroyed -= OnControlDestroyed;
    }
    public void DrawThemeBackground(Graphics g, Rectangle draw, int part, int state)
    {
      if (this.IsControlThemed)
      {
        RECT rECT = default(RECT);
        rECT.left = draw.X;
        rECT.top = draw.Y;
        rECT.right = draw.Right;
        rECT.bottom = draw.Bottom;
        IntPtr hdc = g.GetHdc();
        Uxtheme.DrawThemeBackground(_hTheme, hdc, part, state, ref rECT, IntPtr.Zero);
        g.ReleaseHdc(hdc);
      }
    }
    public void DrawThemeBackground(Graphics g, Rectangle draw, Rectangle exclude, int part, int state)
    {
      if (this.IsControlThemed)
      {
        RECT rECT = default(RECT);
        rECT.left = draw.X;
        rECT.top = draw.Y;
        rECT.right = draw.Right;
        rECT.bottom = draw.Bottom;
        RECT rECT2 = default(RECT);
        rECT2.left = exclude.X;
        rECT2.top = exclude.Y;
        rECT2.right = exclude.Right;
        rECT2.bottom = exclude.Bottom;
        IntPtr hdc = g.GetHdc();
        IntPtr zero = IntPtr.Zero;
        Gdi32.GetClipRgn(hdc, ref zero);
        IntPtr intPtr = Gdi32.CreateRectRgnIndirect(ref rECT);
        IntPtr intPtr2 = Gdi32.CreateRectRgnIndirect(ref rECT2);
        Gdi32.CombineRgn(intPtr2, intPtr, intPtr2, 4);
        Gdi32.SelectClipRgn(hdc, intPtr2);
        Uxtheme.DrawThemeBackground(_hTheme, hdc, part, state, ref rECT, IntPtr.Zero);
        Gdi32.SelectClipRgn(hdc, zero);
        Gdi32.DeleteObject(intPtr);
        Gdi32.DeleteObject(intPtr2);
        g.ReleaseHdc(hdc);
      }
    }
    public Size GetThemePartSize(Graphics g, int part, int state, THEMESIZE themeSize)
    {
      Size empty = Size.Empty;
      if (this.IsControlThemed)
      {
        SIZE sIZE = default(SIZE);
        IntPtr hdc = g.GetHdc();
        Uxtheme.GetThemePartSize(_hTheme, hdc, part, state, IntPtr.Zero, themeSize, ref sIZE);
        g.ReleaseHdc(hdc);
        empty.Width = sIZE.cx;
        empty.Height = sIZE.cy;
      }
      return empty;
    }
    public void WndProc(ref Message m)
    {
      if (m.Msg == 794)
      {
        this.CloseTheme();
        this.OpenTheme();
      }
    }
    public static bool GetCurrentThemeName(ref string theme, ref string color, ref string size)
    {
      bool result;
      try
      {
        char[] array = new char[256];
        char[] array2 = new char[256];
        char[] array3 = new char[256];
        Uxtheme.GetCurrentThemeName(array, 255, array2, 255, array3, 255);
        int num = 0;
        while (num < 256 && array[num] != '\0')
        {
          num++;
        }
        int num2 = 0;
        while (num2 < 256 && array2[num2] != '\0')
        {
          num2++;
        }
        int num3 = 0;
        while (num3 < 256 && array3[num3] != '\0')
        {
          num3++;
        }
        theme = new string(array, 0, num);
        color = new string(array2, 0, num2);
        size = new string(array3, 0, num3);
        result = true;
      }
      catch
      {
        result = false;
      }
      return result;
    }
    protected void OnThemeOpened()
    {
      if (this.ThemeOpened != null)
      {
        this.ThemeOpened.Invoke(this, EventArgs.Empty);
      }
    }
    protected void OnThemeClosed()
    {
      if (this.ThemeClosed != null)
      {
        this.ThemeClosed.Invoke(this, EventArgs.Empty);
      }
    }
    private void OnControlCreated(object sender, EventArgs e)
    {
      this.OpenTheme();
    }
    private void OnControlDestroyed(object sender, EventArgs e)
    {
      this.CloseTheme();
    }
    private void OpenTheme()
    {
      if (ThemeHelper.IsAppThemed && ThemeHelper.IsThemeActive)
      {
        _hTheme = Uxtheme.OpenThemeData(_control.Handle, _classList);
        this.OnThemeOpened();
      }
    }
    private void CloseTheme()
    {
      if (this.IsControlThemed)
      {
        Uxtheme.CloseThemeData(_hTheme);
        _hTheme = IntPtr.Zero;
        this.OnThemeClosed();
      }
    }
  }
}
