using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;
using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Controls
{
  public class PopupBase : Form
  {
    private PopupShadow _shadow;
    private Container components = null;
    public PopupShadow PopupShadow
    {
      get
      {
        return _shadow;
      }
    }
    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams createParams = base.CreateParams;
        createParams.Style = -2147483648;
        createParams.ExStyle = 136;
        return createParams;
      }
    }
    public PopupBase()
    {
      this.InitializeComponent();
      this.InternalConstruct(VisualStyle.Office2003);
    }
    public PopupBase(VisualStyle style)
    {
      this.InitializeComponent();
      this.InternalConstruct(style);
    }
    private void InternalConstruct(VisualStyle style)
    {
      if (style == VisualStyle.IDE || style == VisualStyle.IDE2005 || style == VisualStyle.Office2003)
      {
        _shadow = new PopupShadow();
      }
    }
    public virtual void ShowWithoutFocus()
    {
      if (_shadow != null)
      {
        _shadow.ShowWithoutFocus();
      }
      User32.ShowWindow(base.Handle, 4);
    }
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_shadow != null)
        {
          _shadow.Dispose();
        }
        if (this.components != null)
        {
          this.components.Dispose();
        }
      }
      base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
      base.ClientSize = new Size(100, 100);
      base.ControlBox = false;
      base.FormBorderStyle = 0;
      base.MaximizeBox = false;
      base.MinimizeBox = false;
      base.Name = "PopupBase";
      base.ShowInTaskbar = false;
      base.SizeGripStyle = SizeGripStyle.Hide;
      base.StartPosition = 0;
      this.Text = "PopupBase";
      base.Resize += OnResize;
      base.Move += OnMove;
    }
    private void OnMove(object sender, EventArgs e)
    {
      if (_shadow != null)
      {
        _shadow.ShowRect = base.RectangleToScreen(base.ClientRectangle);
      }
    }
    private void OnResize(object sender, EventArgs e)
    {
      if (_shadow != null)
      {
        _shadow.ShowRect = base.RectangleToScreen(base.ClientRectangle);
      }
    }
  }
}
