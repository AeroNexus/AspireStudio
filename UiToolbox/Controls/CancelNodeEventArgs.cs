using System;
using System.ComponentModel;

namespace Aspire.UiToolbox.Controls
{
  public class CancelNodeEventArgs : CancelEventArgs
  {
    private Node _node;
    public Node Node
    {
      get
      {
        return this._node;
      }
    }
    public CancelNodeEventArgs(Node node)
    {
      this._node = node;
    }
  }
}
