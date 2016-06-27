using System;
using System.Collections;
using System.Drawing;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  public class GroupCollectionVC : DefaultCollectionVC
  {
    public virtual bool IsRealRootCollection(TreeControl tc, NodeCollection nc)
    {
      return tc.Nodes == nc;
    }
    public override bool IsRootCollection(TreeControl tc, NodeCollection nc)
    {
      return nc.ParentNode != null && nc.ParentNode.ParentNodes == tc.Nodes;
    }
    public override void SetBounds(TreeControl tc, NodeCollection nc, Rectangle bounds)
    {
      if (this.IsRootCollection(tc, nc))
      {
        bounds.X = bounds.X + tc.GroupIndentLeft;
        bounds.Width = bounds.Width - tc.GroupIndentLeft;
      }
      base.SetBounds(tc, nc, bounds);
    }
    public override Edges MeasureEdges(TreeControl tc, NodeCollection nc, Graphics g)
    {
      if (this.IsRealRootCollection(tc, nc))
      {
        return Edges.Empty;
      }
      Edges result = base.MeasureEdges(tc, nc, g);
      if (nc.VisibleCount > 0 && this.IsRootCollection(tc, nc))
      {
        result.Left += tc.GroupIndentLeft;
        result.Top += tc.GroupIndentTop;
        result.Bottom += tc.GroupIndentBottom;
        if (tc.GroupImageBox && tc.GroupImageBoxColumn)
        {
          result.Left += tc.GroupImageBoxWidth;
        }
      }
      return result;
    }
    public override void Draw(TreeControl tc, NodeCollection nc, Graphics g, Rectangle clipRectangle, bool preDraw)
    {
      if (!this.IsRealRootCollection(tc, nc))
      {
        base.Draw(tc, nc, g, clipRectangle, preDraw);
        if (!preDraw && this.IsRootCollection(tc, nc) && tc.GroupImageBox && tc.GroupImageBoxColumn)
        {
          Rectangle rectangle = tc.ClientToNodeSpace(tc.DrawRectangle);
          Rectangle bounds = nc.Cache.Bounds;
          bool flag = bounds.Top <= rectangle.Top && bounds.Bottom >= rectangle.Top;
          bounds.X = bounds.X - tc.GroupIndentLeft;
          bounds.Width = tc.GroupImageBoxWidth;
          g.FillRectangle(tc.GetCacheGroupImageBoxColumnBrush(), bounds);
          g.DrawLine(tc.GetCacheGroupImageBoxLinePen(), bounds.Right, bounds.Top, bounds.Right, bounds.Bottom - 1);
          if (tc.BorderStyle == TreeBorderStyle.None)
          {
            g.DrawLine(tc.GetCacheGroupImageBoxLinePen(), rectangle.Right - 1, bounds.Top, rectangle.Right - 1, bounds.Bottom - 1);
            if (flag)
            {
              g.DrawLine(tc.GetCacheGroupImageBoxLinePen(), bounds.Right, rectangle.Top, rectangle.Right - 1, rectangle.Top);
            }
            bool flag2 = true;
            for (int i = nc.ParentNode.Index + 1; i < nc.ParentNode.ParentNodes.Count; i++)
            {
              Node node = nc.ParentNode.ParentNodes[i];
              if (node.Visible)
              {
                flag2 = false;
                break;
              }
            }
            if (flag2 || bounds.Bottom > rectangle.Bottom)
            {
              int num = bounds.Bottom - 1;
              if (num >= rectangle.Bottom)
              {
                num = rectangle.Bottom - 1;
              }
              g.DrawLine(tc.GetCacheGroupImageBoxLinePen(), bounds.Right, num, rectangle.Right - 1, num);
            }
          }
        }
      }
    }
    public override void AdjustBeforeDrawing(TreeControl tc, NodeCollection nc, ref Rectangle ncBounds)
    {
      if (this.IsRootCollection(tc, nc) && tc.GroupImageBox && tc.GroupImageBoxColumn)
      {
        ncBounds.X = ncBounds.X + tc.GroupImageBoxWidth;
        ncBounds.Width = ncBounds.Width - tc.GroupImageBoxWidth;
      }
    }
    public override void SizeChanged(TreeControl tc)
    {
      if (tc.GroupAutoAllocate)
      {
        tc.InvalidateNodeDrawing();
      }
    }
    public override void PostDrawNodes(TreeControl tc, Graphics g, ArrayList displayNodes)
    {
      if (tc.GroupImageBox)
      {
        Rectangle bounds = tc.Nodes.Cache.Bounds;
        Rectangle rectangle = tc.ClientToNodeSpace(tc.DrawRectangle);
        if (bounds.Bottom < rectangle.Bottom)
        {
          Rectangle rectangle2 = new Rectangle(rectangle.Left, bounds.Bottom, rectangle.Width, rectangle.Bottom - bounds.Bottom);
          if (tc.BorderStyle != TreeBorderStyle.None)
          {
            rectangle2.Width = tc.GroupImageBoxWidth;
            g.FillRectangle(tc.GetCacheGroupImageBoxColumnBrush(), rectangle2);
            g.DrawLine(tc.GetCacheGroupImageBoxLinePen(), rectangle2.Right, rectangle2.Top, rectangle2.Right, rectangle2.Bottom);
            return;
          }
          g.FillRectangle(tc.GetCacheGroupImageBoxColumnBrush(), rectangle2);
        }
      }
    }
    public override void PostCalculateNodes(TreeControl tc, ArrayList displayNodes)
    {
      if (tc.GroupAutoAllocate)
      {
        int height = tc.Nodes.Cache.Bounds.Height;
        int height2 = tc.DrawRectangle.Height;
        if (tc.VerticalGranularity == VerticalGranularity.Pixel && height < height2)
        {
          int num = height2 - height;
          Node node = null;
          for (int i = tc.Nodes.Count - 1; i >= 0; i--)
          {
            Node node2 = tc.Nodes[i];
            if (node2.Visible && node2.Expanded)
            {
              node = node2;
              break;
            }
          }
          if (node != null)
          {
            Rectangle bounds = tc.Nodes.Cache.Bounds;
            Rectangle childBounds = node.Cache.ChildBounds;
            Rectangle bounds2 = node.Nodes.Cache.Bounds;
            bounds.Height = bounds.Height + num;
            childBounds.Height = childBounds.Height + num;
            bounds2.Height = bounds2.Height + num;
            tc.Nodes.Cache.Bounds = bounds;
            node.Cache.ChildBounds = childBounds;
            node.Nodes.Cache.Bounds = bounds2;
            Node node3 = null;
            for (int j = tc.Nodes.IndexOf(node) + 1; j <= tc.Nodes.Count - 1; j++)
            {
              if (tc.Nodes[j].Visible)
              {
                node3 = tc.Nodes[j];
                break;
              }
            }
            if (node3 != null)
            {
              bool flag = false;
              IEnumerator enumerator = displayNodes.GetEnumerator();
              try
              {
                while (enumerator.MoveNext())
                {
                  Node node4 = (Node)enumerator.Current;
                  if (!flag && node4 == node3)
                  {
                    flag = true;
                  }
                  if (flag)
                  {
                    Rectangle bounds3 = node4.Cache.Bounds;
                    Rectangle childBounds2 = node4.Cache.ChildBounds;
                    bounds3.Offset(0, num);
                    childBounds2.Offset(0, num);
                    node4.Cache.Bounds = bounds3;
                    node4.Cache.ChildBounds = childBounds2;
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
          }
        }
      }
    }
  }
}
