using System;

namespace Aspire.Core.Utilities
{
	public class Clock
	{
		private static DateTime gpsTare = new DateTime(1980, 1, 6, 0, 0, 0);
		private static DateTime start = DateTime.Now;

		public static void GetGps(ref SecTime secTime)
		{
			TimeSpan diff = DateTime.Now - gpsTare;
			int sec = (int)diff.TotalSeconds;
			secTime.Set(sec, diff.Milliseconds*1000);
		}

		public static void GetTime(ref SecTime secTime)
		{
			TimeSpan diff = DateTime.Now - start;
			int sec = (int)diff.TotalSeconds;
			secTime.Set(sec, diff.Milliseconds*1000);
		}

		public static double Elapsed
		{
			get
			{
				TimeSpan diff = DateTime.Now - start;
				return diff.TotalSeconds;
			}
		}
	}
}
