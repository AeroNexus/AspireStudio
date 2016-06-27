using System;
using System.Collections.Generic;


namespace Aspire.Framework
{
	/// <summary>
	/// An item on the Blackboard
	/// </summary>
	public class DoubleItem : Blackboard.Item
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public DoubleItem() { }

		/// <summary>
		/// Construct using a full path
		/// </summary>
		/// <param name="path"></param>
		public DoubleItem(string path)
		{
			Path = path;
		}

		///// <summary>
		///// Indexer for a one-dimensional array item
		///// </summary>
		//public new double this[int index]
		//{
		//	get { return (double)base[index]; }
		//	set
		//	{
		//		base[index] = value;
		//	}
		//}

		/// <summary>
		/// The target datum the Item points to
		/// </summary>
		public new object Datum { get; set; }

		/// <summary>
		/// Accesses the value of a Blackboard item
		/// </summary>
		public new object Value
		{
			get
			{
				return (double)base.Value;
			}
			set
			{
				base.Value = value;
			}
		}
	}
}
