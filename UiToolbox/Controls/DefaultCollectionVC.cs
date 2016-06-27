using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  public class DefaultCollectionVC : INodeCollectionVC
  {
    public virtual void Initialize(TreeControl tc)
    {
    }
    public virtual void Detaching(TreeControl tc)
    {
    }
    public virtual bool IsRootCollection(TreeControl tc, NodeCollection nc)
    {
      return tc.Nodes == nc;
    }
    public virtual Edges MeasureEdges(TreeControl tc, NodeCollection nc, Graphics g)
    {
      int num = tc.ColumnWidth;
      if (this.IsRootCollection(tc, nc))
      {
        if (!this.ShowLines(tc, nc) && !this.ShowBoxes(tc, nc))
        {
          num = 0;
        }
        if (tc.Indicators != Indicators.None)
        {
          num += tc.IndicatorSize.Width;
        }
      }
      else if (tc.Indicators == Indicators.AtGroup)
      {
        num += tc.IndicatorSize.Width;
      }
      return new Edges(num, 0, 0, 0);
    }
    public virtual void SetBounds(TreeControl tc, NodeCollection nc, Rectangle bounds)
    {
      nc.Cache.Bounds = bounds;
    }
    public virtual bool IntersectsWith(TreeControl tc, NodeCollection nc, Rectangle rectangle)
    {
      return nc.Cache.Bounds.IntersectsWith(rectangle);
    }
    public virtual void Draw(TreeControl tc, NodeCollection nc, Graphics g, Rectangle clipRectangle, bool preDraw)
    {
      if (preDraw && nc.VisibleCount > 0)
      {
        bool flag = this.ShowLines(tc, nc);
        bool flag2 = this.ShowBoxes(tc, nc);
        bool flag3 = (this.IsRootCollection(tc, nc) && tc.Indicators != Indicators.None) || (!this.IsRootCollection(tc, nc) && tc.Indicators == Indicators.AtGroup);
        if (flag || flag2 || flag3)
        {
          Pen cacheLineDashPen = tc.GetCacheLineDashPen();
          Rectangle bounds = nc.Cache.Bounds;
          Size size = flag3 ? tc.IndicatorSize : Size.Empty;
          this.AdjustBeforeDrawing(tc, nc, ref bounds);
          int num = bounds.Left + size.Width + tc.ColumnWidth / 2 - 1;
          int xRight = bounds.Left + size.Width + tc.ColumnWidth;
          int xMidAdjustL = num - tc.LineWidth / 2;
          int xMidAdjustR = num + 1;
          int firstVisibleIndex = nc.FirstVisibleIndex;
          int lastVisibleIndex = nc.LastVisibleIndex;
          int num2 = nc[firstVisibleIndex].Cache.Bounds.Top + nc[firstVisibleIndex].Cache.Bounds.Height / 2;
          int num3 = nc[lastVisibleIndex].Cache.Bounds.Top + nc[lastVisibleIndex].Cache.Bounds.Height / 2;
          if (num3 > clipRectangle.Bottom)
          {
            num3 = clipRectangle.Bottom;
          }
          if (flag)
          {
            if (this.IsRootCollection(tc, nc))
            {
              if (num2 < clipRectangle.Top)
              {
                num2 = clipRectangle.Top;
                switch (tc.LineDashStyle)
                {
                  case LineDashStyle.Dot:
                    num2 -= num2 % (2 * tc.LineWidth);
                    break;
                  case LineDashStyle.Dash:
                    num2 -= num2 % (4 * tc.LineWidth);
                    break;
                }
              }
              g.DrawLine(cacheLineDashPen, num, num2, num, num3);
            }
            else
            {
              g.DrawLine(cacheLineDashPen, num, bounds.Top, num, num3);
            }
          }
          for (int i = nc.ChildFromY(clipRectangle.Top); i < nc.Count; i++)
          {
            if (this.DrawNode(tc, nc[i], g, clipRectangle, flag, flag2, flag3, i == 0 || i == nc.Count - 1, cacheLineDashPen, xMidAdjustL, xMidAdjustR, num, xRight, bounds.Left))
            {
              return;
            }
          }
        }
      }
    }
    public virtual bool DrawNode(TreeControl tc, Node n, Graphics g, Rectangle clipRectangle, bool showLines, bool showBoxes, bool showIndicators, bool firstOrLast, Pen lineDashPen, int xMidAdjustL, int xMidAdjustR, int xMid, int xRight, int xLeft)
    {
      if (n.Visible)
      {
        Rectangle bounds = n.Cache.Bounds;
        if (bounds.Top > clipRectangle.Bottom)
        {
          return true;
        }
        int num = bounds.Top + bounds.Height / 2;
        if (showLines)
        {
          if (firstOrLast)
          {
            g.DrawLine(lineDashPen, xMidAdjustL, num, xRight, num);
          }
          else
          {
            g.DrawLine(lineDashPen, xMidAdjustR, num, xRight, num);
          }
        }
        if (showBoxes)
        {
          this.DrawExpandCollapseBox(tc, n, g, xMid, num);
        }
        if (showIndicators)
        {
          if (n.Indicator != Indicator.None)
          {
            this.DrawIndicator(tc, n, g, xLeft, num);
          }
          if (tc.Indicators == Indicators.AtRoot && n.Expanded)
          {
            for (int i = 0; i < n.Nodes.Count; i++)
            {
              Node node = n.Nodes[i];
              if (node.Visible && this.DrawNode(tc, node, g, clipRectangle, false, false, showIndicators, i == 0 || i == n.Nodes.Count - 1, lineDashPen, xMidAdjustL, xMidAdjustR, xMid, xRight, xLeft))
              {
                break;
              }
            }
          }
        }
      }
      return false;
    }
    public virtual void DrawIndicator(TreeControl tc, Node n, Graphics g, int x, int y)
    {
      Image indicatorImage = tc.GetIndicatorImage(n.Indicator);
      g.DrawImage(indicatorImage, x, y - tc.IndicatorSize.Height / 2);
      indicatorImage.Dispose();
    }
    public virtual void DrawExpandCollapseBox(TreeControl tc, Node n, Graphics g, int x, int y)
    {
      bool flag = n.Nodes.VisibleCount > 0;
      if (tc.BoxShownAlways || flag)
      {
        Rectangle rectangle = new Rectangle(x, y, 0, 0);
        if (tc.IsControlThemed && tc.BoxDrawStyle == DrawStyle.Themed)
        {
          if (flag)
          {
            int num = tc.GlyphThemeSize.Height / 2;
            int num2 = tc.GlyphThemeSize.Width / 2;
            rectangle.Y = rectangle.Y - num;
            rectangle.X = rectangle.X - num2;
            rectangle.Size = tc.GlyphThemeSize;
            tc.DrawThemedBox(g, rectangle, n.IsExpanded);
            return;
          }
        }
        else
        {
          bool flag2 = tc.BoxDrawStyle == DrawStyle.Gradient;
          int num3 = tc.BoxLength / 2;
          rectangle.X = rectangle.X - num3;
          rectangle.Y = rectangle.Y - num3;
          rectangle.Width = tc.BoxLength - 1;
          rectangle.Height = tc.BoxLength - 1;
          if (flag2)
          {
            Rectangle rectangle2 = rectangle;
            rectangle2.Inflate(1, 1);
            using (Brush brush = new LinearGradientBrush(rectangle2, tc.BoxInsideColor, tc.BackColor, 225f))
            {
              g.FillRectangle(brush, rectangle);
            }
            using (Pen pen = new Pen(tc.BoxBorderColor, 1f))
            {
              using (Pen pen2 = new Pen(ControlPaint.Light(tc.BoxBorderColor, 1f)))
              {
                g.DrawLine(pen, rectangle.Left + 1, rectangle.Bottom, rectangle.Right - 1, rectangle.Bottom);
                g.DrawLine(pen, rectangle.Right, rectangle.Top + 1, rectangle.Right, rectangle.Bottom - 1);
                g.DrawLine(pen2, rectangle.Left + 1, rectangle.Top, rectangle.Right - 1, rectangle.Top);
                g.DrawLine(pen2, rectangle.Left, rectangle.Top + 1, rectangle.Left, rectangle.Bottom - 1);
              }
              goto IL_235;
            }
          }
          g.FillRectangle(tc.GetCacheBoxInsideBrush(), rectangle);
          g.DrawRectangle(tc.GetCacheBoxBorderPen(), rectangle);
        IL_235:
          if (flag)
          {
            if (tc.BoxLength >= 7)
            {
              Pen cacheBoxSignPen = tc.GetCacheBoxSignPen();
              int num4 = tc.BoxLength / 3 - 1;
              g.DrawLine(cacheBoxSignPen, rectangle.X + num3, rectangle.Y + num3, rectangle.X + num3 - num4, rectangle.Y + num3);
              g.DrawLine(cacheBoxSignPen, rectangle.X + num3, rectangle.Y + num3, rectangle.X + num3 + num4, rectangle.Y + num3);
              if (!n.IsExpanded)
              {
                g.DrawLine(cacheBoxSignPen, rectangle.X + num3, rectangle.Y + num3, rectangle.X + num3, rectangle.Y + num3 - num4);
                g.DrawLine(cacheBoxSignPen, rectangle.X + num3, rectangle.Y + num3, rectangle.X + num3, rectangle.Y + num3 + num4);
                return;
              }
            }
          }
          else
          {
            g.DrawLine(tc.GetCacheBoxSignPen(), new PointF((float)(rectangle.X + num3), (float)(rectangle.Y + num3)), new PointF((float)(rectangle.X + num3) + 0.1f, (float)(rectangle.Y + num3)));
          }
        }
      }
    }
    public virtual void AdjustBeforeDrawing(TreeControl tc, NodeCollection nc, ref Rectangle ncBounds)
    {
    }
    public virtual bool ShowLines(TreeControl tc, NodeCollection nc)
    {
      if (tc.ColumnWidth == 0)
      {
        return false;
      }
      switch (tc.LineVisibility)
      {
        case LineBoxVisibility.Nowhere:
          return false;
        case LineBoxVisibility.OnlyAtRoot:
          return this.IsRootCollection(tc, nc);
        case LineBoxVisibility.OnlyBelowRoot:
          return !this.IsRootCollection(tc, nc);
        case LineBoxVisibility.Everywhere:
          return true;
        default:
          return true;
      }
    }
    public virtual bool ShowBoxes(TreeControl tc, NodeCollection nc)
    {
      if (tc.ColumnWidth == 0)
      {
        return false;
      }
      switch (tc.BoxVisibility)
      {
        case LineBoxVisibility.Nowhere:
          return false;
        case LineBoxVisibility.OnlyAtRoot:
          return this.IsRootCollection(tc, nc);
        case LineBoxVisibility.OnlyBelowRoot:
          return !this.IsRootCollection(tc, nc);
        case LineBoxVisibility.Everywhere:
          return true;
        default:
          return true;
      }
    }
    public virtual bool MouseDown(TreeControl tc, NodeCollection nc, Node n, MouseButtons button, Point pt)
    {
      if (button == MouseButtons.Left && n != null && n.Nodes.VisibleCount > 0 && this.ShowBoxes(tc, nc))
      {
        Rectangle expandCollapseBox = this.GetExpandCollapseBox(tc, nc, n);
        expandCollapseBox.Inflate(2, 2);
        if (expandCollapseBox.Contains(pt))
        {
          if (n.Expanded)
          {
            if (n.VC.CanCollapseNode(tc, n, false, true))
            {
              n.Collapse();
            }
          }
          else if (n.VC.CanExpandNode(tc, n, false, true))
          {
            n.Expand();
          }
          return true;
        }
      }
      return false;
    }
    public virtual bool DoubleClick(TreeControl tc, NodeCollection nc, Node n, Point pt)
    {
      return false;
    }
    public virtual Rectangle GetExpandCollapseBox(TreeControl tc, NodeCollection nc, Node n)
    {
      Rectangle bounds = nc.Cache.Bounds;
      Rectangle bounds2 = n.Cache.Bounds;
      this.AdjustBeforeDrawing(tc, nc, ref bounds);
      Size size = ((this.IsRootCollection(tc, nc) && tc.Indicators != Indicators.None) || (!this.IsRootCollection(tc, nc) && tc.Indicators == Indicators.AtGroup)) ? tc.IndicatorSize : Size.Empty;
      int num = bounds.Left + size.Width + tc.ColumnWidth / 2 - 1;
      int num2 = bounds2.Top + bounds2.Height / 2;
      Rectangle result = new Rectangle(num, num2, 0, 0);
      if (tc.IsControlThemed && tc.BoxDrawStyle == DrawStyle.Themed)
      {
        int num3 = tc.GlyphThemeSize.Height / 2;
        int num4 = tc.GlyphThemeSize.Width / 2;
        result.Y = result.Y - num3;
        result.X = result.X - num4;
        result.Size = tc.GlyphThemeSize;
      }
      else
      {
        int num5 = tc.BoxLength / 2;
        result.X = result.X - num5;
        result.Y = result.Y - num5;
        result.Width = tc.BoxLength - 1;
        result.Height = tc.BoxLength - 1;
      }
      return result;
    }
    public virtual void NodeCollectionClearing(TreeControl tc, NodeCollection nc)
    {
      IEnumerator enumerator = nc.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          var n = enumerator.Current as Node;
          tc.DeselectNode(n, true);
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
      tc.NodeContentCleared(false);
    }
    public virtual void SizeChanged(TreeControl tc)
    {
    }
    public virtual void PostDrawNodes(TreeControl tc, Graphics g, ArrayList displayNodes)
    {
    }
    public virtual void PostCalculateNodes(TreeControl tc, ArrayList displayNodes)
    {
    }
  }
}
