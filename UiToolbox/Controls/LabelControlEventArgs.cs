using System;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Controls
{
  public class LabelControlEventArgs : EventArgs
  {
    private TextBox _textBox;
    public TextBox TextBox
    {
      get
      {
        return _textBox;
      }
    }
    public LabelControlEventArgs(TextBox textBox)
    {
      _textBox = textBox;
    }
  }
}
