using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Aspire.UiToolbox.Common;

namespace Aspire.UiToolbox.Controls
{
  public class DefaultNodeVC : INodeVC
  {
    protected static StringFormat _format;
    private bool _mouseDownSelected;
    private bool _mouseDoubleClick;
    static DefaultNodeVC()
    {
      DefaultNodeVC._format = new StringFormat();
      DefaultNodeVC._format.Alignment = 0;
      DefaultNodeVC._format.LineAlignment = StringAlignment.Center;
    }
    public virtual void Initialize(TreeControl tc)
    {
    }
    public virtual void Detaching(TreeControl tc)
    {
    }
    public virtual bool IsRootNode(TreeControl tc, Node n)
    {
      return n != null && n.ParentNodes == tc.Nodes;
    }
    public virtual Size MeasureSize(TreeControl tc, Node n, Graphics g)
    {
      if (n.Cache.IsSizeDirty)
      {
        Size textSize = this.GetTextSize(tc, n, g);
        Size checkSize = this.GetCheckSize(tc, n);
        if (checkSize != Size.Empty)
        {
          textSize.Width = textSize.Width + (tc.CheckGapLeft + checkSize.Width + tc.CheckGapRight);
          if (textSize.Height < checkSize.Height)
          {
            textSize.Height = checkSize.Height;
          }
        }
        Size imageSize = this.GetImageSize(tc, n);
        if (imageSize != Size.Empty)
        {
          textSize.Width = textSize.Width + (tc.ImageGapLeft + imageSize.Width + tc.ImageGapRight);
          if (textSize.Height < imageSize.Height)
          {
            textSize.Height = imageSize.Height;
          }
        }
        n.Cache.Size = textSize;
      }
      if (n.Cache.Size.Height > tc.MaximumNodeHeight)
      {
        n.Cache.Size = new Size(n.Cache.Size.Width, tc.MaximumNodeHeight);
      }
      if (n.Cache.Size.Height < tc.MinimumNodeHeight)
      {
        n.Cache.Size = new Size(n.Cache.Size.Width, tc.MinimumNodeHeight);
      }
      return n.Cache.Size;
    }
    public virtual Rectangle SetPosition(TreeControl tc, Node n, Point topLeft)
    {
      n.Cache.Bounds = new Rectangle(topLeft, n.Cache.Size);
      return n.Cache.Bounds;
    }
    public virtual void SetChildBounds(TreeControl tc, Node n, Rectangle bounds)
    {
      n.Cache.ChildBounds = bounds;
    }
    public virtual bool IntersectsWith(TreeControl tc, Node n, Rectangle rectangle, bool recurse)
    {
      if (recurse)
      {
        return n.Cache.ChildBounds.IntersectsWith(rectangle);
      }
      return n.Cache.Bounds.IntersectsWith(rectangle);
    }
    public virtual void Draw(TreeControl tc, Node n, Graphics g, Rectangle clipRectangle, int leftOffset, int rightOffset)
    {
      Rectangle bounds = n.Cache.Bounds;
      bounds.X = bounds.X + leftOffset;
      bounds.Width = bounds.Width - (leftOffset + rightOffset);
      if (tc.ExtendToRight)
      {
        Point point = tc.ClientToNodeSpace(new Point(tc.DrawRectangle.Right, 0));
        if (point.X > bounds.Right)
        {
          bounds.Width = bounds.Width + (point.X - bounds.Right);
        }
      }
      Size checkSize = this.GetCheckSize(tc, n);
      if (checkSize != Size.Empty)
      {
        Rectangle checkRectangle = this.GetCheckRectangle(tc, checkSize, bounds);
        this.DrawCheck(tc, n, g, checkRectangle);
        int num = tc.CheckGapLeft + checkSize.Width + tc.CheckGapRight;
        bounds.X = bounds.X + num;
        bounds.Width = bounds.Width - num;
      }
      int left = bounds.Left;
      Size imageSize = this.GetImageSize(tc, n);
      if (imageSize != Size.Empty)
      {
        int num2 = 0;
        int height = imageSize.Height;
        if (imageSize.Height > bounds.Height)
        {
          height = bounds.Height;
        }
        else
        {
          num2 = (bounds.Height - imageSize.Height) / 2;
        }
        this.DrawImage(tc, n, g, bounds.X + tc.ImageGapLeft, bounds.Y + num2, imageSize.Width, height);
        int num3 = tc.ImageGapLeft + imageSize.Width + tc.ImageGapRight;
        bounds.X = bounds.X + num3;
        bounds.Width = bounds.Width - num3;
      }
      this.DrawText(tc, n, g, bounds, left);
      if (tc.ContainsFocus && tc.FocusNode == n)
      {
        this.DrawFocusIndication(tc, n, g, bounds);
      }
    }
    public virtual void DrawCheck(TreeControl tc, Node n, Graphics g, Rectangle drawRect)
    {
      bool flag = drawRect.Contains(tc.HotPoint);
      CheckStates checkStates = this.GetCheckStates(tc, n);
      if (tc.IsControlThemed && tc.CheckDrawStyle == DrawStyle.Themed)
      {
        tc.DrawThemedCheckbox(g, drawRect, n.CheckState, checkStates, flag);
        return;
      }
      bool flag2 = checkStates == CheckStates.Radio;
      bool flag3 = tc.CheckDrawStyle == DrawStyle.Gradient;
      if (flag2)
      {
        g.FillEllipse(tc.GetCacheCheckBorderBrush(), drawRect);
      }
      else
      {
        g.FillRectangle(tc.GetCacheCheckBorderBrush(), drawRect);
      }
      drawRect.Inflate(-tc.CheckBorderWidth, -tc.CheckBorderWidth);
      if (flag3)
      {
        Rectangle rectangle = drawRect;
        rectangle.Inflate(1, 1);
        if (rectangle.Width <= 0 || rectangle.Height <= 0)
        {
          goto IL_12C;
        }
        using (Brush brush = new LinearGradientBrush(rectangle, flag ? tc.CheckInsideHotColor : tc.CheckInsideColor, tc.BackColor, 45f))
        {
          if (flag2)
          {
            g.FillEllipse(brush, drawRect);
          }
          else
          {
            g.FillRectangle(brush, drawRect);
          }
          goto IL_12C;
        }
      }
      if (flag2)
      {
        g.FillEllipse(flag ? tc.GetCacheCheckInsideHotBrush() : tc.GetCacheCheckInsideBrush(), drawRect);
      }
      else
      {
        g.FillRectangle(flag ? tc.GetCacheCheckInsideHotBrush() : tc.GetCacheCheckInsideBrush(), drawRect);
      }
    IL_12C:
      switch (n.CheckState)
      {
        case CheckState.Mixed:
          if (!flag2)
          {
            drawRect.Inflate(-2, -2);
            g.FillRectangle(flag ? tc.GetCacheCheckMixedHotBrush() : tc.GetCacheCheckMixedBrush(), drawRect);
            return;
          }
          break;
        case CheckState.Checked:
          {
            if (flag2)
            {
              Brush brush2 = flag ? tc.GetCacheCheckTickHotBrush() : tc.GetCacheCheckTickBrush();
              drawRect.Inflate(-2, -2);
              g.FillEllipse(brush2, drawRect);
              return;
            }
            int num = drawRect.Left + (int)((double)drawRect.Width / 11.0 * 2.0);
            int num2 = drawRect.Left + (int)((double)drawRect.Width / 11.0 * 4.0);
            int num3 = drawRect.Left + (int)((double)drawRect.Width / 11.0 * 8.0);
            int num4 = drawRect.Top + (int)((double)drawRect.Height / 11.0 * 6.0);
            int num5 = drawRect.Top + (int)((double)drawRect.Height / 11.0 * 8.0);
            int num6 = drawRect.Top + (int)((double)drawRect.Height / 11.0 * 4.0);
            Pen pen = flag ? tc.GetCacheCheckTickHotPen() : tc.GetCacheCheckTickPen();
            g.DrawLine(pen, num, num4, num2, num5);
            g.DrawLine(pen, num2, num5, num3, num6);
            g.DrawLine(pen, num, num4 - 1, num2, num5 - 1);
            g.DrawLine(pen, num2, num5 - 1, num3, num6 - 1);
            g.DrawLine(pen, num, num4 - 2, num2, num5 - 2);
            g.DrawLine(pen, num2, num5 - 2, num3, num6 - 2);
            break;
          }
        default:
          return;
      }
    }
    public virtual void DrawImage(TreeControl tc, Node n, Graphics g, int x, int y, int width, int height)
    {
      bool flag = false;
      if (n.IsSelected)
      {
        if (n.SelectedIcon != null)
        {
          g.DrawIcon(n.SelectedIcon, new Rectangle(x, y, width, height));
          flag = true;
        }
        else if (n.SelectedImage != null)
        {
          g.DrawImage(n.SelectedImage, new Rectangle(x, y, width, height));
          flag = true;
        }
        else
        {
          int selectedImageIndex;
          if (n.SelectedImageIndex >= 0)
          {
            selectedImageIndex = n.SelectedImageIndex;
          }
          else
          {
            selectedImageIndex = tc.SelectedImageIndex;
          }
          if (selectedImageIndex >= 0 && selectedImageIndex <= tc.ImageList.Images.Count - 1)
          {
            Image image = tc.ImageList.Images[selectedImageIndex];
            g.DrawImage(image, new Rectangle(x, y, width, height));
            image.Dispose();
            flag = true;
          }
        }
      }
      if (!flag)
      {
        if (n.Icon != null)
        {
          g.DrawIcon(n.Icon, new Rectangle(x, y, width, height));
          return;
        }
        if (n.Image != null)
        {
          g.DrawImage(n.Image, new Rectangle(x, y, width, height));
          return;
        }
        int imageIndex;
        if (n.ImageIndex >= 0)
        {
          imageIndex = n.ImageIndex;
        }
        else
        {
          imageIndex = tc.ImageIndex;
        }
        if (imageIndex >= 0 && imageIndex <= tc.ImageList.Images.Count - 1)
        {
          Image image2 = tc.ImageList.Images[imageIndex];
          g.DrawImage(image2, new Rectangle(x, y, width, height));
          image2.Dispose();
        }
      }
    }
    public virtual void DrawText(TreeControl tc, Node n, Graphics g, Rectangle rect, int hotLeft)
    {
      if (tc.LabelEditNode != n)
      {
        bool flag = tc.DragOverNode == n;
        bool isSelected = n.IsSelected;
        bool flag2 = tc.HotNode == n;
        bool containsFocus = tc.ContainsFocus;
        if (flag2)
        {
          Rectangle rectangle = new Rectangle(hotLeft, rect.Top, rect.Right - hotLeft, rect.Height);
          flag2 = rectangle.Contains(tc.HotPoint);
        }
        if (isSelected || flag)
        {
          if (containsFocus || flag)
          {
            g.FillRectangle(tc.GetCacheSelectedBackBrush(), rect);
          }
          else
          {
            g.FillRectangle(tc.GetCacheSelectedNoFocusBackBrush(), rect);
          }
        }
        else if (flag2 && tc.HotBackColor != Color.Empty)
        {
          g.FillRectangle(tc.GetCacheHotBackBrush(), rect);
        }
        else if (n.BackColor != tc.BackColor)
        {
          using (SolidBrush solidBrush = new SolidBrush(n.BackColor))
          {
            g.FillRectangle(solidBrush, rect);
          }
        }
        Color color;
        if (isSelected || flag)
        {
          if (containsFocus || flag)
          {
            color = tc.SelectedForeColor;
          }
          else
          {
            color = n.ForeColor;
          }
        }
        else if (flag2 && tc.HotForeColor != Color.Empty)
        {
          color = tc.HotForeColor;
        }
        else
        {
          color = n.ForeColor;
        }
        using (SolidBrush solidBrush2 = new SolidBrush(color))
        {
          bool flag3 = false;
          if (flag2 && tc.UseHotFontStyle)
          {
            using (Font font = new Font(n.NodeFont, tc.HotFontStyle))
            {
              g.DrawString(n.Text, font, solidBrush2, rect, DefaultNodeVC._format);
            }
            flag3 = true;
          }
          else if (isSelected && tc.UseSelectedFontStyle)
          {
            using (Font font2 = new Font(n.NodeFont, tc.SelectedFontStyle))
            {
              g.DrawString(n.Text, font2, solidBrush2, rect, DefaultNodeVC._format);
            }
            flag3 = true;
          }
          if (!flag3)
          {
            g.DrawString(n.Text, n.NodeFont, solidBrush2, rect, DefaultNodeVC._format);
          }
        }
      }
    }
    public virtual void DrawFocusIndication(TreeControl tc, Node n, Graphics g, Rectangle rect)
    {
      if (tc.LabelEditNode != n)
      {
        ControlPaint.DrawFocusRectangle(g, rect);
      }
    }
    public virtual CheckStates GetCheckStates(TreeControl tc, Node n)
    {
      CheckStates result = CheckStates.None;
      if (n.CheckStates == NodeCheckStates.Inherit)
      {
        result = tc.CheckStates;
      }
      else
      {
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
        }
      }
      return result;
    }
    public virtual Size GetTextSize(TreeControl tc, Node n, Graphics g)
    {
      SizeF sizeF;
      if (tc.UseHotFontStyle || tc.UseSelectedFontStyle)
      {
        Font nodeFont = n.GetNodeFont();
        Font font = (nodeFont == null) ? tc.GetFontBoldItalic() : new Font(nodeFont, FontStyle.Bold);
        sizeF = g.MeasureString(n.Text, font);
        if (nodeFont != null)
        {
          font.Dispose();
        }
      }
      else
      {
        sizeF = g.MeasureString(n.Text, n.NodeFont);
      }
      return new Size((int)(sizeF.Width + 1f), (int)((double)n.GetNodeFontHeight() * 1.25));
    }
    public virtual Size GetCheckSize(TreeControl tc, Node n)
    {
      if (this.GetCheckStates(tc, n) == CheckStates.None)
      {
        return Size.Empty;
      }
      return new Size(tc.CheckLength, tc.CheckLength);
    }
    public virtual Size GetImageSize(TreeControl tc, Node n)
    {
      if (n.IsSelected)
      {
        if (n.SelectedIcon != null)
        {
          return n.SelectedIcon.Size;
        }
        if (n.SelectedImage != null)
        {
          return n.SelectedImage.Size;
        }
      }
      if (n.Icon != null)
      {
        return n.Icon.Size;
      }
      if (n.Image != null)
      {
        return n.Image.Size;
      }
      if (tc.ImageList != null)
      {
        return tc.ImageList.ImageSize;
      }
      return Size.Empty;
    }
    public virtual Rectangle GetCheckRectangle(TreeControl tc, Size checkSize, Rectangle client)
    {
      int num = 0;
      int height = checkSize.Height;
      if (checkSize.Height > client.Height)
      {
        height = client.Height;
      }
      else
      {
        num = (client.Height - checkSize.Height) / 2;
      }
      int num2 = (height < checkSize.Width) ? height : checkSize.Width;
      return new Rectangle(client.X + tc.CheckGapLeft, client.Y + num, num2, num2);
    }
    public virtual ClickExpandAction CanExpandOnClick(TreeControl tc, Node n)
    {
      return tc.ClickExpand;
    }
    public virtual bool CanAutoEdit(TreeControl tc, Node n)
    {
      return tc.AutoEdit;
    }
    public virtual bool CanToolTip(TreeControl tc, Node n)
    {
      return true;
    }
    public virtual ClickExpandAction CanExpandOnDoubleClick(TreeControl tc, Node n)
    {
      return tc.DoubleClickExpand;
    }
    public virtual bool MouseDown(TreeControl tc, Node n, MouseButtons button, Point pt)
    {
      Rectangle bounds = n.Cache.Bounds;
      Size checkSize = this.GetCheckSize(tc, n);
      if (checkSize != Size.Empty)
      {
        Rectangle checkRectangle = this.GetCheckRectangle(tc, checkSize, bounds);
        if (button == MouseButtons.Left && checkRectangle.Contains(pt))
        {
          CheckStates checkStates = this.GetCheckStates(tc, n);
          if (checkStates == CheckStates.Radio)
          {
            if (n.CheckState != CheckState.Checked)
            {
              n.CheckState = CheckState.Checked;
            }
          }
          else
          {
            switch (n.CheckState)
            {
              case CheckState.Unchecked:
                n.CheckState = CheckState.Checked;
                break;
              case CheckState.Mixed:
                n.CheckState = CheckState.Unchecked;
                break;
              case CheckState.Checked:
                if (checkStates != CheckStates.ThreeStateCheck)
                {
                  n.CheckState = CheckState.Unchecked;
                }
                else
                {
                  n.CheckState = CheckState.Mixed;
                }
                break;
            }
          }
          return true;
        }
        int num = tc.CheckGapLeft + checkSize.Width + tc.CheckGapRight;
        bounds.X = bounds.X + num;
        bounds.Width = bounds.Width - num;
      }
      if (this.ClickPointInNode(tc, n, bounds, pt))
      {
        if (button == MouseButtons.Left)
        {
          switch (this.CanExpandOnClick(tc, n))
          {
            case ClickExpandAction.Expand:
              if (this.CanExpandNode(tc, n, false, true))
              {
                n.Expand();
              }
              break;
            case ClickExpandAction.Toggle:
              if (n.Expanded)
              {
                if (this.CanCollapseNode(tc, n, false, true))
                {
                  n.Collapse();
                }
              }
              else if (this.CanExpandNode(tc, n, false, true))
              {
                n.Expand();
              }
              break;
          }
        }
        this._mouseDownSelected = n.IsSelected;
        if (tc.SelectMode != SelectMode.None)
        {
          if (!n.IsSelected)
          {
            tc.SelectNode(n);
          }
          else
          {
            tc.SetFocusNode(n);
          }
        }
        else
        {
          tc.SetFocusNode(n);
        }
        return true;
      }
      return false;
    }
    public virtual bool MouseUp(TreeControl tc, Node n, MouseButtons button, Point pt)
    {
      Rectangle bounds = n.Cache.Bounds;
      if (this.ClickPointInNode(tc, n, bounds, pt))
      {
        if (button == MouseButtons.Left && tc.SelectMode != SelectMode.None)
        {
          bool flag = false;
          if (n.IsSelected != this._mouseDownSelected && KeyHelper.CTRLPressed)
          {
            flag = true;
          }
          if (n.IsSelected && !flag)
          {
            tc.SelectNode(n);
          }
          if (n.IsSelected && this._mouseDownSelected && this.CanAutoEdit(tc, n) && tc.SelectedCount == 1 && this.GetTextRectangle(tc, n).Contains(pt) && !this._mouseDoubleClick)
          {
            tc.BeginAutoEdit(n);
          }
        }
        if (button == MouseButtons.Right && tc.OnShowContextMenuNode(n))
        {
          ContextMenu contextMenuNode = tc.ContextMenuNode;
          if (contextMenuNode != null)
          {
            contextMenuNode.Show(tc, tc.NodeSpaceToClient(pt));
          }
        }
        this._mouseDoubleClick = false;
        return true;
      }
      this._mouseDoubleClick = false;
      return false;
    }
    public virtual bool DoubleClick(TreeControl tc, Node n, Point pt)
    {
      this._mouseDoubleClick = true;
      Rectangle bounds = n.Cache.Bounds;
      Size checkSize = this.GetCheckSize(tc, n);
      if (checkSize != Size.Empty)
      {
        int num = tc.CheckGapLeft + checkSize.Width + tc.CheckGapRight;
        bounds.X = bounds.X + num;
        bounds.Width = bounds.Width - num;
      }
      if (this.ClickPointInNode(tc, n, bounds, pt))
      {
        switch (this.CanExpandOnDoubleClick(tc, n))
        {
          case ClickExpandAction.Expand:
            if (this.CanExpandNode(tc, n, false, true))
            {
              n.Expand();
            }
            break;
          case ClickExpandAction.Toggle:
            if (n.Expanded)
            {
              if (this.CanCollapseNode(tc, n, false, true))
              {
                n.Collapse();
              }
            }
            else if (this.CanExpandNode(tc, n, false, true))
            {
              n.Expand();
            }
            break;
        }
        tc.CancelAutoEdit();
        return true;
      }
      return false;
    }
    public virtual bool ClickPointInNode(TreeControl tc, Node n, Rectangle rect, Point pt)
    {
      if (tc.ExtendToRight)
      {
        return pt.X >= rect.Left && pt.Y >= rect.Top && pt.Y <= rect.Bottom;
      }
      return rect.Contains(pt);
    }
    public virtual void NodeExpandedChanged(TreeControl tc, Node n)
    {
      if (n.Expanded && tc.AutoCollapse)
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
      tc.NodeExpandedChanged(n);
    }
    public virtual void NodeVisibleChanged(TreeControl tc, Node n)
    {
      if (!n.Visible)
      {
        bool makeSelected = tc.SelectedCount == 1 && n.IsSelected;
        if (tc.SelectedCount > 0)
        {
          tc.DeselectNode(n, true);
        }
        tc.NodeVisibleChanged(n, makeSelected);
      }
    }
    public virtual void NodeSelectableChanged(TreeControl tc, Node n)
    {
      if (!n.Selectable)
      {
        tc.DeselectNode(n, false);
      }
    }
    public virtual void NodeCheckStateChanged(TreeControl tc, Node n)
    {
      if (n.CheckState == CheckState.Checked && this.GetCheckStates(tc, n) == CheckStates.Radio)
      {
        IEnumerator enumerator = n.ParentNodes.GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
          {
            Node node = (Node)enumerator.Current;
            if (node != n && (node.CheckStates == NodeCheckStates.Radio || (node.CheckStates == NodeCheckStates.Inherit && tc.CheckStates == CheckStates.Radio)))
            {
              node.CheckState = CheckState.Unchecked;
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
    public virtual void NodeRemoving(TreeControl tc, Node n)
    {
      bool makeSelected = tc.SelectedCount == 1 && n.IsSelected;
      tc.DeselectNode(n, true);
      if (tc.FocusNode == n)
      {
        tc.NodeContentRemoved(makeSelected);
      }
    }
    public virtual bool CanSelectNode(TreeControl tc, Node n)
    {
      return tc.NodesSelectable && n.Selectable;
    }
    public virtual bool CanCollapseNode(TreeControl tc, Node n, bool key, bool mouse)
    {
      return tc.CanUserExpandCollapse;
    }
    public virtual bool CanExpandNode(TreeControl tc, Node n, bool key, bool mouse)
    {
      return tc.CanUserExpandCollapse;
    }
    public virtual void BeginEditNode(TreeControl tc, Node n)
    {
      if (tc.LabelEdit && tc.IsNodeDisplayed(n) && tc.EnsureDisplayed(n))
      {
        Rectangle textRectangle = this.GetTextRectangle(tc, n);
        Font nodeFont = this.GetNodeFont(tc, n);
        using (Graphics graphics = tc.CreateGraphics())
        {
          SizeF sizeF = graphics.MeasureString(n.Text + "W", nodeFont);
          SizeF sizeF2 = graphics.MeasureString("01234", nodeFont);
          if (sizeF2.Width > sizeF.Width)
          {
            sizeF.Width = sizeF2.Width;
          }
          if (sizeF2.Height < (float)textRectangle.Height)
          {
            int num = textRectangle.Height - (int)sizeF2.Height;
            textRectangle.Height = textRectangle.Height - num;
            textRectangle.Y = textRectangle.Y + (1 + num / 2);
          }
          textRectangle.Width = (int)sizeF.Width + SystemInformation.BorderSize.Width * 2;
          tc.BeginEditLabel(n, textRectangle);
        }
      }
    }
    public virtual Rectangle GetTextRectangle(TreeControl tc, Node n)
    {
      Rectangle bounds = n.Cache.Bounds;
      Size checkSize = this.GetCheckSize(tc, n);
      if (checkSize != Size.Empty)
      {
        int num = tc.CheckGapLeft + checkSize.Width + tc.CheckGapRight;
        bounds.X = bounds.X + num;
        bounds.Width = bounds.Width - num;
      }
      Size imageSize = this.GetImageSize(tc, n);
      if (imageSize != Size.Empty)
      {
        int num2 = tc.ImageGapLeft + imageSize.Width + tc.ImageGapRight;
        bounds.X = bounds.X + num2;
        bounds.Width = bounds.Width - num2;
      }
      return bounds;
    }
    public virtual void SizeChanged(TreeControl tc)
    {
    }
    public virtual Node DragOverNodeFromPoint(TreeControl tc, Point pt)
    {
      if (tc.ExtendToRight)
      {
        return tc.FindDisplayNodeFromY(pt.Y);
      }
      return tc.FindDisplayNodeFromPoint(pt);
    }
    public virtual void DragEnter(TreeControl tc, Node n, DragEventArgs drgevent)
    {
      tc.OnNodeDragEnter(n, drgevent);
    }
    public virtual void DragOver(TreeControl tc, Node n, DragEventArgs drgevent)
    {
      tc.OnNodeDragOver(n, drgevent);
    }
    public virtual void DragLeave(TreeControl tc, Node n)
    {
      tc.OnNodeDragLeave(n);
    }
    public virtual void DragDrop(TreeControl tc, Node n, DragEventArgs drgevent)
    {
      tc.OnNodeDragDrop(n, drgevent);
    }
    public virtual void DragHover(TreeControl tc, Node n)
    {
      if (tc.ExpandOnDragHover && !n.Expanded && n.Nodes.VisibleCount > 0)
      {
        n.Expand();
      }
    }
    public virtual void PostDrawNodes(TreeControl tc, Graphics g, ArrayList displayNodes)
    {
    }
    public virtual void PostCalculateNodes(TreeControl tc, ArrayList displayNodes)
    {
    }
    public virtual Font GetNodeFont(TreeControl tc, Node n)
    {
      return n.NodeFont;
    }
  }
}
