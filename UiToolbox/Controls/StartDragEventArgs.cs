using System;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Controls
{
  public class StartDragEventArgs : CancelNodeEventArgs
  {
    private object _object;
    private DragDropEffects _effect;
    public DragDropEffects Effect
    {
      get
      {
        return _effect;
      }
      set
      {
        _effect = value;
      }
    }
    public object Object
    {
      get
      {
        return _object;
      }
      set
      {
        _object = value;
      }
    }
    public StartDragEventArgs(Node node, DragDropEffects effect)
      : base(node)
    {
      _object = node;
      _effect = effect;
    }
  }
}
