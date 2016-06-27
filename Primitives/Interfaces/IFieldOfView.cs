using System;

namespace Aspire.Primitives
{
	public interface IFieldOfView
	{
		/// <summary>
		/// FovChanged is raised whenever the FOV's geometry changes
		/// </summary>
		event EventHandler FovChanged;
		/// <summary>
		/// Access the Field of view geometry
		/// </summary>
		FieldOfView FieldOfView { get; }
		/// <summary>
		/// Inertial boresight unit vector
		/// </summary>
		Vector3 FovBoresight { get; }
		/// <summary>
		/// Inertial position, [m]
		/// </summary>
		Vector3 FovPosition { get; }
		/// <summary>
		/// Inertial up unit vector, ALWAYS perpendicular to the boresight
		/// </summary>
		Vector3 FovUp { get; }
	}
}
