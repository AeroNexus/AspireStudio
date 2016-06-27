using System;
using System.ComponentModel;
using Aspire.Framework;

namespace Aspire.Primitives
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public struct Dcm : IArrayProxy
    {
		internal double xx, xy, xz, yx, yy, yz, zx, zy, zz;

		static Dcm temp1 = new Dcm();

		public Dcm(params double[] values)
		{
			xx = xy = xz = yx = yy = yz = zx = zy = zz = 0;

			for (int i = 0; i < values.Length; i++)
				switch (i)
				{
					case 0: xx = values[i]; break;
					case 1: xy = values[i]; break;
					case 2: xz = values[i]; break;
					case 3: yx = values[i]; break;
					case 4: yy = values[i]; break;
					case 5: yz = values[i]; break;
					case 6: zx = values[i]; break;
					case 7: zy = values[i]; break;
					case 8: zz = values[i]; break;
				}
		}

		public Dcm(string csv)
		{
			var items = csv.Split(','); // need to look for semi-colons for rank=2

			xx = xy = xz = yx = yy = yz = zx = zy = zz = 0;

			for (int i = 0; i < items.Length; i++)
				switch (i)
				{
					case 0: xx = double.Parse(items[i]); break;
					case 1: xy = double.Parse(items[i]); break;
					case 2: xz = double.Parse(items[i]); break;
					case 3: yx = double.Parse(items[i]); break;
					case 4: yy = double.Parse(items[i]); break;
					case 5: yz = double.Parse(items[i]); break;
					case 6: zx = double.Parse(items[i]); break;
					case 7: zy = double.Parse(items[i]); break;
					case 8: zz = double.Parse(items[i]); break;
				}
		}

		/// <summary>
		/// Multiply lhs * rhs to a temporary
		/// </summary>
		/// <param name="lhs">The LHS Dcm</param>
		/// <param name="rhs">The RHS Dcm</param>
		/// <returns>The temporary result</returns>
		public static Dcm operator *(Dcm lhs, Dcm rhs)
		{
			var result = new Dcm();

			result.xx = lhs.xx * rhs.xx + lhs.xy * rhs.yx + lhs.xz * rhs.zx;
			result.xy = lhs.xx * rhs.xy + lhs.xy * rhs.yy + lhs.xz * rhs.zy;
			result.xz = lhs.xx * rhs.xz + lhs.xy * rhs.yz + lhs.xz * rhs.zz;
			result.yx = lhs.yx * rhs.xx + lhs.yy * rhs.yx + lhs.yz * rhs.zx;
			result.yy = lhs.yx * rhs.xy + lhs.yy * rhs.yy + lhs.yz * rhs.zy;
			result.yz = lhs.yx * rhs.xz + lhs.yy * rhs.yz + lhs.yz * rhs.zz;
			result.zx = lhs.zx * rhs.xx + lhs.zy * rhs.yx + lhs.zz * rhs.zx;
			result.zy = lhs.zx * rhs.xy + lhs.zy * rhs.yy + lhs.zz * rhs.zy;
			result.zz = lhs.zx * rhs.xz + lhs.zy * rhs.yz + lhs.zz * rhs.zz;

			return result;
		}

		/// <summary>
		/// Multiply a Dcm by a Vector3 (dcm[3,3]*vec[1,3])
		/// </summary>
		/// <param name="lhs">The Dcm LHS</param>
		/// <param name="rhs">The Vector3 RHS</param>
		/// <returns>A temporary Vector = lhs*rhs</returns>
		public static Vector3 operator *(Dcm lhs, Vector3 rhs)
		{
			return new Vector3(
				lhs.xx * rhs.X + lhs.xy * rhs.Y + lhs.xz * rhs.Z,
				lhs.yx * rhs.X + lhs.yy * rhs.Y + lhs.yz * rhs.Z,
				lhs.zx * rhs.X + lhs.zy * rhs.Y + lhs.zz * rhs.Z);
		}

		#region Euler

		static Quaternion tempQ = new Quaternion();

		/// <summary>
		/// Calculate an Euler rotation from this Dcm
		/// </summary>
		/// <param name="euler">The resultant euler sequence [radians]</param>
		/// <param name="axis1">Which axis to rotate about first [1|2|3]</param>
		/// <param name="axis2">Which axis to rotate about second [1|2|3]</param>
		/// <param name="axis3">Which axis to rotate about third [1|2|3]</param>
		/// <returns>The Euler rotation Vector3</returns>
		public Vector3 ToEuler(Vector3 euler, int axis1, int axis2, int axis3)
		{
			ToQuaternion(this, ref tempQ);
			return tempQ.ToEuler(euler, axis1, axis2, axis3);
		}

		/// <summary>
		/// Calculate an Euler rotation sequence from this Dcm
		/// </summary>
		/// <param name="euler">The resultant euler sequence [radians]</param>
		/// <param name="seq">The rotation sequence "123", "321", "xyz", "ZYX", etc.</param>
		/// <returns>The Euler rotation Vector2</returns>
		public Vector3 ToEuler(Vector3 euler, string seq)
		{
			if (seq.Length != 3) throw new ArgumentException("Dcm.ToEuler: The rotation sequence must be exactly 3 characters long.  Requested sequence: " + seq);
			int[] axis = new int[3];
			for (int i = 0; i < seq.Length; i++)
			{
				switch (seq[i])
				{
					case '1':
					case 'x':
					case 'X':
						axis[i] = 1;
						break;
					case '2':
					case 'y':
					case 'Y':
						axis[i] = 2;
						break;
					case '3':
					case 'z':
					case 'Z':
						axis[i] = 3;
						break;
					default:
						throw new Exception("Bad angle sequence (" + seq + ") in Dcm.ToEuler()");
				}
			}
			ToQuaternion(this, ref tempQ);
			return tempQ.ToEuler(euler, axis[0], axis[1], axis[2]);
		}

		/// <summary>
		/// Build a Dcm from euler rotations. Initialized to Identity first.
		/// </summary>
		/// <param name="euler">A Vector3 containing 3 rotation angles [radians]</param>
		/// <param name="seq">
		/// A string containing 1, 2 or 3 characters [1|2|3] in any order. 1=rotate about the x-axis,
		/// 2=rotate about y-axis and 3=rotate about z-axis.
		/// </param>
		/// <returns>This Dcm resultant from the Euler rotations</returns>
		/// <example>
		/// <code>
		/// Dcm = new Dcm();
		/// Vector3 euler = new Vector3(0.1, 0.2, 0.3);
		/// dcm.FromEuler( euler, "1");
		/// dcm.FromEuler( euler, "12");
		/// dcm.FromEuler( euler, "321");
		/// </code>
		/// </example>
		public Dcm FromEuler(Vector3 euler, string seq)
		{
			Identity();
			RotationFrom(seq, euler);
			return this;
		}

		#endregion

		public Dcm Identity()
		{
			xx = yy = zz = 1;
			xy = xz = yx = yz = zx = zy = 0;

			return this;
		}

		public void Set(params double[] values)
		{
			xx = xy = xz = yx = yy = yz = zx = zy = zz = 0;

			for (int i = 0; i < values.Length; i++)
				switch (i)
				{
					case 0: xx = values[i]; break;
					case 1: xy = values[i]; break;
					case 2: xz = values[i]; break;
					case 3: yx = values[i]; break;
					case 4: yy = values[i]; break;
					case 5: yz = values[i]; break;
					case 6: zx = values[i]; break;
					case 7: zy = values[i]; break;
					case 8: zz = values[i]; break;
				}
		}

		public void SetColumn(int col, Vector3 values)
		{
			switch (col)
			{
				case 0: xx = values.X; yx = values.Y; zx = values.Z; break;
				case 1: xy = values.X; yy = values.Y; zy = values.Z; break;
				case 2: xz = values.X; yz = values.Y; zz = values.Z; break;
			}
		}

		public void SetRow(int row, Vector3 values)
		{
			switch (row)
			{
				case 0: xx = values.X; xy = values.Y; xz = values.Z; break;
				case 1: yx = values.X; yy = values.Y; yz = values.Z; break;
				case 2: zx = values.X; zy = values.Y; zz = values.Z; break;
			}
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2};{3},{4},{5};{6},{7},{8}", xx, xy, xz, yx, yy, yz, zx, zy, zz);
		}

		public void FromQuaternon(Quaternion q)
		{
			q.ToDcm(ref this);
		}

		/// <summary>
		/// Generalize rotate with discrete parameters
		/// </summary>
		/// <param name="axis">The orthogonal axis (1,2,3) to rotate about</param>
		/// <param name="angle">The angle to rotate through, [radians]</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotateFrom(int axis, double angle)
		{
			temp1.Identity();
			temp1.RotationFrom(axis, angle);
			this *= temp1;
			return this;
		}

		/// <summary>
		/// Calculate the rotation DCM in 'seq' order using the double[] angle. Rotates the frame
		/// </summary>
		/// <param name="seq">Each character of the string is a rotation axis. An axis may be 1,2 or 3 or x,y or z. The sequence may be any arbitrary length and order. o can be used to skip an angle.</param>
		/// <param name="angle"> Each angle value [radians[ must match the corresponding axis character in sequence</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotateTo(string seq, params double[] angle)
		{
			if (seq == null || seq == string.Empty)
				seq = "123";
			temp1.Identity();
			for (int i = seq.Length - 1; i >= 0; i--)
			{
				switch (seq[i])
				{
					case '1':
					case 'X':
					case 'x':
						temp1.RotationTo(1, angle[i]);
						this *= temp1;
						temp1.yy = temp1.zz = 1;
						temp1.yz = temp1.zy = 0;
						break;
					case '2':
					case 'Y':
					case 'y':
						temp1.RotationTo(2, angle[i]);
						this *= temp1;
						temp1.xx = temp1.zz = 1;
						temp1.xz = temp1.zx = 0;
						break;
					case '3':
					case 'Z':
					case 'z':
						temp1.RotationTo(3, angle[i]);
						this *= temp1;
						temp1.xx = temp1.yy = 1;
						temp1.xy = temp1.yx = 0;
						break;
				}
			}

			return this;
		}

		/// <summary>
		/// Calculate the rotation DCM in 'seq' order using the Vector3 angle. Rotates a point in frame
		/// </summary>
		/// <param name="seq">Each character of the string is a rotation axis. An axis may be 1,2 or 3 or x,y or z. The sequence may be any arbitrary order, but a maximum or 3 characters. 0 can be used to skip an angle.</param>
		/// <param name="angle">Each angle value [degrees] must match the corresponding axis character in sequence.</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotateToDegrees(string seq, params double[] angle)
		{
			switch (angle.Length)
			{
				case 1:
					return RotateTo(seq,
						angle[0] * Constant.RadPerDeg);
				case 2:
					return RotateTo(seq,
						angle[0] * Constant.RadPerDeg,
						angle[1] * Constant.RadPerDeg);
				case 3:
					return RotateTo(seq,
						angle[0] * Constant.RadPerDeg,
						angle[1] * Constant.RadPerDeg,
						angle[2] * Constant.RadPerDeg);
				default: return this;
			}
		}

		/// <summary>
		/// Create the rotation matrix, ASSUMING the matrix is initially Identity
		/// </summary>
		/// <param name="axis">The orthogonal axis (1,2,3) to rotate about</param>
		/// <param name="angle">Rotation angle [radians]</param>
		/// <returns>The original DCM with the new rotation overlaid</returns>
		public Dcm RotationFrom(int axis, double angle)
		{
			double sin = Math.Sin(angle), cos = Math.Cos(angle);

			switch (axis)
			{
				case 1:
					yy = cos;
					yz = sin;
					zy = -sin;
					zz = cos;
					break;
				case 2:
					xx = cos;
					xz = -sin;
					zx = sin;
					zz = cos;
					break;
				case 3:
					xx = cos;
					xy = sin;
					yx = -sin;
					yy = cos;
					break;
			}

			return this;
		}

		/// <summary>
		/// Calculate the rotation DCM in 'seq' order using the double[] angle. Rotates a point in frame
		/// </summary>
		/// <param name="seq">Each character of the string is a rotation axis. An axis may be 1,2 or 3 or x,y or z. The sequence may be any arbitrary length and order. o can be used to skip an angle.</param>
		/// <param name="angle"> Each angle value [radians] must match the corresponding axis character in sequence</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotationFrom(string seq, params double[] angle)
		{
			if (seq == null || seq == string.Empty)
				seq = "123";
			temp1.Identity();
			for (int i = seq.Length - 1; i >= 0; i--)
			{
				switch (seq[i])
				{
					case '1':
					case 'X':
					case 'x':
						temp1.RotationFrom(1, angle[i]);
						this *= temp1;
						temp1.yy = temp1.zz = 1;
						temp1.yz = temp1.zy = 0;
						break;
					case '2':
					case 'Y':
					case 'y':
						temp1.RotationFrom(2, angle[i]);
						this *= temp1;
						temp1.xx = temp1.zz = 1;
						temp1.xz = temp1.zx = 0;
						break;
					case '3':
					case 'Z':
					case 'z':
						temp1.RotationFrom(3, angle[i]);
						this *= temp1;
						temp1.xx = temp1.yy = 1;
						temp1.xy = temp1.yx = 0;
						break;
				}
			}

			return this;
		}

		/// <summary>
		/// Calculate the rotation DCM in 'seq' order using the Vector3 angle. Rotates the frame
		/// </summary>
		/// <param name="seq">Each character of the string is a rotation axis. An axis may be 1,2 or 3 or x,y or z. The sequence may be any arbitrary order, but a maximum or 3 characters. 0 can be used to skip an angle.</param>
		/// <param name="angle">Each angle value [radians] must match the corresponding axis character in sequence.</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotationFrom(string seq, Vector3 angle)
		{
			return RotationFrom(seq, angle.X, angle.Y, angle.Z);
		}

		/// <summary>
		/// Calculate the rotation DCM in 'seq' order using the Vector3 angle. Rotates the frame
		/// </summary>
		/// <param name="seq">Each character of the string is a rotation axis. An axis may be 1,2 or 3 or x,y or z. The sequence may be any arbitrary order, but a maximum or 3 characters. 0 can be used to skip an angle.</param>
		/// <param name="angle">Each angle value [degrees] must match the corresponding axis character in sequence.</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotationFromDegrees(string seq, params double[] angle)
		{
			switch (angle.Length)
			{
				case 1:
					return RotationFrom(seq,
						angle[0] * Constant.RadPerDeg);
				case 2:
					return RotationFrom(seq,
						angle[0] * Constant.RadPerDeg,
						angle[1] * Constant.RadPerDeg);
				case 3:
					return RotationFrom(seq,
						angle[0] * Constant.RadPerDeg,
						angle[1] * Constant.RadPerDeg,
						angle[2] * Constant.RadPerDeg);
				default: return this;
			}
		}

		/// <summary>
		/// Calculate the rotation DCM in 'seq' order using the Vector3 angle. Rotates the frame
		/// </summary>
		/// <param name="seq">Each character of the string is a rotation axis. An axis may be 1,2 or 3 or x,y or z. The sequence may be any arbitrary order, but a maximum or 3 characters. 0 can be used to skip an angle.</param>
		/// <param name="angle">Each angle value [degrees] must match the corresponding axis character in sequence.</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotationFromDegrees(string seq, Vector3 angle)
		{
			switch (seq.Length)
			{
				case 3: return RotationFromDegrees(seq, angle.X, angle.Y, angle.Z);
				case 2: return RotationFromDegrees(seq, angle.X, angle.Y);
				case 1: return RotationFromDegrees(seq, angle.X);
				default: return this;
			}
		}

		/// <summary>
		/// Create the rotation matrix, ASSUMING the matrix is initially Identity
		/// </summary>
		/// <param name="axis">The orthogonal axis (1,2,3) to rotate about</param>
		/// <param name="angle">The angle to rotate through, [radians]</param>
		/// <returns>This Dcm, set to the rotation</returns>
		public Dcm RotationTo(int axis, double angle)
		{
			double sin = Math.Sin(angle), cos = Math.Cos(angle);

			switch (axis)
			{
				case 1:
					yy = cos;
					yz = -sin;
					zy = sin;
					zz = cos;
					break;
				case 2:
					xx = cos;
					xz = sin;
					zx = -sin;
					zz = cos;
					break;
				case 3:
					xx = cos;
					xy = -sin;
					yx = sin;
					yy = cos;
					break;
			}
			return this;
		}

		#region Quaternion

		/// <summary>
		/// Calculate a quaternion from a Dcm
		/// </summary>
		/// <param name="dcm">The source Dcm</param>
		/// <param name="q">The destination Quaternion</param>
		/// <returns>The destination Quaternion</returns>
		public static Quaternion ToQuaternion(Dcm dcm, ref Quaternion q)
		{
			int max_key;
			double max_value;
			double denom;
			double trace;
			double testValue;

			trace = dcm.xx + dcm.yy + dcm.zz;
			max_value = 2.0 * dcm.xx + 1.0 - trace;
			max_key = 0;

			testValue = 2.0 * dcm.yy + 1.0 - trace;
			if (testValue > max_value)
			{
				max_value = testValue;
				max_key = 1;
			}
			testValue = (2.0 * dcm.zz + 1.0 - trace);
			if (testValue > max_value)
			{
				max_value = testValue;
				max_key = 2;
			}
			testValue = trace + 1.0;
			if (testValue > max_value)
			{
				max_value = testValue;
				max_key = 3;
			}
			if (max_value < 0)
			{
				throw new Exception("Invalid rotation Dcm");
			}
			// compute quaternion extract based 
			// on which element was the largest 

			switch (max_key)
			{
				case 0:
					q.q0 = 0.5 * Math.Sqrt(max_value);
					denom = 0.25 / q.q0;
					q.q1 = (dcm.xy + dcm.yx) * denom;
					q.q2 = (dcm.xz + dcm.zx) * denom;
					q.q3 = (dcm.yz - dcm.zy) * denom;
					break;

				case 1:
					q.q1 = 0.5 * Math.Sqrt(max_value);
					denom = 0.25 / q.q1;
					q.q0 = (dcm.xy + dcm.yx) * denom;
					q.q2 = (dcm.zy + dcm.yz) * denom;
					q.q3 = (dcm.zx - dcm.xz) * denom;
					break;

				case 2:
					q.q2 = 0.5 * Math.Sqrt(max_value);
					denom = 0.25 / q.q2;
					q.q0 = (dcm.xz + dcm.zx) * denom;
					q.q1 = (dcm.zy + dcm.yz) * denom;
					q.q3 = (dcm.xy - dcm.yx) * denom;
					break;

				case 3:
					q.q3 = 0.5 * Math.Sqrt(max_value);
					denom = 0.25 / q.q3;
					q.q0 = (dcm.yz - dcm.zy) * denom;
					q.q1 = (dcm.zx - dcm.xz) * denom;
					q.q2 = (dcm.xy - dcm.yx) * denom;
					break;
			}
			return q;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q">The destination Quaternion</param>
		/// <returns>The destination Quaternion</returns>
		public Quaternion ToQuaternion(ref Quaternion q)
		{
			ToQuaternion(this, ref q);
			return q;
		}

		#endregion

		public void xTranspose()
		{
			double xy0 = xy, xz0 = xz, yz0 = yz;
			xy = yx; xz = zx; yx = xy0; yz = zy; zx = xz0; zy = yz0;
		}
		public void Transpose(ref Dcm rhs)
		{
			rhs.xx = xx; rhs.yy = yy; rhs.zz = zz;
			rhs.xy = yx; rhs.xz = zx; rhs.yx = xy; rhs.yz = zy; rhs.zx = xz; rhs.zy = yz;
		}

		public Dcm yTranspose()
		{
			Dcm lhs = new Dcm();
			lhs.xx = xx; lhs.yy = yy; lhs.zz = zz;
			lhs.xy = yx; lhs.xz = zx; lhs.yx = xy; lhs.yz = zy; lhs.zx = xz; lhs.zy = yz;
			return lhs;
		}

		#region IArrayProxy Members

		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return xx;
					case 1: return xy;
					case 2: return xz;
					case 3: return yx;
					case 4: return yy;
					case 5: return yz;
					case 6: return zx;
					case 7: return zy;
					case 8: return zz;
				}
				throw new IndexOutOfRangeException(string.Format("Dcm[{0}]: > 8", index));
			}
			set
			{
				switch (index)
				{
					case 0: xx = value; break;
					case 1: xy = value; break;
					case 2: xz = value; break;
					case 3: yx = value; break;
					case 4: yy = value; break;
					case 5: yz = value; break;
					case 6: zx = value; break;
					case 7: zy = value; break;
					case 8: zz = value; break;
				}
			}
		}

		public object ConvertFrom(string csv) { return new Dcm(csv); }

		public int GetUpperBound(int dimension) { return 8; }

		public int Length { get { return 9; } }

		public int Rank { get { return 2; } }

		public string Suffix(int index)
		{
			switch (index)
			{
				case 0: return "xx";
				case 1: return "xy";
				case 2: return "xz";
				case 3: return "yx";
				case 4: return "yy";
				case 5: return "yz";
				case 6: return "zx";
				case 7: return "zy";
				case 8: return "zz";
			}
			throw new IndexOutOfRangeException(string.Format("Dcm.Suffix({0}: > 3", index));
		}

		#endregion
	}
}
