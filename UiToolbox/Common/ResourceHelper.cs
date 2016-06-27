using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Aspire.UiToolbox.Common
{
  public sealed class ResourceHelper
  {
    private ResourceHelper()
    {
    }
    public static Cursor LoadCursor(Type assemblyType, string cursorName)
    {
      Assembly assembly = Assembly.GetAssembly(assemblyType);
      Stream manifestResourceStream = assembly.GetManifestResourceStream(cursorName);
      Cursor result = new Cursor(manifestResourceStream);
      manifestResourceStream.Close();
      return result;
    }
    public static Icon LoadIcon(Type assemblyType, string iconName)
    {
      Assembly assembly = Assembly.GetAssembly(assemblyType);
      Stream manifestResourceStream = assembly.GetManifestResourceStream(iconName);
      Icon result = new Icon(manifestResourceStream);
      manifestResourceStream.Close();
      return result;
    }
    public static Icon LoadIcon(Type assemblyType, string iconName, Size iconSize)
    {
      Icon icon = ResourceHelper.LoadIcon(assemblyType, iconName);
      return new Icon(icon, iconSize);
    }
    public static Bitmap LoadBitmap(Type assemblyType, string imageName)
    {
      return ResourceHelper.LoadBitmap(assemblyType, imageName, false, new Point(0, 0));
    }
    public static Bitmap LoadBitmap(Type assemblyType, string imageName, Point transparentPixel)
    {
      return ResourceHelper.LoadBitmap(assemblyType, imageName, true, transparentPixel);
    }
    public static ImageList LoadBitmapStrip(Type assemblyType, string imageName, Size imageSize)
    {
      return ResourceHelper.LoadBitmapStrip(assemblyType, imageName, imageSize, false, new Point(0, 0));
    }
    public static ImageList LoadBitmapStrip(Type assemblyType, string imageName, Size imageSize, Point transparentPixel)
    {
      return ResourceHelper.LoadBitmapStrip(assemblyType, imageName, imageSize, true, transparentPixel);
    }
    private static Bitmap LoadBitmap(Type assemblyType, string imageName, bool makeTransparent, Point transparentPixel)
    {
      Assembly assembly = Assembly.GetAssembly(assemblyType);
      Stream manifestResourceStream = assembly.GetManifestResourceStream(imageName);
      Bitmap bitmap = new Bitmap(manifestResourceStream, true);
      if (makeTransparent)
      {
        Color pixel = bitmap.GetPixel(transparentPixel.X, transparentPixel.Y);
        bitmap.MakeTransparent(pixel);
      }
      manifestResourceStream.Close();
      return bitmap;
    }
    private static ImageList LoadBitmapStrip(Type assemblyType, string imageName, Size imageSize, bool makeTransparent, Point transparentPixel)
    {
      ImageList imageList = new ImageList();
      imageList.ImageSize = imageSize;
      Assembly assembly = Assembly.GetAssembly(assemblyType);
      Stream manifestResourceStream = assembly.GetManifestResourceStream(imageName);
      Bitmap bitmap = new Bitmap(manifestResourceStream, true);
      if (makeTransparent)
      {
        Color pixel = bitmap.GetPixel(transparentPixel.X, transparentPixel.Y);
        bitmap.MakeTransparent(pixel);
      }
      imageList.Images.AddStrip(bitmap);
      manifestResourceStream.Close();
      return imageList;
    }
  }
}
