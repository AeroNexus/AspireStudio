using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Common
{
  public class CommandImage : IDisposable
  {
    private readonly Color _transparent = Color.FromArgb(0, 0, 0, 0);
    private readonly int EXTRA_IDE_WIDTH = 2;
    private readonly int EXTRA_IDE_HEIGHT = 2;
    private readonly int EXTRA_OFFICE2003_WIDTH = 2;
    private readonly int EXTRA_OFFICE2003_HEIGHT = 2;
    private readonly int EXTRA_PLAIN_WIDTH = 1;
    private readonly int EXTRA_PLAIN_HEIGHT = 1;
    private Image _image;
    private Image _disabledImage;
    private Color _disabledBackColor;
    private Image _whiteImage;
    private Image _fadedImage;
    public static CommandImage Empty
    {
      get
      {
        return new CommandImage();
      }
    }
    public virtual Image Image
    {
      get
      {
        return _image;
      }
      set
      {
        if (_image != value)
        {
          if (_image != null)
          {
            _image = null;
          }
          if (_disabledImage != null)
          {
            _disabledImage.Dispose();
            _disabledImage = null;
          }
          if (_whiteImage != null)
          {
            _whiteImage.Dispose();
            _whiteImage = null;
          }
          if (_fadedImage != null)
          {
            _fadedImage.Dispose();
            _fadedImage = null;
          }
          _image = value;
        }
      }
    }
    public virtual Image WhiteImage
    {
      get
      {
        if (_whiteImage == null)
        {
          Bitmap bitmap = new Bitmap(_image);
          Size imageSize = ImageSize;
          for (int i = 0; i < imageSize.Width; i++)
          {
            for (int j = 0; j < imageSize.Height; j++)
            {
              Color pixel = bitmap.GetPixel(i, j);
              if (pixel != _transparent)
              {
                if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255 && pixel.A == 255)
                {
                  bitmap.SetPixel(i, j, _transparent);
                }
                else
                {
                  bitmap.SetPixel(i, j, Color.White);
                }
              }
            }
          }
          _whiteImage = bitmap;
        }
        return _whiteImage;
      }
    }
    public virtual Image FadedImage
    {
      get
      {
        if (_fadedImage == null)
        {
          Bitmap bitmap = new Bitmap(_image);
          Size imageSize = ImageSize;
          for (int i = 0; i < imageSize.Width; i++)
          {
            for (int j = 0; j < imageSize.Height; j++)
            {
              Color pixel = bitmap.GetPixel(i, j);
              if (pixel != _transparent)
              {
                Color color = Color.FromArgb((int)(pixel.R + 76 - (pixel.R + 32) / 64 * 19), (int)(pixel.G + 76 - (pixel.G + 32) / 64 * 19), (int)(pixel.B + 76 - (pixel.B + 32) / 64 * 19));
                bitmap.SetPixel(i, j, color);
              }
            }
          }
          _fadedImage = bitmap;
        }
        return _fadedImage;
      }
    }
    public virtual Size ImageSize
    {
      get
      {
        if (Image == null)
        {
          return Size.Empty;
        }
        return Image.Size;
      }
    }
    public CommandImage()
    {
      _image = null;
      _disabledImage = null;
      _disabledBackColor = Color.Empty;
      _whiteImage = null;
      _fadedImage = null;
    }
    public void Dispose()
    {
      if (_image != null)
      {
        _image = null;
      }
      if (_disabledImage != null)
      {
        _disabledImage.Dispose();
        _disabledImage = null;
      }
      if (_whiteImage != null)
      {
        _whiteImage.Dispose();
        _whiteImage = null;
      }
      if (_fadedImage != null)
      {
        _fadedImage.Dispose();
        _fadedImage = null;
      }
    }
    public CommandImage(Image image)
    {
      _image = image;
      _disabledImage = null;
      _whiteImage = null;
      _fadedImage = null;
    }
    public virtual Image GetDisabledImage(Color backColor)
    {
      if (_disabledImage == null || backColor != _disabledBackColor)
      {
        Bitmap bitmap = new Bitmap(_image.Width, _image.Height, PixelFormat.Format32bppArgb);
        Graphics graphics = Graphics.FromImage(bitmap);
        ControlPaint.DrawImageDisabled(graphics, _image, 0, 0, backColor);
        graphics.Dispose();
        _disabledBackColor = backColor;
        _disabledImage = bitmap;
      }
      return _disabledImage;
    }
    public virtual Size ImageSpace(VisualStyle style)
    {
      Size imageSize = ImageSize;
      switch (style)
      {
        case VisualStyle.IDE:
          imageSize.Width = imageSize.Width + EXTRA_IDE_WIDTH;
          imageSize.Height = imageSize.Height + EXTRA_IDE_HEIGHT;
          return imageSize;
        case VisualStyle.Office2003:
        case VisualStyle.IDE2005:
          imageSize.Width = imageSize.Width + EXTRA_OFFICE2003_WIDTH;
          imageSize.Height = imageSize.Height + EXTRA_OFFICE2003_HEIGHT;
          return imageSize;
      }
      imageSize.Width = imageSize.Width + EXTRA_PLAIN_WIDTH;
      imageSize.Height = imageSize.Height + EXTRA_PLAIN_HEIGHT;
      return imageSize;
    }
    public static Image CreateGrayScaleImage(Image source)
    {
      Bitmap bitmap = new Bitmap(source);
      Size arg_0D_0 = source.Size;
      for (int i = 0; i < bitmap.Width; i++)
      {
        for (int j = 0; j < bitmap.Height; j++)
        {
          Color pixel = bitmap.GetPixel(i, j);
          int num = (int)(((float)pixel.R * 0.3f + (float)pixel.G * 0.59f + (float)pixel.B * 0.11f) * 1.5f);
          if (num > 255)
          {
            num = 255;
          }
          bitmap.SetPixel(i, j, Color.FromArgb((int)pixel.A, num, num, num));
        }
      }
      return bitmap;
    }
  }
}
