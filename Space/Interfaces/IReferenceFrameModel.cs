using Aspire.Framework;
using Aspire.Primitives;

namespace Aspire.Space
{
	/// <summary>
	/// An implementor computes the body frame of a celestial body wrt/ an inertial frame
	/// Earth fixed model, as in J2000, IAU2000, FK5
	/// </summary>
	public interface IReferenceFrameModel
	{
		/// <summary>
		/// The astro-system clock
		/// </summary>
		AstroClock Clock { set; }
		/// <summary>
		/// Clone the current ReferenceFrameModel
		/// </summary>
		/// <returns>A clone of the current ReferenceFrameModel</returns>
		IReferenceFrameModel Clone();
		/// <summary>
		/// Angle of the ecliptic wrt/ equatorial plane
		/// </summary>
		/// <returns></returns>
		double EclipticAngle(double j2000CenturyTT);
		/// <summary>
		/// Angle of the ecliptic wrt/ equatorial plane
		/// </summary>
		/// <returns></returns>
		double EclipticAngleDeg { get; }
		/// <summary>
		/// Calculate the Inertial frame to ECEF transform DCM
		/// </summary>
		/// <param name="dcm">The DCM to rotate</param>
		/// <returns>The DCM</returns>
		Dcm FromInertial(Dcm dcm);
		/// <summary>
		/// Initialize to the current time
		/// </summary>
		void Initialize();
	}

	/// <summary>
	/// Hosts a IReferenceFrameModel
	/// </summary>
	public interface IReferenceFrameModelHost
	{
		/// <summary>
		/// Allows a ReferenceFrameModel to be assigned to the host
		/// </summary>
		IReferenceFrameModel ReferenceFrameModel { set; }
	}

}
