using System;
using System.Text;

using Aspire.Framework;

namespace Aspire.Primitives
{
	public struct Matrix : IArrayProxy
    {
		double[] values;
		int length, rank;
		int[] dimension;

		public Matrix(int x, int y)
		{
			rank = 2;
			dimension = new int[] { x, y };
			length = x * y;

			values = new double[length];
		}

		public Matrix(params double[] values)
		{
			rank = 1;
			dimension = new int[1] { values.Length };
			length = values.Length;

			this.values = new double[values.Length];
			values.CopyTo(this.values, 0);
		}

		public Matrix(string csv)
		{
			var items = csv.Split(','); // need to look for semi-colons for rank=2
			rank = 1;
			dimension = new int[1] { items.Length };
			length = items.Length;

			values = new double[items.Length];
			for(int i=0; i<items.Length; i++)
				values[i] = double.Parse(items[i]);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (rank == 1)
			{
				sb.Append(values[0]);
				for (int i = 1; i < length; i++)
					sb.AppendFormat(",{0}", values[i]);
			}
			else if (rank == 2)
			{
				for (int i = 0; i < dimension[0]; i++)
				{
					int dim1 = dimension[1];
					sb.Append(values[i*dim1]);
					for (int j = 0; j < dim1; j++)
						sb.AppendFormat(",{0}", values[i*dim1+j]);
					if (i < rank - 1)
						sb.Append(';');
				}
			}
			return sb.ToString();
		}

		#region IArrayProxy Members

		public double this[int index]
		{
			get
			{
				if (index < length)
					return values[index];
				throw new IndexOutOfRangeException(string.Format("Matrix[{0}]: > {1}", index, length));
			}
			set
			{
				if (index < length)
					values[index] = value;
			}
		}

		public object ConvertFrom(string csv) { return new Matrix(csv); }

		public int GetUpperBound(int dimension) { return this.dimension[dimension] - 1; }

		public int Length { get { return length; } }

		public int Rank { get { return rank; } }

		public string Suffix(int index)
		{
			if (index < length)
				return string.Format("[{0}]",index);
			throw new IndexOutOfRangeException(string.Format("Matrix.Suffix({0}: > 2", index));
		}

		#endregion
	}
}
