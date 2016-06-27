using System;
using System.Text;

namespace Aspire.Primitives
{
	public class Maths
    {
		/// <summary>
		/// n! (by 2s)
		/// </summary>
		/// <param name="n">Number to factorialize</param>
		/// <returns>n!</returns>
		public static double AltFactorial(double n)
		{
			if (n <= 1.0)
				return 1.0;
			else
				return n * AltFactorial(n - 2.0);
		}

		/// <summary>
		/// n!
		/// </summary>
		/// <param name="n">Number to factorialize</param>
		/// <returns>n!</returns>
		public static double Factorial(double n)
		{
			if (n <= 2)
				return n;
			else
				return n * Factorial(n - 1);
		}

		/// <summary>
		/// Format an angle according to the format
		/// </summary>
		/// <param name="format">A series of characters, embedded with field denoted by %X. Valid fields are:
		/// %+	Use the sign, even if the angle is positive
		/// %@  degree symbol
		/// %D  Degrees
		/// %M  Minutes
		/// %S  Seconds
		/// %H  Hours
		/// %f  remaining fraction of the least significant field written so far
		/// There can also be [width].[precision] between the % and the field identifier.
		///		This is currently only used for %f
		/// </param>
		/// <param name="angle">The angle to format [deg]</param>
		/// <returns>A formatted string</returns>
		/// <example>
		/// angle = 316.17291;
		/// FormatAngle( "%D^%f, angle)                 => 316^.17291
		/// FormatAngle( "%Hh%f", angle )               => 21h.078194
		/// FormatAngle( "%Hh%Mm%Ss%.2f", angle )       => 21h04m41s.50
		/// FormatAngle( "%+%D^%M'%S\"%.1f", 18.88802 ) => +18^53'16".9
		/// </example>
		public static string FormatAngle(string format, double angle)
		{
			var sb = new StringBuilder();

			int cursor = 0, deg = 0, hour = 0, min = 0, sec = 0, width, precision;
			bool negative = angle < 0, doingWidth;
			string fractionFormat;
			char c;
			angle = Math.Abs(angle);
			if (negative)
				sb.Append('-');
			while (cursor < format.Length)
			{
				while (format[cursor] != '%')
				{
					sb.Append(format[cursor++]);
					if (cursor >= format.Length) break;
				}
				doingWidth = true;
				width = precision = 0;
			Again:
				if (cursor + 1 >= format.Length) break;
				switch (format[++cursor])
				{
					case '+':
					case '-':
						if (!negative)
							sb.Append('+');
						break;
					case '@': // Unicode degree symbol
						sb.Append('\u00B0');
						break;
					case 'D':
					case 'd':
						deg = (int)angle;
						sb.Append(deg);
						angle -= deg;
						break;
					case 'H':
					case 'h':
						angle /= 15;
						hour = (int)angle;
						sb.AppendFormat("{0:0#}", hour);
						angle -= hour;
						break;
					case 'M':
					case 'm':
						angle *= 60.0;
						min = (int)angle;
						sb.AppendFormat("{0:0#}", min);
						angle -= min;
						break;
					case 'S':
					case 's':
						angle *= 60;
						sec = (int)angle;
						sb.AppendFormat("{0:0#}", sec);
						angle -= sec;
						break;
					case 'F':
					case 'f':
						if (width > 0 || precision > 0)
						{
							var fb = new StringBuilder();
							fb.Append("{0");
							if (width > 0)
							{
								fb.Append(',');
								fb.Append(width);
							}
							fb.Append(":F");
							fb.Append(precision);
							fb.Append('}');
							fractionFormat = fb.ToString();
						}
						else
							fractionFormat = "{0:G5}";
						string str = String.Format(fractionFormat, angle);
						sb.Append(str.Substring(1));
						break;
					case '.':
						doingWidth = false;
						goto Again;
					default:
						c = format[cursor];
						if ('0' <= c && c <= '9')
						{
							if (doingWidth)
								width = width * 10 + c - '0';
							else
								precision = precision * 10 + c - '0';
						}
						goto Again;
				}
				cursor++;
			}
			return sb.ToString();
		}

		/// <summary>
		/// Returns the remainder of operand / mudulus between 0 and modulus
		/// </summary>
		/// <param name="operand">Number to take modulo of</param>
		/// <param name="modulus">Divisor</param>
		/// <returns>The remainder of operane / modulus</returns>
		public static double Mod(double operand, double modulus)
		{
			double modulo = Math.IEEERemainder(operand, modulus);
			if (modulo < 0.0) modulo += modulus;
			return modulo;
		}

		/// <summary>
		/// Returns the remainder of operand / mudulus between 0 and modulus
		/// </summary>
		/// <param name="operand">Number to take modulo of</param>
		/// <param name="modulus">Divisor</param>
		/// <returns>The remainder of operane / modulus</returns>
		public static float Mod(float operand, float modulus)
		{
			float modulo = (float)Math.IEEERemainder(operand, modulus);
			if (modulo < 0.0f) modulo += modulus;
			return modulo;
		}
	}
}
