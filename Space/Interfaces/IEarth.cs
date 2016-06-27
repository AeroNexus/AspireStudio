using Aspire.Primitives;

namespace Aspire.Space
{
	/// <summary>
	/// Interface to earth models
	/// </summary>
	public interface IEarth
	{
		/// <summary>
		/// gets the earth model
		/// </summary>
		/// <returns>The Earth model for this context</returns>
		Earth GetEarth();
		/// <summary>
		/// Sets the earth local frame Ecf
		/// </summary>
		EcefState Ecef { set; }
		/// <summary>
		/// Gets the earth body(ECEF) frame
		/// </summary>
		Frame EarthBodyFrame { get; }
	}
}
