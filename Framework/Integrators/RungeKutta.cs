namespace Aspire.Framework
{
	/// <summary>
	/// Fixed step Runge-Kutta methods. 4th order only for now
	/// </summary>
	public class RungeKutta4 : Integrator
	{
		/// <summary>
		/// Integrate a state
		/// </summary>
		/// <param name="dynamics"></param>
		public override void Integrate(Dynamics dynamics)
		{
			double h = dynamics.StepSize;
			// Derivatives are current for time now.

			foreach (State state in dynamics.States)
				state.K1(h);

			dynamics.CalcDerivatives(0.5);
			foreach (State state in dynamics.States)
				state.K2(h);

			dynamics.CalcDerivatives(0.5);
			foreach (State state in dynamics.States)
				state.K3(h);

			dynamics.CalcDerivatives(1.0);
			foreach (State state in dynamics.States)
				state.K4_Integrate(h);
		}

		// Runga-Kutta4 adds arrays k1,k2,k3,k4, and y0 to the base State definition
		class State : Dynamics.State
		{
			internal double[] k1, k2, k3, k4, y0;
			public State(int size) : base()
			{
				Size = size;
				if (size > 0)
				{
					k1 = new double[size];
					k2 = new double[size];
					k3 = new double[size];
					k4 = new double[size];
					y0 = new double[size];
				}
			}

			internal void K1(double h)
			{
				for (int j = 0; j < size; j++)
				{
					y0[j] = y[j];
					k1[j] = h * yDot[j];
					y[j] = y0[j] + 0.5 * k1[j];
				}
			}

			internal void K2(double h)
			{
				for (int j = 0; j < size; j++)
				{
					k2[j] = h * yDot[j];
					y[j] = y0[j] + 0.5 * k2[j];
				}
			}

			internal void K3(double h)
			{
				for (int j = 0; j < size; j++)
				{
					k3[j] = h * yDot[j];
					y[j] = y0[j] + k3[j];
				}
			}

			internal void K4_Integrate(double h)
			{
				const double K = 1 / 6.0;
				for (int j = 0; j < size; j++)
				{
					k4[j] = h * yDot[j];
					y[j] = y0[j] + K * (k1[j] + 2 * (k2[j] + k3[j]) + k4[j]);
				}
			}

			/// <summary>
			/// Zero out the derivative vector and set the value vector
			/// to the supplied argument.
			/// Also sets the delta state estimates (k1-k4) to zero.
			/// </summary>
			public override void Zero(double value)
			{
				for (int i = 0; i < y.Length; i++)
				{
					k1[i] = k2[i] = k3[i] = k4[i] = 0;
					y0[i] = value;
				}
				base.Zero(value);
			}

		}

		/// <summary>
		/// Runga-Kutta4 adds k1,k2,k3,k4, and y0 to the base State definition
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public override Dynamics.State NewState(int size)
		{
			return new State(size);
		}
	}
}
