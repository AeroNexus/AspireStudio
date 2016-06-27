using System;

namespace Aspire.Framework
{
	/// <summary>
	/// ObjectInfo implements IObjectInfo for standalone objects in the Blackboard
	/// </summary>
	public class ObjectInfo : IObjectInfo
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ObjectInfo() { }

		/// <summary>
		/// COnstruct using an initial value and read only flag
		/// </summary>
		/// <param name="initialValue"></param>
		/// <param name="isReadOnly"></param>
		public ObjectInfo(object initialValue, bool isReadOnly=false)
		{
			mValue = initialValue;
			mIsReadOnly = isReadOnly;
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
					  mValue = value; 
			}
		}
		object mValue;

		/// <summary>
		/// Whether or not the value is read-only
		/// </summary>
		public bool IsReadOnly { get { return mIsReadOnly; } }
		bool mIsReadOnly;

		#endregion
	}
}
