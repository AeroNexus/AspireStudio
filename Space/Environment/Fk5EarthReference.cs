using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Fk5EarthReference : Model, IReferenceFrameModel
	{
		#region Declarations
		const double ArcSecPerDeg = 60.0*60.0;
		const double DegPerArcSec = 1/ArcSecPerDeg;
		const double RadPerArcSec = DegPerArcSec*Constant.RadPerDeg;
		const double TAItoTT = 32.184;

		AstroClock clock;

		Dcm precession = new Dcm();
		Dcm nutation = new Dcm();
		Dcm precessionNutation = new Dcm();
		Dcm gastDcm = new Dcm().Identity();
		Dcm celestialTerrestrial = new Dcm();
		Dcm polarMotionDcm = new Dcm();

		double
			dUT1,
			equationOfTheEquinoxes,
			gast, gmst,
			Xp, Yp;

		double[] Cgmst = { 24110.54841, 8640184.812866, 0.093104, -6.2e-6 };

		double[] Mm_s = { 485866.733, 715922.633, 31.310, 0.064, 1325 };
		double[] Ms_s = { 1287099.804, 1292581.224, -0.577, -0.012, 99 };
		double[] uMm_s = { 335778.877, 295263.137, -13.257, 0.011, 1342 };
		double[] Ds_s = { 1072261.307, 1105601.328, -6.891, 0.019, 1236 };
		double[] Wm_s = { 450160.280, -482890.539, 7.455, 0.008, -5 };

		struct NutCoeffs
		{
			internal short a, b, c, d, e;
			internal double A, B, C, D;
			internal NutCoeffs(short a, short b, short c, short d,short e,
				double A, double B, double C, double D)
			{
				this.a = a; this.b = b; this.c = c; this.d = d; this.e = e;
				this.A = A; this.B = B; this.C = C; this.D = D;
			}
		};
		// http://bowie.mit.edu/~tah/mhb2000/IAU_1980.f = Vallado TABLE D-6
		// A, B, C, and D coefficients are in 0.0001" per Julian century
		#region Nutation coefficients
		static NutCoeffs[] nutCoeffs = {
			new NutCoeffs( 0,  0,  0,  0, 1, -171996, -174.2, 92025,  8.9),
			new NutCoeffs( 0,  0,  0,  0, 2,    2062,    0.2,  -895,  0.5),
			new NutCoeffs(-2,  0,  2,  0, 1,      46,    0.0,   -24,  0.0),
			new NutCoeffs( 2,  0, -2,  0, 0,      11,    0.0,     0,  0.0),
			new NutCoeffs(-2,  0,  2,  0, 2,      -3,    0.0,     1,  0.0),
			new NutCoeffs( 1, -1,  0, -1, 0,      -3,    0.0,     0,  0.0),
			new NutCoeffs( 0, -2,  2, -2, 1,      -2,    0.0,     1,  0.0),
			new NutCoeffs( 2,  0, -2,  0, 1,       1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0,  2, -2, 2,  -13187,   -1.6,  5736, -3.1),
			new NutCoeffs( 0,  1,  0,  0, 0,    1426,   -3.4,    54, -0.1),

			new NutCoeffs( 0,  1,  2, -2, 2,    -517,    1.2,   224, -0.6),
			new NutCoeffs( 0, -1,  2, -2, 2,     217,   -0.5,   -95,  0.3),
			new NutCoeffs( 0,  0,  2, -2, 1,     129,    0.1,   -70,  0.0),
			new NutCoeffs( 2,  0,  0, -2, 0,      48,    0.0,     1,  0.0),
			new NutCoeffs( 0,  0,  2, -2, 0,     -22,    0.0,     0,  0.0),
			new NutCoeffs( 0,  2,  0,  0, 0,      17,   -0.1,     0,  0.0),
			new NutCoeffs( 0,  1,  0,  0, 1,     -15,    0.0,     9,  0.0),
			new NutCoeffs( 0,  2,  2, -2, 2,     -16,    0.1,     7,  0.0),
			new NutCoeffs( 0, -1,  0,  0, 1,     -12,    0.0,     6,  0.0),
			new NutCoeffs(-2,  0,  0,  2, 1,      -6,    0.0,     3,  0.0),

			new NutCoeffs( 0, -1,  2, -2, 1,      -5,    0.0,     3,  0.0),
			new NutCoeffs( 2,  0,  0, -2, 1,       4,    0.0,    -2,  0.0),
			new NutCoeffs( 0,  1,  2, -2, 1,       4,    0.0,    -2,  0.0),
			new NutCoeffs( 1,  0,  0, -1, 0,      -4,    0.0,     0,  0.0),
			new NutCoeffs( 2,  1,  0, -2, 0,       1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0, -2,  2, 1,       1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1, -2,  2, 0,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1,  0,  0, 2,       1,    0.0,     0,  0.0),
			new NutCoeffs(-1,  0,  0,  1, 1,       1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1,  2, -2, 0,      -1,    0.0,     0,  0.0),

			new NutCoeffs( 0,  0,  2,  0, 2,   -2274,   -0.2,   977, -0.5),
			new NutCoeffs( 1,  0,  0,  0, 0,     712,    0.1,    -7,  0.0),
			new NutCoeffs( 0,  0,  2,  0, 1,    -386,   -0.4,   200,  0.0),
			new NutCoeffs( 1,  0,  2,  0, 2,    -301,    0.0,   129, -0.1),
			new NutCoeffs( 1,  0,  0, -2, 0,    -158,    0.0,    -1,  0.0),
			new NutCoeffs(-1,  0,  2,  0, 2,     123,    0.0,   -53,  0.0),
			new NutCoeffs( 0,  0,  0,  2, 0,      63,    0.0,    -2,  0.0),
			new NutCoeffs( 1,  0,  0,  0, 1,      63,    0.1,   -33,  0.0),
			new NutCoeffs(-1,  0,  0,  0, 1,     -58,   -0.1,    32,  0.0),
			new NutCoeffs(-1,  0,  2,  2, 2,     -59,    0.0,    26,  0.0),

			new NutCoeffs( 1,  0,  2,  0, 1,     -51,    0.0,    27,  0.0),
			new NutCoeffs( 0,  0,  2,  2, 2,     -38,    0.0,    16,  0.0),
			new NutCoeffs( 2,  0,  0,  0, 0,      29,    0.0,    -1,  0.0),
			new NutCoeffs( 1,  0,  2, -2, 2,      29,    0.0,   -12,  0.0),
			new NutCoeffs( 2,  0,  2,  0, 2,     -31,    0.0,    13,  0.0),
			new NutCoeffs( 0,  0,  2,  0, 0,      26,    0.0,    -1,  0.0),
			new NutCoeffs(-1,  0,  2,  0, 1,      21,    0.0,   -10,  0.0),
			new NutCoeffs(-1,  0,  0,  2, 1,      16,    0.0,    -8,  0.0),
			new NutCoeffs( 1,  0,  0, -2, 1,     -13,    0.0,     7,  0.0),
			new NutCoeffs(-1,  0,  2,  2, 1,     -10,    0.0,     5,  0.0),

			new NutCoeffs( 1,  1,  0, -2, 0,      -7,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1,  2,  0, 2,       7,    0.0,    -3,  0.0),
			new NutCoeffs( 0, -1,  2,  0, 2,      -7,    0.0,     3,  0.0),
			new NutCoeffs( 1,  0,  2,  2, 2,      -8,    0.0,     3,  0.0),
			new NutCoeffs( 1,  0,  0,  2, 0,       6,    0.0,     0,  0.0),
			new NutCoeffs( 2,  0,  2, -2, 2,       6,    0.0,    -3,  0.0),
			new NutCoeffs( 0,  0,  0,  2, 1,      -6,    0.0,     3,  0.0),
			new NutCoeffs( 0,  0,  2,  2, 1,      -7,    0.0,     3,  0.0),
			new NutCoeffs( 1,  0,  2, -2, 1,       6,    0.0,    -3,  0.0),
			new NutCoeffs( 0,  0,  0, -2, 1,      -5,    0.0,     3,  0.0),

			new NutCoeffs( 1, -1,  0,  0, 0,       5,    0.0,     0,  0.0),
			new NutCoeffs( 2,  0,  2,  0, 1,      -5,    0.0,     3,  0.0),
			new NutCoeffs( 0,  1,  0, -2, 0,      -4,    0.0,     0,  0.0),
			new NutCoeffs( 1,  0, -2,  0, 0,       4,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0,  0,  1, 0,      -4,    0.0,     0,  0.0),
			new NutCoeffs( 1,  1,  0,  0, 0,      -3,    0.0,     0,  0.0),
			new NutCoeffs( 1,  0,  2,  0, 0,       3,    0.0,     0,  0.0),
			new NutCoeffs( 1, -1,  2,  0, 2,      -3,    0.0,     1,  0.0),
			new NutCoeffs(-1, -1,  2,  2, 2,      -3,    0.0,     1,  0.0),
			new NutCoeffs(-2,  0,  0,  0, 1,      -2,    0.0,     1,  0.0),

			new NutCoeffs( 3,  0,  2,  0, 2,      -3,    0.0,     1,  0.0),
			new NutCoeffs( 0, -1,  2,  2, 2,      -3,    0.0,     1,  0.0),
			new NutCoeffs( 1,  1,  2,  0, 2,       2,    0.0,    -1,  0.0),
			new NutCoeffs(-1,  0,  2, -2, 1,      -2,    0.0,     1,  0.0),
			new NutCoeffs( 2,  0,  0,  0, 1,       2,    0.0,    -1,  0.0),
			new NutCoeffs( 1,  0,  0,  0, 2,      -2,    0.0,     1,  0.0),
			new NutCoeffs( 3,  0,  0,  0, 0,       2,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0,  2,  1, 2,       2,    0.0,    -1,  0.0),
			new NutCoeffs(-1,  0,  0,  0, 2,       1,    0.0,    -1,  0.0),
			new NutCoeffs( 1,  0,  0, -4, 0,      -1,    0.0,     0,  0.0),

			new NutCoeffs(-2,  0,  2,  2, 2,       1,    0.0,    -1,  0.0),
			new NutCoeffs(-1,  0,  2,  4, 2,      -2,    0.0,     1,  0.0),
			new NutCoeffs( 2,  0,  0, -4, 0,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 1,  1,  2, -2, 2,       1,    0.0,    -1,  0.0),
			new NutCoeffs( 1,  0,  2,  2, 1,      -1,    0.0,     1,  0.0),
			new NutCoeffs(-2,  0,  2,  4, 2,      -1,    0.0,     1,  0.0),
			new NutCoeffs(-1,  0,  4,  0, 2,       1,    0.0,     0,  0.0),
			new NutCoeffs( 1, -1,  0, -2, 0,       1,    0.0,     0,  0.0),
			new NutCoeffs( 2,  0,  2, -2, 1,       1,    0.0,    -1,  0.0),
			new NutCoeffs( 2,  0,  2,  2, 2,      -1,    0.0,     0,  0.0),

			new NutCoeffs( 1,  0,  0,  2, 1,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0,  4, -2, 2,       1,    0.0,     0,  0.0),
			new NutCoeffs( 3,  0,  2, -2, 2,       1,    0.0,     0,  0.0),
			new NutCoeffs( 1,  0,  2, -2, 0,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1,  2,  0, 1,       1,    0.0,     0,  0.0),
			new NutCoeffs(-1, -1,  0,  2, 1,       1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0, -2,  0, 1,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0,  2, -1, 2,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1,  0,  2, 0,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 1,  0, -2, -2, 0,      -1,    0.0,     0,  0.0),

			new NutCoeffs( 0, -1,  2,  0, 1,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 1,  1,  0, -2, 1,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 1,  0, -2,  2, 0,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 2,  0,  0,  2, 0,       1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  0,  2,  4, 2,      -1,    0.0,     0,  0.0),
			new NutCoeffs( 0,  1,  0,  1, 0,       1,    0.0,     0,  0.0)
			};
		#endregion

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			Enabled = false;

			clock = scenario.Clock.GetService(typeof(AstroClock)) as AstroClock;
			base.Discover(scenario);

			var referenceHost = Parent as IReferenceFrameModelHost;
			if (referenceHost != null)
				referenceHost.ReferenceFrameModel = this;
		}

		#endregion

		#region IReferenceFrameModel Members

		public AstroClock Clock
		{
			set
			{
				clock = value;
			}
		}

		public IReferenceFrameModel Clone()
		{
			var clone = new Fk5EarthReference();
			clone.Enabled = Enabled;
			clone.EopFileName = EopFileName;
			clone.EopFormat = EopFormat;
			clone.Xp = Xp;
			clone.Yp = Yp;
			clone.dUT1 = dUT1;
			return clone;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="j2000CenturyTT"></param>
		/// <returns>Ecliptic angle [deg]</returns>
		public double EclipticAngle(double j2000CenturyTT)
		{
			return ObliquityOfTheEcliptic(j2000CenturyTT) * Constant.DegPerRad;
		}

		public double EclipticAngleDeg
		{
			get { return ObliquityOfTheEcliptic(clock.J2000CenturyTT) * Constant.DegPerRad; }
		}

		public Dcm FromInertial(Dcm dcm)
		{
			// GMST 1982
			double mjd = clock.ModifiedJulianDate;
			double date = (int)mjd;
			double mjdFrac = mjd - date;
			double time = mjdFrac + dUT1 / Constant.SecPerDay;
			double Tut1 = AstroClock.J2000CenturyFromMjd(date) + time / Constant.JulianDayPerCentury;
			double f = Constant.SecPerDay * time;
			gmst = Limit.FoldPi(
				((Cgmst[0] + (Cgmst[1] + (Cgmst[2] + Cgmst[3] * Tut1) * Tut1) * Tut1) + f) * (Constant.TwoPi / Constant.SecPerDay));
			gast = Limit.FoldTwoPi(gmst + equationOfTheEquinoxes);
			gastDcm.RotationFrom(3, gast);
			celestialTerrestrial = gastDcm * precessionNutation;
			dcm = polarMotionDcm * celestialTerrestrial;
			//gastDcm.Print("GAST", ",21:G18");
			//celestialTerrestrial.Print("Celectial-Terrestrial", ",21:G18");
			//dcm.Print("Celectial-Terrestrial w/ Polar Motion", ",21:G18");
			return dcm;
		}

		/// <summary>
		/// Initialize to the current time
		/// </summary>
		void IReferenceFrameModel.Initialize()
		{
			double
				epsilon,	// true obliquity of the ecliptic ( Eps = meanEps + dEps )
				dPsi,	// delta_psi, nutation in longitude
				Wm;		// mean lunar orbit lon. of ascend. node
			double mjdUTC = clock.ModifiedJulianDate;
			double mjdTT = mjdUTC + (clock.Clock.LeapSeconds + TAItoTT) * Constant.DayPerSec;

			epsilon = PrecessionNutationfromMjdTT(mjdTT, precession, nutation, out dPsi, out Wm);
			//epsilon = precessionNutationfromJdTT(clock.JulianDateTT, precession, nutation,
			//    out dPsi, out Wm);
			precessionNutation = nutation * precession;

			equationOfTheEquinoxes = EquationOfTheEquinoxes(epsilon, dPsi, Wm);

			EarthOrientationParameters(mjdUTC, out Xp, out Yp, out dUT1);
			double
				cosXp = Math.Cos(Xp), sinXp = Math.Sin(Xp),
				cosYp = Math.Cos(Yp), sinYp = Math.Sin(Yp);
			polarMotionDcm.Set(cosXp, sinXp * sinYp, sinXp * cosYp,
								 0, cosYp, -sinYp,
								-sinXp, cosXp * sinYp, cosXp * cosYp);
			//precession.Print("Precession", ",21:G18");
			//nutation.Print("Nutation", ",21:G18");
			//precessionNutation.Print("NPB", ",21:G18");
			//polarMotionDcm.Print("PolarMotion", ",21:G18");
		}

		#endregion

		#region Properties

		[Description("Celestial Terrestrial DCM, Celestial -> center of rotation")]
		public Dcm CestialTerrestrial { get { return celestialTerrestrial; } }

		[Description("UT1 - UTC, [s]")]
		public double DeltaUT1 { get { return dUT1; } }

		[XmlAttribute("eopFileName")]
		public string EopFileName { get; set; }

		public enum FileFormat { MjdXpYpDut1, Finals };

		[XmlAttribute("eopFormat"),DefaultValue(FileFormat.MjdXpYpDut1)]
		public FileFormat EopFormat { get { return eopFormat; } set { eopFormat = value; } }
		FileFormat eopFormat = FileFormat.MjdXpYpDut1;

		[Description("Equation of the equinoxes, GAST - GMST, [rad]")]
		public double EqnOfTheEquinones { get { return equationOfTheEquinoxes; } }

		[Description("Greenwich Apparent Sideral Time DCM, Mean of Data -> Terrestrial")]
		public Dcm GAST { get { return gastDcm; } }

		[Description("Greenwich Mean Sideral Time [rad]")]
		public double GreenwichMeanSideralTime { get { return gmst; } }

		[Description("Greenwich Apparent Sideral Time [rad]")]
		public double GreenwichApparentSideralTime { get { return gast; } }

		[Description("Nutation DCM, True of Date -> Mean of Date")]
		public Dcm Nutation { get { return nutation; } }

		[Description("Polar motion DCM, center of rotation -> 90 deg latitude")]
		public Dcm PolarMotion { get { return polarMotionDcm; } }

		[Description("Pole rotation, X, [arc-sec]")]
		public double PoleX { get { return Xp / RadPerArcSec; } }

		[Description("Pole rotation, Y, [arc-sec]")]
		public double PoleY { get { return Yp / RadPerArcSec; } }

		[Description("Precession DCM, Celestial -> True of Date")]
		public Dcm Precession { get { return precession; } }

		[Description("Precession/Nutation DCM, Celestial -> Mean of Date")]
		public Dcm PrecessionNutation { get { return precessionNutation; } }

		[XmlIgnore]
		public bool ReCalc { get { return false; } set { (this as IReferenceFrameModel).Initialize(); } }

		#endregion

		void GetXpYpDut1( TokenParser parser, string line, out double Xp, out double Yp, out double dUT1 )
		{
			switch (EopFormat)
			{
				case FileFormat.MjdXpYpDut1:
					Xp = parser.ReadDouble()*RadPerArcSec;
					Yp = parser.ReadDouble()*RadPerArcSec;
					dUT1 = parser.ReadDouble();
					break;
				case FileFormat.Finals:
					Xp = Double.Parse(line.Substring(18, 9))*RadPerArcSec;
					Yp = Double.Parse(line.Substring(37, 9))*RadPerArcSec;
					dUT1 = Double.Parse(line.Substring(58, 10));
					break;
				default:
					Xp = Yp = dUT1 = 0;
					break;
			}
		}

		void EarthOrientationParameters(double mjdUTC, out double Xp, out double Yp, out double dUT1)
		{
			Xp = 0;
			Yp = 0;
			dUT1 = clock.DeltaUT1;

			if (EopFileName != null)
			{
				int mjd = 0, mjdMatch = (int)mjdUTC;
				var parser = new TokenParser(' ');
				string path = ApplicationInfo.InstallDirectory+@"Data\"+EopFileName;
				try
				{
					var sr = new StreamReader(path);
					while (!sr.EndOfStream)
					{
						var line = sr.ReadLine();
						switch (EopFormat)
						{
							case FileFormat.MjdXpYpDut1:
								parser.Text = line;
								mjd = parser.ReadInt32();
								break;
							case FileFormat.Finals:
								mjd = Int32.Parse(line.Substring(6,6));
								break;
						}

						if (mjd == mjdMatch)
						{
							double Xp0, Xp1, Yp0, Yp1, dUT10, dUT11;
							GetXpYpDut1(parser, line, out Xp0, out Yp0, out dUT10);
							line = sr.ReadLine();
							parser.Text = line;
							GetXpYpDut1(parser, line, out Xp1, out Yp1, out dUT11);
							double frac = mjdUTC - mjdMatch;
							double OneMinusFrac = 1 - frac;

							Xp = OneMinusFrac*Xp0 + frac*Xp1;
							Yp = OneMinusFrac*Yp0 + frac*Yp1;
							dUT1 = OneMinusFrac*dUT10 + frac*dUT11;

							sr.Close();
							return;
						}
					}
					Write("{0} does not contain MJD {1}, defaulting Xp,Yp,dUT1.",
						EopFileName,mjdMatch);
					sr.Close();
					return;
				}
				catch (System.IO.FileNotFoundException)
				{
					Write("Unable to findFK5 Earth Reference file {0}. Defaulting Xp,Yp,dUT1.", path);
				}
			}
			if (!logged)
			{
				Write("EopFileName not specified, defaulting Xp,Yp,dUT1.");
				logged = true;
			}
		} static bool logged;

		/// <summary>
		/// Equation of the Equinoxes 
		/// </summary>
		/// <param name="epsilon"></param>
		/// <param name="dPsi"></param>
		/// <param name="Wm"></param>
		/// <returns>Equation of the Equinoxes (GAST-GMST) [rad]</returns>
		double EquationOfTheEquinoxes(double epsilon, double dPsi, double Wm)
		{
			return dPsi*Math.Cos(epsilon) + RadPerArcSec*(0.00264*Math.Sin(Wm) + 0.000063*Math.Sin(Wm+Wm));
		}

		double EquationOfTheEquinoxTerms(double Ttdb, out double dPsi, out double Wm )
		{
			// SunMoonDelaunayElements (add smallest first):
			double moonMeanAnomaly =   (Ttdb*(Ttdb*(Ttdb*Mm_s[3] + Mm_s[2]) + Mm_s[1]) + Mm_s[0])*RadPerArcSec +
				Math.IEEERemainder(Mm_s[4]*Ttdb,1)*Constant.TwoPi;
			double sunMeanAnomaly =    (Ttdb*(Ttdb*(Ttdb*Ms_s[3] + Ms_s[2]) + Ms_s[1]) + Ms_s[0])*RadPerArcSec +
				Math.IEEERemainder(Ms_s[4]*Ttdb, 1)*Constant.TwoPi;
			double moonMeanArgLat =    (Ttdb*(Ttdb*(Ttdb*uMm_s[3] + uMm_s[2]) + uMm_s[1]) + uMm_s[0])*RadPerArcSec +
				Math.IEEERemainder(uMm_s[4]*Ttdb, 1)*Constant.TwoPi;
			double sunMeanElongation = (Ttdb*(Ttdb*(Ttdb*Ds_s[3] + Ds_s[2]) + Ds_s[1]) + Ds_s[0])*RadPerArcSec +
				Math.IEEERemainder(Ds_s[4]*Ttdb, 1)*Constant.TwoPi;
			Wm =                       (Ttdb*(Ttdb*(Ttdb*Wm_s[3] + Wm_s[2]) + Wm_s[1]) + Wm_s[0])*RadPerArcSec +
				Math.IEEERemainder(Wm_s[4]*Ttdb, 1)*Constant.TwoPi;

			// bring angles to limit: < pi
			moonMeanAnomaly = Limit.FoldPi(moonMeanAnomaly);
			sunMeanAnomaly = Limit.FoldPi(sunMeanAnomaly);
			sunMeanElongation = Limit.FoldPi(sunMeanElongation);
			Wm = Limit.FoldPi(Wm);
			moonMeanArgLat = Limit.FoldPi(moonMeanArgLat);
			//Write("Mm:{0}, Ms:{1}, MALm:{2}, Ds:{3}, Wm:{4}", moonMeanAnomaly,
			//    sunMeanAnomaly, moonMeanArgLat, sunMeanElongation, Wm);

			double dEpsilon = NutationAnglesIau1980(Ttdb, 0, moonMeanAnomaly, sunMeanAnomaly,
				moonMeanArgLat, sunMeanElongation, Wm, out dPsi);
			//Write("dPsi, dEps: {0:F23},{1:F23}", dPsi, dEpsilon);
			return dEpsilon;
		}

		/// <summary>
		/// IAU 1980 Nutation terms, Vallado, p.217
		/// </summary>
		/// <param name="Ttdb">J2000 Julian centuries (TDB)</param>
		/// <param name="numTerms"></param>
		/// <param name="moonMeanAnomaly"></param>
		/// <param name="sunMeanAnomaly"></param>
		/// <param name="moonMeanArgLat"></param>
		/// <param name="sunMeanElongation"></param>
		/// <param name="Wm"></param>
		/// <param name="dPsi"></param>
		/// <returns>dEpsilon [rad]</returns>
		double NutationAnglesIau1980(double Ttdb, int numTerms, double moonMeanAnomaly,
			double sunMeanAnomaly, double moonMeanArgLat, double sunMeanElongation, double Wm, 
			out double dPsi)
		{
			int i;

			// 0.1 milli-arcsec
			double dPsiUArcSec = 0, dEpsilonUArcSec = 0;

			if (numTerms < 1 || numTerms > nutCoeffs.Length)
				numTerms = nutCoeffs.Length;

			// sum in reverse order to preserve accuracy
			for (i = numTerms - 1; i >= 0; i--)
			{
				//ap = c[i].a1*Mm + c[i].a2*Ms + c[i].a3*uMm + c[i].a4*Ds + c[i].a5*Wm ;
				double ap =
					nutCoeffs[i].a * moonMeanAnomaly +
					nutCoeffs[i].b * sunMeanAnomaly +
					nutCoeffs[i].c * moonMeanArgLat +
					nutCoeffs[i].d * sunMeanElongation +
					nutCoeffs[i].e * Wm;
				dPsiUArcSec += (nutCoeffs[i].A + nutCoeffs[i].B * Ttdb) * Math.Sin(ap);
				dEpsilonUArcSec += (nutCoeffs[i].C + nutCoeffs[i].D * Ttdb) * Math.Cos(ap);
			}

			//Nutation corrections wrt IAU 1976/1980 ( 0.1 milli arcsec)
			const double
				ddp80 = -55.0655 * 10,
				dde80 = -6.3580 * 10;

			dPsiUArcSec += ddp80;
			dEpsilonUArcSec += dde80;

			dPsi = dPsiUArcSec * 0.0001 * RadPerArcSec;
			return dEpsilonUArcSec * 0.0001 * RadPerArcSec;
		}

		/// <summary>
		/// FK5 frame to Mean of Date frame, Precession
		/// IAU 1976 Precession, Vallado, p220
		/// </summary>
		/// <param name="Ttdb"></param>
		/// <param name="dcm"></param>
		/// <returns>The DCM</returns>
		Dcm PrecessionIau1976(double Ttdb, Dcm dcm)
		{
			double tRadPerArcSec = Ttdb*RadPerArcSec;
			double xi    = tRadPerArcSec*(2306.2181 + Ttdb*(0.30188 + 0.017998*Ttdb));
			double theta = tRadPerArcSec*(2004.3109 - Ttdb*(0.42665 + 0.041833*Ttdb));
			double zeta  = tRadPerArcSec*(2306.2181 + Ttdb*(1.09468 + 0.018203*Ttdb));

			//Write("xi,zeta,theta:{0:G17},{1:G17},{2:G17}", xi, zeta, theta);
			double
				cX = Math.Cos(xi), sX = Math.Sin(xi),
				cT = Math.Cos(theta), sT = Math.Sin(theta),
				cZ = Math.Cos(zeta), sZ = Math.Sin(zeta);

			dcm.Set( cT*cZ*cX - sZ*sX, -sX*cT*cZ - sZ*cX, -sT*cZ,
						sZ*cT*cX + sX*cZ, -sZ*sX*cT + cZ*cX, -sT*sZ,
						sT*cX,    -sT*sX,                     cT);

			return dcm;
		}

		/// <summary>
		/// FK5 inertial frame to precessed, nutated Mean of Date. No GMST or Polar Motion yet.
		/// </summary>
		/// <param name="JdTT">Terestrial Time, JD</param>
		/// <param name="precession"></param>
		/// <param name="nutation"></param>
		/// <param name="Epsilon"></param>
		/// <param name="dPsi"></param>
		/// <param name="Wm"></param>
		/// <returns>true obliquity of the ecliptic ( Eps = meanEps + dEps )</returns>
		double PrecessionNutationfromJdTT(double JdTT,
			Dcm precession,    // FK5 -> MOD
			Dcm nutation,    // MOD -> TOD
			out double dPsi,    // delta_psi, nutation in longitude
			out double Wm)		// mean lunar orbit lon. of ascend. node
		{
			double mjdTT = JdTT - 2400000.5;
			return PrecessionNutationfromMjdTT(mjdTT, precession, nutation, out dPsi, out Wm);
		}
		
		/// <summary>
		/// FK5 inertial frame to precessed, nutated Mean of Date. No GMST or Polar Motion yet.
		/// </summary>
		/// <param name="mjdTT">Terestrial Time, MLD</param>
		/// <param name="precession"></param>
		/// <param name="nutation"></param>
		/// <param name="dPsi"></param>
		/// <param name="Wm"></param>
		/// <returns>true obliquity of the ecliptic ( Eps = meanEps + dEps )</returns>
		double PrecessionNutationfromMjdTT(double mjdTT,
			Dcm precession,    // FK5 -> MOD
			Dcm nutation,    // MOD -> TOD
			out double dPsi,    // delta_psi, nutation in longitude
			out double Wm)		// mean lunar orbit lon. of ascend. node
		{
			double mjdTdb = mjdTT;// Clock.BarycentricTime(mjdTT);
			double Ttdb = AstroClock.J2000CenturyFromMjd(mjdTdb);
			//Write("Ttdb:{0:G17}", Ttdb);

			// find precession DCM: FK5 -> MOD
			PrecessionIau1976(Ttdb, precession);

			// mean obliquity of the ecliptic
			double meanEpsilon = ObliquityOfTheEcliptic(Ttdb);

			// find nutation (1980 IAU) DCM: MOD -> TOD
			double dEpsilon = EquationOfTheEquinoxTerms(Ttdb, out dPsi, out Wm);

			NutationIau1980(dPsi, dEpsilon, meanEpsilon, nutation);
			return meanEpsilon;
		}

		/// <summary>
		/// Mean of Date frame to True of Date frame, Nutation
		/// </summary>
		/// <param name="dPsi"></param>
		/// <param name="dEpsilon"></param>
		/// <param name="dcm"></param>
		void NutationIau1980(double dPsi, double dEpsilon, double meanEpsilon, Dcm dcm)
		{
			double epsilon = meanEpsilon + dEpsilon;
			double
				cdp = Math.Cos(dPsi),
				sdp = Math.Sin(dPsi),
				ce = Math.Cos(epsilon),
				se = Math.Sin(epsilon),
				cme = Math.Cos(meanEpsilon),
				sme = Math.Sin(meanEpsilon);

			dcm.Set( cdp,    -sdp*cme,            -sdp*sme,
						sdp*ce,  se*sme + cdp*cme*ce, sme*ce*cdp - se*cme,
						se*sdp,  se*cdp*cme - sme*ce, se*sme*cdp + ce*cme);
		}

		/// <summary>
		/// Mean obliquity of the ecliptic, Meeus, p147, 22.2, Astronimical Almanac 1984
		/// </summary>
		/// <param name="Ttdb"></param>
		/// <returns>[radians]</returns>
		double ObliquityOfTheEcliptic(double Ttdb)
		{
			//double ooe = 23.4392911111111 - TT*(0.01300416666666667 + TT*(1.638888888888889e-7 - 5.04444444444444e-7*TT));
			double ooe = RadPerArcSec*(84381.448 - Ttdb*(46.8150 + Ttdb*(0.00059 - Ttdb*0.001813)));
			return Limit.FoldPi(ooe);
		}

	}

#if false
	static class DcmExtensions
	{
		static public void Print(this Dcm dcm, string label, string format)
		{
			Log.WriteLine("{0}:", label);
			format = "{0"+(Char.IsLetter(format, 0)?":":string.Empty)+format+'}';
			string s0 = string.Format(format, dcm[0, 0]);
			string s1 = string.Format(format, dcm[0, 1]);
			string s2 = string.Format(format, dcm[0, 2]);
			Log.WriteLine("{0} {1} {2}", s0, s1, s2);
			s0 = string.Format(format, dcm[1, 0]);
			s1 = string.Format(format, dcm[1, 1]);
			s2 = string.Format(format, dcm[1, 2]);
			Log.WriteLine("{0} {1} {2}", s0, s1, s2);
			s0 = string.Format(format, dcm[2, 0]);
			s1 = string.Format(format, dcm[2, 1]);
			s2 = string.Format(format, dcm[2, 2]);
			Log.WriteLine("{0} {1} {2}", s0, s1, s2);
		}
	}
#endif
}
