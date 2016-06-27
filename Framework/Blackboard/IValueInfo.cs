using System;

namespace Aspire.Framework
{
	/// <summary>
	/// IValueInfo interface used for ValueInfo implementations for the Blackboard
	/// </summary>
	public interface IValueInfo
	{
		///// <summary>
		///// Indexer for one-dimensional arrays
		///// </summary>
		//object this[int index] { get; set; }

		/// <summary>
		/// The value
		/// </summary>
		object Value{ get; set; }

		/// <summary>
		/// Whether or not the value is read-only
		/// </summary>
		bool IsReadOnly{ get; set; }
	}
}
