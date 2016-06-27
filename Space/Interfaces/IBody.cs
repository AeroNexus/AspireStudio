
namespace Aspire.Primitives
{
	/// <summary>
	/// The IBody interface allows models to get position and velocity of a body
	/// </summary>
	public interface IBody
	{
		/// <summary>
		/// Get The Earth Centered Inertial Position vector [m]
		/// </summary>
		Vector3 EciR { get; set; }
		/// <summary>
		/// Get The Earth Centered Inertial Position magnitude [m]
		/// </summary>
		double EciRmag { get; }
		/// <summary>
		/// Get The Earth Centered Inertial Position unit vector
		/// </summary>
		Vector3 EciRunit { get; }
		/// <summary>
		/// Get The Earth Centered Inertial Velocity vector [m/s]
		/// </summary>
		Vector3 EciV { get; set; }
		/// <summary>
		/// Get The Earth Centered Inertial Velocity magnitude [m/s]
		/// </summary>
		double EciVmag { get; }
		/// <summary>
		/// Get The Earth Centered Inertial Velocity unit vector
		/// </summary>
		Vector3 EciVunit { get; }
		/// <summary>
		/// Get The body rate vector
		/// </summary>
		Vector3 BodyRate { get; set; }

		MassProperties MassProperties { get; }

		void NormalizePosVel();
	}

}
