using System.ComponentModel;

namespace Aspire.UiToolbox.Common
{
  public struct Edges
  {
    public static readonly Edges Empty = new Edges(0, 0, 0, 0);
    private int _left;
    private int _top;
    private int _right;
    private int _bottom;
    [DefaultValue(0), Description("left edge.")]
    public int Left
    {
      get
      {
        return _left;
      }
      set
      {
        _left = value;
      }
    }
    [DefaultValue(0), Description("Top edge.")]
    public int Top
    {
      get
      {
        return _top;
      }
      set
      {
        _top = value;
      }
    }
    [DefaultValue(0), Description("Right edge.")]
    public int Right
    {
      get
      {
        return _right;
      }
      set
      {
        _right = value;
      }
    }
    [DefaultValue(0), Description("Bottom edge.")]
    public int Bottom
    {
      get
      {
        return _bottom;
      }
      set
      {
        _bottom = value;
      }
    }
    public Edges(int left, int top, int right, int bottom)
    {
      _left = left;
      _top = top;
      _right = right;
      _bottom = bottom;
    }
  }
}
