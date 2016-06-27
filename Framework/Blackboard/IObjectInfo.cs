using System;

namespace Aspire.Framework
{
	/// <summary>
	/// IObjectInfo interface used to wrap standalone objects for the Blackboard
	/// </summary>
	public interface IObjectInfo
	{
		/// <summary>
		/// The value
		/// </summary>
		object Value{ get; set; }

		/// <summary>
		/// Whether or not the value is read-only
		/// </summary>
		bool IsReadOnly{ get; }
	}
}
