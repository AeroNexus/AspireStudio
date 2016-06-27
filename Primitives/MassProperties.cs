using System.ComponentModel;

namespace Aspire.Primitives
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class MassProperties
    {
		Matrix3 inertia = new Matrix3().Identity();
		Matrix3 invInertia = new Matrix3().Identity();
		double mass = 1, invMass = 1;

		public Matrix3 Inertia
		{
			get { return inertia; }
			set
			{
				inertia = value;
				//invInertia = inertia.Inverse();
			}
		}
		public Matrix3 InvInertia { get { return invInertia; } }
		public double InvMass { get { return invMass; } }
		public double Mass
		{
			get { return mass; }
			set
			{
				mass = value;
				invMass = 1.0 / mass;
			}
		}
	}
}
