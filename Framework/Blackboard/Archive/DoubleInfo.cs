using System;

namespace Aspire.Framework
{
	/// <summary>
	/// ObjectInfo implements IObjectInfo for standalone doubles in the Blackboard
	/// </summary>
	public class DoubleInfo : IObjectInfo
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public DoubleInfo() { }

		/// <summary>
		/// COnstruct using an initial value and read only flag
		/// </summary>
		/// <param name="initialValue"></param>
		/// <param name="isReadOnly"></param>
		public DoubleInfo(double initialValue, bool isReadOnly = false)
		{
			mValue = initialValue;
			mIsReadOnly = isReadOnly;
		}

		/// <summary>
		/// Gets the value as a double
		/// </summary>
		public double DoubleValue
		{
			get { return (mValue); }
			set
			{
				if ( !mIsReadOnly )
					mValue = value;
			}
		}

		#region IObjectInfo implementation

		/// <summary>
		/// The value
		/// </summary>
		public object Value{
			get { return mValue; }
			set
			{
				if( !mIsReadOnly)
					mValue = Convert.ToDouble(value, null);
			}
		}
		double mValue;

		/// <summary>
		/// Whether or not the value is read-only
		/// </summary>
		public bool IsReadOnly { get { return mIsReadOnly; } }
		bool mIsReadOnly;

		#endregion
	}
}
