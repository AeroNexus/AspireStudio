using System;
using System.Text;

using Aspire.Framework;

namespace Aspire.Primitives
{
	public struct Vector : IArrayProxy
    {
		double[] values;
		int length;

		public Vector(params double[] values)
		{
			length = values.Length;

			this.values = new double[values.Length];
			values.CopyTo(this.values, 0);
		}

		public Vector(string csv)
		{
			var items = csv.Split(','); // need to look for semi-colons for rank=2
			length = items.Length;

			values = new double[items.Length];
			for(int i=0; i<items.Length; i++)
				values[i] = double.Parse(items[i]);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(values[0]);
			for (int j = 1; j < Length; j++)
				sb.AppendFormat(",{0}", values[j]);
			return sb.ToString();
		}

		public double[] Values { get { return values; } }

		#region IArrayProxy Members

		public double this[int index]
		{
			get
			{
				if (index < length)
					return values[index];
				throw new IndexOutOfRangeException(string.Format("Vector[{0}]: > 2", index));
			}
			set
			{
				if (index < length)
					values[index] = value;
			}
		}

		public object ConvertFrom(string csv) { return new Vector(csv); }

		public int GetUpperBound(int dimension) { return length - 1; }

		public int Length { get { return length; } }

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
