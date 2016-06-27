using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Utilities;

namespace Aspire.Space
{
	public class EcefState
	{
		#region Declarations

		const string category = "Ecef";

		Dcm eciToEcefDcm;
		[Blackboard]
		DynamicFrame localFrame = new DynamicFrame("local");
		Frame earthBodyFrame = new Frame("ecf instance");
		Vector3 R = new Vector3(), prevR = new Vector3();
		Vector3 V = new Vector3();

		double dt = 0.125;
		double altitude, latitude, longitude;
		double earthRadius, geocentricAltitude, geocentricLatitude;
		[Blackboard(Units = "m", Description = "Local Earth radius of curvature")]
		double latitudeRad, longitudeRad;
		double radiusOfCurvature;
		double visualHorizon;
		double sinT, cosT, sinRA, cosRA;
		[Blackboard(Units = "deg", Description = "Distance along earth's surface to visual horizon")]
		double visualHorizonDeg;

		const double Requator = Constant.EarthEquatorialRadius;
		const double a = Constant.EarthEquatorialRadius;
		const double e2 = Constant.EarthEccentricity2;
		const double f = Constant.EarthFlattening;
		const double flattenSquared = (1 / (1 - Constant.EarthFlattening)) * (1 / (1 - Constant.EarthFlattening));

		#endregion

		/// <summary>
		/// Calculate the radius of the WGS-84 oblate spheroid earth as a function 
		/// of geodetic latitude.
		/// </summary>
		/// <param name="geodeticLatitude">Geodetic latitude, [rad]</param>
		/// <returns>Radius of the spheroid, [m]</returns>
		public static double CalcEarthRadius(double geodeticLatitude)
		{
			double sinLat = Math.Sin(geodeticLatitude);
			double cosLat = Math.Sqrt(1 - sinLat * sinLat);
			double roc = Requator / Math.Sqrt(1 - e2 * sinLat * sinLat);
			double gamma2 = (Constant.EarthPolarRadius * Constant.EarthPolarRadius) / (Constant.EarthEquatorialRadius * Constant.EarthEquatorialRadius);
			gamma2 *= gamma2;

			double radius2 = roc * roc * (cosLat * cosLat + gamma2 * sinLat * sinLat);

			double radius = Math.Sqrt(radius2);

			return radius;
		}

		/// <summary>
		/// Calculate ECEF parameters
		/// </summary>
		/// <param name="body">The IBody to perform the calculation for.</param>
		public void Calculate(IBody body)
		{
			if (!Enabled) return;

			Vector3 eciR = body.EciR;
			double eciRmag = body.EciRmag;

			CalculateLatLon(body);

			earthRadius = CalcEarthRadius(latitudeRad);

			geocentricAltitude = eciRmag - earthRadius;

			double eciRxy = Math.Sqrt(eciR[0] * eciR[0] + eciR[1] * eciR[1]);
			cosT = Limit.Clamp(eciR[2] / eciRmag, -1.0, 1.0);
			sinT = eciRxy / eciRmag;
			cosRA = eciR[0] / eciRxy;
			sinRA = eciR[1] / eciRxy;

			V = eciToEcefDcm * body.EciV;
			V[0] += R[1] * Constant.EarthRate;
			V[1] -= R[0] * Constant.EarthRate;

			//		ecf.coelevation = 	Math.Acos(cosT);
			if (eciRmag >= earthRadius)
			{
				double horizon = Math.Acos(earthRadius / eciRmag);
				visualHorizon = horizon * earthRadius;
				visualHorizonDeg = horizon * Constant.DegPerRad;
			}
			else
				visualHorizonDeg = visualHorizon = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="body"></param>
		public void CalculateLatLon(IBody body)
		{
			double gclat = Math.Asin(body.EciR[2] / body.EciRmag);
			geocentricLatitude = gclat * Constant.DegPerRad;

			if (environCtx == null || environCtx.Earth == null)
				eciToEcefDcm = EarthInertialToFixedDcm(0);
			else
				eciToEcefDcm = environCtx.Earth.GetEarth().InertialToBodyDcm;

			R = eciToEcefDcm * body.EciR;
			localFrame.Location = R;

			if (gclat == Constant.HalfPi)
			{
				latitudeRad = Constant.HalfPi;
				longitudeRad = Math.Atan2(R[1], R[0]);
			}
			else if (gclat == -Constant.HalfPi)
			{
				latitudeRad = -Constant.HalfPi;
				longitudeRad = Math.Atan2(R[1], R[0]);
			}
			else
			{
				// _latitudeRad = Math.Atan( flattenSquared*Math.Tan(gclat) );
				EcefToGeodeticLatLonAlt(R, out latitudeRad, out longitudeRad, out altitude, out radiusOfCurvature);
			}
			latitude = latitudeRad * Constant.DegPerRad;

			longitude = longitudeRad * Constant.DegPerRad;
		}

		/// <summary>
		/// Returns the radius of curvature in the prime vertical
		/// </summary>
		/// <param name="geodeticLatitudeRadians"></param>
		/// <returns></returns>
		public static double CalcRadiusOfCurvatureInPrimeVertical(double geodeticLatitudeRadians)
		{
			double sinLat = Math.Sin(geodeticLatitudeRadians);

			double N = Constant.EarthEquatorialRadius / Math.Sqrt(1.0 - e2 * sinLat * sinLat);

			return N;
		}

		/// <summary>
		/// Access the ECI to ECEF Direction Cosine Matrix
		/// </summary>
		private Dcm EarthInertialToFixedDcm(double greenwichHourAngle)
		{
			earthBodyFrame.FromParentDcm.RotationFrom(3, greenwichHourAngle);
			return earthBodyFrame.FromParentDcm;
		}

		/// <summary>
		/// This is an approximation with results that are extremely close to actual values. See below.
		/// 
		/// Method given by:
		/// Bowring, B.R. (1985). "The accuracy of geodetic latitude and height 
		/// equations," Survey Review.  28 Oct., pp. 202-206.
		/// 
		/// The accuracy of this method is discussed in the paper
		/// "A Comparison of Methods Used in Rectangular to Geodetic Coordinate Transformations"
		/// Burtch, Robert, presented at the ACSM 2006 Annual Conference and Technology Exhibition 
		/// http://www.ferris.edu/faculty/burtchr/papers 
		/// </summary>
		/// <param name="rEcef">ECEF position[m]</param>
		/// <param name="geodeticLatitude">geodetic latitude [rad], accurate to 1e-7 seconds of latitude</param>
		/// <param name="longitude">longitude [rad]</param>
		/// <param name="geodeticHeight">height above the ellipsoid [m], accurate to 1e-9 meters</param>
		/// <param name="radiusOfCurvature">radius of curvature in the prime vertical</param>
		public static void EcefToGeodeticLatLonAlt(Vector3 rEcef,
			out double geodeticLatitude, out double longitude, out double geodeticHeight, out double radiusOfCurvature)
		{
			double rXY;
			double sinLat = SinGeodeticLatitude(rEcef, out rXY);

			double cosLat = Math.Sqrt(1.0 - sinLat * sinLat);

			double h = rXY * cosLat + rEcef.Z * sinLat - Constant.EarthEquatorialRadius * Math.Sqrt(1.0 - e2 * sinLat * sinLat);

			geodeticLatitude = Math.Asin(sinLat);
			longitude = Math.Atan2(rEcef.Y, rEcef.X);
			geodeticHeight = h;

			// Need to optimize with height calculation
			radiusOfCurvature = CalcRadiusOfCurvatureInPrimeVertical(geodeticLatitude);
		}

		/// <summary>
		/// Convert Geodetic Latitude, Longitude, Altitude to an exact ECEF position.
		/// </summary>
		/// <param name="latitude">Geodetic latitude, [deg].</param>
		/// <param name="longitude">Longitude, [deg].</param>
		/// <param name="altitude">Altitude above the ellipsoid [MSL, m].</param>
		/// <param name="ecefPosition">ECEF position <see cref="Vector3"/>.</param>
		/// <returns>The ECEF position <see cref="Vector3"/>.</returns>
		public static Vector3 LatLonAltToEcef(double latitude, double longitude, double altitude, Vector3 ecefPosition)
		{
			double lat = latitude * Constant.RadPerDeg;
			double sinLat = Math.Sin(lat), cosLat = Math.Cos(lat);
			double lon = longitude * Constant.RadPerDeg;
			double sinLon = Math.Sin(lon), cosLon = Math.Cos(lon);

			// this is from "Fundamentals of Astrodynamics and Applications, Second Ed., p. 150, Vallado"
			// and can also be found in the Astronomical Almanac in Appendix K, although
			// the Almanac presents the equation without the simplifying trigonometric identity
			// found in "Fundamentals"
			//
			double radiusOfCurvature = CalcRadiusOfCurvatureInPrimeVertical(lat);

			double radius = radiusOfCurvature + altitude;
			ecefPosition[0] = radius * cosLat * cosLon;
			ecefPosition[1] = radius * cosLat * sinLon;
			ecefPosition[2] = (radiusOfCurvature * (1 - e2) + altitude) * sinLat;

			return ecefPosition;
		}

		/// <summary>
		/// Set the Ecf state using Geodetic Latitude, Longitude, Altitude
		/// </summary>
		/// <param name="latitude">Geodetic latitude, [deg].</param>
		/// <param name="longitude">Longitude, [deg].</param>
		/// <param name="altitude">Altitude above the ellipsoid [MSL, m].</param>
		/// <param name="dt">Time change from last invocation, [sec].</param>
		public void SetLatLonAlt(double latitude, double longitude, double altitude, double dt=0)
		{
			LatLonAltToEcef(latitude, longitude, altitude, R);

			if (dt > 0)
			{
				V.X = (R.X - prevR.X) / dt;
				V.Y = (R.Y - prevR.Y) / dt;
				V.Z = (R.Z - prevR.Z) / dt;

				prevR = R;
			}

			this.latitude = latitude;
			this.longitude = longitude;
			this.altitude = altitude;

			double Rmag = R.Magnitude;
			geocentricAltitude = Rmag - Requator;
			double lat = latitude * Constant.RadPerDeg;
			radiusOfCurvature = CalcRadiusOfCurvatureInPrimeVertical(lat);
			earthRadius = CalcEarthRadius(lat);
			if (latitude == 90)
				geocentricLatitude = 90;
			else if (latitude == -90)
				geocentricLatitude = -90;
			else
				geocentricLatitude = Math.Atan(Math.Tan(lat) / flattenSquared) * Constant.DegPerRad;

			if (Rmag > earthRadius)
			{
				// horizon is angle, measured at the center of the earth from
				// the object to the horizon seen by the object
				double horizon = Math.Acos(earthRadius / Rmag);
				visualHorizon = horizon * earthRadius;
				visualHorizonDeg = horizon * Constant.DegPerRad;
			}
			else
				visualHorizonDeg = visualHorizon = 0;
		}

		public void SetPosition(Vector3 ecefPosition)
		{
			if (dt > 0)
			{
				V.X = (ecefPosition.X - R.X) / dt;
				V.Y = (ecefPosition.Y - R.Y) / dt;
				V.Z = (ecefPosition.Z - R.Z) / dt;
			}

			R = ecefPosition;
			localFrame.Location = R;
			double Rmag = R.Magnitude;
			double gclat = Math.Asin(R.Z / Rmag), lat, lon;
			if (Rmag == 0) gclat = 0;
			geocentricLatitude = gclat * Constant.DegPerRad;
			if (gclat == Constant.HalfPi)
			{
				lat = Constant.HalfPi;
				lon = Math.Atan2(R.Y, R.X);
			}
			else if (gclat == -Constant.HalfPi)
			{
				lat = -Constant.HalfPi;
				lon = Math.Atan2(R.Y, R.X);
			}
			else
				EcefToGeodeticLatLonAlt(R, out lat, out lon, out altitude, out radiusOfCurvature);

			latitude = lat * Constant.DegPerRad;
			longitude = lon * Constant.DegPerRad;
			geocentricAltitude = Rmag - Requator;
			earthRadius = CalcEarthRadius(lat);

			if (Rmag > earthRadius)
			{
				// horizon is angle, measured at the center of the earth from
				// the object to the horizon to the horizon seen by the object
				double horizon = Math.Acos(earthRadius / Rmag);
				visualHorizon = horizon * earthRadius;
				visualHorizonDeg = horizon * Constant.DegPerRad;
			}
			else
				visualHorizonDeg = visualHorizon = 0;
		}

		/// <summary>
		/// Calculates geodetic latitude at an ECEF position
		/// </summary>
		/// <param name="rEcef">ECEF position [m]</param>
		/// <param name="rXY">Radius to the ellipsoid surface in the XY plane, [m]</param>
		/// <returns>sin(geodetic latitude)</returns>
		public static double SinGeodeticLatitude(Vector3 rEcef, out double rXY)
		{
			double p2 = rEcef.X * rEcef.X + rEcef.Y * rEcef.Y;
			rXY = Math.Sqrt(p2);

			double r = Math.Sqrt(p2 + rEcef.Z * rEcef.Z);
			if (r == 0) return 0;

			// divide by zero is allowed here, because atan() handles infinities properly
			double u = Math.Atan((rEcef.Z / rXY) * ((1.0 - f) + e2 * a / r));

			double sinU = Math.Sin(u);
			double sinU3 = sinU * sinU * sinU;

			// using the identity seems to be more stable, in addition to being faster
			double cosU = Math.Sqrt(1.0 - sinU * sinU);
			double cosU3 = cosU * cosU * cosU;

			double Z = rEcef.Z * (1.0 - f) + e2 * a * sinU3;
			double XY = (1.0 - f) * (rXY - e2 * a * cosU3);

			return Z / Math.Sqrt(Z * Z + XY * XY);
		}

		#region Properties

		public bool Enabled { get; set; }

		/// <summary>
		/// The environment context, typically used for Earth 
		/// </summary>
		public EnvironmentContext EnvironmentContext
		{
			get { return environCtx; }
			set
			{
				environCtx = value;
				if (environCtx == null)
				{
					Log.WriteLine("Ecef.setEnvironmentContext", Log.Severity.Warning,
						"null Environment Context");
					return;
				}
				if (environCtx.Earth != null)
				{
					environCtx.Earth.Ecef = this;
					if ((environCtx.Earth as EnvironmentModelContext).Owner is IBody)
						(environCtx.Earth as Model).Enabled = true;

					localFrame.Parent = environCtx.Earth.EarthBodyFrame;
				}
			}

		} EnvironmentContext environCtx;

		public double Altitude { get { return altitude; } }
		public double CosRA { get { return cosRA; } }
		public double CosT { get { return cosT; } }
		public double LatitudeRad { get { return latitudeRad; } }
		public double LongitudeRad { get { return longitudeRad; } }
		public double SinRA { get { return sinRA; } }
		public double SinT { get { return sinT; } }

		/// <summary>
		/// Read access to geodetic latitude, [deg]
		/// </summary>
		[Description("ECEF geodetic latitude")]
		[Blackboard(Units = "deg")]
		public double Latitude { get { return latitude; } }

		/// <summary>
		/// Read access to longitude, [deg]
		/// </summary>
		[Description("ECEF longitude")]
		[Blackboard(Units = "deg")]
		public double Longitude { get { return longitude; } }

		/// <summary>
		/// Position of entity
		/// </summary>
		[Blackboard(Units = "m"), Category(category), XmlIgnore]
		[Description("ECEF position vector [m]")]
		public Vector3 Position
		{
			get { return R; }
			set { R = value; }
		}

		/// <summary>
		/// Velocity of entity
		/// </summary>
		[Blackboard(Units = "m/s"), Category(category), XmlIgnore]
		[Description("ECEF velocity vector [m/s]")]
		public Vector3 Velocity
		{
			get { return V; }
			set { V = value; }
		}

		#endregion

		/// <summary>
		/// Convert internal ECEF velocity to ECI velocity
		/// </summary>
		/// <param name="eciV">Resultant ECI velocity, [m/s]</param>
		public Vector3 ToEciVelocity
		{
			get
			{
				var eciV = V;
				eciV[0] -= R[1] * Constant.EarthRate;
				eciV[1] += R[0] * Constant.EarthRate;
				Dcm ecefToEci = environCtx.Earth.GetEarth().BodyToInertialDcm;
				eciV = ecefToEci * eciV;
				return eciV;
			}
		}

	}
}
