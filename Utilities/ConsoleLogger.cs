using System;

namespace Aspire.Utilities
{
	/// <summary>
	/// LogListener for console output
	/// </summary>
	public class ConsoleLogger
	{
		/// <summary>
		/// Defaultconstructor
		/// </summary>
		public ConsoleLogger()
		{
			Log.NewText += Log_NewText;
			//Logger.Logged += new Logger.LoggedEventHandler(Log_Logged);
		}

		void Log_NewText(string text, Log.Severity severity)
		{
			Console.WriteLine("{0}:{1}", severity, text);
		}

		//void Log_Logged(LogEventArgs e)
		//{
		//	Console.WriteLine("{0}:{1} {2}",e.Source,e.Severity, e.Text);
		//}
	}
}
