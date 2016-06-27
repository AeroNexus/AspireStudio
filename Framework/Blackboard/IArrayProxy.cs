using System;

namespace Aspire.Framework
{
	/// <summary>
	/// This interface is used to inform marshalers that a class is mimicing 
	/// System.Array and it should be handled as such
	/// </summary>
	public interface IArrayProxy
	{
		/// <summary>
		/// Access each element within the array
		/// </summary>
		double this[int index] { get; set; }
		// <summary>
		// 
		// </summary>
		// <param name="arrayProxy"></param>
		//void Associate(IArrayProxy arrayProxy);
		//ToDo:Implement Associate
		/// <summary>
		/// Parse and create from a string
		/// </summary>
		/// <param name="csv">The string to parse</param>
		/// <returns></returns>
		object ConvertFrom(string csv);
		/// <summary>
		/// Get the maximum value of the index per dimension
		/// </summary>
		int GetUpperBound(int dimension);
		/// <summary>
		/// Get the number of elements contained within the array
		/// </summary>
		int Length { get; }
		/// <summary>
		/// Get the number of dimensions
		/// </summary>
		int Rank { get; }
		/// <summary>
		/// Get the element name at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		string Suffix(int index);
	}
}
