using System;
using System.Text;

namespace Aspire.Core.Utilities
{
	public struct SecTime
	{
		private int mSec, mUSec;

		static SecTime infinite = new SecTime(-1, 0);
		public static SecTime Infinite { get { return infinite; } }

		public SecTime(int sec, int uSec)
		{
			mSec = sec;
			mUSec = uSec;
		}

		public SecTime(double seconds)
		{
			mSec = (int)seconds;
			mUSec = (int)((seconds-mSec)*1000000);
		}

		public SecTime(SecTime rhs)
		{
			mSec = rhs.mSec;
			mUSec = rhs.mUSec;
		}

		public int this[int i]
		{
			get { return i == 0 ? mSec : mUSec; }
			set
			{
				if (i == 0) mSec = value;
				else if (i == 1) mUSec = value;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			return ((SecTime)obj).mSec == mSec && ((SecTime)obj).mUSec == mUSec;
		}

		public override int GetHashCode()
		{
			return mSec+mUSec;
		}

		static public SecTime Milliseconds(int ms) { return new SecTime(0, ms * 1000); }

		public static SecTime operator+(SecTime lhs, SecTime rhs)
		{
			SecTime result;
			result.mUSec = lhs.mUSec + rhs.mUSec;
			if (result.mUSec > 1000000)
			{
				result.mUSec -= 1000000;
				result.mSec = lhs.mSec + 1 + rhs.mSec;
			}
			else
				result.mSec = lhs.mSec + rhs.mSec;
			return result;
		}

		public static SecTime operator+(SecTime lhs, int rhs)
		{
			SecTime result;
			result.mUSec = lhs.mUSec;
			result.mSec = lhs.mSec + rhs;
			return result;
		}

		public static SecTime operator-(SecTime lhs, SecTime rhs)
		{
			SecTime result;
			result.mUSec = lhs.mUSec - rhs.mUSec;
			if (result.mUSec > 0)
				result.mSec = lhs.mSec - rhs.mSec;
			else
			{
				result.mUSec += 1000000;
				result.mSec = lhs.mSec - 1 - rhs.mSec;
			}
			return result;
		}

		public static SecTime operator-(SecTime lhs, int rhs)
		{
			SecTime result;
			result.mUSec = lhs.mUSec;
			result.mSec = lhs.mSec - rhs;
			return result;
		}

		public static bool operator>(SecTime lhs, int rhsUsec)
		{
			return lhs.mSec >= 0 || lhs.mUSec > rhsUsec;
		}

		public static bool operator<(SecTime lhs, SecTime rhs)
		{
			if (lhs.mSec < rhs.mSec)
				return true;
			else if (lhs.mSec == rhs.mSec && lhs.mUSec < rhs.mUSec)
				return true;
			return false;
		}

		public static bool operator<=(SecTime lhs, SecTime rhs)
		{
			if (lhs.mSec < rhs.mSec)
				return true;
			else if (lhs.mSec == rhs.mSec && lhs.mUSec <= rhs.mUSec)
				return true;
			return false;
		}

		public static bool operator>(SecTime lhs, SecTime rhs)
		{
			if (lhs.mSec > rhs.mSec)
				return true;
			else if (lhs.mSec == rhs.mSec && lhs.mUSec > rhs.mUSec)
				return true;
			return false;
		}

		public static bool operator>=(SecTime lhs, SecTime rhs)
		{
			if (lhs.mSec > rhs.mSec)
				return true;
			else if (lhs.mSec == rhs.mSec && lhs.mUSec >= rhs.mUSec)
				return true;
			return false;
		}

		public static bool operator<(SecTime lhs, int rhsUsec)
		{
			return lhs.mSec <= 0 || lhs.mUSec < rhsUsec;
		}

		public bool IsInfinite { get { return mSec == -1; } }

		public int Seconds
		{
			get { return mSec; }
			set { mSec = value; }
		}

		public int USeconds
		{
			get { return mUSec; }
			set { mUSec = value; }
		}

		public void Set(int seconds, int microSeconds)
		{
			mSec = seconds;
			mUSec = microSeconds;
		}

		public double ToDouble
		{
			get { return mUSec*0.000001 + mSec; }
		}

		public int ToMicroSeconds { get { return mSec*1000000+mUSec; } }

		public int ToMilliSeconds { get { return mSec*1000+mUSec/1000; } }

		public override string ToString()
		{
			int dSec = mSec;
			int dUsec = mUSec;
			var sb = new StringBuilder(); 
			if ( dSec < 0 )
			{
				dSec++;
				if ( dSec == 0)
					sb.Append('-');
				dUsec = 1000000 - dUsec;
			}
			sb.AppendFormat("{0}.{1,6:D6}", dSec, dUsec);
			return sb.ToString();
		}
	}
}
