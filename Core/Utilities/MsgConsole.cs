using System;

using AspireUtilities = Aspire.Utilities;

namespace Aspire.Core.Utilities
{
	public class MsgConsole : ILogger
	{
		public void Flush()
		{
			//AspireUtilities.Log.Flush();
		}

		public void Log(string text)
		{
			AspireUtilities.Log.WriteLine(text);
		}

		public static void WriteLine(string text, params object[] args)
		{
			AspireUtilities.Log.WriteLine(text, args);
		}

		public static void WriteLine(string text, MsgLevel level, string format, params object[] args)
		{
			AspireUtilities.Log.WriteLine(text+": "+format, args);
		}

		public static void ReportException(string text, MsgLevel severity, Exception exception)
		{
			AspireUtilities.Log.ReportException(exception, text);
		}

		public static void ReportException(string text, MsgLevel severity, string header, Exception exception)
		{
			AspireUtilities.Log.ReportException(exception, header + ": " + text );
		}
	}

	public enum MsgLevel
	{
		Info=0,
		Warning,
		Error
	};
}
