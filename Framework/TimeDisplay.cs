using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Framework
{
	/// <summary>
	/// Format time for presentation on the Time toolbar
	/// </summary>
	public class TimeDisplay
	{
		static List<ITimeFormatter> formatters = new List<ITimeFormatter>();
		/// <summary>
		/// The current list of time formatters
		/// </summary>
		public List<ITimeFormatter> Formatters { get { return formatters; } }

		/// <summary>
		/// Default UTC format
		/// </summary>
		public const string UtcFormat = "yyyy/MM/dd HH:mm:ss.fff";

		Clock mClock;
		ITimeFormatter formatter;
		MissionTime missionTime = new MissionTime("T+0 00:00:00.000");
		string pendingFormat;
		static int numFormatters;

		/// <summary>
		/// Construct with parent Clock
		/// </summary>
		/// <param name="clock"></param>
		public TimeDisplay(Clock clock)
		{
			mClock = clock;
			formatter = clock;
		}

		/// <summary>
		/// Allow external time formatters to be added
		/// </summary>
		public static void AddFormatter(ITimeFormatter formatter)
		{
			foreach (var fmt in formatters)
				if (fmt.ToString() == formatter.ToString()) return;
			formatters.Add(formatter);
		}

		/// <summary>
		/// Configure the UI based on the time formatters we have
		/// </summary>
		public void Discover()
		{
			//sort
		}

		/// <summary>
		/// Gets the current display value using the current format
		/// </summary>
		public string Display
		{
			get
			{
				if (pendingFormat != null)
				{
					if (formatters.Count > numFormatters)
					{
						Format = pendingFormat;
						if (mFormat == pendingFormat)
							pendingFormat = null;
						else
							numFormatters = formatters.Count;
					}
				}
				return formatter.FormatTime(mClock);
			}
		}

		/// <summary>
		/// Access the format
		/// </summary>
		public string Format
		{
			get { return mFormat; }
			set
			{
				mFormat = value;
				foreach (var formatter in formatters)
					foreach (var name in formatter.Names)
						if (name == mFormat)
						{
							SetFormatter(formatter, mFormat);
							return;
						}
				if (this.formatter == null)
				{
					pendingFormat = mFormat;
					Format = "ElapsedSeconds";
					numFormatters = formatters.Count;
				}
			}
		} string mFormat;

		/// <summary>
		/// Resets internal state
		/// </summary>
		public void Initialize()
		{
			missionTime.Initialize(mClock.Seconds);
		}

		/// <summary>
		/// Parse internal state from a serialized string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static DateTime Parse(string value)
		{
			var tokens = value.Split('/', ' ', ':', '.');
			int year = 2000, month = 1, day = 1, hour = 0, minute = 0, second = 0, millisec = 0;
			try
			{
				if (tokens.Length > 0)
				{
					year = int.Parse(tokens[0]);
					if (tokens.Length > 1)
					{
						month = int.Parse(tokens[1]);
						if (tokens.Length > 2)
						{
							day = int.Parse(tokens[2]);
							if (tokens.Length > 3)
							{
								hour = int.Parse(tokens[3]);
								if (tokens.Length > 4)
								{
									minute = int.Parse(tokens[4]);
									if (tokens.Length > 5)
									{
										second = int.Parse(tokens[5]);
										if (tokens.Length > 6)
											millisec = int.Parse(tokens[6]);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception)
			{
				//Log.ReportException(e, "TimeDisplay.Parse({0})", value);
			}
			return new DateTime(year, month, day, hour, minute, second, millisec);
		}

		/// <summary>
		/// Allow the UI to set the time formatter
		/// </summary>
		public void SetFormatter(ITimeFormatter formatter, string format)
		{
			this.formatter = formatter;
			formatter.Format = format;
		}

		/// <summary>
		/// string representation
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Display;
		}
	}

	/// <summary>
	/// Format elapsed time as a mission clock +/-DDD HH:MM:SS
	/// </summary>
	public class MissionTime: ITimeFormatter
	{
		StringBuilder sb = new StringBuilder();
		bool countdown;
		int day, hour, hourStart, minute, second, milliSecond, milliStart;
		uint lastElapsedSeconds;
		string initial;

		/// <summary>
		/// Construct using a formatted string
		/// T+ddd hh:mm:ss, T-ddd hh:mm:ss, T-hh:mm:ss T+hh:mm T-hh T-:mm:ss T-::ss T-:mm
		/// </summary>
		/// <param name="timeStr"></param>
		public MissionTime(string timeStr)
		{
			if (timeStr.Length <= 3)
				throw new ArgumentException("Must at least start with T<+/-><ss>", "timeStr");
			if (timeStr[0] != 'T')
				throw new ArgumentException("Must start with T", "timeStr");
			countdown = timeStr[1] == '-';

			sb.Length = 0;
			sb.Append('T');
			if (countdown)
				sb.Append('-');
			else
				sb.Append('+');
			initial = timeStr;
			Initialize(0);
			TimeDisplay.AddFormatter(this);
		}

		/// <summary>
		/// Initialize to current elapsedSeconds, which might be negative
		/// </summary>
		/// <param name="elapsedSeconds"></param>
		public void Initialize(uint elapsedSeconds)
		{
			Parse(initial);
			sb.Length = 2;
			sb.AppendFormat("{0} ", day);
			hourStart = sb.Length;
			sb.AppendFormat("{0:00}:{1:00}:{2:00}", hour, minute, second);
			if (milliStart > 0)
			{
				sb.Length = milliStart - 1;
				sb.AppendFormat(".{0:000}", milliSecond);
			}
			lastElapsedSeconds = elapsedSeconds;
		}

		/// <summary>
		/// Parse internal state from a serialized string
		/// </summary>
		/// <param name="timeStr"></param>
		void Parse(string timeStr)
		{
			int start = 2;
			int i = timeStr.IndexOf(' ', start);
			if (i > 0)
			{
				day = Int32.Parse(timeStr.Substring(start, i - start));
				start = i + 1;
			}
			i = timeStr.IndexOf(':', start);
			if (i > 0)
			{
				if (i > start)
					hour = Int32.Parse(timeStr.Substring(start, i - start));
				start = i + 1;

				i = timeStr.IndexOf(':', start);
				if (i > 0)
				{
					if (i > start)
						minute = Int32.Parse(timeStr.Substring(start, i - start));
					start = i + 1;
					i = timeStr.IndexOf('.', start);
					if (i > 0)
					{
						if (i > start)
							second = Int32.Parse(timeStr.Substring(start, i - start));
						start = i + 1;
						if (start < timeStr.Length)
						{
							milliStart = start;
							milliSecond = Int32.Parse(timeStr.Substring(start));
						}
					}
					else if (start < timeStr.Length)
						second = Int32.Parse(timeStr.Substring(start));
				}
				else if (start < timeStr.Length)
					minute = Int32.Parse(timeStr.Substring(start, i - start));
			}
			else if (start < timeStr.Length)
				hour = Int32.Parse(timeStr.Substring(start));
		}

		/// <summary>
		/// Set from seconds:microSeconds
		/// </summary>
		/// <param name="elapsedSeconds"></param>
		/// <param name="microSeconds"></param>
		public void Set(uint elapsedSeconds, int microSeconds)
		{
			int dSec;
			if (elapsedSeconds != lastElapsedSeconds)
			{
				dSec = (int)(elapsedSeconds - lastElapsedSeconds);
				lastElapsedSeconds = elapsedSeconds;
				if (dSec != 0)
				{
					second += dSec;
					if (second >= 60 || second < 0)
					{
						if ( second >= 60 )
						{
							second = 0;
							minute++;
						}
						else
						{
							second = 59;
							minute--;
						}
						if (minute >= 60 || minute < 0)
						{
							if ( minute >= 60 )
							{
								minute = 0;
								hour++;
							}
							else
							{
								minute = 59;
								hour--;
							}
							if (hour >= 24 || hour < 0)
							{
								if (hour >= 24)
								{
									hour = 0;
									day++;
								}
								else
								{
									hour = 23;
									day--;
								}
								sb.Length = 2;
								sb.AppendFormat("{0} ", day);
								if (sb.Length != hourStart)
								{
									if (milliStart > 0)
										milliStart += sb.Length - hourStart;
									hourStart = sb.Length;
								}
							}
							sb.Length = hourStart;
							sb.AppendFormat("{0:00}:{1:00}:{2:00}", hour, minute, second);
						}
						else
						{
							sb.Length = hourStart + 3;
							sb.AppendFormat("{0:00}:{1:00}", minute, second);
						}
					}
					else
					{
						sb.Length = hourStart + 6;
						sb.AppendFormat("{0:00}", second);
					}
				}
			}
			else
				dSec = 0;
			if (milliStart > 0)
			{
				milliSecond = microSeconds / 1000;
				if (dSec != 0)
				{
					sb.Length = milliStart - 1;
					sb.AppendFormat(".{0:000}", milliSecond);
				}
				else
				{
					sb.Length = milliStart;
					sb.AppendFormat("{0:000}", milliSecond);
				}
			}
		}

		/// <summary>
		/// String representation
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return sb.ToString();
		}

		#region ITimeFormatter Members

		/// <summary>
		/// Selects one of many formats that the formatter knows
		/// </summary>
		public string Format { set { } }
		/// <summary>
		/// Format the time as mission time
		/// </summary>
		/// <param name="clock"></param>
		/// <returns></returns>
		public string FormatTime(Clock clock)
		{
			Set(clock.Seconds, clock.MicroSeconds);
			return ToString();
		}

		/// <summary>
		/// Displayed name
		/// </summary>
		public string[] Names { get { return new string[] {"Mission"}; } }

		#endregion
	}


}
