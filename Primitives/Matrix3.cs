using System;

using Aspire.Framework;

namespace Aspire.Primitives
{
	public struct Matrix3 : IArrayProxy
    {
		double xx, xy, xz, yx, yy, yz, zx, zy, zz;

		public Matrix3(params double[] values)
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

		public Matrix3(string csv)
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

		public Matrix3 Identity()
		{
			xx = yy = zz = 1;
			xy = xz = yx = yz = zx = zy = 0;

			return this;
		}

		public override string ToString()
		{
			return string.Format("{0},{1},{2};{3},{4},{5};{6},{7},{8}", xx, xy, xz, yx, yy, yz, zx, zy, zz);
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
				throw new IndexOutOfRangeException(string.Format("Matrix3[{0}]: > 8", index));
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

		public object ConvertFrom(string csv) { return new Matrix3(csv); }

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
			throw new IndexOutOfRangeException(string.Format("Matrix3.Suffix({0}: > 3", index));
		}

		#endregion

		/// <summary>
		/// Multiply lhs * rhs to a temporary
		/// </summary>
		/// <param name="lhs">The LHS Matrix3</param>
		/// <param name="rhs">The RHS Matrix3</param>
		/// <returns>The temporary result</returns>
		public static Matrix3 operator *(Matrix3 lhs, Matrix3 rhs)
		{
			var result = new Matrix3();

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
		/// Multiply a Matrix3 by a Vector3 (matrix3[3,3]*vec[1,3])
		/// </summary>
		/// <param name="lhs">The MAtrix3 LHS</param>
		/// <param name="rhs">The Vector3 RHS</param>
		/// <returns>A temporary Vector = lhs*rhs</returns>
		public static Vector3 operator *(Matrix3 lhs, Vector3 rhs)
		{
			return new Vector3(
				lhs.xx * rhs.X + lhs.xy * rhs.Y + lhs.xz * rhs.Z,
				lhs.yx * rhs.X + lhs.yy * rhs.Y + lhs.yz * rhs.Z,
				lhs.zx * rhs.X + lhs.zy * rhs.Y + lhs.zz * rhs.Z);
		}

	}
}
