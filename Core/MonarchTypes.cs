
using Aspire.Core.Utilities;

namespace Aspire.Core
{
	public class MonarchTypes
	{
		public enum ComponentType { CAS=0, LS, SML, SM, Gateway, Other, Unknown };
	}
	public class Debug
	{
		public enum Level { Off, Fatal, Error, Warning, Info, Debug, Trace, All };

		static Level mLevel;
		public static void SetLevel(Level level) { mLevel = level; }

		public static void Printf(Level level, string format, params object[] values)
		{
			if (mLevel >= level)
				MsgConsole.WriteLine(format, values);
		}
	}
}
