using System;

namespace Aspire.UiToolbox.Controls
{
  public class NodeEventArgs : EventArgs
  {
    private Node _node;
    public Node Node
    {
      get
      {
        return _node;
      }
    }
    public NodeEventArgs(Node node)
    {
      _node = node;
    }
  }
}
