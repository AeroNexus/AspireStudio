using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Aspire.Framework
{
	/// <summary>
	/// Integration of all dynamic states in the system
	/// </summary>
	public class Dynamics : Model
	{
		const string category = "Dynamics";

		/// <summary>
		/// Notify all those who need to know when the step size changes
		/// </summary>
		public event EventHandler StepSizeChange;
		Integrator integrator;
		List<State> states = new List<State>();

		double stepSizeRequest, stepOffset;
		bool integrating;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public Dynamics()
		{
		}

		#region Model implementation

		Clock clock;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scenario"></param>
		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock;
			StepSize = clock.StepSize;
			base.Discover(scenario);
			if (integrator == null) integrator = new RungeKutta4();
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Initialize()
		{
			integrator.Initialize(this);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Execute()
		{
			integrating = true;
			// This is the common call to evaluate derivatives, no matter which integration scheme is
			// being used. If you are using Euler, you are done. All other integrators add complexity
			// (and CD calls) for lower errors
			CalcDerivatives(0);
			integrator.Integrate(this);
			//clock.Increment(stepSize); // Need to coordinate clock/dynamics vis a vis: fixed/variable step size
			integrating = false;
			if (stepSizeRequest != 0)
			{
				stepSize = stepSizeRequest;
				if (StepSizeChange != null)
					StepSizeChange(this, EventArgs.Empty);
				stepSizeRequest = 0;
			}
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stepSizeFraction"></param>
		public void CalcDerivatives(double stepSizeFraction)
		{
			stepOffset = stepSizeFraction * stepSize;
			foreach (var deriv in derivatives)
				if (!deriv.DontIntegrate)
					deriv.HaveDerivatives.CalculateDerivatives();
		} List<DerivativeContext> derivatives = new List<DerivativeContext>();

		State CreateState(string name, int size, IHaveDerivatives haveDerivatives)
		{
			State state;
			if (integrator != null)
				state = integrator.NewState(size);
			else
				state = new State() {Size = 1 };
			state.Name = name;

			foreach (var dc in derivatives)
				if (dc.HaveDerivatives == haveDerivatives)
					state.DerivativeCtx = dc;

			if (state.DerivativeCtx == null)
			{
				state.DerivativeCtx = new DerivativeContext(haveDerivatives);
				derivatives.Add(state.DerivativeCtx);
			}

			states.Add(state);
			return state;
		}

		/// <summary>
		/// Create a new state for IArrayProxy
		/// </summary>
		/// <param name="name">Moniker</param>
		/// <param name="value">An double that is the state to be integrated</param>
		/// <param name="haveDerivatives">The IHaveDerivatives that is evaluated each minor step</param>
		/// <returns></returns>
		public State NewState(string name, double value, IHaveDerivatives haveDerivatives)
		{
			return CreateState(name, 1, haveDerivatives);
		}

		/// <summary>
		/// Create a new state for IArrayProxy
		/// </summary>
		/// <param name="name">Moniker</param>
		/// <param name="value">An IArrayProxy (Vector3, Quaternion) that is the state to be integrated</param>
		/// <returns></returns>
		/// <param name="haveDerivatives">The IHaveDerivatives that is evaluated each minor step</param>
		public State NewState(string name, IArrayProxy value, IHaveDerivatives haveDerivatives)
		{
			var state = CreateState(name, value.Length, haveDerivatives);
				state.Proxy = value;
			return state;
		}

		#region Properties

		/// <summary>
		///Provides clock elapsed time [s] + partial step time for
		/// use by CalculateDerivatives. If time is needed in some other form other than
		/// elapsed seconds, add ElapsedSecondOffset + [other time form].
		/// </summary>
		[Category(category)]
    [Description("Clock elapsed time [s].")]
    [XmlIgnore]
		public double ElapsedSeconds
		{
			get { return clock.ElapsedSeconds + stepOffset; }
		}

		/// <summary>
		/// Serializes the particular integrator method
		/// </summary>
		[Category(category)]
		public Integrator Integrator
		{
			get { return integrator; }
			set { integrator = value; }
		}

		internal List<State> States { get { return states; } }

		/// <summary>
		/// The current integration time step size.
		/// </summary>
		[Category(category)]
		[XmlAttribute("stepSize")]
		[Description("The current integration time step size [s].")]
		[DefaultValue(0.125)]
		[Blackboard(Units = "s")]
		public double StepSize
		{
			get { return stepSize; }
			set
			{
				if (integrating)
					stepSizeRequest = value;
				else
				{
					stepSize = value;
					if (StepSizeChange != null)
						StepSizeChange(this, EventArgs.Empty);
				}
			}
		}
		double stepSize = 0.125; // seconds

		#endregion

		/// <summary>
		/// An integrated state. Contains Y, Ydot.
		/// </summary>
		public class State
		{
			IArrayProxy mProxy;
			/// <summary>
			/// Number of elements in the value and derivative arrays
			/// </summary>
			protected int size;
			/// <summary>
			/// Current value and first derivatives
			/// </summary>
			protected double[] y, yDot;

			internal DerivativeContext DerivativeCtx { get; set; }
			/// <summary>
			/// Moniker
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Number of elements in x, xDot
			/// </summary>
			public int Size
			{
				get { return size; }
				set
				{
					size = value;
					y = new double[size];
					yDot = new double[size];
				}
			}

			internal IArrayProxy Proxy { set { mProxy = value; } }

			/// <summary>
			/// The integrated state, Y;
			/// </summary>
			public double[] Y { get { return y; } set { y = value; } }

			/// <summary>
			/// 
			/// </summary>
			public IArrayProxy Yproxy
			{
				get
				{
					for (int i = 0; i < size; i++) mProxy[i] = y[i];
					return mProxy;
				}
				set
				{
					for (int i = 0; i < size; i++) y[i] = value[i];
				}
			}

			/// <summary>
			/// Derivative of x: dx/dt
			/// </summary>
			public double[] Ydot { get { return yDot; } set { yDot = value; } }

			/// <summary>
			/// 
			/// </summary>
			public IArrayProxy YdotProxy
			{
				get
				{
					for (int i = 0; i < size; i++) mProxy[i] = yDot[i];
					return mProxy;
				}
				set
				{
					for(int i=0; i<size; i++) yDot[i] = value[i];
				}
			}

			/// <summary>
			/// Zero out the derivative vector and set the value vector to the supplied argument.
			/// /// </summary>
			/// <param name="value"></param>
			public virtual void Zero(double value)
			{
				for (int i = 0; i < size; i++)
				{
					y[0] = value;
					yDot[0] = 0;
				}
			}
		}

		internal class DerivativeContext
		{
			internal DerivativeContext(IHaveDerivatives haveDerivatives)
			{
				HaveDerivatives = haveDerivatives;
			}

			internal bool DontIntegrate { get; private set; }
			internal IHaveDerivatives HaveDerivatives { get; private set; }
		}
	}

	/// <summary>
	/// Base class for integrators
	/// </summary>
	public abstract class Integrator
	{
		/// <summary>
		/// Initialization phase of configuration
		/// </summary>
		public virtual void Initialize(Dynamics dynamics)
		{
		}

		/// <summary>
		/// Integrate a state with time
		/// </summary>
		/// <param name="dynamics"></param>
		public abstract void Integrate(Dynamics dynamics);

		/// <summary>
		/// Create a (derived) State for a particular integration method
		/// </summary>
		/// <param name="size">number of elements in this State vector</param>
		/// <returns>A new Integrator-specific State</returns>
		public virtual Dynamics.State NewState(int size)
		{
			return new Dynamics.State() { Size = size };
		}
	}
}
