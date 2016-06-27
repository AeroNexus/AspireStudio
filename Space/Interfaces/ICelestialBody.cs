
using Aspire.Primitives;

namespace Aspire.Space
{
	/// <summary>
	/// An implementor behaves like a celestial body
	/// </summary>
	public interface ICelestialBody
	{
		/// <summary>
		/// Transformation DCM: celestial body to inertial
		/// </summary>
		Dcm BodyToInertialDcm { get; }

		/// <summary>
		/// Transformation DCM: inertial to celestial body
		/// </summary>
		Dcm InertialToBodyDcm { get; }

		/// <summary>
		/// Mass of the body in kg
		/// </summary>
		double Mass { get; }

		/// <summary>
		/// Inertial position in meters
		/// </summary>
		Vector3 Position { get; }

		/// <summary>
		/// Radius at the poles
		/// </summary>
		double PolarRadius { get; }

		/// <summary>
		/// Visible radius in meters
		/// </summary>
		double Radius { get; }

		/// <summary>
		/// Inertial position in meters
		/// </summary>
		Vector3 Velocity { get; }
	}

}
