using System;

using Aspire.Primitives;

namespace Aspire.Space
{
	public class JacciaRobertsAtmosphere : IAtmosphere
	{
		#region Declarations

		const double Rair = 286.99758;

		double T0 = 288;
		double P0 = 101.325;
		double lapseRate = 1;
		double density, pressure, temperature;

		#endregion

		#region Properties

		#endregion

		void Compute(double geodeticAltitude)
		{
			double h = geodeticAltitude / Constant.Kilo;
			pressure = P0 * Math.Log(-h);
			temperature = T0 - lapseRate * h;
			density = pressure / (Rair * temperature);
		}

		#region IAtmosphere Members

		public double Density(double geodeticAltitude)
		{
			Compute(geodeticAltitude);
			return density;
		}

		public double Pressure(double geodeticAltitude)
		{
			Compute(geodeticAltitude);
			return pressure;
		}

		public double Temperature(double geodeticAltitude)
		{
			Compute(geodeticAltitude);
			return temperature;
		}

		#endregion
	}
}
