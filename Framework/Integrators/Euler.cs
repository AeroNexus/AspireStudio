
namespace Aspire.Framework
{
	/// <summary>
	/// Simplest of all integrators
	/// </summary>
	public class Euler : Integrator
	{
		/// <summary>
		/// Integrate a state
		/// </summary>
		/// <param name="dynamics"></param>
		public override void Integrate(Dynamics dynamics)
		{
			double h = dynamics.StepSize;//, error, errorSq=0, Kerr = -0.5*h*h;
			foreach ( var state in dynamics.States )
				for (int j = 0; j < state.Size; j++)
					state.Y[j] += h * state.Ydot[j];
		}

		/// <summary>
		/// Euler uses the default State constructor, but exists here as an example for a
		/// multi-sample integrator to construct its own derived State
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public override Dynamics.State NewState(int length)
		{
			return base.NewState(length);
		}
	}
}
