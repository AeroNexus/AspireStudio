using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  [TypeConverter(typeof(ExpandableObjectConverter))]
  public class IndentPaddingEdges
  {
    private Edges _edges;
    public event EventHandler IndentChanged;

    [DefaultValue(0), Description("Number of pixels to indent the left edge.")]
    public int Left
    {
      get
      {
        return _edges.Left;
      }
      set
      {
        if (value >= 0 && _edges.Left != value)
        {
          _edges.Left = value;
          this.OnIndentChanged();
        }
      }
    }
    [DefaultValue(0), Description("Number of pixels to indent the top edge.")]
    public int Top
    {
      get
      {
        return _edges.Top;
      }
      set
      {
        if (value >= 0 && _edges.Top != value)
        {
          _edges.Top = value;
          this.OnIndentChanged();
        }
      }
    }
    [DefaultValue(0), Description("Number of pixels to indent the right edge.")]
    public int Right
    {
      get
      {
        return _edges.Right;
      }
      set
      {
        if (value >= 0 && _edges.Right != value)
        {
          _edges.Right = value;
          this.OnIndentChanged();
        }
      }
    }
    [DefaultValue(0), Description("Number of pixels to indent the bottom edge.")]
    public int Bottom
    {
      get
      {
        return _edges.Bottom;
      }
      set
      {
        if (value >= 0 && _edges.Bottom != value)
        {
          _edges.Bottom = value;
          this.OnIndentChanged();
        }
      }
    }
    public IndentPaddingEdges()
    {
      _edges = default(Edges);
      this.ResetLeft();
      this.ResetTop();
      this.ResetRight();
      this.ResetBottom();
    }
    public void ResetLeft()
    {
      this.Left = 0;
    }
    public void ResetTop()
    {
      this.Top = 0;
    }
    public void ResetRight()
    {
      this.Right = 0;
    }
    public void ResetBottom()
    {
      this.Bottom = 0;
    }
    public override string ToString()
    {
      return string.Empty;
    }
    protected virtual void OnIndentChanged()
    {
      if (this.IndentChanged != null)
      {
        this.IndentChanged.Invoke(this, EventArgs.Empty);
      }
    }
  }
}
