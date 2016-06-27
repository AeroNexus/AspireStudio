using System;
using System.ComponentModel;

using Aspire.Framework;

namespace Aspire.Primitives
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Vector3 : IArrayProxy
    {
		double mX, mY, mZ;
		//static int count = 1;
		//int tag;

		public Vector3(double x, double y, double z)
		{
			mX = x; mY = y; mZ = z;
			//tag = count++;
		}

		public Vector3(string csv)
		{
			var items = csv.Split(',');
			mX = mY = mZ = 0;
			try
			{
				if (items.Length > 0 && items[0].Length > 0) mX = double.Parse(items[0]);
				if (items.Length > 1 && items[1].Length > 0) mY = double.Parse(items[1]);
				if (items.Length > 2 && items[2].Length > 0) mZ = double.Parse(items[2]);
			}
			catch (Exception) { }
		}

		public static implicit operator Vector3(string csv)
		{
			return new Vector3(csv);
		}

		public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
		{
			Vector3 result = new Vector3()
			{
				mX = lhs.mX + rhs.mX,
				mY = lhs.mY + rhs.mY,
				mZ = lhs.mZ + rhs.mZ
			};
			return result;
		}

		public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
		{
			Vector3 result = new Vector3()
			{
				mX = lhs.mX - rhs.mX,
				mY = lhs.mY - rhs.mY,
				mZ = lhs.mZ - rhs.mZ
			};
			return result;
		}

		/// <summary>
		/// Negation. Unary minus operator.  Returns minus 'this'.
		/// </summary>
		/// <returns>A temporary resultant Vector3 = -this.</returns>
		/// <remarks>Not recommended for real time code. Dynamic allocation might cause a page fault.</remarks>
		public static Vector3 operator -(Vector3 rhs)
		{
			Vector3 result = new Vector3(-rhs.mX,-rhs.mY,-rhs.mZ);
			return result;
		}

		public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
		{
			Vector3 result = new Vector3()
			{
				mX = lhs.mX * rhs.mX,
				mY = lhs.mY * rhs.mY,
				mZ = lhs.mZ * rhs.mZ
			};
			return result;
		}

		public static Vector3 operator *(double lhs, Vector3 rhs)
		{
			Vector3 result = new Vector3(lhs * rhs.mX, lhs * rhs.mY, lhs * rhs.mZ);
			return result;
		}

		public static Vector3 operator *(Vector3 lhs, double rhs)
		{
			Vector3 result = new Vector3(lhs.mX*rhs, lhs.mY*rhs, lhs.mZ*rhs);
			return result;
		}

		public static Vector3 operator /(Vector3 lhs, Vector3 rhs)
		{
			Vector3 result = new Vector3()
			{
				mX = lhs.mX / rhs.mX,
				mY = lhs.mY / rhs.mY,
				mZ = lhs.mZ / rhs.mZ
			};
			return result;
		}

		#region Cross Product

		/// <summary>
		/// Calculate the cross product of two Vector3s and store the result in a parameter.
		/// <para>'result' may be 'rhs' or 'lhs', if desired.</para>
		/// <para>CrossProduct(result, lhs, rhs)</para>
		/// <para>(result = lhs X rhs)</para>
		/// </summary>
		/// <param name="result">The resultant Vector3.</param>
		/// <param name="lhs">The LHS Vector3.</param>
		/// <param name="rhs">The RHS Vector3.</param>
		/// <returns>The resultant Vetor3, result.</returns>
		public static Vector3 CrossProduct(Vector3 result, Vector3 lhs, Vector3 rhs)
		{
			double a0 = lhs.mY * rhs.mZ - lhs.mZ * rhs.mY;
			double a1 = lhs.mZ * rhs.mX - lhs.mX * rhs.mZ;
			result.mZ = lhs.mX * rhs.mY - lhs.mY * rhs.mX;
			result.mX = a0;
			result.mY = a1;
			return result;
		}

		/// <summary>
		/// Calculate the cross product of two Vector3's and store the result in this Vector3.
		/// <para>Either 'rhs' or 'lhs' may be 'this', if desired.</para>
		/// <para>(syntax: this.CrossProduct(lhs, rhs))</para>
		/// <para>(this = lhs X rhs)</para>
		/// </summary>
		/// <param name="lhs">The LHS vector.</param>
		/// <param name="rhs">The RHS vector.</param>
		/// <returns>This Vector3 = lhs X rhs.</returns>
		public Vector3 CrossProduct(Vector3 lhs, Vector3 rhs)
		{
			double a0 = lhs.mY * rhs.mZ - lhs.mZ * rhs.mY;
			double a1 = lhs.mZ * rhs.mX - lhs.mX * rhs.mZ;
			mZ = lhs.mX * rhs.mY - lhs.mY * rhs.mX;
			mX = a0;
			mY = a1;
			return this;
		}

		#endregion

		#region Dot and AngleBetween

		/// <summary>
		/// Dot product of this Vector3 and another Vector3.
		/// <para>sytnax: result = anyvec.Dot(rhs)</para>
		/// </summary>
		/// <param name="rhs">The RHS vector.</param>
		/// <returns>The dot product.</returns>
		public double Dot(Vector3 rhs)
		{
			return mX * rhs.mX + mY * rhs.mY + mZ * rhs.mZ;
		}

		/// <summary>
		/// Angle between this Vector3 and another Vector3, in radians.
		/// <para>result = anyvec.AngleBetween(b)</para>
		/// <para>More accurate than simple acos(a/|a| dot b/|b|) for small angles.</para>
		/// </summary>
		/// <param name="b">The second vector.</param>
		/// <returns>The angle (in radians) between this Vector3 and Vector3 b.)</returns>
		public double AngleBetween(Vector3 b)
		{
			return AngleBetween(this, b);
		}

		/// <summary>
		/// Angle between two Vector3.
		/// <para>result = AngleBetween(a,b)</para>
		/// <para>More accurate than simple acos(a/|a| dot b/|b|) for some angles.</para>
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The angle (in radians) between Vector3 a and Vector3 b.)</returns>
		public static double AngleBetween(Vector3 a, Vector3 b)
		{
			// This method avoids the arc-cosine inaccuracy problems with argument
			// near +/- 1.
			// I am not convinced that the argument to asin will never be
			// greater than one due to rounding or truncation, so the test
			// on 'value' is in place, but it may not be needed.
			Vector3 aunit = a;
			aunit.Normalize();
			Vector3 bunit = b;
			bunit.Normalize();
			double dotprod = aunit.Dot(bunit);
			if (Math.Abs(dotprod) < 0.8) return Math.Acos(dotprod);
			// vectors are somewhat parallel (or anti-parallel)
			if (dotprod < 0)
			{
				double value = (aunit + bunit).Magnitude / 2;
				if (value > 1) value = 1;
				return Math.PI - 2 * Math.Asin(value);
			}
			return 2 * Math.Asin((aunit - bunit).Magnitude / 2);
		}

		#endregion

		public Vector3 Identity()
		{
			mX = 1; mY = mZ = 0;
			return this;
		}

		public double[] Fill(double[] array)
		{
			array[0] = mX; array[1] = mY; array[2] = mZ;
			return array;
		}

		public bool IsZero
		{
			get
			{
				return mX == 0 && mY == 0 && mZ == 0;
			}
		}

		public double Magnitude
		{
			get
			{
				return Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
			}
		}

		public double MagnitudeSquared
		{
			get
			{
				return mX * mX + mY * mY + mZ * mZ;
			}
		}

		#region Normalize

		/// <summary>
		/// Normalize this Vector3 in situ.
		/// <P>Caution: A zero vector is returned unchanged, does not throw an exception.</P>
		/// </summary>
		/// <returns>This Vector3, normalized.</returns>
		public Vector3 Normalize()
		{
			double magnitude = Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
			if (magnitude != 0)
			{
				mX /= magnitude;
				mY /= magnitude;
				mZ /= magnitude;
			}
			return this;
		}

		/// <summary>
		/// Normalize this Vector3 in situ and return (in the parameter) the original magnitude.
		/// <P>Caution: A zero vector is returned unchanged, does not throw an exception.</P>
		/// </summary>
		/// <param name="magnitude">The magnitude of this Vector3 prior to normalization.</param>
		/// <returns>This Vector3, normalized.</returns>
		public Vector3 Normalize(out double magnitude)
		{
			magnitude = Math.Sqrt(mX * mX + mY * mY + mZ * mZ);
			if (magnitude != 0)
			{
				mX /= magnitude;
				mY /= magnitude;
				mZ /= magnitude;
			}
			return this;
		}

		#endregion

		#region Rotate

		/// <summary>
		/// Rotate this vector in situ CCW about the X axis
		/// </summary>
		/// <param name="angle">Angle to rotate about the X axis [rad]</param>
		public void RotateX(double angle)
		{
			double s = Math.Sin(angle);
			double c = Math.Cos(angle);
			double y = c * mY - s * mZ;
			      mZ = s * mY + c * mZ;
			mY = y;
		}

		/// <summary>
		/// Rotate this vector in situ CCW about the Y axis
		/// </summary>
		/// <param name="angle">Angle to rotate about the Y axis [rad]</param>
		public void RotateY(double angle)
		{
			double s = Math.Sin(angle);
			double c = Math.Cos(angle);
			double x =  c * mX + s * mZ;
			      mZ = -s * mX + c * mZ;
			mX = x;
		}

		/// <summary>
		/// Rotate this vector in situ CCW about the Z axis
		/// </summary>
		/// <param name="angle">Angle to rotate about the Z axis [rad]</param>
		public void RotateZ(double angle)
		{
			double s = Math.Sin(angle);
			double c = Math.Cos(angle);
			double x = c * mX - s * mY;
			      mY = s * mX + c * mY;
			mX = x;
		}


		#endregion

		public Vector3 Set(double x, double y, double z)
		{
			mX = x;
			mY = y;
			mZ = z;
			return this;
		}

		public Vector3 Set(double[] array)
		{
			mX = array[0];
			mY = array[1];
			mZ = array[2];
			return this;
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2}",mX, mY, mZ);
		}

		public double X
		{
			get { return mX; }
			set { mX = value; }
		}
		public double Y
		{
			get { return mY; }
			set { mY = value; }
		}
		public double Z
		{
			get { return mZ; }
			set { mZ = value; }
		}

		public Vector3 Zero()
		{
			mX = mY = mZ = 0;
			return this;
		}

		#region IArrayProxy Members

		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return mX;
					case 1: return mY;
					case 2: return mZ;
				}
				throw new IndexOutOfRangeException(string.Format("Vector3[{0}]: > 2", index));
			}
			set
			{
				switch (index)
				{
					case 0: mX = value; break;
					case 1: mY = value; break;
					case 2: mZ = value; break;
				}
			}
		}

		public object ConvertFrom(string csv) { return new Vector3(csv); }

		public int GetUpperBound(int dimension) { return 2; }

		public int Length { get { return 3; } }

		public int Rank { get { return 1; } }

		public string Suffix(int index)
		{
			switch (index)
			{
				case 0: return "X";
				case 1: return "Y";
				case 2: return "Z";
			}
			throw new IndexOutOfRangeException(string.Format("Vector3.Suffix({0}: > 2", index));
		}

		#endregion
	}
}
