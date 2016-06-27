using System;
using System.ComponentModel;


namespace Aspire.Framework
{
	/// <summary>
	/// Executive mode of operation of a scenario
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public enum ExecutiveMode {
		/// <summary>
		/// The time base is paused
		/// </summary>
		Stop,
		/// <summary>
		/// The time base is running and the models are executing
		/// </summary>
		Executing,
		/// <summary>
		/// Reset the timebase to elapsed time = 0
		/// </summary>
		Reset,
		/// <summary>
		/// Increment the time base one frame
		/// </summary>
		Increment,
		/// <summary>
		/// Decrement the time base one frame
		/// </summary>
		Decrement,
		/// <summary>
		/// Run the time base and the models forward a fixed period without updating the UI
		/// </summary>
		FastForward
	};

	/// <summary>
	/// The executive mode of execution of a scenario
	/// </summary>
	public class Executive
	{
		/// <summary>
		/// ModeChanged event handler signature
		/// </summary>
		/// <param name="previousMode"></param>
		/// <param name="mode"></param>
		public delegate void ModeHandler(ExecutiveMode previousMode, ExecutiveMode mode);

		static Executive theExecutive;

		/// <summary>
		/// Centralized Exit event
		/// </summary>
		public static event EventHandler Exit;
		/// <summary>
		/// Mode changed event
		/// </summary>
		public event ModeHandler ModeChanged;

		Scenario mScenario;
		Scheduler mScheduler;

		/// <summary>
		/// Construct with a scenario
		/// </summary>
		/// <param name="scenario"></param>
		public Executive(Scenario scenario)
		{
			theExecutive = this;
			mScenario = scenario;
			mScheduler = new Scheduler(scenario);
		}

		/// <summary>
		/// The executive mode
		/// </summary>
		public ExecutiveMode Mode
		{
			get { return mMode; }
			set
			{
				// Exit logic
				switch (mMode)
				{
					case ExecutiveMode.Executing:
						break;
				}

				var previousMode = mMode;

				mMode = value;

				// Entry logic
				switch (mMode)
				{
					case ExecutiveMode.Executing:
						mScheduler.Dispatch = true;
						break;
					case ExecutiveMode.Stop:
						mScheduler.Dispatch = false;
						break;
					case ExecutiveMode.Reset:
						mScheduler.Initialize();
						mScenario.Initialize();
						if (ModeChanged != null) ModeChanged(previousMode, mMode);
						Mode = ExecutiveMode.Stop;
						break;
					case ExecutiveMode.Increment:
						mScheduler.FrameCount++;
						mScenario.Execute();
						mScenario.Executive.Mode = ExecutiveMode.Stop;
						break;
					case ExecutiveMode.Decrement:
						mScheduler.FrameCount++;
						mScenario.Clock.StepSize *= -1;
						mScenario.Execute();
						mScenario.Clock.StepSize *= -1;
						if (ModeChanged != null) ModeChanged(previousMode, mMode);
						Mode = ExecutiveMode.Stop;
						break;
				}
				if (ModeChanged != null) ModeChanged(previousMode, mMode);
			}
		} ExecutiveMode mMode;

		/// <summary>
		/// Exit handler
		/// </summary>
		public void OnExit()
		{
			Mode = ExecutiveMode.Stop;
			if (Exit != null)
				Exit(this, EventArgs.Empty);
		}

		/// <summary>
		/// The scheduler
		/// </summary>
		public Scheduler Scheduler { get { return mScheduler; } }

	}
}
