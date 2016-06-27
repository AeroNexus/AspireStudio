using System;
using System.ComponentModel;

namespace Aspire.Primitives
{
	/// <summary>
	/// A PolyTable implements a multi-variate polynomial lookup table
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class PolyTable
	{
		Vector[] polynomials;

		/// <summary>
		/// Default constructor
		/// </summary>
		public PolyTable() { }

		public PolyTable(string csv)
		{
			Rank = 1;
			polynomials = new Vector[Rank];
			polynomials[0] = new Vector(csv);
		}

		/// <summary>
		/// Construct a PolyTable from a Vector of coefficients
		/// </summary>
		/// <param name="s0">The first polynomial coefficients</param>
		public PolyTable(Vector s0)
		{
			Rank = 1;
			polynomials = new Vector[Rank];
			polynomials[0] = s0;
		}

		/// <summary>
		/// Construct a PolyTable from a pair of coefficient vectors
		/// </summary>
		/// <param name="s0">The first polynomial coefficients</param>
		/// <param name="s1">The second polynomial coefficients</param>
		public PolyTable(Vector s0, Vector s1)
		{
			Rank = 2;
			polynomials = new Vector[Rank];

			polynomials[0] = s0;
			polynomials[1] = s1;
		}

		/// <summary>
		/// Evaluate the derivative for a rank 1 poly table
		/// </summary>
		/// <param name="x">The independent variable</param>
		/// <returns>The evaluated value</returns>
		public double Derivative(double x)
		{
			double y = 0;

			for (int i = polynomials[0].Length - 1; i >= 1; i--)
				y = y * x + polynomials[0].Values[i] * (double)i;
			return y;
		}

		/// <summary>
		/// The array of Polynomial coefficients
		/// </summary>
		public Vector[] Polynomials { get { return polynomials; } set { polynomials = value; } }

		/// <summary>
		/// Number of dimensions
		/// </summary>
		public int Rank { get; set; }

		/// <summary>
		/// Evaluate the value for a rank 1 poly table
		/// </summary>
		/// <param name="x">The independent variable</param>
		/// <returns>The evaluated value</returns>
		public double Value(double x)
		{
			double y;
			double[] c = polynomials[0].Values;

			switch (polynomials[0].Length)
			{
				case 1:
					y = c[0];
					break;
				case 2:
					y = x * c[1] + c[0];
					break;
				case 3:
					y = x * (x * c[2] + c[1]) + c[0];
					break;
				case 4:
					y = x * (x * (x * c[3] + c[2]) + c[1]) + c[0];
					break;
				case 5:
					y = x * (x * (x * (x * c[4] + c[3]) + c[2]) + c[1]) + c[0];
					break;
				default:
					y = 0;
					for (int i = polynomials[0].Length - 1; i >= 0; i--)
						y = y * x + c[i];
					break;
			}
			return y;
		}

		/// <summary>
		/// Evaluate the value for a rank 1 poly table
		/// </summary>
		/// <param name="x1">The first independent variable</param>
		/// <param name="x2">The second independent variable</param>
		/// <returns>The evaluated value</returns>
		public double Value(double x1, double x2)
		{
			double ai, y = 0;

			y = 0;
			for (int i = Rank-1; i >= 0; i--)
			{
				ai = 0;
				for (int j = polynomials[i].Length - 1; j >= 0; j--)
					ai = ai * x1 + polynomials[i].Values[j];
				y = y * x2 + ai;
			}
			return y;
		}
	}
}
