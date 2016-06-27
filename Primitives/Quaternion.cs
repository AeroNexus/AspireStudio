using System;
using System.ComponentModel;
using Aspire.Framework;

namespace Aspire.Primitives
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public struct Quaternion : IArrayProxy
    {
		internal double q0, q1, q2, q3;
		const string where = "Quaternion";

		public Quaternion(double q0, double q1, double q2, double q3)
		{
			this.q0 = q0; this.q1 = q1; this.q2 = q2; this.q3 = q3;
		}

		public Quaternion(string csv)
		{
			var items = csv.Split(',');
			q0 = q1 = q2 = q3 = 0;
			if (items.Length > 0) q0 = double.Parse(items[0]);
			if (items.Length > 1) q1 = double.Parse(items[1]);
			if (items.Length > 2) q2 = double.Parse(items[2]);
			if (items.Length > 3) q3 = double.Parse(items[3]);
		}

		public void Conjugate(ref Quaternion rhs)
		{
			rhs.q0 = q0; rhs.q1 = q1; rhs.q2 = q2; rhs.q3 = -q3;
		}

		public void AngularRate(ref Vector3 angularRate, Quaternion previous, double dt)
		{
			if (dt == 0)
			{
				angularRate.Zero();
				return;
			}
			double inv2dt = 2.0 / dt;
			double q0Dot = (q0 - previous.q0) * inv2dt;
			double q1Dot = (q1 - previous.q1) * inv2dt;
			double q2Dot = (q2 - previous.q2) * inv2dt;
			double q3Dot = (q3 - previous.q3) * inv2dt;
			angularRate.X =  q3 * q0Dot + q2 * q1Dot - q1 * q2Dot - q0 * q3Dot;
			angularRate.Y = -q2 * q0Dot + q3 * q1Dot + q0 * q2Dot - q1 * q3Dot;
			angularRate.Z =  q1 * q0Dot - q0 * q1Dot + q3 * q2Dot - q2 * q3Dot;
		}

		#region Axis-angle

		/// <summary>
		/// Convert a Quaternion to an axis and a rotation angle about that axis.
		/// <P>The 'axis' parameter is modified to contain the axis unit vector,
		/// and the angle is the returned value.</P>
		/// <P>Assumes the Quaternion represents a rotation.</P>
		/// </summary>
		/// <param name="axis">The Vector3 to store the computed axis, as a unit vector.</param>
		/// <returns>The angle of rotation about the axis, [radians]</returns>
		public double ToAxisAngle(Vector3 axis)
		{
			axis.X = q0;
			axis.Y = q1;
			axis.Z = q2;
			double sinHalfAngle = axis.Magnitude;
			if (sinHalfAngle == 0)
				axis.X = 1;
			else
				axis.Normalize();
			// following preserves accuracy for cos(angle) near +/-1
			if (Math.Abs(q3) < Constant.HalfSqrt2) return 2.0 * Math.Acos(q3);
			return 2.0 * Math.Atan(sinHalfAngle / q3);
		}

		/// <summary>
		/// Convert a Quaternion to an axis and a rotation angle about that axis.
		/// <P>The 'axis' parameter is modified to contain the axis unit vector,
		/// and the smallest angle is the returned value.</P>
		/// <P>Assumes the Quaternion represents a rotation.</P>
		/// </summary>
		/// <param name="axis">The Vector3 to store the computed axis, as a unit vector.</param>
		/// <returns>The smallest angle of rotation about the axis, [radians]</returns>
		public double ToAxisSmallestAngle(Vector3 axis)
		{
			var angle = ToAxisAngle(axis);
			if (angle > Constant.Pi)
			{
				axis = -axis;
				angle = Constant.TwoPi - angle;
			}
			else if (angle < -Constant.Pi)
			{
				axis = -axis;
				angle = -Constant.TwoPi - angle;
			}
			return angle;
		}

		/// <summary>
		/// Set 'this' quaternion to represent a rotation by 'angle' radians about 'axis'.
		/// </summary>
		/// <param name="axis">The desired rotation axis.</param>
		/// <param name="angle">The desired rotation angle. [radians]</param>
		public void FromAxisAngle(Vector3 axis, double angle)
		{
			double halfAngle = 0.5 * angle;
			double sinHalfAngle = Math.Sin(halfAngle);
			double inverseScale = 1 / axis.Magnitude;
			q0 = sinHalfAngle * axis.X * inverseScale;
			q1 = sinHalfAngle * axis.Y * inverseScale;
			q2 = sinHalfAngle * axis.Z * inverseScale;
			q3 = Math.Cos(halfAngle);
		}

		/// <summary>
		/// Compute rotation of two unit vectors
		/// </summary>
		/// <param name="fromUnit"></param>
		/// <param name="toUnit"></param>
		/// <param name="axis"></param>
		public void FromUnitVectors(Vector3 fromUnit, Vector3 toUnit, Vector3 axis)
		{
			//// compute angle between the two vectors
			double cosPhi = fromUnit.Dot(toUnit);
			double phi = Math.Acos(cosPhi);

			//// take care of special cases
			if (phi == 0)
			{
				q0 = q1 = q3 = 0;
				q2 = 1;
				return;
			}
			if (phi == Constant.Pi || phi == -Constant.Pi)
			{
				q0 = 1;
				q1 = q2 = 0;
				q3 = 0;
				return;
			}

			//// compute axis of single rotation
			axis.CrossProduct(fromUnit, toUnit);
			//axis.Normalize() ;
			FromAxisAngle(axis, phi);
		}

		#endregion

		#region DCM

		public void FromDcm(Dcm dcm)
		{
			dcm.ToQuaternion(ref this);
		}

		/// <summary>
		/// Convert this Quaternion to a rotation DCM
		/// <P>Assumes this Quaternion represents a valid rotation.</P>
		/// </summary>
		/// <returns>The dcm</returns>
		public Dcm ToDcm()
		{
			var dcm = new Dcm();
			return ToDcm(ref dcm);
		}

		/// <summary>
		/// Convert this Quaternion to a rotation DCM
		/// <P>Assumes this Quaternion represents a valid rotation.</P>
		/// </summary>
		/// <param name="dcm">The resultant DCM Matrix3.</param>
		/// <returns>The dcm</returns>
		public Dcm ToDcm(ref Dcm dcm)
		{
			double temp0, temp1, temp2, temp3, temp4, temp5;

			// useful intermediate values
			temp0 = q0 * q0;
			temp1 = q1 * q1;
			temp2 = q2 * q2;
			temp3 = q3 * q3;

			// diagonal elements
			dcm.xx = temp0 - temp1 - temp2 + temp3;
			dcm.yy = -temp0 + temp1 - temp2 + temp3;
			dcm.zz = -temp0 - temp1 + temp2 + temp3;

			// more, useful intermediate values
			temp0 = q0 * q1;
			temp1 = q1 * q2;
			temp2 = q0 * q2;
			temp3 = q0 * q3;
			temp4 = q1 * q3;
			temp5 = q2 * q3;

			// Off-diagonal elements
			dcm.yx = 2 * (temp0 - temp5);
			dcm.zx = 2 * (temp2 + temp4);
			dcm.xy = 2 * (temp0 + temp5);
			dcm.zy = 2 * (temp1 - temp3);
			dcm.xz = 2 * (temp2 - temp4);
			dcm.yz = 2 * (temp1 + temp3);
			return dcm;
		}

		#endregion

		#region Euler

		// In all of the ToEuler() forms below, the valid axis sequences are:
		//   123, 132, 213, 231, 312, 321  and
		//   121, 131, 212, 232, 313, 323

		/// <summary>
		/// Convert this Quaternion to an Euler angle sequence as a Vector3
		/// <P>Returns a valid sequence, but be aware that Euler angles are not unique.</P>
		/// <P>Assumes this Quaternion represents a valid rotation.</P>
		/// </summary>
		/// <param name="euler">A Vector3 to store the three returned Euler angles in radians.</param>
		/// <param name="axis1">The first axis [1|2|3] to rotate about.</param>
		/// <param name="axis2">The second axis [1|2|3] to rotate about.</param>
		/// <param name="axis3">The third axis [1|2|3] to rotate about.</param>
		/// <returns>The Euler angles as a Vector3, in radians.</returns>
		public Vector3 ToEuler(Vector3 euler, int axis1, int axis2, int axis3)
		{
			if ((axis1 < 1) || (axis1 > 3) || (axis2 < 1) || (axis2 > 3) || (axis3 < 1) || (axis3 > 3))
			{
				throw new Exception(where + ".ToEuler() An axis value is invalid: " + axis1 + "," + axis2 + "," + axis3);
			}
			if ((axis1 * axis2 * axis3) == 6)
			{
				// Euler Rotation sequence of type1
				return ToEuler1(euler, axis1 - 1, axis2 - 1, axis3 - 1);
			}
			if ((axis1 == axis3) && (axis1 != axis2))
			{
				// Euler Rotation sequence of type 2
				return ToEuler2(euler, axis1 - 1, axis2 - 1);
			}
			throw new Exception(where + ".ToEuler() Invalid axis sequence: " + axis1 + "," + axis2 + "," + axis3);
		}

		/// <summary>
		/// Convert a Quaternion to an Euler angle sequence in a Vector3.
		/// <P>Returns a valid sequence, but be aware that Euler angles are not unique.</P>
		/// <P>Assumes this Quaternion represents a valid rotation.</P>
		/// </summary>
		/// <param name="quat">The quaternion as a Quaternion</param>
		/// <param name="euler">A Vector3 to store the three returned Euler angles in radians.</param>
		/// <param name="axis1">The first axis [1|2|3] to rotate about.</param>
		/// <param name="axis2">The second axis [1|2|3] to rotate about.</param>
		/// <param name="axis3">The second axis [1|2|3] to rotate about.</param>
		/// <returns>The Euler angles as a Vector3, in radians.</returns>
		public static Vector3 ToEuler(Quaternion quat, Vector3 euler, int axis1, int axis2, int axis3)
		{
			if ((axis1 < 1) || (axis1 > 3) || (axis2 < 1) || (axis2 > 3) || (axis3 < 1) || (axis3 > 3))
			{
				throw new Exception(where + ".ToEuler() An axis value is invalid: " + axis1 + "," + axis2 + "," + axis3);
			}
			if ((axis1 * axis2 * axis3) == 6)
			{
				// Euler Rotation sequence of type1
				return quat.ToEuler1(euler, axis1 - 1, axis2 - 1, axis3 - 1);
			}
			if ((axis1 == axis3) && (axis1 != axis2))
			{
				// Euler Rotation sequence of type 2
				return quat.ToEuler2(euler, axis1 - 1, axis2 - 1);
			}
			throw new Exception(where + ".ToEuler() Invalid axis sequence: " + axis1 + "," + axis2 + "," + axis3);
		}

		private Vector3 ToEuler1(Vector3 angles_out, int n0, int n1, int n2)
		{
			// equations used are from:
			// http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/Quaternions.pdf
			// equation set 39
			// but their final equation should read: tan(theta1 / 2) = p1/p0;
			double p0 = q3;
			double p1 = this[n0];
			double p2 = this[n1];
			double p3 = this[n2];
			int e = 1;
			if ((n0 == 0) && (n1 == 2))
			{         // "xzy" order
				e = -1;
			}
			else if ((n0 == 1) && (n1 == 0))
			{  // "yxz" order
				e = -1;
			}
			else if ((n0 == 2) && (n1 == 1))
			{  // "zyx" order
				e = -1;
			}
			angles_out.X = Math.Atan2(2 * (p0 * p1 - e * p2 * p3), (1 - 2 * (p1 * p1 + p2 * p2)));
			double temp = 2 * (p0 * p2 + e * p1 * p3);
			angles_out.Y = Math.Asin(Limit.Clamp(temp, 1));
			angles_out.Z = Math.Atan2(2 * (p0 * p3 - e * p1 * p2), (1 - 2 * (p2 * p2 + p3 * p3)));
			// special case when second angle is +/- 90 degrees
			if (Math.Abs(Math.Abs(angles_out.Y) - Math.PI / 2) < 1e-7)
			{
				angles_out.Z = 0;
				angles_out.X = 2 * Math.Atan2(p1, p0);
			}
			return angles_out;
		}

		// n0 and n1 are related to the axis numbers in a rotation of type "aba"
		// a=n0+1 and b=n1+1
		private Vector3 ToEuler2(Vector3 euler, int n0, int n1)
		{
			double denom1, denom2;
			int temp; // Represents axis which no rotation takes place about

			// Handle special cases where the general algorithm fials due
			// to "gimbal lock" in its processing steps.
			// In the special cases, all the rotation can be expressed about
			// the "a" axis.
			Vector3 axis = new Vector3();
			double angle = ToAxisAngle(axis);
			if (angle == 0) return euler.Zero();
			if ((n0 == 0) && (Math.Abs(axis[0]) == 1)) return euler.Set(Math.Sign(axis[0]) * angle, 0, 0);
			if ((n0 == 1) && (Math.Abs(axis[1]) == 1)) return euler.Set(Math.Sign(axis[1]) * angle, 0, 0);
			if ((n0 == 2) && (Math.Abs(axis[2]) == 1)) return euler.Set(Math.Sign(axis[2]) * angle, 0, 0);

			// general algorithm
			if ((n0 != 0) && (n1 != 0))
				temp = 0;
			else if ((n0 != 1) && (n1 != 1))
				temp = 1;
			else
				temp = 2;

			if ((n1 == n0 + 1) || ((n0 == 2) && (n1 == 0)))
			{
				denom1 = this[temp] * this[n0] + this[n1] * q3;
				denom2 = this[n0] * this[temp] - this[n1] * q3;

				// psi
				euler.Z = Math.Atan2(this[n1] * this[n0] - this[temp] * q3, denom1);

				// phi
				euler[0] = Math.Atan2(this[n0] * this[n1] + this[temp] * q3, -denom2);
			}
			else
			{
				denom1 = this[temp] * this[n0] - this[n1] * q3;

				denom2 = this[n0] * this[temp] + this[n1] * q3;

				// psi
				euler[2] = Math.Atan2(-(this[n1] * this[n0] + this[temp] * q3), denom1);
				// empirical fix for 3 cases: xzx, yxy, zyz
				if ((n1 == n0 - 1) || (n1 == n0 + 2)) euler[2] += Math.PI;

				// phi
				euler[0] = Math.Atan2(this[n0] * this[n1] - this[temp] * q3, denom2);
			}

			// theta
			double acos = this[n0] * this[n0] + q3 * q3 - this[n1] * this[n1] - this[temp] * this[temp];
			euler[1] = Math.Acos(Limit.Clamp(acos, 1.0));

			return euler;
		}

		/// <summary>
		/// Set 'this' quaternion to represent a euler rotation. Assumes 123 rotation sequence
		/// </summary>
		/// <param name="eulerAngle">Euler rotation angle. [radians]</param>
		public Quaternion FromEuler(Vector3 eulerAngle)
		{
			Dcm dcm = new Dcm();
			dcm.FromEuler(eulerAngle, "123");
			return Dcm.ToQuaternion(dcm, ref this);
		}

		#endregion

		public Quaternion Identity()
		{
			q0 = q1 = q2 = 0;
			q3 = 1;
			return this;
		}

		/// <summary>
		/// Normalize this quaternion, in situ.
		/// </summary>
		/// <returns>This Quaternion, normalized.</returns>
		public Quaternion Normalize()
		{
			double magnitude = Math.Sqrt(q0*q0 + q1*q1 + q2*q2 + q3*q3);
			q0 /= magnitude;
			q1 /= magnitude;
			q2 /= magnitude;
			q3 /= magnitude;
			return this;
		}

		public Quaternion Set(double x, double y, double z, double w)
		{
			q0 = x;
			q1 = y;
			q2 = z;
			q3 = w;
			return this;
		}

		public Quaternion Set(double[] array)
		{
			q0 = array[0];
			q1 = array[1];
			q2 = array[2];
			q3 = array[3];
			return this;
		}
		public override string ToString()
		{
			return string.Format("{0},{1},{2},{3}",q0, q1, q2, q3);
		}

		#region IArrayProxy Members

		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return q0;
					case 1: return q1;
					case 2: return q2;
					case 3: return q3;
				}
				throw new IndexOutOfRangeException(string.Format("Quaternion[{0}]: > 3", index));
			}
			set
			{
				switch (index)
				{
					case 0: q0 = value; break;
					case 1: q1 = value; break;
					case 2: q2 = value; break;
					case 3: q3 = value; break;
				}
			}
		}

		public object ConvertFrom(string csv) { return new Quaternion(csv); }

		public int GetUpperBound(int dimension) { return 3; }

		public int Length { get { return 4; } }

		public int Rank { get { return 1; } }

		public string Suffix(int index)
		{
			switch (index)
			{
				case 0: return "q0";
				case 1: return "q1";
				case 2: return "q2";
				case 3: return "q3";
			}
			throw new IndexOutOfRangeException(string.Format("Quaternion.Suffix({0}: > 3", index));
		}

		#endregion
	}
}
