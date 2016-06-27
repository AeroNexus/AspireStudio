
namespace Aspire.Space
{
	/// <summary>
	/// The interface to all atmospheric Models
	/// </summary>
	public interface IAtmosphere
	{
		double Density(double altitude);

		double Temperature(double altitude);
	}
}
