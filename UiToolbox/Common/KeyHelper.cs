using System;

using Aspire.UiToolbox.Win32;

namespace Aspire.UiToolbox.Common
{
  public sealed class KeyHelper
  {
    public static bool CTRLPressed
    {
      get
      {
        return (User32.GetKeyState(17) & 32768) != 0;
      }
    }
    public static bool SHIFTPressed
    {
      get
      {
        return (User32.GetKeyState(16) & 32768) != 0;
      }
    }
    public static bool ALTPressed
    {
      get
      {
        return (User32.GetKeyState(18) & 32768) != 0;
      }
    }
  }
}
