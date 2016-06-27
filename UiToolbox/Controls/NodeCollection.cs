using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  [DefaultEvent("Inserted"), DefaultProperty("ParentNode")]
  public class NodeCollection : CollectionWithEvents, ICloneable
  {
    private int _visibleCount;
    private NodeCache _cache;
    private INodeCollectionVC _vc;
    public event EventHandler VCChanged;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Node ParentNode
    {
      get
      {
        return this.Cache.ParentNode;
      }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public TreeControl TreeControl
    {
      get
      {
        return this.Cache.TreeControl;
      }
    }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public INodeCollectionVC VC
    {
      get
      {
        if (_vc != null)
        {
          return _vc;
        }
        if (this.Cache.TreeControl != null)
        {
          return this.Cache.TreeControl.CollectionVC;
        }
        return null;
      }
      set
      {
        if (_vc != value)
        {
          _vc = value;
          OnVCChanged();
        }
      }
    }
    [Browsable(false)]
    public Size Size
    {
      get
      {
        return Cache.Size;
      }
    }
    [Browsable(false)]
    public Rectangle Bounds
    {
      get
      {
        return Cache.Bounds;
      }
    }
    [Browsable(false)]
    public Rectangle ChildBounds
    {
      get
      {
        return Cache.ChildBounds;
      }
    }
    public int FirstVisibleIndex
    {
      get
      {
        for (int i = 0; i < base.Count; i++)
        {
          if (this[i].Visible)
          {
            return i;
          }
        }
        return -1;
      }
    }
    public int LastVisibleIndex
    {
      get
      {
        for (int i = base.Count - 1; i >= 0; i--)
        {
          if (this[i].Visible)
          {
            return i;
          }
        }
        return -1;
      }
    }
    public int VisibleCount
    {
      get
      {
        return _visibleCount;
      }
    }
    public Node this[int index]
    {
      get
      {
        return base.List[index] as Node;
      }
    }
    protected internal NodeCache Cache
    {
      get
      {
        return _cache;
      }
    }
    public NodeCollection()
    {
      this.CommonConstruct();
    }
    public NodeCollection(TreeControl tl)
    {
      this.CommonConstruct();
      this.Cache.TreeControl = tl;
    }
    public NodeCollection(Node parentNode)
    {
      this.CommonConstruct();
      this.Cache.ParentNode = parentNode;
    }
    private void CommonConstruct()
    {
      _cache = new NodeCache();
      _vc = null;
      _visibleCount = 0;
    }
    public Node Add(Node value)
    {
      base.List.Add(value);
      return value;
    }
    public void AddRange(Node[] values)
    {
      for (int i = 0; i < values.Length; i++)
      {
        Node value = values[i];
        Add(value);
      }
    }
    public void Remove(Node value)
    {
      base.List.Remove(value);
    }
    public void Insert(int index, Node value)
    {
      base.List.Insert(index, value);
    }
    public bool Contains(Node value)
    {
      return base.List.Contains(value);
    }
    public int IndexOf(Node value)
    {
      return base.List.IndexOf(value);
    }
    public Node GetFirstNode()
    {
      Node result = null;
      if (base.Count > 0)
      {
        result = this[0];
      }
      return result;
    }
    public Node GetFirstDisplayedNode()
    {
      Node result = null;
      if (base.Count > 0)
      {
        IEnumerator enumerator = base.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node node = (Node)enumerator.Current;
            if (node.Visible)
            {
              result = node;
              break;
            }
          }
        }
        finally
        {
          IDisposable disposable = enumerator as IDisposable;
          if (disposable != null)
          {
            disposable.Dispose();
          }
        }
      }
      return result;
    }
    public Node GetLastNode()
    {
      Node node = null;
      if (base.Count > 0)
      {
        int index = base.Count - 1;
        node = this[index].Nodes.GetLastNode();
        if (node == null)
        {
          node = this[index];
        }
      }
      return node;
    }
    public Node GetLastDisplayedNode()
    {
      Node node = null;
      if (base.Count > 0)
      {
        for (int i = base.Count - 1; i >= 0; i--)
        {
          if (this[i].Visible)
          {
            node = this[i];
            break;
          }
        }
        if (node != null && node.Expanded)
        {
          Node lastDisplayedNode = node.Nodes.GetLastDisplayedNode();
          if (lastDisplayedNode != null)
          {
            node = lastDisplayedNode;
          }
        }
      }
      return node;
    }
    public object Clone()
    {
      NodeCollection nodeCollection = new NodeCollection();
      IEnumerator enumerator = base.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          nodeCollection.Add((Node)node.Clone());
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      return nodeCollection;
    }
    protected override void OnInsertComplete(int index, object value)
    {
      base.OnInsertComplete(index, value);
      Node node = value as Node;
      node.SetReferences(this.ParentNode, this, this.TreeControl);
      if (node.Visible)
      {
        _visibleCount++;
      }
      node.VisibleChanged += new EventHandler(this.OnNodeVisibleChanged);
      if (this.Cache.TreeControl != null)
      {
        this.Cache.TreeControl.InvalidateNodeDrawing();
      }
    }
    protected override void OnRemove(int index, object value)
    {
      Node node = value as Node;
      node.Removing = true;
      INodeVC vC = node.VC;
      if (vC != null && this.Cache.TreeControl != null)
      {
        vC.NodeRemoving(this.Cache.TreeControl, node);
      }
      base.OnRemove(index, value);
    }
    protected override void OnRemoveComplete(int index, object value)
    {
      Node node = value as Node;
      node.VisibleChanged -= new EventHandler(this.OnNodeVisibleChanged);
      if (node.Visible)
      {
        _visibleCount--;
      }
      node.SetReferences(null, null, null);
      if (this.Cache.TreeControl != null)
      {
        this.Cache.TreeControl.InvalidateNodeDrawing();
      }
      base.OnRemoveComplete(index, value);
    }
    protected override void OnSetComplete(int index, object oldValue, object newValue)
    {
      Node node = oldValue as Node;
      Node node2 = newValue as Node;
      node.SetReferences(null, null, null);
      node2.SetReferences(this.ParentNode, this, this.TreeControl);
      if (node.Visible)
      {
        _visibleCount--;
      }
      if (node2.Visible)
      {
        _visibleCount++;
      }
      node.VisibleChanged -= new EventHandler(this.OnNodeVisibleChanged);
      node2.VisibleChanged += new EventHandler(this.OnNodeVisibleChanged);
      if (this.Cache.TreeControl != null)
      {
        this.Cache.TreeControl.InvalidateNodeDrawing();
      }
      base.OnSetComplete(index, oldValue, newValue);
    }
    protected override void OnClear()
    {
      INodeCollectionVC vC = this.VC;
      if (vC != null && this.Cache.TreeControl != null)
      {
        vC.NodeCollectionClearing(this.Cache.TreeControl, this);
        this.Cache.TreeControl.InvalidateNodeDrawing();
      }
      IEnumerator enumerator = base.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.VisibleChanged -= new EventHandler(this.OnNodeVisibleChanged);
          node.SetReferences(null, null, null);
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
      _visibleCount = 0;
      base.OnClear();
    }
    protected virtual void OnVCChanged()
    {
      if (this.VCChanged != null)
      {
        this.VCChanged.Invoke(this, EventArgs.Empty);
      }
      if (this.Cache.TreeControl != null)
      {
        this.Cache.TreeControl.InvalidateNodeDrawing();
      }
    }
    internal void SetTreeControl(TreeControl tl)
    {
      this.Cache.TreeControl = tl;
      IEnumerator enumerator = base.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          node.SetTreeControl(tl);
        }
      }
      finally
      {
        IDisposable disposable = enumerator as IDisposable;
        if (disposable != null)
        {
          disposable.Dispose();
        }
      }
    }
    internal int ChildFromY(int y)
    {
      if (base.Count == 0)
      {
        return 0;
      }
      int num = 0;
      int num2 = base.Count - 1;
      int bottom;
      while (true)
      {
        int num3 = num + (num2 - num) / 2;
        bottom = this[num3].Cache.ChildBounds.Bottom;
        if (num == num2)
        {
          break;
        }
        if (bottom < y)
        {
          num = num3 + 1;
        }
        else
        {
          num2 = num3;
        }
      }
      if (bottom >= y)
      {
        return num2;
      }
      return num2 + 1;
    }
    private void OnNodeVisibleChanged(object sender, EventArgs e)
    {
      Node node = sender as Node;
      if (node.Visible)
      {
        _visibleCount++;
        return;
      }
      _visibleCount--;
    }
  }
}
