namespace Aspire.Framework
{
	/// <summary>
	/// Otherwise known as mid-point euler
	/// </summary>
	public class Trapezoidal : Integrator
	{
		/// <summary>
		/// Initialize the extended States
		/// </summary>
		/// <param name="dynamics"></param>
		public override void Initialize(Dynamics dynamics)
		{
			foreach ( State state in dynamics.States ) // note: State is our member class, derived from Dynamics.State
				for (int j = 0; j < state.Size; j++)
					state.yDot_1[j] = state.Ydot[j];
		}

		/// <summary>
		/// Integrate a state
		/// </summary>
		/// <param name="dynamics"></param>
		public override void Integrate(Dynamics dynamics)
		{
			double h = dynamics.StepSize;
			foreach (State state in dynamics.States)
				state.SaveY(h);

			dynamics.CalcDerivatives(1);
			h /= 2;

			foreach (State state in dynamics.States)
				state.Integrate(h);
		}

		// State for use with Trapezoidal Integrator (only)
		class State : Dynamics.State
		{
			internal double[] yDot_1, y0;

			public State(int size)
			{
				Size = size;
				if (size > 0)
				{
					yDot_1 = new double[size];
					y0 = new double[size];
				}
			}

			internal void Integrate(double h_2)
			{
				for (int j = 0; j < size; j++)
					y[j] = y0[j] + h_2 * (yDot[j] + yDot_1[j]); // new y computed from average slope
			}

			internal void SaveY(double h)
			{
				for (int j = 0; j < size; j++)
				{
					y0[j] = y[j];                   // save y
					y[j] = y0[j] + h * yDot[j];  // compute new y
					yDot_1[j] = yDot[j];            // save ydot
				}

			}

			/// <summary>
			/// Zero out the derivatives and set all y0[] to value
			/// </summary>
			public override void Zero(double value)
			{
				for (int i = 0; i < size; i++)
				y0[i] = value;
				base.Zero(value);
			}
		}

		/// <summary>
		/// Create a new instance of State.
		/// (Trapezoidal adds Xdot_1, and X0 to the base State definition.)
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public override Dynamics.State NewState(int size)
		{
			return new State(size);
		}

	}
}
