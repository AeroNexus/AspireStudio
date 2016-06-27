using Aspire.Core.Messaging;

namespace Aspire.Core
{
	public enum PmInput
	{
		PmRemoved,
		Start,
		Stop,
		SubscriptionReplyRecieved,
		Timeout
	}
	public enum PmState
	{
		Broken,
		Operational,
		Shutdown,
		Subscribing,
		Unsubscribing
	}

	public interface IPmStateMachineListener
	{
		void OnStateChanged(PmState state);
		void Subscribe();
		void Unsubscribe();
	}

	public class PmStateMachine : StateMachine<PmState, PmInput>
	{
		IPmStateMachineListener mListener;

		public PmStateMachine(IPmStateMachineListener listener) : base(PmState.Shutdown)
		{
			mListener = listener;
		}

		protected override string Name
		{
			get { return "PmSM"; }
		}

		protected override void OnEnterState(PmState state)
		{
			switch(state)
			{
			case PmState.Shutdown:
				break;
			case PmState.Subscribing:
				TimeoutTimerSet(0,100);
				mListener.Subscribe();
				break;
			case PmState.Operational:
				break;
			case PmState.Unsubscribing:
				mListener.Unsubscribe();
				State = PmState.Shutdown;
				break;
			case PmState.Broken:
				break;
			}
		}

		protected override void OnExitState(PmState state)
		{
			switch (state)
			{
				case PmState.Subscribing:
					TimeoutTimerCancel();
					break;
			}
		}

		protected override void OnStateChanged(PmState state)
		{
			mListener.OnStateChanged(State);
		}

		protected override void OnProcess(PmInput input)
		{
			switch (input)
			{
				case PmInput.PmRemoved:
					ProcessPmRemoved();
					break;
				case PmInput.Start:
					ProcessStart();
					break;
				case PmInput.Stop:
					ProcessStop();
					break;
				case PmInput.SubscriptionReplyRecieved:
					ProcessSubscriptionReplyRecieved();
					break;
				case PmInput.Timeout:
					ProcessTimeout();
					break;
			}
		}

		protected override void OnTimeout()
		{
			base.OnTimeout();
			Process(PmInput.Timeout);
		}

		void ProcessPmRemoved()
		{
			switch (State)
			{
				case PmState.Operational:
					State = PmState.Broken;
					break;
			}
		}

		void ProcessStart()
		{
			switch (State)
			{
				case PmState.Shutdown:
					State = PmState.Subscribing;
					break;
				case PmState.Broken:
					State = PmState.Subscribing;
					break;
			}
		}

		void ProcessStop()
		{
			switch (State)
			{
				case PmState.Subscribing:
					State = PmState.Shutdown;
					break;
				case PmState.Operational:
					State = PmState.Unsubscribing;
					break;
				case PmState.Broken:
					State = PmState.Shutdown;
					break;
			}
		}

		void ProcessSubscriptionReplyRecieved()
		{
			switch (State)
			{
				case PmState.Shutdown:
					State = PmState.Unsubscribing;
					break;
				case PmState.Subscribing:
					State = PmState.Operational;
					break;
			}
		}

		void ProcessTimeout()
		{
			switch(State)
			{
			case PmState.Subscribing:
				State = PmState.Subscribing;
				break;
			}
		}

		public void PmRemoved()
		{
			Process(PmInput.PmRemoved);
		}

		public void Start()
		{
			Process(PmInput.Start);
		}

		public void Stop()
		{
			Process(PmInput.Stop);
		}

		public void SubscriptionReplyRecieved()
		{
			Process(PmInput.SubscriptionReplyRecieved);
		}
	}
}

