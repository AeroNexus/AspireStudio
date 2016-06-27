using System;

namespace Aspire.Utilities
{
	public class Log
	{
		public enum Severity { Debug, Info, Warning, Error };

		public delegate void TextHandler(string text, Severity severity);
		public static event TextHandler NewText;
		static int mDebugLevel = 1;

		public static string ExceptionText(Exception e)
		{
			if (e.InnerException == null)
				return e.Message;
			else
				return e.Message + Environment.NewLine + ExceptionText(e.InnerException);
		}

		public static void ReportException(Exception e, string format="", params object[] args)
		{
			var text = ExceptionText(e) + Environment.NewLine +
				"In assembly: " + e.Source + Environment.NewLine +
				e.StackTrace + Environment.NewLine +
				string.Format(format, args);
			NewText(text,Severity.Error);
		}

		public static void Warning(string format, params object[] args)
		{
			string text;
			if (args.Length > 0)
				text = string.Format(format, args);
			else
				text = format;
			NewText(text, Severity.Warning);
		}

		public static void WriteLine(string format, params object[] args)
		{
			string text;
			if (args.Length > 0)
				text = string.Format(format, args);
			else
				text = format;
			NewText(text, Severity.Info);
		}

    public static void WriteLine(int debugLevel, string format, params object[] args)
    {
      if (debugLevel > mDebugLevel) return;

      string text;
      if (args.Length > 0)
        text = string.Format(format, args);
      else
        text = format;
      NewText(text, Severity.Info);
    }

    public static void WriteLine(Severity debugLevel, string format, params object[] args)
    {
      WriteLine((int)debugLevel, format, args);
    }
  }
}
