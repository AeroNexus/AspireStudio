
using Aspire.Framework;
using Aspire.Primitives;

namespace Aspire.Space
{
	/// <summary>
	/// Support for N-Body gravity modeling. Let individual gravity models assess pieces of the spacecraft context
	/// ans the central body they all in relationship with
	/// </summary>
	public interface I_NBodyGravity
	{
		/// <summary>
		/// Perturbing body's position wrt/ the inertial body [m]
		/// </summary>
		Vector3 InertialToPerturbR { get; }
		/// <summary>
		/// Owner body's position wrt/ perturbing body, [m]
		/// </summary>
		Vector3 PerturbToOwnerR { get; }
		/// <summary>
		/// Owner body's unit position wrt/ perturbing body, [m]
		/// </summary>
		Vector3 PerturbToOwnerRunit { get; }
		/// <summary>
		/// Position centered, fixed wrt/ central body (ECEF), [m]
		/// NOTE: only used for Earth Spherical Harmonics
		/// </summary>
		Vector3 CenteredFixedPosition { get; }
		/// <summary>
		/// Distance from perturbing body to the inertial body, [m]
		/// </summary>
		double InertialToPerturbRmag { get; }
		/// <summary>
		/// Distance from perturbing body to owner body, [m]
		/// </summary>
		double PerturbToOwnerRmag { get; }
	}
}
