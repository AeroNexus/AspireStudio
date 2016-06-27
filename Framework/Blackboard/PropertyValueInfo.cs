using System;
using System.Reflection;

using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// PropertyValueInfo is an IValueInfo constructed from a PropertyInfo, used for member properties.
	/// </summary>
	public class PropertyValueInfo : IValueInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="owner"></param>
		/// <param name="isReadOnly"></param>
		public PropertyValueInfo(PropertyInfo info, object owner, bool isReadOnly = false)
		{
			mInfo = info;
			mOwner = owner;
			mIsReadOnly = isReadOnly;
		}

		private PropertyValueInfo() { }

		/// <summary>
		/// Gets the owner of this value (the implementing type)
		/// </summary>
		public object Owner{ get { return mOwner; } }
		private object mOwner;

		/// <summary>
		/// Gets the FieldInfo this IValueInfo was created from.
		/// </summary>
		public PropertyInfo Info{ get { return mInfo; } }
		PropertyInfo mInfo;

		#region IValueInfo implementation

		/// <summary>
		/// Indexer for one-dimensional arrays
		/// </summary>
		public object this[int index]
		{
			get
			{
				if (mInfo != null && mInfo.PropertyType.IsArray && mOwner != null)
				{
					Array array = (Array)mInfo.GetValue(mOwner,null);
					if (array.Rank == 1)
						return array.GetValue(index);
					else
					{
						// calculate array idices
						int firstIndex = index / array.GetLength(0);
						int secondIndex = index - (firstIndex * array.GetLength(0));
						return array.GetValue(firstIndex, secondIndex);
					}
				}
				else
					return Value;
			}
			set
			{
				if (mInfo != null && mInfo.PropertyType.IsArray && mOwner != null)
				{
					Array array = (Array)mInfo.GetValue(mOwner,null);
					if (array.Rank == 1)
						array.SetValue(value, new long[] { index });
					else
					{
						// calculate array idices
						int firstIndex = index / array.GetLength(0);
						int secondIndex = index - (firstIndex * array.GetLength(0));
						array.SetValue(value, firstIndex, secondIndex);
					}
				}
				else
					Value = value;
			}
		}

		/// <summary>
		/// Gets and sets the value
		/// </summary>
		public object Value
		{
			get
			{
				if (mInfo != null && mOwner != null)
					return mInfo.GetValue(mOwner,null);
				return null;
			}
			set
			{
				if (mInfo != null && mOwner != null)
					try
					{
						mInfo.SetValue(mOwner, value, null);
					}
					catch (Exception e)
					{
						Log.ReportException(e, "pvi.setValue");
					}
			}
		}


		/// <summary>
		/// Gets and sets whether or not the item is read-only
		/// </summary>
		public bool IsReadOnly
		{
			get { return mIsReadOnly; }
			set { mIsReadOnly = value; }
		}
		bool mIsReadOnly;

		#endregion

	}
}
