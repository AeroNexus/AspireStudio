using System;
using System.Reflection;

namespace Aspire.Framework
{
	/// <summary>
	/// ObjectValueInfo is an IValueInfo constructed from a IObjectInfo, used for standalone objects.
	/// </summary>
	public class ObjectValueInfo : IValueInfo
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="info"></param>
		/// <param name="owner"></param>
		/// <param name="isReadOnly"></param>
		public ObjectValueInfo(IObjectInfo info, object owner, bool isReadOnly = false)
		{
			mInfo = info;
			mOwner = owner;
			mIsReadOnly = isReadOnly;
		}

		/// <summary>
		/// Construct with owner
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="isReadOnly"></param>
		public ObjectValueInfo(object owner, bool isReadOnly = false)
		{
			mInfo = new ObjectInfo(owner);
			mOwner = owner;
			mIsReadOnly = isReadOnly;
		}

		private ObjectValueInfo() { }

		/// <summary>
		/// Gets the owner of this value (the implementing type)
		/// </summary>
		public object Owner{ get { return mOwner; } }
		private object mOwner;


		/// <summary>
		/// Gets the FieldInfo this IValueInfo was created from.
		/// </summary>
		public IObjectInfo Info { get { return mInfo; } }
		IObjectInfo mInfo;

		#region IValueInfo implementation

		/// <summary>
		/// Indexer for one-dimensional arrays
		/// </summary>
		public object this[int index]
		{
			get { return Value; }
			set { Value = value; }
		}

		/// <summary>
		/// Gets and sets the value
		/// </summary>
		public object Value
		{
			get
			{
				if (mInfo != null)
					return mInfo.Value;
				return null;
			}
			set
			{
				if (mInfo != null)
					mInfo.Value = value;
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
