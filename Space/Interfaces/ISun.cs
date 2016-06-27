using Aspire.Primitives;

namespace Aspire.Space
{
	/// <summary>
	/// Interface to sun models
	/// </summary>
	public interface ISun
	{
		/// <summary>
		/// Position vector in inertial frame, [m]]
		/// </summary>
		Vector3 EciR { get; }
		/// <summary>
		/// Position unit vector in inertial frame]
		/// </summary>
		Vector3 EciRunit { get; }
		/// <summary>
		/// Position unit vector in EnvironmentContext's body frame
		/// </summary>
		Vector3 UnitBody { get; }
		/// <summary>
		/// Visible fraction. 0=occluded, 1=fully visible
		/// </summary>
		double Visible { get; }
		/// <summary>
		/// Alpha angle, [deg]. Corresponds to satellite time
		/// </summary>
		double Alpha { get; }
		/// <summary>
		/// Beta angle, [deg].  Equivalent to inclination with body's orbital plane.
		/// </summary>
		double Beta { get; }
		/// <summary>
		/// Equivalent alpha, expressed in a 24 hour clock hour [0-24 hours]
		/// </summary>
		double SatelliteTime { get; }
		/// <summary>
		/// Cos(alpha), useful for temperature profiles.
		/// </summary>
		double CosAlpha { get; }
	}
}
