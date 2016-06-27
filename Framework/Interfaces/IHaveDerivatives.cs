
namespace Aspire.Framework
{
	/// <summary>
	/// An implementor has derivatives that need to be integrated
	/// </summary>
	public interface IHaveDerivatives
	{
		/// <summary>
		/// Implementors evaluate their derivatives here to set a state's derivative and access a State's value
		/// </summary>
		void CalculateDerivatives();

		/// <summary>
		/// Implementors copy the actual state values into the integrated states if they have changed outside the integrators
		/// </summary>
		void Synchronize(bool force=false);
	}
}
