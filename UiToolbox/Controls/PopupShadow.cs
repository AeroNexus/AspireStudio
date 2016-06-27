using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;
using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Controls
{
  public class PopupShadow : NativeWindow, IDisposable
  {
    private static readonly int SHADOW_SIZE = 4;
    private Pen[] _pens;
    private Brush[] _brushes;
    private Rectangle _showRect;
    private bool _valid;
    public int ShadowLength
    {
      get
      {
        return PopupShadow.SHADOW_SIZE;
      }
    }
    public Rectangle ShowRect
    {
      get
      {
        return _showRect;
      }
      set
      {
        if (_valid)
        {
          value.Inflate(PopupShadow.SHADOW_SIZE, PopupShadow.SHADOW_SIZE);
          if (_showRect != value)
          {
            _showRect = value;
            this.UpdateLayeredWindow(_showRect);
          }
        }
      }
    }
    public PopupShadow()
    {
      _valid = (OSFeature.Feature.GetVersionPresent(OSFeature.LayeredWindows) != null && ColorHelper.ColorDepth() > 8);
      if (_valid)
      {
        _showRect = Rectangle.Empty;
        _pens = new Pen[PopupShadow.SHADOW_SIZE];
        _brushes = new Brush[PopupShadow.SHADOW_SIZE];
        for (int i = 0; i < PopupShadow.SHADOW_SIZE; i++)
        {
          _pens[i] = new Pen(Color.FromArgb(64 - i * 16, 0, 0, 0));
          _brushes[i] = new SolidBrush(Color.FromArgb(64 - i * 16, 0, 0, 0));
        }
        CreateParams createParams = new CreateParams();
        createParams.Caption = "NativePopupShadow";
        createParams.X = _showRect.Left;
        createParams.Y = _showRect.Top;
        createParams.Height = _showRect.Width;
        createParams.Width = _showRect.Height;
        createParams.Parent = IntPtr.Zero;
        createParams.Style = -2147483648;
        createParams.ExStyle = 136;
        CreateParams expr_123 = createParams;
        expr_123.ExStyle = expr_123.ExStyle + 524288;
        this.CreateHandle(createParams);
      }
    }
    public void Dispose()
    {
      if (_valid)
      {
        this.DestroyHandle();
        if (_pens != null)
        {
          Pen[] pens = _pens;
          for (int i = 0; i < pens.Length; i++)
          {
            Pen pen = pens[i];
            pen.Dispose();
          }
          _pens = null;
        }
        if (_brushes != null)
        {
          Brush[] brushes = _brushes;
          for (int i = 0; i < brushes.Length; i++)
          {
            Brush brush = brushes[i];
            brush.Dispose();
          }
          _brushes = null;
        }
      }
    }
    public virtual void ShowWithoutFocus()
    {
      if (_valid)
      {
        this.UpdateLayeredWindow(_showRect);
        User32.ShowWindow(base.Handle, 4);
      }
    }
    private void UpdateLayeredWindow(Rectangle rect)
    {
      if (rect.Width > 0 && rect.Height > 0)
      {
        Bitmap bitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
          Rectangle area = new Rectangle(0, 0, rect.Width, rect.Height);
          this.DrawShadow(graphics, area);
          IntPtr dC = User32.GetDC(IntPtr.Zero);
          IntPtr intPtr = Gdi32.CreateCompatibleDC(dC);
          IntPtr hbitmap = bitmap.GetHbitmap(Color.FromArgb(0));
          IntPtr hObject = Gdi32.SelectObject(intPtr, hbitmap);
          SIZE sIZE;
          sIZE.cx = rect.Width;
          sIZE.cy = rect.Height;
          POINT pOINT;
          pOINT.x = rect.Left;
          pOINT.y = rect.Top;
          POINT pOINT2;
          pOINT2.x = 0;
          pOINT2.y = 0;
          BLENDFUNCTION bLENDFUNCTION = default(BLENDFUNCTION);
          bLENDFUNCTION.BlendOp = 0;
          bLENDFUNCTION.BlendFlags = 0;
          bLENDFUNCTION.SourceConstantAlpha = 255;
          bLENDFUNCTION.AlphaFormat = 1;
          User32.UpdateLayeredWindow(base.Handle, dC, ref pOINT, ref sIZE, intPtr, ref pOINT2, 0, ref bLENDFUNCTION, 2);
          Gdi32.SelectObject(intPtr, hObject);
          User32.ReleaseDC(IntPtr.Zero, dC);
          Gdi32.DeleteObject(hbitmap);
          Gdi32.DeleteDC(intPtr);
        }
      }
    }
    private void DrawShadow(Graphics g, Rectangle area)
    {
      this.DrawVerticalShadow(g, area);
      this.DrawHorizontalShadow(g, area);
    }
    private void DrawVerticalShadow(Graphics g, Rectangle area)
    {
      int num = area.Right - PopupShadow.SHADOW_SIZE;
      int arg_15_0 = area.Right;
      int num2 = area.Top + PopupShadow.SHADOW_SIZE * 2;
      int num3 = area.Bottom - 1;
      int num4 = num3 - num2;
      for (int i = 0; i < PopupShadow.SHADOW_SIZE; i++)
      {
        for (int j = i; j < PopupShadow.SHADOW_SIZE; j++)
        {
          g.FillRectangle(_brushes[i + (PopupShadow.SHADOW_SIZE - 1 - j)], num + i, num2 + j, 1, 1);
        }
      }
      num2 += PopupShadow.SHADOW_SIZE;
      num4 -= PopupShadow.SHADOW_SIZE;
      if (num4 > PopupShadow.SHADOW_SIZE)
      {
        int num5 = num3 - PopupShadow.SHADOW_SIZE;
        for (int k = 0; k < PopupShadow.SHADOW_SIZE; k++)
        {
          g.DrawLine(_pens[k], num + k, num2, num + k, num5);
        }
      }
      if (num4 > PopupShadow.SHADOW_SIZE)
      {
        int num6 = num3 - PopupShadow.SHADOW_SIZE + 1;
        g.FillRectangle(_brushes[0], num, num6, 1, 1);
        for (int l = 1; l < PopupShadow.SHADOW_SIZE; l++)
        {
          g.DrawLine(_pens[l], num + l, num6, num + l, num6 + l);
        }
      }
    }
    private void DrawHorizontalShadow(Graphics g, Rectangle area)
    {
      int num = area.Left + PopupShadow.SHADOW_SIZE * 2;
      int right = area.Right;
      int num2 = area.Bottom - PopupShadow.SHADOW_SIZE;
      int arg_2D_0 = area.Bottom;
      int num3 = right - num;
      for (int i = 0; i < PopupShadow.SHADOW_SIZE; i++)
      {
        for (int j = i; j < PopupShadow.SHADOW_SIZE; j++)
        {
          g.FillRectangle(_brushes[i + (PopupShadow.SHADOW_SIZE - 1 - j)], num + j, num2 + i, 1, 1);
        }
      }
      num += PopupShadow.SHADOW_SIZE;
      num3 -= PopupShadow.SHADOW_SIZE;
      if (num3 > PopupShadow.SHADOW_SIZE)
      {
        int num4 = right - PopupShadow.SHADOW_SIZE - 1;
        for (int k = 0; k < PopupShadow.SHADOW_SIZE; k++)
        {
          g.DrawLine(_pens[k], num, num2 + k, num4, num2 + k);
        }
      }
      if (num3 > PopupShadow.SHADOW_SIZE)
      {
        int num5 = right - PopupShadow.SHADOW_SIZE;
        g.FillRectangle(_brushes[1], num5, num2 + 1, 1, 1);
        for (int l = 2; l < PopupShadow.SHADOW_SIZE; l++)
        {
          g.DrawLine(_pens[l], num5, num2 + l, num5 + l - 1, num2 + l);
        }
      }
    }
  }
}
