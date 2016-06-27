using System;
using System.Drawing;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;
using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Controls
{
  public class PopupTooltipSingle : PopupBase
  {
    private static int PADDING = 3;
    private Size _size;
    private int _textHeight;
    private string _toolText;
    public string ToolText
    {
      get
      {
        return _toolText;
      }
      set
      {
        value = value.Replace("&", "");
        _toolText = value;
        this.CalculateSizePosition(base.Location);
        this.Refresh();
      }
    }
    public int TextHeight
    {
      get
      {
        return _textHeight;
      }
      set
      {
        _textHeight = value;
      }
    }
    public PopupTooltipSingle()
      : base(VisualStyle.Office2003)
    {
      this.InternalConstruct(new Font(SystemInformation.MenuFont, 0));
    }
    public PopupTooltipSingle(Font font)
      : base(VisualStyle.Office2003)
    {
      this.InternalConstruct(font);
    }
    public PopupTooltipSingle(VisualStyle style)
      : base(style)
    {
      this.InternalConstruct(new Font(SystemInformation.MenuFont, 0));
    }
    public PopupTooltipSingle(VisualStyle style, Font font)
      : base(style)
    {
      this.InternalConstruct(font);
    }
    public virtual void ShowWithoutFocus(Point screenPos)
    {
      this.CalculateSizePosition(screenPos);
      this.ShowWithoutFocus();
      this.Refresh();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      Rectangle clientRectangle = base.ClientRectangle;
      clientRectangle.Width = clientRectangle.Width - 1;
      clientRectangle.Height = clientRectangle.Height - 1;
      e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, clientRectangle);
      using (StringFormat stringFormat = new StringFormat())
      {
        stringFormat.Alignment = StringAlignment.Near;
        stringFormat.LineAlignment = StringAlignment.Center;
        using (SolidBrush solidBrush = new SolidBrush(this.ForeColor))
        {
          e.Graphics.DrawString(this.ToolText, this.Font, solidBrush, new RectangleF((float)PopupTooltipSingle.PADDING, 0f, (float)((double)base.Width * 1.25),
            (float)base.Height), stringFormat);
        }
      }
    }
    protected override void WndProc(ref Message m)
    {
      if (m.Msg == 132)
      {
        m.Result = (IntPtr)(-1L);
        return;
      }
      base.WndProc(ref m);
    }
    private void InternalConstruct(Font font)
    {
      this.BackColor = SystemColors.Info;
      this.ForeColor = SystemColors.InfoText;
      this.Font = font;
      _textHeight = -1;
    }
    private void CalculateSizePosition(Point screenPos)
    {
      using (Graphics graphics = base.CreateGraphics())
      {
        SizeF sizeF = graphics.MeasureString(this.ToolText, this.Font);
        _size = new Size((int)sizeF.Width, (int)sizeF.Height);
        if (_textHeight != -1)
        {
          _size.Height = _textHeight;
        }
        else
        {
          _size.Height = _size.Height + PopupTooltipSingle.PADDING * 2;
        }
        Size size = new Size(_size.Width + PopupTooltipSingle.PADDING * 3, _size.Height);
        int num = 0;
        if (base.PopupShadow != null)
        {
          num = base.PopupShadow.ShadowLength;
        }
        if (screenPos.X + size.Width + num > Screen.GetWorkingArea(this).Width)
        {
          screenPos.X = Screen.GetWorkingArea(this).Width - size.Width - num;
        }
        if (screenPos.Y + size.Height + num > Screen.GetWorkingArea(this).Height)
        {
          screenPos.Y = Screen.GetWorkingArea(this).Height - size.Height - num;
        }
        User32.SetWindowPos(base.Handle, IntPtr.Zero, screenPos.X, screenPos.Y, size.Width, size.Height, 532u);
      }
    }
  }
}
