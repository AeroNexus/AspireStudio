using System;
using System.Drawing;

namespace Aspire.UiToolbox.Controls
{
  public class NodeCache
  {
    private Node _parentNode;
    private TreeControl _TreeControl;
    private Size _size;
    private Rectangle _bounds;
    private Rectangle _childBounds;
    public TreeControl TreeControl
    {
      get
      {
        return _TreeControl;
      }
      set
      {
        _TreeControl = value;
      }
    }
    public Node ParentNode
    {
      get
      {
        return _parentNode;
      }
      set
      {
        _parentNode = value;
      }
    }
    public bool IsSizeDirty
    {
      get
      {
        return _size == Size.Empty;
      }
    }
    public Size Size
    {
      get
      {
        return _size;
      }
      set
      {
        _size = value;
      }
    }
    public Rectangle Bounds
    {
      get
      {
        return _bounds;
      }
      set
      {
        _bounds = value;
      }
    }
    public Rectangle ChildBounds
    {
      get
      {
        return _childBounds;
      }
      set
      {
        _childBounds = value;
      }
    }
    public NodeCache()
    {
      _parentNode = null;
      _TreeControl = null;
      _size = Size.Empty;
      _bounds = Rectangle.Empty;
    }
    public void InvalidateSize()
    {
      _size = Size.Empty;
    }
  }
}
