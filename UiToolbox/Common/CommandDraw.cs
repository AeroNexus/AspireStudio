using System;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Common
{
  public class CommandDraw
  {
    private static CommandImage _checkMark;
    private static CommandImage _radioCheckMark;
    private static CommandImage _subMenuMark;
    public static Size StandardMenuCheckSize
    {
      get
      {
        return CommandDraw._checkMark.ImageSize;
      }
    }
    public static int StandardMenuColumnGap
    {
      get
      {
        return 2;
      }
    }
    static CommandDraw()
    {
      Bitmap image = ResourceHelper.LoadBitmap(typeof(CommandDraw), "Crownwood.DotNetMagic.Resources.MenuCheckMark.bmp", new Point(0, 0));
      Bitmap image2 = ResourceHelper.LoadBitmap(typeof(CommandDraw), "Crownwood.DotNetMagic.Resources.MenuRadioCheckMark.bmp", new Point(0, 0));
      Bitmap image3 = ResourceHelper.LoadBitmap(typeof(CommandDraw), "Crownwood.DotNetMagic.Resources.MenuSubMenuMark.bmp", new Point(0, 0));
      CommandDraw._checkMark = new CommandImage(image);
      CommandDraw._radioCheckMark = new CommandImage(image2);
      CommandDraw._subMenuMark = new CommandImage(image3);
    }
    public static void DrawButtonCommand(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, bool enabled, TextEdge edge, Font font, Color textColor, Color baseColor, string text, CommandImage image, ImageAttributes imageAttr, Color trackBase1, Color trackBase2, Color trackLight1, Color trackLight2, Color trackLightLight1, Color trackLightLight2, Color trackBorder, ButtonStyle buttonStyle, bool pushed, bool staticButton, bool drawBorder)
    {
      CommandDraw.DrawButtonCommandBack(g, style, direction, drawRect, state, baseColor, trackBase1, trackBase2, trackLight1, trackLight2, trackLightLight1, trackLightLight2, trackBorder, buttonStyle, pushed);
      CommandDraw.DrawButtonCommandInside(g, style, direction, drawRect, state, enabled, edge, font, textColor, baseColor, text, image, imageAttr, staticButton);
      CommandDraw.DrawButtonCommandOutline(g, style, direction, drawRect, state, baseColor, trackBase1, trackBase2, trackLight1, trackLight2, trackLightLight1, trackLightLight2, trackBorder, buttonStyle, pushed, drawBorder);
    }
    public static void DrawButtonCommandBack(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, Color baseColor, Color trackBase1, Color trackBase2, Color trackLight1, Color trackLight2, Color trackLightLight1, Color trackLightLight2, Color trackBorder, ButtonStyle buttonStyle, bool pushed)
    {
      new Rectangle(drawRect.Left, drawRect.Top, drawRect.Width - 1, drawRect.Height - 1);
      switch (style)
      {
        case VisualStyle.IDE:
          break;
        case VisualStyle.Plain:
          if (buttonStyle == ButtonStyle.ToggleButton && pushed && state == ItemState.Normal)
          {
            using (HatchBrush hatchBrush = new HatchBrush(HatchStyle.Percent50, baseColor, ControlPaint.Light(baseColor))) 
            {
              g.FillRectangle(hatchBrush, drawRect);
              return;
            }
          }
          using (SolidBrush solidBrush = new SolidBrush(baseColor))
          {
            g.FillRectangle(solidBrush, drawRect);
            return;
          }
        case VisualStyle.Office2003:
        case VisualStyle.IDE2005:
          goto IL_11F;
        default:
          return;
      }
      if (buttonStyle == ButtonStyle.ToggleButton && pushed && state == ItemState.Normal)
      {
        using (SolidBrush solidBrush2 = new SolidBrush(trackLightLight1))
        {
          g.FillRectangle(solidBrush2, drawRect);
          return;
        }
      }
      switch (state)
      {
        case ItemState.Normal:
          return;
        case ItemState.HotTrack:
        case ItemState.Open:
          using (SolidBrush solidBrush3 = new SolidBrush(trackLight1))
          {
            g.FillRectangle(solidBrush3, drawRect);
            return;
          }
        case ItemState.Pressed:
          break;
        default:
          return;
      }
      using (SolidBrush solidBrush4 = new SolidBrush(trackBase1))
      {
        g.FillRectangle(solidBrush4, drawRect);
        return;
      }
    IL_11F:
      float num = (float)((direction == LayoutDirection.Horizontal) ? 90 : 0);
      if (buttonStyle == ButtonStyle.ToggleButton && pushed)
      {
        if (drawRect.Width <= 0 || drawRect.Height <= 0)
        {
          return;
        }
        switch (state)
        {
          case ItemState.Normal:
            using (Brush brush = new LinearGradientBrush(drawRect, trackLightLight1, trackLightLight2, num))
            {
              g.FillRectangle(brush, drawRect);
              return;
            }
          case ItemState.HotTrack:
          case ItemState.Pressed:
          case ItemState.Open:
            break;
          default:
            return;
        }
        using (Brush brush2 = new LinearGradientBrush(drawRect, trackBase1, trackBase2, num))
        {
          g.FillRectangle(brush2, drawRect);
          return;
        }
      }
      if (drawRect.Width > 0 && drawRect.Height > 0)
      {
        switch (state)
        {
          case ItemState.Normal:
            return;
          case ItemState.HotTrack:
          case ItemState.Open:
            using (Brush brush3 = new LinearGradientBrush(drawRect, trackLight1, trackLight2, num))
            {
              g.FillRectangle(brush3, drawRect);
              return;
            }
          case ItemState.Pressed:
            break;
          default:
            return;
        }
        using (Brush brush4 = new LinearGradientBrush(drawRect, trackBase1, trackBase2, num))
        {
          g.FillRectangle(brush4, drawRect);
        }
      }
    }
    public static void DrawButtonCommandInside(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, bool enabled, TextEdge edge, Font font, Color textColor, Color baseColor, string text, CommandImage image, ImageAttributes imageAttr, bool staticButton)
    {
      new Rectangle(drawRect.Left, drawRect.Top, drawRect.Width - 1, drawRect.Height - 1);
      if (image == null || image.Image == null)
      {
        CommandDraw.DrawCommandText(g, style, direction, state, enabled, drawRect, font, textColor, baseColor, text);
        return;
      }
      if (text == null || text.Length == 0)
      {
        CommandDraw.DrawCommandImage(g, style, state, enabled, drawRect, baseColor, image, imageAttr, staticButton);
        return;
      }
      Size size = CommandDraw.TextSize(g, font, text);
      int num = 0;
      int num2 = 0;
      int num3 = 0;
      if (direction == LayoutDirection.Horizontal)
      {
        switch (edge)
        {
          case TextEdge.Top:
          case TextEdge.Bottom:
            num = drawRect.Height;
            num2 = image.ImageSpace(style).Height + size.Height;
            num3 = image.ImageSpace(style).Height;
            break;
          case TextEdge.Left:
          case TextEdge.Right:
            num = drawRect.Width;
            num2 = image.ImageSpace(style).Width + size.Width;
            num3 = image.ImageSpace(style).Width;
            break;
        }
      }
      else
      {
        switch (edge)
        {
          case TextEdge.Top:
          case TextEdge.Bottom:
            num = drawRect.Width;
            num2 = image.ImageSpace(style).Width + size.Height;
            num3 = image.ImageSpace(style).Height;
            break;
          case TextEdge.Left:
          case TextEdge.Right:
            num = drawRect.Height;
            num2 = image.ImageSpace(style).Height + size.Width;
            num3 = image.ImageSpace(style).Width;
            break;
        }
      }
      if (num <= num3)
      {
        CommandDraw.DrawCommandImage(g, style, state, enabled, drawRect, baseColor, image, imageAttr, staticButton);
        return;
      }
      if (num <= num2)
      {
        Rectangle rectangle = CommandDraw.ImageFromEdge(drawRect, image.ImageSpace(style), edge, direction, 0);
        CommandDraw.DrawCommandImage(g, style, state, enabled, rectangle, baseColor, image, imageAttr, staticButton);
        Rectangle drawRect2 = CommandDraw.TextFromImageEdge(drawRect, rectangle, edge, direction, 0);
        CommandDraw.DrawCommandText(g, style, direction, state, enabled, drawRect2, font, textColor, baseColor, text);
        return;
      }
      int offset = (num - num2) / 3;
      Rectangle rectangle2 = CommandDraw.ImageFromEdge(drawRect, image.ImageSpace(style), edge, direction, offset);
      CommandDraw.DrawCommandImage(g, style, state, enabled, rectangle2, baseColor, image, imageAttr, staticButton);
      Rectangle drawRect3 = CommandDraw.TextFromImageEdge(drawRect, rectangle2, edge, direction, offset);
      CommandDraw.DrawCommandText(g, style, direction, state, enabled, drawRect3, font, textColor, baseColor, text);
    }
    public static void DrawButtonCommandOutline(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, Color baseColor, Color trackBase1, Color trackBase2, Color trackLight1, Color trackLight2, Color trackLightLight1, Color trackLightLight2, Color trackBorder, ButtonStyle buttonStyle, bool pushed, bool drawBorder)
    {
      Rectangle rectangle = new Rectangle(drawRect.Left, drawRect.Top, drawRect.Width - 1, drawRect.Height - 1);
      switch (style)
      {
        case VisualStyle.IDE:
        case VisualStyle.Office2003:
        case VisualStyle.IDE2005:
          if (buttonStyle == ButtonStyle.ToggleButton && pushed)
          {
            using (Pen pen = new Pen(trackBorder))
            {
              g.DrawRectangle(pen, rectangle);
              break;
            }
          }
          switch (state)
          {
            case ItemState.Normal:
              if (!drawBorder)
              {
                return;
              }
              using (Pen pen2 = new Pen(ControlPaint.Light(ControlPaint.Dark(baseColor))))
              {
                g.DrawRectangle(pen2, rectangle);
                return;
              }
            case ItemState.HotTrack:
            case ItemState.Pressed:
            case ItemState.Open:
              break;
            default:
              return;
          }
          using (Pen pen3 = new Pen(trackBorder))
          {
            g.DrawRectangle(pen3, rectangle);
          }
          break;
        case VisualStyle.Plain:
          if (buttonStyle == ButtonStyle.ToggleButton && pushed)
          {
            CommandDraw.DrawPlainSunken(g, rectangle, baseColor);
            return;
          }
          switch (state)
          {
            case ItemState.Normal:
              if (drawBorder)
              {
                ControlPaint.DrawBorder(g, rectangle, ControlPaint.Light(ControlPaint.Dark(baseColor)), ButtonBorderStyle.Solid);
                return;
              }
              break;
            case ItemState.HotTrack:
            case ItemState.Open:
              CommandDraw.DrawPlainRaised(g, rectangle, baseColor);
              return;
            case ItemState.Pressed:
              CommandDraw.DrawPlainSunken(g, rectangle, baseColor);
              return;
            default:
              return;
          }
          break;
        default:
          return;
      }
    }
    public static void DrawCommandText(Graphics g, VisualStyle style, LayoutDirection direction, ItemState state, bool enabled, Rectangle drawRect, Font font, Color textColor, Color baseColor, string text)
    {
      using (StringFormat stringFormat = new StringFormat())
      {
        stringFormat.FormatFlags = StringFormatFlags.NoWrap;
        stringFormat.Alignment = StringAlignment.Center;
        stringFormat.LineAlignment = StringAlignment.Center;
        stringFormat.Trimming = StringTrimming.EllipsisCharacter;
        stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
        if (direction == LayoutDirection.Vertical)
        {
          StringFormat expr_31 = stringFormat;
          expr_31.FormatFlags = expr_31.FormatFlags | StringFormatFlags.DirectionVertical;
        }
        if (enabled)
        {
          Color color = textColor;
          if (style == VisualStyle.IDE && state == ItemState.Pressed)
          {
            color = ControlPaint.Light(color);
          }
          if (style == VisualStyle.Plain && state == ItemState.Pressed)
          {
            drawRect.Offset(1, 1);
          }
          new RectangleF((float)drawRect.Left, (float)drawRect.Top, (float)drawRect.Width, (float)drawRect.Height);
          using (SolidBrush solidBrush = new SolidBrush(color))
          {
            g.DrawString(text, font, solidBrush, drawRect, stringFormat);
            goto IL_176;
          }
        }
        switch (style)
        {
          case VisualStyle.IDE:
          case VisualStyle.Office2003:
          case VisualStyle.IDE2005:
            break;
          case VisualStyle.Plain:
            {
              Rectangle rectangle = drawRect;
              rectangle.Offset(1, 1);
              g.DrawString(text, font, Brushes.White, rectangle, stringFormat);
              using (SolidBrush solidBrush2 = new SolidBrush(SystemColors.GrayText))
              {
                g.DrawString(text, font, solidBrush2, drawRect, stringFormat);
                goto IL_176;
              }
            }
          default:
            goto IL_176;
        }
        new RectangleF((float)drawRect.Left, (float)drawRect.Top, (float)drawRect.Width, (float)drawRect.Height);
        using (SolidBrush solidBrush3 = new SolidBrush(SystemColors.GrayText))
        {
          g.DrawString(text, font, solidBrush3, drawRect, stringFormat);
        }
      IL_176: ;
      }
    }
    public static void DrawCommandImage(Graphics g, VisualStyle style, ItemState state, bool enabled, Rectangle drawRect, Color baseColor, CommandImage image, ImageAttributes imageAttr, bool staticButton)
    {
      switch (style)
      {
        case VisualStyle.IDE:
          if (!enabled)
          {
            CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.GetDisabledImage(baseColor), imageAttr);
            return;
          }
          switch (state)
          {
            case ItemState.Normal:
              CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.FadedImage, imageAttr);
              return;
            case ItemState.HotTrack:
            case ItemState.Open:
              {
                if (staticButton)
                {
                  CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.Image, imageAttr);
                  return;
                }
                Rectangle drawRect2 = new Rectangle(drawRect.Left + 2, drawRect.Top + 2, drawRect.Width - 2, drawRect.Height - 2);
                Rectangle drawRect3 = new Rectangle(drawRect.Left, drawRect.Top, drawRect.Width - 2, drawRect.Height - 2);
                CommandDraw.DrawImageInCentre(g, drawRect2, image.ImageSize, image.GetDisabledImage(baseColor), imageAttr);
                CommandDraw.DrawImageInCentre(g, drawRect3, image.ImageSize, image.Image, imageAttr);
                return;
              }
            case ItemState.Pressed:
              CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.Image, imageAttr);
              return;
            default:
              return;
          }
        case VisualStyle.Plain:
          if (!enabled)
          {
            Rectangle drawRect4 = new Rectangle(drawRect.Left + 1, drawRect.Top + 1, drawRect.Width - 1, drawRect.Height - 1);
            CommandDraw.DrawImageInCentre(g, drawRect4, image.ImageSize, image.WhiteImage, imageAttr);
            CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.GetDisabledImage(baseColor), imageAttr);
            return;
          }
          switch (state)
          {
            case ItemState.Normal:
            case ItemState.HotTrack:
            case ItemState.Open:
              CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.Image, imageAttr);
              return;
            case ItemState.Pressed:
              {
                Rectangle drawRect5 = new Rectangle(drawRect.Left + 1, drawRect.Top + 1, drawRect.Width - 1, drawRect.Height - 1);
                CommandDraw.DrawImageInCentre(g, drawRect5, image.ImageSize, image.Image, imageAttr);
                return;
              }
            default:
              return;
          }
        case VisualStyle.Office2003:
        case VisualStyle.IDE2005:
          if (!enabled)
          {
            CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.GetDisabledImage(baseColor), imageAttr);
            return;
          }
          switch (state)
          {
            case ItemState.Normal:
            case ItemState.HotTrack:
            case ItemState.Pressed:
            case ItemState.Open:
              CommandDraw.DrawImageInCentre(g, drawRect, image.ImageSize, image.Image, imageAttr);
              return;
            default:
              return;
          }
        default:
          return;
      }
    }
    public static void DrawImageInCentre(Graphics g, Rectangle drawRect, Size imageSize, Image image, ImageAttributes imageAttr)
    {
      if (image != null)
      {
        int num = (drawRect.Width - imageSize.Width) / 2;
        int num2 = (drawRect.Height - imageSize.Height) / 2;
        if (drawRect.Width >= imageSize.Width && drawRect.Height >= imageSize.Height)
        {
          Rectangle rectangle = new Rectangle(drawRect.Left + num, drawRect.Top + num2, imageSize.Width, imageSize.Height);
          if (imageAttr != null)
          {
            g.DrawImage(image, rectangle, 0, 0, imageSize.Width, imageSize.Height, GraphicsUnit.Pixel, imageAttr);
            return;
          }
          g.DrawImage(image, rectangle, 0, 0, imageSize.Width, imageSize.Height, GraphicsUnit.Pixel);
          return;
        }
        else
        {
          int num3 = imageSize.Width - drawRect.Width;
          int num4 = imageSize.Height - drawRect.Height;
          if (num3 > 0)
          {
            num = 0;
            imageSize.Width = drawRect.Width;
          }
          if (num4 > 0)
          {
            num2 = 0;
            imageSize.Height = drawRect.Height;
          }
          Rectangle rectangle2 = new Rectangle(drawRect.Left + num, drawRect.Top + num2, imageSize.Width, imageSize.Height);
          if (imageAttr != null)
          {
            g.DrawImage(image, rectangle2, 0, 0, image.Size.Width, image.Size.Height, GraphicsUnit.Pixel, imageAttr);
            return;
          }
          g.DrawImage(image, rectangle2, 0, 0, image.Size.Width, image.Size.Height, GraphicsUnit.Pixel);
        }
      }
    }
    public static void DrawMenuTopCommand(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, bool enabled, TextEdge edge, Font font, Color textColor, Color baseColor, string text, CommandImage image, Color trackBase, Color trackLight, Color trackLightLight, Color trackBorder, Color openBase, Color openBorder, OmitEdge omitEdge)
    {
      CommandDraw.DrawMenuTopCommandBack(g, style, direction, drawRect, state, baseColor, trackBase, trackLight, trackLightLight, trackBorder, openBase);
      CommandDraw.DrawButtonCommandInside(g, style, direction, drawRect, state, enabled, edge, font, textColor, baseColor, text, image, null, false);
      CommandDraw.DrawMenuTopCommandOutline(g, style, direction, drawRect, state, baseColor, trackBase, trackLight, trackLightLight, trackBorder, openBorder, omitEdge);
    }
    public static void DrawMenuTopCommandBack(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, Color baseColor, Color trackBase, Color trackLight, Color trackLightLight, Color trackBorder, Color openBase)
    {
      new Rectangle(drawRect.Left, drawRect.Top, drawRect.Width - 1, drawRect.Height - 1);
      switch (style)
      {
        case VisualStyle.IDE:
        case VisualStyle.Office2003:
        case VisualStyle.IDE2005:
          break;
        case VisualStyle.Plain:
          using (SolidBrush solidBrush = new SolidBrush(baseColor))
          {
            g.FillRectangle(solidBrush, drawRect);
            return;
          }
        default:
          return;
      }
      switch (state)
      {
        case ItemState.Normal:
          using (SolidBrush solidBrush2 = new SolidBrush(baseColor))
          {
            g.FillRectangle(solidBrush2, drawRect);
            return;
          }
        case ItemState.HotTrack:
          break;
        case ItemState.Pressed:
          goto IL_B4;
        case ItemState.Open:
          goto IL_D0;
        default:
          return;
      }
      using (SolidBrush solidBrush3 = new SolidBrush(trackLight))
      {
        g.FillRectangle(solidBrush3, drawRect);
        return;
      }
    IL_B4:
      using (SolidBrush solidBrush4 = new SolidBrush(trackBase))
      {
        g.FillRectangle(solidBrush4, drawRect);
        return;
      }
    IL_D0:
      using (SolidBrush solidBrush5 = new SolidBrush(openBase))
      {
        g.FillRectangle(solidBrush5, drawRect);
      }
    }
    public static void DrawMenuTopCommandOutline(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, ItemState state, Color baseColor, Color trackBase, Color trackLight, Color trackLightLight, Color trackBorder, Color openBorder, OmitEdge omitEdge)
    {
      Rectangle rectangle = new Rectangle(drawRect.Left, drawRect.Top, drawRect.Width - 1, drawRect.Height - 1);
      switch (style)
      {
        case VisualStyle.IDE:
        case VisualStyle.Office2003:
        case VisualStyle.IDE2005:
          switch (state)
          {
            case ItemState.Normal:
              return;
            case ItemState.HotTrack:
            case ItemState.Pressed:
              using (Pen pen = new Pen(trackBorder))
              {
                g.DrawRectangle(pen, rectangle);
                return;
              }
            case ItemState.Open:
              break;
            default:
              return;
          }
          if (omitEdge == OmitEdge.None)
          {
            using (Pen pen2 = new Pen(openBorder))
            {
              g.DrawRectangle(pen2, rectangle);
              break;
            }
          }
          using (Pen pen3 = new Pen(openBorder))
          {
            if (omitEdge != OmitEdge.Top)
            {
              g.DrawLine(pen3, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
            }
            if (omitEdge != OmitEdge.Bottom)
            {
              g.DrawLine(pen3, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
            }
            if (omitEdge != OmitEdge.Left)
            {
              g.DrawLine(pen3, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom);
            }
            if (omitEdge != OmitEdge.Right)
            {
              g.DrawLine(pen3, rectangle.Right, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }
          }
          break;
        case VisualStyle.Plain:
          switch (state)
          {
            case ItemState.Normal:
              break;
            case ItemState.HotTrack:
              CommandDraw.DrawPlainRaised(g, rectangle, baseColor);
              return;
            case ItemState.Pressed:
            case ItemState.Open:
              CommandDraw.DrawPlainSunken(g, rectangle, baseColor);
              return;
            default:
              return;
          }
          break;
        default:
          return;
      }
    }
    public static void DrawMenuFullCommand(Graphics g, VisualStyle style, Rectangle drawRect, ItemState state, bool enabled, Font font, string text, Shortcut shortCut, bool hasChildren, bool needsCheckSpace, bool isChecked, bool isRadioChecked, Size maxImageSize, CommandImage image, Color textColor, Color baseColor, Color trackBase, Color trackLight, Color trackLightLight, Color trackBorder, Color openBase, Color openBorder)
    {
      int num = 0;
      if (needsCheckSpace)
      {
        num = CommandDraw.StandardMenuCheckSize.Width;
        num += CommandDraw.StandardMenuColumnGap * 2;
      }
      if (maxImageSize.Width <= 0)
      {
        num = CommandDraw.StandardMenuCheckSize.Width;
      }
      else
      {
        num += maxImageSize.Width;
      }
      num += CommandDraw.StandardMenuColumnGap * 2;
      int num2 = CommandDraw.StandardMenuCheckSize.Width;
      num2 += CommandDraw.StandardMenuColumnGap * 2;
    }
    public static void DrawSeparatorCommand(Graphics g, VisualStyle style, LayoutDirection direction, Rectangle drawRect, Color sepDarkColor, Color sepLightColor)
    {
      switch (style)
      {
        case VisualStyle.IDE:
          using (Pen pen = new Pen(sepDarkColor))
          {
            if (direction == LayoutDirection.Horizontal)
            {
              int num = drawRect.Left + (drawRect.Width - 1) / 2;
              g.DrawLine(pen, num, drawRect.Top, num, drawRect.Bottom - 1);
            }
            else
            {
              int num2 = drawRect.Top + (drawRect.Height - 1) / 2;
              g.DrawLine(pen, drawRect.Left, num2, drawRect.Right - 1, num2);
            }
            return;
          }
        case VisualStyle.Plain:
          break;
        case VisualStyle.Office2003:
          goto IL_214;
        case VisualStyle.IDE2005:
          goto IL_16E;
        default:
          return;
      }
      using (Pen pen2 = new Pen(sepDarkColor))
      {
        using (Pen pen3 = new Pen(sepLightColor))
        {
          if (direction == LayoutDirection.Horizontal)
          {
            int num3 = drawRect.Left + (drawRect.Width - 2) / 2;
            g.DrawLine(pen2, num3, drawRect.Top + 2, num3, drawRect.Bottom - 3);
            g.DrawLine(pen3, num3 + 1, drawRect.Top + 2, num3 + 1, drawRect.Bottom - 3);
          }
          else
          {
            int num4 = drawRect.Top + (drawRect.Height - 2) / 2;
            g.DrawLine(pen2, drawRect.Left + 2, num4, drawRect.Right - 3, num4);
            g.DrawLine(pen3, drawRect.Left + 2, num4 + 1, drawRect.Right - 3, num4 + 1);
          }
        }
        return;
      }
    IL_16E:
      using (Pen pen4 = new Pen(sepDarkColor))
      {
        if (direction == LayoutDirection.Horizontal)
        {
          int num5 = drawRect.Left + (drawRect.Width - 2) / 2;
          int num6 = drawRect.Height / 10;
          g.DrawLine(pen4, num5, drawRect.Top + num6, num5, drawRect.Bottom - num6 - 2);
        }
        else
        {
          int num7 = drawRect.Top + (drawRect.Height - 2) / 2;
          int num8 = drawRect.Width / 10;
          g.DrawLine(pen4, drawRect.Left + num8, num7, drawRect.Right - num8 - 2, num7);
        }
        return;
      }
    IL_214:
      using (Pen pen5 = new Pen(sepDarkColor))
      {
        using (Pen pen6 = new Pen(sepLightColor))
        {
          if (direction == LayoutDirection.Horizontal)
          {
            int num9 = drawRect.Left + (drawRect.Width - 2) / 2;
            int num10 = drawRect.Height / 10;
            g.DrawLine(pen5, num9, drawRect.Top + num10, num9, drawRect.Bottom - num10 - 2);
            g.DrawLine(pen6, num9 + 1, drawRect.Top + num10 + 1, num9 + 1, drawRect.Bottom - num10 - 1);
          }
          else
          {
            int num11 = drawRect.Top + (drawRect.Height - 2) / 2;
            int num12 = drawRect.Width / 10;
            g.DrawLine(pen5, drawRect.Left + num12, num11, drawRect.Right - num12 - 2, num11);
            g.DrawLine(pen6, drawRect.Left + num12 + 1, num11 + 1, drawRect.Right - num12 - 1, num11 + 1);
          }
        }
      }
    }
    public static void DrawPlainRaised(Graphics g, Rectangle boxRect, Color baseColor)
    {
      using (Pen pen = new Pen(ControlPaint.LightLight(baseColor)))
      {
        using (Pen pen2 = new Pen(ControlPaint.Dark(baseColor)))
        {
          g.DrawLine(pen, boxRect.Left, boxRect.Bottom, boxRect.Left, boxRect.Top);
          g.DrawLine(pen, boxRect.Left, boxRect.Top, boxRect.Right, boxRect.Top);
          g.DrawLine(pen2, boxRect.Right, boxRect.Top, boxRect.Right, boxRect.Bottom);
          g.DrawLine(pen2, boxRect.Right, boxRect.Bottom, boxRect.Left, boxRect.Bottom);
        }
      }
    }
    public static void DrawPlainSunken(Graphics g, Rectangle boxRect, Color baseColor)
    {
      using (Pen pen = new Pen(ControlPaint.LightLight(baseColor)))
      {
        using (Pen pen2 = new Pen(ControlPaint.Dark(baseColor)))
        {
          g.DrawLine(pen2, boxRect.Left, boxRect.Bottom, boxRect.Left, boxRect.Top);
          g.DrawLine(pen2, boxRect.Left, boxRect.Top, boxRect.Right, boxRect.Top);
          g.DrawLine(pen, boxRect.Right, boxRect.Top, boxRect.Right, boxRect.Bottom);
          g.DrawLine(pen, boxRect.Right, boxRect.Bottom, boxRect.Left, boxRect.Bottom);
        }
      }
    }
    public static void DrawHandles(Graphics g, VisualStyle style, Rectangle rect, Color darkColor, Color lightColor, bool vertical)
    {
      if (!vertical)
      {
        if (rect.Height < 3)
        {
          return;
        }
        int num = 0;
        int num2 = rect.Width;
        num2 -= 8;
        if (num2 <= 3)
        {
          num++;
          num2 -= 3;
        }
        int num3 = num2 / 4;
        num += num3;
        if (num <= 0)
        {
          return;
        }
        int num4 = (rect.Height - 3) / 2;
        int num5 = 3 + (num - 1) * 4;
        int num6 = (rect.Width - num5) / 2;
        using (SolidBrush solidBrush = new SolidBrush(darkColor))
        {
          using (SolidBrush solidBrush2 = new SolidBrush(lightColor))
          {
            for (int i = 0; i < num; i++)
            {
              g.FillRectangle(solidBrush2, rect.X + num6 + 1, rect.Y + num4 + 1, 2, 2);
              g.FillRectangle(solidBrush, rect.X + num6, rect.Y + num4, 2, 2);
              num6 += 4;
            }
          }
          return;
        }
      }
      if (rect.Width >= 3)
      {
        int num7 = 0;
        int num8 = rect.Height;
        num8 -= 8;
        if (num8 <= 3)
        {
          num7++;
          num8 -= 3;
        }
        int num9 = num8 / 4;
        num7 += num9;
        if (num7 > 0)
        {
          int num10 = (rect.Width - 3) / 2;
          int num11 = 3 + (num7 - 1) * 4;
          int num12 = (rect.Height - num11) / 2;
          using (SolidBrush solidBrush3 = new SolidBrush(darkColor))
          {
            using (SolidBrush solidBrush4 = new SolidBrush(lightColor))
            {
              for (int j = 0; j < num7; j++)
              {
                g.FillRectangle(solidBrush4, rect.X + num10 + 1, rect.Y + num12 + 1, 2, 2);
                g.FillRectangle(solidBrush3, rect.X + num10, rect.Y + num12, 2, 2);
                num12 += 4;
              }
            }
          }
        }
      }
    }
    public static void DrawGradientBackground(Graphics g, Control control, Color base0, Color base1, Color base2, bool gradient)
    {
      CommandDraw.DrawGradientBackground(g, control, base0, base1, base2, gradient, control.ClientRectangle);
    }
    public static void DrawGradientBackground(Graphics g, Control control, Color base0, Color base1, Color base2, bool gradient, Rectangle drawRect)
    {
      if (gradient)
      {
        Form form = control.FindForm();
        Rectangle rectangle = control.RectangleToScreen(control.ClientRectangle);
        Rectangle rectangle2;
        if (form != null)
        {
          rectangle2 = form.RectangleToClient(rectangle);
        }
        else
        {
          rectangle2 = control.ClientRectangle;
        }
        rectangle2.X = -rectangle2.X;
        rectangle2.Y = -rectangle2.Y;
        rectangle2.Size = SystemInformation.VirtualScreen.Size;
        if (rectangle2.Width <= 0 || rectangle2.Height <= 0)
        {
          return;
        }
        using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangle2, base2, base1, 0f))
        {
          Blend blend = new Blend();
          blend.Factors = new float[]
					{
						default(float),
						1f,
						1f
					};
          blend.Positions = new float[]
					{
						default(float),
						0.58f,
						1f
					};
          linearGradientBrush.Blend = blend;
          g.FillRectangle(linearGradientBrush, drawRect);
          return;
        }
      }
      using (SolidBrush solidBrush = new SolidBrush(base0))
      {
        g.FillRectangle(solidBrush, drawRect);
      }
    }
    public static Color TrackBaseFromTrack(Color track, bool colors256)
    {
      if (!colors256)
      {
        return ColorHelper.CalculateColor(track, Color.White, 130);
      }
      if (track == SystemColors.Highlight)
      {
        return SystemColors.Highlight;
      }
      return ControlPaint.Light(track);
    }
    public static Color TrackLightFromTrack(Color track, bool colors256)
    {
      if (!colors256)
      {
        return ColorHelper.CalculateColor(track, Color.White, 70);
      }
      if (track == SystemColors.Highlight)
      {
        return SystemColors.Window;
      }
      return ControlPaint.LightLight(track);
    }
    public static Color TrackLightLightFromTrack(Color track, Color baseColor, bool colors256)
    {
      if (!colors256)
      {
        return ColorHelper.CalculateColor(CommandDraw.TrackLightFromTrack(track, colors256), baseColor, 128);
      }
      if (track == SystemColors.Highlight)
      {
        return SystemColors.Window;
      }
      return ControlPaint.LightLight(track);
    }
    public static Color MenuSeparatorFromBase(Color baseColor, bool colors256)
    {
      return baseColor;
    }
    public static Color TrackMenuInsideFromTrack(Color track, bool colors256)
    {
      return track;
    }
    public static Color MenuCheckInsideColor(Color track, bool colors256)
    {
      return track;
    }
    public static Color TrackMenuCheckInsideColor(Color track, bool colors256)
    {
      return track;
    }
    public static Color TrackDarkFromTrack(Color track, bool colors256)
    {
      return track;
    }
    public static Color BaseDarkFromBase(Color baseColor, bool colors256)
    {
      return ControlPaint.Dark(baseColor);
    }
    public static Color BaseLightFromBase(Color baseColor, bool colors256)
    {
      return ControlPaint.Light(baseColor);
    }
    public static Color OpenBaseFromBase(Color baseColor)
    {
      return CommandDraw.OpenBaseFromBase(baseColor, false);
    }
    public static Color OpenBaseFromBase(Color baseColor, bool colors256)
    {
      if (colors256)
      {
        return baseColor;
      }
      return ColorHelper.CalculateColor(baseColor, Color.White, 200);
    }
    public static Color OpenBorderFromBase(Color baseColor, bool colors256)
    {
      if (!colors256)
      {
        return ControlPaint.Dark(baseColor);
      }
      if (baseColor == SystemColors.Control)
      {
        return SystemColors.ControlDark;
      }
      return ControlPaint.Dark(baseColor);
    }
    public static Size TextSize(Graphics g, Font font, string text)
    {
      SizeF sizeF = g.MeasureString(text, font);
      return new Size((int)sizeF.Width + 1, (int)sizeF.Height + 1);
    }
    public static Size RawTextSize(Graphics g, Font font, string text)
    {
      Region[] array = new Region[1];
      RectangleF bounds = new RectangleF(0f, 0f, 9999f, 9999f);
      CharacterRange[] measurableCharacterRanges = new CharacterRange[]
			{
				new CharacterRange(0, text.Length)
			};
      using (StringFormat stringFormat = new StringFormat())
      {
        stringFormat.SetMeasurableCharacterRanges(measurableCharacterRanges);
        array = g.MeasureCharacterRanges(text, font, bounds, stringFormat);
        bounds = array[0].GetBounds(g);
      }
      return new Size((int)(bounds.Right + 1f), (int)font.GetHeight());
    }
    private static Rectangle ImageFromEdge(Rectangle drawRect, Size imageSize, TextEdge edge, LayoutDirection direction, int offset)
    {
      if (direction == LayoutDirection.Vertical)
      {
        switch (edge)
        {
          case TextEdge.Top:
            edge = TextEdge.Right;
            break;
          case TextEdge.Left:
            edge = TextEdge.Top;
            break;
          case TextEdge.Bottom:
            edge = TextEdge.Left;
            break;
          case TextEdge.Right:
            edge = TextEdge.Bottom;
            break;
        }
      }
      switch (edge)
      {
        case TextEdge.Top:
          {
            int num = (drawRect.Width - imageSize.Width) / 2;
            return new Rectangle(drawRect.Left + num, drawRect.Bottom - imageSize.Height - offset, imageSize.Width, imageSize.Height);
          }
        case TextEdge.Left:
          {
            int num2 = (drawRect.Height - imageSize.Height) / 2;
            return new Rectangle(drawRect.Right - imageSize.Width - offset, drawRect.Top + num2, imageSize.Width, imageSize.Height);
          }
        case TextEdge.Bottom:
          {
            int num3 = (drawRect.Width - imageSize.Width) / 2;
            return new Rectangle(drawRect.Left + num3, drawRect.Top + offset, imageSize.Width, imageSize.Height);
          }
      }
      int num4 = (drawRect.Height - imageSize.Height) / 2;
      return new Rectangle(drawRect.Left + offset, drawRect.Top + num4, imageSize.Width, imageSize.Height);
    }
    private static Rectangle TextFromImageEdge(Rectangle drawRect, Rectangle imageRect, TextEdge edge, LayoutDirection direction, int offset)
    {
      if (direction == LayoutDirection.Vertical)
      {
        switch (edge)
        {
          case TextEdge.Top:
            edge = TextEdge.Right;
            break;
          case TextEdge.Left:
            edge = TextEdge.Top;
            break;
          case TextEdge.Bottom:
            edge = TextEdge.Left;
            break;
          case TextEdge.Right:
            edge = TextEdge.Bottom;
            break;
        }
      }
      switch (edge)
      {
        case TextEdge.Top:
          return new Rectangle(drawRect.Left, drawRect.Top + offset, drawRect.Width, imageRect.Top - drawRect.Top - offset * 2);
        case TextEdge.Left:
          return new Rectangle(drawRect.Left + offset, drawRect.Top, imageRect.Left - drawRect.Left - offset * 2, drawRect.Height);
        case TextEdge.Bottom:
          return new Rectangle(drawRect.Left, imageRect.Bottom + offset, drawRect.Width, drawRect.Bottom - imageRect.Bottom - offset * 2);
      }
      return new Rectangle(imageRect.Right + offset, drawRect.Top, drawRect.Right - imageRect.Right - offset * 2, drawRect.Height);
    }
  }
}
