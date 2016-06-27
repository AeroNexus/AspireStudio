using System;

namespace Aspire.Primitives
{
	public static class Limit
    {
		/// <summary>
		/// Clamp constrains its (double) value such that min less= value less= max
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="min">The minimum allowable value.</param>
		/// <param name="max">The maximum allowable value.</param>
		/// <returns>The clamped value</returns>
		public static double Clamp(double value, double min, double max)
		{
			if (value < min) value = min;
			else if (value > max) value = max;
			return value;
		}

		/// <summary>
		/// Clamp constrains its (double) value such that -limit less= value less= limit
		/// </summary>
		/// <param name="value">The value to clamp</param>
		/// <param name="limit">The allowable limit, used as +/-.</param>
		/// <returns>The clamped value</returns>
		public static double Clamp(double value, double limit)
		{
			if (value < -limit) value = -limit;
			else if (value > limit) value = limit;
			return value;
		}

		/// <summary>
		/// Returns an angle between 0 and 360 deg, equivalent to the argument
		/// <p>(0 less= result less 360)</p>
		/// </summary>
		/// <param name="angle">The periodic value to constrain [deg]</param>
		/// <returns>A value between 0 and 360 [deg]</returns>
		public static double Fold360(double angle)
		{
			if (angle < 0)
			{
				int n = (int)(Math.Ceiling(-angle / 360));
				angle += n * 360.0;
			}
			else if (angle >= 360.0)
			{
				int n = (int)(angle / 360);
				angle -= n * 360.0;
			}
			return angle;
		}

		/// <summary>
		/// Returns an angle between -180 and 180 deg, equivalent to the argument
		/// <p>(-180 less result less= 180)</p>
		/// </summary>
		/// <param name="angle">The periodic value to constrain [deg]</param>
		/// <returns>A value between -180 and 180 [deg]</returns>
		public static double Fold180(double angle)
		{
			if (angle <= -180.0)
			{
				int n = (int)((180.0 - angle) / 360.0);
				angle += n * 360.0;
			}
			else if (angle >= 180.0)
			{
				int n = (int)(Math.Ceiling((angle + 180) / 360.0)) - 1;
				angle -= n * 360.0;
			}
			return angle;
		}

		/// <summary>
		/// Returns an angle between 0 and 2 pi (rad), equivalent to the argument
		/// <p>(0 less= result less 2pi)</p>
		/// </summary>
		/// <param name="angle">The periodic value to constrain [rad]</param>
		/// <returns>A value between 0 and 2*pi [rad]</returns>
		public static double FoldTwoPi(double angle)
		{
			if (angle < 0)
			{
				int n = (int)(Math.Ceiling(-angle / Constant.TwoPi));
				angle += n * Constant.TwoPi;
			}
			else if (angle >= Constant.TwoPi)
			{
				int n = (int)(angle / Constant.TwoPi);
				angle -= n * Constant.TwoPi;
			}
			return angle;
		}

		/// <summary>
		/// Returns an angle between -pi and pi (rad), equivalent to the argument
		/// <p>(-pi less result less= pi)</p>
		/// </summary>
		/// <param name="angle">The periodic value to constrain [rad]</param>
		/// <returns>A value between -pi and pi [rad]</returns>
		public static double FoldPi(double angle)
		{
			if (angle <= -Constant.Pi)
			{
				int n = (int)((Constant.Pi - angle) / Constant.TwoPi);
				angle += n * Constant.TwoPi;
			}
			else if (angle >= Constant.Pi)
			{
				int n = (int)(Math.Ceiling((angle + Constant.Pi) / Constant.TwoPi)) - 1;
				angle -= n * Constant.TwoPi;
			}
			return angle;
		}

	}
}
