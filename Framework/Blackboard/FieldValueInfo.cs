using System;
using System.Reflection;

namespace Aspire.Framework
{
	/// <summary>
	/// FieldValueInfo is an IValueInfo constructed from a FieldInfo, used for member fields.
	/// </summary>
	public class FieldValueInfo : IValueInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="owner"></param>
		/// <param name="isReadOnly"></param>
		public FieldValueInfo(FieldInfo info, object owner, bool isReadOnly = false)
		{
			mInfo = info;
			mOwner = owner;
			mIsReadOnly = isReadOnly;
		}

		private FieldValueInfo() { }

		/// <summary>
		/// Gets the owner of this value (the implementing type)
		/// </summary>
		public object Owner{ get { return mOwner; } }
		private object mOwner;


		/// <summary>
		/// Gets the FieldInfo this IValueInfo was created from.
		/// </summary>
		public FieldInfo Info{ get { return mInfo; } }
		FieldInfo mInfo;

		#region IValueInfo implementation

		/// <summary>
		/// Indexer for one-dimensional arrays
		/// </summary>
		public object this[int index]
		{
			get
			{
				if (mInfo != null && mInfo.FieldType.IsArray && mOwner != null)
				{
					Array array = (Array)mInfo.GetValue(mOwner);
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
				if (mInfo != null && mInfo.FieldType.IsArray && mOwner != null)
				{
					Array array = (Array)mInfo.GetValue(mOwner);
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
					return mInfo.GetValue(mOwner);
				return null;
			}
			set
			{
				if (mInfo != null && mOwner != null)
					mInfo.SetValue(mOwner, value);
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
