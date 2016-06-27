using System;

namespace Aspire.Core.Utilities
{
	public static class Logger
	{
		static int mLogLevel;
		static ILogger mLogger = new MsgConsole();

		public static void Flush()
		{
			mLogger.Flush();
		}

		public static void Log(int level, string format, params object[] args)
		{
			if (level > mLogLevel)
				return;

			var text = string.Format(format,args);

			mLogger.Log(text);
		}

		public static int LogLevel { get { return mLogLevel; } set { mLogLevel = value; } }
	}
}
