using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  public class GroupNodeVC : DefaultNodeVC
  {
    private static Image _upArrow;
    private static Image _downArrow;
    private static int _arrowHeight;
    private static int _arrowWidth;
    private static int _arrowHeightSpace;
    private static int _arrowWidthSpace;
    private static ImageAttributes _imageAttr;
    static GroupNodeVC()
    {
      GroupNodeVC._upArrow = ResourceHelper.LoadBitmap(typeof(GroupNodeVC), "Crownwood.DotNetMagic.Controls.TitleBar.ImageUp.bmp", Point.Empty);
      GroupNodeVC._downArrow = ResourceHelper.LoadBitmap(typeof(GroupNodeVC), "Crownwood.DotNetMagic.Controls.TitleBar.ImageDown.bmp", Point.Empty);
      GroupNodeVC._arrowWidth = GroupNodeVC._upArrow.Width;
      GroupNodeVC._arrowHeight = GroupNodeVC._upArrow.Height;
      GroupNodeVC._arrowWidthSpace = GroupNodeVC._arrowWidth + 2;
      GroupNodeVC._arrowHeightSpace = GroupNodeVC._arrowHeight + 2;
      GroupNodeVC._imageAttr = new ImageAttributes();
    }
    public override void Initialize(TreeControl tc)
    {
      tc.GroupAutoCollapseChanged += new EventHandler(this.OnAutoCollapseChanged);
      this.EnforceSingleExpandedGroup(tc);
    }
    public override void Detaching(TreeControl tc)
    {
      tc.GroupAutoCollapseChanged -= new EventHandler(this.OnAutoCollapseChanged);
    }
    public override Size MeasureSize(TreeControl tc, Node n, Graphics g)
    {
      if (!this.IsRootNode(tc, n))
      {
        return base.MeasureSize(tc, n, g);
      }
      if (n.Cache.IsSizeDirty)
      {
        SizeF sizeF;
        if (tc.GroupUseHotFontStyle || tc.GroupUseSelectedFontStyle)
        {
          Font nodeFont = n.GetNodeFont();
          Font font = (nodeFont == null) ? tc.GetGroupFontBoldItalic() : new Font(nodeFont, FontStyle.Bold);
          sizeF = g.MeasureString(n.Text, font);
          if (nodeFont != null)
          {
            font.Dispose();
          }
        }
        else
        {
          Font font2 = n.GetNodeFont();
          if (font2 == null)
          {
            font2 = tc.GroupFont;
          }
          sizeF = g.MeasureString(n.Text, font2);
        }
        Size size = new Size((int)(sizeF.Width + 1f), (int)((double)n.GetNodeGroupFontHeight() * 1.25));
        size.Width = size.Width + tc.GroupExtraLeft;
        size.Height = size.Height + tc.GroupExtraHeight;
        Size checkSize = this.GetCheckSize(tc, n);
        if (checkSize != Size.Empty)
        {
          size.Width = size.Width + (tc.CheckGapLeft + checkSize.Width + tc.CheckGapRight);
          if (size.Height < checkSize.Height)
          {
            size.Height = checkSize.Height;
          }
        }
        if (tc.GroupArrows)
        {
          size.Width = size.Width + GroupNodeVC._arrowWidthSpace;
          if (size.Height < GroupNodeVC._arrowHeightSpace)
          {
            size.Height = GroupNodeVC._arrowHeightSpace;
          }
        }
        if (tc.GroupImageBox)
        {
          size.Width = size.Width + (tc.GroupImageBoxWidth + tc.GroupImageBoxGap);
        }
        else
        {
          Size imageSize = this.GetImageSize(tc, n);
          if (imageSize != Size.Empty)
          {
            size.Width = size.Width + (tc.ImageGapLeft + imageSize.Width + tc.ImageGapRight);
            if (size.Height < imageSize.Height)
            {
              size.Height = imageSize.Height;
            }
          }
        }
        n.Cache.Size = size;
      }
      return n.Cache.Size;
    }
    public override bool IntersectsWith(TreeControl tc, Node n, Rectangle rectangle, bool recurse)
    {
      if (!this.IsRootNode(tc, n))
      {
        return base.IntersectsWith(tc, n, rectangle, recurse);
      }
      Rectangle rectangle2;
      if (recurse)
      {
        rectangle2 = n.Cache.ChildBounds;
      }
      else
      {
        rectangle2 = n.Cache.Bounds;
      }
      return (rectangle.Top >= rectangle2.Top && rectangle.Top <= rectangle2.Bottom) || (rectangle.Bottom >= rectangle2.Top && rectangle.Bottom <= rectangle2.Bottom) || (rectangle.Top < rectangle2.Top && rectangle.Bottom > rectangle2.Bottom);
    }
    public override void Draw(TreeControl tc, Node n, Graphics g, Rectangle clipRectangle, int leftOffset, int rightOffset)
    {
      bool flag = true;
      if (this.IsRootNode(tc, n))
      {
        bool hotNode = tc.GroupHotTrack && tc.HotNode == n;
        bool flag2 = tc.DragOverNode == n;
        bool flag3 = n.IsSelected;
        if (!tc.GroupNodesSelectable)
        {
          flag3 = n.Expanded;
        }
        Rectangle bounds = n.Cache.Bounds;
        Point point = tc.ClientToNodeSpace(new Point(tc.DrawRectangle.Right, 0));
        bounds.X = 0;
        bounds.Width = (point.X > bounds.Width) ? point.X : bounds.Width;
        leftOffset += tc.GroupExtraLeft;
        bounds.Width = bounds.Width - rightOffset;
        if (bounds.Width > 0 && bounds.Height > 0)
        {
          if (tc.GroupColoring != GroupColoring.ControlProperties)
          {
            Color empty = Color.Empty;
            Color empty2 = Color.Empty;
            this.GetGroupColouringOffice2003(tc, flag3, hotNode, ref empty, ref empty2);
            using (Brush brush = new LinearGradientBrush(bounds, empty, empty2, 90f))
            {
              g.FillRectangle(brush, bounds);
              goto IL_1AC;
            }
          }
          Color backColor = this.GetBackColor(tc, n, hotNode, flag3 || flag2);
          Brush brush2;
          if (tc.GroupGradientBack)
          {
            Rectangle rectangle = bounds;
            rectangle.Inflate(bounds.Width / 4, bounds.Height / 4);
            Color empty3 = Color.Empty;
            Color empty4 = Color.Empty;
            tc.GradientColors(tc.GroupGradientColoring, backColor, ref empty3, ref empty4);
            brush2 = new LinearGradientBrush(rectangle, empty3, empty4, (float)tc.GroupGradientAngle);
          }
          else
          {
            brush2 = new SolidBrush(backColor);
          }
          g.FillRectangle(brush2, bounds);
          brush2.Dispose();
        IL_1AC:
          Rectangle rect = this.DrawGroupBorder(tc, n, g, bounds);
          if (tc.GroupImageBox)
          {
            Rectangle rect2 = new Rectangle(bounds.Left, bounds.Top, tc.GroupImageBoxWidth, bounds.Height);
            Rectangle rect3 = new Rectangle(rect2.Right + tc.GroupImageBoxGap, bounds.Top, bounds.Width - rect2.Width - tc.GroupImageBoxGap, bounds.Height);
            this.DrawImageBox(tc, n, g, rect2);
            this.DrawImageInsideImageBox(tc, n, g, rect2);
            if (tc.GroupArrows)
            {
              Rectangle rect4 = new Rectangle(rect3.Right - GroupNodeVC._arrowWidthSpace, rect3.Top, GroupNodeVC._arrowWidthSpace, rect3.Height);
              if (tc.GroupColoring != GroupColoring.ControlProperties)
              {
                hotNode = false;
              }
              this.DrawArrow(tc, n, g, rect4, this.GetForeColor(tc, n, hotNode, flag3));
              rect3.Width = rect3.Width - GroupNodeVC._arrowWidthSpace;
            }
            this.DrawText(tc, n, g, rect3, rect3.Left);
            int num = rect2.Right - rect.Left + 1;
            rect.X = rect.X + num;
            rect.Width = rect.Width - num;
            flag = false;
          }
          else if (tc.GroupArrows)
          {
            Rectangle rect5 = new Rectangle(bounds.Right - GroupNodeVC._arrowWidthSpace, bounds.Top, GroupNodeVC._arrowWidthSpace, bounds.Height);
            this.DrawArrow(tc, n, g, rect5, this.GetForeColor(tc, n, hotNode, flag3));
            rightOffset = GroupNodeVC._arrowWidthSpace;
          }
          if (tc.ContainsFocus && tc.FocusNode == n)
          {
            base.DrawFocusIndication(tc, n, g, rect);
          }
        }
      }
      if (flag)
      {
        base.Draw(tc, n, g, clipRectangle, leftOffset, rightOffset);
      }
    }
    public virtual void GetGroupColouringOffice2003(TreeControl tc, bool selectedNode, bool hotNode, ref Color top, ref Color bottom)
    {
      if (selectedNode)
      {
        if (hotNode)
        {
          top = tc.ColorDetails.TrackLightLightColor2;
          bottom = tc.ColorDetails.TrackLightLightColor1;
          return;
        }
        if (tc.ContainsFocus)
        {
          top = tc.ColorDetails.TrackLightLightColor1;
          bottom = tc.ColorDetails.TrackLightLightColor2;
          return;
        }
        if (tc.GroupColoring == GroupColoring.Office2003Light)
        {
          top = tc.ColorDetails.BaseColor1;
          bottom = tc.ColorDetails.BaseColor2;
          return;
        }
        top = tc.ColorDetails.DarkBaseColor;
        bottom = tc.ColorDetails.DarkBaseColor2;
        return;
      }
      else
      {
        if (hotNode)
        {
          top = tc.ColorDetails.TrackLightColor1;
          bottom = tc.ColorDetails.TrackLightColor2;
          return;
        }
        if (tc.GroupColoring == GroupColoring.Office2003Light)
        {
          top = tc.ColorDetails.BaseColor1;
          bottom = tc.ColorDetails.BaseColor2;
          return;
        }
        top = tc.ColorDetails.DarkBaseColor;
        bottom = tc.ColorDetails.DarkBaseColor2;
        return;
      }
    }
    public override void DrawFocusIndication(TreeControl tc, Node n, Graphics g, Rectangle rect)
    {
    }
    public virtual Rectangle DrawGroupBorder(TreeControl tc, Node n, Graphics g, Rectangle rect)
    {
      Rectangle result = rect;
      if (tc.GroupBorderStyle != GroupBorderStyle.None)
      {
        bool hotNode = tc.GroupHotTrack && tc.HotNode == n;
        bool isSelected = n.IsSelected;
        bool flag = tc.BorderStyle == TreeBorderStyle.None;
        bool flag2 = tc.BorderIndent.Left > 0 || flag;
        bool flag3 = tc.BorderIndent.Right > 0 || flag;
        Point point = tc.ClientToNodeSpace(new Point(tc.DrawRectangle.Right, tc.DrawRectangle.Top));
        if (tc.GroupBorderStyle != GroupBorderStyle.BottomEdge)
        {
          bool flag4 = tc.IsFirstDisplayedNode(n) && (tc.BorderIndent.Top > 0 || flag);
          if (!flag4 && flag)
          {
            Rectangle bounds = n.Cache.Bounds;
            flag4 = (bounds.Top <= point.Y && bounds.Bottom >= point.Y);
          }
          if (!flag4)
          {
            int i = n.Index - 1;
            while (i >= 0)
            {
              Node node = n.ParentNodes[i];
              if (node.Visible)
              {
                if (node.Expanded && node.Bounds.Bottom < n.Bounds.Top - 1)
                {
                  flag4 = true;
                  break;
                }
                break;
              }
              else
              {
                i--;
              }
            }
          }
          if (flag4)
          {
            if (tc.GroupColoring != GroupColoring.ControlProperties)
            {
              Color empty = Color.Empty;
              Color empty2 = Color.Empty;
              this.GetGroupColouringOffice2003(tc, isSelected, hotNode, ref empty, ref empty2);
              using (Pen pen = new Pen(empty2))
              {
                g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Top);
                goto IL_1E0;
              }
            }
            g.DrawLine(tc.GetCacheGroupLinePen(), rect.Left, rect.Top, rect.Right, rect.Top);
          IL_1E0:
            result.Y = result.Y + 1;
            result.Height = result.Height - 1;
          }
        }
        if (tc.GroupBorderStyle == GroupBorderStyle.VerticalEdges || tc.GroupBorderStyle == GroupBorderStyle.BottomEdge)
        {
          flag2 = false;
          flag3 = false;
        }
        if (!tc.IsLastDisplayedNode(n) || tc.BorderIndent.Bottom != 0 || flag || rect.Bottom - point.Y != tc.DrawRectangle.Bottom - 1)
        {
          if (tc.GroupColoring != GroupColoring.ControlProperties)
          {
            using (Pen pen2 = new Pen(tc.ColorDetails.ActiveBorderColor))
            {
              g.DrawLine(pen2, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
              goto IL_2D6;
            }
          }
          g.DrawLine(tc.GetCacheGroupLinePen(), rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
        }
      IL_2D6:
        result.Height = result.Height - 1;
        if (tc.GroupBorderStyle != GroupBorderStyle.VerticalEdges && tc.GroupBorderStyle != GroupBorderStyle.BottomEdge)
        {
          if (flag3 || tc.DrawRectangle.Right != tc.InnerRectangle.Right)
          {
            g.DrawLine(tc.GetCacheGroupLinePen(), rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
            result.Width = result.Width - 1;
          }
          if (flag2)
          {
            g.DrawLine(tc.GetCacheGroupLinePen(), rect.Left, rect.Top, rect.Left, rect.Bottom - 1);
            result.X = result.X + 1;
            result.Width = result.Width - 1;
          }
        }
      }
      return result;
    }
    public virtual void DrawImageBox(TreeControl tc, Node n, Graphics g, Rectangle rect)
    {
      bool flag = n.IsSelected;
      if (!tc.GroupNodesSelectable)
      {
        flag = n.Expanded;
      }
      Color color;
      if (flag)
      {
        color = tc.GroupImageBoxSelectedBackColor;
      }
      else
      {
        color = tc.GroupImageBoxBackColor;
      }
      Brush brush;
      if (tc.GroupImageBoxGradientBack)
      {
        Rectangle rectangle = rect;
        rectangle.Inflate(rect.Width / 4, rect.Height / 4);
        Color empty = Color.Empty;
        Color empty2 = Color.Empty;
        tc.GradientColors(tc.GroupImageBoxGradientColoring, color, ref empty, ref empty2);
        brush = new LinearGradientBrush(rectangle, empty, empty2, (float)tc.GroupImageBoxGradientAngle);
      }
      else
      {
        brush = new SolidBrush(color);
      }
      g.FillRectangle(brush, rect);
      brush.Dispose();
      if (tc.GroupImageBoxBorder)
      {
        Pen cacheGroupImageBoxLinePen = tc.GetCacheGroupImageBoxLinePen();
        Point point = tc.ClientToNodeSpace(new Point(tc.DrawRectangle.Right, tc.DrawRectangle.Top));
        bool flag2 = tc.BorderStyle == TreeBorderStyle.None;
        bool flag3 = tc.IsFirstDisplayedNode(n) && (tc.BorderIndent.Top > 0 || flag2);
        bool flag4 = tc.IsLastDisplayedNode(n) && tc.BorderIndent.Bottom == 0 && !flag2 && rect.Bottom - point.Y == tc.InnerRectangle.Bottom - 1;
        g.DrawLine(cacheGroupImageBoxLinePen, rect.Right, rect.Bottom - 1, rect.Right, rect.Top);
        if (!flag4)
        {
          g.DrawLine(cacheGroupImageBoxLinePen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
        }
        if (flag2 || tc.BorderIndent.Left > 0)
        {
          g.DrawLine(cacheGroupImageBoxLinePen, rect.Left, rect.Top, rect.Left, rect.Bottom - 1);
        }
        if (!flag3 && flag2)
        {
          Rectangle bounds = n.Cache.Bounds;
          flag3 = (bounds.Top <= point.Y && bounds.Bottom >= point.Y);
        }
        if (!flag3)
        {
          int i = n.Index - 1;
          while (i >= 0)
          {
            Node node = n.ParentNodes[i];
            if (node.Visible)
            {
              if (node.Expanded && node.Bounds.Bottom < n.Bounds.Top - 1)
              {
                flag3 = true;
                break;
              }
              break;
            }
            else
            {
              i--;
            }
          }
        }
        if (flag3)
        {
          g.DrawLine(cacheGroupImageBoxLinePen, rect.Left, rect.Top, rect.Right, rect.Top);
        }
      }
    }
    public virtual void DrawImageInsideImageBox(TreeControl tc, Node n, Graphics g, Rectangle rect)
    {
      Size imageSize = this.GetImageSize(tc, n);
      if (imageSize != Size.Empty)
      {
        int num = 0;
        int num2 = 0;
        int height = imageSize.Height;
        int width = imageSize.Width;
        if (imageSize.Height > rect.Height)
        {
          height = rect.Height;
        }
        else
        {
          num = (rect.Height - imageSize.Height) / 2;
        }
        if (imageSize.Width > rect.Height)
        {
          width = rect.Width;
        }
        else
        {
          num2 = (rect.Width - imageSize.Width) / 2;
        }
        this.DrawImage(tc, n, g, rect.X + num2, rect.Y + num, width, height);
      }
    }
    public override void DrawText(TreeControl tc, Node n, Graphics g, Rectangle rect, int hotLeft)
    {
      if (tc.LabelEditNode != n)
      {
        if (!this.IsRootNode(tc, n))
        {
          g.TextRenderingHint = tc.TextRenderingHint;
          base.DrawText(tc, n, g, rect, hotLeft);
          return;
        }
        bool flag = tc.GroupHotTrack && tc.HotNode == n;
        bool flag2 = tc.DragOverNode == n;
        bool flag3 = n.IsSelected;
        if (!tc.GroupNodesSelectable)
        {
          flag3 = n.Expanded;
        }
        Color foreColor = this.GetForeColor(tc, n, flag, flag3 || flag2);
        Font font = n.GetNodeFont();
        if (font == null)
        {
          font = tc.GroupFont;
        }
        g.TextRenderingHint = tc.GroupTextRenderingHint;
        using (SolidBrush solidBrush = new SolidBrush(foreColor))
        {
          bool flag4 = false;
          if (flag && tc.GroupUseHotFontStyle)
          {
            using (Font font2 = new Font(font, tc.GroupHotFontStyle))
            {
              g.DrawString(n.Text, font2, solidBrush, rect, DefaultNodeVC._format);
            }
            flag4 = true;
          }
          else if (flag3 && tc.GroupUseSelectedFontStyle)
          {
            using (Font font3 = new Font(font, tc.GroupSelectedFontStyle))
            {
              g.DrawString(n.Text, font3, solidBrush, rect, DefaultNodeVC._format);
            }
            flag4 = true;
          }
          if (!flag4)
          {
            g.DrawString(n.Text, font, solidBrush, rect, DefaultNodeVC._format);
          }
        }
      }
    }
    public virtual void DrawArrow(TreeControl tc, Node n, Graphics g, Rectangle rect, Color color)
    {
      if (n.Nodes.VisibleCount > 0)
      {
        int num = (rect.Height - GroupNodeVC._arrowHeightSpace) / 2;
        Image image = n.Expanded ? GroupNodeVC._upArrow : GroupNodeVC._downArrow;
        ColorMap colorMap = new ColorMap();
        colorMap.OldColor = Color.White;
        colorMap.NewColor = color;
        GroupNodeVC._imageAttr.SetRemapTable(new ColorMap[]
				{
					colorMap
				}, ColorAdjustType.Bitmap);
        g.DrawImage(image, new Rectangle(rect.Left, rect.Top + num, GroupNodeVC._arrowWidth, GroupNodeVC._arrowHeight), 0, 0, GroupNodeVC._arrowWidth, GroupNodeVC._arrowHeight, GraphicsUnit.Pixel, GroupNodeVC._imageAttr);
      }
    }
    public override CheckStates GetCheckStates(TreeControl tc, Node n)
    {
      if (!this.IsRootNode(tc, n))
      {
        return base.GetCheckStates(tc, n);
      }
      CheckStates result;
      switch (n.CheckStates)
      {
        case NodeCheckStates.TwoStateCheck:
          result = CheckStates.TwoStateCheck;
          break;
        case NodeCheckStates.ThreeStateCheck:
          result = CheckStates.ThreeStateCheck;
          break;
        case NodeCheckStates.Radio:
          result = CheckStates.Radio;
          break;
        default:
          result = CheckStates.None;
          break;
      }
      return result;
    }
    public override bool ClickPointInNode(TreeControl tc, Node n, Rectangle rect, Point pt)
    {
      return n.ParentNodes == tc.Nodes || rect.Contains(pt);
    }
    public override bool CanSelectNode(TreeControl tc, Node n)
    {
      if (!this.IsRootNode(tc, n))
      {
        return base.CanSelectNode(tc, n);
      }
      return tc.GroupNodesSelectable && n.Selectable;
    }
    public override bool CanCollapseNode(TreeControl tc, Node n, bool key, bool mouse)
    {
      bool result = base.CanCollapseNode(tc, n, key, mouse);
      if (this.IsRootNode(tc, n))
      {
        result = (mouse || (key && (!tc.GroupAutoCollapse || !tc.GroupAutoAllocate)));
      }
      return result;
    }
    public override ClickExpandAction CanExpandOnClick(TreeControl tc, Node n)
    {
      if (n.ParentNodes != tc.Nodes)
      {
        return tc.ClickExpand;
      }
      return tc.GroupClickExpand;
    }
    public override bool CanAutoEdit(TreeControl tc, Node n)
    {
      if (n.ParentNodes != tc.Nodes)
      {
        return tc.AutoEdit;
      }
      return tc.GroupAutoEdit;
    }
    public override ClickExpandAction CanExpandOnDoubleClick(TreeControl tc, Node n)
    {
      if (n.ParentNodes != tc.Nodes)
      {
        return tc.DoubleClickExpand;
      }
      return tc.GroupDoubleClickExpand;
    }
    public override void NodeExpandedChanged(TreeControl tc, Node n)
    {
      if (n.ParentNodes != tc.Nodes)
      {
        base.NodeExpandedChanged(tc, n);
        return;
      }
      if (n.Expanded && tc.GroupAutoCollapse)
      {
        IEnumerator enumerator = n.ParentNodes.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node node = (Node)enumerator.Current;
            if (node != n)
            {
              node.Collapse();
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
        tc.InvalidateNodeDrawing();
      }
      if (!n.Expanded && tc.SelectedCount > 0)
      {
        IEnumerator enumerator = n.Nodes.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node node2 = (Node)enumerator.Current;
            if (node2.Visible)
            {
              tc.DeselectNode(node2, true);
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
    public override Node DragOverNodeFromPoint(TreeControl tc, Point pt)
    {
      Node node = tc.FindDisplayNodeFromY(pt.Y);
      if (this.IsRootNode(tc, node))
      {
        return node;
      }
      return base.DragOverNodeFromPoint(tc, pt);
    }
    public override void DragEnter(TreeControl tc, Node n, DragEventArgs drgevent)
    {
      if (this.IsRootNode(tc, n))
      {
        tc.OnGroupDragEnter(n, drgevent);
        return;
      }
      base.DragEnter(tc, n, drgevent);
    }
    public override void DragOver(TreeControl tc, Node n, DragEventArgs drgevent)
    {
      if (this.IsRootNode(tc, n))
      {
        tc.OnGroupDragOver(n, drgevent);
        return;
      }
      base.DragOver(tc, n, drgevent);
    }
    public override void DragLeave(TreeControl tc, Node n)
    {
      if (this.IsRootNode(tc, n))
      {
        tc.OnGroupDragLeave(n);
        return;
      }
      base.DragLeave(tc, n);
    }
    public override void DragDrop(TreeControl tc, Node n, DragEventArgs drgevent)
    {
      if (this.IsRootNode(tc, n))
      {
        tc.OnGroupDragDrop(n, drgevent);
        return;
      }
      base.DragDrop(tc, n, drgevent);
    }
    public override void DragHover(TreeControl tc, Node n)
    {
      if (this.IsRootNode(tc, n))
      {
        if (tc.GroupExpandOnDragHover && !n.Expanded && n.Nodes.VisibleCount > 0)
        {
          n.Expand();
          return;
        }
      }
      else
      {
        base.DragHover(tc, n);
      }
    }
    public override Font GetNodeFont(TreeControl tc, Node n)
    {
      if (!this.IsRootNode(tc, n))
      {
        return base.GetNodeFont(tc, n);
      }
      Font font = n.GetNodeFont();
      if (font == null)
      {
        font = tc.GroupFont;
      }
      return font;
    }
    public override Rectangle GetTextRectangle(TreeControl tc, Node n)
    {
      if (!this.IsRootNode(tc, n))
      {
        return base.GetTextRectangle(tc, n);
      }
      Rectangle bounds = n.Cache.Bounds;
      Point point = tc.ClientToNodeSpace(new Point(tc.DrawRectangle.Right, 0));
      bounds.X = 0;
      bounds.Width = (point.X > bounds.Width) ? point.X : bounds.Width;
      if (tc.GroupImageBox)
      {
        int num = tc.GroupImageBoxWidth + tc.GroupImageBoxGap;
        bounds.X = bounds.X + num;
        bounds.Width = bounds.Width - num;
      }
      else
      {
        Size imageSize = this.GetImageSize(tc, n);
        if (imageSize != Size.Empty)
        {
          int num2 = tc.ImageGapLeft + imageSize.Width + tc.ImageGapRight;
          bounds.X = bounds.X + num2;
          bounds.Width = bounds.Width - num2;
        }
      }
      if (tc.GroupArrows)
      {
        bounds.Width = bounds.Width - GroupNodeVC._arrowWidthSpace;
      }
      return bounds;
    }
    public virtual Color GetForeColor(TreeControl tc, Node n, bool hotNode, bool selectedNode)
    {
      Color color;
      if (selectedNode)
      {
        color = tc.GroupSelectedForeColor;
      }
      else if (hotNode && tc.GroupHotForeColor != Color.Empty)
      {
        color = tc.GroupHotForeColor;
      }
      else
      {
        color = n.GetNodeForeColor();
        if (color == Color.Empty)
        {
          color = tc.GroupForeColor;
        }
      }
      return color;
    }
    public virtual Color GetBackColor(TreeControl tc, Node n, bool hotNode, bool selectedNode)
    {
      Color result;
      if (selectedNode)
      {
        if (tc.ContainsFocus)
        {
          result = tc.GroupSelectedBackColor;
        }
        else
        {
          result = tc.GroupSelectedNoFocusBackColor;
        }
      }
      else if (hotNode && tc.GroupHotBackColor != Color.Empty)
      {
        result = tc.GroupHotBackColor;
      }
      else
      {
        result = tc.GroupBackColor;
      }
      return result;
    }
    private void EnforceSingleExpandedGroup(TreeControl tc)
    {
      bool flag = false;
      IEnumerator enumerator = tc.Nodes.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          Node node = (Node)enumerator.Current;
          if (node.Visible && node.Expanded)
          {
            if (!flag)
            {
              flag = true;
            }
            else
            {
              node.Collapse();
            }
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
    private void OnAutoCollapseChanged(object sender, EventArgs e)
    {
      this.EnforceSingleExpandedGroup(sender as TreeControl);
    }
  }
}
