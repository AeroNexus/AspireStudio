using System;
using System.Reflection;

namespace Aspire.Utilities.Extensions
{
  public static class MiscExtensions
  {
    /// <summary>
    /// Execute an <see cref="Action"/> and ignore any exceptions that are raised.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public static void ExecuteAndIgnoreException(this Action action)
    {
      try
      {
        action.Invoke();
      }
      catch { }
    }

    /// <summary>
    /// Execute an <see cref="Action"/> that takes a single parameter, ignoring any exceptions that are raised.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="param">The parameter value.</param>
    public static void ExecuteAndIgnoreException<T>(this Action<T> action, T param)
    {
      try
      {
        action.Invoke(param);
      }
      catch { }
    }

    /// <summary>
    /// Execute an <see cref="Action"/> and report any exceptions that are raised.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public static void ExecuteAndReportException(this Action action)
    {
      try
      {
        action.Invoke();
      }
      catch (Exception ex)
      {
        Log.ReportException(ex,string.Empty);
      }
    }


    /// <summary>
    /// Attempts to get the value of a property from a <see cref="PropertyInfo"/>
    /// </summary>
    /// <param name="pi">The <see cref="PropertyInfo"/>.</param>
    /// <param name="obj">The <see cref="object"/> whose property value will be returned.</param>
    /// <param name="index">Optional index values for indexed properties. This value should be null for non-indexed properties.</param>
    /// <param name="value">An out parameter that will hold the property value if available; null if an exception occurs</param>
    /// <returns>True if the property value was successfully retrieved; false otherwise.</returns>
    public static bool TryGetValue(this PropertyInfo pi, object obj, object[] index, out object value)
    {
      try
      {
        value = pi.GetValue(obj, index);
      }
      catch
      {
        value = null;
        return false;
      }
      return true;
    }

  }
}
