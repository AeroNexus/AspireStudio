using System;

namespace Aspire.Framework
{
	/// <summary>
	/// This interface is used to inform the Blackboard that a class has an
	/// underlying type of System.Array and it should be handled as such
	/// </summary>
	public interface IHostArray
	{
		/// <summary>
		/// Access the underlying System.Array
		/// </summary>
		System.Array HostedArray { get; }
		/// <summary>
		/// Get the number of elements contained within the array
		/// </summary>
		int Length { get; }
		/// <summary>
		/// Get the number of dimensions
		/// </summary>
		int Rank { get; }
		/// <summary>
		/// Get the maximum value of the index per dimension
		/// </summary>
		int GetUpperBound(int dimension);
		/// <summary>
		/// Access each element within the array
		/// </summary>
		double this[int index] { get; set; }
		/// <summary>
		/// Fired when en element has been changed through a property
		/// </summary>
		event System.EventHandler ValueChanged;
		/// <summary>
		/// Called when a value has changed
		/// </summary>
		void OnValueChanged();
	}
}
