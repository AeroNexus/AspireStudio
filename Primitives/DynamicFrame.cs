using System.ComponentModel;

using Aspire.Framework;

namespace Aspire.Primitives
{
	/// <summary>
	/// Dynamic frames implement relative kinematics and dynamics.
	/// </summary>
	public class DynamicFrame : Frame
	{
		/// <summary>
		/// Construct a frame with the given name
		/// </summary>
		/// <param name="name">The name for the new frame</param>
		public DynamicFrame(string name) : base(name) { }

		/// <summary>
		/// Construct a frame with the given name and frameList
		/// </summary>
		/// <param name="name">The name for the new frame</param>
		/// <param name="frames">The collection of frames to add to</param>
		public DynamicFrame(string name, FrameList frames) : base(name, frames) { }

		/// <summary>
		/// Angular rotational rate, [rad/s]
		/// </summary>
		[Description("Angular rotational rate,  [rad/s]")]
		[Blackboard]
		public Vector3 AngularRate
		{
			get { return angularRate; }
			set { angularRate = value; }
		} Vector3 angularRate;

		/// <summary>
		/// Rectilinear velocity, [m/s]
		/// </summary>
		[Description("Rectilinear velocity, measured wrt the parent frame [m/s]")]
		[Blackboard]
		public Vector3 RelativeVelocity
		{
			get { return relativeVelocity; }
			set { relativeVelocity = value; }
		} Vector3 relativeVelocity;

    }
}
