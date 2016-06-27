
namespace Aspire.Primitives
{
	/// <summary>
	/// The interface to all magnetics Models
	/// </summary>
	public interface IMagnetics
	{
		/// <summary>
		/// Access BField in Body frame vector, [nTesla]
		/// </summary>
		/// <returns></returns>
		Vector3 BodyBField { get; }

		/// <summary>
		/// Access BField in inertial frame, [nTesla]
		/// </summary>
		/// <returns></returns>
		Vector3 BField { get; }
	}
}
