using System;
using System.Runtime.InteropServices;

namespace Aspire.Utilities
{
  /// <summary>
  /// MediaUtilities provides media-related methods and properties.
  /// </summary>
  public sealed class MediaUtilities
  {
    [DllImport("kernel32.dll")]
    private static extern bool Beep(int freq, int dur);

    /// <summary>
    /// Play a sound of the specified frequency for the specified number of milliseconds. Returns immediately.
    /// </summary>
    /// <param name="frequency">The frequency of the sound</param>
    /// <param name="duration">The duration, in milliseconds, of the sound</param>
    public static void PlaySound(int frequency, int duration)
    {
      PlaySound(frequency, duration, false);
    }

    /// <summary>
    /// Play a sound of the specified frequency for the specified number of milliseconds.
    /// </summary>
    /// <param name="frequency">The frequency of the sound</param>
    /// <param name="duration">The duration, in milliseconds, of the sound</param>
    /// <param name="waitForSoundToFinish">If true, waits for the sound to complete before returning.</param>
    public static void PlaySound(int frequency, int duration, bool waitForSoundToFinish)
    {
      if (waitForSoundToFinish)
      {
        Beep(frequency, duration);
      }
      else
      {
        Action wrappedAction = () =>
        {
          Beep(frequency, duration);
        };

        IAsyncResult result = wrappedAction.BeginInvoke(null, null);
      }
    }

  }
}
