using System;

namespace StarTechnologies.Utilities
{
	/// <summary>
	/// MinMaxAttribute is an <see cref="Attribute"/> used to decorate values that have a min/max range.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
	public class MinMaxAttribute: Attribute
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public MinMaxAttribute() {}

		/// <summary>
		/// Specify the attribute with min and max values
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public MinMaxAttribute( double min, double max ): this()
		{
			Min = min;
			Max = max;
		}

		/// <summary>
		/// Specify the attribute with min and max values
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="useTrackbarEditor">Whether a trackbar editor should be used to edit this member's value</param>
		public MinMaxAttribute( double min, double max, bool useTrackbarEditor ): this(min, max)
		{
			UseTrackbarEditor = useTrackbarEditor;
		}
		
		/// <summary>
		/// Specify the attribute with min and max values
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="useTrackbarEditor">Whether a trackbar editor should be used to edit this member's value</param>
		/// <param name="significantDigitsInEditor"></param>
		public MinMaxAttribute( double min, double max, bool useTrackbarEditor, int significantDigitsInEditor ): this(min, max, useTrackbarEditor)
		{
			SignificantDigitsInEditor = significantDigitsInEditor;
		}

		private int m_SignificantDigitsInEditor = 0;

		/// <summary>
		/// Gets and sets the number of significant digits to allow in the property editor
		/// </summary>
		public int SignificantDigitsInEditor
		{
			get { return(m_SignificantDigitsInEditor); }
			set { m_SignificantDigitsInEditor = value; }
		}

		private bool m_UseTrackbarEditor = false;

		/// <summary>
		/// Gets the minimum
		/// </summary>
		public bool UseTrackbarEditor
		{
			get { return(m_UseTrackbarEditor); }
			set { m_UseTrackbarEditor = value; }
		}

		private double m_Min = double.MinValue;

		/// <summary>
		/// Gets the minimum
		/// </summary>
		public double Min
		{
			get { return(m_Min); }
			set { m_Min = value; }
		}

		private double m_Max = double.MaxValue;

		/// <summary>
		/// Gets the maximum
		/// </summary>
		public double Max
		{
			get { return(m_Max); }
			set { m_Max = value; }
		}
	}
}
